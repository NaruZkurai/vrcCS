#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class SpsManifestManager
  { public static System.String GetManifestPath(System.String avatarName)
    { return "Assets/!_NZK_Generated/" + avatarName + "/SPS/sps_manifest.json"; }
    public static SpsManifest Load(System.String avatarName)
    { var path = GetManifestPath(avatarName);
      var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(path);
      if (existing != null) return UnityEngine.JsonUtility.FromJson<SpsManifest>(existing.text);
      return new SpsManifest { avatarName = avatarName,buildDate = System.DateTime.Now.ToString("O") }; }
    public static void Save(SpsManifest manifest,System.String avatarName)
    { var path = GetManifestPath(avatarName); var json = UnityEngine.JsonUtility.ToJson(manifest,true);
      var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(path);
      if (existing != null) { System.IO.File.WriteAllText(path,json); UnityEditor.EditorUtility.SetDirty(existing); }
      else { System.IO.File.WriteAllText(path,json); UnityEditor.AssetDatabase.Refresh(); } }
    public static void RegisterSocket(SpsSocketBaker.SocketData data,SpsSocketConfig config,System.String markerId,System.Collections.Generic.List<System.String> assetPaths)
    { var manifest = Load(Vars.Names.Sanitize(data.gameObject.transform.root.name));
      var entry = new ManifestEntry { name = data.socketName,type = data.socketName.EndsWith("_Plug") ? "plug" : "socket",
        compatMode = data.compatMode,configHash = config != null ? config.toggleParamName ?? "" : "",markerId = markerId,
        prefabCachePath = "Generator/Prefabs/" + Vars.Names.Sanitize(data.socketName),
        assetPaths = assetPaths ?? new System.Collections.Generic.List<System.String>() };
      manifest.sockets.Add(entry);
      manifest.shaders = new System.Collections.Generic.List<System.String> { SpsMarkerService.SpsSocketShader,SpsMarkerService.SpsResolverShader };
      Save(manifest,manifest.avatarName); }
    public static void UpdateShaders(System.String avatarName)
    { var manifest = Load(avatarName);
      manifest.shaders = new System.Collections.Generic.List<System.String> { SpsMarkerService.SpsSocketShader,SpsMarkerService.SpsResolverShader };
      Save(manifest,avatarName); } }
}
}
#endif
