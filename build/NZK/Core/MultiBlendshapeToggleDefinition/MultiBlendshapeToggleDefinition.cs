namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Multi Blendshape Toggle Definition")]
  public class MultiBlendshapeToggleDefinition : UnityEngine.MonoBehaviour
  {
    public System.Collections.Generic.List<BlendshapeToggleDefinition> childToggles = new();
    public System.String parameterName;
    public MultiMode mode;
    public float[] blendValues;
    public UnityEngine.AnimationCurve blendCurve;
  }
}
}
