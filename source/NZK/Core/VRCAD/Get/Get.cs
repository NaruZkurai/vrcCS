#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class VRCAD {
public static class Get
{ public static UnityEngine.RuntimeAnimatorController Controller(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType layerType)
  { if (vrcad == null) return null;
    if (vrcad.baseAnimationLayers != null)
    for (int i = 0; i < vrcad.baseAnimationLayers.Length; i++)
      if (vrcad.baseAnimationLayers[i].type == layerType) return vrcad.baseAnimationLayers[i].animatorController;
    if (vrcad.specialAnimationLayers != null)
    for (int i = 0; i < vrcad.specialAnimationLayers.Length; i++)
      if (vrcad.specialAnimationLayers[i].type == layerType) return vrcad.specialAnimationLayers[i].animatorController;
    return null; }
  public static UnityEngine.Vector3 ViewPosition(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad) => vrcad != null ? vrcad.ViewPosition : UnityEngine.Vector3.zero;
}
}
}
}
#endif
