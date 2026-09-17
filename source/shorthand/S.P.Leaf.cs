namespace NZK{  public static partial class S{
  /* Path helpers that return a string - see S.P.Abs.cs for the full naming
     scheme: S = returns a String, .P = the concept is a Path, member = the
     thing you get back. */
  public static partial class P{
    /*
     * The LAST segment of a path, with its extension removed.
     *
     *     "Assets/Models/Avatar.blend"        ->  "Avatar"
     *     "Assets/Gen/X/Meshes/Body_0.asset"  ->  "Body_0"
     *
     * "Leaf" and not "short name" / "file name": the return value is not
     * necessarily a file name (it may be a folder), and "short" describes
     * length rather than identity. The concept is the terminal segment,
     * the same sense of "leaf" used by the generated leaf assets.
     *
     * This is the avatar-name derivation used by every NaNimate entry point
     * when no explicit avatar name is supplied: the model file's own leaf
     * name is the fallback identity for the generated output tree.
     */
    public static string Leaf(string path){return System.IO.Path.GetFileNameWithoutExtension(path);}
  }
  }
}
