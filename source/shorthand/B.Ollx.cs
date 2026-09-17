namespace NZK{  public static partial class B{
  /*
   * OR helpers, counted by arity.
   *
   * NAME: Oll = Or, with the arity as the suffix:
   *
   *     B.Oll2(a,b)          two inputs or'd
   *     B.Oll3(a,b,c)        three inputs or'd
   *     B.Oll4 / B.Oll5      same shape, one more input each
   *
   * The count is part of the NAME, not an argument, so the call site states
   * how many things it combined and a reader can check the arity by eye.
   *
   * ===== what does NOT live here =====
   *
   * ARITY-NOT-KNOWN: B.OllAny used to sit in this file, but a `params bool[]`
   * body contradicts the one thing this group is about - the count being
   * fixed and visible in the name. It moved to B.I.A.t / B.I.A.nt, which are
   * explicitly the collection forms of the any-true question.
   *
   * NULL-TESTS: the counted-null helper was here as B.llNll. A null test asks
   * what each input IS, not how many booleans were combined, so it belongs
   * with B.N.ll.e. See B.N.cs.
   */
  public static bool Oll2(bool a,bool b)
  { return a || b; }

  public static bool Oll3(bool a,bool b,bool c)
  { return a || b || c; }

  public static bool Oll4(bool a,bool b,bool c,bool d)
  { return a || b || c || d; }

  public static bool Oll5(bool a,bool b,bool c,bool d,bool e)
  { return a || b || c || d || e; }
}}
