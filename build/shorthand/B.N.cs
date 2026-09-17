namespace NZK{  public static partial class B{
  /*
   * NULL tests, named N.<operator>.<condition>.
   *
   *   N    = Null.  The subject is always "is it null", never a value test.
   *   .ll  = operator OR.  Two vertical lines, as in the B.Oll group, because
   *          neither "or" nor "||" reads well inside a name.
   *   .a   = operator AND.  Pairs with a caller-supplied condition: N.a.x is
   *          "null AND x", where x is the caller's own boolean.
   *   .ny  = ANY of them, over a collection.  The arity is not known at the
   *          call site, so it cannot be a counted name like the others - the
   *          same reason B.OllAny sits beside B.Oll2..5.
   *   .e   = condition empty.
   *   .ws  = condition whitespace.
   *
   * Read left to right as a sentence: N.ll.e is "null, or, empty"; N.ll.ws is
   * "null, or, whitespace"; N.a.x is "null, and, x"; N.ny is "null, any".
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
    { if (NZK.B.mpty.t(a)) return false;
      for (int i=0;i<a.Length;i++) if (a[i] == null) return true;
      return false; }

    /*
     * Null AND cond - true only when the value is null AND the caller's
     * condition also holds.
     *
     *     N.ll.ws   null OR  whitespace      (the value decides alone)
     *     N.a.x     null AND cond            (both must be true)
     *
     * The `.a` slot is the AND operator, `.x` is the caller-supplied
     * condition.  Generic so it works for any reference type, with `where T :
     * class` so the null test is legal.
     *
     * Short-circuit order matters: the null test runs FIRST, so a caller can
     * pass a condition that would throw or dereference on a null value:
     *
     *     B.N.a.x(name, name.Length > 3)     safe - the right side is not
     *                                        evaluated when name is null
     *
     * Reversed, that same call would throw.  Keep the null test on the left.
     *
     * Reads as a sentence at the call site: "is `name` null and longer than
     * 3" - false for null, false for short, true only for both.
     */
    public static bool x<T>(T a, bool cond) where T : class
    { return a == null && cond; }
  }
}}
