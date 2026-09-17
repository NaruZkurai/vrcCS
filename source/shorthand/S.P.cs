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
   *   So S.P.r2a reads out as "String, Path, relative to absolute" - return
   *   type, kind, conversion. S.P.Next is the same shape: a string, a path,
   *   the NEXT segment, and S.P.C the same again for Path.Combine.
   *
   * ONE FILE, because every member here is a string-returning path helper.
   * The leaves were split one-member-per-file and the names repeated the
   * member (S.P.Abs.cs, S.P.Next.cs, S.pc.cs), which made the group read as
   * three unrelated files and let one of them drift to a name - S.pc - that
   * did not match the class inside it. The group IS the identity here, so the
   * file carries the group name and the members are read inside it.
   *
   * This is why the nesting exists rather than a flat S.Abs: at the call site
   * S.P.r2a announces both the type and the subject, so a reader never has to
   * open the file to learn what came back.
   */
  public static partial class P{
    /*
     * RELATIVE TO ABSOLUTE path on disk, for a project-relative asset path.
     *
     *     "Assets/Foo/Bar.blend"  ->  "/nzk/unity/.../Assets/Foo/Bar.blend"
     *
     * Called r2a - relative to absolute - because that names the conversion
     * rather than its result. "Abs" said what came back but not what it came
     * FROM, so a reader still had to check the parameter to learn which of
     * the two directions this went.
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
    public static string r2a(string projectRelative){
      string root=System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName;
      return System.IO.Path.Combine(root,(projectRelative??string.Empty).Replace('/',System.IO.Path.DirectorySeparatorChar));
    }

    /*
     * The NEXT segment of a path - the terminal one - with its extension
     * removed. Called Next, not Leaf, because a path is READ left to right
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

    /*
     * Path.Combine as a shorthand.
     *
     * Was NZK.E.PC in a file called S.pc.cs: class E, member PC, in a file
     * named after S. Three names for one idea, and the file name disagreed
     * with the class inside it. It is a string-returning path helper, so it
     * sits in S.P with the rest of them.
     *
     *     NZK.E.PC(folder, name)  ->  NZK.S.P.C(folder, name)
     */
    public static string C(string a,string b){return System.IO.Path.Combine(a,b);}
  }
  }
}
