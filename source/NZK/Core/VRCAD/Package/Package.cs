#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class VRCAD {
public static partial class Package {
 
  public static VRCADSnapshot Snapshot(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad)
  { var s = new VRCADSnapshot(); if (vrcad == null) return s;
    s.viewPosition = vrcad.ViewPosition; s.lipSync = vrcad.lipSync;
    s.visemeSkinnedMesh = vrcad.VisemeSkinnedMesh; s.visemeBlendShapes = vrcad.VisemeBlendShapes;
    s.mouthOpenBlendShapeName = vrcad.MouthOpenBlendShapeName;
    s.customExpressions = vrcad.customExpressions; s.customizeAnimationLayers = vrcad.customizeAnimationLayers;
    s.expressionsMenu = vrcad.expressionsMenu; s.expressionParameters = vrcad.expressionParameters;
    return s; }
  public static void Restore(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,VRCADSnapshot s)
  { if (vrcad == null) return; UnityEditor.Undo.RecordObject(vrcad,"Restore VRCAD");
    vrcad.ViewPosition = s.viewPosition; vrcad.lipSync = s.lipSync;
    vrcad.VisemeSkinnedMesh = s.visemeSkinnedMesh; vrcad.VisemeBlendShapes = s.visemeBlendShapes;
    vrcad.MouthOpenBlendShapeName = s.mouthOpenBlendShapeName;
    vrcad.customExpressions = s.customExpressions; vrcad.customizeAnimationLayers = s.customizeAnimationLayers;
    vrcad.expressionsMenu = s.expressionsMenu; vrcad.expressionParameters = s.expressionParameters;
    UnityEditor.EditorUtility.SetDirty(vrcad); }
 
}
}
}
}
#endif
