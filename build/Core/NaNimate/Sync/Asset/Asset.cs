#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static partial class Sync {
public static class Asset
  { public static void Menu(System.String menuPath,System.String a,System.Collections.Generic.List<System.String> tp)
    { var menu = NaNimate.Menu.GllC(menuPath); NaNimate.Sync.Menu2(menu,a,tp); UnityEditor.EditorUtility.SetDirty(menu); } }
}
}
}
}
#endif
