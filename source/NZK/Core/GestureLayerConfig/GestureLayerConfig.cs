namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Gesture Layer Config")]
  public class GestureLayerConfig : UnityEngine.MonoBehaviour
  { /* Hand references */
    public UnityEngine.Transform leftHandTransform;         /* Left hand bone reference */
    public UnityEngine.Transform rightHandTransform;        /* Right hand bone reference */
    /* Hand state machine configs */
    public HandStateMachineConfig leftHand;       /* Left hand state machine config */
    public HandStateMachineConfig rightHand;      /* Right hand state machine config */
    public HandStateMachineConfig leftHandIdle;     /* Left hand idle state machine */
    public HandStateMachineConfig rightHandIdle;    /* Right hand idle state machine */
    /* Reference controller (Faery 2.0 FX) */
    public UnityEngine.RuntimeAnimatorController referenceController;  /* Reference controller to mirror */
    public System.Boolean mirrorFromReference;          /* Whether to mirror reference controller structure */
    /* VRChat gesture parameters to generate */
    public System.Boolean generateGestureParams = true;       /* Auto-create VRC gesture parameters */
    public System.String gestureLeftParam = "GestureLeft";    /* Left gesture integer param */
    public System.String gestureRightParam = "GestureRight";  /* Right gesture integer param */
    public System.String gestureLeftWeightParam = "GestureLeftWeight";  /* Left weight param */
    public System.String gestureRightWeightParam = "GestureRightWeight"; /* Right weight param */
    /* Generated controller settings */
    public System.String controllerName = "Gesture";      /* Controller asset name */
    public System.String avatarRootName = "";           /* UnityEngine.Avatar name for folder scoping */
  }
}
}
