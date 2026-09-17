namespace NZK{  public static partial class B{
  /*
   * IF-ANY tests over a collection. The whole name decodes:
   *
   *     B                 = Bool, the return type.
   *     .I                = If.
   *     .A                = Any.
   *     .t / .nt          = true / NOT true.
   *
   * So NZK.B.I.A.t(a) reads "bool, if, any, true" and NZK.B.I.A.nt(a) reads
   * "bool, if, any, NOT true". The question the call site asks is spelled out
   * in the name rather than left to a positive/negative helper pair whose
   * polarity the reader has to remember.
   *
   * These take a COLLECTION, not a fixed arity: the arity is not known at the
   * call site. For the counted forms where it is known, see B.Oll2 / Oll3 /
   * Oll4 / Oll5, which are plain or'd booleans.
   *
   * EMPTY IS FALSE FOR BOTH.
   *   Vacuously, "any is true" over nothing is false, and "any is not true"
   *   over nothing is ALSO false rather than true. The alternative would make
   *   an accidental empty array behave like a failed test, so both return
   *   false and neither turns an empty input into a silent pass. Callers that
   *   need "nothing supplied" to mean something must test the array first.
   */
  public static partial class I{
    public static partial class A{
      /*
       * B.I.A.t - is ANY of them true?
       *
       *     t   = true is the thing being looked for.
       *
       * Returns false for an empty array (nothing can be true), true as soon
       * as one element is true.
       */
      public static bool t(params bool[] a)
      { if (NZK.L.IsEmpty(a)) return false;
        for (int i=0;i<a.Length;i++) if (a[i]) return true;
        return false; }

      /*
       * B.I.A.nt - is ANY of them NOT true?
       *
       *     nt  = NOT true. The test is for a FALSE element, not for the
       *           absence of a true one - over a collection those differ only
       *           when the collection is empty, and empty returns false here.
       *
       * The complement of B.I.A.t would be "NONE of them is true"; this is
       * the weaker "at least one is false". Use !B.I.A.t(...) when the
       * all-true case is what matters.
       */
      public static bool nt(params bool[] a)
      { if (NZK.L.IsEmpty(a)) return false;
        for (int i=0;i<a.Length;i++) if (!a[i]) return true;
        return false; }
    }
  }
}}
