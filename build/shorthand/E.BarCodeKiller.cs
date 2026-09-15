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
    value2 < 0 means the condition is described by a single code. */
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
 if (!hasA || !hasB) {return false;}
 string title; string message;
 BarCodeKiller(value, value2, u, out title, out message);
 NZK.E.D.OK(title, message);
 return true;
}}}