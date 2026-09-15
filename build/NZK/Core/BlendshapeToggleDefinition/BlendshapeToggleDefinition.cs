namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Blendshape Toggle Definition")]
  public class BlendshapeToggleDefinition : UnityEngine.MonoBehaviour
  {
    public UnityEngine.SkinnedMeshRenderer targetRenderer;
    public System.String blendshapeName;
    public float onValue = 100f;
    public float offValue = 0f;
    public float[] onValues;
    public float[] offValues;
    public System.String parameterName;
    public System.Boolean isMulti;
  }
}
}
