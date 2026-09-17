#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public struct ReceiverRequest
  { public UnityEngine.GameObject obj; public System.String paramName; public System.String objName; public float radius;
    public UnityEngine.Vector3 pos; public UnityEngine.Vector3 rotation; public float height; public System.String[] tags;
    public System.Boolean  localOnly; public System.Boolean  useHipAvoidance;
    public SpsHapticUtils.ReceiverParty party;
    public VRC.Dynamics.ContactReceiver.ReceiverType type;
    public ReceiverRequest Clone() => (ReceiverRequest)MemberwiseClone(); }
}
}
#endif
