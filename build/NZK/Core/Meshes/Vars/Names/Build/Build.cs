#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
public static partial class Vars {
public static partial class Names {
public static class Build
  { public static System.String MergedMeshName(System.String avatarName,System.String meshGroupName) { return "Merged_" + NZK.Core.Vars.Names.Sanitize(avatarName) + "_" + NZK.Core.Vars.Names.Sanitize(meshGroupName); }
    public static System.String BoneName(System.String baseName,float weight) { return (UnityEngine.Mathf.Approximately(weight,Meshes.Vars.Consts.DefaultNearZeroVertexWeight) ? Meshes.Vars.Consts.BonePrefix_Nanimate : Meshes.Vars.Consts.BonePrefix_Generic) + Meshes.Vars.Names.Build.SanitizeBoneName(baseName); }
    public static System.String SanitizeBoneName(System.String name)
    { if (System.String.IsNullOrEmpty(name)) { return "Bone"; }
    System.String sanitized = name.Trim().Replace(' ','_').Replace('/','_').Replace('\\','_');
    foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
    { sanitized = sanitized.Replace(invalid,'_'); }
    return sanitized; }
    public static System.String MergedMeshAssetPath(System.String avatarName,System.String meshGroupName) { return Meshes.Vars.Consts.MeshOutputFolder + "/" + NZK.Core.Vars.Names.Sanitize(avatarName) + "/" + Meshes.Vars.Names.Build.MergedMeshName(avatarName,meshGroupName) + ".asset"; }
  }
}
}
}
}
}
#endif
