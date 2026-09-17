#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/SPS Socket")]
 public partial class C_NzkSpsSocket : UnityEngine.MonoBehaviour {
 
    
    
         // Dynamic Penetration System (old DPS lights)
    public CompatibilityMode compatMode = CompatibilityMode.All;
    public E_AddLight V_addLight = E_AddLight.Auto;
    public new System.String name;
    public System.String oscId;
    public EnableTouchZone enableHandTouchZone2 = EnableTouchZone.Auto;
    public float length;
    public System.Boolean  unitsInMeters = true;
    public System.Boolean  addMenuItem = true;
    public SpsMenuIcon menuIcon;
    public System.Boolean  enableAuto = true;
    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 rotation;
    [UnityEngine.SerializeField] public System.String _spsPrefabPath;
    [System.NonSerialized] public System.Boolean  fromSpsForAll = false;
    public System.Collections.Generic.List<DepthActionNew> depthActions2 =  new System.Collections.Generic.List<DepthActionNew>();
    public SpsState activeActions = new SpsState();
    public System.Boolean  useHipAvoidance = true;
    public System.Boolean  enablePlugLengthParameter;
    public System.String plugLengthParameterName;
    public System.Boolean  enablePlugWidthParameter;
    public System.String plugWidthParameterName;
    public System.Boolean  IsValidPlugLength => enablePlugLengthParameter && !System.String.IsNullOrWhiteSpace(plugLengthParameterName);
    public System.Boolean  IsValidPlugWidth => enablePlugWidthParameter && !System.String.IsNullOrWhiteSpace(plugWidthParameterName);
    
    
    
    
    /* ── No auto-bake on property change (user must bake manually) ── */
    /* ── Scene gizmo ── */
    void OnDrawGizmosSelected()
    { UnityEngine.Gizmos.color = UnityEngine.Color.magenta;
      UnityEngine.Gizmos.DrawSphere(transform.TransformPoint(position),0.02f);
      var dir = transform.TransformDirection(UnityEngine.Quaternion.Euler(rotation) * UnityEngine.Vector3.forward);
      UnityEditor.Handles.color = UnityEngine.Color.magenta;
      UnityEditor.Handles.DrawDottedLine(transform.TransformPoint(position),transform.TransformPoint(position) + dir * (length > 0.001f ? length : 0.1f),2f);
      var style = new UnityEngine.GUIStyle { normal = new UnityEngine.GUIStyleState { textColor = UnityEngine.Color.magenta },fontSize = 10 };
      UnityEditor.Handles.Label(transform.TransformPoint(position) + UnityEngine.Vector3.up * 0.03f,name ?? gameObject.name + " (SPS)",style);
      System.String modeLabel = "ALL"; if ((int)compatMode == 1) modeLabel = "SPS1"; else if ((int)compatMode == 2) modeLabel = "SPS2"; else if ((int)compatMode == 3) modeLabel = "TPS"; else if ((int)compatMode == 4) modeLabel = "DPS";
      UnityEditor.Handles.Label(transform.TransformPoint(position) + UnityEngine.Vector3.down * 0.03f,"[" + modeLabel + "]",style); }
  
}
}
}
#endif
