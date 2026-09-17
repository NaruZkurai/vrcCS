#if false
namespace NZK
{
public static partial class Core {
public partial class BakedPhysBone {
[System.Serializable]
  public class PhysBoneEntry
  { public System.String rootTransform;   // System.IO.Path to the root bone
    public float radius;       // Collision radius
    public float pull;         // Pull force
    public float spring;       // Spring force
    public float stiffness;      // Stiffness
    public System.String[] collisionTags;   // Collision tags
  }
}
}
}
#endif
