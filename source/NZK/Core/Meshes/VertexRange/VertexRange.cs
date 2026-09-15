#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
public struct VertexRange {
  public int StartVertex;
  public int VertexCount;
  public int StartSubmesh;
  public int SubmeshCount;
  public int BoneIndex;
  public float BoneWeight; }
}
}
}
#endif
