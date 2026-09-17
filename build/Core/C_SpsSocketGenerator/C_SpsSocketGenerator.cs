#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/SPS Socket Generator")]
  public class C_SpsSocketGenerator : UnityEngine.MonoBehaviour
  { public System.String avatarName;
    public System.String outputFolder = "Assets/!_NZK_Generated";
    public System.Boolean  generateFXLayer = true;
    public System.Boolean  generateMenu = true;
    public System.Boolean  generateToggle = true;
    public System.String fxLayerName = "SPS";
    public UnityEngine.AnimationClip masterOnClip;
    public UnityEngine.AnimationClip masterOffClip;
    public System.String generatedFolder;
    public System.Collections.Generic.List<SpsSocketConfig> sockets = new();
    public System.Boolean  autoDetectSockets = true;
    public void Awake() { if (autoDetectSockets) RefreshSockets(); }
    public void OnEnable() { if (autoDetectSockets) RefreshSockets(); }
    public void RefreshSockets() { sockets.Clear(); GetComponentsInChildren(true,sockets); }
    public SpsSocketConfig AddSocket(System.String name)
    { var go = new UnityEngine.GameObject(name); go.transform.SetParent(transform,false);
      var config = go.AddComponent<SpsSocketConfig>(); config.socketName = name;
      if (!sockets.Contains(config)) sockets.Add(config); return config; }
    /* ── Prefab cache: baked results stored as children under Prefabs/ ── */
    public UnityEngine.Transform PrefabsRoot
    { get { var t = transform.Find("Prefabs"); if (t == null) { var g = new UnityEngine.GameObject("Prefabs"); g.tag = "EditorOnly"; g.transform.SetParent(transform,false); t = g.transform; } return t; } }
    public UnityEngine.Transform GetCachedBakeRoot(System.String socketName)
    { var safe = Vars.Names.Sanitize(socketName); var t = PrefabsRoot.Find(safe); return t != null ? t.Find("BakedSpsSocket") : null; }
    public void SaveToPrefabCache(System.String socketName,UnityEngine.GameObject bakeRoot)
    { var safe = Vars.Names.Sanitize(socketName); var existing = PrefabsRoot.Find(safe);
      if (existing != null) { for (int i = existing.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(existing.GetChild(i).gameObject); }
      else { var g = new UnityEngine.GameObject(safe); g.tag = "EditorOnly"; g.transform.SetParent(PrefabsRoot,false); existing = g.transform; }
      if (bakeRoot != null) { bakeRoot.transform.SetParent(existing,false); bakeRoot.name = "BakedSpsSocket"; }
      UnityEngine.Debug.Log("[SPS Cache] Cached " + socketName); }
    public System.String ConfigHash(SpsSocketConfig s)
    { var raw = s.socketName + "|" + s.V_addLight + "|" + s.compatMode + "|" + s.enableAuto + "|" + s.position.x + "," + s.position.y + "," + s.position.z + "|" + s.rotation.x + "," + s.rotation.y + "," + s.rotation.z + "|" + s.length + "|" + (s.parameterOverride ?? "") + "|" + s.enableHandTouchZone2 + "|" + s.boneLinkIndex;
      var b = System.Text.Encoding.UTF8.GetBytes(raw); var h = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(b);
      return System.String.Join("",System.Linq.Enumerable.Select(h,x => x.ToString("x2"))); }
    public System.String GetCachedHash(SpsSocketConfig s)
    { var safe = Vars.Names.Sanitize(s.socketName); var t = PrefabsRoot.Find(safe); return t != null ? t.GetComponentInChildren<SpsCachedHash>()?.hash ?? "" : ""; }
    public void SetCachedHash(SpsSocketConfig s)
    { var safe = Vars.Names.Sanitize(s.socketName); var t = PrefabsRoot.Find(safe); if (t == null) return; var h = t.GetComponentInChildren<SpsCachedHash>() ?? t.gameObject.AddComponent<SpsCachedHash>(); h.hash = ConfigHash(s); }
  }
}
}
#endif
