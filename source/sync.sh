#!/usr/bin/env bash
# sync.sh — regen ./NZK/ from the .nzk sources.  The first five are each a
# single `namespace NZK { ... partial class Core { ... } }`; -bc emits ONE whole
# slice per top-level type, and since the same top-level name (`Core`) appears in
# every input, call 1 creates Core/Core.cs and calls 2/3/4/5 (-u) append the next
# partial segment Core/Core_p1.cs / Core/Core_p2.cs / Core/Core_p3.cs /
# Core/Core_p4.cs.  All segments share namespace NZK + `static partial class Core`
# so C# folds them into one type.  Any OTHER .cs.nzk in ./ (currently
# rctoan_menuItem.cs.nzk) is a STANDALONE source and gets its own -bc -u pass in
# step 6, emitting NZK/<TypeName>/<TypeName>.cs from the type name inside it.
#
# Split of duties:  Yaml.cs.nzk = how a YAML block is made (markers, spans, type
# tags, line/field IO).  nan.cs.nzk = how a scale value becomes NaN (detect /
# set-zero-only / force).  Neither knows the other's job.
#
# Flags:
#   --nopush, --np   mirror + commit to the vrcCS repo, but skip the git push.
#                    Use when you want build/ updated and versioned without
#                    publishing a release for consuming projects to pick up.
#                    Nothing is lost - commit still lands, push it yourself.
#   -h, --help       usage.
set -euo pipefail
cd "$(dirname "$0")"

# --nopush / --np: mirror + commit locally, but do NOT push to the remote.
# Useful when you want the build/ tree updated and versioned without publishing
# a new package for every consuming project to pick up.  Commits still happen,
# so nothing is lost - run `git -C "$VCS" push` yourself when ready.
NOPUSH=0
for arg in "$@"; do
  case "$arg" in
    --nopush|--np) NOPUSH=1 ;;
    -h|--help)
      echo "usage: sync.sh [--nopush|--np]"
      echo "  --nopush, --np   mirror + commit, but skip the git push"
      exit 0 ;;
    *)
      echo "sync.sh: unknown argument '$arg' (try --help)" >&2
      exit 2 ;;
  esac
done

PY=python3
OUT=./NZK

# NOTE: convert.py -d shutil.rmtree's ./NZK, then re-emits it fresh.  Unity
# mints a .meta next to every folder and file it imports, and those GUIDs are
# what every serialized reference points at - so we do NOT invent them, we wait
# for Unity to write them and rsync them straight through.
#
# Unity only does that on a project refresh, which is ASYNC and out of our
# control.  So after the convert passes we POLL until the metas show up (see
# the gate below) instead of racing it and shipping a meta-less package.
#
# Do NOT hand-write .meta files / hash your own GUIDs here.  Unity's real ones
# already exist on disk; the job is only to wait for and copy them.

# 1) monolith: -d wipes ./NZK, emit fresh (Core/Core.cs)
$PY convert.py -bc -d nzktoolkit_monolith_compilable.cs.nzk -o "$OUT" -ns NZK

# 2) rctonan: -u appends its Core segment as Core/Core_p1.cs (no delete)
$PY convert.py -bc -u rctonan.cs.nzk -o "$OUT" -ns NZK

# 3) NZK.Toolkit: -u appends its Core segment as Core/Core_p2.cs (no delete)
$PY convert.py -bc -u NZK.Toolkit.cs.nzk -o "$OUT" -ns NZK

# 4) Yaml: -u appends its Core segment as Core/Core_p3.cs (no delete)
$PY convert.py -bc -u Yaml.cs.nzk -o "$OUT" -ns NZK

# 5) nan: -u appends its Core segment as Core/Core_p4.cs (no delete)
$PY convert.py -bc -u nan.cs.nzk -o "$OUT" -ns NZK

# 6) every OTHER .cs.nzk in ./ is a standalone source too (not a Core partial),
#    so it cannot ride the -u append chain above: -u would name its output from
#    the FIRST top-level type it finds and grow Core_pN, not make its own file.
#    Each one gets its own -bc -u pass, which emits NZK/<TypeName>/<TypeName>.cs
#    driven by the type name INSIDE the file (never by the input filename).
#
#    This is what was missing: rctoan_menuItem.cs.nzk used to be rsynced to
#    build/ VERBATIM, still carrying the .nzk suffix.  Unity only compiles .cs,
#    so the file was inert in every consuming project while the local harness
#    (which globbed *.cs.nzk) happily linked it and reported a clean build.
#    Parsing it here is what puts a real .cs on the wire.
#
#    The five inputs above are excluded by name: they already ran.  Anything
#    else ending in .cs.nzk is discovered, so adding a new standalone source
#    needs no edit to this script.
for f in *.cs.nzk; do
  [ -e "$f" ] || continue
  case "$f" in
    nzktoolkit_monolith_compilable.cs.nzk|rctonan.cs.nzk|NZK.Toolkit.cs.nzk|Yaml.cs.nzk|nan.cs.nzk) continue ;;
  esac
  echo "convert  -> $f (standalone)"
  $PY convert.py -bc -u "$f" -o "$OUT" -ns NZK
