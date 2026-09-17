namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Gesture/Transition Set")]
  public class TransitionSet : UnityEngine.MonoBehaviour
  {
    public UnityEngine.GameObject target;
    public System.Collections.Generic.List<ConditionEntry> conditions = new();
    public System.Collections.Generic.List<ConditionEntry> entryConditions = new(); // original entry conditions (no inversion needed)
    /* Transition timing */
    public float duration = 0.11f;
    public System.Boolean hasExitTime;
    public System.Boolean hasFixedDuration = true;
    public float exitTime;
    public System.Boolean canTransitionToSelf;
    public System.Boolean isExit;
  }
}
}
