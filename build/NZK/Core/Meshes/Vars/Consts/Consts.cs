#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
public static partial class Vars {
public static class Consts
  { public const float DefaultNearZeroVertexWeight = 0.0000001f;
    public const System.String MeshOutputFolder = "Assets/NZK_Generated/Meshes";
    /* The prefix the NANIMATION BONES carry in a built avatar: "NaNimate ".
       The nanimation toggle works by animating a bone's scale to NaN, so a mesh
       only disappears if it is WEIGHTED to the exact bone the clip targets.
       The clip paths are built as "Armature/NaNimations/NaNimate " + the source
       object's name, so the bone must be named to match character for character.
       An audit of the built scene found 28 bones named "NaNimate X" and ZERO
       matching any "NaNim_" form - so a merge path that named bones "NaNim..."
       created bones no clip ever referenced, and the toggles did nothing. */
    public const System.String BonePrefix_Nanimate = "NaNimate ";
    public const System.String BonePrefix_Generic = "G_"; }
}
}
}
}
#endif
