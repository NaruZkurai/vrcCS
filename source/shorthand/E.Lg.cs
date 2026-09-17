namespace NZK{  public static partial class E
{
    /* Logging siblings of D.OK.
       D.OK shows a DIALOG; these write the SAME resolved rr-code text to the
       Unity console instead, which is what a non-interactive or batch path needs.
       Kept beside D so the code lookup is shared: both go through BarCodeKiller,
       so a code is defined ONCE in E.rr.cs and never duplicated as a string.

       usage:
         NZK.E.D.Lg(47, someObject)        -> Log("Operation failed: x")
         NZK.E.D.LgErr(23, path)           -> LogError("Is missing: Assets/x")
         NZK.E.D.LgWarn(34, path)          -> LogWarning("Write permission denied: x")
    */

    /// <summary>Log one rr-code at message level. value2 &lt; 0 means single code.</summary>
    public static void Lg<T>(System.Int64 value, T u){ Lg(value, -1, u); }

    /// <summary>Log an rr-code PAIR as title + message at message level.</summary>
    public static void Lg<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.Log(message == null ? title : title + " " + message); }

    /// <summary>Log one rr-code as an error.</summary>
    public static void LgErr<T>(System.Int64 value, T u){ LgErr(value, -1, u); }

    /// <summary>Log an rr-code PAIR as an error.</summary>
    public static void LgErr<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.LogError(message == null ? title : title + " " + message); }

    /// <summary>Log one rr-code as a warning.</summary>
    public static void LgWarn<T>(System.Int64 value, T u){ LgWarn(value, -1, u); }

    /// <summary>Log an rr-code PAIR as a warning.</summary>
    public static void LgWarn<T>(System.Int64 value, System.Int64 value2, T u)
    { string title; string message;
      NZK.E.BarCodeKiller(value, value2, u, out title, out message);
      UnityEngine.Debug.LogWarning(message == null ? title : title + " " + message); }

    /// <summary>
    /// True when the condition holds, and logs the code when it does.
    /// Mirrors BarCodePair but reports through the console instead of a dialog,
    /// so guard clauses read as: if (E.D.LgIf(bad, 23, path)) return;
    /// </summary>
    public static bool LgIf<T>(bool condition, System.Int64 value, T u)
    { if (!condition) return false; NZK.E.D.LgErr(value, u); return true; }
}}
