#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class C_NzkSpsSocket {
[System.Serializable] public class DepthActionNew { public SpsState actionSet = new SpsState(); public UnityEngine.Vector2 range = new UnityEngine.Vector2(-0.25f,0); public DepthActionUnits units = DepthActionUnits.Meters; public System.Boolean  enableSelf; public float smoothingSeconds; public System.Boolean  reverseClip; }
}
}
}
#endif
