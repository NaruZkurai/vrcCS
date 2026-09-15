#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class PlayableLayers
  {
  static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType[] BaseTypes = new[]
  { VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Additive,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Action,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX,};
  static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType[] SpecialTypes = new[]
  { VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Sitting,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.TPose,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.IKPose,};
  // Assign a single layer. Writes directly to the VRCAD array (struct-safe).
  public static void AssignLayer(
    VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType layerType,UnityEngine.RuntimeAnimatorController controller,System.Boolean isEnabled = true,System.Boolean isDefault = false,UnityEngine.AvatarMask mask = null)
  { if (vrcad == null) return;
    // Find which array and index this layer type maps to
    for (int i = 0; i < BaseTypes.Length; i++)
    {
    if (BaseTypes[i] == layerType)
    {
      if (vrcad.baseAnimationLayers == null || vrcad.baseAnimationLayers.Length < 5)
      vrcad.baseAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[5];
      vrcad.baseAnimationLayers[i].type = layerType;
      vrcad.baseAnimationLayers[i].animatorController = controller;
      vrcad.baseAnimationLayers[i].isEnabled = isEnabled;
      vrcad.baseAnimationLayers[i].isDefault = isDefault || controller == null;
      vrcad.baseAnimationLayers[i].mask = mask;
      return; }
    }
    for (int i = 0; i < SpecialTypes.Length; i++)
    {
    if (SpecialTypes[i] == layerType)
    {
      if (vrcad.specialAnimationLayers == null || vrcad.specialAnimationLayers.Length < 3)
      VRCAD.Set.InitAllLayers(vrcad); // InitAllLayers handles both base and special
      vrcad.specialAnimationLayers[i].type = layerType;
      vrcad.specialAnimationLayers[i].animatorController = controller;
      vrcad.specialAnimationLayers[i].isEnabled = isEnabled;
      vrcad.specialAnimationLayers[i].isDefault = isDefault || controller == null;
      vrcad.specialAnimationLayers[i].mask = mask;
      return; }
    }
  }
  // Assign all 8 layers at once from a dictionary. Keys missing from the dict keep their current value.
  public static void AssignAll(
    VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,System.Collections.Generic.Dictionary<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType,UnityEngine.RuntimeAnimatorController> layers)
  { if (vrcad == null || layers == null) return;
    foreach (var kvp in layers)
    AssignLayer(vrcad,kvp.Key,kvp.Value); }
  }
}
}
}
#endif
