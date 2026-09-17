namespace NZK{  public static partial class B
{
    /* ll = two vertical lines, i.e. a NAME for || that is not a programming word.
       "or" reads badly and || is an operator, so a counted or gets ll<count>:
           ll2  = 2 inputs or'd
           ll3  = 3 inputs or'd        (3 cals or 3 vals)
       Name is the ARITY, so the call site states how many things are combined:
           if (NZK.B.ll3(a, b, c)) ...
       Mirrors B.I3eeI3 (which is the fixed-3 AND case) so and/or families read alike.
    */

    public static bool ll2(bool a,bool b)
    { return a || b; }

    public static bool ll3(bool a,bool b,bool c)
    { return a || b || c; }

    public static bool ll4(bool a,bool b,bool c,bool d)
    { return a || b || c || d; }

    public static bool ll5(bool a,bool b,bool c,bool d,bool e)
    { return a || b || c || d || e; }

    /* Any-of over a collection. Use when the arity is not known at the call site. */
    public static bool llAny(params bool[] a)
    { if (NZK.L.IsEmpty(a)) return false;
      for (int i=0;i<a.Length;i++) if (a[i]) return true;
      return false; }

    /* Any of the supplied values is null. Convenience over the common
       "is any of these missing" guard, which otherwise repeats B.NllE. */
    public static bool llNll<T>(params T[] a)
    { if (NZK.L.IsEmpty(a)) return false;
      for (int i=0;i<a.Length;i++) if (a[i] == null) return true;
      return false; }
}}
