#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class SpsMarkerService {
 /* ── Shader property constants (match VRCFury SPS2 spec) ── */
    public const System.String SpsSocketShader     = "Hidden/VRCFury/SpsSocketMarker";
    public const System.String SpsResolverShader   = "Hidden/VRCFury/SpsResolver";
    public const System.String SpsDataGrabShader   = "Hidden/VRCFury/SpsDataGrabPass";
    public const System.String PropConfigured      = "_SPS_Configured";
    public const System.String PropIdLow           = "_SPS_IdLow";
    public const System.String PropIdHigh          = "_SPS_IdHigh";
    public const System.String PropPlayerIdLow     = "_SPS_PlayerIdLow";
    public const System.String PropPlayerIdHigh    = "_SPS_PlayerIdHigh";
    public const System.String PropSocketHole      = "_SPS_SocketHole";
    public const System.String PropDoubleSided     = "_SPS_SocketDoubleSided";
    public const System.String PropPortal          = "_SPS_SocketPortal";
    public const System.String PropRadiusOffset    = "_SPS_SocketRadiusOffset";
    public const System.String PropGuidedTargetIdLow  = "_SPS_GuidedTargetIdLow";
    public const System.String PropGuidedTargetIdHigh = "_SPS_GuidedTargetIdHigh";
    public const System.String PropUseTangentIn    = "_SPS_SocketUseTangentIn";
    public const System.String PropUseTangentOut   = "_SPS_SocketUseTangentOut";
    public const System.String PropTangentInX      = "_SPS_SocketTangentIn.x";
    public const System.String PropTangentInY      = "_SPS_SocketTangentIn.y";
    public const System.String PropTangentInZ      = "_SPS_SocketTangentIn.z";
    public const System.String PropTangentOutX     = "_SPS_SocketTangentOut.x";
    public const System.String PropTangentOutY     = "_SPS_SocketTangentOut.y";
    public const System.String PropTangentOutZ     = "_SPS_SocketTangentOut.z";
    public const System.String TagPropPrefix       = "_SPS_SocketTag";
    public const System.String TagPropSuffixLow    = "Low";
    public const System.String TagPropSuffixHigh   = "High";
    public const System.UInt32 SharedTag           = 1337;
    public const System.UInt32 IncludeSelf         = 1;
    public const System.UInt32 IncludeOthers       = 2;
    public const float MarkerBoundsExtent          = 5f;
    
    /* ── Generate a unique marker ID ── */
    public static System.UInt32 NewMarkerId()
    { var hash = (System.UInt32)System.Guid.NewGuid().GetHashCode(); return hash == 0 ? 1u : hash; }
    public static System.UInt32 GetLow(System.UInt32 v) => v & 0xffffu;
    public static System.UInt32 GetHigh(System.UInt32 v) => v >> 16;
    /* ── Create the trigger mesh (tiny triangle) ── */
    private static UnityEngine.Mesh _triggerMesh;
    public static UnityEngine.Mesh GetTriggerMesh()
    { if (_triggerMesh != null) return _triggerMesh;
      _triggerMesh = new UnityEngine.Mesh();
      _triggerMesh.name = "SpsTriggerMesh";
      _triggerMesh.vertices = new UnityEngine.Vector3[] {
        new UnityEngine.Vector3(-0.005f,-0.005f,0),
        new UnityEngine.Vector3(-0.005f,0.005f,0),
        new UnityEngine.Vector3(0.005f,0.005f,0) };
      _triggerMesh.uv = new UnityEngine.Vector2[] {
        new UnityEngine.Vector2(0,0),
        new UnityEngine.Vector2(1,0),
        new UnityEngine.Vector2(0,1) };
      _triggerMesh.triangles = new int[] { 0,1,2 };
      _triggerMesh.bounds = new UnityEngine.Bounds(UnityEngine.Vector3.zero,UnityEngine.Vector3.one * MarkerBoundsExtent * 2);
      return _triggerMesh; }
    /* ── Create a socket marker GameObject at position ── */
    public static UnityEngine.GameObject CreateSocketMarker(UnityEngine.Transform parent,UnityEngine.Vector3 localPos,UnityEngine.Quaternion localRot,MarkerConfig cfg)
    { var go = new UnityEngine.GameObject("SPS_Marker_" + (cfg.socketName ?? "Socket"));
      go.transform.SetParent(parent,false);
      go.transform.localPosition = localPos;
      go.transform.localRotation = localRot;
      go.transform.localScale = UnityEngine.Vector3.one * 0.001f;
      var mf = go.AddComponent<UnityEngine.MeshFilter>();
      mf.sharedMesh = GetTriggerMesh();
      var mr = go.AddComponent<UnityEngine.MeshRenderer>();
      var mat = CreateMarkerMaterial();
      mr.sharedMaterials = new[] { mat,null };
      mr.enabled = true;
      return go; }
    /* ── Create the shared marker material ── */
    private static UnityEngine.Material _markerMaterial;
    public static UnityEngine.Material CreateMarkerMaterial()
    { if (_markerMaterial != null) return _markerMaterial;
      var shader = UnityEngine.Shader.Find(SpsSocketShader);
      if (shader == null) { UnityEngine.Debug.LogWarning("[SPS2] Shader " + SpsSocketShader + " not found. Using fallback.");
        _markerMaterial = new UnityEngine.Material(UnityEngine.Shader.Find("Standard")); return _markerMaterial; }
      _markerMaterial = new UnityEngine.Material(shader);
      _markerMaterial.enableInstancing = true;
      return _markerMaterial; }
    /* ── Hash a tag string to uint ── */
    public static System.UInt32 HashTag(System.String tag)
    { if (System.String.IsNullOrEmpty(tag)) return 0;
      var hash = (System.UInt32)tag.GetHashCode(); return hash == 0 ? 1u : hash; }
    /* ── Build the 8 socket tags array from config ── */
    public static System.UInt32[] BuildSocketTags(MarkerConfig cfg,UnityEngine.Vector3 worldPos,UnityEngine.Transform avatarRoot)
    { var tags = new System.UInt32[8];
      if (cfg.compatMode == 2 || cfg.compatMode == 0) /* SPS2 or All */
      { /* Auto-tag based on body position (hips/head/chest/hand/foot) */
        System.String[] autoTags = GetAutoTags(worldPos,avatarRoot);
        for (int i = 0; i < autoTags.Length && i < 5; i++) tags[i] = HashTag(autoTags[i]);
        tags[7] = SharedTag; }
      /* Apply user tags */
      if (cfg.tags != null) for (int i = 0; i < cfg.tags.Length && i < 2; i++) tags[i] = HashTag(cfg.tags[i]);
      return tags; }
    /* ── Auto-detect body zone tags ── */
    public static System.String[] GetAutoTags(UnityEngine.Vector3 worldPos,UnityEngine.Transform root)
    { if (root == null) return new System.String[] { };
      var results = new System.Collections.Generic.List<System.String>();
      var anim = root.GetComponent<UnityEngine.Animator>();
      if (anim == null || !anim.isHuman) return new[] { "body" };
      float nearestDist = float.MaxValue;
      System.String nearestName = "body";
      var bones = new (UnityEngine.HumanBodyBones bone,System.String name)[]
      { (UnityEngine.HumanBodyBones.Hips,"hips"),(UnityEngine.HumanBodyBones.Head,"head"),(UnityEngine.HumanBodyBones.Chest,"chest"),
        (UnityEngine.HumanBodyBones.LeftHand,"handleft"),(UnityEngine.HumanBodyBones.RightHand,"handright"),
        (UnityEngine.HumanBodyBones.LeftFoot,"footleft"),(UnityEngine.HumanBodyBones.RightFoot,"footright") };
      foreach (var (bone,name) in bones)
      { var t = anim.GetBoneTransform(bone); if (t == null) continue;
        float d = UnityEngine.Vector3.Distance(worldPos,t.position);
        if (d < 0.15f) results.Add(name);
        if (d < nearestDist) { nearestDist = d; nearestName = name; } }
      if (results.Count == 0) results.Add(nearestName);
      return results.ToArray(); }
    /* ── Generate material property list from MarkerConfig ── */
    public static System.Collections.Generic.List<(System.String property,System.Single value)> GetMaterialProperties(MarkerConfig cfg,UnityEngine.Transform markerTransform)
    { var props = new System.Collections.Generic.List<(System.String,System.Single)>();
      void Add(System.String p,System.Single v) { props.Add((p,v)); }
      Add(PropConfigured,1f);
      SplitId(Add,PropIdLow,PropIdHigh,cfg.socketId);
      Add(PropSocketHole,(cfg.lightType == 1 || cfg.lightType == 4) ? 1f : 0f); /* Hole or RingOneWay */
      Add(PropDoubleSided,(cfg.lightType == 2 || cfg.lightType == 4) ? 1f : 0f); /* Ring or RingOneWay */
      Add(PropRadiusOffset,cfg.useRadiusOffset ? 1f : 0f);
      if (cfg.guidedTargetId != 0) SplitId(Add,PropGuidedTargetIdLow,PropGuidedTargetIdHigh,cfg.guidedTargetId);
      Add(PropUseTangentIn,cfg.useTangentIn ? 1f : 0f);
      Add(PropUseTangentOut,cfg.useTangentOut ? 1f : 0f);
      if (cfg.useTangentIn) { Add(PropTangentInX,cfg.tangentIn.x); Add(PropTangentInY,cfg.tangentIn.y); Add(PropTangentInZ,cfg.tangentIn.z); }
      if (cfg.useTangentOut) { Add(PropTangentOutX,cfg.tangentOut.x); Add(PropTangentOutY,cfg.tangentOut.y); Add(PropTangentOutZ,cfg.tangentOut.z); }
      if (markerTransform != null && markerTransform.parent != null)
      { Add("m_LocalScale.x",markerTransform.localScale.x); Add("m_LocalScale.y",markerTransform.localScale.y); Add("m_LocalScale.z",markerTransform.localScale.z); }
      return props; }
    static void SplitId(System.Action<System.String,System.Single> add,System.String lowProp,System.String highProp,System.UInt32 val)
    { add(lowProp,GetLow(val)); add(highProp,GetHigh(val)); }
  
}
}
}
#endif
