namespace NZK{  public static partial class M{
  /* Per-vertex byte budget: position(12) + normal(12) + tangent(16) + uv0(8).
     Shared so the generator and the freezer cannot drift apart on what a
     "48 bytes per vertex" estimate means. */
  public const long BytesPerVertex48=48;

  /* Estimated mesh size in bytes from its vertex count. */
  public static long Vb(UnityEngine.Mesh mesh){return (long)mesh.vertexCount*BytesPerVertex48;}

  /* Human-readable byte size, one decimal on KB/MB. */
  public static string By(long bytes){
    if(bytes<1024) return bytes+" B";
    if(bytes<1024*1024) return (bytes/1024.0).ToString("0.0")+" KB";
    return (bytes/(1024.0*1024.0)).ToString("0.0")+" MB";
  }

  /* Estimated mesh size, already formatted. */
  public static string Mb(UnityEngine.Mesh mesh){return By(Vb(mesh));}
  }
}
