namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Gesture/Hand State Machine Config")]
  public class HandStateMachineConfig : UnityEngine.MonoBehaviour
  {
    public System.Boolean isLeftHand;               /* True = left hand,False = right hand */
    public System.Collections.Generic.List<HandPoseDefinition> poses = new();    /* All poses for this hand */
    public System.String defaultPoseName;            /* Default state name */
    public System.String blendParameter = "GestureLeft";     /* Blend parameter (GestureLeft/GestureRight) */
    public UnityEngine.Transform handTransform;           /* Reference to the hand bone UnityEngine.Transform */
    public UnityEngine.AvatarMask handAvatarMask;           /* Mask for this hand */
    public float layerWeight = 1f;
  }
}
}
