#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.InitializeOnLoad]
public static class RemoteConsole
{ const System.String CmdFile = "Assets/NZK toolkit v4/remote_cmd.txt";
  static System.String _lastContent;
  static double _lastCheck;
  static RemoteConsole()
  { var dir = System.IO.Path.GetDirectoryName(CmdFile);
    if (!System.String.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);
    if (!System.IO.File.Exists(CmdFile))
    System.IO.File.WriteAllText(CmdFile,"# NZK Remote Console\n# Commands: ping,compile,log <msg>,scene <path>,select <name>,list scenes\n");
  _lastContent = System.IO.File.ReadAllText(CmdFile);
  UnityEditor.EditorApplication.update += OnUpdate;
  UnityEngine.Debug.Log("[RemoteConsole] Listening: " + CmdFile); }
  static void OnUpdate()
  { if (UnityEditor.EditorApplication.timeSinceStartup - _lastCheck < 0.5) return;
  _lastCheck = UnityEditor.EditorApplication.timeSinceStartup;
  if (!System.IO.File.Exists(CmdFile)) return;
  var current = System.IO.File.ReadAllText(CmdFile);
  if (current == _lastContent) return;
  var oldLines = _lastContent.Split('\n');
  var newLines = current.Split('\n');
  _lastContent = current;
  foreach (var line in newLines)
  { var t = line.Trim();
    if (t.Length == 0 || t.StartsWith("#")) continue;
    var isNew = true;
    foreach (var o in oldLines) if (o.Trim() == t) { isNew = false; break; }
    if (!isNew) continue;
    Execute(t); }
  }
  static void Execute(System.String cmd)
  { if (cmd == "ping")
    UnityEngine.Debug.Log("[RemoteConsole] pong — Unity alive. PID=" + System.Diagnostics.Process.GetCurrentProcess().Id);
  else if (cmd.StartsWith("log "))
    UnityEngine.Debug.Log("[RemoteConsole] " + cmd.Substring(4));
  else if (cmd.StartsWith("compile") || cmd.StartsWith("recompile"))
  { UnityEngine.Debug.Log("[RemoteConsole] Requesting recompilation...");
    UnityEditor.EditorApplication.delayCall += () => UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(); }
  else if (cmd.StartsWith("scene "))
  { var p = cmd.Substring(6).Trim();
    if (p.EndsWith(".unity"))
    { UnityEditor.SceneManagement.EditorSceneManager.OpenScene(p);
    UnityEngine.Debug.Log("[RemoteConsole] Scene: " + p); }
    else UnityEngine.Debug.LogWarning("[RemoteConsole] Scene path must end with .unity: " + p); }
  else if (cmd.StartsWith("select "))
  { var n = cmd.Substring(7).Trim();
    var go = UnityEngine.GameObject.Find(n);
    if (go != null) { UnityEditor.Selection.activeGameObject = go; UnityEngine.Debug.Log("[RemoteConsole] Selected: " + n); }
    else UnityEngine.Debug.LogWarning("[RemoteConsole] Not found: " + n); }
  else if (cmd == "list scenes")
  { foreach (var s in UnityEditor.EditorBuildSettings.scenes)
    if (s != null && !System.String.IsNullOrEmpty(s.path))
      UnityEngine.Debug.Log("[RemoteConsole] Scene: " + s.path); }
  else UnityEngine.Debug.Log("[RemoteConsole] Unknown: " + cmd); }
}
}
}
#endif
