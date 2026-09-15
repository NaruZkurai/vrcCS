namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class VertexGroupSplitEntry
  {
    public System.String groupName;      // name of the vertex group (bone name)
    public System.String outputMeshName;   // name for the output mesh/object
    public System.Boolean extract = true;     // true = include in target,false = exclude
  }
}
}
