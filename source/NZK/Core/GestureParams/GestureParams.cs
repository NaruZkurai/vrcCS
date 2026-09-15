namespace NZK
{
public static partial class Core {
public static class GestureParams
  {
    public const System.String GestureLeft = "GestureLeft";
    public const System.String GestureRight = "GestureRight";
    public const System.String GestureLeftW = "GestureLeftWeight";
    public const System.String GestureRightW = "GestureRightWeight";
    public const System.String GestureW = "GestureWeight";
    public const System.String GestureLR = "GestureLeftRight";
    public const System.String GestureLRW = "GestureLeftRightWeight";
    public const System.String Viseme = "Viseme";
    public const System.String TogglePrefix = "(b-gt)";
    /* VRChat built-in params that should never be rewritten */
    public static readonly System.Collections.Generic.HashSet<System.String> VRChatBuiltIns = new()
  { "IsLocal","PreviewMode","Viseme","Voice","GestureLeft","GestureRight","GestureLeftWeight","GestureRightWeight","AngularY","VelocityX","VelocityY","VelocityZ","VelocityMagnitude","Upright","Grounded","Seated","AFK","TrackingType","VRMode","MuteSelf","InStation","Earmuffs","IsOnFriendsList","AvatarVersion","IsAnimatorEnabled","ScaleModified","ScaleFactor","ScaleFactorInverse","EyeHeightAsMeters","EyeHeightAsPercent"
  };
  }
}
}
