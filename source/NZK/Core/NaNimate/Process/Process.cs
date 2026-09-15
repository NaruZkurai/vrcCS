#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Process
  { public static void NaN(UnityEngine.GameObject sel,System.String a)
  { System.String dbt = Vars.Names.Build.ItemDbtName(a,Vars.Consts.NaNimationCategoryName,sel.name),op = UnityEditor.AnimationUtility.CalculateTransformPath(sel.transform,sel.transform.root);
    Systems.Folder.Toggle(a,Vars.Consts.NaNimationCategoryName);
    System.String normPath = Vars.Names.Get.ItemAnimPath(a,Vars.Consts.NaNimationCategoryName,dbt,Vars.Consts.AnimSuffixNormal);
    System.String nanPath = Vars.Names.Get.ItemAnimPath(a,Vars.Consts.NaNimationCategoryName,dbt,Vars.Consts.AnimSuffixNaNim);
    var normClip = NaNimate.Clip.GllC(normPath,NaNimate.Clip.Scale(op,sel.transform.localScale));
    var nanClip = NaNimate.Clip.GllC(nanPath,NaNimate.Clip.Scale(op,UnityEngine.Vector3.zero));
    if (nanClip != null) NaNimate.Fix.FixAnimationClipScale(nanClip);
    NaNimate.Process.Finalize(sel,dbt,Vars.Names.Build.GeneratedToggleParamName(dbt),normClip,nanClip,Vars.Names.Get.ItemBlendTreePath(a,Vars.Consts.NaNimationCategoryName,dbt),"NaNimate"); }
  public static void Toggle(UnityEngine.GameObject sel,System.String a)
  { System.String dbt = Vars.Names.Build.ItemDbtName(a,Vars.Consts.ObjectToggleCategoryName,sel.name),op = UnityEditor.AnimationUtility.CalculateTransformPath(sel.transform,sel.transform.root);
    Systems.Folder.Toggle(a,Vars.Consts.ObjectToggleCategoryName);
    NaNimate.Process.Finalize(sel,dbt,Vars.Names.Build.GeneratedToggleParamName(dbt),NaNimate.Clip.GllC(Vars.Names.Get.ItemAnimPath(a,Vars.Consts.ObjectToggleCategoryName,dbt,Vars.Consts.AnimSuffixOn),NaNimate.Clip.Active(op,sel.activeSelf)),NaNimate.Clip.GllC(Vars.Names.Get.ItemAnimPath(a,Vars.Consts.ObjectToggleCategoryName,dbt,Vars.Consts.AnimSuffixOff),NaNimate.Clip.Active(op,!sel.activeSelf)),Vars.Names.Get.ItemBlendTreePath(a,Vars.Consts.ObjectToggleCategoryName,dbt),"UnityEngine.Object Toggle"); }
  public static void Finalize(UnityEngine.GameObject sel,System.String dbtName,System.String paramName,UnityEngine.AnimationClip c0,UnityEngine.AnimationClip c1,System.String btPath,System.String label)
  { if (c0 == null || c1 == null) { Vars.Errors.Log(Vars.Errors.E_NullClip,label + " failed to bind clips. c0=" + (c0 != null) + " c1=" + (c1 != null) + "\nbtPath=" + btPath); return; }
    UnityEditor.Selection.activeObject = NaNimate.DBT.Binary(dbtName,paramName,c0,c1,btPath);
    UnityEngine.Debug.Log(label + " created for " + sel.name + ":\n- UnityEditor.Animations.BlendTree: " + btPath + "\n- Blend Parameter: " + paramName); } }
}
}
}
#endif
