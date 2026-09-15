namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/SPS Socket Config")]
  public class SpsSocketConfig : UnityEngine.MonoBehaviour
  {/** <summary>Display name (e.g.,"Blowjob","Pussy").</summary> */
    public System.String socketName;
    /** <summary>Full menu path (e.g.,"Special/Thighjob","Handjob/Handjob Left").</summary> */
    public System.String menuPath;
    /** <summary>The actual socket object UnityEngine.Transform (may be this UnityEngine.Transform or a child).</summary> */
    public UnityEngine.Transform socketTransform;
    /** <summary>UnityEngine.Light mode from VRCFury DPS Plug (0=none,1=on penetration,2=while active,3=always).</summary> */
    public int V_addLight;
    /** <summary>Auto-detect enable.</summary> */
    public System.Boolean enableAuto;
    /** <summary>Local position offset.</summary> */
    public UnityEngine.Vector3 position;
    /** <summary>Local rotation offset.</summary> */
    public UnityEngine.Vector3 rotation;
    /** <summary>Plug length.</summary> */
    public float length;
    /** <summary>Secondary hand touch zone.</summary> */
    public System.Boolean enableHandTouchZone2;
    /** <summary>Optional custom parameter name (null/empty = auto).</summary> */
    public System.String parameterOverride;
    /** <summary>Bone index this socket links to (from ArmatureLink,-1 = no link).</summary> */
    public int boneLinkIndex = -1;
    /** <summary>System.IO.Path on UnityEngine.Avatar to the linked bone (resolved during bake).</summary> */
    public System.String boneLinkPath;
    /** <summary>Generated on-animation clip (assigned during generation).</summary> */
    public UnityEngine.AnimationClip onClip;
    /** <summary>Generated off-animation clip (assigned during generation).</summary> */
    public UnityEngine.AnimationClip offClip;
    /** <summary>Parameter name for this socket's toggle (e.g.,"(b-gt)BlowjobSPS").</summary> */
    public System.String toggleParamName;
    /** <summary>Compatibility mode: 0=All,1=SPS1,2=SPS2,3=TPS,4=DPS.</summary> */
    public int compatMode;
  }
}
}
