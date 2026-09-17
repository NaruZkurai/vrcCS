namespace NZK{  public static partial class S{
  /*
   * Path helpers that RETURN A STRING. See S.P.A.cs for the naming scheme:
   * S = returns a String, .P = the concept is a Path, member = the result.
   *
   * FILE NAME: these leaves are S.P.<letter>.cs - the member's FIRST letter
   * only - so the name states the SHAPE (String, Path, then which member)
   * instead of repeating the member name the file already contains.
   */
  public static partial class P{
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
