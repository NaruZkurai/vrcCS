#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class UnpackConsts
{ public const System.String MenuAsset = "Assets/nzk unpack";
  public const System.String MenuContextCtrl = "CONTEXT/UnityEditor.Animations.AnimatorController/nzk unpack";
  public const System.String MenuContextMenu = "CONTEXT/VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu/nzk unpack";
  public const System.String NameClip = "clip";
  public const System.String NameBT = "bt";
  public const System.String NameMenu = "menu";
  public const System.String PrefixUnpacked = "u_";
  public const System.String UnknownLayer = "u_FX_Animations_Unknown_layer_NZK";
  public const System.String FolderAnims = "animations";
  public const System.String FolderBTs = "bts";
  public const System.String FolderMenus = "menus";
  public const System.String FolderMasks = "masks";
  public const System.String SfxController = ".controller";
  public const System.String SfxAnim = ".anim";
  public const System.String SfxAsset = ".asset";
  public const System.String SfxMask = ".mask";
  public const System.String FxParamsAsset = "fx_params.asset";
  public const System.String LogPrefix = "[nzk unpack] ";
  public const System.String LogBadPath = "[HB] " + "invalid source path";
  public const System.String LogBadFolder = "[HB] " + "invalid source folder";
  public const System.String LogOutFolder = "[HB] " + "output folder create failed";
  public const System.String LogSubFolder = "[HB] " + "subfolder create failed";
  public const System.String LogCloneFail = "[HB] " + "asset clone failed: ";
  public const System.String LogLoadFail = "[HB] " + "cloned asset load failed";
  public const System.String LogMoveFail = "[HB] " + "failed removing embedded motion: ";
  public const System.String LogMovePassFail = "[HB] " + "embedded motions still present after detach pass";
  public const System.String LogAnimPath = "[HB] " + "animation path invalid";
  public const System.String LogBTPath = "[HB] " + "blendtree path invalid";
  public const System.String LogMenuPath = "[HB] " + "menu path invalid";
  public const System.String LogDone = "[HB] " + "unpacked ";
  public const System.String LogTo = " to ";
  public const System.Boolean VrcSaved = true; }
}
}
#endif
