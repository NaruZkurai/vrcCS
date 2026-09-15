#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class BlendShapeToggleGenerator
  {
  public const System.String BSCategory = "Blendshape";
  public const System.String BSAnimSuffixOff = "_0_off.anim";
  public const System.String BSAnimSuffixOn = "_1_on.anim";
  public static UnityEngine.AnimationClip GenerateBlendshapeClip(UnityEngine.SkinnedMeshRenderer smr,System.String shapeName,float value,System.String clipName)
  { if (smr == null || System.String.IsNullOrEmpty(shapeName)) return null;
    var clip = new UnityEngine.AnimationClip { name = clipName };
    var path = UnityEditor.AnimationUtility.CalculateTransformPath(smr.transform,smr.transform.root);
    UnityEditor.AnimationUtility.SetEditorCurve(clip,UnityEditor.EditorCurveBinding.FloatCurve(path,typeof(UnityEngine.SkinnedMeshRenderer),"blendShape." + shapeName),UnityEngine.AnimationCurve.Constant(0f,0f,value));
    return clip; }
  public static UnityEditor.Animations.BlendTree CreateBinaryBlendshapeDBT(System.String name,System.String paramName,UnityEngine.AnimationClip onClip,UnityEngine.AnimationClip offClip,System.String path)
  { var bt = IF_UE.Load<UnityEditor.Animations.BlendTree>(path);
    if (bt != null) { if (bt.blendParameter != paramName) { bt.blendParameter = paramName; IF_UE.SetDirty(bt); } return bt; }
    bt = new UnityEditor.Animations.BlendTree { name = name,blendType = UnityEditor.Animations.BlendTreeType.Simple1D,blendParameter = paramName,useAutomaticThresholds = false,children = new UnityEditor.Animations.ChildMotion[0] };
    bt.AddChild(offClip,0f);
    bt.AddChild(onClip,1f);
    IF_UE.CreateAsset(bt,path); return bt; }
  public static System.String GenerateToggle(UnityEngine.GameObject target,System.String avatarName,System.String categoryName)
  { var btd = target.GetComponent<BlendshapeToggleDefinition>();
    if (btd == null || btd.targetRenderer == null || System.String.IsNullOrEmpty(btd.blendshapeName)) { UnityEngine.Debug.LogError("BlendshapeToggleDefinition missing or incomplete on " + target.name); return null; }
    var sanitizedName = BTH.S(btd.parameterName ?? target.name);
    var paramName = BTH.TglPrefix + sanitizedName;
    var smr = btd.targetRenderer;
    var cat = System.String.IsNullOrEmpty(categoryName) ? BSCategory : categoryName;
    System.IO.Directory.CreateDirectory(BTH.AF(avatarName,cat));
    System.IO.Directory.CreateDirectory(BTH.BF(avatarName,cat));
    var offClip = GenerateBlendshapeClip(smr,btd.blendshapeName,btd.offValue,sanitizedName + "_Off");
    var onClip = GenerateBlendshapeClip(smr,btd.blendshapeName,btd.onValue,sanitizedName + "_On");
    if (offClip == null || onClip == null) { UnityEngine.Debug.LogError("Failed to generate blend shape clips for " + target.name); return null; }
    var finalOff = BTH.LCC(BTH.IAP(avatarName,cat,sanitizedName,BSAnimSuffixOff),offClip);
    var finalOn = BTH.LCC(BTH.IAP(avatarName,cat,sanitizedName,BSAnimSuffixOn),onClip);
    var btPath = BTH.IBP(avatarName,cat,sanitizedName);
    CreateBinaryBlendshapeDBT(sanitizedName,paramName,finalOn,finalOff,btPath);
    btd.parameterName = paramName;
    IF_UE.SetDirty(btd);
    UnityEngine.Debug.Log("Blendshape toggle created for " + target.name + " -> " + btPath);
    return paramName; }
  }
}
}
#endif
