#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class Toggle
  { public static void Create(System.String toggleType,UnityEngine.GameObject[] objs)
  { /* Find or create generator hierarchy for rebuildability */
    foreach (var go in objs)
    { if (go == null) continue;
      var avatarName = Vars.Names.Sanitize(go.transform.root.name);
      var aviRoot = Systems.Baking.FindAviRoot(avatarName,go);
      if (aviRoot == null) aviRoot = go.transform.root.gameObject;
      var genRoot = Systems.Baking.GetGenRoot(avatarName,aviRoot);
      var tChild = genRoot.Find("Toggle Generator");
      UnityEngine.GameObject tgo;
      if (tChild == null) { tgo = new UnityEngine.GameObject("Toggle Generator"); tgo.transform.SetParent(genRoot,false); }
      else tgo = tChild.gameObject;
      var hb = tgo.GetComponent<C_AviGenerator>();
      if (hb == null) hb = tgo.AddComponent<C_AviGenerator>();
      hb.mode = E_AviGeneratorMode.ToggleGenerator; hb.avatarRootName = avatarName; hb.NZKC_GO_AviRoot = aviRoot;
      if (toggleType == Vars.ToggleGeneratorMode_NaNimation) { if (!hb.nanimationSources.Contains(go)) hb.nanimationSources.Add(go); }
      else if (toggleType == Vars.ToggleGeneratorMode_onoff) { if (!hb.objectToggleSources.Contains(go)) hb.objectToggleSources.Add(go); }
      else { if (!hb.blendshapeSources.Contains(go)) hb.blendshapeSources.Add(go); } }
    /* Run the actual generation */
    if (toggleType == Vars.ToggleGeneratorMode_NaNimation) Systems.Toggle.NaNimate(objs);
    else if (toggleType == Vars.ToggleGeneratorMode_onoff) Systems.Toggle.OnOff(objs);
    else Systems.Toggle.Blendshape(objs); }
  public static void NaNimate(UnityEngine.GameObject[] objs) => Systems.Toggle.Process(objs,NZK.Core.NaNimate.Process.NaN);
  public static void OnOff(UnityEngine.GameObject[] objs) => Systems.Toggle.Process(objs,NZK.Core.NaNimate.Process.Toggle);
  public static void Blendshape(UnityEngine.GameObject[] objs)
  { if (objs == null || objs.Length == 0) return;
    Systems.Folder.Ensure(Vars.Consts.RootFolder);
    var set = new System.Collections.Generic.HashSet<System.String>();
    foreach (var obj in objs)
    { if (obj == null) continue;
      BlendshapeMenuActions.CreateBlendshapeToggleSingle(obj);
      set.Add(Vars.Names.Sanitize(obj.transform.root.name)); }
    UnityEditor.AssetDatabase.Refresh();
    foreach (var a in set) { NZK.Core.NaNimate.Update.DBTs(a); NZK.Core.NaNimate.Update.Sync(a); }
    UnityEditor.AssetDatabase.SaveAssets(); }
  public static void Process(UnityEngine.GameObject[] objs,System.Action<UnityEngine.GameObject,System.String> processItem)
  { Systems.Folder.Ensure(Vars.Consts.RootFolder); var set = new System.Collections.Generic.HashSet<System.String>();
    /* Suppress inspector repaint during batch asset creation */
    var prevSel = UnityEditor.Selection.activeGameObject;
    UnityEditor.Selection.activeObject = null;
    try { foreach (var sel in objs) { System.String a = Vars.Names.Get.Avatar(sel); set.Add(a); processItem(sel,a); } }
    catch { UnityEditor.Selection.activeObject = prevSel; throw; }
    UnityEditor.AssetDatabase.Refresh();
    foreach (System.String a in set) { NZK.Core.NaNimate.Update.DBTs(a); NZK.Core.NaNimate.Update.Sync(a); }
    UnityEditor.AssetDatabase.SaveAssets();
    UnityEditor.Selection.activeObject = prevSel; } }
}
}
}
#endif
