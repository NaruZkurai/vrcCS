namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Proxy Sync")]
  public class ProxySync : UnityEngine.MonoBehaviour
  {   /* ------------------------------------------------------------------ */
    /* Inspector                              */
    /* ------------------------------------------------------------------ */
    public UnityEngine.GameObject targetObject;
    public SwapComponentType componentType = SwapComponentType.SkinnedMeshRenderer;
    /* ------------------------------------------------------------------ */
    /* Stored original (A) -- written by UnityEditor.Editor/ObjectSync.cs       */
    /* ------------------------------------------------------------------ */
    [UnityEngine.HideInInspector, UnityEngine.SerializeField] UnityEngine.Mesh _origMesh;
    [UnityEngine.HideInInspector, UnityEngine.SerializeField] UnityEngine.Material[] _origMaterials;
    [UnityEngine.HideInInspector, UnityEngine.SerializeField] System.Boolean _hasOriginalData;
    public UnityEngine.Mesh OriginalMesh => _origMesh;
    public UnityEngine.Material[] OriginalMats => _origMaterials;
    public System.Boolean HasOriginal => _hasOriginalData;
    /* Called by ObjectSync.cs -- no editor API here */
    public void SetOriginalData(UnityEngine.Mesh mesh, UnityEngine.Material[] mats)
    {
      _origMesh = mesh;
      _origMaterials = mats != null ? (UnityEngine.Material[])mats.Clone() : null;
      _hasOriginalData = true;
    }
  }
}
}
