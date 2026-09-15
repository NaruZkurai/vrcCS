namespace NZK
{
public static partial class Core {
public class MeshEntry
  {
    public UnityEngine.GameObject SourceObject; public UnityEngine.Mesh SourceMesh; public UnityEngine.Material[] Materials;
    public System.Boolean CreateNewBone; public System.String BoneName;
    public float BoneWeight = 0.0000001f; public System.String GroupKey;
    public System.Boolean UseUniqueMaterial;
  }
}
}
