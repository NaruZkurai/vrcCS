#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Vars {
public static partial class Names {
public static class Build
  { public static System.String DbtS(System.String a) => Vars.Consts.DbtPrefix + " " + Vars.Names.Sanitize(a);
    public static System.String ItemDbtName(System.String a,System.String c,System.String i) => Vars.Names.Build.DbtS(a) + " " + Vars.Names.Sanitize(c) + " Toggles " + Vars.Names.Sanitize(i);
    public static System.String CategoryDbtName(System.String a,System.String c) => Vars.Names.Build.DbtS(a) + " " + Vars.Names.Sanitize(c);
    public static System.String MainDbtName(System.String a) => Vars.Names.Build.DbtS(a) + " " + Vars.Consts.MainCategoryName;
    public static System.String GeneratedFxName(System.String a) => Vars.Names.Build.DbtS(a) + " FX";
    public static System.String GeneratedExpressionParamsName(System.String a) => Vars.Names.Build.DbtS(a) + " Params";
    public static System.String GeneratedMenuName(System.String a) => Vars.Names.Build.DbtS(a) + " Menu";
    public static System.String GeneratedMenuPageName(System.String a,int p) => Vars.Names.Build.DbtS(a) + " Menu Page " + p.ToString("00");
    public static System.String GeneratedMasterParamsName(System.String a) => Vars.Names.Build.DbtS(a) + " Master Params System.IO.File";
    public static System.String GeneratedToggleParamName(System.String sourceName)
    { if (System.String.IsNullOrEmpty(sourceName)) return Vars.Consts.GeneratedTogglePrefix + "Unnamed";
    System.String n = sourceName.StartsWith("DBT ",System.StringComparison.Ordinal) ? sourceName.Substring(4) : sourceName;
    int i = n.IndexOf(" NaNimations Toggles ",System.StringComparison.Ordinal); if (i >= 0) n = n.Substring(i + " NaNimations Toggles ".Length); else { int j = n.IndexOf(" UnityEngine.Object Toggle Toggles ",System.StringComparison.Ordinal); if (j >= 0) n = n.Substring(j + " UnityEngine.Object Toggle Toggles ".Length); }
    return Vars.Consts.GeneratedTogglePrefix + Vars.Names.Sanitize(n.Replace("NaNimations","NaNim8")); }
    public static UnityEditor.Animations.ChildMotion[] ChildMotionsForFolder(System.String sourceFolder)
    { var results = new System.Collections.Generic.List<UnityEditor.Animations.ChildMotion>();
      System.String absFolder = NaNimate.Paths.Abs(sourceFolder);
      if (!System.IO.Directory.Exists(absFolder)) return results.ToArray();
      foreach (System.String absFile in System.IO.Directory.GetFiles(absFolder,"*.asset")) {
        System.String relPath = "Assets" + absFile.Substring(UnityEngine.Application.dataPath.Length).Replace('\\','/');
        var bt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(relPath);
        if (bt == null) { UnityEditor.AssetDatabase.ImportAsset(relPath,UnityEditor.ImportAssetOptions.ForceSynchronousImport); bt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(relPath); }
        if (bt != null) results.Add(Systems.BlendTrees.Direct(bt));
      }
      return results.ToArray(); } }
}
}
}
}
#endif
