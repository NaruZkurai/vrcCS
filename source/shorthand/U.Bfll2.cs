namespace NZK{  public static partial class U{
  /*
   * Byte/size helpers.
   *
   * NAME: U = Misc (see M.ec.cs). Members here are size-related but have no
   * single more specific initial, so they sit in Misc alongside the rest.
   *
   *   U.Bfll2.VC = Bytes From vertex count, ll2 = the two inputs multiplied
   *                (vertex count x bytes per vertex). VC names the INPUT, so
   *                the call site says which mesh the byte count came from.
   *   U.B2.PFX   = unit prefix for a byte count: "512 B", "3.4 KB", "1.2 MB".
   *   U.B2.Mb    = Mesh Bytes, formatted. Convenience for PFX(VC(mesh)).
   *
   * The trailing-letter style is deliberate: the members differ only in
   * WHAT they take (a Mesh vs a byte count) and are meant to be read as a set.
   */

  /* Per-vertex byte budget: position(12) + normal(12) + tangent(16) + uv0(8).
     Shared so the generator and the freezer cannot drift apart on what a
     "48 bytes per vertex" estimate means. */
  public const long BytesPerVertex48=48;

public static partial class Bfll2{
  
  /* U.Bfll2.VC - Bytes from vertex count
   - estimated mesh size in BYTES, from the mesh's vertex COUNT. */
  public static long VC(UnityEngine.Mesh mesh){return (long)mesh.vertexCount*BytesPerVertex48;}
  }  
  public static partial class B2
  { /* U.B2.PFX - a raw byte COUNT,
    AS UNIT PREFIX
    formatted for humans. 
    One decimal on KB/MB. */
    public static string PFX(long bytes)
    { if(bytes<1024) return bytes+" B";
      if(bytes<1024*1024) return (bytes/1024.0).ToString("0.0")+" KB";
      return (bytes/(1024.0*1024.0)).ToString("0.0")+" MB"; }

    /* Estimated mesh size, already formatted. */
    public static string Mb(UnityEngine.Mesh mesh){return B2.PFX(Bfll2.VC(mesh));}
  }
}}