done

echo "synced -> $(pwd)/${OUT}"

# ===========================================================================
# Wait for Unity to mint the .meta files.
#
# convert.py just wiped and re-emitted ./NZK, so the metas that WERE in there
# are gone.  Unity writes them back only when it next refreshes the project,
# which happens on its own schedule.  Racing that is what left build/NZK/ with
# 222 .cs and 0 metas and made Unity spam "has no meta file, but it's in an
# immutable folder" for every single file.
#
# So: poll until at least one .meta exists.  A meta appearing means Unity has
# picked up the new tree and is writing siblings for it.
#
# Bounded on purpose.  An unbounded `while true` here would hang the script - /
# and a CI job, and a git hook - forever if Unity is closed.  After the timeout
# we warn and sync anyway: a package with stale/missing metas still beats a
# wedged pipeline, and the next run (with Unity open) fixes it.
# `find -print -quit` (not a hardcoded filename) because the first meta Unity
# writes is not predictable; ANY meta proves the import ran.
#
# Unity writes metas INCREMENTALLY, so "one appeared" is not "it finished".
# Exiting on the first meta caught the import mid-flight (239 of 429) and
# rsynced a half-meta'd tree.  So we wait for the count to STOP CHANGING: a
# settled count means the import finished.  STABLE_REQ consecutive unchanged
# polls is the settle window.
# =========================================================================
META_WAIT=${META_WAIT:-120}    # seconds; override via env when scripting
STABLE_REQ=${STABLE_REQ:-3}    # consecutive identical counts = settled
count_meta() { find "$OUT" -name '*.meta' 2>/dev/null | wc -l; }
waited=0; last=-1; stable=0
echo "metas    -> waiting up to ${META_WAIT}s for Unity to settle .meta..."
while :; do
  now=$(count_meta)
  if [ "$now" -gt 0 ] && [ "$now" -eq "$last" ]; then
    stable=$((stable + 1))
    [ "$stable" -ge "$STABLE_REQ" ] && break
  else
    stable=0
  fi
  last=$now
  if [ "$waited" -ge "$META_WAIT" ]; then
    echo "metas    -> TIMEOUT after ${META_WAIT}s at $now .meta (not settled)" >&2
    echo "metas    -> open Unity, let it finish importing, then re-run ./sync.sh" >&2
    break
  fi
  sleep 1
  waited=$((waited + 1))
done
echo "metas    -> $last unity .meta settled (waited ${waited}s)"

