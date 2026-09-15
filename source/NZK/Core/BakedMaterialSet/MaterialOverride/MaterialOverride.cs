#if false
namespace NZK
{
public static partial class Core {
public partial class BakedMaterialSet {
[System.Serializable]
  public class MaterialOverride
  { public System.String rendererPath;     // System.IO.Path to the renderer on the avatar
    public int materialSlot;      // Which material slot to swap
     public UnityEngine.Material material;     // The material to apply
    public System.String originalMaterial;   // Name of the original material (for reversion)
  }
}
}
}
#endif
