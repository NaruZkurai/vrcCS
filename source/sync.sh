#!/usr/bin/env bash
# sync.sh — generate the toolkit, then push it repo -> projects.
#
# DIRECTION: /nzk/git/vrcCS is the SOURCE OF TRUTH.  It owns both trees:
#
#   source/   the .cs.nzk monolith inputs + the hand-written shorthand leaves
#   build/    the generated .cs output + shorthand, i.e. what ships
#
# and this script pushes build/ OUT to every consuming Unity project.  Nothing
# is ever pulled back in: a project that edits its copy has that edit
# overwritten on the next run, which is the point - one authoritative tree.
#
# Flow:
#   1. source/ -> build/     convert every .cs.nzk into .cs (convert.py -bc)
#   2. build/  -> projects   rsync the generated tree into each target
#
# Targets are listed in TARGETS below; add a line to add a consumer.
#
# Flags:
#   --nopush, --np, --nogit   regenerate + push, but skip the git commit.
#   -h, --help                usage.
set -euo pipefail
cd "$(dirname "$0")"

VCS=/nzk/git/vrcCS
SRC="$VCS/source"
BUILD="$VCS/build"

TARGETS=(
  "/nzk/unity/blank project/Assets/NZK-Toolkit"
  "/nzk/unity/vrc/Nemesis Main/Assets/NZK toolkit v6/vrcCS"
)

NOPUSH=0
for arg in "$@"; do
  case "$arg" in
    --nopush|--np|--nogit) NOPUSH=1 ;;
    -h|--help)
      echo "usage: sync.sh [--nopush|--np|--nogit]"
      echo "  repo source/ -> repo build/ -> each consumer project"
      echo "  --nopush, --np, --nogit   regenerate + push, but skip the git commit"
      exit 0 ;;
    *) echo "sync.sh: unknown argument '$arg' (try --help)" >&2; exit 2 ;;
  esac
done

PY=python3
[ -d "$SRC" ] || { echo "sync.sh: no source tree at $SRC" >&2; exit 2; }

SCRATCH=$(mktemp -d)
trap 'rm -rf "$SCRATCH"' EXIT

shopt -s nullglob
sources=("$SRC"/*.cs.nzk)
shopt -u nullglob
[ ${#sources[@]} -gt 0 ] || { echo "sync.sh: no .cs.nzk sources in $SRC" >&2; exit 2; }

mkdir -p "$BUILD"
FIRST=1
for f in "${sources[@]}"; do
  base=$(basename "$f")
  cp -f "$f" "$SCRATCH/"
  if [ "$FIRST" = "1" ]; then
    echo "generate -> $base (wipe + fresh)"
    ( cd "$SCRATCH" && $PY "$SRC/convert.py" -bc -d "$base" -o "$BUILD/NZK" -ns NZK )
    FIRST=0
  else
    echo "generate -> $base (append)"
    ( cd "$SCRATCH" && $PY "$SRC/convert.py" -bc -u "$base" -o "$BUILD/NZK" -ns NZK )
  fi
done
echo "generated -> $BUILD/NZK"

command -v rsync >/dev/null 2>&1 || { echo "sync.sh: rsync not found" >&2; exit 2; }

RSYNC_BUILD=(rsync -a --delete --exclude='*.meta' --exclude='*.csproj' \
                  --exclude='*.dll' --exclude='*.pdb')
RSYNC_KEEP=(rsync -a --exclude='*.csproj' --exclude='*.dll' --exclude='*.pdb')
# --delete for the same reason as RSYNC_BUILD: a leaf renamed or removed at the
# source must disappear from the target, or the project keeps compiling a class
# that no longer exists upstream.  *.meta stays excluded from deletion because
# Unity mints those and their GUIDs must survive.
RSYNC_SH=(rsync -a --delete --exclude='*.meta' --exclude='*.csproj' \
                --exclude='*.dll' --exclude='*.pdb')

for T in "${TARGETS[@]}"; do
  if [ ! -d "$T" ]; then echo "push     -> SKIP (missing): $T" >&2; continue; fi
  mkdir -p "$T/build"

  "${RSYNC_BUILD[@]}" --include='*/' --include='*.cs' --exclude='*' \
                     "$BUILD/NZK/" "$T/build/NZK/"

  "${RSYNC_SH[@]}" --include='*/' --include='*.cs' --include='*.meta' --exclude='*' \
                     "$BUILD/shorthand/" "$T/build/shorthand/"

  for f in NZK shorthand package.json; do
    [ -f "$BUILD/$f.meta" ] && cp -f "$BUILD/$f.meta" "$T/build/$f.meta"
  done
  [ -f "$BUILD/package.json" ] && cp -f "$BUILD/package.json" "$T/build/package.json"

  # NOTE: source/ is deliberately NOT mirrored into the target.  Targets live
  # under Unity's Assets/, where every .cs is compiled - mirroring
  # source/shorthand/*.cs next to build/shorthand/*.cs makes Unity compile the
  # same class twice (CS0102 "already contains a definition for", CS0111
  # "already defines a member").  The .cs.nzk inputs are not needed to consume
  # the toolkit; they stay in the repo, which is the source of truth.

  echo "pushed   -> $T"
done

if [ "$NOPUSH" = "1" ]; then echo "versioned -> skipped (--nopush)"; exit 0; fi

if command -v git >/dev/null 2>&1 &&
   env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE \
       git -C "$VCS" rev-parse --git-dir >/dev/null 2>&1; then
  if [ -n "$(env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE git -C "$VCS" status --porcelain)" ]; then
    env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE git -C "$VCS" add -A
    MSG="sync from repo source $(date -u +%Y-%m-%dT%H:%M:%SZ)"
    env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE git -C "$VCS" commit -q -m "$MSG"
    echo "committed -> $MSG"
    if env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE git -C "$VCS" remote | grep -q .; then
      if env -u GIT_DIR -u GIT_WORK_TREE -u GIT_INDEX_FILE git -C "$VCS" push -q; then
        echo "pushed repo -> $MSG"
      else
        echo "commit ok, push FAILED (offline or auth?) -> $MSG" >&2
      fi
    else
      echo "committed -> $MSG (no remote configured)"
    fi
  else
    echo "versioned -> no changes to commit"
  fi
fi
