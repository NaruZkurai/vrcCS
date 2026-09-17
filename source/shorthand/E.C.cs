namespace NZK{  public static partial class E{
  /*
   * CONSOLE output.  RUNTIME-SAFE: uses UnityEngine.Debug only, so it compiles
   * in a player build unchanged - no guard needed, nothing here reaches for
   * UnityEditor.
   *
   *   E.C.d   Debug.Log        - information
   *   E.C.w   Debug.LogWarning - something looks wrong but is survivable
   *   E.C.e   Debug.LogError   - something failed
   *
   * "C" is Console; the member is the LEVEL.  The spelling is deliberately one
   * letter each, and d/w/e never repeat within the group - earlier drafts used
   * Lg/LgErr/LgWarn, which grew a "Lg" prefix on every call and collided with
   * the DIALOGUE family in E.D.
   *
   * Codes are resolved by BarCodeKiller, so an rr-code is defined ONCE in
   * E.rr.cs and never repeated as a literal string.
   *
   * VERBOSITY.  Two forms per level:
   *
   *   E.C.d(47, someObject)                 always logs
   *   E.C.d(enabled, 47, someObject)        logs only when `enabled`
   *
   * The bool-first overload is the "global debug" gate: a caller passes its own
   * switch and the call site stays silent without an `if` around it.  Kept
   * separate from the unconditional form rather than defaulted, so a call that
   * can be silenced is visible at the call site.
   */
  public static partial class C
  {
    /* ---- E.C.d : information ---- */
    public static void d<T>(System.Int64 value, T u){ d(value, -1, u); }
    public static void d<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.Log(message == null ? title : title + " " + message); }

    /* ---- E.C.w : warning ---- */
    public static void w<T>(System.Int64 value, T u){ w(value, -1, u); }
    public static void w<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.LogWarning(message == null ? title : title + " " + message); }

    /* ---- E.C.e : error ---- */
    public static void e<T>(System.Int64 value, T u){ e(value, -1, u); }
    public static void e<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.LogError(message == null ? title : title + " " + message); }

    /* ---- verbosity gates: log only when the caller's switch says so ----
       Reading `if (NZK.E.C.e(verbose, 23, path)) return;` keeps the guard on
       one line instead of an `if` plus a call plus the same condition repeated. */
    public static bool d<T>(bool enabled, System.Int64 value, T u)
    { if (!enabled) return false; d(value, u); return true; }
    public static bool w<T>(bool enabled, System.Int64 value, T u)
    { if (!enabled) return false; w(value, u); return true; }
    public static bool e<T>(bool enabled, System.Int64 value, T u)
    { if (!enabled) return false; e(value, u); return true; }

    /* Condition-guarded error, mirroring the old E.D.LgIf:
       if (NZK.E.C.eIf(bad, 23, path)) return; */
    public static bool eIf<T>(bool condition, System.Int64 value, T u)
    { if (!condition) return false; e(value, u); return true; }
  }
}}
