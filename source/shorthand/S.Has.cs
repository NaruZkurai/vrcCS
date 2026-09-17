namespace NZK{  public static partial class S
{
    /* Case-insensitive / ordinal string predicates and builders.
       Every comparison in the toolkit should route through one of these so the
       System.StringComparison argument is written ONCE instead of at each site -
       a wrong comparison mode is otherwise invisible and easy to introduce.
    */

    /* Does a contain b, ignoring case? Empty/absent operands are false, never a throw. */
    public static bool HasOIC(string a,string b)
    { return !NZK.B.NoE(a) && !NZK.B.NoE(b) && a.IndexOf(b,System.StringComparison.OrdinalIgnoreCase) >= 0; }

    /* Does a start with b, ignoring case? */
    public static bool StartsOIC(string a,string b)
    { return !NZK.B.NoE(a) && !NZK.B.NoE(b) && a.StartsWith(b,System.StringComparison.OrdinalIgnoreCase); }

    /* Does a equal b, ignoring case? Null-safe. */
    public static bool EqOIC(string a,string b)
    { return string.Equals(a,b,System.StringComparison.OrdinalIgnoreCase); }

    /* Does a end with ANY of the supplied suffixes (ordinal, case-insensitive)? */
    public static bool EndsAnyOIC(string a,params string[] suffixes)
    { if (NZK.B.NoE(a) || suffixes == null) return false;
      for (int i=0;i<suffixes.Length;i++) if (NZK.S.EndsWithOIC(a,suffixes[i])) return true;
      return false; }

    /* Is the value present, i.e. a non-empty string? Reads as the POSITIVE of B.NoE. */
    public static bool Has(string a){ return !NZK.B.NoE(a); }

    /* first token of a path/name split on a separator char, or the whole value. */
    public static string Head(string a,char sep)
    { if (NZK.B.NoE(a)) return a; int i=a.IndexOf(sep); return i < 0 ? a : a.Substring(0,i); }

    /* everything after the first separator, or "" when absent. */
    public static string Tail(string a,char sep)
    { if (NZK.B.NoE(a)) return string.Empty; int i=a.IndexOf(sep); return i < 0 ? string.Empty : a.Substring(i+1); }
}}
