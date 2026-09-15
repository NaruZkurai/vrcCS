#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class CircleMenuBakerStub
  {
  public static void PopulateMenuSlots(System.String avatarName,UnityEngine.GameObject avatarRoot,System.String genFolder,System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control> allControls,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad)
  { if (avatarRoot == null || vrcad == null || allControls == null || allControls.Count == 0) return;
    // Find or create BakedMenuSlot on avatar root
    var slot = avatarRoot.GetComponent<BakedMenuSlot>();
    if (slot == null) slot = avatarRoot.AddComponent<BakedMenuSlot>();
    int pages = (allControls.Count + 7) / 8;
    slot.pageNumber = 0; slot.isRoot = true; slot.controlCount = allControls.Count;
    // Assign expression menu and params if available
    var menuPath = Vars.Names.Get.GeneratedMenuPath(avatarName);
    var paramPath = Vars.Names.Get.GeneratedExpressionParamsPath(avatarName);
    var menu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(menuPath);
    var prm = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(paramPath);
    if (menu != null) { slot.menu = menu; vrcad.expressionsMenu = menu; }
    if (prm != null) vrcad.expressionParameters = prm;
    if (menu != null) vrcad.customExpressions = true;
    UnityEditor.EditorUtility.SetDirty(slot); UnityEditor.EditorUtility.SetDirty(vrcad);
    UnityEngine.Debug.Log("[CircleMenu] Populated " + allControls.Count + " controls across " + pages + " page(s) for " + avatarName); }
  }
}
}
#endif
