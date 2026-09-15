namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Toggle Builder")]
  public class ToggleBuilder : UnityEngine.MonoBehaviour
  {
    public System.String builderName;
    public System.Collections.Generic.List<UnityEngine.GameObject> toggleEntries = new();
    public System.Collections.Generic.List<ToggleLink> links = new();
    public System.Boolean autoGenerateOnBuild = true;
    public System.String avatarName;
  }
}
}
