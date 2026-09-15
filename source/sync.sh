#!/usr/bin/env bash
# sync.sh — regen ./NZK/ from the five .nzk sources.  Each input is a
# single `namespace NZK { ... partial class Core { ... } }`; -bc emits ONE whole
# slice per top-level type, and since the same top-level name (`Core`) appears in
# every input, call 1 creates Core/Core.cs and calls 2/3/4/5 (-u) append the next
# partial segment Core/Core_p1.cs / Core/Core_p2.cs / Core/Core_p3.cs /
# Core/Core_p4.cs.  All segments share namespace NZK + `static partial class Core`
# so C# folds them into one type.
#
# Split of duties:  Yaml.cs.nzk = how a YAML block is made (markers, spans, type
# tags, line/field IO).  nan.cs.nzk = how a scale value becomes NaN (detect /
# set-zero-only / force).  Neither knows the other's job.
set -euo pipefail
cd "$(dirname "$0")"

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
# =========================================================================
META_WAIT=${META_WAIT:-60}   # seconds; override via env when scripting
have_meta() { find "$OUT" -name '*.meta' -print -quit 2>/dev/null | grep -q .; }
if have_meta; then
  echo "metas    -> $(find "$OUT" -name '*.meta' | wc -l) unity .meta already present"
else
  echo "metas    -> waiting up to ${META_WAIT}s for Unity to mint .meta..."
  waited=0
  while ! have_meta; do
    if [ "$waited" -ge "$META_WAIT" ]; then
      echo "metas    -> TIMEOUT after ${META_WAIT}s, no Unity .meta in $OUT" >&2
      echo "metas    -> open Unity, let it refresh, then re-run ./sync.sh" >&2
      break
    fi
    sleep 1
    waited=$((waited + 1))
  done
  have_meta && echo "metas    -> $(find "$OUT" -name '*.meta' | wc -l) unity .meta (waited ${waited}s)"
fi

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
  # NO --delete here: Unity writes .meta files in there after importing the
  # package, and a delete pass would wipe GUIDs Unity just minted.  Copy in
  # only, never delete out.
  "${RSYNC_KEEP[@]}" "$OUT/" "$VCS/build/NZK/"
  "${RSYNC_KEEP[@]}" rctoan_menuItem.cs.nzk "$VCS/build/"

  # shorthand is pre-leafed and NOT generated: mirror as-is (no changes).
  # Carries its own .meta files (Unity minted them) - same no-delete rule.
  "${RSYNC_KEEP[@]}" ./shorthand/ "$VCS/build/shorthand/"

  # Unity-minted folder metas: COPY the real ones, never invent a GUID.
  # Everything inside NZK/ and shorthand/ gets its .meta via the rsyncs above;
  # these two are the folder metas for the dirs themselves.
  [ -f ./NZK.meta ] && cp -f ./NZK.meta "$VCS/build/NZK.meta"
  [ -f ./shorthand.meta ] && cp -f ./shorthand.meta "$VCS/build/shorthand.meta"

  # package.json: stamp a UPM version built from UTC wall-clock time.
  # 0.MMDDHHmm -> 0.<month><day><hour><min>, so versions sort strictly
  # ascending as time passes and stay inside the requested "0.1.x"-style
  # 0.x.y range ("0.1" prefix kept literal, time fills the rest).
  [ -f ./package.json ] && cp -f ./package.json "$VCS/build/package.json"
  VER=""
  if [ -f "$VCS/build/package.json" ]; then
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
      if env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
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
