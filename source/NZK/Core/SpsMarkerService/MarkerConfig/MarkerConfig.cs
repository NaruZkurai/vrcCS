#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class SpsMarkerService {
public class MarkerConfig
    { public System.UInt32 socketId;
      public System.Int32 lightType;
      public System.Int32 compatMode;
      public System.String[] tags;
      public System.String socketName;
      public System.Boolean useRadiusOffset;
      public System.Boolean useTangentIn;
      public UnityEngine.Vector3 tangentIn;
      public System.Boolean useTangentOut;
      public UnityEngine.Vector3 tangentOut;
      public System.UInt32 nextSocketId;
      public System.UInt32 guidedTargetId; }
}
}
}
#endif
