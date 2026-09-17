namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class ToggleLink
  {
    public UnityEngine.GameObject sourceToggle;
    public UnityEngine.GameObject targetToggle;
    public ToggleLinkType linkType;
    public System.String conditionParameter;
    public float conditionThreshold;
    public int priority;
  }
}
}
