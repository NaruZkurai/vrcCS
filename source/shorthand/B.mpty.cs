namespace NZK{  public static partial class B{
  /*
   * EMPTINESS tests, named mpty.t / mpty.nt.
   *
   *     B        = Bool, the return type.
   *     .mpty    = the CONDITION: empty.  "empty" is spelled without its first
   *                letter because "empty" COLLIDES with the framework concept
   *                the reader is matching against - `IsEmpty` - and a name
   *                that looks like the thing it wraps says nothing.  The
   *                dropped vowel is the standard telegraphic form, the same
   *                trade as S.mk.unq.
   *     .t / .nt = true / NOT true, matching B.I.A.t / B.I.A.nt.
   *
   * So B.mpty.t(a) reads "bool, empty, true" and B.mpty.nt(a) reads "bool,
   * empty, not true".  The polarity is in the name rather than carried by a
   * reader's memory of which helper was the negated one.
   *
   * REPLACES L.IsEmpty / L.NotEmpty.  Those were spelled out IN FULL on the
   * argument that a predicate should read as a sentence - but `L.IsEmpty(a)`
   * names the CONDITION twice (the type L, the member IsEmpty) and says nothing
   * about polarity, while `B.mpty.t(a)` names type, condition and polarity in
   * three short slots.  The class moved from L to B as well: L was
   * "List/collection helpers", which described the PARAMETER, not the RESULT -
   * every one of these returns bool.
   *
   * NULL AND EMPTY ARE THE SAME ANSWER.  The Unity APIs that produce these
   * collections (mesh.boneWeights, mesh.bindposes, renderer.bones) return
   * either depending on whether the channel was ever authored, and every call
   * site needs one "nothing to work with" answer for both.  A caller that must
   * tell "null" from "empty" cannot use this pair.
   */
  public static partial class mpty
  {
    /* Array has no usable elements. */
    public static bool t<T>(T[] a){return a==null||a.Length==0;}

    /* List has no usable elements. */
    public static bool t<T>(System.Collections.Generic.List<T> a){return a==null||a.Count==0;}

    /* Array has usable elements. */
    public static bool nt<T>(T[] a){return !t(a);}

    /* List has usable elements. */
    public static bool nt<T>(System.Collections.Generic.List<T> a){return !t(a);}
  }
}}
