namespace NZK{  public static partial class B{
  /*
   * NULL-family predicates - "is it missing".
   *
   * NAME: Nll = Null. One file, because these all answer the same question
   * and only differ in HOW MANY things are tested:
   *
   *     B.NllE(a)            a single string is null or empty
   *     B.Nll2(a,b)          any of two values is null
   *     B.Nll3 / B.Nll4 / B.Nll5
   *     B.NllAny(params[])   arity not known at the call site
   *
   * WHY THESE SIT WITH NllE AND NOT WITH THE OR GROUP:
   *   The counted-null helper used to live in the or-arity file as B.llNll,
   *   which put a null test under an OR name and left B.NllE with no family
   *   around it. A null test is a PREDICATE ABOUT ITS INPUTS, not a statement
   *   about how many booleans were combined, so it belongs next to NllE.
   *
   * The generic count forms take T[] rather than T,T,... because a null test
   * is meaningful for any reference type and the params form keeps one body
   * instead of five near-identical ones. Call them as B.Nll3(a,b,c).
   *
   * Null only: an EMPTY string is not null, so use NllE when "" must count.
   */
  public static bool Nll<T>(T a)
  { return a == null; }

  public static bool NllAny<T>(params T[] a)
  { if (NZK.L.IsEmpty(a)) return false;
    for (int i=0;i<a.Length;i++) if (a[i] == null) return true;
    return false; }
}}
