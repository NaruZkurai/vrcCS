#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class VRCAD {
public static partial class Package {
public struct VRCADSnapshot
  { public UnityEngine.Vector3 viewPosition; public VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle lipSync;
    public UnityEngine.SkinnedMeshRenderer visemeSkinnedMesh; public System.String[] visemeBlendShapes;
    public System.String mouthOpenBlendShapeName; public System.Boolean  customExpressions; public System.Boolean  customizeAnimationLayers;
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu expressionsMenu; public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters expressionParameters; }
}
}
}
}
#endif
