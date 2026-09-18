namespace NZK
{
  public static partial class E
  {
 public static string Psv<T>(T v) => $"Please select {v}";
 /* Called when no u is passed */
public static string BarCodeKiller<T>(System.Int64 value, T u)
{
 /* The parameter is Int64 and every call site passes one - E.C.d/w/e all take
    System.Int64 - so the guard must ACCEPT Int64.  It used to require
    value.GetType() == typeof(int) against that Int64 parameter, which no call
    could ever satisfy: a long is boxed as System.Int64 and never as
    System.Int32, so EVERY E.C and E.D call logged
    "IDE ERR BCC NOT INT" instead of resolving its rr-code.  The diagnostic
    that reports the failure was itself the failure.

    The check is still worth keeping, just pointed at the right question:
    reject an out-of-range INDEX rather than a type.  Resolution builds the
    name "rr" + value and looks it up by reflection, so a negative index
    produces the nonsense name "rr-1" and an absurd one produces a
    twenty-digit lookup that can only fail.  Both are caller mistakes worth
    naming, and both are cheap to catch here. */
 if (value < 0) { return "IDE ERR BCC NEGATIVE: " + value; }
 if (value > MaxCode) { return "IDE ERR BCC OUT OF RANGE: " + value; }

 string name = "rr" + value;
 var flags = System.Reflection.BindingFlags.Public |
 System.Reflection.BindingFlags.Static;

 var field = typeof(NZK.E).GetField(name, flags);
 if (field != null)
  return field.GetValue(null) as string;

 var method = typeof(NZK.E).GetMethod(name, flags);
 if (method != null && method.IsGenericMethodDefinition)
  return method.MakeGenericMethod(typeof(T)).Invoke(null, new object[] { u }) as string;

 return "unknown:" + value;
}

 /** <summary>Highest rr-code number this table defines.
  *
  *  A sanity bound, not a contract: it exists so a fat-fingered literal in a
  *  call site is reported as out-of-range rather than as the much less
  *  specific "unknown:N", which reads like a missing definition instead of a
  *  typo.  Raise it when E.rr.cs grows past it.</summary> */
 public const System.Int64 MaxCode = 200;

 /* compound: resolve a pair of codes as title + message, mirroring NerrOK.
    negative value in the second slot means "no second code" (single dialogue).
    value2 == null -> plain two-code pair. */
public static void BarCodeKiller<T>(System.Int64 value, System.Int64 value2, T u, out string title, out string message)
{
 title   = BarCodeKiller(value, u);
 message = value2 < 0 ? null : BarCodeKiller(value2, u);
}

 /* compound boolean: true when the pair resolves to a real condition.
    value2 < 0 means the condition is described by a single code.

    RUNTIME-SAFE and assembly-independent: this only resolves and validates
    codes.  The members of D that show a dialogue are #if UNITY_EDITOR guarded
    in the same assembly, so a player build simply loses them - there is no
    cross-assembly reach left to break.  (It used to be a hard error:
    "error CS0117: 'E' does not contain a definition for 'BarCodeKiller'",
    caused by an editor assembly re-declaring a shadow copy of E.) */
public static bool BarCodePair<T>(bool condition, System.Int64 value, System.Int64 value2, T u)
{
 if (!condition) {return false;}
 var flags = System.Reflection.BindingFlags.Public |
 System.Reflection.BindingFlags.Static;
 bool hasA = typeof(NZK.E).GetField("rr" + value, flags) != null ||
             typeof(NZK.E).GetMethod("rr" + value, flags) != null;
 bool hasB = value2 < 0 ? true :
             typeof(NZK.E).GetField("rr" + value2, flags) != null ||
             typeof(NZK.E).GetMethod("rr" + value2, flags) != null;
 return hasA && hasB;
}}}