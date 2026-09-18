#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
public static partial class Vars {
public static partial class Names {
public static class Build
  { public static System.String MergedMeshName(System.String avatarName,System.String meshGroupName) { return "Merged_" + NZK.Core.Vars.Names.Sanitize(avatarName) + "_" + NZK.Core.Vars.Names.Sanitize(meshGroupName); }
    /** <summary>Name a generated bone for a source object.
     *
     *  The nanimation form keeps the source name EXACTLY as-is (spaces and all),
     *  because that is what the animation clip path contains.  SanitizeBoneName
     *  would turn "Outer Necklace" into "Outer_Necklace" and produce a bone the
     *  clip never targets, which silently disables the toggle rather than
     *  failing visibly.  The generic form keeps sanitizing, since nothing
     *  references those by name.</summary> */
    public static System.String BoneName(System.String baseName,float weight) { return (UnityEngine.Mathf.Approximately(weight,Meshes.Vars.Consts.DefaultNearZeroVertexWeight) ? Meshes.Vars.Consts.BonePrefix_Nanimate + (System.String.IsNullOrEmpty(baseName) ? "Bone" : baseName) : Meshes.Vars.Consts.BonePrefix_Generic + Meshes.Vars.Names.Build.SanitizeBoneName(baseName)); }
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
