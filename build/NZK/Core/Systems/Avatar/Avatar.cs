#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class Avatar
  { public static System.Collections.Generic.HashSet<System.String> From(UnityEngine.GameObject[] objs) => new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(objs, Vars.Names.Get.Avatar),System.StringComparer.Ordinal);
  public static System.Collections.Generic.HashSet<System.String> Selected() => From(UnityEditor.Selection.gameObjects);
  public static System.Collections.Generic.HashSet<System.String> Scene()
  { var set = new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
    for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) { var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i); if (!s.isLoaded) continue; foreach (var r in s.GetRootGameObjects()) { if (r != null) set.Add(Vars.Names.Sanitize(r.name)); } }
    return set; }
  public static System.Collections.Generic.HashSet<System.String> All()
  { System.String root = NaNimate.Paths.Abs(Vars.Consts.RootFolder); var set = new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
    if (System.IO.Directory.Exists(root)) foreach (System.String d in System.IO.Directory.GetDirectories(root)) { System.String n = System.IO.Path.GetFileName(d); if (!System.String.IsNullOrEmpty(n)) set.Add(n); }
    return set; }
  public static System.Collections.Generic.HashSet<System.String> Active()
  { var sel = Selected(); if (sel.Count > 0) return sel;
    var scene = Scene(); if (scene.Count > 0) return scene;
    var all = All(); if (all.Count == 0) UnityEngine.Debug.Log("[NZK] No avatars found under " + Vars.Consts.RootFolder + ".");
    return all; } }
}
}
}
#endif
