#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Update
  { public static void DBTs(System.String a) => NaNimate.DBT.Rebuild(a);
  public static void Sync(System.String a)
  { Systems.Folder.Ensure(Vars.Names.Get.MenuFolder(a)); var tp = NaNimate.Params.Collect(a); System.String mdbtp = Vars.Names.Get.GetDbtPath(a,Vars.Names.Build.MainDbtName(a));
    var mdbt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(mdbtp); if (mdbt == null) { Vars.Errors.Warn(Vars.Errors.E_NoMainDbt,"Main DBT not found at " + mdbtp + " — run DBTs() first or regenerate toggles."); return; }
    NaNimate.Update.SyncAssets(a,mdbt,mdbtp,tp); }
  public static void SyncAssets(System.String a,UnityEditor.Animations.BlendTree mdbt,System.String mdbtp,System.Collections.Generic.List<System.String> tp)
  { NaNimate.Sync.FX(a,mdbt,tp); NaNimate.Sync.VrcAsset(Vars.Names.Get.GeneratedExpressionParamsPath(a),tp,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool); NaNimate.Sync.VrcAsset(Vars.Names.Get.GeneratedFxParamsPath(a),tp,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Float); NaNimate.Sync.Asset.Menu(Vars.Names.Get.GeneratedMenuPath(a),a,tp);
    NaNimate.Write.MasterParams(a,Vars.Names.Get.GeneratedMasterParamsPath(a),Vars.Names.Get.GeneratedFxControllerPath(a),Vars.Names.Get.GeneratedExpressionParamsPath(a),Vars.Names.Get.GeneratedMenuPath(a),mdbtp,tp);
    /* Assign generated assets to avatar VRCAD */
    var aviRoot = Systems.Baking.FindAviRoot(a);
    if (aviRoot == null) { var vrcads = UnityEngine.Resources.FindObjectsOfTypeAll<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(); foreach (var v in vrcads) { if (v != null && v.gameObject != null && v.gameObject.scene != null && v.gameObject.scene.isLoaded) { var rn = v.gameObject.transform.root.name; if (Vars.Names.Sanitize(rn) == a) { aviRoot = v.gameObject; break; } } } }
    if (aviRoot != null)
    { var vrcad = aviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
      if (vrcad != null)
      { var menu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(Vars.Names.Get.GeneratedMenuPath(a));
        var prm = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(Vars.Names.Get.GeneratedExpressionParamsPath(a));
        if (menu != null) { vrcad.expressionsMenu = menu; vrcad.customExpressions = true; UnityEditor.EditorUtility.SetDirty(vrcad); }
        if (prm != null) vrcad.expressionParameters = prm; } } }
  public static void SyncFromDbt(System.String a) => NaNimate.Update.Sync(a); }
}
}
}
#endif
