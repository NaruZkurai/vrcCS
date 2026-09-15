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

  # Unity NEEDS the folder .meta files: without NZK.meta / shorthand.meta,
  # Unity re-imports those folders with fresh GUIDs and every reference to
  # them breaks.  These two come from the Unity source so they carry the REAL
  # unity-minted guid, and are copied before the generator below (which fills
  # in whatever is still missing, including everything convert.py emits).
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

  # =========================================================================
  # LAST: fill in EVERY missing .meta under build/.
  #
  # UPM packages are IMMUTABLE: Unity will not import anything under
  # Packages/<name>/ that lacks a .meta, and it will not generate them either
  # (it cannot write into the package cache).  That produced ~300 lines of
  # "has no meta file, but it's in an immutable folder. The asset will be
  # ignored." and the whole toolkit silently failed to import.
  #
  # Must run LAST: convert.py, the rsyncs, the folder-meta copies and the
  # package.json stamp all create files, and anything created after this pass
  # would be an orphan again.
  #
  # GUIDs are DETERMINISTIC (md5 of package-relative path -> 32 hex chars,
  # which is exactly Unity's guid format).  Stability is the whole point: a
  # random guid per sync makes Unity re-import every file and breaks every
  # serialized reference to these scripts.  Path-keyed means a rebuild keeps
  # its guid and a rename gets a new one (correct: a rename IS a new asset).
  # These are NOT Unity's own hashes and don't need to be - guid uniqueness
  # and stability is all that matters.  Already-present .meta files are never
  # touched, so the two real folder guids above survive.
  # =========================================================================
  $PY - "$VCS/build" <<'EOF'
import hashlib, os, sys

root = sys.argv[1].rstrip('/')
FOLDER = ("fileFormatVersion: 2\n"
          "guid: {g}\n"
          "folderAsset: yes\n"
          "DefaultImporter:\n"
          "  externalObjects: {{}}\n"
          "  userData: \n"
          "  assetBundleName: \n"
          "  assetBundleVariant: \n")
# plain ASSET (package.json, .nzk) = no folderAsset line, DefaultImporter
ASSET = ("fileFormatVersion: 2\n"
         "guid: {g}\n"
         "DefaultImporter:\n"
         "  externalObjects: {{}}\n"
         "  userData: \n"
         "  assetBundleName: \n"
         "  assetBundleVariant: \n")
SCRIPT = ("fileFormatVersion: 2\n"
          "guid: {g}\n"
          "MonoImporter:\n"
          "  externalObjects: {{}}\n"
          "  serializedVersion: 2\n"
          "  defaultReferences: []\n"
          "  executionOrder: 0\n"
          "  icon: {{instanceID: 0}}\n"
          "  userData: \n"
          "  assetBundleName: \n"
          "  assetBundleVariant: \n")

def guid(rel):
    # namespace prefix keeps these disjoint from any other hash scheme
    return hashlib.md5(("nzk.toolkit/" + rel).encode()).hexdigest()

made = 0
for dirpath, dirnames, filenames in os.walk(root):
    dirnames.sort(); filenames.sort()
    rel = os.path.relpath(dirpath, root).replace(os.sep, '/')
    if rel == '.':
        # build/ IS the package root: the UPM manifest lives there and the
        # folder is not itself an asset inside the package, so no meta.  A
        # meta here would be an extra asset inside the package.
        continue
    mp = dirpath + '.meta'
    if not os.path.exists(mp):
        open(mp, 'w').write(FOLDER.format(g=guid(rel)))
        made += 1
    for fn in filenames:
        if fn.endswith('.meta'):
            continue
        fp = os.path.join(dirpath, fn)
        if os.path.exists(fp + '.meta'):
            continue
        if fn.endswith('.cs') or fn.endswith('.cs.nzk'):
            tpl = SCRIPT
        else:
            tpl = ASSET
        open(fp + '.meta', 'w').write(tpl.format(g=guid(rel + '/' + fn)))
        made += 1
print("metas    -> %d written" % made)
EOF

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
