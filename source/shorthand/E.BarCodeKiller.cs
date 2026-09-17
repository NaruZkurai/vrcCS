namespace NZK
{
  public static partial class E
  {
 public static string Psv<T>(T v) => $"Please select {v}";
 /* Called when no u is passed */
public static string BarCodeKiller<T>(System.Int64 value, T u)
{
 if (value.GetType() != typeof(int)){return "IDE ERR BCC NOT INT";}

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

    RUNTIME-SAFE: resolves and validates codes only.  It does NOT show a
    dialogue - this file compiles into the RUNTIME assembly, and the modal
    lives in E.D.cs which compiles into the EDITOR assembly, so reaching for
    E.D.OK here is a compile error, not a style choice:

      error CS0117: 'E' does not contain a definition for 'DOK'

    Run out of this file, that call resolved E.D.OK from the runtime side where
    no such member exists.  Callers that want the modal use E.D.NerrOK, which
    lives beside the dialogue and asks this helper whether to fire. */
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