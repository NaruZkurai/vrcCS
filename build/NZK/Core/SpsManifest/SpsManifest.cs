#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class SpsManifest
  { public System.String avatarName;
    public System.String buildDate;
    public System.Collections.Generic.List<ManifestEntry> sockets = new System.Collections.Generic.List<ManifestEntry>();
    public System.Collections.Generic.List<System.String> shaders = new System.Collections.Generic.List<System.String>();
    public System.Collections.Generic.List<System.String> generatedPaths = new System.Collections.Generic.List<System.String>(); }
}
}
#endif
