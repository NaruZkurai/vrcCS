#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MA {
public sealed class Group
    {
      /** The name of the group, which becomes the bone's name. */
      public System.String name;
      /** Vertices this group must be able to hide. */
      public System.Collections.Generic.HashSet<System.Int32> vertices =
        new System.Collections.Generic.HashSet<System.Int32>();
      /** Bones allocated to this group, in creation order. */
      public System.Collections.Generic.List<AddedBone> bones =
        new System.Collections.Generic.List<AddedBone>();
      /** The EXISTING armature bone under NaNimations that the clip drives.
       *
       *  The new bones hang UNDER this one, so the clip's NaN scale propagates
       *  down to them.  They must NOT be reparented onto the mesh's influence
       *  bone: that bone is an ordinary rig bone (Hips, Spine, a toe), and
       *  renaming or re-owning it corrupts the rig. */
      public UnityEngine.Transform nanimationBone;
      /** The ONE influence bone this group produced (section 6).  Multiple
       *  allocations are folded onto it, so a group maps to a single bone at
       *  weight 1 rather than a chain. */
      public UnityEngine.Transform influenceBone;
    }
}
}
}
#endif
