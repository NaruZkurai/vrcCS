#!/usr/bin/env bash
# Compile the NZK synced output with Unity's bundled Roslyn csc.
#
# RULE: only synced output is compiled -> run sync.sh FIRST, every time.
# Reads the mirror at /nzk/git/vrcCS/build (generated NZK/ + shorthand/),
# so nothing here touches the Unity project. Output lands in this folder.
set -uo pipefail

VCS=/nzk/git/vrcCS
BUILD="$VCS/build"
HERE="$(cd "$(dirname "$0")" && pwd)"

# sync first: generated code is only compilable after this
( cd "/nzk/unity/blank project/Assets/NZK-Toolkit" && ./sync.sh >/dev/null )

[ -d "$BUILD/NZK" ] || { echo "no $BUILD/NZK - did sync.sh run?"; exit 1; }

U=/home/naruzkurai/Unity/Hub/Editor/2022.3.22f1/Editor/Data
NET="$U/NetCoreRuntime/dotnet"
MG="$U/Managed"
REF="$U/UnityReferenceAssemblies/unity-4.8-api"
FAC="$REF/Facades"

refs=()
VRC="/nzk/unity/vrc/!_CC_Kiga 4/Packages"
for d in \
  "$REF/mscorlib.dll" "$REF/System.dll" "$REF/System.Core.dll" \
  "$FAC/netstandard.dll" "$FAC/System.Runtime.dll" "$FAC/System.Collections.dll" \
  "$MG/UnityEngine/UnityEngine.dll" \
  "$MG/UnityEngine/UnityEngine.CoreModule.dll" \
  "$MG/UnityEngine/UnityEngine.AnimationModule.dll" \
  "$MG/UnityEngine/UnityEngine.PhysicsModule.dll" \
  "$MG/UnityEngine/UnityEngine.IMGUIModule.dll" \
  "$MG/UnityEngine/UnityEngine.AudioModule.dll" \
  "$MG/UnityEngine/UnityEngine.ParticleSystemModule.dll" \
  "$MG/UnityEngine/UnityEngine.TextRenderingModule.dll" \
  "$MG/UnityEngine/UnityEngine.UIModule.dll" \
  "$MG/UnityEngine/UnityEngine.JSONSerializeModule.dll" \
  "$MG/UnityEngine/UnityEngine.ClothModule.dll" \
  "$MG/UnityEngine/UnityEngine.AnimationModule.dll" \
  "$MG/UnityEngine/UnityEditor.CoreModule.dll" \
  "$VRC/com.vrchat.base/Editor/VRCSDK/Plugins/VRCSDKBase-Editor.dll" \
  "$VRC/com.vrchat.base/Runtime/VRCSDK/Plugins/VRCSDKBase.dll" \
  "$VRC/com.vrchat.base/Runtime/VRCSDK/Plugins/VRC.Dynamics.dll" \
  "$VRC/com.vrchat.base/Runtime/VRCSDK/Plugins/VRC.Utility.dll" \
  "$VRC/com.vrchat.base/Runtime/VRCSDK/Plugins/VRC.SDK3.Dynamics.PhysBone.dll" \
  "$VRC/com.vrchat.base/Runtime/VRCSDK/Plugins/VRC.SDK3.Dynamics.Constraint.dll" \
  "$VRC/com.vrchat.base/Runtime/VRCSDK/Plugins/VRC.SDK3.Dynamics.Contact.dll" \
  "$VRC/com.vrchat.avatars/Runtime/VRCSDK/Plugins/VRCSDK3A.dll" \
  "$VRC/com.vrchat.avatars/Runtime/VRCSDK/Plugins/VRCSDK3A-Editor.dll" \
; do
  [ -f "$d" ] && refs+=("-r:$d")
done

# Compile ONLY the synced mirror: generated NZK/**, pre-leafed shorthand/**,
# and the hand-written non-sync rctoan_menuItem.cs.nzk.
# The five .cs.nzk sync inputs are absent from build/ (they live in source/).
srcs=$(find "$BUILD" -type f \( -name '*.cs' -o -name '*.cs.nzk' \) | sort)

"$NET" "$U/DotNetSdkRoslyn/csc.dll" \
  -nologo -target:library -langversion:9 -nullable:disable -unsafe+ \
  -out:"$HERE/nzk.dll" -nostdlib+ -noconfig \
  -define:UNITY_EDITOR,UNITY_2022_3_OR_NEWER \
  "${refs[@]}" $srcs
