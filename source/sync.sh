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
# Side cache for Unity-minted .meta files.  convert.py -d wipes ./NZK, which
# would also delete the .meta Unity wrote next to each generated file, so we
# stash them here first and put them back afterwards.  See the passthrough
# block at the end of this script.
META_CACHE=./.nzk-meta-cache

# 0) snapshot Unity's .meta files out of ./NZK BEFORE the wipe.
#    Unity mints these when it imports the project; they are the GUIDs every
#    serialized reference points at, so they must survive the regen.
#    Only .meta is captured - the .cs themselves are always regenerated.
if [ -d "$OUT" ]; then
  mkdir -p "$META_CACHE"
  ( cd "$OUT" && find . -name '*.meta' -print0 ) | while IFS= read -r -d '' m; do
    mkdir -p "$META_CACHE/$(dirname "$m")"
    cp -f "$OUT/${m#./}" "$META_CACHE/${m#./}"
  done
  N=$(find "$META_CACHE" -name '*.meta' | wc -l)
  [ "$N" -gt 0 ] && echo "metas    -> snapshotted $N unity .meta before wipe"
fi

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
if [ -d "$VCS" ] && command -v rsync >/dev/null 2>&1; then
  mkdir -p "$VCS/source" "$VCS/build"

  # source: the five sync inputs + the standalone menu-item source,
  # plus the pipeline itself (sync.sh is untracked in the Unity project).
  # Synced as a DIRECTORY (not a file list) so --delete can remove renaming
  # leftovers; the include globs whitelist exactly what belongs in source/.
  "${RSYNC[@]}" --include='*.cs.nzk' --include='sync.sh' --include='convert.py' \
                 --exclude='*' ./ "$VCS/source/"

  # build: the generated tree + the standalone menu-item source
  "${RSYNC[@]}" "$OUT/" "$VCS/build/NZK/"
  "${RSYNC[@]}" rctoan_menuItem.cs.nzk "$VCS/build/"

  # shorthand is pre-leafed and NOT generated: mirror as-is (no changes).
  # --delete here removes shorthand leaves that no longer exist at the source.
  "${RSYNC[@]}" ./shorthand/ "$VCS/build/shorthand/"

  # =========================================================================
  # meta passthrough: build/ GETS THE REAL UNITY-MINTED .meta FILES.
  #
  # Unity is the only thing that mints GUIDs here.  When Unity imports
  # `NZK-Toolkit/` it writes .meta next to every folder and file, and those
  # GUIDs are what serialized references (scene components, prefabs, the .anim
  # assets) point at.  So we COPY them, we never invent them.
  #
  # Folder metas (NZK.meta / shorthand.meta) come from the Unity source tree.
  # Per-file metas come through the rsyncs, because shorthand/ and NZK/ are
  # both mirrored whole.
  #
  # *** THE CATCH: convert.py -d shutil.rmtree's ./NZK on every run, taking
  # Unity's freshly-written per-file .meta files with it.  That is why the
  # source NZK tree sits at 0 metas / 222 .cs - they are deleted each sync.
  # Step 0 at the top of this script snapshots them to $META_CACHE first; this
  # step puts them back on top of the regenerated tree.
  #
  # Restore is path-keyed and only for paths that still exist after regen, so
  # a deleted/renamed file drops its stale meta.  A rename gets a new GUID -
  # correct, because to Unity a rename IS a new asset.
  # =========================================================================
  [ -f ./NZK.meta ] && cp -f ./NZK.meta "$VCS/build/NZK.meta"
  [ -f ./shorthand.meta ] && cp -f ./shorthand.meta "$VCS/build/shorthand.meta"
  if [ -d "$META_CACHE" ]; then
    M=0
    ( cd "$META_CACHE" && find . -name '*.meta' -print0 ) |
      while IFS= read -r -d '' m; do
        [ -e "$OUT/${m#./}" ] || continue
        cp -f "$META_CACHE/${m#./}" "$OUT/${m#./}"
      done
    M=$(find "$OUT" -name '*.meta' | wc -l)
    echo "metas    -> $M unity .meta restored into $OUT"
  else
    echo "metas    -> none to restore; open Unity to mint them, then re-sync" >&2
  fi

  # re-mirror ONLY the metas, now that they are back on disk.
  # (the earlier rsyncs ran before the restore; rsync -a would otherwise have
  # deleted the target metas in build/ because ./NZK had none at that moment)
  if [ -d "$META_CACHE" ]; then
    ( cd "$OUT" && find . -name '*.meta' -print0 ) |
      while IFS= read -r -d '' m; do
        d="$VCS/build/NZK/$(dirname "${m#./}")"
        mkdir -p "$d"
        cp -f "$OUT/${m#./}" "$d/"
      done
  fi

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
