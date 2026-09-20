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
    }
}
}
}
#endif
