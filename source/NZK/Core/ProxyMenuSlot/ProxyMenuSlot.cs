namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Proxy Menu Slot")]
  public class ProxyMenuSlot : UnityEngine.MonoBehaviour
  {
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType controlType;
    public System.String controlName;
    public System.String parameterName;
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter parameter = new();
    public float value;
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu subMenu;
    public System.Boolean subMenuBuilt;
    public UnityEngine.Texture2D icon;
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[] labels;
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style style;
    public System.Boolean isQuickActions;
    public void CopyFrom(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control ctrl, int slotIndex)
    {
      controlType = ctrl.type;
      controlName = ctrl.name;
      parameterName = ctrl.parameter?.name ?? "";
      parameter = ctrl.parameter;
      value = ctrl.value;
      subMenu = ctrl.subMenu;
      icon = ctrl.icon;
      labels = ctrl.labels;
      style = ctrl.style;
      isQuickActions = (slotIndex == 1);
    }
    public void CopyFrom(ProxyMenuSlot other)
    {
      controlType = other.controlType;
      controlName = other.controlName;
      parameterName = other.parameterName;
      parameter = other.parameter;
      value = other.value;
      subMenu = other.subMenu;
      icon = other.icon;
      labels = other.labels;
      style = other.style;
      isQuickActions = other.isQuickActions;
    }
  }
}
}
