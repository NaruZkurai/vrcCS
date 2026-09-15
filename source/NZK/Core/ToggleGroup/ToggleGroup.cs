namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Toggle Group")]
  public class ToggleGroup : UnityEngine.MonoBehaviour
  {
    public System.String groupName;
    public System.Collections.Generic.List<UnityEngine.GameObject> toggleReferences = new();
    public System.Boolean exclusive;
    public int buildOrder;
  }
}
}
