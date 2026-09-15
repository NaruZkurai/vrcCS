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
RSYNC=(rsync -a --delete --exclude='*.meta' --exclude='*.dll' --exclude='*.pdb')
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

  echo "mirrored -> $VCS/{source,build}"
fi
