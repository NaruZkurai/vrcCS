namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Material Link")]
  public class MaterialLink : UnityEngine.MonoBehaviour
  {
    [UnityEngine.Tooltip("Default Material Settings")]
    public System.Collections.Generic.List<UnityEngine.Material> defaultMaterials = new();
    [UnityEngine.Tooltip("Object Material Settings")]
    public System.Collections.Generic.List<UnityEngine.GameObject> targetObjects = new();
  }
}
}
