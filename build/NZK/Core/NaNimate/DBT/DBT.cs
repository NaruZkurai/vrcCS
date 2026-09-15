#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class DBT
  { public static UnityEditor.Animations.BlendTree New(System.String name,System.String param)
    => new UnityEditor.Animations.BlendTree { name = name,blendType = UnityEditor.Animations.BlendTreeType.Simple1D,blendParameter = param,useAutomaticThresholds = Vars.Consts.BtAutoThresholds,children = new UnityEditor.Animations.ChildMotion[0] };
  public static UnityEditor.Animations.BlendTree Binary(System.String name,System.String param,UnityEngine.Motion c0,UnityEngine.Motion c1,System.String path)
  { var bt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(path);
    if (bt != null) { if (bt.blendParameter != param) { bt.blendParameter = param; UnityEditor.EditorUtility.SetDirty(bt); } return bt; }
    bt = NaNimate.DBT.New(name,param); bt.AddChild(c0,Vars.Consts.BtThresholdOff); bt.AddChild(c1,Vars.Consts.BtThresholdOn); UnityEditor.AssetDatabase.CreateAsset(bt,path); UnityEditor.EditorUtility.SetDirty(bt); return bt; }
  public static UnityEditor.Animations.BlendTree Direct(System.String path,System.String name)
  { var dbt = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(path); if (dbt != null) return dbt;
    dbt = new UnityEditor.Animations.BlendTree { name = name,blendType = UnityEditor.Animations.BlendTreeType.Direct,blendParameter = Vars.Consts.MainDbtWeightParameter,useAutomaticThresholds = Vars.Consts.BtAutoThresholds };
    UnityEditor.AssetDatabase.CreateAsset(dbt,path); return dbt; }
  public static UnityEditor.Animations.BlendTree Category(System.String a,System.String category)
  { System.String dn = Vars.Names.Build.CategoryDbtName(a,category),btf = Vars.Names.Get.BTFolderCategory(a,category);
    Systems.Folder.Ensure(btf);
    var tree = NaNimate.DBT.Direct(Vars.Names.Get.GetDbtPath(a,dn),dn);
    tree.children = Vars.Names.Build.ChildMotionsForFolder(btf); UnityEditor.EditorUtility.SetDirty(tree); return tree; }
  public static void Rebuild(System.String a)
  { Systems.Folder.Ensure(Vars.Names.Get.AviRoot(a) + "/DBT"); System.String mdn = Vars.Names.Build.MainDbtName(a);
    var mdbt = Direct(Vars.Names.Get.GetDbtPath(a,mdn),mdn);
    mdbt.children = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(System.Linq.Enumerable.Select(Vars.Consts.MainCategoryOrder, c => NaNimate.DBT.Category(a,c)), t => t.children != null && t.children.Length > 0), t => Systems.BlendTrees.Direct(t)));
    UnityEditor.EditorUtility.SetDirty(mdbt); } }
}
}
}
#endif
