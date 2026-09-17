namespace NZK{  public static partial class E{
  /*
   * DIALOGUE output - modal boxes the user must dismiss.  EDITOR-ONLY.
   *
   * This lives in editor/ because every member ends at
   * UnityEditor.EditorUtility.DisplayDialog.  UnityEditor does not exist in a
   * player build, so if this file were runtime-visible the VRC upload would
   * die with:
   *
   *   error CS0234: The type or namespace name 'MenuItemAttribute' does not
   *   exist in the namespace 'UnityEditor'
   *
   * RULE: if it is reachable from a scene and fails to compile there, it is
   * editor.  Dialogue is the clearest case - a modal box in a shipped avatar
   * is meaningless.
   *
   *   E.D.Show(title, message, ok)   the raw dialog
   *   E.D.OK(a, b)                   plain text pair
   *   E.D.OK<T>(u, a, b)             rr-code pair, resolved via BarCodeKiller
   *   E.D.NerrOK<T>(c, a, b, u)      the "if this is null, tell the user" guard
   *
   * Codes come from E.rr.cs through BarCodeKiller, shared with E.C, so a code
   * is defined once and both the dialog and the console paths print the same
   * text.
   *
   * D IS PARTIAL, and that is load-bearing.  It used to be declared here as
   * `public static class D` while another leaf declared
   * `public static partial class D`, which C# rejects:
   *
   *   error CS0260: Missing partial modifier on declaration of type 'D';
   *   another partial declaration of this type exists
   *
   * Merging the old E.Dd leaf in below is what makes this the single
   * declaration, so the modifier must stay.
   */
  public static partial class D
  {
    /*
     * E.D.Show - the raw dialogue.
     *
     * Carried over from the standalone E.Dd leaf.  Was NZK.E.Dd(title, message,
     * ok); kept as a separate member rather than inlined because older callers
     * reach it by that shape, and it reads as the primitive the rest build on.
     * Callers that want the pairwise API should use OK/NerrOK above.
     */
    public static void Show(string title, string message, string ok)
    { UnityEditor.EditorUtility.DisplayDialog(title, message, ok); }

    /* Plain text pair - no code lookup. */
    public static void OK(string a, string b){ Show(a, b, "ok"); }

    /* Numeric code pair: resolves via BarCodeKiller, same as NerrOK.
       The object u supplies the value interpolated into rrNN<T>(T u) codes. */
    public static void OK<T>(T u, System.Int64 a, System.Int64 b)
    { string title; string message;
      NZK.E.BarCodeKiller(a, b, u, out title, out message);
      Show(title, message, "ok"); }

    /* Guard: dialogue ONLY when c is null, and returns whether it fired.
       Reads as `if (NZK.E.D.NerrOK(x, 45, 10, x)) return;`

       Delegates to BarCodePair rather than resolving the codes here.  That
       helper ALSO validates that both codes exist and returns false without
       showing anything when they do not - a local re-implementation silently
       dropped that check, which would have shown a dialog titled
       "unknown:45" instead of staying quiet. */
    public static bool NerrOK<T>(T c, int a, int b, T u) where T : class
    { return NZK.E.BarCodePair(c == null, a, b, u); }
  }
}}
