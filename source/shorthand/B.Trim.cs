namespace NZK{  public static partial class B{
  /* Whitespace-only test for a string, null-safe.
     Needed because System.String.IsNullOrWhiteSpace is a STATIC call with the same
     shape as B.NoE, so call sites that must not accept "   " otherwise have to
     write the framework call inline at every site. Kept beside NoE so the two
     emptiness questions are answered by one type. */
  public static bool Trim(string a){return string.IsNullOrWhiteSpace(a);}
  }
  }
