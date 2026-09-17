namespace NZK{  public static partial class M{
  /*
   * Byte/size helpers.
   *
   * NAME: M = Misc (see M.ec.cs). Members here are size-related but have no
   * single more specific initial, so they sit in Misc alongside the rest.
   *
   *   M.Vb = Vertex Bytes. V for vertex, b for bytes. "b" alone was taken by
   *          By below, so the vertex-count estimate is Vb.
   *   M.By = Bytes, formatted. Read "by" as in "bytes" - it takes a byte count
   *          and returns a string.
   *   M.Mb = Mesh Bytes, formatted. Convenience for By(Vb(mesh)).
   *
   * The trailing-letter style is deliberate: Vb, By and Mb differ only in
   * WHAT they take (a Mesh vs a byte count) and are meant to be read as a set.
   */

  /* Per-vertex byte budget: position(12) + normal(12) + tangent(16) + uv0(8).
     Shared so the generator and the freezer cannot drift apart on what a
     "48 bytes per vertex" estimate means. */
  public const long BytesPerVertex48=48;

  /* M.Vb - estimated mesh size in BYTES, from the mesh's vertex COUNT. */
  public static long Vb(UnityEngine.Mesh mesh){return (long)mesh.vertexCount*BytesPerVertex48;}

  /* M.By - a raw byte COUNT, formatted for humans. One decimal on KB/MB. */
  public static string By(long bytes){
    if(bytes<1024) return bytes+" B";
    if(bytes<1024*1024) return (bytes/1024.0).ToString("0.0")+" KB";
    return (bytes/(1024.0*1024.0)).ToString("0.0")+" MB";
  }

  /* Estimated mesh size, already formatted. */
  public static string Mb(UnityEngine.Mesh mesh){return By(Vb(mesh));}
  }
}
