namespace NZK{  public static partial class E{
  /*
   * DIALOGUE output - modal boxes the user must dismiss.  EDITOR-ONLY BY GUARD.
   *
   * WHY THIS FILE IS NOT IN AN editor/ FOLDER ANY MORE.
   *
   * It used to live in shorthand/editor/ under its own NZK.Shorthand.Editor
   * asmdef, which re-declared `public static partial class E` in a SECOND
   * assembly.  Partial classes merge only within one assembly, so that second
   * `E` shadowed the runtime one and the calls below resolved against the empty
   * editor shell instead of the real type:
   *
   *   error CS0436: The type 'E' in 'E.D.cs' conflicts with the imported type
   *   'E' in 'NZK.Shorthand.Runtime' ... Using the type defined in 'E.D.cs'.
   *   error CS0117: 'E' does not contain a definition for 'BarCodeKiller'
   *   error CS0117: 'E' does not contain a definition for 'BarCodePair'
   *
   * BarCodeKiller and BarCodePair are global: they just parse rr-codes, they
   * are not editor functionality.  The only genuinely editor-only line in this
   * whole file is Show's EditorUtility.DisplayDialog.  So the split was in the
   * wrong place: instead of cutting the FILE out into an editor assembly, cut
   * the single editor CALL out with #if UNITY_EDITOR.
   *
   * Result: `E` is one partial class in one assembly, every member is visible
   * to every other, and nothing has to be aliased or duplicated.  In a player
   * build Show/OK/NerrOK drop out and the rest of E is untouched.
   *
   *   E.D.Show(title, message, ok)   the raw dialog        (#if UNITY_EDITOR)
   *   E.D.OK(a, b)                   plain text pair
   *   E.D.OK<T>(u, a, b)             rr-code pair, resolved via BarCodeKiller
   *   E.D.NerrOK<T>(c, a, b, u)      the "if this is null, tell the user" guard
   *
   * Codes come from E.rr.cs through BarCodeKiller, shared with E.C, so a code
   * is defined once and both the dialog and the console paths print the same
   * text.
   *
   * D IS A NESTED CLASS, and it mirrors E.C on purpose.  E.C is the console
   * family (d/w/e) and E.D is the dialogue family (Show/OK/NerrOK): the letter
   * names the OUTPUT, and the members are read INSIDE that family.  A flat
   * E.DOK / E.DShow spelling was tried and reverted - it read as if "DOK" were
   * one word, and it broke the parallel with E.C.d, so the two output families
   * no longer looked like the same idea.
   */
  public static partial class D
  {
#if UNITY_EDITOR
    /*
     * E.D.Show - the raw dialogue.
     *
     * Carried over from the standalone E.Dd leaf.  Was NZK.E.Dd(title, message,
     * ok); kept as a separate member rather than inlined because older callers
     * reach it by that shape, and it reads as the primitive the rest build on.
     * Callers that want the pairwise API should use OK/NerrOK below.
     *
     * The ONLY editor-dependent line in the file.  Guarded rather than moved so
     * its callers do not have to care which assembly they are in.
     */
    public static void Show(string title, string message, string ok)
    { UnityEditor.EditorUtility.DisplayDialog(title, message, ok); }

    /* Plain text pair - no code lookup. */
    public static void OK(string a, string b){ Show(a, b, "ok"); }

    /* Numeric code pair: resolves via BarCodeKiller, same as NerrOK.
       The object u supplies the value interpolated into rrNN<T>(T u) codes. */
    public static void OK<T>(T u, System.Int64 a, System.Int64 b)
    { string title; string message;
      BarCodeKiller<T>(a, b, u, out title, out message);
      Show(title, message, "ok"); }

    /* Guard: dialogue ONLY when c is null, and returns whether it fired.
       Reads as `if (NZK.E.D.NerrOK(x, 45, 10, x)) return;`

       Asks BarCodePair whether the condition holds AND both codes exist, then
       shows the dialogue itself.  Same-assembly now, so the call is a plain
       member lookup - no qualification, no assembly hop. */
    public static bool NerrOK<T>(T c, int a, int b, T u) where T : class
    { if (!BarCodePair<T>(c == null, a, b, u)) return false;
      OK(u, a, b);
      return true; }
#endif
  }
}}
