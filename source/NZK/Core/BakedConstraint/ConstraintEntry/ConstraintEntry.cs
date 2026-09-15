#if false
namespace NZK
{
public static partial class Core {
public partial class BakedConstraint {
[System.Serializable]
  public class ConstraintEntry
  { public System.String sourcePath;    // System.IO.Path to source object
    public System.String targetPath;    // System.IO.Path to target object
    public System.String constraintType;  // "Position","Rotation","Scale","Parent"
    public float weight;       // Constraint weight
    public System.Boolean  active;        // Is the constraint active
  }
}
}
}
#endif
