#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Control
  { public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control Toggle(System.String paramName) => new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control { name = paramName,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle,parameter = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter { name = paramName },icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1 };
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control SubMenu(System.String name,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu subMenu) => new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control { name = name,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu,subMenu = subMenu,icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1 }; }
}
}
}
#endif
