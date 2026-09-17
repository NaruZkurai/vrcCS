namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/UnityEngine.Mesh Splitter")]
  public class MeshSplitterDefinition : UnityEngine.MonoBehaviour
  {
    public UnityEngine.SkinnedMeshRenderer sourceSMR;             // source SMR to split
    public System.Collections.Generic.List<VertexGroupSplitEntry> splitGroups = new();   // groups to extract
    public System.String targetMeshName = "";              // name for target mesh object
    public System.String remainingMeshName = "";             // name for remaining mesh object
    public System.Boolean createTargetMesh = true;            // generate the target mesh
    public System.Boolean createRemainingMesh = true;           // generate the remaining mesh
    public System.String outputFolder = "Assets/NZK_Generated/Meshes"; // where to save
    public System.Boolean preserveBlendShapes = true;           // copy blend shapes to outputs
    public System.Boolean preserveMaterials = true;             // copy materials to outputs
  }
}
}
