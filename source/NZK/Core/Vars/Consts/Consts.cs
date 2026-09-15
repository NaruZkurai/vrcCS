#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Vars {
public static class Consts
  { public const System.String RootFolder = "Assets/!_NZK_Generated";
  public const System.String DbtPrefix = "DBT";
  public const System.String MainCategoryName = "Main";
  public const System.String NaNimationCategoryName = "NaNimations";
  public const System.String ObjectToggleCategoryName = "UnityEngine.Object Toggle";
  public const System.String BlendshapeCategoryName = "Blendshape";
  public const System.String MainDbtWeightParameter = "(f)Weight";
  public const System.String GeneratedFolder = Vars.Consts.RootFolder; /* Will be avatar-scoped in methods */
  public const System.String GeneratedMenuPagesFolder = Vars.Consts.RootFolder; /* Will be avatar-scoped in methods */
  public const System.String GeneratedFxLayerName = "NZK DBT Driver";
  public const System.String GeneratedFxStateName = "DBT Driver";
  public const System.String GeneratedTogglePrefix = "(b-gt)";
  public const System.String GeneratedSwitchPrefix = GeneratedTogglePrefix;
  public static readonly System.String[] MainCategoryOrder = { NaNimationCategoryName,/* slot 0 */ ObjectToggleCategoryName,/* slot 1 */ BlendshapeCategoryName /* slot 2 */ };
  public const System.String HbPrefix = "HB_";
  public const System.String HbMeshBuilder = "Mesh Output";
  public const System.String HbAviRootBuilder = "Avatar Root Builder";
  public const System.String HbToggleGenerator = "Toggle Generator";
  public const System.String HbSpsBuilder = "SPS Builder";
  public const System.String HbArmatureLinks = "ArmatureLinks";
  public const System.String HbBakedControllers = "BakedControllers";
  public const System.String NzkRootFolder = "Assets/!_NZK_Generated";
  public const float LayerWeight = 1f;
  public const System.Boolean LayerIKPass = false;
  public const int LayerSyncedIndex = -1;
  public const System.Boolean LayerSyncedTiming = false;
  public const UnityEditor.Animations.AnimatorLayerBlendingMode LayerBlendMode = UnityEditor.Animations.AnimatorLayerBlendingMode.Override;
  public const System.Boolean StateWriteDefaults = true;
  public const float BtTimeScale = 1f;
  public const float BtCycleOffset = 0f;
  public const float BtChildThreshold = 1f;
  public const System.Boolean BtMirror = false;
  public const System.Boolean BtAutoThresholds = false;
  public const float BtThresholdOff = 0f;
  public const float BtThresholdOn = 1f;
  public const float VrcParamDefault = 0f;
  public const System.Boolean VrcParamSaved = true;
  public const System.Boolean VrcParamSynced = true;
  public const int MenuPageLimit = 8;
  public const System.String AnimSuffixNaNim = "_1_NaN.anim";
  public const System.String AnimSuffixNormal = "_0_Normal.anim";
  public const System.String AnimSuffixOn = "_0_on.anim";
  public const System.String AnimSuffixOff = "_1_off.anim"; }
}
}
}
#endif
