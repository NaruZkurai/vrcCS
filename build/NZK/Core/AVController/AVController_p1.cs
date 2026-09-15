#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class AVController
  {/* ================================================================
   *  Child allow check
   * ================================================================ */
  static System.Boolean ChildAllowed(C_AviGenerator hb,System.String name) => !hb.IsIgnored(HBChildren.Prefix + name) && hb.FindHB(name) != null;
  /// <summary>Create or assign the UnityEngine.Avatar root UnityEngine.GameObject on the C_AviGenerator.</summary>
  public static void SetAviRoot(C_AviGenerator hb)
  { if (hb == null) return;
    if (hb.NZKC_GO_AviRoot == null)
    { var t = hb.transform;
      var root = t.root;
      var rootGo = root.gameObject;
      /* Mode Root / This: C_AviGenerator sits on the actual avatar root — use it */
      if (hb.mode == E_AviGeneratorMode.Root || hb.mode == E_AviGeneratorMode.This)
      { hb.NZKC_GO_AviRoot = rootGo;
        hb.avatarRootName = rootGo.name;
        return; }
      /* Walk up parent transforms to find scene root */
      var walker = t;
      while (walker.parent != null) walker = walker.parent;
      var walkedRoot = walker.gameObject;
      hb.NZKC_GO_AviRoot = walkedRoot;
      hb.avatarRootName = walkedRoot.name;
      return; }
    /* Already set — just sync name */
    hb.avatarRootName = hb.NZKC_GO_AviRoot.name;
  }
  /** <summary>Build a VRCADSnapshot from current bake state and apply it atomically
   *  via VRCAD.Package.Restore. Ensures face mesh,expression assets,and all
   *  VRCAD settings are consistent at the end of a bake.
   *  Also validates UnityEngine.Avatar for VRChat upload readiness — warns about illegal
   *  scripts (BoneViewer,etc.) and missing critical components.</summary> */
  static void ConfigureVRCAD(C_AviGenerator hb)
  { if (hb.NZKC_GO_AviRoot == null) return;
    var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad == null) return;
    // Snapshot current VRCAD state (preserves controller slots already set)
    var snap = VRCAD.Package.Snapshot(vrcad);
    // Auto-assign face mesh if missing: search UnityEngine.Avatar root children first,// then fall back to scene-wide search for any UnityEngine.SkinnedMeshRenderer with 'face' in name
    UnityEngine.SkinnedMeshRenderer facePick = snap.visemeSkinnedMesh;
    if (facePick == null)
    {
    var smrs = hb.NZKC_GO_AviRoot.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
    foreach (var s in smrs)
    { var n = s.name.ToLower();
      if (n.Contains("face")) { facePick = s; break; }
      if (facePick == null && n.Contains("body")) facePick = s;
      if (facePick == null && s.sharedMesh != null && s.sharedMesh.blendShapeCount > 0) facePick = s; }
    // Fallback: scene-wide search
    if (facePick == null)
    { var allSmrs = UnityEngine.Object.FindObjectsOfType<UnityEngine.SkinnedMeshRenderer>();
      foreach (var s in allSmrs)
      { var n = s.name.ToLower();
      if (n.Contains("face")) { facePick = s; break; } } }
    if (facePick != null)
    { snap.visemeSkinnedMesh = facePick;
      UnityEngine.Debug.Log("[HB] ConfigureVRCAD: auto-assigned face mesh " + facePick.name); }
    else
      UnityEngine.Debug.LogWarning("[HB] ConfigureVRCAD: no face mesh found — VRCFaceMissing"); }
    // Map viseme blend shapes from the face mesh (even if reusing existing snapshot mesh)
    if (facePick != null && facePick.sharedMesh != null && snap.visemeBlendShapes == null)
    { var vNames = VisemeNames;
    var bs = new System.Collections.Generic.List<System.String>();
    for (int i = 0; i < facePick.sharedMesh.blendShapeCount; i++)
      bs.Add(facePick.sharedMesh.GetBlendShapeName(i).ToLower());
    var mapped = new System.String[15];
    for (int i = 0; i < 15; i++)
    { var found = false;
      foreach (var b in bs)
      { var parts = b.Split('_'); var last = parts.Length > 0 ? parts[parts.Length - 1] : "";
      if (last == vNames[i]) { mapped[i] = b; found = true; break; } }
      if (!found) mapped[i] = "-none-"; }
    snap.visemeBlendShapes = mapped;
    int matched = System.Linq.Enumerable.Count(mapped,s => s != "-none-");
    UnityEngine.Debug.Log("[HB] ConfigureVRCAD: mapped visemes for " + facePick.name + " matched=" + matched + "/15"); }
    // Ensure expression menu/params
    if (snap.expressionsMenu == null || snap.expressionParameters == null)
    AssignDefaultExpressionAssets(hb);
    // Ensure custom settings
    snap.customExpressions = true;
    snap.customizeAnimationLayers = true;
    if (snap.lipSync == VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle.Default)
    snap.lipSync = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape;
    // Apply all settings atomically
    VRCAD.Package.Restore(vrcad,snap);
    IF_UE.SetDirty(vrcad);
    // ── Strip BoneAssignment from UnityEngine.Avatar root hierarchy ──
    // (they stay on Armature_Armature/Armature_Stripped for reference,//  but VRChat rejects them on the upload target)
    int stripped = C_BoneAssignmentRegistry.DestroyAllInChildren(hb.NZKC_GO_AviRoot.transform);
    if (stripped > 0)
    UnityEngine.Debug.Log("[HB] ConfigureVRCAD: stripped " + stripped + " BoneAssignment components from UnityEngine.Avatar root");
    // ── Validation: scan for illegal/VRChat-blocked scripts on UnityEngine.Avatar root + children ──
    var illegal = VRC.SDK3.Validation.AvatarValidation.FindIllegalComponents(hb.NZKC_GO_AviRoot);
    if (illegal != null)
    { var names = new System.Collections.Generic.List<System.String>();
    foreach (var c in illegal) names.Add(c.GetType().Name);
    UnityEngine.Debug.LogError("[HB] ConfigureVRCAD: ILLEGAL COMPONENTS on avatar: " + System.String.Join(",",names) +
      " — these will be stripped by VRChat on upload. Remove them first."); }
    // ── Validation: check UnityEngine.Avatar state after build ──
    var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    if (anim == null)
    UnityEngine.Debug.LogError("[HB] ConfigureVRCAD: No UnityEngine.Animator on UnityEngine.Avatar root — CRITICAL");
    else if (anim.avatar == null)
    UnityEngine.Debug.LogError("[HB] ConfigureVRCAD: No UnityEngine.Avatar assigned to UnityEngine.Animator — INVALID AVATAR");
    else if (!anim.avatar.isValid || !anim.avatar.isHuman)
    UnityEngine.Debug.LogWarning("[HB] ConfigureVRCAD: UnityEngine.Avatar is not a valid humanoid — check armature mapping");
    else
    UnityEngine.Debug.Log("[HB] ConfigureVRCAD: UnityEngine.Avatar validated — humanoid OK (" + anim.avatar.name + ")");
    // ── Validation: check armature root exists ──
    if (hb.NZKC_GO_AviRoot.transform.Find("Armature") == null)
    UnityEngine.Debug.LogWarning("[HB] ConfigureVRCAD: No Armature child under UnityEngine.Avatar root — armature may not be synced");
    UnityEngine.Debug.Log("[HB] ConfigureVRCAD: done (faceMesh=" +
    (snap.visemeSkinnedMesh != null ? snap.visemeSkinnedMesh.name : "null") +
    " expMenu=" + (snap.expressionsMenu != null ? "set" : "null") +
    " expParams=" + (snap.expressionParameters != null ? "set" : "null") + ")"); }
  /* ================================================================
   *  UnityEngine.Avatar root
   * ================================================================ */
  public static void BakeAviRoot(C_AviGenerator hb)
  { /* Always create a fresh UnityEngine.Avatar root — don't reuse an inspector-picked Armature */
    if (hb.NZKC_GO_AviRoot != null)
    { /* When the avatar root IS the model root (Root/This mode) — keep it, don't destroy it */
      var t = hb.transform;
      var rootGo = t.root.gameObject;
      if (hb.NZKC_GO_AviRoot == rootGo)
      { IF_UE.RecObj(hb.NZKC_GO_AviRoot,"Update avatar root"); }
      else if (hb.NZKC_GO_AviRoot.name == "Armature" || UnityEditor.PrefabUtility.IsPartOfPrefabInstance(hb.NZKC_GO_AviRoot))
      hb.NZKC_GO_AviRoot = null;
      else { IF_UE.DestroyImmediate(hb.NZKC_GO_AviRoot); hb.NZKC_GO_AviRoot = null; } }
    SetAviRoot(hb);
    /* ── Null-position fix ────────────────────────────────── */
    if (hb.NZKC_GO_AviRoot != null)
    { hb.NZKC_GO_AviRoot.transform.position = UnityEngine.Vector3.zero; }
    var genTr = hb.transform;
    genTr.position = UnityEngine.Vector3.zero;
    foreach (UnityEngine.Transform c in genTr)
    { c.localPosition = UnityEngine.Vector3.zero; }
    if (hb.NZKC_GO_AviRoot == null) return;
    var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad == null) vrcad = hb.NZKC_GO_AviRoot.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad.ViewPosition == UnityEngine.Vector3.zero) VRCAD.Set.ViewPosition(vrcad,new UnityEngine.Vector3(0f,1f,0f));
    var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    if (anim == null) anim = hb.NZKC_GO_AviRoot.AddComponent<UnityEngine.Animator>();
    var pid = hb.pipelineId;
    UnityEngine.Debug.Log("[HB] Pipeline search: self.pipelineId='" + (pid ?? "null") + "'");
    if (System.String.IsNullOrEmpty(pid))
    { var pmType = FindPMType();
    UnityEngine.Debug.Log("[HB] Pipeline search: pmType=" + (pmType != null ? pmType.Name : "null"));
    foreach (UnityEngine.Transform c in hb.transform)
    { var h = c.GetComponent<C_AviGenerator>();
      if (h != null && (h.mode == E_AviGeneratorMode.AviRootBuilder || h.mode == E_AviGeneratorMode.PipelineID))
      { pid = ReadPipelineId(c,h,pmType); if (!System.String.IsNullOrEmpty(pid)) break; } }
    if (System.String.IsNullOrEmpty(pid))
    { foreach (var h in UnityEngine.Object.FindObjectsOfType<C_AviGenerator>())
      { if (h == hb || h.mode != E_AviGeneratorMode.PipelineID) continue;
      pid = ReadPipelineId(h.transform,h,pmType); if (!System.String.IsNullOrEmpty(pid)) break; } } }
    if (!System.String.IsNullOrEmpty(pid))
    { var pmType = FindPMType();
    if (pmType != null)
    { var old = hb.NZKC_GO_AviRoot.GetComponent(pmType);
      if (old != null) IF_UE.DestroyImmediate(old);
      var pm = hb.NZKC_GO_AviRoot.AddComponent(pmType);
      WriteBlueprintId(pm,pmType,pid);
      UnityEngine.Debug.Log("[HB] " + "Pipeline set: " + pid); }
    else UnityEngine.Debug.LogWarning("[HB] " + "VRC_PipelineManager type not found — can't set blueprint ID."); }
    else
    { var pmType = FindPMType();
    if (pmType != null && hb.NZKC_GO_AviRoot.GetComponent(pmType) == null)
    { var pm = hb.NZKC_GO_AviRoot.AddComponent(pmType);
      pid = System.Guid.NewGuid().ToString();
      WriteBlueprintId(pm,pmType,pid);
      UnityEngine.Debug.Log("[HB] " + "PipelineManager added with auto-assigned blueprint ID: " + pid); }
    }
    anim.avatar = null;
    UnityEngine.Debug.Log("[HB] UnityEngine.Avatar cleared — BuildAvatarFromArmature will rebuild from current armature.");
    if (hb.faceMesh != null && hb.useForVrcadFace)
    { var smr = hb.faceMesh.GetComponent<UnityEngine.SkinnedMeshRenderer>();
    if (smr != null) VRCAD.Set.VisemeMesh(vrcad,smr); }
    VRCAD.Set.LipSync(vrcad,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape);
    VRCAD.Set.CustomExpressions(vrcad,true);
    VRCAD.Set.CustomizeAnimLayers(vrcad,true);
    VRCAD.Set.InitAllLayers(vrcad);
    var eyeRoot = hb.NZKC_GO_AviRoot.transform.Find("Armature");
    if (eyeRoot != null)
    { UnityEngine.Transform leftEye = null,rightEye = null;
    foreach (UnityEngine.Transform t in eyeRoot.GetComponentsInChildren<UnityEngine.Transform>())
    { var ln = t.name.ToLower();
      if (ln.Contains("lefteye") || (ln.Contains("eye") && ln.Contains("left"))) leftEye = t;
      else if (ln.Contains("righteye") || (ln.Contains("eye") && ln.Contains("right"))) rightEye = t; }
    if (leftEye != null && rightEye != null)
    { VRCAD.Set.EyeLookSettings(vrcad,leftEye,rightEye);
      UnityEngine.Debug.Log("[HB] Auto-assigned eye bones: " + leftEye.name + "," + rightEye.name); } }
    UnityEngine.Debug.Log("[HB] VRC.SDK3.Avatars.Components.VRCAvatarDescriptor " + (vrcad != null ? "updated" : "added") + " on " + hb.avatarRootName);
    UnityEngine.Debug.Log("[HB] UnityEngine.Avatar root: " + hb.avatarRootName); }
  /* ================================================================
   *  Armature building
   * ================================================================ */
  static System.Boolean HasHips(UnityEngine.Transform arm)
  { foreach (UnityEngine.Transform c in arm.GetComponentsInChildren<UnityEngine.Transform>())
    { var n = c.name.ToLower();
    if (n == "hips" || n == "pelvis" || n.Contains("hips")) return true;
    if (c == arm && arm.childCount > 0)
    { int total = 0; CountDescendants(arm,ref total,0);
      if (total >= 10) return true; } }
    return false; }
  static void CountDescendants(UnityEngine.Transform t,ref int count,int depth)
  { if (depth > 0) count++;
    if (depth > 50) return;
    for (int i = 0; i < t.childCount; i++) CountDescendants(t.GetChild(i),ref count,depth + 1); }
  public static void BuildAvatarFromArmature(C_AviGenerator hb)
  { if (hb.NZKC_GO_AviRoot == null) { UnityEngine.Debug.LogWarning("[HB] No UnityEngine.Avatar root."); return; }
    var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    if (anim == null) { UnityEngine.Debug.LogWarning("[HB] No UnityEngine.Animator on UnityEngine.Avatar root."); return; }
    if (anim.avatar != null && anim.avatar.isHuman)
    { UnityEngine.Debug.Log("[HB] Humanoid UnityEngine.Avatar already set — FORCING REBUILD with current armature.");
    anim.avatar = null; }
    if (anim.avatar != null) UnityEngine.Debug.Log("[HB] Current UnityEngine.Avatar is generic — will attempt humanoid build from armature.");
    else UnityEngine.Debug.Log("[HB] No UnityEngine.Avatar set — attempting humanoid build from armature.");
    var arm = hb.NZKC_GO_AviRoot.transform.Find("Armature");
    System.String foundSource = null;
    if (arm == null)
    { UnityEngine.Transform ar = hb.armatureRoot; if (ar != null) foundSource = "armatureRoot field";
    if (ar == null) foreach (UnityEngine.Transform c in hb.transform)
    { var h = c.GetComponent<C_AviGenerator>();
      if (h != null && h.mode == E_AviGeneratorMode.ArmatureBuilder && h.armatureRoot != null) { ar = h.armatureRoot; foundSource = "child ArmatureBuilder"; break; } }
    if (ar == null) foreach (UnityEngine.Transform c in hb.transform)
      if (c.name.StartsWith("Armature_"))
      { var m = c.GetComponent<C_AviGenerator>();
      if (m != null && m.originalArmatureSource != null) { ar = m.originalArmatureSource; foundSource = "cloned armature (" + c.name + ")"; break; } }
    if (ar != null)
    { UnityEngine.Debug.Log("[HB] Found armature source via " + foundSource + " — syncing to UnityEngine.Avatar root.");
      SyncAvatarArmature(hb,ar,ar); arm = hb.NZKC_GO_AviRoot.transform.Find("Armature"); }
    else UnityEngine.Debug.Log("[HB] No armature source found anywhere."); }
    if (arm == null) { UnityEngine.Debug.LogWarning("[HB] No armature source found — cannot build humanoid avatar. Assign armatureRoot and re-bake."); return; }
    var armChildren = System.String.Join(",",System.Linq.Enumerable.Select(arm.GetComponentsInChildren<UnityEngine.Transform>(), t => t.name));
    if (!HasHips(arm))
    { UnityEngine.Debug.LogWarning("[HB] No Hips in armature. Armature children (" + arm.GetComponentsInChildren<UnityEngine.Transform>().Length + "): " + armChildren);
    return; }
    /* Clean up any Armature_Auto nested under Armature to prevent bone name ambiguity */
    var autoChild = arm.Find("Armature_Auto");
    if (autoChild != null)
    { UnityEngine.Debug.Log("[HB] Removing Armature_Auto nested under Armature to prevent duplicate bone names.");
    IF_UE.DestroyImmediateGameObject(autoChild.gameObject); }
    UnityEngine.Transform armatureStrip = null,armatureArm = null;
    /* Search direct children + HB_Generated container */
    foreach (UnityEngine.Transform c in hb.transform)
    { if (c.name == "Armature_Stripped") armatureStrip = c;
    if (c.name == "Armature_Armature") armatureArm = c; }
    var hbGen = hb.transform.Find("HB_Generated");
    if (hbGen != null) {
    foreach (UnityEngine.Transform c in hbGen)
    { if (c.name == "Armature_Stripped") armatureStrip = c;
      if (c.name == "Armature_Armature") armatureArm = c; }
    }
    /* Search inside ArmatureBuilder children */
    if (armatureStrip == null || armatureArm == null)
    { foreach (UnityEngine.Transform c in hb.transform)
    { if (c.GetComponent<C_AviGenerator>()?.mode != E_AviGeneratorMode.ArmatureBuilder) continue;
      foreach (UnityEngine.Transform cc in c)
      { if (cc.name == "Armature_Stripped") armatureStrip = cc;
      if (cc.name == "Armature_Armature") armatureArm = cc; } } }
    /* Fallback: find orphaned root-level clones via UnityEngine.GameObject.Find (from previous auto-gen bakes) */
    if (armatureStrip == null)
    { var go = UnityEngine.GameObject.Find("Armature_Stripped");
    if (go != null) armatureStrip = go.transform; }
    if (armatureArm == null)
    { var go = UnityEngine.GameObject.Find("Armature_Armature");
    if (go != null) armatureArm = go.transform; }
    /* Prefer UnityEngine.Avatar root's Armature over Armature_Auto to avoid duplicate bone names */
    if ((armatureStrip == null || armatureArm == null) && arm != null)
    { armatureStrip = armatureStrip ?? arm;
    armatureArm = armatureArm ?? arm; }
    /* Last resort: find Armature_Auto anywhere in scene (may be orphaned root object) */
    if (armatureStrip == null || armatureArm == null)
    { var autoGO = UnityEngine.GameObject.Find("Armature_Auto");
    if (autoGO != null)
    { if (armatureStrip == null) armatureStrip = autoGO.transform;
      if (armatureArm == null) armatureArm = autoGO.transform; } }
    if (armatureStrip == null) armatureStrip = arm;
    if (armatureArm == null) armatureArm = arm;
    UnityEngine.Transform mapSource = armatureStrip;
    UnityEngine.Transform skelSource = arm;
    UnityEngine.Debug.Log("[HB] Building humanoid UnityEngine.Avatar — mapping from " + mapSource.name
    + ",skeleton from " + skelSource.name + " (" + skelSource.GetComponentsInChildren<UnityEngine.Transform>().Length + " total bones).");
    var hbMap = MapHumanoid(mapSource);
    var skel = BuildSkeleton(skelSource,hb.NZKC_GO_AviRoot.transform);
    var desc = new UnityEngine.HumanDescription { human = hbMap,skeleton = skel,armStretch = 0.05f,legStretch = 0.05f,upperArmTwist = 0.5f,lowerArmTwist = 0.5f,upperLegTwist = 0.5f,lowerLegTwist = 0.5f,feetSpacing = 0f,hasTranslationDoF = false };
    var av = UnityEngine.AvatarBuilder.BuildHumanAvatar(hb.NZKC_GO_AviRoot,desc);
    if (av != null) av.name = hb.avatarRootName + "_Avatar";
    UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: BuildHumanAvatar result=" + (av != null ? (av.isValid.ToString() + " name=" + av.name) : "NULL"));
    if (av != null && av.isValid)
    { var avDir = NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Avatars) + "/";
    if (!System.IO.Directory.Exists(avDir)) System.IO.Directory.CreateDirectory(avDir);
    System.String refAvPath = avDir + hb.avatarRootName + "_Stripped.asset";
    UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: saving stripped UnityEngine.Avatar to " + refAvPath);
    var existingRef = IF_UE.Load<UnityEngine.Avatar>(refAvPath);
    if (existingRef != null) { UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: deleting existing stripped avatar"); IF_UE.DeleteAsset(refAvPath); }
    IF_UE.CreateAsset(av,refAvPath);
    var refAvatar = IF_UE.Load<UnityEngine.Avatar>(refAvPath);
    UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: refAvatar (loaded)=" + (refAvatar != null ? ("name=" + refAvatar.name + " valid=" + refAvatar.isValid + " human=" + refAvatar.isHuman) : "NULL"));
    if (refAvatar == null) { UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: Load returned null,via in-memory av"); refAvatar = av; }
    UnityEngine.Debug.Log("[HB] Stripped UnityEngine.Avatar saved: " + refAvPath);
    var fullSkel = BuildSkeleton(skelSource,hb.NZKC_GO_AviRoot.transform);
    var fullDesc = new UnityEngine.HumanDescription { human = hbMap,skeleton = fullSkel,armStretch = 0.05f,legStretch = 0.05f,upperArmTwist = 0.5f,lowerArmTwist = 0.5f,upperLegTwist = 0.5f,lowerLegTwist = 0.5f,feetSpacing = 0f,hasTranslationDoF = false };
    var fullAv = UnityEngine.AvatarBuilder.BuildHumanAvatar(hb.NZKC_GO_AviRoot,fullDesc);
    if (fullAv != null) fullAv.name = hb.avatarRootName + "_FullAvatar";
    UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: fullAv (built)=" + (fullAv != null ? ("name=" + fullAv.name + " valid=" + fullAv.isValid) : "NULL"));
    System.String fullAvPath = avDir + hb.avatarRootName + "_Full.asset";
    if (fullAv != null && fullAv.isValid)
    { UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: saving full UnityEngine.Avatar to " + fullAvPath);
      var existingFull = IF_UE.Load<UnityEngine.Avatar>(fullAvPath);
      if (existingFull != null) { UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: deleting existing full avatar"); IF_UE.DeleteAsset(fullAvPath); }
      IF_UE.CreateAsset(fullAv,fullAvPath);
      var loadedFull = IF_UE.Load<UnityEngine.Avatar>(fullAvPath);
      UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: fullAv (loaded)=" + (loadedFull != null ? ("name=" + loadedFull.name + " valid=" + loadedFull.isValid) : "NULL,via in-memory"));
      fullAv = loadedFull ?? fullAv;
      UnityEngine.Debug.Log("[HB] Full UnityEngine.Avatar saved: " + fullAvPath); }
    else
    { UnityEngine.Debug.LogWarning("[HB] Full UnityEngine.Avatar build failed — via reference for both.");
      fullAv = refAvatar; }
    /* Single batch save for both avatars */
    IF_UE.SaveAndRefresh();
    UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: assigning avatars anim.avatar=" + (fullAv != null ? fullAv.name : "NULL") + " hb.animatorAvatar=" + (fullAv != null ? fullAv.name : "NULL"));
    anim.avatar = fullAv; hb.animatorAvatar = fullAv; IF_UE.SetDirty(hb);
    UnityEngine.Debug.Log("[HB] BuildAvatarFromArmature: assigning stripAnim refAvatar=" + (refAvatar != null ? refAvatar.name : "NULL"));
    var stripAnim = armatureStrip.GetComponent<UnityEngine.Animator>();
    if (stripAnim == null) stripAnim = armatureStrip.gameObject.AddComponent<UnityEngine.Animator>();
    stripAnim.avatar = refAvatar;
    var armAnim = armatureArm.GetComponent<UnityEngine.Animator>();
    if (armAnim == null) armAnim = armatureArm.gameObject.AddComponent<UnityEngine.Animator>();
    armAnim.avatar = fullAv;
    UnityEngine.Debug.Log("[HB] Two-avatar assignment: Stripped=" + (refAvatar != null ? refAvatar.name : "null") + " Full=" + (fullAv != null ? fullAv.name : "null"));
    PopulateBoneAssignments(armatureStrip,hbMap,refAvatar,"Armature_Stripped");
    PopulateBoneAssignments(armatureArm,hbMap,fullAv,"Armature_Armature");
    TryExportFbx(hb.NZKC_GO_AviRoot,hb.avatarRootName,armatureArm,armatureStrip,refAvatar,fullAv);
    /* Set ViewPosition from UnityEngine.Animator bone mapping now that UnityEngine.Avatar is valid */
    SetViewPositionFromAnimator(hb); }
    else { UnityEngine.Debug.LogWarning("[HB] Humanoid UnityEngine.Avatar build failed — UnityEngine.Avatar will remain unset. Check armature bone structure and naming."); }
  }
  /** <summary>Validate and repair VRC.SDK3.Avatars.Components.VRCAvatarDescriptor state. Reassigns missing face mesh
   *  by searching UnityEngine.Avatar root children for a UnityEngine.SkinnedMeshRenderer with 'face' in name.
   *  Returns true if VRCAD is fully configured after repair.</summary> */
  public static System.Boolean ValidateVRCAD(C_AviGenerator hb)
  { if (hb == null || hb.NZKC_GO_AviRoot == null) return false;
    var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad == null) return false;
    System.Boolean repaired = false;
    // Repair missing face mesh
    if (vrcad.VisemeSkinnedMesh == null)
    {
    var smrs = hb.NZKC_GO_AviRoot.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>();
    UnityEngine.SkinnedMeshRenderer pick = null;
    foreach (var s in smrs)
    {
      var n = s.name.ToLower();
      if (n.Contains("face")) { pick = s; break; }
      if (pick == null && n.Contains("body")) pick = s;
      if (pick == null && s.sharedMesh != null && s.sharedMesh.blendShapeCount > 0) pick = s; }
    if (pick != null)
    {
      VRCAD.Set.VisemeMesh(vrcad,pick);
      repaired = true;
      UnityEngine.Debug.Log("[HB] ValidateVRCAD: reassigned face mesh " + pick.name); }
    }
    // Repair missing animator avatar
    var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    if (anim != null && anim.avatar == null && hb.animatorAvatar != null && hb.animatorAvatar.isHuman)
    {
    anim.avatar = hb.animatorAvatar;
    IF_UE.SetDirty(anim);
    repaired = true;
    UnityEngine.Debug.Log("[HB] ValidateVRCAD: restored animator UnityEngine.Avatar from hb.animatorAvatar"); }
    // Repair missing expression assets (VRC SDK Control Panel requires them)
    if (vrcad.expressionParameters == null || vrcad.expressionsMenu == null)
    {
    AssignDefaultExpressionAssets(hb);
    repaired = true;
    UnityEngine.Debug.Log("[HB] ValidateVRCAD: assigned default expression menu/params"); }
    return repaired; }
  /** <summary>Set ViewPosition via UnityEngine.Animator.GetBoneTransform for precise eye placement. */
  /** Called after the humanoid UnityEngine.Avatar is built and assigned to the UnityEngine.Animator.</summary> */
  public static void SetViewPositionFromAnimator(C_AviGenerator hb)
  { if (hb.NZKC_GO_AviRoot == null) return;
    var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (anim == null || vrcad == null) return;
    UnityEngine.Transform leftEye = anim.GetBoneTransform(UnityEngine.HumanBodyBones.LeftEye);
    UnityEngine.Transform rightEye = anim.GetBoneTransform(UnityEngine.HumanBodyBones.RightEye);
    if (leftEye != null && rightEye != null)
    { var mid = (leftEye.position + rightEye.position) / 2f;
    var localMid = hb.NZKC_GO_AviRoot.transform.InverseTransformPoint(mid);
    VRCAD.Set.ViewPosition(vrcad,new UnityEngine.Vector3(0f,localMid.y,localMid.z));
    UnityEngine.Debug.Log("[HB] ViewPosition from UnityEngine.Animator bones: (" +
      vrcad.ViewPosition.x.ToString("F4") + "," + vrcad.ViewPosition.y.ToString("F4") +
      "," + vrcad.ViewPosition.z.ToString("F4") + ")"); }
    else if (leftEye != null)
    { var localPos = hb.NZKC_GO_AviRoot.transform.InverseTransformPoint(leftEye.position);
    VRCAD.Set.ViewPosition(vrcad,new UnityEngine.Vector3(0f,localPos.y,localPos.z));
    UnityEngine.Debug.Log("[HB] ViewPosition from single UnityEngine.Animator eye bone: (" +
      vrcad.ViewPosition.x.ToString("F4") + "," + vrcad.ViewPosition.y.ToString("F4") +
      "," + vrcad.ViewPosition.z.ToString("F4") + ")"); }
    else
    { UnityEngine.Debug.Log("[HB] No eye bones in UnityEngine.Animator mapping — keeping default ViewPosition (" +
      vrcad.ViewPosition.y.ToString("F4") + ")."); }
  }
  static void TryExportFbx(UnityEngine.GameObject root,System.String name,UnityEngine.Transform armArm,UnityEngine.Transform armStrip,UnityEngine.Avatar refAv,UnityEngine.Avatar fullAv)
  { var exportType = System.Type.GetType("NZK.Core.NzkFbxExport,Assembly-CSharp-UnityEditor.Editor");
    if (exportType == null)
    exportType = System.Type.GetType("NZK.Core.NzkFbxExport,Assembly-CSharp-UnityEditor.Editor-firstpass");
    if (exportType != null)
    { var method = exportType.GetMethod("ExportArmatures",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    if (method != null)
      method.Invoke(null,new object[] { root,name,armArm,armStrip,refAv,fullAv });
    else
      UnityEngine.Debug.Log("[HB] NzkFbxExport.ExportArmatures method not found — skipping FBX export."); }
    else
    UnityEngine.Debug.Log("[HB] NzkFbxExport type not found — FBX export unavailable."); }
  static void PopulateBoneAssignments(UnityEngine.Transform armatureRoot,UnityEngine.HumanBone[] mapping,UnityEngine.Avatar avatar,System.String armatureType)
  { if (armatureRoot == null) return;
    var slotLookup = new System.Collections.Generic.Dictionary<System.String,System.String>();
    foreach (var hb in mapping) slotLookup[hb.boneName] = hb.humanName;
    foreach (var t in armatureRoot.GetComponentsInChildren<UnityEngine.Transform>())
    { if (t == armatureRoot) continue;
    var ba = C_BoneAssignmentRegistry.GetOrCreate(t.gameObject);
    ba.generatedAvatar = avatar;
    ba.armatureType = armatureType;
    ba.RecordBakeTransform(t);
    if (slotLookup.TryGetValue(t.name,out var humanName))
    { ba.slotName = humanName;
      ba.slot = C_BoneAssignment.HumanNameToSlot(humanName);
      ba.isConfirmed = true; }
    else
    { ba.slot = C_BoneAssignment.E_BoneSlot.Extra;
      ba.slotName = t.name;
      ba.isConfirmed = false; }
    }
    UnityEngine.Debug.Log("[HB] BoneAssignments populated on " + armatureType + " (" +
    armatureRoot.GetComponentsInChildren<UnityEngine.Transform>().Length + " bones)"); }
  /* ================================================================
   *  Armature operations (editor-only)
   * ================================================================ */
  public static void RemoveEndBones(UnityEngine.Transform root)
  { for (int i = root.childCount - 1; i >= 0; i--)
    { var child = root.GetChild(i);
    RemoveEndBones(child);
    var nameLower = child.name.ToLower();
    if (nameLower.EndsWith("_end") || nameLower.EndsWith(".end"))
    { UnityEngine.Object.DestroyImmediate(child.gameObject); } } }
  public static void CloneHierarchy(UnityEngine.Transform src,UnityEngine.Transform dst)
  { foreach (UnityEngine.Transform child in src)
    { var go = new UnityEngine.GameObject(child.name); go.transform.SetParent(dst,false);
    go.transform.SetPositionAndRotation(child.position,child.rotation);
    go.transform.localScale = child.localScale;
    CloneHierarchy(child,go.transform); } }
  public static void ResetHierarchy(UnityEngine.Transform src,UnityEngine.Transform dst)
  { dst.localPosition = src.localPosition; dst.localRotation = src.localRotation; dst.localScale = src.localScale;
    for (int i = 0; i < UnityEngine.Mathf.Min(src.childCount,dst.childCount); i++)
    ResetHierarchy(src.GetChild(i),dst.GetChild(i)); }
  /* ---- Armature baking ---- */
  public static void BakeArmature(C_AviGenerator hb)
  { var ar = hb.armatureRoot;
    UnityEngine.Debug.Log("[HB] BakeArmature: cloneOriginalArmature=" + hb.cloneOriginalArmature + " armatureRoot=" + (ar != null ? ar.name : "null"));
    if (ar == null) foreach (UnityEngine.Transform c in hb.transform)
    { var h = c.GetComponent<C_AviGenerator>(); if (h != null && h.mode == E_AviGeneratorMode.ArmatureBuilder && h.armatureRoot != null) { ar = h.armatureRoot; UnityEngine.Debug.Log("[HB] BakeArmature: found armatureRoot via ArmatureBuilder child=" + c.name + " root=" + ar.name); break; } }
    if (hb.cloneOriginalArmature && ar != null)
    { var srcPos = ar.localPosition;
    var srcRot = ar.localRotation;
    var srcScale = ar.localScale;
    var hbRoot = hb.transform;
    foreach (UnityEngine.Transform c in hb.transform)
    { var h = c.GetComponent<C_AviGenerator>(); if (h != null && h.mode == E_AviGeneratorMode.ArmatureBuilder) { hbRoot = c; break; } }
    /* Parent clones under HB_Generated container instead of root level */
    var hbGen = hb.transform.Find("HB_Generated");
    if (hbGen == null) { var g = new UnityEngine.GameObject("HB_Generated"); g.transform.SetParent(hb.transform,false); hbGen = g.transform; }
    var armatureArmName = "Armature_Armature";
    var existingFull = UnityEngine.GameObject.Find(armatureArmName)?.transform;
    UnityEngine.Transform armatureArm;
    if (existingFull != null)
    { existingFull.SetParent(hbGen,false);
      for (int i = existingFull.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(existingFull.GetChild(i).gameObject);
      armatureArm = existingFull; }
    else { var g = new UnityEngine.GameObject(armatureArmName); g.transform.SetParent(hbGen,false); armatureArm = g.transform; }
    CloneHierarchy(ar,armatureArm);
    armatureArm.localPosition = UnityEngine.Vector3.zero;
    armatureArm.localRotation = UnityEngine.Quaternion.identity;
    armatureArm.localScale = UnityEngine.Vector3.one;
    int fullBoneCount = armatureArm.GetComponentsInChildren<UnityEngine.Transform>().Length;
    var strippedName = "Armature_Stripped";
    var existingStrip = UnityEngine.GameObject.Find(strippedName)?.transform;
    UnityEngine.Transform armatureStrip;
    if (existingStrip != null)
    { existingStrip.SetParent(hbGen,false);
      for (int i = existingStrip.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(existingStrip.GetChild(i).gameObject);
      armatureStrip = existingStrip; }
    else { var g = new UnityEngine.GameObject(strippedName); g.transform.SetParent(hbGen,false); armatureStrip = g.transform; }
    CloneHierarchy(armatureArm,armatureStrip);
    armatureStrip.localPosition = UnityEngine.Vector3.zero;
    armatureStrip.localRotation = UnityEngine.Quaternion.identity;
    armatureStrip.localScale = UnityEngine.Vector3.one;
    if (hb.removeEndBones) RemoveEndBones(armatureStrip);
    foreach (var t in new[] { armatureArm,armatureStrip })
    { var m = t.GetComponent<C_AviGenerator>() ?? t.gameObject.AddComponent<C_AviGenerator>();
      m.mode = E_AviGeneratorMode.OriginalState;
      m.originalArmatureSource = ar;
      m.NZKC_GO_AviRoot = hb.NZKC_GO_AviRoot;
      m.avatarRootName = hb.avatarRootName; }
    ar.localPosition = srcPos;
    ar.localRotation = srcRot;
    ar.localScale = srcScale;
    SyncAvatarArmature(hb,armatureStrip,ar);
    if (hb.armatureRoot == null) hb.armatureRoot = ar;
    UnityEditor.Undo.RegisterCreatedObjectUndo(armatureArm.gameObject,"Clone Armature");
    UnityEditor.Undo.RegisterCreatedObjectUndo(armatureStrip.gameObject,"Clone Armature Stripped");
    UnityEngine.Debug.Log("[HB] Two-armature bake: " + armatureArmName + " (" + fullBoneCount + " bones) + " + strippedName);
    return; }
    UnityEngine.Transform autoHbRoot = hb.transform;
    foreach (UnityEngine.Transform c in hb.transform)
    { var h = c.GetComponent<C_AviGenerator>(); if (h != null && h.mode == E_AviGeneratorMode.ArmatureBuilder) { autoHbRoot = c; break; } }
    if (ar == null)
    {
    /* Find or create Armature_Auto. Use UnityEngine.GameObject.Find to locate orphaned root-level instances from previous bakes. */
    var autoRootGO = UnityEngine.GameObject.Find("Armature_Auto");
    UnityEngine.Transform autoRoot = autoRootGO != null ? autoRootGO.transform : null;
    if (autoRoot == null) { var g = new UnityEngine.GameObject("Armature_Auto"); g.transform.SetParent(null); autoRoot = g.transform; }
    else
    {
      /* Destroy all children and reparent to autoHbRoot for consistent hierarchy */
      for (int i = autoRoot.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(autoRoot.GetChild(i).gameObject);
      autoRoot.SetParent(autoHbRoot,false); }
    ar = autoRoot;
    /* Reset armatureRoot so next bake finds the correct root */
    hb.armatureRoot = ar;
    if (hb.cloneOriginalArmature) UnityEngine.Debug.Log("[HB] cloneOriginalArmature is on but no armatureRoot set — falling through to mesh-source auto-generate."); }
    var entries =  new System.Collections.Generic.List<C_AviGenerator>(); hb.Ac(entries);
    var allBones = new System.Collections.Generic.HashSet<UnityEngine.Transform>();
    foreach (var e in entries) if (e.meshGenSources != null)
    foreach (var src in e.meshGenSources) if (src != null)
    { var smr = src.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr != null) foreach (var b in smr.bones) if (b != null) allBones.Add(b); }
    if (allBones.Count == 0) { UnityEngine.Debug.LogWarning("[HB] No bones found in mesh sources."); return; }
    var sorted = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(allBones, b => BoneDepth(b)));
    foreach (var src in sorted)
    { var p = CreateBoneChain(ar,src);
    if (p.Find(src.name) == null) { var b = new UnityEngine.GameObject(src.name); b.transform.SetParent(p,false);
      b.transform.SetPositionAndRotation(src.position,src.rotation); b.transform.localScale = src.localScale; } }
    UnityEngine.Debug.Log("[HB] Armature_Auto children after build: " + ar.childCount + " first5=" + System.String.Join(",",System.Linq.Enumerable.Select(System.Linq.Enumerable.Take(ar.GetComponentsInChildren<UnityEngine.Transform>(), 6), t => t.name)));
    SyncAvatarArmature(hb,ar,ar);
    if (hb.removeEndBones) RemoveEndBones(ar);
    if (hb.armatureRoot == null) hb.armatureRoot = ar;
    UnityEngine.Debug.Log("[HB] Armature auto-built: " + ar.name + " bones=" + allBones.Count);
    var autoGenExistingStrip = autoHbRoot.Find("Armature_Stripped");
    UnityEngine.Transform autoGenArmatureStrip;
    if (autoGenExistingStrip != null)
    { for (int i = autoGenExistingStrip.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(autoGenExistingStrip.GetChild(i).gameObject);
    autoGenArmatureStrip = autoGenExistingStrip; }
    else { var g = new UnityEngine.GameObject("Armature_Stripped"); g.transform.SetParent(autoHbRoot,false); autoGenArmatureStrip = g.transform; }
    CloneHierarchy(ar,autoGenArmatureStrip);
    if (hb.removeEndBones) RemoveEndBones(autoGenArmatureStrip);
    var autoGenExistingFull = autoHbRoot.Find("Armature_Armature");
    UnityEngine.Transform autoGenArmatureArm;
    if (autoGenExistingFull != null)
    { for (int i = autoGenExistingFull.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(autoGenExistingFull.GetChild(i).gameObject);
    autoGenArmatureArm = autoGenExistingFull; }
    else { var g = new UnityEngine.GameObject("Armature_Armature"); g.transform.SetParent(autoHbRoot,false); autoGenArmatureArm = g.transform; }
    CloneHierarchy(ar,autoGenArmatureArm);
    foreach (var t in new[] { autoGenArmatureArm,autoGenArmatureStrip })
    { var m = t.GetComponent<C_AviGenerator>() ?? t.gameObject.AddComponent<C_AviGenerator>();
    m.mode = E_AviGeneratorMode.OriginalState;
    m.originalArmatureSource = ar;
    m.NZKC_GO_AviRoot = hb.NZKC_GO_AviRoot;
    m.avatarRootName = hb.avatarRootName; }
    UnityEditor.Undo.RegisterCreatedObjectUndo(autoGenArmatureArm.gameObject,"Clone Armature");
    UnityEditor.Undo.RegisterCreatedObjectUndo(autoGenArmatureStrip.gameObject,"Clone Armature Stripped");
    UnityEngine.Debug.Log("[HB] Auto-armature clones created: Armature_Armature + Armature_Stripped"); }
  public static void BakeArmatureFinal(C_AviGenerator hb)
  { var self = hb.GetComponent<C_AviGenerator>();
    if (self != null && self.mode == E_AviGeneratorMode.OriginalState) { UnityEngine.Object.DestroyImmediate(self); }
    foreach (var c in hb.GetComponentsInChildren<C_AviGenerator>())
    if (c.mode == E_AviGeneratorMode.OriginalState) UnityEngine.Object.DestroyImmediate(c);
    UnityEngine.Debug.Log("[HB] OriginalState stripped for VRChat compatibility."); }
  /* ================================================================
   *  Expression / FX layer baking
   * ================================================================ */
  public static void EnsureAvatarOnAnimator(C_AviGenerator hb)
  { if (hb.NZKC_GO_AviRoot == null || hb.animatorAvatar == null) return;
    var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    if (anim != null && anim.avatar == null && hb.animatorAvatar.isHuman) anim.avatar = hb.animatorAvatar; }
  public static void BakeExpressionLayers(C_AviGenerator hb)
  { EnsureAvatarOnAnimator(hb);
    var expr = hb.FindHB(HBChildren.ExpressionsGenerator); if (expr == null) { UnityEngine.Debug.LogWarning("[HB] No Expressions Generator found."); return; }
    var ehb = expr.GetComponent<C_AviGenerator>();
    var controller = GetOrCreateController(hb,"Expressions");
    System.Boolean hasViseme = false;
    foreach (var p in controller.parameters) if (p.name == "Viseme") { hasViseme = true; break; }
    if (!hasViseme) controller.AddParameter("Viseme",UnityEngine.AnimatorControllerParameterType.Int);
    var layer = GetOrCreateLayer(controller,"Visemes",ehb != null ? ehb.mask : null,ehb != null ? ehb.weight : 1f);
    System.String[] visemeBS = null;
    System.String facePath = null;
    if (hb.NZKC_GO_AviRoot != null)
    { var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad != null && vrcad.VisemeBlendShapes != null && vrcad.VisemeBlendShapes.Length >= 15 && vrcad.VisemeSkinnedMesh != null)
    { visemeBS = vrcad.VisemeBlendShapes;
      facePath = UnityEditor.AnimationUtility.CalculateTransformPath(vrcad.VisemeSkinnedMesh.transform,hb.NZKC_GO_AviRoot.transform); } }
    var visemeVrcNames = VisemeNames;
    var defaultState = (UnityEditor.Animations.AnimatorState)null;
    int idx = 0;
    foreach (UnityEngine.Transform child in expr.transform)
    { var ahb = child.GetComponent<C_AviGenerator>();
    if (ahb == null || ahb.mode != E_AviGeneratorMode.Animation) continue;
    var displayName = idx < visemeVrcNames.Length ? visemeVrcNames[idx] : child.name.Replace(HBChildren.VisemePrefix,"Viseme ").Replace(HBChildren.VisemePrefixUpper,"Viseme ");
    var clip = ahb.animationClip;
    if (clip == null)
    { clip = new UnityEngine.AnimationClip(); clip.name = "Viseme_" + displayName;
      if (visemeBS != null && facePath != null && idx < visemeBS.Length && visemeBS[idx] != "-none-")
      {
      /* Set active viseme to 100 */
      var curve = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f,100f),new UnityEngine.Keyframe(0.016f,100f));
      clip.SetCurve(facePath,typeof(UnityEngine.SkinnedMeshRenderer),"blendShape." + visemeBS[idx],curve);
      /* Set all OTHER visemes to 0 so they don't bleed during transitions */
      for (int v = 0; v < visemeBS.Length; v++)
      { if (v == idx) continue;
        if (visemeBS[v] == "-none-") continue;
        var zeroCurve = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f,0f),new UnityEngine.Keyframe(0.016f,0f));
        clip.SetCurve(facePath,typeof(UnityEngine.SkinnedMeshRenderer),"blendShape." + visemeBS[v],zeroCurve); }
      clip.wrapMode = UnityEngine.WrapMode.Clamp; }
      var clipPath = NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Visemes) + "/Viseme_" + displayName + ".anim";
      IF_UE.EnsureDir(clipPath);
      IF_UE.CreateAsset(clip,clipPath);
      ahb.animationClip = clip; }
    var state = AddState(layer.stateMachine,displayName,clip);
    var entryTrans = layer.stateMachine.AddEntryTransition(state);
    entryTrans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals,idx,"Viseme");
    if (idx == 0) defaultState = state;
    idx++; }
    /* Batch-save all created viseme clips at once instead of per-clip */
    IF_UE.SaveAndRefresh();
    if (defaultState != null) layer.stateMachine.defaultState = defaultState;
    UnityEngine.Debug.Log("[HB] Expression layers baked: " + controller.name + " (" + idx + " visemes)");
    /* Sync hierarchy → YamlBlocks */
    hb.BuildYamlBlocks();
    UnityEngine.Debug.Log("[HB] YamlBlocks synced from expression hierarchy.");
    /* Merge sources */
    var exprHB = expr?.GetComponent<C_AviGenerator>();
    var exprMerged = AXController.MergeControllerSources(exprHB,controller,NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Controllers) + "/Expressions.controller");
    if (exprHB != null) { exprHB.generatedController = exprMerged; IF_UE.SetDirty(exprHB); }
    if (hb.NZKC_GO_AviRoot != null)
    { VRCAD.Set.ControllerSlot(hb,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Action,exprMerged);
    UnityEngine.Debug.Log("[HB] Assigned expression controller to VRCAD Action layer."); } }
  public static void BakeFxLayers(C_AviGenerator hb)
  { EnsureAvatarOnAnimator(hb);
    var fx = hb.FindHB(HBChildren.FxGenerator); if (fx == null) { UnityEngine.Debug.LogWarning("[HB] No FX Generator found."); return; }
    var fxb = fx.GetComponent<C_AviGenerator>();
    var controller = GetOrCreateController(hb,"FX");
    foreach (UnityEngine.Transform layer in fx.transform)
    { var lhb = layer.GetComponent<C_AviGenerator>();
    if (lhb == null || (lhb.mode != E_AviGeneratorMode.Expression_Layers
      && lhb.mode != E_AviGeneratorMode.ControllerBuilder)) continue;
    var unityLayer = GetOrCreateLayer(controller,layer.name,lhb.mask,lhb.weight);
    foreach (UnityEngine.Transform anim in layer.transform)
    { var ahb = anim.GetComponent<C_AviGenerator>();
      if (ahb == null || ahb.mode != E_AviGeneratorMode.Animation || ahb.animationClip == null) continue;
      var state = AddState(unityLayer.stateMachine,anim.name.Replace(HBChildren.AnimPrefix,"").Replace(HBChildren.AnimPrefix.ToUpperInvariant(),""),ahb.animationClip); } }
    UnityEngine.Debug.Log("[HB] FX layers baked: " + controller.name);
    /* Sync hierarchy → YamlBlocks */
    hb.BuildYamlBlocks();
    UnityEngine.Debug.Log("[HB] YamlBlocks synced from FX hierarchy.");
    /* Merge sources */
    var fxMerged = AXController.MergeControllerSources(fxb,controller,NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Controllers) + "/FX.controller");
    if (fxb != null) { fxb.generatedController = fxMerged; IF_UE.SetDirty(fxb); }
    VRCAD.Set.ControllerSlot(hb,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX,fxMerged);
    UnityEngine.Debug.Log("[HB] Assigned FX controller to VRCAD FX layer."); }
  /** <summary>Create and assign default VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu + VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters
   *  if missing. Required to clear VRC SDK Control Panel warnings.</summary> */
  static void AssignDefaultExpressionAssets(C_AviGenerator hb)
  { if (hb.NZKC_GO_AviRoot == null) return;
    var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad == null) return;
    var avDir = NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Avatars) + "/";
    // Default expression parameters
    if (vrcad.expressionParameters == null)
    {
    System.String paramsPath = avDir + hb.avatarRootName + "_Params.asset";
    var existing = IF_UE.Load<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(paramsPath);
    if (existing == null)
    {
      var p = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>();
      p.name = hb.avatarRootName + "_Params";
      p.parameters = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter[0];
      IF_UE.CreateAsset(p,paramsPath);
      IF_UE.SaveAndRefresh();
      existing = p; }
    VRCAD.Set.ExpressionParams(vrcad,existing); }
    // Default expression menu
    if (vrcad.expressionsMenu == null)
    {
    System.String menuPath = avDir + hb.avatarRootName + "_Menu.asset";
    var existing = IF_UE.Load<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(menuPath);
    if (existing == null)
    {
      var m = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
      m.name = hb.avatarRootName + "_Menu";
      m.controls = new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>();
      IF_UE.CreateAsset(m,menuPath);
      IF_UE.SaveAndRefresh();
      existing = m; }
    VRCAD.Set.ExpressionsMenu(vrcad,existing); }
  }
  /* ================================================================
   *  Toggle generator
   * ================================================================ */
  public static void BakeToggleGenerator(C_AviGenerator hb)
  { if (hb.blendshapeSources.Count == 0 && hb.nanimationSources.Count == 0 && hb.objectToggleSources.Count == 0 && hb.userAnimationClips.Count == 0
    && hb.toggleGroup == null) { UnityEngine.Debug.LogWarning("[HB] No toggle sources to bake."); return; }
    if (hb.toggleGroup != null)
    { foreach (var tr in hb.toggleGroup.toggleReferences)
    { if (tr == null) continue;
      var btd = tr.GetComponent<BlendshapeToggleDefinition>();
      if (btd != null)
      { var result = BlendShapeToggleGenerator.GenerateToggle(tr,hb.avatarRootName,null);
      if (result != null) UnityEngine.Debug.Log("[HB] Toggle generated: " + result); }
      else UnityEngine.Debug.Log("[HB] No BlendshapeToggleDefinition on " + tr.name); }
    UnityEngine.Debug.Log("[HB] Toggle generator baked (legacy): " + hb.toggleGroup.groupName); return; }
    BakeToggleGeneratorFull(hb); }
  static void BakeToggleGeneratorFull(C_AviGenerator hb)
  { var tc = hb.transform.Find(HBChildren.Prefix + HBChildren.Toggles);
    if (tc == null) { UnityEngine.Debug.LogWarning("[HB] No Toggles container."); return; }
    var a = System.String.IsNullOrEmpty(hb.avatarRootName) ? SanitizeParamName(hb.transform.root.name) : hb.avatarRootName;
    var baseP = NZKPaths.AvatarPath(a,NZKPaths.ToggleGen) + "/";
    var animP = baseP + "Animations/";
    var btP = baseP + "BlendTrees/";
    var ctrlP = baseP + "FX.controller";
    var allParams =  new System.Collections.Generic.List<System.String>();
    var catBts = new System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<System.String>>();
    foreach (UnityEngine.Transform c in tc)
    { var chb = c.GetComponent<C_AviGenerator>();
    if (chb == null || !chb.isToggleChild) continue;
    var cn = c.name;
    var srcs = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(chb.toggleSourceObjects, s => s != null));
    if (srcs.Count == 0)
    { var sn = cn; System.String sfx = null;
      if (cn.EndsWith(SuffixBS)) { sn = cn.Substring(0,cn.Length - 3); sfx = "BS"; }
      else if (cn.EndsWith(SuffixNaN)) { sn = cn.Substring(0,cn.Length - 4); sfx = "NaN"; }
      else if (cn.EndsWith(SuffixOT)) { sn = cn.Substring(0,cn.Length - 3); sfx = "OT"; }
      if (sfx != null)
      { var oldSrc = System.Linq.Enumerable.FirstOrDefault(sfx == "BS" ? (System.Collections.Generic.IEnumerable<UnityEngine.GameObject>)hb.blendshapeSources : sfx == "NaN" ? hb.nanimationSources : hb.objectToggleSources, s => s != null && SanitizeName(s.name) == sn);
      if (oldSrc != null) srcs.Add(oldSrc); } }
    System.String cat;
    UnityEngine.AnimationClip onClip = chb.animationClip,offClip = chb.offClip;
    switch (chb.toggleSourceType)
    { case C_AviGenerator.TToggleSourceType.Blendshape:
      cat = "Blendshape";
      if (srcs.Count > 0 && onClip == null)
      { if (chb.toggleSourceMode == C_AviGenerator.TToggleSourceMode.Linked && !System.String.IsNullOrEmpty(chb.toggleBlendshapeName))
        { var bn = chb.toggleBlendshapeName;
        onClip = new UnityEngine.AnimationClip(); onClip.name = cn + "_on"; onClip.legacy = false;
        offClip = new UnityEngine.AnimationClip(); offClip.name = cn + "_off"; offClip.legacy = false;
        foreach (var src in srcs)
        { var btd = src.GetComponent<BlendshapeToggleDefinition>();
          if (btd == null || btd.targetRenderer == null) continue;
          var p = UnityEditor.AnimationUtility.CalculateTransformPath(btd.targetRenderer.transform,btd.targetRenderer.transform.root);
          onClip.SetCurve(p,typeof(UnityEngine.SkinnedMeshRenderer),"blendShape." + bn,new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,btd.onValue)));
          offClip.SetCurve(p,typeof(UnityEngine.SkinnedMeshRenderer),"blendShape." + bn,new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,btd.offValue))); } }
        else GenBSClips(srcs[0],cn,ref onClip,ref offClip); }
      break;
      case C_AviGenerator.TToggleSourceType.NaNimation:
      cat = "NaNimations";
      if (srcs.Count > 0) GenNaNClips(srcs[0],cn,ref onClip,ref offClip);
      break;
      case C_AviGenerator.TToggleSourceType.ObjectToggle:
      cat = "UnityEngine.Object Toggle";
      if (srcs.Count > 0) GenOTClips(srcs[0],cn,ref onClip,ref offClip);
      break;
      default: cat = GetToggleCategory(cn); break; }
    var param = chb.toggleParameterName;
    if (System.String.IsNullOrEmpty(param)) { param = "(b-gt)" + SanitizeParamName(System.String.IsNullOrEmpty(chb.toggleDisplayName) ? cn : chb.toggleDisplayName); chb.toggleParameterName = param; }
    allParams.Add(param);
    if (!catBts.ContainsKey(cat)) catBts[cat] =  new System.Collections.Generic.List<System.String>();
    if (onClip == null || offClip == null) continue;
    System.IO.Directory.CreateDirectory(animP + cat);
    var onPath = animP + cat + "/" + cn + "_on.anim"; var offPath = animP + cat + "/" + cn + "_off.anim";
    onClip = SvClip(onClip,onPath); offClip = SvClip(offClip,offPath);
    System.IO.Directory.CreateDirectory(btP + cat);
    var dbtPath = btP + cat + "/" + cn + ".asset";
    BinDBT(param,onClip,offClip,dbtPath); catBts[cat].Add(dbtPath); }
    if (allParams.Count == 0) { UnityEngine.Debug.LogWarning("[HB] No toggle children to bake."); return; }
    var cats =  new System.Collections.Generic.List<System.String>();
    foreach (var kv in catBts) { if (kv.Value.Count == 0) continue; var cp = btP + kv.Key + ".asset"; CatDBT(kv.Key,kv.Value.ToArray(),cp); cats.Add(kv.Key); }
    if (cats.Count == 0) { UnityEngine.Debug.LogWarning("[HB] No categories with items to bake."); return; }
    var mp = btP + "Main.asset"; MainDBT(cats.ToArray(),btP,mp);
    var ctrl = GOC(a,ctrlP);
    SyncCtrl(ctrl,mp,allParams.ToArray());
    if (hb.NZKC_GO_AviRoot != null)
    { var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad != null)
    { if (vrcad.baseAnimationLayers == null || vrcad.baseAnimationLayers.Length < 5)
      vrcad.baseAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[5];
      VRCAD.Set.BaseLayerSlot(vrcad,AXController.VRCFX,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX,ctrl);
      /* ── Create expression parameters for toggles ── */
      var exprFolder = System.IO.Path.GetDirectoryName(baseP.TrimEnd('/')) + "/ExpressionParameters";
      Systems.Folder.Ensure(exprFolder);
      var exprPath = exprFolder + "/ToggleParams.asset";
      var paramAsset = NaNimate.Params.GllC(exprPath);
      NaNimate.Sync.Bool(paramAsset,allParams);
      UnityEditor.EditorUtility.SetDirty(paramAsset);
      if (vrcad.expressionParameters == null || UnityEditor.AssetDatabase.GetAssetPath(vrcad.expressionParameters) != exprPath)
        vrcad.expressionParameters = paramAsset;
      /* ── Create menu with toggle controls ── */
      var menuFolder = System.IO.Path.GetDirectoryName(baseP.TrimEnd('/')) + "/Menus";
      Systems.Folder.Ensure(menuFolder);
      var menuPath = menuFolder + "/ToggleMenu.asset";
      var rootMenu = NaNimate.Menu.GllC(menuPath);
      NaNimate.Sync.Menu2(rootMenu,a,allParams);
      UnityEditor.EditorUtility.SetDirty(rootMenu);
      vrcad.expressionsMenu = rootMenu; vrcad.customExpressions = true;
      UnityEditor.EditorUtility.SetDirty(vrcad);
      UnityEngine.Debug.Log("[HB] Assigned toggle controller + params + menu to VRCAD."); } }
    IF_UE.SaveAndRefresh();
    UnityEngine.Debug.Log("[HB] Toggle Generator baked: " + hb.transform.name + " (" + allParams.Count + " toggles)"); }
  /* ── Animation clip helpers ──────────────────────────────────── */
  static UnityEngine.AnimationClip NewClip(System.String name) => new UnityEngine.AnimationClip { name = name,legacy = false };
  static void GenBSClips(UnityEngine.GameObject src,System.String name,ref UnityEngine.AnimationClip on,ref UnityEngine.AnimationClip off)
  { var btd = src.GetComponent<BlendshapeToggleDefinition>();
    if (btd == null || btd.targetRenderer == null) return;
    var smr = btd.targetRenderer; var bn = btd.blendshapeName;
    on = BlendShapeToggleGenerator.GenerateBlendshapeClip(smr,bn,btd.onValue,name + "_on");
    off = BlendShapeToggleGenerator.GenerateBlendshapeClip(smr,bn,btd.offValue,name + "_off"); }
  static void GenNaNClips(UnityEngine.GameObject src,System.String name,ref UnityEngine.AnimationClip on,ref UnityEngine.AnimationClip off)
  { var path = UnityEditor.AnimationUtility.CalculateTransformPath(src.transform,src.transform.root);
    on = NewClip(name + "_on"); off = NewClip(name + "_off");
    var sx = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,src.transform.localScale.x));
    on.SetCurve(path,typeof(UnityEngine.Transform),"m_LocalScale.x",sx);
    on.SetCurve(path,typeof(UnityEngine.Transform),"m_LocalScale.y",new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,src.transform.localScale.y)));
    on.SetCurve(path,typeof(UnityEngine.Transform),"m_LocalScale.z",new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,src.transform.localScale.z)));
    var one = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,1f));
    off.SetCurve(path,typeof(UnityEngine.Transform),"m_LocalScale.x",one);
    off.SetCurve(path,typeof(UnityEngine.Transform),"m_LocalScale.y",one);
    off.SetCurve(path,typeof(UnityEngine.Transform),"m_LocalScale.z",one); }
  static void GenOTClips(UnityEngine.GameObject src,System.String name,ref UnityEngine.AnimationClip on,ref UnityEngine.AnimationClip off)
  { var path = UnityEditor.AnimationUtility.CalculateTransformPath(src.transform,src.transform.root);
    on = NewClip(name + "_on"); off = NewClip(name + "_off");
    on.SetCurve(path,typeof(UnityEngine.GameObject),"m_IsActive",new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,1f)));
    off.SetCurve(path,typeof(UnityEngine.GameObject),"m_IsActive",new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0,0f))); }
  static UnityEngine.AnimationClip SvClip(UnityEngine.AnimationClip clip,System.String path)
  { IF_UE.EnsureDir(path);
    var existing = IF_UE.Load<UnityEngine.AnimationClip>(path);
    if (existing != null) { UnityEditor.EditorUtility.CopySerialized(clip,existing); return existing; }
    IF_UE.CreateAsset(clip,path); return clip; }
  static void BinDBT(System.String param,UnityEngine.AnimationClip on,UnityEngine.AnimationClip off,System.String path)
  { var existing = IF_UE.Load<UnityEditor.Animations.BlendTree>(path);
    if (existing != null) { existing.blendParameter = param; existing.blendParameterY = param;
    existing.children = new UnityEditor.Animations.ChildMotion[] {
      new UnityEditor.Animations.ChildMotion { motion = off,threshold = 0f,timeScale = 1f },new UnityEditor.Animations.ChildMotion { motion = on,threshold = 1f,timeScale = 1f } };
    IF_UE.SetDirty(existing); return; }
    var bt = new UnityEditor.Animations.BlendTree(); bt.name = System.IO.Path.GetFileNameWithoutExtension(path);
    bt.blendParameter = param; bt.blendParameterY = param; bt.blendType = UnityEditor.Animations.BlendTreeType.Simple1D;
    bt.AddChild(off,0f); bt.AddChild(on,1f);
    IF_UE.CreateAsset(bt,path); }
  static void CatDBT(System.String name,System.String[] itemPaths,System.String path)
  { var existing = IF_UE.Load<UnityEditor.Animations.BlendTree>(path);
    if (existing != null)
    { var motions =  new System.Collections.Generic.List<UnityEditor.Animations.ChildMotion>();
    for (int i = 0; i < itemPaths.Length; i++)
    { var bt = IF_UE.Load<UnityEditor.Animations.BlendTree>(itemPaths[i]);
      if (bt != null) motions.Add(new UnityEditor.Animations.ChildMotion { motion = bt,directBlendParameter = "(f)Weight",timeScale = 1f }); }
    existing.children = motions.ToArray(); IF_UE.SetDirty(existing); return; }
    var cat = new UnityEditor.Animations.BlendTree(); cat.name = name;
    cat.blendType = UnityEditor.Animations.BlendTreeType.Direct;
    for (int i = 0; i < itemPaths.Length; i++)
    { var bt = IF_UE.Load<UnityEditor.Animations.BlendTree>(itemPaths[i]);
    if (bt != null) cat.AddChild(bt,1f); }
    var children = cat.children;
    for (int i = 0; i < children.Length; i++)
    children[i].directBlendParameter = "(f)Weight";
    cat.children = children;
    IF_UE.CreateAsset(cat,path); }
  static void MainDBT(System.String[] cats,System.String btP,System.String path)
  { var existing = IF_UE.Load<UnityEditor.Animations.BlendTree>(path);
    if (existing != null)
    { var motions =  new System.Collections.Generic.List<UnityEditor.Animations.ChildMotion>();
    for (int i = 0; i < cats.Length; i++)
    { var bt = IF_UE.Load<UnityEditor.Animations.BlendTree>(btP + cats[i] + ".asset");
      if (bt != null) motions.Add(new UnityEditor.Animations.ChildMotion { motion = bt,directBlendParameter = "(f)Weight",timeScale = 1f }); }
    existing.children = motions.ToArray(); IF_UE.SetDirty(existing); return; }
    var main = new UnityEditor.Animations.BlendTree(); main.name = "Main";
    main.blendType = UnityEditor.Animations.BlendTreeType.Direct;
    for (int i = 0; i < cats.Length; i++)
    { var bt = IF_UE.Load<UnityEditor.Animations.BlendTree>(btP + cats[i] + ".asset");
    if (bt != null) main.AddChild(bt,1f); }
    var kids = main.children;
    for (int i = 0; i < kids.Length; i++) kids[i].directBlendParameter = "(f)Weight";
    main.children = kids;
    IF_UE.CreateAsset(main,path); }
  static UnityEditor.Animations.AnimatorController GOC(System.String a,System.String path)
  { var dir = System.IO.Path.GetDirectoryName(path);
    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
    var existing = IF_UE.Load<UnityEditor.Animations.AnimatorController>(path);
    if (existing != null) return existing;
    var ctrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
    if (ctrl.layers.Length > 0) ctrl.RemoveLayer(0);
    IF_UE.SaveAndRefresh(); return ctrl; }
  static void SyncCtrl(UnityEditor.Animations.AnimatorController ctrl,System.String mainDbtPath,System.String[] allParams)
  { const System.String layerName = "DBT Driver";
    var layers = ctrl.layers;
    int idx = -1; for (int i = 0; i < layers.Length; i++) { if (layers[i].name == layerName) { idx = i; break; } }
    UnityEditor.Animations.AnimatorControllerLayer layer;
    if (idx >= 0) { layer = layers[idx]; }
    else { layer = new UnityEditor.Animations.AnimatorControllerLayer();
    layer.name = layerName; layer.defaultWeight = 1f;
    layer.stateMachine = new UnityEditor.Animations.AnimatorStateMachine();
    ctrl.AddLayer(layer); layers = ctrl.layers; idx = layers.Length - 1; layer = layers[idx]; }
    UnityEditor.Animations.AnimatorState state = null;
    foreach (var s in layer.stateMachine.states) { if (s.state.name == "DBT Driver") { state = s.state; break; } }
    if (state == null) { state = layer.stateMachine.AddState("DBT Driver"); state.writeDefaultValues = false; }
    var mainBT = IF_UE.Load<UnityEditor.Animations.BlendTree>(mainDbtPath);
    if (mainBT != null) state.motion = mainBT;
    foreach (var p in allParams)
    { System.Boolean found = false;
    foreach (var cp in ctrl.parameters) { if (cp.name == p) { found = true; break; } }
    if (!found) ctrl.AddParameter(p,UnityEngine.AnimatorControllerParameterType.Float); } }
  /* ================================================================
   *  Controller / layer / state helpers
   * ================================================================ */
  public static UnityEditor.Animations.AnimatorController GetOrCreateController(C_AviGenerator hb,System.String name)
  { var path = NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Controllers) + "/" + name + ".controller";
    IF_UE.EnsureDir(path);
    var dir = System.IO.Path.GetDirectoryName(path);
    var existing = IF_UE.Load<UnityEditor.Animations.AnimatorController>(path);
    if (existing != null)
    { if (existing.layers.Length == 0)
    { var defLayer = new UnityEditor.Animations.AnimatorControllerLayer
      { name = "Base",stateMachine = new UnityEditor.Animations.AnimatorStateMachine() };
      existing.AddLayer(defLayer);
      IF_UE.SaveAndRefresh(); }
    return existing; }
    var ctrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
    if (ctrl.layers.Length > 0) { var l = ctrl.layers; l[0].name = "Base"; ctrl.layers = l; }
    IF_UE.SaveAndRefresh(); return ctrl; }
  public static UnityEditor.Animations.AnimatorControllerLayer GetOrCreateLayer(UnityEditor.Animations.AnimatorController controller,System.String name,UnityEngine.AvatarMask mask,float weight)
  { var layers = controller.layers;
    for (int i = 0; i < layers.Length; i++)
    if (layers[i].name == name)
    { layers[i].avatarMask = mask; layers[i].defaultWeight = weight;
      if (layers[i].stateMachine == null)
      layers[i].stateMachine = new UnityEditor.Animations.AnimatorStateMachine();
      controller.layers = layers; return controller.layers[i]; }
    var layer = new UnityEditor.Animations.AnimatorControllerLayer
    { name = name,stateMachine = new UnityEditor.Animations.AnimatorStateMachine(),defaultWeight = weight,avatarMask = mask };
    controller.AddLayer(layer);
    var final = controller.layers;
    return final[final.Length - 1]; }
  public static UnityEditor.Animations.AnimatorState AddState(UnityEditor.Animations.AnimatorStateMachine sm,System.String name,UnityEngine.AnimationClip clip)
  { foreach (var s in sm.states) if (s.state.name == name) return s.state;
    var state = sm.AddState(name); state.motion = clip; state.writeDefaultValues = false;
    return state; }
  public static UnityEditor.Animations.AnimatorState FindState(UnityEditor.Animations.AnimatorStateMachine sm,System.String name)
  { foreach (var s in sm.states) if (s.state.name == name) return s.state; return null; }
  public static void AddTransition(UnityEditor.Animations.AnimatorState state,UnityEditor.Animations.AnimatorStateMachine sm,System.String targetState,System.String param,float threshold,System.Boolean isLess)
  { var target = FindState(sm,targetState);
    if (target == null) return;
    var mode = isLess ? UnityEditor.Animations.AnimatorConditionMode.Less : UnityEditor.Animations.AnimatorConditionMode.Greater;
    var trans = state.AddTransition(target);
    trans.AddCondition(mode,threshold,param);
    trans.duration = 0.11f;
    trans.hasExitTime = false;
    trans.hasFixedDuration = true; }
  /* ================================================================
   *  Reset transforms
   * ================================================================ */
  public static void ResetTransforms(C_AviGenerator hb)
  { if (hb.originalArmatureSource == null) { UnityEngine.Debug.LogWarning("[HB] No original armature source."); return; }
    ResetHierarchy(hb.originalArmatureSource,hb.transform);
    UnityEngine.Debug.Log("[HB] Transforms reset from " + hb.originalArmatureSource.name); }
  }
}
}
#endif
