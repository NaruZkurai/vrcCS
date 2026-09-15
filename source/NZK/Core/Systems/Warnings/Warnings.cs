#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class Warnings
  { public static void NoObjects(System.String menuName,System.Boolean silent = false) { var code = Vars.Errors.E_NoObjects; if (!silent) Vars.Errors.Warn(code,"No GameObjects in selection for " + menuName + ". " + Vars.Errors.Fix(code)); }
  public static System.Boolean HasObject(UnityEngine.GameObject[] objs) => objs != null && objs.Length > 0; }
}
}
}
#endif
