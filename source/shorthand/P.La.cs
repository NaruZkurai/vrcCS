namespace NZK{  public static partial class P{
  /* Project-relative path of one generated mesh leaf: folder + sanitized
     renderer name + "_" + submesh index + ".asset".

     Extracted from NZKNaNimateMeshFolder.LeafAssetPath so the naming rule
     lives in ONE place. Callers otherwise repeat
     Sanitize(x)+"_"+i+".asset" and any drift silently breaks the match
     between a written asset and the renderer it belongs to. */
  public static string La(string folder,string rendererName,int subMeshIndex){
    return NZK.E.PC(folder,NZK.NaNimate.NZKNaNimateMeshFolder.Sanitize(rendererName)+"_"+subMeshIndex+".asset");
  }
  }
}
