#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class Generate
  { public static void DBTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.ForEach(names,NaNimate.Update.DBTs);
  public static void BTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Generate.DBTs(names);
  public static void Menus(System.Collections.Generic.IEnumerable<System.String> names) => Systems.ForEach(names,NaNimate.Update.Sync);
  public static void Params(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Generate.Menus(names);
  public static void All(System.Collections.Generic.IEnumerable<System.String> names) => Systems.ForEach(names,n => { NaNimate.Update.DBTs(n); NaNimate.Update.Sync(n); }); }
}
}
}
#endif
