namespace NZK{  public static partial class S{
  /* Path helpers that return a string - see S.P.A.cs for the full naming
     scheme: S = returns a String, .P = the concept is a Path, member = the
     thing you get back. */
  public static partial class P{
    /*
     * The NEXT segment of a path - the terminal one - with its extension
     * removed.  Called Next, not Leaf, because a path is READ left to right
     * and the last segment is the one you reach next.
     *
     *     "Assets/Models/Avatar.blend"        ->  "Avatar"
     *     "Assets/Gen/X/Meshes/Body_0.asset"  ->  "Body_0"
     *
     * The return value is not necessarily a file name (the path may point at
     * a FOLDER, in which case the next segment is a directory), so neither
     * "file name" nor "short name" describes it. Next is about POSITION in
     * the path; that holds for files and folders alike.
     *
     * This is the avatar-name derivation used by every NaNimate entry point
     * when no explicit avatar name is supplied: the model file's own terminal
     * segment is the fallback identity for the generated output tree.
     */
    public static string Next(string path){return System.IO.Path.GetFileNameWithoutExtension(path);}
  }
  }
}
