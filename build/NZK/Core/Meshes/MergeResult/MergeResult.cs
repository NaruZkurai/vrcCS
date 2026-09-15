#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
public struct MergeResult {
  public System.Boolean  Success;
  public System.String ErrorMessage;
  public UnityEngine.Mesh MergedMesh;
   public UnityEngine.Material[] Materials;
  public System.Collections.Generic.List<UnityEngine.Transform> GeneratedBones; }
}
}
}
#endif
