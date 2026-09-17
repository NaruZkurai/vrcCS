namespace NZK{  public static partial class P{
  /* Project-relative path of one generated mesh leaf: folder + already
     sanitized renderer name + "_" + submesh index + ".asset".

     The caller passes the SANITIZED name, so this helper stays free of any
     dependency on the NaNimate folder type. Shorthand must not reference
     NZK.NaNimate: this file is in namespace NZK, where "NZK.NaNimate"
     resolves as NZK.NZK.NaNimate and fails with CS0234. */
  public static string La(string folder,string sanitizedName,int subMeshIndex){
    return NZK.E.PC(folder,sanitizedName+"_"+subMeshIndex+".asset");
  }
  }
}
