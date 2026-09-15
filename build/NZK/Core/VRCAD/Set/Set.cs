#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class VRCAD {
public static class Set{
  public static void ViewPosition(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,UnityEngine.Vector3 pos)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set ViewPosition"); vrcad.ViewPosition = pos; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void LipSync(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle style)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set LipSync"); vrcad.lipSync = style; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void VisemeMesh(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,UnityEngine.SkinnedMeshRenderer smr)
  { if (vrcad != null && smr != null) { UnityEditor.Undo.RecordObject(vrcad,"Set VisemeMesh"); vrcad.VisemeSkinnedMesh = smr; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void VisemeBlendShapes(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,System.String[] shapes)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set VisemeBlendShapes"); vrcad.VisemeBlendShapes = shapes; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void CustomExpressions(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,System.Boolean val)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set CustomExpressions"); vrcad.customExpressions = val; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void CustomizeAnimLayers(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,System.Boolean val)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set CustomizeAnimLayers"); vrcad.customizeAnimationLayers = val; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void ExpressionsMenu(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set ExpressionsMenu"); vrcad.expressionsMenu = menu; vrcad.customExpressions = true; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void ExpressionParams(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters p)
  { if (vrcad != null) { UnityEditor.Undo.RecordObject(vrcad,"Set ExpressionParams"); vrcad.expressionParameters = p; UnityEditor.EditorUtility.SetDirty(vrcad); } }
  public static void EyeLookSettings(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,UnityEngine.Transform leftEye,UnityEngine.Transform rightEye,UnityEngine.SkinnedMeshRenderer eyelidsMesh = null,int[] eyelidsBlendshapes = null)
  { if (vrcad == null) return; UnityEditor.Undo.RecordObject(vrcad,"Set EyeLookSettings");
    vrcad.customEyeLookSettings.leftEye = leftEye; vrcad.customEyeLookSettings.rightEye = rightEye;
    if (eyelidsMesh != null) vrcad.customEyeLookSettings.eyelidsSkinnedMesh = eyelidsMesh;
    if (eyelidsBlendshapes != null) vrcad.customEyeLookSettings.eyelidsBlendshapes = eyelidsBlendshapes;
    vrcad.enableEyeLook = leftEye != null || rightEye != null; UnityEditor.EditorUtility.SetDirty(vrcad); }
  public static void InitAllLayers(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad)
  { if (vrcad == null) return; UnityEditor.Undo.RecordObject(vrcad,"Init animation layers");
    vrcad.baseAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[5];
    var baseTypes = new[] { VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Additive,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Action,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX };
    for (int i = 0; i < 5; i++)
    vrcad.baseAnimationLayers[i] = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer { type = baseTypes[i],isEnabled = true,isDefault = true };
    vrcad.specialAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[3];
    var specialTypes = new[] { VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Sitting,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.TPose,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.IKPose };
    for (int i = 0; i < 3; i++)
    vrcad.specialAnimationLayers[i] = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer { type = specialTypes[i],isEnabled = true,isDefault = true };
    UnityEditor.EditorUtility.SetDirty(vrcad); }
      public static void BaseLayerSlot(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,int slot,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType type,UnityEngine.RuntimeAnimatorController ctrl)
    { if (vrcad == null || vrcad.baseAnimationLayers == null || slot >= vrcad.baseAnimationLayers.Length) return;
      vrcad.baseAnimationLayers[slot].type = type;
      vrcad.baseAnimationLayers[slot].animatorController = ctrl;
      vrcad.baseAnimationLayers[slot].isEnabled = true;
      vrcad.baseAnimationLayers[slot].isDefault = ctrl == null;
      UnityEditor.EditorUtility.SetDirty(vrcad); }
  public static void ControllerSlot(C_AviGenerator hb,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType layerType,UnityEngine.RuntimeAnimatorController controller,System.Boolean isEnabled = true,System.Boolean isDefault = false)
  { if (hb == null || hb.NZKC_GO_AviRoot == null) return;
    var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad == null) return;
    Systems.PlayableLayers.AssignLayer(vrcad,layerType,controller,isEnabled,isDefault); }
}
}
}
}
#endif
