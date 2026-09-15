#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static partial class AvatarValidator {
 
  /** <summary>Entry point: give any UnityEngine.GameObject,get full UnityEngine.Avatar stats. Fully offline.</summary> */
  public static Metrics Test(UnityEngine.GameObject go)
  { var m = new Metrics();
  if (go == null) { m.Warning = "Null UnityEngine.GameObject"; return m; }
  var ad = go.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
  if (ad == null) ad = go.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
  if (ad == null) { m.Warning = "No VRC.SDK3.Avatars.Components.VRCAvatarDescriptor on " + go.name; return m; }
  return Collect(ad); }
  static readonly System.Reflection.PropertyInfo _lbsn = typeof(UnityEditor.ModelImporter).GetProperty("legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes",System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
  const int MAX_ACTION_TEXTURE_SIZE = 256;
  static Metrics Collect(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { var m = new Metrics();
  if (ad == null) { m.Warning = "Null descriptor"; return m; }
  /* Manual counting works in both UnityEditor.Editor and batch mode — no SDK dependency */
  var go = ad.gameObject;
  var allSmrs = go.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
  var allMrs = go.GetComponentsInChildren<UnityEngine.MeshRenderer>(true);
  var allRenderers = go.GetComponentsInChildren<UnityEngine.Renderer>(true);
  var allTransforms = go.GetComponentsInChildren<UnityEngine.Transform>(true);
  m.SkinnedMeshCount = allSmrs.Length;
  int totalTris = 0;
  var mats = new System.Collections.Generic.HashSet<UnityEngine.Material>();
  foreach (var smr in allSmrs)
  { if (smr.sharedMesh != null)
    { totalTris += smr.sharedMesh.triangles.Length / 3; }
    foreach (var mat in smr.sharedMaterials) if (mat != null) mats.Add(mat); }
  foreach (var mr in allMrs)
  { var mf = mr.GetComponent<UnityEngine.MeshFilter>();
    if (mf != null && mf.sharedMesh != null) totalTris += mf.sharedMesh.triangles.Length / 3;
    foreach (var mat in mr.sharedMaterials) if (mat != null) mats.Add(mat); }
  m.PolyCount = totalTris;
  m.MaterialCount = mats.Count;
  /* Count non-SMR transforms as bones */
  m.BoneCount = 0;
  foreach (var t in allTransforms) { if (t.GetComponent<UnityEngine.SkinnedMeshRenderer>() == null) m.BoneCount++; }
  /* Count mesh count (unique meshes) */
  var meshes = new System.Collections.Generic.HashSet<UnityEngine.Mesh>();
  foreach (var smr in allSmrs) if (smr.sharedMesh != null) meshes.Add(smr.sharedMesh);
  foreach (var mr in allMrs)
  { var mf = mr.GetComponent<UnityEngine.MeshFilter>(); if (mf != null && mf.sharedMesh != null) meshes.Add(mf.sharedMesh); }
  m.MeshCount = meshes.Count;
  /* Animators */
  var animators = go.GetComponentsInChildren<UnityEngine.Animator>(true);
  m.AnimatorCount = animators.Length;
  /* Audio sources */
  m.AudioSourceCount = go.GetComponentsInChildren<UnityEngine.AudioSource>(true).Length;
  /* Lights */
  m.LightCount = go.GetComponentsInChildren<UnityEngine.Light>(true).Length;
  /* Particle systems */
  var pss = go.GetComponentsInChildren<UnityEngine.ParticleSystem>(true);
  m.ParticleSystemCount = pss.Length;
  m.ParticleTotalCount = 0;
  foreach (var ps in pss) m.ParticleTotalCount += ps.particleCount;
  /* UnityEngine.Cloth */
  m.ClothCount = go.GetComponentsInChildren<UnityEngine.Cloth>(true).Length;
  /* Line / trail renderers */
  m.LineRendererCount = go.GetComponentsInChildren<UnityEngine.LineRenderer>(true).Length;
  m.TrailRendererCount = go.GetComponentsInChildren<UnityEngine.TrailRenderer>(true).Length;
  /* Physics colliders / rigidbodies */
  m.PhysicsColliderCount = go.GetComponentsInChildren<UnityEngine.Collider>(true).Length;
  m.PhysicsRigidbodyCount = go.GetComponentsInChildren<UnityEngine.Rigidbody>(true).Length;
  /* PhysBones */
  var physBones = go.GetComponentsInChildren<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>(true);
  m.PhysBoneComponentCount = physBones.Length;
  m.PhysBoneTransformCount = 0;
  foreach (var pb in physBones) if (pb.rootTransform != null) m.PhysBoneTransformCount += pb.rootTransform.GetComponentsInChildren<UnityEngine.Transform>(true).Length;
  m.PhysBoneColliderCount = go.GetComponentsInChildren<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider>(true).Length;
  /* Contacts */
  m.ContactCount = go.GetComponentsInChildren<VRC.Dynamics.ContactBase>(true).Length;
  /* Constraints */
  m.ConstraintCount = go.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true).Length;
  m.RaycastCount = go.GetComponentsInChildren<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>(true).Length + 1; /* Approximate: one raycast per PhysBone + contact */
  /* Performance rank — approximate based on SDK guidelines */
  m.PerformanceRank = EstimatePerformanceRank(m);
  m.GlobalColliderCount = CountGlobalColliders(ad);
  var anim = ad.GetComponent<UnityEngine.Animator>();
  m.HasAnimator = anim != null;
  m.IsHumanoid = anim != null && anim.isHuman;
  var ad3 = ad as VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
  if (ad3 != null)
  { m.HasExpressionParams = ad3.expressionParameters != null;
    m.HasExpressionMenu = ad3.expressionsMenu != null;
    m.ParamCost = ad3.expressionParameters != null ? ad3.expressionParameters.CalcTotalCost() : 0;
    m.HasMixedWriteDefaults = ScanAvatarForWriteDefaultsMixture(ad3);
    m.HasWDOffEmptyClips = ScanAvatarForWriteDefaultsOffEmptyClips(ad3);
    m.DynamicBoneCount = CountDynamicBones(ad3);
    m.DynamicBoneColliderCount = CountDynamicBoneColliders(ad3);
    m.UnityConstraintCount = CountUnityConstraints(ad3);
    m.IllegalComponentCount = CountIllegalComponents(ad3);
    m.GlobalColliderCount = 0;
    m.AudioClipsMissingLoadInBackground = CountAudioClipsMissingLoadInBackground(ad3);
    CountParticleIssues(ad3,out m.ParticleAutoDestructCount,out m.ParticleAutoDisableRootCount,out m.TrailAutoDestructCount,out m.ParticleCollisionLayerCount,out m.ParticleLightCount); }
  m.HasVisemes = ad.lipSync == VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape && ad.VisemeSkinnedMesh != null;
  m.OversizeTextures = CountOversizeTextures(ad);
  m.NonStreamingMipTextures = CountNonStreamingMipTextures(ad);
  m.HasUnassignedGroups = false;
  m.HasCyclicDependencies = false;
  m.Warning = BuildWarning(m,ad,anim,ad3);
  return m; }
  /** <summary>Approximate SDK performance rank from manual counts. Works in any mode.</summary> */
  static System.String EstimatePerformanceRank(Metrics m)
  { /* VRChat SDK approximate thresholds for PC */
  if (m.PolyCount > 70000 || m.BoneCount > 200 || m.MaterialCount > 50 || m.PhysBoneComponentCount > 24) return "VeryPoor";
  if (m.PolyCount > 32000 || m.BoneCount > 150 || m.MaterialCount > 30 || m.PhysBoneComponentCount > 12) return "Poor";
  if (m.PolyCount > 10000 || m.BoneCount > 100 || m.MaterialCount > 15 || m.PhysBoneComponentCount > 8) return "Medium";
  if (m.PolyCount > 5000 || m.BoneCount > 75 || m.MaterialCount > 10 || m.PhysBoneComponentCount > 4) return "Good";
  return "Excellent"; }
  static System.String BuildWarning(Metrics m,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad,UnityEngine.Animator anim,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad3)
  { var sb = new System.Text.StringBuilder();
  sb.Append($"<b>Rank:</b>{m.PerformanceRank}  <b>Poly:</b>{m.PolyCount:N0}  <b>Bones:</b>{m.BoneCount}  <b>Mat:</b>{m.MaterialCount}");
  sb.Append($"  <b>PhysB:</b>{m.PhysBoneComponentCount}/{m.PhysBoneColliderCount}  <b>Constr:</b>{m.ConstraintCount}  <b>Contacts:</b>{m.ContactCount}");
  sb.Append($"  <b>Params:</b>{m.ParamCost}/256  <b>Human:</b>{m.IsHumanoid}");
  if (anim != null && !anim.isHuman) sb.Append("  <color=orange>!NonHumanoid</color>");
  if (m.PerformanceRank is "Poor" or "VeryPoor") sb.Append($"  <color=red>!Rank:{m.PerformanceRank}</color>");
  if (m.HasMixedWriteDefaults) sb.Append("  <color=orange>!MixedWD</color>");
  if (m.HasWDOffEmptyClips) sb.Append("  <color=red>!WDOffEmpty</color>");
  if (m.DynamicBoneCount + m.DynamicBoneColliderCount > 0) sb.Append($"  <color=orange>!DB:{m.DynamicBoneCount}/{m.DynamicBoneColliderCount}</color>");
  if (m.UnityConstraintCount > 0) sb.Append($"  <color=orange>!UnityC:{m.UnityConstraintCount}</color>");
  if (m.IllegalComponentCount > 0) sb.Append($"  <color=red>!Illegal:{m.IllegalComponentCount}</color>");
  if (m.GlobalColliderCount > 16) sb.Append($"  <color=red>!GlobCol:{m.GlobalColliderCount}</color>");
  if (m.AudioClipsMissingLoadInBackground > 0) sb.Append($"  <color=red>!AudioBg:{m.AudioClipsMissingLoadInBackground}</color>");
  if (m.ParticleAutoDestructCount > 0) sb.Append($"  <color=red>!PAutoD:{m.ParticleAutoDestructCount}</color>");
  if (m.ParticleAutoDisableRootCount > 0) sb.Append($"  <color=red>!PRootD:{m.ParticleAutoDisableRootCount}</color>");
  if (m.TrailAutoDestructCount > 0) sb.Append($"  <color=red>!TAutoD:{m.TrailAutoDestructCount}</color>");
  if (m.ParticleCollisionLayerCount > 0) sb.Append($"  <color=red>!PColL:{m.ParticleCollisionLayerCount}</color>");
  if (m.ParticleLightCount > 0) sb.Append($"  <color=orange>!PLight:{m.ParticleLightCount}</color>");
  if (ad.lipSync == VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape && ad.VisemeSkinnedMesh == null) sb.Append("  <color=red>!VisemeNoMesh</color>");
  if (m.OversizeTextures > 0) sb.Append($"  <color=red>!OvrTex:{m.OversizeTextures}</color>");
  if (m.NonStreamingMipTextures > 0) sb.Append($"  <color=red>!MipStr:{m.NonStreamingMipTextures}</color>");
  if (m.HasUnassignedGroups) sb.Append("  <color=red>!PBUnassigned</color>");
  if (m.HasCyclicDependencies) sb.Append("  <color=orange>!PBCyclic</color>");
  if (ad3 != null)
  { if (m.ParamCost > 256) sb.Append("  <color=red>!ParamOvr</color>");
    if (ad3.expressionsMenu != null && ad3.expressionParameters == null) sb.Append("  <color=red>!MenuNoParams</color>");
    if (ad3.expressionParameters != null && ad3.expressionsMenu == null) sb.Append("  <color=red>!ParamsNoMenu</color>");
    if (ad3.expressionParameters == null && ad3.expressionsMenu == null) sb.Append("  <color=orange>!NoExpressions</color>");
    /* Gesture layer mask check */
    if (ad3.baseAnimationLayers != null && ad3.baseAnimationLayers.Length > 2)
    { var gl = ad3.baseAnimationLayers[2];
    if (gl.animatorController != null && !gl.isDefault && gl.type == VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture)
    { var gestCtrl = gl.animatorController as UnityEditor.Animations.AnimatorController;
      if (gestCtrl != null && gestCtrl.layers.Length > 0 && gestCtrl.layers[0].avatarMask == null)
      sb.Append("  <color=red>!GestureMask</color>"); } }
    /* ViewPosition too low or at default */
    if (ad.ViewPosition.y < 0.5f) sb.Append("  <color=red>!ViewLow</color>"); }
  return sb.ToString(); }
  public static System.String Summary(UnityEngine.GameObject go)
  { var m = Test(go);
  return System.String.IsNullOrEmpty(m.Warning) ? "UnityEngine.Avatar OK" : m.Warning; }
  /* ── Internal helpers ────────────────────────────────────────────── */
  static int CountDynamicBones(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { return 0; }
  static int CountDynamicBoneColliders(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { return 0; }
  static int CountUnityConstraints(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { var cs = ad.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true);
  return cs.Length; }
  static int CountIllegalComponents(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { var illegal = VRC.SDK3.Validation.AvatarValidation.FindIllegalComponents(ad.gameObject);
  return System.Linq.Enumerable.Count(illegal); }
  static int CountGlobalColliders(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { return 0; }
  static int CountAudioClipsMissingLoadInBackground(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad)
  { var clips = new System.Collections.Generic.HashSet<UnityEngine.AudioClip>();
  foreach (var src in ad.GetComponentsInChildren<UnityEngine.AudioSource>(true))
    if (src.clip != null && src.clip.loadType == UnityEngine.AudioClipLoadType.DecompressOnLoad && !src.clip.loadInBackground)
    clips.Add(src.clip);
  return clips.Count; }
  static int CountOversizeTextures(UnityEngine.Component avatar)
  { var renderers = System.Linq.Enumerable.ToList(avatar.GetComponentsInChildren<UnityEngine.Renderer>(true));
  var bad = VRCSdkControlPanel.GetOversizeTextureImporters(renderers);
  return bad.Count; }
  static int CountNonStreamingMipTextures(UnityEngine.Component avatar)
  { var bad = new System.Collections.Generic.HashSet<System.String>();
  foreach (var r in avatar.GetComponentsInChildren<UnityEngine.Renderer>(true))
    foreach (var m in r.sharedMaterials)
    { if (!m) continue;
    foreach (int id in m.GetTexturePropertyNameIDs())
    { var t = m.GetTexture(id);
      if (!t) continue;
      var path = UnityEditor.AssetDatabase.GetAssetPath(t);
      if (System.String.IsNullOrEmpty(path)) continue;
      var imp = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
      if (imp != null && imp.mipmapEnabled && !imp.streamingMipmaps) bad.Add(path); } }
  return bad.Count; }
  static void CountParticleIssues(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad,out int autoDestroy,out int autoDisableRoot,out int trailAutoDestruct,out int collisionLayer,out int lightCount)
  { autoDestroy = 0; autoDisableRoot = 0; trailAutoDestruct = 0; collisionLayer = 0; lightCount = 0;
  foreach (var ps in ad.GetComponentsInChildren<UnityEngine.ParticleSystem>(true))
  { if (ps.main.stopAction == UnityEngine.ParticleSystemStopAction.Destroy) autoDestroy++;
    else if (ps.main.stopAction == UnityEngine.ParticleSystemStopAction.Disable && ps.gameObject == ad.gameObject) autoDisableRoot++;
    if (ps.lights.light != null && ps.lights.maxLights > 0) lightCount++; }
  foreach (var tr in ad.GetComponentsInChildren<UnityEngine.TrailRenderer>(true))
    if (tr.autodestruct) trailAutoDestruct++; }
  /* ── Write Defaults scanners (mirrored from SDK) ─────────────────── */
#if UNITY_EDITOR
  
  static System.Boolean ScanAvatarForWriteDefaultsMixture(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad3)
  { if (ad3 == null) return false;
  foreach (var cl in ad3.baseAnimationLayers)
  { var ctrl = cl.animatorController as UnityEditor.Animations.AnimatorController;
    if (ctrl == null) continue;
    var result = WDSR.NoStates;
    foreach (var layer in ctrl.layers)
    { if (layer.blendingMode == UnityEditor.Animations.AnimatorLayerBlendingMode.Additive) continue;
    result = ScanSMForWDMixture(layer.stateMachine,result);
    if (result == WDSR.Mixture) return true; } }
  return false; }
  static WDSR ScanSMForWDMixture(UnityEditor.Animations.AnimatorStateMachine sm,WDSR running)
  { if (running == WDSR.Mixture) return WDSR.Mixture;
  foreach (var cs in sm.states)
  { var bt = cs.state.motion as UnityEditor.Animations.BlendTree;
    if (bt != null && bt.blendType == UnityEditor.Animations.BlendTreeType.Direct) continue;
    System.Boolean wd = cs.state.writeDefaultValues;
    if (running == WDSR.NoStates) running = wd ? WDSR.AllEnabled : WDSR.AllDisabled;
    else { System.Boolean expected = running == WDSR.AllEnabled; if (wd != expected) return WDSR.Mixture; } }
  foreach (var csm in sm.stateMachines)
  { running = ScanSMForWDMixture(csm.stateMachine,running);
    if (running == WDSR.Mixture) return WDSR.Mixture; }
  return running; }
  static System.Boolean ScanAvatarForWriteDefaultsOffEmptyClips(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ad3)
  { if (ad3 == null) return false;
  foreach (var cl in ad3.baseAnimationLayers)
  { var ctrl = cl.animatorController as UnityEditor.Animations.AnimatorController;
    if (ctrl == null) continue;
    foreach (var layer in ctrl.layers)
    if (ScanSMForWDOffEmpty(layer.stateMachine)) return true; }
  return false; }
  static System.Boolean ScanSMForWDOffEmpty(UnityEditor.Animations.AnimatorStateMachine sm)
  { foreach (var cs in sm.states)
  { if (cs.state.writeDefaultValues) continue;
    if (CheckMotion(cs.state.motion)) return true; }
  foreach (var csm in sm.stateMachines)
    if (ScanSMForWDOffEmpty(csm.stateMachine)) return true;
  return false;
  System.Boolean CheckMotion(UnityEngine.Motion motion)
  { if (motion == null) return true;
    if (motion is UnityEngine.AnimationClip ac && ac.empty) return true;
    if (motion is UnityEditor.Animations.BlendTree bt)
    foreach (var cm in bt.children)
      if (CheckMotion(cm.motion)) return true;
    return false; } } 
#endif
}
}
}
}
#endif
