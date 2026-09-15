#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class SPS
  { public static void BakeSelected()
  { var objs = UnityEditor.Selection.gameObjects;
    if (objs == null || objs.Length == 0) { Systems.Warnings.NoObjects("SPS Bake"); return; }
    foreach (var obj in objs) { if (obj == null) continue; Systems.SPS.CreateSPSBuilderTree(obj); } }
  static void CreateSPSBuilderTree(UnityEngine.GameObject target)
  { System.String avatarName = Vars.Names.Sanitize(target.transform.root.name);
    var spsRoot = new UnityEngine.GameObject("SPS Builder");
    spsRoot.transform.SetParent(target.transform,false);
    IF_UE.RegCr(spsRoot,"Create SPS Builder");
    var hb = spsRoot.AddComponent<C_AviGenerator>();
    hb.mode = E_AviGeneratorMode.SpsGenerator;
    hb.avatarRootName = avatarName;
    var config = spsRoot.AddComponent<SpsConfig>();
    config.generationVersion = "1.0.0";
    config.avatarName = avatarName;
    config.layerName = "SPS";
    config.toggleParamName = "(b-gt)SPS";
    var smrs = target.transform.root.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>();
    var allBones = new System.Collections.Generic.HashSet<UnityEngine.Transform>();
    foreach (var smr in smrs)
    if (smr.bones != null)
      foreach (var b in smr.bones)
      if (b != null) allBones.Add(b);
    foreach (var bone in allBones)
    { var nameLower = bone.name.ToLower();
    if (nameLower.Contains("sps") || nameLower.Contains("physics") || nameLower.Contains("jiggle") || nameLower.Contains("boob") || nameLower.Contains("bounce"))
      config.boneEntries.Add(new SpsBoneEntry { boneTransform = bone,chainName = bone.name,radius = 0.05f,stiffness = 0.2f }); }
    hb.ReloadChildren();
    SpsLayerBuilderStub.BuildAll(config);
    UnityEngine.Debug.Log("[SPS] Builder created for " + target.name + " (v" + config.generationVersion + ")"); }
  public static void GenerateAll()
  { var allSps = UnityEngine.Object.FindObjectsOfType<SpsConfig>(true);
    foreach (var sc in allSps) { SpsLayerBuilderStub.BuildAll(sc); UnityEngine.Debug.Log("[SPS] Generated: " + sc.name); } } }
}
}
}
#endif
