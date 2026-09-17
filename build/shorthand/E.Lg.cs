namespace NZK{  public static partial class E{ public static partial class D
{
    /* Console-logging siblings of D.OK.
       D.OK shows a DIALOG; these write the SAME resolved rr-code text to the
       Unity console, which is what a non-interactive or batch path needs.
       Same nesting as OK so callers read NZK.E.D.Lg, .LgErr and .LgWarn,
       and the code lookup is shared: both go through BarCodeKiller, so a code is
       defined ONCE in E.rr.cs and never duplicated as a string.

       usage:
         NZK.E.D.Lg(47, someObject)      message  "Operation failed: x"
         NZK.E.D.LgErr(23, path)         error    "Is missing: Assets/x"
         NZK.E.D.LgWarn(34, path)        warning  "Write permission denied: x"
    */

    /* Log one rr-code at message level. value2 < 0 means single code. */
    public static void Lg<T>(System.Int64 value, T u){ Lg(value, -1, u); }

    /* Log an rr-code PAIR as title + message at message level. */
    public static void Lg<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.Log(message == null ? title : title + " " + message); }

    /* Log one rr-code as an error. */
    public static void LgErr<T>(System.Int64 value, T u){ LgErr(value, -1, u); }

    /* Log an rr-code PAIR as an error. */
    public static void LgErr<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.LogError(message == null ? title : title + " " + message); }

    /* Log one rr-code as a warning. */
    public static void LgWarn<T>(System.Int64 value, T u){ LgWarn(value, -1, u); }

    /* Log an rr-code PAIR as a warning. */
    public static void LgWarn<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.LogWarning(message == null ? title : title + " " + message); }

    /* True when the condition holds, and logs the code when it does.
       Mirrors BarCodePair but reports through the console instead of a dialog,
       so guard clauses read as: if (NZK.E.D.LgIf(bad, 23, path)) return; */
    public static bool LgIf<T>(bool condition, System.Int64 value, T u)
    { if (!condition) return false; NZK.E.D.LgErr(value, u); return true; }
}}}
