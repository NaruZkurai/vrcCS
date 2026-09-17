namespace NZK{  public static partial class B{
  /*
   * NULL tests, named N.<operator>.<condition>.
   *
   *   N    = Null.  The subject is always "is it null", never a value test.
   *   .ll  = operator OR.  Two vertical lines, as in the B.Oll group, because
   *          neither "or" nor "||" reads well inside a name.
   *   .a   = operator AND.
   *   .ny  = ANY of them, over a collection.  The arity is not known at the
   *          call site, so it cannot be a counted name like the others - the
   *          same reason B.OllAny sits beside B.Oll2..5.
   *   .e   = condition empty.
   *   .ws  = condition whitespace.
   *
   * Read left to right as a sentence: N.ll.e is "null, or, empty"; N.ll.ws is
   * "null, or, whitespace"; N.ny is "null, any".
   *
   * ONE FILE.  These were four leaves (NllE.cs, Nll.cs, NoE.cs, Trim.cs) plus
   * members that had drifted into the or-arity group, all answering one
   * question between them.  They differ only in operator and condition, so the
   * operator belongs in the NAME rather than in a filename.
   */
  public static partial class N
  {
    public static partial class ll
    {
      /* Null OR empty - the common "is this string missing" guard.
         "   " is NOT empty; use N.ll.ws when whitespace must also fail. */
      public static bool e(string a){return System.String.IsNullOrEmpty(a);}

      /* Null OR whitespace.
         Useful because System.String.IsNullOrWhiteSpace is a static framework
         call of the same shape as the emptiness test, so a call site that must
         reject "   " would otherwise write it inline every time.
         Replaces B.Trim, whose name described the input ("trim it") rather
         than the question. */
      public static bool ws(string a){return System.String.IsNullOrWhiteSpace(a);}
    }

    /*
     * Null ANY of them - true when at least one element is null.
     *
     * Generic over T so it covers strings, Transforms, renderers and anything
     * else with a null state, instead of a string-only overload per type.
     *
     * EMPTY IS FALSE.  "any of nothing is null" is vacuously false, and the
     * alternative would make an accidentally empty array read like a failed
     * guard, turning a missing input into a silent pass.  Callers that need
     * "nothing supplied" to mean something must test the count first.
     *
     * Replaces the old B.NllAny, which said "null" twice and carried no
     * operator slot.  `ny` matches B.I.A.t / .nt, where the collection forms
     * also get a distinct name rather than a count.
     */
    public static bool ny<T>(params T[] a)
    { if (NZK.L.IsEmpty(a)) return false;
      for (int i=0;i<a.Length;i++) if (a[i] == null) return true;
      return false; }
  }
}}
