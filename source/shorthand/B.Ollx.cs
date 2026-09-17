namespace NZK{  public static partial class B{
  /*
   * OR helpers, counted by arity.
   *
   * NAME: Oll = Or, with the arity as the suffix:
   *
   *     B.Oll2(a,b)          two inputs or'd
   *     B.Oll3(a,b,c)        three inputs or'd
   *     B.Oll4 / B.Oll5      same shape, one more input each
   *     B.OllAny(params[])   arity not known at the call site
   *
   * The count is part of the NAME, not an argument, so the call site states
   * how many things it combined and a reader can check the arity by eye.
   * Mirrors B.I3eeI3 (the fixed-3 AND case) so the and/or families read alike.
   *
   *   if (NZK.B.Oll3(a,b,c)) ...
   *
   * ===== null-tests do NOT live here =====
   *
   * The "is any of these null" helper was called B.llNll and sat in this file.
   * That was wrong: it answers a NULL question, and B already has exactly one
   * file for null/empty/equality predicates - B.NllE ("null or empty"). The
   * or-arity group is about how many BOOLEANS are combined; a null test is
   * about what each input IS.
   *
   * See B.Nll.cs for the null family: B.Nll2 / B.Nll3 / B.Nll4 / B.Nll5 /
   * B.NllAny, alongside NllE.
   */
  public static bool Oll2(bool a,bool b)
  { return a || b; }

  public static bool Oll3(bool a,bool b,bool c)
  { return a || b || c; }

  public static bool Oll4(bool a,bool b,bool c,bool d)
  { return a || b || c || d; }

  public static bool Oll5(bool a,bool b,bool c,bool d,bool e)
  { return a || b || c || d || e; }

  /* Any-of over a collection. Use when the arity is not known at the call site. */
  public static bool OllAny(params bool[] a)
  { if (NZK.L.IsEmpty(a)) return false;
    for (int i=0;i<a.Length;i++) if (a[i]) return true;
    return false; }
}}
