namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/SPS Config")]
  public class SpsConfig : UnityEngine.MonoBehaviour
  {
    public SpsGenVersion spsVersion = SpsGenVersion.V1_Only;
    public System.String generationVersion = "1.0.0";
    public System.Collections.Generic.List<SpsBoneEntry> boneEntries = new();
    public System.Collections.Generic.List<SpsContactReceiver> contactReceivers = new();
    public System.Collections.Generic.List<SpsMaterialConfig> materialConfigs = new();
    public System.String avatarName = "";
    public System.String outputFolder = ""; /* Auto-resolved: Assets/!_NZK_Generated/{avatarName}/SPS */
    public System.String layerName = "SPS";
    public System.Boolean generateToggle = true;
    public System.String toggleParamName = "(b-gt)SPS";
    public System.Boolean generateMenu = true;
    public System.Boolean generateFXLayer = true;
    public UnityEngine.AnimationClip onClip;
    public UnityEngine.AnimationClip offClip;
  }
}
}
