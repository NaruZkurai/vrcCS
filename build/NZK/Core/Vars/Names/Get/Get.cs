#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Vars {
public static partial class Names {
public static class Get
  { public static System.String MenuAsset(System.String a,System.String name,System.String ext) => Vars.Names.Get.MenuFolder(a) + "/" + name + ext;
    public static System.String AviRoot(System.String a) => Vars.Consts.RootFolder + "/" + Vars.Names.Sanitize(a);
    public static System.String Avatar(UnityEngine.GameObject sel) => Vars.Names.Sanitize(sel.transform.root.name);
    public static System.String AnimFolderCategory(System.String a,System.String c) => Vars.Names.Get.AviRoot(a) + "/Animations/" + c;
    public static System.String BTFolderCategory(System.String a,System.String c) => Vars.Names.Get.AviRoot(a) + "/BlendTrees/" + c;
    public static System.String GetDbtPath(System.String a,System.String dbt) => Vars.Names.Get.AviRoot(a) + "/DBT/" + dbt + ".asset";
    public static System.String MenuFolder(System.String a) => Vars.Names.Get.AviRoot(a) + "/Menus";
    public static System.String GeneratedFxParamsPath(System.String a) => Vars.Names.Get.MenuAsset(a,Vars.Names.Build.GeneratedExpressionParamsName(a) + "_FX",".asset");
    public static System.String GeneratedExpressionParamsPath(System.String a) => Vars.Names.Get.MenuAsset(a,Vars.Names.Build.GeneratedExpressionParamsName(a),".asset");
    public static System.String GeneratedFxControllerPath(System.String a) => Vars.Names.Get.MenuAsset(a,Vars.Names.Build.GeneratedFxName(a),".controller");
    public static System.String GeneratedMenuPath(System.String a) => Vars.Names.Get.MenuAsset(a,Vars.Names.Build.GeneratedMenuName(a),".asset");
    public static System.String GeneratedMasterParamsPath(System.String a) => Vars.Names.Get.MenuAsset(a,Vars.Names.Build.GeneratedMasterParamsName(a),".txt");
    public static System.String ItemAnimPath(System.String a,System.String c,System.String dbt,System.String suffix) => Vars.Names.Get.AnimFolderCategory(a,c) + "/" + dbt + suffix;
    public static System.String ItemBlendTreePath(System.String a,System.String c,System.String dbt) => Vars.Names.Get.BTFolderCategory(a,c) + "/" + dbt + ".asset"; }
}
}
}
}
#endif
