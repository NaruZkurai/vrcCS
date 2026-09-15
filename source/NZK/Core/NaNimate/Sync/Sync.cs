#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static partial class Sync {
 public static void FX(System.String a,UnityEditor.Animations.BlendTree mainDbt,System.Collections.Generic.List<System.String> tp)
  { /* Merge into the master FX controller path used by the Baker */
    System.String fxPath = Vars.Names.Get.AviRoot(a) + "/Controllers/FX.controller";
    Systems.Folder.Ensure(System.IO.Path.GetDirectoryName(fxPath).Replace('\\','/'));
    var fx = NaNimate.FX.GllC(fxPath);
    /* Copy from generated path if master doesn't exist yet */
    if (fx.layers == null || fx.layers.Length == 0)
    { var genFx = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(Vars.Names.Get.GeneratedFxControllerPath(a));
      if (genFx != null) { UnityEditor.EditorUtility.CopySerialized(genFx,fx); } }
    NaNimate.Sync.FXParams(fx,tp); NaNimate.Sync.Layer2(fx,mainDbt); UnityEditor.EditorUtility.SetDirty(fx); }
  public static void VrcAsset(System.String assetPath,System.Collections.Generic.List<System.String> tp,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType vt)
  { var asset = NaNimate.Params.GllC(assetPath); NaNimate.Sync.Vrc(asset,tp,vt); UnityEditor.EditorUtility.SetDirty(asset); }
  
  public static void Vrc(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters ep,System.Collections.Generic.List<System.String> generatedParams,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType valueType)
  { var merged = ep.parameters != null ? System.Linq.Enumerable.ToList(ep.parameters) :  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();
    var existing = new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(merged, p => p != null && !System.String.IsNullOrEmpty(p.name)), p => p.name),System.StringComparer.Ordinal);
    foreach (System.String n in generatedParams) { if (!existing.Contains(n)) { merged.Add(NaNimate.Params.New(n,valueType)); existing.Add(n); } }
    ep.parameters = merged.ToArray(); }
  public static void Float(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters p,System.Collections.Generic.List<System.String> gen) => NaNimate.Sync.Vrc(p,gen,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Float);
  public static void Bool(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters p,System.Collections.Generic.List<System.String> gen) => NaNimate.Sync.Vrc(p,gen,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool);
  public static void FXParams(UnityEditor.Animations.AnimatorController controller,System.Collections.Generic.List<System.String> generatedParams)
  { var existing = new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(controller.parameters, p => p.name),System.StringComparer.Ordinal);
    if (!existing.Contains(Vars.Consts.MainDbtWeightParameter)) { controller.AddParameter(new UnityEngine.AnimatorControllerParameter { name = Vars.Consts.MainDbtWeightParameter,type = UnityEngine.AnimatorControllerParameterType.Float,defaultFloat = 1f }); existing.Add(Vars.Consts.MainDbtWeightParameter); }
    foreach (System.String n in generatedParams) { if (!existing.Contains(n)) { controller.AddParameter(n,UnityEngine.AnimatorControllerParameterType.Float); existing.Add(n); } } }
  public static void Layer2(UnityEditor.Animations.AnimatorController controller,UnityEditor.Animations.BlendTree mainDbt)
  { var layer = NaNimate.Layer.GllC(controller,Vars.Consts.GeneratedFxLayerName);
    layer.defaultWeight = Vars.Consts.LayerWeight; layer.blendingMode = Vars.Consts.LayerBlendMode; layer.iKPass = Vars.Consts.LayerIKPass;
    var state = NaNimate.Layer.State(layer.stateMachine,Vars.Consts.GeneratedFxStateName); state.motion = mainDbt; state.writeDefaultValues = Vars.Consts.StateWriteDefaults; layer.stateMachine.defaultState = state;
    NaNimate.Layer.Set(controller,layer); UnityEditor.EditorUtility.SetDirty(layer.stateMachine); }
  public static void Menu2(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu rootMenu,System.String a,System.Collections.Generic.List<System.String> generatedParams)
  { rootMenu.controls.Clear();
    if (generatedParams.Count <= Vars.Consts.MenuPageLimit) { generatedParams.ForEach(p => rootMenu.controls.Add(NaNimate.Control.Toggle(p))); return; }
    int pc = (generatedParams.Count + Vars.Consts.MenuPageLimit - 1) / Vars.Consts.MenuPageLimit;
    for (int page = 0; page < pc; page++) { int start = page * Vars.Consts.MenuPageLimit; var pm = NaNimate.Menu.GllC(Vars.Names.Get.MenuFolder(a) + "/" + Vars.Names.Build.GeneratedMenuPageName(a,page + 1) + ".asset"); pm.controls.Clear(); System.Linq.Enumerable.ToList(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(generatedParams,start),System.Math.Min(Vars.Consts.MenuPageLimit,generatedParams.Count - start))).ForEach(p => pm.controls.Add(NaNimate.Control.Toggle(p))); UnityEditor.EditorUtility.SetDirty(pm); rootMenu.controls.Add(NaNimate.Control.SubMenu("Page " + (page + 1),pm)); } } 
}
}
}
}
#endif
