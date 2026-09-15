#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
public class MeshEntry {
  public UnityEngine.GameObject SourceObject;
  public UnityEngine.Mesh SourceMesh;
  public UnityEngine.Material[] Materials;
  public System.Boolean  CreateNewBone;
  public System.String BoneName;
  public float BoneWeight = Meshes.Vars.Consts.DefaultNearZeroVertexWeight;
  public System.String GroupKey;
  public System.Boolean  UseUniqueMaterial; }
}
}
}
#endif
