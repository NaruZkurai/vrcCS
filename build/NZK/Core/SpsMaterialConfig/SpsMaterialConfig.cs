namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class SpsMaterialConfig
  {
    public UnityEngine.Material sourceMaterial;
    public System.String materialName;
    public System.String shaderName = "Standard";
    public UnityEngine.Color baseColor = UnityEngine.Color.white;
    public UnityEngine.Texture2D mainTexture;
  }
}
}
