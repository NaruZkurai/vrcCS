#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class NZKNaNimateMeshFolder{
    /*
     * Layout + IO for the frozen mesh output folder.
     *
     * Output convention mirrors the project's other generated tooling:
     * Assets/!_NZK_Generated/<Avatar>/Meshes/
     *
     * These files are DISPOSABLE BUILD OUTPUT in the same sense as vrcCS's
     * build/ tree: regenerated from source, never hand-edited, but written in
     * place so GUIDs survive and existing prefab references never break.
     */
    /*
     * Root of the generated output tree, PROJECT-RELATIVE including "Assets".
     *
     * The leading "Assets/" is mandatory. Without it the path resolves
     * against the project root (the parent of Assets/), so the output landed
     * in <project>/!_NZK_Generated - a directory Unity does not manage,
     * has no .meta for, and will never register. The symptom was
     * "folder exists on disk but Unity will not register it:
     * !_NZK_Generated/Nemasis".
     */
    public const string GeneratedRootName="Assets/!_NZK_Generated";
    /* Bare folder name, for display only. */
    public const string GeneratedRootLeafName="!_NZK_Generated";
    public const string MeshesFolderName="Meshes";
    public const string PrefabsFolderName="Prefabs";
    /* Bare file name of an asset path, without its extension. */
    static string LeafNameOf(string path){return NZK.S.P.Next(path);}
    /* Generated avatar root for an already-sanitized avatar name. */
    static string AvatarRoot(string avatarName){return GeneratedRootName+"/"+NZK.SS.An(avatarName);}
    /* Project-relative folder for a model's frozen meshes. */
    public static string FolderFor(string modelAssetPath,string avatarName){return AvatarRoot(NZK.B.NoE(avatarName)?LeafNameOf(modelAssetPath):avatarName)+"/"+MeshesFolderName;}
    /*
     * Project-relative folder for a model's generated prefabs, a sibling of
     * the Meshes folder under the same avatar root.
     */
    public static string PrefabFolderFor(string modelAssetPath,string avatarName){return AvatarRoot(NZK.B.NoE(avatarName)?LeafNameOf(modelAssetPath):avatarName)+"/"+PrefabsFolderName;}
    /* Project-relative path for one frozen mesh leaf. */
    public static string LeafAssetPath(string folder,string rendererName,int subMeshIndex){return NZK.SS.AssetPath(folder,NZK.SS.An(rendererName),subMeshIndex);}
    /* Project-relative path for the frozen prefab. */
    public static string PrefabPath(string folder,string avatarName){return NZK.S.P.C(folder,NZK.SS.An(NZK.B.NoE(avatarName)?"Avatar":avatarName)+"Frozen.prefab");}
    /*
     * Ensure a project-relative folder exists.
     *
     * NEVER calls AssetDatabase.CreateFolder. CreateFolder is a MAIN-THREAD
     * API, and it REFUSES during an out-of-process import with:
     *
     * "CreateFolder is not supported while importing out-of-process"
     *
     * OnPostprocessModel runs on an import worker, so anything reached from
     * there - directly or through a deferred flush that lands inside the
     * import window - must create folders on the FILESYSTEM only. Writing
     * the .meta sidecar is the seam that makes the folder a real asset
     * without touching the AssetDatabase.
     */
    public static void EnsureFolder(string projectRelativeFolder){
        if(NZK.B.NoE(projectRelativeFolder)) return;
        string absolute=AbsoluteFromProject(projectRelativeFolder);
        System.IO.Directory.CreateDirectory(absolute);
        /*
         * Write the .meta for THIS folder and every parent below Assets/ so
         * Unity adopts the whole chain. A folder with no .meta is invisible
         * to the AssetDatabase, and CreateAsset then fails with
         * "Creating asset at path ... failed".
         */
        string[] parts=projectRelativeFolder.Split('/');
        string current=parts[0];
        System.IO.Directory.CreateDirectory(AbsoluteFromProject(current));
        for(int i=1;i<parts.Length;i++){
            current=current+"/"+parts[i];
            WriteFolderMetaIfMissing(AbsoluteFromProject(current),current);
        }
        WriteFolderMetaIfMissing(absolute,projectRelativeFolder);
    }
    /* Absolute filesystem path for a project-relative asset path. */
    public static string AbsoluteFromProject(string projectRelative){return NZK.S.P.r2a(projectRelative);}
    /*
     * Write a folder's .meta when it is absent.
     *
     * The GUID is derived from the project-relative path so the same folder
     * always keeps the same identity across regenerations, instead of every
     * run orphaning the previous one.
     */
    static void WriteFolderMetaIfMissing(string absoluteFolder,string projectRelative){
        if(!System.IO.Directory.Exists(absoluteFolder)) return;
        string metaPath=absoluteFolder+".meta";
        if(System.IO.File.Exists(metaPath)) return;
        System.IO.File.WriteAllText(
            metaPath,
            "fileFormatVersion: 2\n"+
            "guid: "+DeterministicGuid(projectRelative)+"\n"+
            "folderAsset: yes\n"+
            "DefaultImporter:\n"+
            "  externalObjects: {}\n"+
            "  userData: \n"+
            "  assetBundleName: \n"+
            "  assetBundleVariant: \n");
    }
    /* Stable 32-hex-char GUID for a project-relative path. */
    public static string DeterministicGuid(string projectRelativePath){
        byte[] bytes=Md5Of(projectRelativePath??string.Empty);
        var sb=new System.Text.StringBuilder(32);
        for(int i=0;i<bytes.Length;i++) sb.Append(bytes[i].ToString("x2"));
        return sb.ToString();
    }
    /* MD5 of a UTF-8 string; the reusable half of DeterministicGuid. */
    static byte[] Md5Of(string s){using(var md5=System.Security.Cryptography.MD5.Create()) return md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));}
    /*
     * Make a name safe to use as a Unity asset FILE name.
     *
     * Delegates to NZK.SS.An. The allowlist rule and the injective '$hex'
     * encoding used to be implemented here; it now lives in exactly one place
     * so the two copies cannot drift. See S.S.cs for why '_' substitution is
     * LOSSY and why '$' is reserved as the escape marker.
     */
    public static string Sanitize(string name){return NZK.SS.An(name);}
  }
}
}
#endif
