#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static partial class Re {
public static class Generate
  { public static void DBTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Re.Generate.DBTsFromBTs(names);
  public static void BTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Re.Generate.DBTsFromBTs(names);
  public static void Menus(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Re.Generate.ParamsFromBTs(names);
  public static void Params(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Re.Generate.ParamsFromBTs(names);
  public static void All(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Re.Generate.AllFromAnims(names);
  public static void DBTsAndParamsFromBTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Re.Generate.AllFromAnims(names);
  public static void DBTsFromBTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.ForEach(names,NaNimate.Update.DBTs);
  public static void DBTsFromBTs(System.String a) => NaNimate.Update.DBTs(a);
  public static void ParamsFromBTs(System.Collections.Generic.IEnumerable<System.String> names) => Systems.ForEach(names,Overloads.ParamsFromBTs);
  public static void AllFromAnims(System.Collections.Generic.IEnumerable<System.String> names) => Systems.Generate.All(names); }
}
}
}
}
#endif
