#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.InitializeOnLoad]
  static class ProxySyncHooks
  {   static ProxySyncHooks()
    {   UnityEditor.SceneManagement.EditorSceneManager.sceneOpened       += OnSceneOpened;
      UnityEditor.AssemblyReloadEvents.afterAssemblyReload += TriggerAll;
      UnityEditor.EditorApplication.hierarchyChanged     += OnHierarchyChanged;   }
    static void OnSceneOpened(
      UnityEngine.SceneManagement.Scene _,UnityEditor.SceneManagement.OpenSceneMode __)
      => TriggerAll();
    static void OnHierarchyChanged()
    {   var all = UnityEngine.Object.FindObjectsOfType<ProxySync>(true);
      foreach (var s in all)
        ProxySyncOps.EnforceChildrenDisabled(s);   }
    internal static void TriggerAll()
    {   var all = UnityEngine.Object.FindObjectsOfType<ProxySync>(true);
      foreach (var s in all)
      {   ProxySyncOps.EnforceChildrenDisabled(s);
        ProxySyncOps.PerformSwap(s);   }   }
  }
}
}
#endif