# ===========================================================================
# Mirror out to the vrcCS working tree.
#   source/  <- the .cs.nzk inputs + pipeline (what gets compiled)
#   build/   <- the generated ./NZK tree + shorthand/ (what ships to Unity)
# rsync -a --delete keeps the mirror an exact copy: renamed/removed sources
# disappear from source/ and build/NZK/ instead of lingering as stale files.
# --delete is scoped to the mirrored SUBDIRS only, so anything else kept in
# build/ (nzk.dll from the test harness, notes, etc.) is left alone.
# ===========================================================================
VCS=/nzk/git/vrcCS
RSYNC=(rsync -a --delete --exclude='*.csproj' --exclude='*.dll' --exclude='*.pdb')
# Same flags but WITHOUT --delete.  Unity writes .meta files into build/ when it
# imports the package; a delete pass would wipe the GUIDs Unity just minted.
# For build/ we only copy in, never delete out.
RSYNC_KEEP=(rsync -a --exclude='*.csproj' --exclude='*.dll' --exclude='*.pdb')
if [ -d "$VCS" ] && command -v rsync >/dev/null 2>&1; then
  mkdir -p "$VCS/source" "$VCS/build"

  # source: the five sync inputs + the standalone menu-item source,
  # plus the pipeline itself (sync.sh is untracked in the Unity project).
  # Synced as a DIRECTORY (not a file list) so --delete can remove renaming
  # leftovers; the include globs whitelist exactly what belongs in source/.
  # .meta is included so Unity's GUIDs travel with the sources too.
  "${RSYNC[@]}" --include='*.cs.nzk' --include='*.meta' --include='sync.sh' \
                 --include='convert.py' --exclude='*' ./ "$VCS/source/"

  # build: the generated tree + the standalone menu-item source.
  # ONLY .cs and .meta travel to build/.  A .cs.nzk here would be dead weight:
  # Unity compiles .cs exclusively, so a raw .nzk-suffixed file in the package
  # is invisible no matter how many files it contains.  Every source is parsed
  # by the convert passes above, so build/ holds nothing but real .cs output.
  #
  # NO --delete for build/: Unity writes .meta files in there after importing
  # the package, and a delete pass would wipe GUIDs Unity just minted.  Copy in
  # only, never delete out.
  "${RSYNC_KEEP[@]}" --include='*/' --include='*.cs' --include='*.meta' --exclude='*' \
                     "$OUT/" "$VCS/build/NZK/"

  # shorthand is pre-leafed, hand-written, and NOT generated: mirror as-is.
  # It is already in final .cs form (one member per file), so it passes the
  # same .cs-only filter.  Carries its own .meta files (Unity minted them).
  "${RSYNC_KEEP[@]}" --include='*/' --include='*.cs' --include='*.meta' --exclude='*' \
                     ./shorthand/ "$VCS/build/shorthand/"

  # Unity-minted folder + root metas: COPY the real ones, never invent a GUID.
  # Everything inside NZK/ and shorthand/ travels via the rsyncs above; these
  # are the metas for the mirrored folders themselves and for the files that
  # live loose in build/ (the package manifest).
  for f in NZK shorthand package.json; do
    [ -f "./$f.meta" ] && cp -f "./$f.meta" "$VCS/build/$f.meta"
  done

  # rctoan_menuItem.cs.nzk is deliberately NOT copied into build/ anymore.  It
  # is parsed in step 6 above and ships as generated .cs inside NZK/MenuItems/,
  # so shipping the raw source alongside it would double-declare NZK.MenuItems
  # (CS0101) in any tree that compiled both.
  # Clean up the stale copies an older sync.sh left behind, plus the meta that
  # was hand-travelled with them - if they linger, Unity keeps importing a file
  # whose type is now also declared by the generated tree.
  rm -f "$VCS/build/rctoan_menuItem.cs.nzk" "$VCS/build/rctoan_menuItem.cs.nzk.meta"

  # =========================================================================
  # package.json — the UPM manifest, and the ONE file that is shipped to
  # build/ rather than rsynced (its version is rewritten on the way through).
  #
  # ./package.json in THIS tree is the template: a real Unity asset that Unity
  # has imported, so package.json.meta exists and carries the GUID Unity uses
  # for it.  The template keeps its version pinned (0.1.0) so that Unity's own
  # view of the asset never changes and its meta never churns.
  #
  # build/package.json gets a fresh time-based version, and its .meta is copied
  # from the source alongside it (see the meta loop above) so the packaged copy
  # has the same GUID.  Without that meta Unity refuses the file:
  #   "Asset Packages/com.nzk.toolkit/package.json has no meta file, but it's
  #    in an immutable folder. The asset will be ignored."
  #
  # Version scheme: 0.1.<MMDDHHmm> with leading zeros stripped, so it is a
  # valid semver that sorts strictly ascending as time passes.
  # =========================================================================
  VER=""
  if [ -f ./package.json ]; then
    cp -f ./package.json "$VCS/build/package.json"
    STAMP=$(date -u +%m%d%H%M)
    $PY - "$VCS/build/package.json" "$STAMP" <<'EOF'
import json, sys
p, stamp = sys.argv[1], sys.argv[2]
d = json.load(open(p))
d['version'] = '0.1.%s' % stamp.lstrip('0')
json.dump(d, open(p, 'w'), indent=2)
open(p, 'a').write('\n')
EOF
    # read the version BACK from the json so the commit message can never
    # disagree with what actually got written (single source of truth).
    VER=$($PY -c "import json,sys; print(json.load(open(sys.argv[1]))['version'])" \
                "$VCS/build/package.json")
    echo "versioned -> $VER"
  else
    echo "versioned -> SKIPPED: no ./package.json template in the Unity tree" >&2
  fi

  echo "mirrored -> $VCS/{source,build}"

  # =========================================================================
  # Commit + push the mirror.  Only runs when the tree actually changed, so a
  # no-op sync doesn't spam empty commits.  Message carries the version that
  # was just stamped into build/package.json.
  # GIT_* unset: this may be invoked from inside a git hook / Unity, where an
  # inherited GIT_DIR or GIT_INDEX_FILE would silently target the WRONG repo.
  # =========================================================================
  if command -v git >/dev/null 2>&1 &&
     env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
         git -C "$VCS" rev-parse --git-dir >/dev/null 2>&1; then
    if [ -n "$(env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
                 git -C "$VCS" status --porcelain)" ]; then
      env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
          git -C "$VCS" add -A
      MSG="sync from testing version ${VER:-unknown}"
      env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
          git -C "$VCS" commit -q -m "$MSG"
      # push only if a remote is configured; tolerate offline (non-fatal so a
      # failed push never aborts a sync that otherwise succeeded).
      # --nopush/--np stops here: the commit above still happened, we just
      # don't publish it.  No remote check needed since we're not pushing.
      if [ "$NOPUSH" = "1" ]; then
        echo "committed -> $MSG (--nopush: skipped push)"
      elif env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
             git -C "$VCS" remote | grep -q .; then
        if env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
               git -C "$VCS" push -q; then
          echo "pushed   -> $MSG"
        else
          echo "commit ok, push FAILED (offline or auth?) -> $MSG" >&2
        fi
      else
        echo "committed -> $MSG (no remote, skipped push)"
      fi
    else
      echo "no changes to commit"
    fi
  fi
fi
