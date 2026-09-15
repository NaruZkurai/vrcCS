namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Gesture/Hand Pose Definition")]
  public class HandPoseDefinition : UnityEngine.MonoBehaviour
  {
    public System.String poseName;               /* Display name */
    public int gestureIndex;              /* VRChat gesture index: 0=Fist,1=Open,2=Point,3=Peace,4=RnR,5=Gun,6=ThumbsUp,7=Idle */
    public UnityEngine.AnimationClip poseClip;            /* The animation clip for this pose */
    public UnityEngine.AvatarMask handMask;             /* Which hand this applies to */
    public System.String timeParameter;            /* Optional: time parameter for blending */
    public System.Collections.Generic.List<GestureCondition> conditions = new();   /* Additional conditions for this state */
    public System.Boolean isIdle;                 /* Is this the idle state */
    public float transitionDuration = 0.11f;
  }
}
}
