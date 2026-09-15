#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Params
  { public static System.Collections.Generic.List<System.String> Collect(System.String a)
  { var results = new System.Collections.Generic.List<System.String>();
    System.String btf = Vars.Names.Get.AviRoot(a) + "/BlendTrees";
    System.String absBtf = NaNimate.Paths.Abs(btf);
    if (!System.IO.Directory.Exists(absBtf)) return results;
    if (!UnityEditor.AssetDatabase.IsValidFolder(btf)) UnityEditor.AssetDatabase.ImportAsset(btf,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
    foreach (System.String absFile in System.IO.Directory.GetFiles(absBtf,"*.asset",System.IO.SearchOption.AllDirectories)) {
      System.String relPath = "Assets" + absFile.Substring(UnityEngine.Application.dataPath.Length).Replace('\\','/');
      if (!relPath.StartsWith(btf + "/",System.StringComparison.Ordinal)) continue;
      var bt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(relPath);
      if (bt == null) { UnityEditor.AssetDatabase.ImportAsset(relPath,UnityEditor.ImportAssetOptions.ForceSynchronousImport); bt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(relPath); }
      if (bt != null) results.Add(Vars.Names.Build.GeneratedToggleParamName(System.IO.Path.GetFileNameWithoutExtension(relPath)));
    }
    results.Sort(System.StringComparer.OrdinalIgnoreCase);
    return results; }
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters Get(System.String path)
    => UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(path);
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters Create(System.String path)
  { var p = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(); p.parameters = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter[0];
    UnityEditor.AssetDatabase.CreateAsset(p,path); return p; }
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters GllC(System.String path)
    => NaNimate.Params.Get(path) ?? NaNimate.Params.Create(path);
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter New(System.String n,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType vt)
    => new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter { name = n,valueType = vt,defaultValue = Vars.Consts.VrcParamDefault,saved = Vars.Consts.VrcParamSaved,networkSynced = Vars.Consts.VrcParamSynced }; }
}
}
}
#endif
