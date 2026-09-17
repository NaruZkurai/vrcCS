namespace NZK{  public static partial class S{
  /*
   * Path helpers that RETURN A STRING.
   *
   * NAMING, the whole scheme in one place:
   *
   *     S        = the return type. S means "this returns a System.String",
   *                which is why S.Has, S.Head and S.EqOIC all live here -
   *                they are string-returning or string-testing helpers.
   *     .P       = the CONCEPT the string describes. P = Path. It is not a
   *                type (a path here IS a string, so a top-level "P" class
   *                would have lied about the type) and not "Project" (every
   *                path in this toolkit is project-relative, so that word
   *                excludes nothing). P narrows WHAT KIND of string.
   *     .Abs     = what you GET BACK: an absolute path.
   *
   *   So S.P.Abs reads out as "String, Path, Absolute" - return type, kind,
   *   result. S.P.Next is the same shape: a string, a path, the NEXT segment.
   *
   * This is why the nesting exists rather than a flat S.Abs: at the call site
   * S.P.Abs announces both the type and the subject, so a reader never has to
   * open the file to learn what came back.
   */
  public static partial class P{
    /*
     * Absolute path on disk for a project-relative asset path.
     *
     *     "Assets/Foo/Bar.blend"  ->  "/nzk/unity/.../Assets/Foo/Bar.blend"
     *
     * The AssetDatabase speaks project-relative paths while System.IO and
     * every File/Directory Exists check needs a real one, so this conversion
     * sits between the two worlds.
     *
     * The parent of Application.dataPath is the project root, i.e. the folder
     * that CONTAINS "Assets". Separators are normalised to the host separator
     * so a forward-slash asset path still resolves on Windows.
     *
     * Extracted from NZKNaNimateMeshGenerator.AbsoluteFromProject and
     * NZKNaNimateMeshFolder.AbsoluteFromProject: more than one consumer needs
     * this exact conversion, and a second copy is a second chance to get the
     * separator handling wrong.
     */
    public static string Abs(string projectRelative){
      string root=System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName;
      return System.IO.Path.Combine(root,(projectRelative??string.Empty).Replace('/',System.IO.Path.DirectorySeparatorChar));
    }
  }
  }
}
