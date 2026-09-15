namespace NZK
{
public static partial class Core {
public static class NZKPaths
  {
    public const System.String Root = "Assets/!_NZK_Generated";
    public const System.String Controllers = "/Controllers";
    public const System.String Animations = "/Animations";
    public const System.String BlendTrees = "/BlendTrees";
    public const System.String Menus = "/Menus";
    public const System.String Meshes = "/Meshes";
    public const System.String Visemes = "/Visemes";
    public const System.String Avatars = "/Avatars";
    public const System.String ToggleGen = "/ToggleGenerator";
    public const System.String SPS = "/SPS";
    /** <summary>Build avatar-scoped root path: Root/SanitizedName</summary> */
    public static System.String AviRoot(System.String avatarName) =>
      Root + "/" + ParamUtil.SanitizeFileName(avatarName);
    /* ── Dependency paths ──────────────────────────────────────── */
    public const System.String DepRoot = "Assets/NZK toolkit v4/Dependencies";
    public const System.String DepGestureRef = DepRoot + "/default nzk Gesture controller.controller";
    public const System.String DepGestureMaskL = DepRoot + "/GestureDefaults/L_Hand.mask";
    public const System.String DepGestureMaskR = DepRoot + "/GestureDefaults/R_Hand.mask";
    public const System.String DepTestingScene = DepRoot + "/Testing scene input.unity";
    public const System.String DepGesturePrefab = DepRoot + "/HB_Gesture.prefab";
    /** <summary>Build a sub-path under UnityEngine.Avatar root: AviRoot/subFolder/rest</summary> */
    public static System.String AvatarPath(System.String avatarName, System.String subFolder, System.String rest = "") =>
      AviRoot(avatarName) + subFolder + rest;
  }
}
}
