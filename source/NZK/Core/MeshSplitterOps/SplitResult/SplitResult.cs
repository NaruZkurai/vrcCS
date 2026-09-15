#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshSplitterOps {
public struct SplitResult
    { public System.Boolean  success; public System.String errorMessage;
      public UnityEngine.Mesh targetMesh; public UnityEngine.Mesh remainingMesh;
      public UnityEngine.Material[] materials;
      public System.Collections.Generic.List<UnityEngine.Transform> targetBones; public System.Collections.Generic.List<UnityEngine.Transform> remainingBones; }
}
}
}
#endif
