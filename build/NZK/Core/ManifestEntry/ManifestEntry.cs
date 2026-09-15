#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class ManifestEntry
  { public System.String name;
    public System.String type; /* "socket" or "plug" */
    public System.Int32 compatMode;
    public System.String configHash;
    public System.String markerId;
    public System.String prefabCachePath;
    public System.Collections.Generic.List<System.String> assetPaths = new System.Collections.Generic.List<System.String>(); }
}
}
#endif
