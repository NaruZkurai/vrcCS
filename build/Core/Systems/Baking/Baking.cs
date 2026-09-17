#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static partial class Baking {
  // Bone names matching UnityEngine.HumanBodyBones enum 0-24
  public static readonly System.String[] HumanBoneNames = {
    "Hips","Spine","Chest","UpperChest","Neck","Head","LeftEye","RightEye","Jaw","LeftShoulder","RightShoulder","LeftUpperArm","RightUpperArm","LeftLowerArm","RightLowerArm","LeftHand","RightHand","LeftUpperLeg","RightUpperLeg","LeftLowerLeg","RightLowerLeg","LeftFoot","RightFoot","LeftToes","RightToes"
  };
  // ── find humanoid bone ──────────────────────────────────────────
  // Uses UnityEngine.Animator.GetBoneTransform first (authoritative),then falls
  // back to Armature_Ref (reference skeleton from avatar.asset),then
  // does a recursive search on the whole UnityEngine.Avatar root.
  static UnityEngine.Transform FindHumanBone(UnityEngine.GameObject avatarRoot,int boneIndex)
  { var animator = avatarRoot.GetComponent<UnityEngine.Animator>();
    if (animator != null && animator.isHuman && animator.avatar != null && animator.avatar.isHuman)
    {
    if (boneIndex >= 0 && boneIndex < HumanBoneNames.Length)
    {
      var bone = animator.GetBoneTransform((UnityEngine.HumanBodyBones)boneIndex);
      if (bone != null) return bone; }
    }
    // Fallback: search Armature_Ref (reference skeleton from avatar.asset)
    var refArm = avatarRoot.transform.Find("Armature_Ref");
    if (refArm != null && boneIndex >= 0 && boneIndex < HumanBoneNames.Length)
    {
    var bone = FindBoneRecursive(refArm,HumanBoneNames[boneIndex]);
    if (bone != null) return bone; }
    return null; }
  // ── FindBoneRecursive ────────────────────────────────────────────
  // Recursive name search. Optionally scoped to a specific root.
  static UnityEngine.Transform FindBoneRecursive(UnityEngine.Transform parent,System.String boneName,UnityEngine.Transform scopeRoot = null)
  { if (scopeRoot != null) parent = scopeRoot;
    if (parent.name.Equals(boneName,System.StringComparison.OrdinalIgnoreCase)) return parent;
    for (int i = 0; i < parent.childCount; i++)
    { var r = FindBoneRecursive(parent.GetChild(i),boneName); if (r != null) return r; }
    return null; }
  // ── ReparentToBone ──────────────────────────────────────────────
  public static void ReparentToBone(UnityEngine.GameObject socketObj,UnityEngine.GameObject avatarRoot,int boneIndex,SpsSocketConfig config)
  { var boneTransform = FindHumanBone(avatarRoot,boneIndex);
    if (boneTransform == null)
    {
    // Try Armature_Ref first (reference skeleton),then full UnityEngine.Avatar root
    var refArm = avatarRoot.transform.Find("Armature_Ref");
    boneTransform = refArm != null
      ? FindBoneRecursive(refArm,HumanBoneNames[boneIndex])
      : FindBoneRecursive(avatarRoot.transform,HumanBoneNames[boneIndex]); }
    if (boneTransform == null)
    {
    UnityEngine.Debug.LogWarning("[Baking] Could not find bone " + boneIndex + " on " + avatarRoot.name +
      ". Socket " + socketObj.name + " not reparented.");
    return; }
    if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(socketObj))
    {
    var root = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(socketObj) ?? socketObj;
    UnityEditor.PrefabUtility.UnpackPrefabInstance(root,UnityEditor.PrefabUnpackMode.Completely,UnityEditor.InteractionMode.UserAction); }
    UnityEngine.Vector3 worldPos = socketObj.transform.position;
    UnityEngine.Quaternion worldRot = socketObj.transform.rotation;
    UnityEngine.Vector3 worldScale = socketObj.transform.lossyScale;
    socketObj.transform.SetParent(boneTransform,false);
    socketObj.transform.position = worldPos;
    socketObj.transform.rotation = worldRot;
    UnityEngine.Vector3 pls = boneTransform.lossyScale;
    socketObj.transform.localScale = new UnityEngine.Vector3(
    pls.x != 0 ? worldScale.x / pls.x : 1f,pls.y != 0 ? worldScale.y / pls.y : 1f,pls.z != 0 ? worldScale.z / pls.z : 1f);
    System.String bonePath = UnityEditor.AnimationUtility.CalculateTransformPath(boneTransform,avatarRoot.transform);
    if (config != null) config.boneLinkPath = bonePath;
    UnityEngine.Debug.Log("[Baking] Reparented " + socketObj.name + " to " + bonePath + " (bone " + boneIndex + ")"); }
  // ── BakeArmatureLink ────────────────────────────────────────────
  // Two-phase process:
  //   Phase A — Create HB_ArmatureLinks hierarchy entries for each
  //       VRCFury ArmatureLink feature.
  //   Phase B — Reparent each entry's targetObject to its destination.
  // After both phases,destroy all processed VRCFury components.
  public static void BakeArmatureLink(UnityEngine.GameObject target,System.Boolean includeChildren = true)
  { if (target == null) { UnityEngine.Debug.LogWarning("[Baking] Target is null."); return; }
    var avatarName = Vars.Names.Sanitize(target.transform.root.name);
    var avatarRoot = FindAviRoot(avatarName,target) ?? target.transform.root.gameObject;
    // Phase A — Create hierarchy entries
    var (entries,processedVrcfury) = CreateArmatureLinkHierarchy(target,avatarName,avatarRoot,includeChildren);
    if (entries.Count == 0)
    {
    UnityEngine.Debug.Log("[Baking] No VRCFury ArmatureLink found on " + target.name);
    return; }
    // Phase B — Bake each entry (reparent)
    int baked = BakeArmatureLinkEntries(entries,avatarRoot);
    // Cleanup — destroy only VRCFury components that had ArmatureLink features
    foreach (var vrc in processedVrcfury)
    {
    if (vrc != null)
    {
      UnityEditor.Undo.RecordObject(vrc,"Remove VRCFury");
      UnityEngine.Object.DestroyImmediate(vrc); }
    }
    UnityEngine.Debug.Log("[Baking] Baked " + baked + " ArmatureLink(s)."); }
  // ── CreateArmatureLinkHierarchy ─────────────────────────────────
  // Phase A: Scans the target for VRCFury ArmatureLink features,// creates GEN_{name} → HB_ArmatureLinks → {propBone} children,// attaches C_ArmatureLinkLog to each child with destination info.
  // Returns the list of C_ArmatureLinkLog entries created.
  static (System.Collections.Generic.List<C_ArmatureLinkLog> logs,System.Collections.Generic.List<UnityEngine.MonoBehaviour> comps) CreateArmatureLinkHierarchy(
    UnityEngine.GameObject target,System.String avatarName,UnityEngine.GameObject avatarRoot,System.Boolean includeChildren)
  { var results = new System.Collections.Generic.List<C_ArmatureLinkLog>();
    var processedComps =  new System.Collections.Generic.List<UnityEngine.MonoBehaviour>();
    // Ensure GEN root exists
    var genRoot = GetGenRoot(avatarName,avatarRoot);
    // Ensure HB_ArmatureLinks child exists with proper mode
    var alFolder = genRoot.Find("HB_ArmatureLinks");
    if (alFolder == null)
    {
    var g = new UnityEngine.GameObject("HB_ArmatureLinks");
    g.transform.SetParent(genRoot,false);
    var hb = g.AddComponent<C_AviGenerator>();
    hb.mode = E_AviGeneratorMode.ArmatureLinks;
    hb.NZKC_GO_AviRoot = avatarRoot;
    hb.avatarRootName = avatarName;
    alFolder = g.transform; }
    // Collect VRCFury components
    var allComponents = includeChildren
    ? target.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true)
    : target.GetComponents<UnityEngine.MonoBehaviour>();
    var countAll = target.GetComponents<UnityEngine.MonoBehaviour>().Length;
    var countAllChildren = target.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Length;
    UnityEngine.Debug.Log("[Baking] [AL-Hierarchy] Searching includeChildren=" + includeChildren + " on " + target.name + ". GetComponents<UnityEngine.MonoBehaviour> count = " + countAll + ",GetComponentsInChildren<UnityEngine.MonoBehaviour> count = " + countAllChildren);
    foreach (var comp in allComponents)
    {
    if (comp == null) continue;
    if (comp.GetType().FullName != "VF.Model.VRCFury")
    {
      UnityEngine.Debug.Log("[Baking] [AL-Hierarchy]   Skipping " + comp.GetType().FullName + " on " + comp.name + " \u2014 not VF.Model.VRCFury");
      continue; }
    var features = GetFeatures(comp);
    if (features == null)
    {
      UnityEngine.Debug.Log("[Baking] [AL-Hierarchy]   VRCFury found on " + comp.name + " but GetFeatures returned null");
      continue; }
    System.Boolean hasArmatureLink = false;
    foreach (var f in features)
    {
      if (f == null) continue;
      var ft = f.GetType();
      UnityEngine.Debug.Log("[Baking] [AL-Hierarchy]   Feature: " + ft.Name + " (FullName=" + ft.FullName + ")");
      if (ft.Name != "ArmatureLink" && !(ft.FullName ?? "").Contains("ArmatureLink")) continue;
      hasArmatureLink = true;
      UnityEngine.GameObject propBone = comp.gameObject;
      try
      {
      var pbField = ft.GetField("propBone",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (pbField != null)
      {
        var pb = pbField.GetValue(f) as UnityEngine.GameObject;
        if (pb != null) propBone = pb; }
      }
      catch (System.Exception _ex) { UnityEngine.Debug.LogError("[Baking] Failed to read propBone field: " + _ex.Message); }
      try
      {
      var ltField = ft.GetField("linkTo",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (ltField == null) continue;
      var linkTo = ltField.GetValue(f) as System.Collections.IList;
      if (linkTo == null) continue;
      foreach (var link in linkTo)
      {
        if (link == null) continue;
        var linkType = link.GetType();
        var useObjField = linkType.GetField("useObj",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Boolean useObj = useObjField != null && (System.Boolean)useObjField.GetValue(link);
        var useBoneField = linkType.GetField("useBone",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Boolean useBone = useBoneField != null && (System.Boolean)useBoneField.GetValue(link);
        // Create hierarchy entry
        System.String alName = Vars.Names.Sanitize(propBone.name);
        var alEntry = alFolder.Find(alName);
        UnityEngine.GameObject alGo;
        if (alEntry == null)
        {
        alGo = new UnityEngine.GameObject(alName);
        alGo.transform.SetParent(alFolder,false); }
        else alGo = alEntry.gameObject;
        var alLog = C_ArmatureLinkLogRegistry.GetOrCreate(alGo);
        alLog.targetObject = propBone;
        if (useObj)
        {
        alLog.destinationMode = C_ArmatureLinkLog.TDestinationMode.Object;
        var objField = linkType.GetField("obj",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var obj = objField?.GetValue(link) as UnityEngine.GameObject;
        if (obj != null) alLog.targetParent = obj.transform;
        UnityEngine.Debug.Log($"[Baking]   [Hierarchy] {alName} → UnityEngine.Object: {(obj != null ? obj.name : "null")}"); }
        else if (useBone)
        {
        var bf = linkType.GetField("bone",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (bf != null)
        {
          int boneIdx = (int)bf.GetValue(link);
          alLog.destinationMode = C_ArmatureLinkLog.TDestinationMode.AvatarDefinition;
          alLog.targetBoneIndex = boneIdx;
          UnityEngine.Debug.Log($"[Baking]   [Hierarchy] {alName} → AvatarDefinition: bone={boneIdx}({HumanBoneNames[boneIdx]})"); }
        }
        else
        {
        alLog.destinationMode = C_ArmatureLinkLog.TDestinationMode.AviRoot;
        UnityEngine.Debug.Log($"[Baking]   [Hierarchy] {alName} → AviRoot"); }
        results.Add(alLog); }
      }
      catch (System.Exception _ex) { UnityEngine.Debug.LogError("[NZK] Unhandled exception: " + _ex.Message); }
    }
    if (hasArmatureLink) processedComps.Add(comp); }
    UnityEngine.Debug.Log("[Baking] [AL-Hierarchy]   Returning " + results.Count + " ArmatureLink entries.");
    return (results,processedComps); }
  // ── BakeArmatureLinkEntries ────────────────────────────────────
  // Phase B: Iterates the hierarchy entries,resolves each destination,// unpacks prefabs,and reparents targetObject to the resolved parent.
  public static int BakeArmatureLinkEntries(System.Collections.Generic.List<C_ArmatureLinkLog> entries,UnityEngine.GameObject avatarRoot)
  { int baked = 0;
    foreach (var alLog in entries)
    {
    if (alLog == null) continue;
    var propBone = alLog.targetObject;
    if (propBone == null) continue;
    // Resolve target parent based on mode
    UnityEngine.Transform targetParent = null;
    switch (alLog.destinationMode)
    {
      case C_ArmatureLinkLog.TDestinationMode.Object:
      targetParent = alLog.targetParent;
      UnityEngine.Debug.Log($"[Baking]   [Bake] {propBone.name} → UnityEngine.Object mode: {(targetParent != null ? targetParent.name : "null")}");
      break;
      case C_ArmatureLinkLog.TDestinationMode.AvatarDefinition:
      if (alLog.targetBoneIndex >= 0 && alLog.targetBoneIndex < HumanBoneNames.Length)
      {
        targetParent = FindBoneOnAvatar(avatarRoot,alLog.targetBoneIndex);
        UnityEngine.Debug.Log($"[Baking]   [Bake] {propBone.name} → AvatarDefinition mode: bone={alLog.targetBoneIndex}({HumanBoneNames[alLog.targetBoneIndex]}) = {(targetParent != null ? targetParent.name : "null")}"); }
      break;
      case C_ArmatureLinkLog.TDestinationMode.Path:
      if (!System.String.IsNullOrEmpty(alLog.targetPath))
      {
        targetParent = avatarRoot.transform.Find(alLog.targetPath);
        UnityEngine.Debug.Log($"[Baking]   [Bake] {propBone.name} → System.IO.Path mode: path={alLog.targetPath} = {(targetParent != null ? targetParent.name : "null")}"); }
      break;
      case C_ArmatureLinkLog.TDestinationMode.AviRoot:
      targetParent = avatarRoot.transform;
      UnityEngine.Debug.Log($"[Baking]   [Bake] {propBone.name} → AviRoot");
      break; }
    if (targetParent == null)
    {
      UnityEngine.Debug.LogWarning($"[Baking]   [Bake] {propBone.name}: targetParent is null,skipping.");
      continue; }
    // Skip cycles
    if (targetParent.IsChildOf(propBone.transform))
    {
      UnityEngine.Debug.LogWarning($"[Baking]   [Bake] {propBone.name}: target is a child of propBone (cycle),skipping.");
      continue; }
    // Save world transform
    UnityEngine.Vector3 wp = propBone.transform.position;
    UnityEngine.Quaternion wr = propBone.transform.rotation;
    UnityEngine.Vector3 ws = propBone.transform.lossyScale;
    // Unpack prefab — lowest level possible
    if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(propBone))
    {
      var prefabRoot = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(propBone) ?? propBone;
      UnityEditor.PrefabUtility.UnpackPrefabInstance(prefabRoot,UnityEditor.PrefabUnpackMode.Completely,UnityEditor.InteractionMode.UserAction); }
    // Reparent
    propBone.transform.SetParent(targetParent,false);
    propBone.transform.position = wp;
    propBone.transform.rotation = wr;
    UnityEngine.Vector3 pls = targetParent.lossyScale;
    propBone.transform.localScale = new UnityEngine.Vector3(
      pls.x != 0 ? ws.x / pls.x : 1f,pls.y != 0 ? ws.y / pls.y : 1f,pls.z != 0 ? ws.z / pls.z : 1f);
    // Update log with resolved path
    alLog.targetPath = UnityEditor.AnimationUtility.CalculateTransformPath(targetParent,avatarRoot.transform);
    baked++; }
    return baked; }
  // ── FindBoneOnAvatar ────────────────────────────────────────────
  static UnityEngine.Transform FindBoneOnAvatar(UnityEngine.GameObject avatarRoot,int boneIndex)
  { var animator = avatarRoot.GetComponent<UnityEngine.Animator>();
    if (animator != null && animator.isHuman && animator.avatar != null && animator.avatar.isHuman)
    {
    if (boneIndex >= 0 && boneIndex < HumanBoneNames.Length)
    {
      var bone = animator.GetBoneTransform((UnityEngine.HumanBodyBones)boneIndex);
      if (bone != null) return bone; }
    }
    // Fallback: search Armature_Ref first (reference skeleton from avatar.asset)
    System.String boneName = boneIndex >= 0 && boneIndex < HumanBoneNames.Length ? HumanBoneNames[boneIndex] : null;
    if (boneName == null) return null;
    var refArm = avatarRoot.transform.Find("Armature_Ref");
    if (refArm != null)
    {
    var bone = FindBoneRecursive(refArm,boneName);
    if (bone != null) return bone; }
    // Final fallback: recursive name search on entire UnityEngine.Avatar root
    return FindBoneRecursive(avatarRoot.transform,boneName); }
  // ── BakeToggleFromVrcfury ───────────────────────────────────────
  // Detects VRCFury Toggle features,creates a source in HB_Sources with
  // generated animation clips,creates a prebaked toggle referencing it,// stores full menu path,then triggers menu bake.
  public static void BakeToggleFromVrcfury(UnityEngine.GameObject target)
  { if (target == null) return;
    var avatarRootTr = target.transform.root;
    System.String avatarName = Vars.Names.Sanitize(avatarRootTr.name);
    var avatarObj = FindAviRoot(avatarName,target);
    var genRoot = GetGenRoot(avatarName,avatarObj);
    var tgTr = genRoot.Find("HB_ToggleGenerator");
    C_AviGenerator tgBaker;
    if (tgTr == null)
    {
    var tgGo = new UnityEngine.GameObject("HB_ToggleGenerator");
    tgGo.transform.SetParent(genRoot,false);
    tgBaker = tgGo.AddComponent<C_AviGenerator>();
    tgBaker.mode = E_AviGeneratorMode.ToggleGenerator;
    UnityEditor.Undo.RegisterCreatedObjectUndo(tgGo,"Create Toggle Generator"); }
    else tgBaker = tgTr.GetComponent<C_AviGenerator>();
    if (tgBaker == null) return;
    // Ensure child containers exist
    tgBaker.CreateToggleChildren();
    var srcC = tgBaker.transform.Find("HB_Sources");
    var tgC = tgBaker.transform.Find("HB_Toggles");
    if (srcC == null || tgC == null) return;
    // Ensure Prebaked Toggles/Settings hierarchy
    var prebaked = tgC.Find("Prebaked Toggles");
    if (prebaked == null) { var g = new UnityEngine.GameObject("Prebaked Toggles"); g.transform.SetParent(tgC,false); prebaked = g.transform; }
    var settings = prebaked.Find("Settings");
    if (settings == null) { var g = new UnityEngine.GameObject("Settings"); g.transform.SetParent(prebaked,false); settings = g.transform; }
    // Find VRCFury components with Toggle features
    var vrcfuryComponents = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(target.GetComponents<UnityEngine.MonoBehaviour>(), c => c != null && c.GetType().FullName == "VF.Model.VRCFury"));
    if (vrcfuryComponents.Count == 0)
    vrcfuryComponents = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(target.GetComponents<UnityEngine.MonoBehaviour>(), c => c != null && (c.GetType().Name.Contains("VRCFury") || c.GetType().Name.Contains("Fury"))));
    if (vrcfuryComponents.Count == 0)
    { UnityEngine.Debug.Log("[Baking] No VRCFury component on " + target.name); return; }
    int togglesFound = 0;
    System.String outputBase = "Assets/!_NZK_Generated/" + avatarName + "/ToggleGenerator/";
    Systems.Folder.Ensure(outputBase + "Animations/");
    Systems.Folder.Ensure(outputBase + "Sources/");
    foreach (var furyComp in vrcfuryComponents)
    {
    var t = furyComp.GetType();
    try
    {
      var getAll = t.GetMethod("GetAllFeatures",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      System.Collections.IList features = null;
      if (getAll != null) features = getAll.Invoke(furyComp,null) as System.Collections.IList;
      if (features == null) { var cf = t.GetField("content",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (cf != null) { var v = cf.GetValue(furyComp); if (v != null) features =  new System.Collections.Generic.List<object> { v }; } }
      if (features == null) continue;
      foreach (var feature in features)
      { if (feature == null) continue;
      var ft = feature.GetType();
      if (ft.Name != "Toggle" && !(ft.FullName ?? "").Contains("Toggle")) continue;
      // Extract toggle data
      var nf = ft.GetField("name",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      System.String toggleName = nf?.GetValue(feature) as System.String ?? target.name;
      System.String sanitized = SanitizeName(toggleName);
      System.String sourceName = sanitized + "_Src";
      System.String entryName = sanitized + "_VT";
      if (settings.Find(entryName) != null) continue;
      // Generate animation clips on the spot
      System.String objPath = UnityEditor.AnimationUtility.CalculateTransformPath(target.transform,avatarRootTr);
      var onClip = new UnityEngine.AnimationClip { name = toggleName + "_On" };
      var offClip = new UnityEngine.AnimationClip { name = toggleName + "_Off" };
      UnityEditor.AnimationUtility.SetEditorCurve(onClip,UnityEditor.EditorCurveBinding.FloatCurve(objPath,typeof(UnityEngine.GameObject),"m_IsActive"),UnityEngine.AnimationCurve.Constant(0f,0f,1f)); // on = active
      UnityEditor.AnimationUtility.SetEditorCurve(offClip,UnityEditor.EditorCurveBinding.FloatCurve(objPath,typeof(UnityEngine.GameObject),"m_IsActive"),UnityEngine.AnimationCurve.Constant(0f,0f,0f)); // off = inactive
      // Save clips to disk
      System.String onPath = outputBase + "Animations/" + sourceName + "_On.anim";
      System.String offPath = outputBase + "Animations/" + sourceName + "_Off.anim";
      var existingOn = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(onPath);
      if (existingOn != null) { UnityEditor.EditorUtility.CopySerialized(onClip,existingOn); onClip = existingOn; }
      else UnityEditor.AssetDatabase.CreateAsset(onClip,onPath);
      var existingOff = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(offPath);
      if (existingOff != null) { UnityEditor.EditorUtility.CopySerialized(offClip,existingOff); offClip = existingOff; }
      else UnityEditor.AssetDatabase.CreateAsset(offClip,offPath);
      UnityEditor.AssetDatabase.SaveAssets();
      // Create source object in HB_Sources with clips attached
      var srcTr = srcC.Find(sourceName);
      UnityEngine.GameObject srcGo;
      C_AviGenerator srcHb;
      if (srcTr == null)
      {
        srcGo = new UnityEngine.GameObject(sourceName);
        srcGo.transform.SetParent(srcC,false);
        srcHb = srcGo.AddComponent<C_AviGenerator>();
        srcHb.mode = E_AviGeneratorMode.Animation;
        UnityEditor.Undo.RegisterCreatedObjectUndo(srcGo,"Create Toggle Source"); }
      else { srcGo = srcTr.gameObject; srcHb = srcGo.GetComponent<C_AviGenerator>(); }
      if (srcHb != null)
      {
        srcHb.animationClip = onClip;
        srcHb.offClip = offClip;
        srcHb.toggleParameterName = "(b-gt)" + sanitized;
        srcHb.toggleDisplayName = toggleName; }
      // Create prebaked toggle referencing the source
      var toggleGo = new UnityEngine.GameObject(entryName);
      toggleGo.transform.SetParent(settings,false);
      var toggleHb = toggleGo.AddComponent<C_AviGenerator>();
      toggleHb.mode = E_AviGeneratorMode.Animation;
      toggleHb.isToggleChild = true;
      toggleHb.toggleSourceType = C_AviGenerator.TToggleSourceType.ObjectToggle;
      toggleHb.toggleSourceObjects =  new System.Collections.Generic.List<UnityEngine.GameObject> { target };
      toggleHb.toggleParameterName = "(b-gt)" + sanitized;
      toggleHb.toggleDisplayName = toggleName;
      toggleHb.animationClip = onClip;
      toggleHb.offClip = offClip;
      toggleHb.avatarRootName = avatarName;
      // Store full menu path as the toggle's name field
      toggleHb.toggleDisplayName = toggleName; // full VRCFury path like "SPS/Options/Sound FX"
      UnityEditor.Undo.RegisterCreatedObjectUndo(toggleGo,"Create Prebaked Toggle");
      togglesFound++; }
    }
    catch (System.Exception _ex) { UnityEngine.Debug.LogError("[NZK] Unhandled exception: " + _ex.Message); }
    }
    // Destroy all VRCFury components after baking
    DestroyAllVrcfuryComponents(target);
    if (togglesFound > 0)
    {
    UnityEngine.Debug.Log("[Baking] Baked " + togglesFound + " VRCFury toggle(s) with generated anims.");
    // Trigger menu bake after toggle bake
    BakeMenuFromToggles(avatarRootTr.gameObject,avatarName); }
    else
    UnityEngine.Debug.Log("[Baking] No Toggle features found on " + target.name); }
  // ── BakeMenuFromToggles ──────────────────────────────────────────
  // Builds a VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu hierarchy from toggles in the Toggle Generator.
  // Root menu → up to 8 items per page. Each toggle gets its own slot.
  // Empty slots are omitted (sparse array).
  public static void BakeMenuFromToggles(UnityEngine.GameObject avatarRoot,System.String avatarName)
  { var avatarObj = FindAviRoot(avatarName,avatarRoot);
    var genRoot = GetGenRoot(avatarName,avatarObj);
    var tgTr = genRoot.Find("HB_ToggleGenerator");
    if (tgTr == null) return;
    var tgBaker = tgTr.GetComponent<C_AviGenerator>();
    if (tgBaker == null) return;
    tgBaker.CreateToggleChildren();
    // Collect all prebaked toggles
    var allToggles =  new System.Collections.Generic.List<(System.String path,System.String param,System.String name)>();
    var settingsTr = tgTr.Find("HB_Toggles/Prebaked Toggles/Settings");
    if (settingsTr != null)
    {
    foreach (UnityEngine.Transform c in settingsTr)
    {
      var hb = c.GetComponent<C_AviGenerator>();
      if (hb == null || !hb.isToggleChild) continue;
      System.String display = !System.String.IsNullOrEmpty(hb.toggleDisplayName) ? hb.toggleDisplayName : c.name;
      allToggles.Add((display,hb.toggleParameterName,c.name)); }
    }
    if (allToggles.Count == 0) { UnityEngine.Debug.Log("[MenuBaker] No toggles to bake into menu."); return; }
    System.String menuDir = "Assets/!_NZK_Generated/" + avatarName + "/Menus/";
    Systems.Folder.Ensure(menuDir);
    const int slotsPerPage = 8;
    int pages = (allToggles.Count + slotsPerPage - 1) / slotsPerPage;
    if (pages == 0) pages = 1;
    // Root menu: first page or submenu reference
    System.String rootPath = menuDir + avatarName + "_ToggleMenu.asset";
    var rootMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(rootPath);
    if (rootMenu == null)
    {
    rootMenu = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
    rootMenu.name = avatarName + " Toggle Menu";
    rootMenu.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>();
    UnityEditor.AssetDatabase.CreateAsset(rootMenu,rootPath); }
    rootMenu.controls.Clear();
    for (int p = 0; p < pages; p++)
    {
    int start = p * slotsPerPage;
    int count = UnityEngine.Mathf.Min(slotsPerPage,allToggles.Count - start);
    VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu pageMenu;
    if (pages == 1)
    {
      pageMenu = rootMenu; }
    else
    {
      System.String pagePath = menuDir + avatarName + "_ToggleMenu_Page" + (p + 1) + ".asset";
      pageMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(pagePath);
      if (pageMenu == null)
      {
      pageMenu = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
      pageMenu.name = avatarName + " Toggle Menu Page " + (p + 1);
      pageMenu.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>();
      UnityEditor.AssetDatabase.CreateAsset(pageMenu,pagePath); }
      pageMenu.controls.Clear();
      if (p == 0)
      rootMenu.controls.Add(MakeSubMenu("Toggle Menu",pageMenu));
      else
      {
      // Link from root to this page
      rootMenu.controls.Add(MakeSubMenu("Page " + (p + 1),pageMenu)); }
    }
    for (int i = 0; i < count; i++)
    {
      var (path,param,_) = allToggles[start + i];
      pageMenu.controls.Add(MakeToggleCtrl(path,param)); }
    }
    UnityEditor.EditorUtility.SetDirty(rootMenu);
    UnityEditor.AssetDatabase.SaveAssets();
    UnityEngine.Debug.Log("[MenuBaker] Baked " + allToggles.Count + " toggles into " + pages + " menu page(s)."); }
  static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control MakeToggleCtrl(System.String name,System.String param)
  { return new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
    {
    name = name,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle,parameter = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter { name = param },icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1
    }; }
  static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control MakeSubMenu(System.String name,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu sub)
  { return new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
    {
    name = name,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu,subMenu = sub,icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1
    }; }
  static System.String SanitizeName(System.String n)
  { var invalids = System.IO.Path.GetInvalidFileNameChars();
    return new System.String(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(n, c => !System.Linq.Enumerable.Contains(invalids, c) && c != ' '))); }
  // ── DestroyVrcfuryFeatureType ────────────────────────────────────
  // Destroys VRCFury components that contain ONLY the specified feature type.
  // If a component has the feature AND other features,it's kept (for separate baking).
  // featureName: "ArmatureLink","Toggle","FullController",etc.
  public static void DestroyVrcfuryFeatureType(UnityEngine.GameObject target,System.String featureName)
  { if (target == null) return;
    foreach (var comp in target.GetComponents<UnityEngine.MonoBehaviour>())
    {
    if (comp == null) continue;
    var t = comp.GetType();
    if (t.FullName != "VF.Model.VRCFury") continue;
    try
    {
      var features = GetFeatures(comp);
      if (features == null) continue;
      System.Boolean hasTarget = false;
      System.Boolean hasOther = false;
      foreach (var f in features)
      {
      if (f == null) continue;
      var fn = f.GetType().Name;
      if (fn == featureName || (f.GetType().FullName ?? "").Contains(featureName))
        hasTarget = true;
      else
        hasOther = true; }
      if (hasTarget && !hasOther)
      {
      UnityEditor.Undo.DestroyObjectImmediate(comp);
      UnityEngine.Debug.Log("[Baking] Removed VRCFury component (only had " + featureName + ")."); }
      else if (hasTarget && hasOther)
      {
      UnityEngine.Debug.Log("[Baking] Kept VRCFury component — has other features besides " + featureName + "."); }
    }
    catch (System.Exception _ex) { UnityEngine.Debug.LogError("[NZK] Unhandled exception: " + _ex.Message); }
    }
  }
  // ── GetFeatures helper ──────────────────────────────────────────
  public static System.Collections.IList GetFeatures(UnityEngine.MonoBehaviour comp)
  { var t = comp.GetType();
    var getAll = t.GetMethod("GetAllFeatures",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    if (getAll != null)
    {
    var features = getAll.Invoke(comp,null) as System.Collections.IList;
    if (features != null) return features; }
    var cf = t.GetField("content",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    if (cf != null) { var v = cf.GetValue(comp); if (v != null) return  new System.Collections.Generic.List<System.Object> { v }; }
    return null; }
  // ── BakeAllVf ───────────────────────────────────────────────────
  // Bakes ALL VRCFury features on the selected object,then removes all VF.* components.
  public static void BakeAllVf(UnityEngine.GameObject target)
  { if (target == null) return;
    UnityEngine.Debug.Log("[Baking] ===== BakeAllVf starting on " + target.name + " =====");
    UnityEngine.Debug.Log("[Baking] Scanning " + target.GetComponentsInChildren<UnityEngine.Transform>(true).Length + " transforms for VF features...");
    // Collect all unique feature type names present (VF + NZK-native)
    var featureTypes = new System.Collections.Generic.HashSet<System.String>();
    int vfComponentCount = 0;
    foreach (var comp in target.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true))
    {
    if (comp == null) continue;
    var typeName = comp.GetType().Name;
    var typeFullName = comp.GetType().FullName ?? "";
    /* Detect by type name (works when VF types are resolvable) */
    if (typeName == "ArmatureLink") { featureTypes.Add("ArmatureLink"); vfComponentCount++; UnityEngine.Debug.Log("[Baking]   Found ArmatureLink (type): " + comp.name); }
    else if (typeName == "Toggle" || typeName == "ToggleBuilder") { featureTypes.Add("Toggle"); vfComponentCount++; }
    else if (typeName == "FullController" || typeName == "BakedControllerSlot") { featureTypes.Add("FullController"); vfComponentCount++; UnityEngine.Debug.Log("[Baking]   Found FullController (type): " + comp.name); }
    else if (typeName.Contains("HapticSocket") || typeName.Contains("HapticPlug") || typeName == "C_NzkSpsSocket") { featureTypes.Add("SPS"); vfComponentCount++; }
    /* Detect by VF namespace prefix (works when VF assemblies are loaded) */
    if (typeFullName.StartsWith("VF.")) vfComponentCount++;
    /* Also check VRCFury's GetFeatures for VF-specific components */
    var features = GetFeatures(comp);
    if (features != null)
    {
      foreach (var f in features)
      {
      if (f == null) continue;
      var fn = f.GetType().Name;
      UnityEngine.Debug.Log("[Baking]   GetFeatures found: " + fn);
      if (fn == "ArmatureLink") { featureTypes.Add("ArmatureLink"); vfComponentCount++; }
      else if (fn == "Toggle") { featureTypes.Add("Toggle"); vfComponentCount++; }
      else if (fn == "FullController") { featureTypes.Add("FullController"); vfComponentCount++; }
      else if (fn.Contains("HapticSocket") || fn.Contains("HapticPlug")) { featureTypes.Add("SPS"); vfComponentCount++; }
      }
    }
    }
    UnityEngine.Debug.Log("[Baking] System.Type-based detection found " + vfComponentCount + " VF components,feature types: " +
    System.String.Join(",",featureTypes));
    // If type-based detection found nothing,try YAML-based detection on the prefab asset
    if (vfComponentCount == 0)
    {
    var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(target);
    if (prefab != null)
    {
      UnityEngine.Debug.Log("[Baking] System.Type detection found 0 — trying YAML-based detection on prefab: " +
      UnityEditor.AssetDatabase.GetAssetPath(prefab));
      var yamlFeatures = ReadVfFeaturesFromPrefabYaml(prefab);
      foreach (var f in yamlFeatures)
      {
      featureTypes.Add(f);
      UnityEngine.Debug.Log("[Baking]   YAML scan found feature: " + f); }
    }
    }
    // Run bakes for each detected type
    if (featureTypes.Contains("FullController")){ UnityEngine.Debug.Log("[Baking] → Running FullController bake..."); BakeFullController(target); }
    if (featureTypes.Contains("SPS")) { UnityEngine.Debug.Log("[Baking] → Running SPS bake..."); SpsSocketBaker.Bake(target); }
    if (featureTypes.Contains("ArmatureLink")) { UnityEngine.Debug.Log("[Baking] → Running ArmatureLink bake..."); BakeArmatureLink(target,true); }
    if (featureTypes.Contains("Toggle")){ UnityEngine.Debug.Log("[Baking] → Running Toggle bake..."); BakeToggleFromVrcfury(target); }
    // Nuke all VF.* components after everything is baked
    int nuked = 0;
    foreach (var comp in target.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true))
    {
    if (comp == null) continue;
    var typeFullName = comp.GetType().FullName ?? "";
    if (typeFullName.StartsWith("VF."))
    { UnityEditor.Undo.DestroyObjectImmediate(comp); nuked++; }
    }
    if (nuked > 0) UnityEngine.Debug.Log("[Baking] Nuked " + nuked + " VF components.");
    UnityEngine.Debug.Log("[Baking] ===== BakeAllVf complete for " + target.name + " ====="); }
  // ── ReadVfFeaturesFromPrefabYaml ────────────────────────────────
  // Scans a prefab's raw YAML for VRCFury script blocks (by GUID) and
  // returns what feature types they contain. Works WITHOUT VF installed.
  const System.String VF_ScriptGuid = "d9e94e501a2d4c95bff3d5601013d923";
  static System.Collections.Generic.HashSet<System.String> ReadVfFeaturesFromPrefabYaml(UnityEngine.GameObject prefab)
  { var features = new System.Collections.Generic.HashSet<System.String>();
    var path = UnityEditor.AssetDatabase.GetAssetPath(prefab);
    if (System.String.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return features;
    var yaml = System.IO.File.ReadAllText(path);
    // Find all !u!114 (UnityEngine.MonoBehaviour) blocks
    int pos = 0;
    while (pos < yaml.Length)
    {
    int blockStart = yaml.IndexOf("--- !u!114 &",pos);
    if (blockStart < 0) break;
    int nextBlock = yaml.IndexOf("--- !u!",blockStart + 12);
    int blockEnd = nextBlock >= 0 ? nextBlock : yaml.Length;
    System.String block = yaml.Substring(blockStart,blockEnd - blockStart);
    // Check if this block has the VF script GUID
    if (block.Contains(VF_ScriptGuid))
    {
      // Extract the feature type class name from references.RefIds[].type.class
      int classIdx = block.IndexOf("class: ");
      if (classIdx >= 0)
      {
      int classEnd = block.IndexOf('\n',classIdx);
      System.String className = block.Substring(classIdx + 7,classEnd - classIdx - 7).Trim();
      features.Add(className);
      UnityEngine.Debug.Log("[Baking] [YAML] Found VF feature: " + className); }
    }
    pos = blockEnd; }
    return features; }
  // ── BakeFullController ──────────────────────────────────────────
  // Replicates VRCFury FullController: merges controllers,menus,params
  // from a VRCFury FullController feature into the avatar,with path
  // rewriting and GoGoLoco compatibility (Go/ prefix,Base controller).
  public static void BakeFullController(UnityEngine.GameObject target)
  { System.String _fcTitle = "FCBaker: " + target.name;
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Preparing...",0f);
    if (target == null) { UnityEditor.EditorUtility.ClearProgressBar(); return; }
    var avatarName = Vars.Names.Sanitize(target.transform.root.name);
    var avatarRoot = FindAviRoot(avatarName,target) ?? target.transform.root.gameObject;
    var vrcad = avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad == null) { UnityEngine.Debug.Log("[FCBaker] No VRC.SDK3.Avatars.Components.VRCAvatarDescriptor on " + avatarRoot.name); UnityEditor.EditorUtility.ClearProgressBar(); return; }
    // Phase A — Create hierarchy
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Creating hierarchy...",0.05f);
    var fcRoot = CreateFullControllerHierarchy(avatarRoot,avatarName);
    // VRChat built-in params that should never be rewritten
    var globalParams = new System.Collections.Generic.HashSet<System.String> {
    "IsLocal","PreviewMode","Viseme","Voice","GestureLeft","GestureRight","GestureLeftWeight","GestureRightWeight","AngularY","VelocityX","VelocityY","VelocityZ","VelocityMagnitude","Upright","Grounded","Seated","AFK","TrackingType","VRMode","MuteSelf","InStation","Earmuffs","IsOnFriendsList","AvatarVersion","IsAnimatorEnabled","ScaleModified","ScaleFactor","ScaleFactorInverse","EyeHeightAsMeters","EyeHeightAsPercent"
    };
    // Phase B — Detect VRCFury FullController features
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Detecting FullController features...",0.1f);
    var results =  new System.Collections.Generic.List<(System.String fullName,System.Collections.Generic.List<(UnityEngine.RuntimeAnimatorController controller,int typeVal)> ctrls,System.Collections.Generic.List<(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu,System.String prefix)> menus,System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters> prms,System.Collections.Generic.List<System.String> globalParamPatterns,System.Boolean rootBindingsApplyToAvatar,System.Collections.Generic.List<(System.String from,System.String to,System.Boolean delete)> rewrites)>();
    var processedFullControllerComps =  new System.Collections.Generic.List<UnityEngine.MonoBehaviour>();
    UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy] Searching for FullController features on " + target.name + "...");
    foreach (var comp in target.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true))
    {
    if (comp == null) continue;
    var t = comp.GetType();
    if (t.FullName != "VF.Model.VRCFury") continue;
    try
    {
      var getAll = t.GetMethod("GetAllFeatures",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      System.Collections.IList features = null;
      if (getAll != null) features = getAll.Invoke(comp,null) as System.Collections.IList;
      if (features == null)
      {
      var cf = t.GetField("content",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (cf != null) { var v = cf.GetValue(comp); if (v != null) features = new System.Collections.Generic.List<System.Object> { v }; }
      }
      if (features == null) continue;
      System.Boolean hasFullController = false;
      foreach (var f in features)
      {
      if (f == null) continue;
      var ft = f.GetType();
      if (ft.Name != "FullController" && !(ft.FullName ?? "").Contains("FullController")) continue;
      hasFullController = true;
      // Extract controller entries — each has a controller + type (AnimLayerType)
      var ctrlsField = ft.GetField("controllers",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      var controllersList =  new System.Collections.Generic.List<(UnityEngine.RuntimeAnimatorController controller,int typeVal)>();
      if (ctrlsField != null)
      {
        var entries = ctrlsField.GetValue(f) as System.Collections.IList;
        if (entries != null)
        {
        UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy]   Found FullController with " + entries.Count + " controller entries");
        foreach (var entry in entries)
        {
          if (entry == null) continue;
          var entryType = entry.GetType();
          var ctrlField = entryType.GetField("controller",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var wrapper = ctrlField?.GetValue(entry);
          if (wrapper == null) continue;
          // Try Get() or GetController() method first (standard VRCFury RuntimeController/GuidController)
          UnityEngine.RuntimeAnimatorController ctrl = null;
          foreach (var methodName in new[] { "Get","GetController" })
          {
          var getMethod = wrapper.GetType().GetMethod(methodName,System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          if (getMethod != null)
          {
            ctrl = getMethod.Invoke(wrapper,null) as UnityEngine.RuntimeAnimatorController;
            if (ctrl != null) break; }
          }
          // If Get() returned null,check if wrapper itself IS the controller
          if (ctrl == null)
          ctrl = wrapper as UnityEngine.RuntimeAnimatorController;
          // Try direct field access (SerializeReference,private fields like _controller)
          if (ctrl == null)
          {
          var directField = wrapper.GetType().GetField("controller",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
          if (directField != null)
            ctrl = directField.GetValue(wrapper) as UnityEngine.RuntimeAnimatorController; }
          // Try alternate property/field name (Controller with capital C)
          if (ctrl == null)
          {
          var altField = wrapper.GetType().GetField("Controller",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
          if (altField != null)
            ctrl = altField.GetValue(wrapper) as UnityEngine.RuntimeAnimatorController; }
          // Try direct objRef field access (GuidWrapper stores resolved object here)
          if (ctrl == null)
          {
          var objRefField = wrapper.GetType().GetField("objRef",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          if (objRefField != null)
            ctrl = objRefField.GetValue(wrapper) as UnityEngine.RuntimeAnimatorController; }
          // Try VrcfObjectId resolution from id field (GuidWrapper serialization)
          if (ctrl == null)
          {
          var idField = wrapper.GetType().GetField("id",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
          if (idField != null)
          {
            var id = idField.GetValue(wrapper) as System.String;
            if (!System.String.IsNullOrEmpty(id))
            {
            // Try direct GUID extraction from id (format: "guid:fileId|fileName|objName")
            var guidPart = id.Split('|')[0].Split(':')[0];
            if (!System.String.IsNullOrEmpty(guidPart))
            {
              var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guidPart);
              if (!System.String.IsNullOrEmpty(assetPath))
              ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.RuntimeAnimatorController>(assetPath); }
            }
          }
          }
          // Read the type field to determine which layer this controller targets
          int typeVal = -1;
          var typeField = entryType.GetField("type",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          if (typeField != null) typeVal = (int)typeField.GetValue(entry);
          System.String ctrlName = ctrl != null ? ctrl.name : "null";
          UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy]   Entry: controller=" + ctrlName + ",typeVal=" + typeVal);
          if (ctrl != null) controllersList.Add((ctrl,typeVal)); }
        }
      }
      // Extract menu entries
      var menusField = ft.GetField("menus",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      var menusList =  new System.Collections.Generic.List<(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu,System.String)>();
      if (menusField != null)
      {
        var entries = menusField.GetValue(f) as System.Collections.IList;
        if (entries != null)
        {
        foreach (var entry in entries)
        {
          if (entry == null) continue;
          var menuField = entry.GetType().GetField("menu",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var prefixField = entry.GetType().GetField("prefix",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var wrapper = menuField?.GetValue(entry);
          System.String prefix = prefixField?.GetValue(entry) as System.String ?? "";
          if (wrapper == null) continue;
          var getMethod = wrapper.GetType().GetMethod("Get",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var menu = getMethod?.Invoke(wrapper,null) as VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
          if (menu != null) menusList.Add((menu,prefix)); }
        }
      }
      // Extract params entries
      var prmsField = ft.GetField("prms",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      var prmsList =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>();
      if (prmsField != null)
      {
        var entries = prmsField.GetValue(f) as System.Collections.IList;
        if (entries != null)
        {
        foreach (var entry in entries)
        {
          if (entry == null) continue;
          var prmField = entry.GetType().GetField("parameters",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var wrapper = prmField?.GetValue(entry);
          if (wrapper == null) continue;
          var getMethod = wrapper.GetType().GetMethod("Get",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var prm = getMethod?.Invoke(wrapper,null) as VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
          if (prm != null) prmsList.Add(prm); }
        }
      }
      // Extract global params
      var gpField = ft.GetField("globalParams",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      var gpList =  new System.Collections.Generic.List<System.String>();
      if (gpField != null)
      {
        var raw = gpField.GetValue(f) as System.Collections.IList;
        if (raw != null) foreach (var p in raw) if (p is System.String s) gpList.Add(s); }
      // Extract rootBindingsApplyToAvatar
      System.Boolean rootBind = false;
      var rbaField = ft.GetField("rootBindingsApplyToAvatar",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (rbaField != null) rootBind = (System.Boolean)rbaField.GetValue(f);
      // Extract rewrite bindings
      var rwField = ft.GetField("rewriteBindings",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      var rwList =  new System.Collections.Generic.List<(System.String,System.String,System.Boolean)>();
      if (rwField != null)
      {
        var raw = rwField.GetValue(f) as System.Collections.IList;
        if (raw != null)
        {
        foreach (var r in raw)
        {
          if (r == null) continue;
          var fromF = r.GetType().GetField("from",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var toF = r.GetType().GetField("to",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          var delF = r.GetType().GetField("delete",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
          System.String from = fromF?.GetValue(r) as System.String ?? "";
          System.String to = toF?.GetValue(r) as System.String ?? "";
          System.Boolean del = delF != null ? (System.Boolean)delF.GetValue(r) : false;
          rwList.Add((from,to,del)); }
        }
      }
      results.Add((comp.gameObject.name,controllersList,menusList,prmsList,gpList,rootBind,rwList)); }
      if (hasFullController) processedFullControllerComps.Add(comp); }
    catch (System.Exception _ex) { UnityEngine.Debug.LogError("[NZK] Unhandled exception: " + _ex.Message); }
    }
    if (results.Count == 0) { UnityEngine.Debug.Log("[FCBaker] No FullController features found."); return; }
    // Phase C — Populate slots from controller data
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Populating controller slots...",0.3f);
    System.String genFolder = "Assets/!_NZK_Generated/" + avatarName + "/";
    Systems.Folder.Ensure(genFolder);
    // Build flat controller entry list from all results
    // Map VRCFury's type enum values to layer indices:
    //   0=Base,1=Additive,2=Gesture,3=Action,4=FX,5=Sitting,6=TPose,7=IKPose
    var typeToLayer = new System.Collections.Generic.Dictionary<int,int>
    {
    {0,0},{1,1},{2,2},{3,3},{4,4},{5,5},{6,6},{7,7}
    };
    var controllerEntries =  new System.Collections.Generic.List<(UnityEngine.RuntimeAnimatorController controller,int layerIndex)>();
    // Collect all rewrite bindings from all FC features (for post-merge path rewriting)
    var allRewrites =  new System.Collections.Generic.List<(System.String from,System.String to,System.Boolean delete)>();
    System.Boolean anyRootBind = false;
    foreach (var (name,ctrls,menus,prms,globs,rootBind,rewrites) in results)
    {
    allRewrites.AddRange(rewrites);
    if (rootBind) anyRootBind = true;
    foreach (var (srcCtrl,typeVal) in ctrls)
    {
      if (srcCtrl == null) { UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy]   Skipping null controller,typeVal=" + typeVal); continue; }
      if (!(srcCtrl is UnityEditor.Animations.AnimatorController)) { UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy]   Skipping non-UnityEditor.Animations.AnimatorController: " + srcCtrl.GetType().Name); continue; }
      int layerIdx = typeToLayer.ContainsKey(typeVal) ? typeToLayer[typeVal] : 4;
      var typeNames = new[] { "Base","Additive","Gesture","Action","FX","Sitting","TPose","IKPose" };
      System.String layerName = (layerIdx >= 0 && layerIdx < typeNames.Length) ? typeNames[layerIdx] : "Unknown(" + layerIdx + ")";
      UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy]   Adding entry: " + srcCtrl.name + " typeVal=" + typeVal + " → " + layerName + "(layerIdx=" + layerIdx + ")");
      controllerEntries.Add((srcCtrl,layerIdx)); }
    }
    UnityEngine.Debug.Log("[FCBaker] [FC-Hierarchy]   Total controller entries: " + controllerEntries.Count + ",rewrite bindings: " + allRewrites.Count);
    // Find root C_AviGenerator for generator mergeSources wiring
    var rootHB = FindRootBaker(avatarRoot);
    PopulateControllerSlots(fcRoot,controllerEntries,vrcad,avatarName,genFolder,rootHB);
    // Phase C2 — Apply path rewrite bindings to all folder slot controllers
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Applying path rewrites...",0.5f);
    if (allRewrites.Count > 0)
    {
    var typeNames = new[] { "Base","Additive","Gesture","Action","FX","Sitting","TPose","IKPose" };
    for (int i = 0; i < 8; i++)
    {
      System.String parentName = i < 5 ? "Base" : "Special";
      var folderTr = fcRoot.Find(parentName + "/" + typeNames[i]);
      if (folderTr == null) continue;
      var folderSlot = folderTr.GetComponent<BakedControllerSlot>();
      if (folderSlot == null || folderSlot.animatorController == null) continue;
      if (!folderSlot.pathRewriteEnabled) continue;
      var ac = folderSlot.animatorController as UnityEditor.Animations.AnimatorController;
      if (ac == null) continue;
      ApplyRewriteBindings(ac,allRewrites,anyRootBind,folderSlot); }
    }
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Distributing menus & params...",0.7f);
    // Phase D — Distribute menus into HB_Circle_Menu hierarchy + merge params
    var allControls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>();
    foreach (var (name,ctrls,menus,prms,globs,rootBind,rewrites) in results)
    {
    foreach (var (menu,prefix) in menus)
    {
      if (menu == null) continue;
      foreach (var ctrl in menu.controls)
      {
      if (System.String.IsNullOrEmpty(prefix))
        allControls.Add(ctrl);
      else
      {
        allControls.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
        {
        name = prefix + "/" + ctrl.name,type = ctrl.type,parameter = ctrl.parameter,subMenu = ctrl.subMenu,icon = ctrl.icon,labels = ctrl.labels,style = ctrl.style
        }); }
      }
    }
    }
    CircleMenuBakerStub.PopulateMenuSlots(avatarName,avatarRoot,genFolder,allControls,vrcad);
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Merging expression params...",0.8f);
    // Phase D2 — Merge expression params
    foreach (var (name,ctrls,menus,prms,globs,rootBind,rewrites) in results)
    {
    var avatarParams = vrcad.expressionParameters;
    if (avatarParams == null)
    {
      avatarParams = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>();
      avatarParams.name = avatarName + " Params";
      avatarParams.parameters = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter[0];
      System.String paramPath = genFolder + avatarName + "_Params.asset";
      UnityEditor.AssetDatabase.CreateAsset(avatarParams,paramPath);
      vrcad.expressionParameters = avatarParams; }
    var existingParamNames = new System.Collections.Generic.HashSet<System.String>(
      System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(
        (avatarParams.parameters ?? new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter[0]),
        p => p != null && !System.String.IsNullOrEmpty(p.name)),
      p => p.name));
    var paramList = System.Linq.Enumerable.ToList(avatarParams.parameters);
    foreach (var prm in prms)
    {
      if (prm == null || prm.parameters == null) continue;
      foreach (var p in prm.parameters)
      {
      if (p == null || System.String.IsNullOrEmpty(p.name)) continue;
      if (existingParamNames.Contains(p.name)) continue;
      paramList.Add(p);
      existingParamNames.Add(p.name); }
    }
    int maxParams = 16;
    if (paramList.Count > maxParams)
      UnityEngine.Debug.LogWarning("[FCBaker] " + avatarName + " has " + paramList.Count + " params (max " + maxParams + ")");
    avatarParams.parameters = paramList.ToArray();
    UnityEditor.EditorUtility.SetDirty(avatarParams); }
    UnityEditor.EditorUtility.DisplayProgressBar(_fcTitle,"Cleanup...",0.9f);
    // Phase E — Cleanup: destroy only VRCFury components that had FullController features
    foreach (var comp in processedFullControllerComps)
    {
    if (comp != null)
    {
      UnityEditor.Undo.RecordObject(comp,"Remove VRCFury");
      UnityEngine.Object.DestroyImmediate(comp); }
    }
    UnityEditor.AssetDatabase.SaveAssets();
    UnityEngine.Debug.Log("[FCBaker] Baked " + results.Count + " FullController(s).");
    UnityEditor.EditorUtility.ClearProgressBar(); }
  // ── CreateFullControllerHierarchy ────────────────────────────────
  // Phase A: Creates GEN → HB_BakedControllers → {type folder} hierarchy.
  // Each type folder (Base/Additive/Gesture/Action/FX/Sitting/TPose/IKPose)
  // gets a BakedControllerSlot component that holds the FINAL merged output.
  // A Default child preserves the original pre-bake controller.
  // Per-controller children are added later by PopulateControllerSlots.
  public static UnityEngine.Transform CreateFullControllerHierarchy(UnityEngine.GameObject avatarRoot,System.String avatarName)
  { var genRoot = GetGenRoot(avatarName,avatarRoot);
    // Create/ensure HB_BakedControllers child with proper C_AviGenerator mode
    System.String bcName = HBChildren.Prefix + HBChildren.BakedControllers;
    var bcTransform = genRoot.Find(bcName);
    if (bcTransform == null)
    {
    var g = new UnityEngine.GameObject(bcName);
    UnityEditor.Undo.RecordObject(genRoot,"Create HB_BakedControllers");
    g.transform.SetParent(genRoot,false);
    var hb = g.AddComponent<C_AviGenerator>();
    UnityEditor.Undo.RecordObject(g,"Add C_AviGenerator");
    hb.mode = E_AviGeneratorMode.BakedControllers;
    hb.NZKC_GO_AviRoot = avatarRoot;
    hb.avatarRootName = avatarName;
    bcTransform = g.transform; }
    // Create type folder containers
    var baseParent = EnsureChild(bcTransform,"Base");
    var specialParent = EnsureChild(bcTransform,"Special");
    // Each type folder gets a BakedControllerSlot (holds final merged output)
    // and a Default child (preserves pre-bake controller)
    EnsureTypeFolder(baseParent,"Base");
    EnsureTypeFolder(baseParent,"Additive");
    EnsureTypeFolder(baseParent,"Gesture");
    EnsureTypeFolder(baseParent,"Action");
    EnsureTypeFolder(baseParent,"FX");
    EnsureTypeFolder(specialParent,"Sitting");
    EnsureTypeFolder(specialParent,"TPose");
    EnsureTypeFolder(specialParent,"IKPose");
    return bcTransform; }
  // ── EnsureTypeFolder ─────────────────────────────────────────────
  // Creates a type folder that has its own BakedControllerSlot
  // (holds the final merged controller for this layer) plus a Default
  // child that preserves the pre-bake controller.
  static UnityEngine.Transform EnsureTypeFolder(UnityEngine.Transform parent,System.String typeName)
  { var folder = EnsureChild(parent,typeName);
    // Add BakedControllerSlot to the folder itself (holds final merged output)
    if (folder.GetComponent<BakedControllerSlot>() == null)
    {
    var slot = folder.gameObject.AddComponent<BakedControllerSlot>();
    UnityEditor.Undo.RecordObject(folder.gameObject,"Add folder BakedControllerSlot");
    SetSlotDefaults(slot,typeName);
    slot.isDefault = false; // folder slot = baked output,never "default"
    }
    // Ensure Default child preserves pre-bake state
    if (folder.Find("Default") == null)
    {
    var def = new UnityEngine.GameObject("Default");
    UnityEditor.Undo.RecordObject(folder,"Create Default child");
    def.transform.SetParent(folder,false);
    var defSlot = def.AddComponent<BakedControllerSlot>();
    UnityEditor.Undo.RecordObject(def,"Add Default BakedControllerSlot");
    SetSlotDefaults(defSlot,typeName); }
    return folder; }
  // ── EnsureChild ──────────────────────────────────────────────────
  static UnityEngine.Transform EnsureChild(UnityEngine.Transform parent,System.String name)
  { var child = parent.Find(name);
    if (child == null)
    {
    var g = new UnityEngine.GameObject(name);
    g.transform.SetParent(parent,false);
    child = g.transform; }
    return child; }
  // ── SetSlotDefaults ──────────────────────────────────────────────
  static void SetSlotDefaults(BakedControllerSlot slot,System.String typeName)
  { switch (typeName)
    {
    case "Base":    slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base;    slot.slotIndex = 0; slot.isSpecialLayer = false; break;
    case "Additive":  slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Additive;  slot.slotIndex = 1; slot.isSpecialLayer = false; break;
    case "Gesture":   slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture;   slot.slotIndex = 2; slot.isSpecialLayer = false; break;
    case "Action":  slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Action;  slot.slotIndex = 3; slot.isSpecialLayer = false; break;
    case "FX":    slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;    slot.slotIndex = 4; slot.isSpecialLayer = false; break;
    case "Sitting":   slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Sitting;   slot.slotIndex = 0; slot.isSpecialLayer = true;  break;
    case "TPose":   slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.TPose;   slot.slotIndex = 1; slot.isSpecialLayer = true;  break;
    case "IKPose":  slot.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.IKPose;  slot.slotIndex = 2; slot.isSpecialLayer = true;  break; }
    slot.isDefault = true;
    slot.isEnabled = true; }
  // ── Known VRC default layer names ────────────────────────────────
  // These are stripped when merging into replacement layers (Base/Sitting/TPose/IKPose).
  static readonly System.Collections.Generic.HashSet<System.String> VrcDefaultLayerNames = new System.Collections.Generic.HashSet<System.String>
  { "Base Layer","Sitting","TPose","IK Pass","Action","FX","Gesture","Additive"
  };
  // ── PopulateControllerSlots ──────────────────────────────────────
  // Phase C: Per VRCFury FullController entry,creates a child under the matching
  // type folder,clones/merges the controller matching VRCFury's layer-level strategy,// stores it in that child's BakedControllerSlot,then assigns all 8 slots to the
  // VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.
  //
  // Merge strategy by layer:
  //   Base/Sitting/TPose/IKPose — "replacement" layers:
  //   Clone VRC descriptor controller,remove default layers,append VRCFury layers.
  //   FX/Gesture/Additive — "append" layers:
  //   Clone VRC descriptor controller,keep all layers,append VRCFury layers.
  //   Action — hybrid:
  //   Clone VRC descriptor,keep all layers,append VRCFury layers.
  public static void PopulateControllerSlots(
    UnityEngine.Transform fcRoot,System.Collections.Generic.List<(UnityEngine.RuntimeAnimatorController controller,int layerIndex)> controllerEntries,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,System.String avatarName,System.String genFolder,C_AviGenerator rootHB = null)
  { var layerTypes = new[] {
    VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Additive,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Action,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Sitting,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.TPose,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.IKPose,};
    var typeNames = new[] { "Base","Additive","Gesture","Action","FX","Sitting","TPose","IKPose" };
    // Layers that use "replacement" merge (strip default layers,then append)
    var replacementLayers = new System.Collections.Generic.HashSet<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType>
    {
    VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Sitting,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.TPose,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.IKPose,};
    // Ensure VRC descriptor arrays exist
    if (vrcad.baseAnimationLayers == null || vrcad.baseAnimationLayers.Length < 5)
    VRCAD.Set.InitAllLayers(vrcad);
    if (vrcad.specialAnimationLayers == null || vrcad.specialAnimationLayers.Length < 3)
    VRCAD.Set.InitAllLayers(vrcad);
    // Capture pre-existing controllers from VRC descriptor (pre-bake state)
    var preExistingControllers = new System.Collections.Generic.Dictionary<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType,UnityEngine.RuntimeAnimatorController>();
    for (int i = 0; i < 5; i++)
    preExistingControllers[layerTypes[i]] = vrcad.baseAnimationLayers[i].animatorController;
    for (int i = 0; i < 3; i++)
    preExistingControllers[layerTypes[5 + i]] = vrcad.specialAnimationLayers[i].animatorController;
    // Track which layers received a baked controller (non-Default)
    var layersWithBaked = new System.Collections.Generic.HashSet<int>();
    // Process each VRCFury controller entry — create a child,merge,store
    UnityEngine.Debug.Log("[FCBaker] [FC-Populate] Processing " + controllerEntries.Count + " controller entries...");
    foreach (var (srcCtrl,layerIdx) in controllerEntries)
    {
    var ac = srcCtrl as UnityEditor.Animations.AnimatorController;
    if (ac == null) continue;
    var layerType = layerTypes[layerIdx];
    // Determine parent folder in hierarchy
    System.String parentName = layerIdx < 5 ? "Base" : "Special";
    var folderTransform = fcRoot.Find(parentName + "/" + typeNames[layerIdx]);
    if (folderTransform == null) { UnityEngine.Debug.LogWarning("[FCBaker] [FC-Populate]   Folder not found: " + parentName + "/" + typeNames[layerIdx]); continue; }
    // Determine child name — use sanitized asset name or fallback
    System.String srcAssetPath = UnityEditor.AssetDatabase.GetAssetPath(ac);
    System.String ctrlName = !System.String.IsNullOrEmpty(srcAssetPath)
      ? SanitizeName(System.IO.Path.GetFileNameWithoutExtension(srcAssetPath))
      : "FC_" + layerIdx;
    // Avoid collision with "Default"
    if (ctrlName == "Default") ctrlName = "FC_" + layerIdx;
    UnityEngine.Debug.Log("[FCBaker] [FC-Populate]   Processing: " + typeNames[layerIdx] + " → childName=" + ctrlName);
    // Create child if it doesn't already exist
    var childTr = folderTransform.Find(ctrlName);
    UnityEngine.GameObject childGo;
    BakedControllerSlot childSlot;
    if (childTr == null)
    {
      childGo = new UnityEngine.GameObject(ctrlName);
      UnityEditor.Undo.RecordObject(folderTransform,"Create controller child");
      childGo.transform.SetParent(folderTransform,false);
      childSlot = childGo.AddComponent<BakedControllerSlot>();
      UnityEditor.Undo.RecordObject(childGo,"Add child BakedControllerSlot");
      childSlot.layerType = layerType;
      childSlot.slotIndex = layerIdx < 5 ? layerIdx : layerIdx - 5;
      childSlot.isSpecialLayer = layerIdx >= 5;
      childSlot.isDefault = false;
      childSlot.isEnabled = true; }
    else
    {
      childGo = childTr.gameObject;
      childSlot = childTr.GetComponent<BakedControllerSlot>();
      if (childSlot == null)
      {
      childSlot = childGo.AddComponent<BakedControllerSlot>();
      UnityEditor.Undo.RecordObject(childGo,"Add missing child BakedControllerSlot");
      childSlot.layerType = layerType;
      childSlot.slotIndex = layerIdx < 5 ? layerIdx : layerIdx - 5;
      childSlot.isSpecialLayer = layerIdx >= 5; }
    }
    layersWithBaked.Add(layerIdx);
    // Get the folder's BakedControllerSlot (stores the final merged result)
    var folderSlot = folderTransform.GetComponent<BakedControllerSlot>();
    // ── Store raw source on child ───────────────────────────
    // The child always stores the unmodified raw VRCFury controller
    childSlot.animatorController = ac;
    childSlot.isDefault = false;
    childSlot.isEnabled = true;
    // ── Also add this VRCFury controller to the corresponding generator's mergeSources ──
    AddToGeneratorMergeSources(rootHB,layerType,ac);
    // ── Build merged controller via ControllerMerger ──────
    // Step 1: Get the pre-existing controller from VRC descriptor
    preExistingControllers.TryGetValue(layerType,out var existingRaw);
    var existingCtrl = existingRaw as UnityEditor.Animations.AnimatorController;
    System.Boolean isReplacement = replacementLayers.Contains(layerType);
    UnityEditor.Animations.AnimatorController baseCtrl = null;
    // Clone the pre-existing VRCAD controller (or VRCFury source if none exists)
    if (existingCtrl != null)
    {
      System.String basePath = genFolder + avatarName + "_" + typeNames[layerIdx] + "_Base.controller";
      baseCtrl = CloneController(existingCtrl,basePath,genFolder,avatarName,typeNames[layerIdx] + "_Base"); }
    UnityEditor.Animations.AnimatorController mergedResult = null;
    if (baseCtrl == null)
    {
      // No existing VRCAD controller — use VRCFury source directly
      System.String mergedPath = genFolder + avatarName + "_" + typeNames[layerIdx] + "_Merged_" + ctrlName + ".controller";
      baseCtrl = CloneController(ac,mergedPath,genFolder,avatarName,typeNames[layerIdx] + "_" + ctrlName);
      mergedResult = baseCtrl; }
    else
    {
      // Strip default VRC layers for replacement layers (Base/Sitting/TPose/IKPose)
      if (isReplacement)
      {
      var layersToKeep =  new System.Collections.Generic.List<UnityEditor.Animations.AnimatorControllerLayer>();
      foreach (var l in baseCtrl.layers)
        if (!VrcDefaultLayerNames.Contains(l.name)) layersToKeep.Add(l);
      if (layersToKeep.Count == 0)
      {
        var emptySm = new UnityEditor.Animations.AnimatorStateMachine { name = "Empty" };
        layersToKeep.Add(new UnityEditor.Animations.AnimatorControllerLayer
        { name = "Empty",stateMachine = emptySm,defaultWeight = 1f });
        UnityEditor.AssetDatabase.AddObjectToAsset(emptySm,baseCtrl); }
      while (baseCtrl.layers.Length > 0) baseCtrl.RemoveLayer(0);
      foreach (var l in layersToKeep) baseCtrl.AddLayer(l);
      UnityEditor.EditorUtility.SetDirty(baseCtrl); }
      // Use ControllerMerger to append VRCFury layers into baseCtrl
      System.String mergedPath = genFolder + avatarName + "_" + typeNames[layerIdx] + "_Merged_" + ctrlName + ".controller";
      mergedResult = ControllerMerger.MergeControllers(mergedPath,baseCtrl,ac); }
    // Update the folder's slot with the merged result
    if (mergedResult != null && folderSlot != null)
    {
      UnityEditor.Undo.RecordObject(folderSlot,"Update folder baked controller");
      folderSlot.animatorController = mergedResult;
      folderSlot.isDefault = false;
      folderSlot.isEnabled = true; }
    UnityEngine.Debug.Log("[FCBaker] [FC-Populate]   Child " + parentName + "/" + typeNames[layerIdx] + "/" + ctrlName + ": rawCtrl=" + (ac != null ? ac.name : "null") +
      ",folderMerged=" + (mergedResult != null ? mergedResult.name : "null")); }
    // ── Finalize: Preserve Default,assign folder slot to VRCAD ──
    // The folder's BakedControllerSlot was already updated during merge phase.
    // Here we:
    //   1. Preserve pre-bake controller in Default (copied once,never overwritten)
    //   2. Assign folder slot (final merged output) to VRCAD
    UnityEngine.Debug.Log("[FCBaker] [FC-Populate] Finalizing " + layersWithBaked.Count + " baked + remaining default slots...");
    void FinalizeLayer(int layerIdx,System.Boolean isSpecial)
    {
    System.String parentName = isSpecial ? "Special" : "Base";
    var folderTr = fcRoot.Find(parentName + "/" + typeNames[layerIdx]);
    if (folderTr == null) return;
    var folderSlot = folderTr.GetComponent<BakedControllerSlot>();
    var defTr = folderTr.Find("Default");
    var defSlot = defTr != null ? defTr.GetComponent<BakedControllerSlot>() : null;
    int vrcadIdx = layerIdx < 5 ? layerIdx : layerIdx - 5;
    var layerRef = isSpecial ? vrcad.specialAnimationLayers[vrcadIdx] : vrcad.baseAnimationLayers[vrcadIdx];
    // ── Preserve pre-bake controller in Default (first bake only) ──
    if (defSlot != null && defSlot.animatorController == null)
    {
      // Read the current VRCAD controller save into Default
      var currentCtrl = isSpecial
      ? vrcad.specialAnimationLayers[vrcadIdx].animatorController
      : vrcad.baseAnimationLayers[vrcadIdx].animatorController;
      var currentIsDefault = isSpecial
      ? vrcad.specialAnimationLayers[vrcadIdx].isDefault
      : vrcad.baseAnimationLayers[vrcadIdx].isDefault;
      var currentEnabled = isSpecial
      ? vrcad.specialAnimationLayers[vrcadIdx].isEnabled
      : vrcad.baseAnimationLayers[vrcadIdx].isEnabled;
      var currentMask = isSpecial
      ? vrcad.specialAnimationLayers[vrcadIdx].mask
      : vrcad.baseAnimationLayers[vrcadIdx].mask;
      defSlot.animatorController = currentCtrl;
      defSlot.isDefault = currentIsDefault;
      defSlot.isEnabled = currentEnabled;
      defSlot.mask = currentMask;
      UnityEditor.Undo.RecordObject(defSlot.gameObject,"Preserve pre-bake controller");
      UnityEngine.Debug.Log("[FCBaker] [FC-Populate]   Preserved pre-bake ctrl in " + parentName + "/" + typeNames[layerIdx] + "/Default: " +
      (defSlot.animatorController != null ? defSlot.animatorController.name : "null")); }
    // ── Assign to VRCAD: use folder's slot (final merged output) ──
    var assignSlot = folderSlot ?? defSlot;
    if (assignSlot == null) return;
    UnityEditor.Undo.RecordObject(vrcad,"Assign baked controller to " + typeNames[layerIdx]);
    // If the final baked controller is null or flagged as default,mark the
    // VRCAD layer as isDefault=true so VRChat uses its built-in controller.
    System.Boolean finalIsDefault = assignSlot.animatorController == null || assignSlot.isDefault;
    Systems.PlayableLayers.AssignLayer(
      vrcad,assignSlot.layerType,assignSlot.animatorController,assignSlot.isEnabled,finalIsDefault,assignSlot.mask
    );
    UnityEngine.Debug.Log("[FCBaker] [FC-Populate]   " + (isSpecial ? "special" : "base") + "AnimationLayers[" + vrcadIdx + "] (" + typeNames[layerIdx] + "): ctrl=" +
      (assignSlot.animatorController != null ? assignSlot.animatorController.name : "null") + ",isDefault=" + finalIsDefault); }
    for (int i = 0; i < 5; i++) FinalizeLayer(i,false);
    for (int i = 5; i < 8; i++) FinalizeLayer(i,true); }
  // ── CloneController ──────────────────────────────────────────────
  // Clones an UnityEditor.Animations.AnimatorController to a new asset path. Returns the clone.
  static UnityEditor.Animations.AnimatorController CloneController(
    UnityEditor.Animations.AnimatorController source,System.String targetPath,System.String genFolder,System.String avatarName,System.String debugName)
  { Systems.Folder.Ensure(genFolder);
    System.String srcPath = UnityEditor.AssetDatabase.GetAssetPath(source);
    if (!System.String.IsNullOrEmpty(srcPath))
    {
    UnityEditor.AssetDatabase.DeleteAsset(targetPath);
    if (UnityEditor.AssetDatabase.CopyAsset(srcPath,targetPath))
    {
      UnityEditor.AssetDatabase.ImportAsset(targetPath);
      return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(targetPath); }
    }
    // Fallback: create a fresh controller
    var fresh = new UnityEditor.Animations.AnimatorController();
    fresh.name = debugName;
    foreach (var p in source.parameters) fresh.AddParameter(p.name,p.type);
    foreach (var l in source.layers)
    {
    var sm = new UnityEditor.Animations.AnimatorStateMachine();
    sm.name = l.stateMachine.name + "_C";
    CopyStates(l.stateMachine,sm);
    CopyTransitions(l.stateMachine,sm);
    UnityEditor.AssetDatabase.AddObjectToAsset(sm,fresh);
    fresh.AddLayer(new UnityEditor.Animations.AnimatorControllerLayer
    {
      name = l.name,stateMachine = sm,defaultWeight = l.defaultWeight,blendingMode = l.blendingMode,iKPass = l.iKPass
    }); }
    UnityEditor.AssetDatabase.CreateAsset(fresh,targetPath);
    UnityEditor.AssetDatabase.SaveAssets();
    return fresh; }
  // ── ApplyRewriteBindings ─────────────────────────────────────────
  // Applies VRCFury-style path rewrite bindings to all animation clips
  // in a controller. Each binding rewrites animation paths from `from`
  // prefix to `to` prefix. If `delete` is true,curves with that prefix
  // are removed.
  public static void ApplyRewriteBindings(
    UnityEditor.Animations.AnimatorController controller,System.Collections.Generic.List<(System.String from,System.String to,System.Boolean delete)> rewrites,System.Boolean rootBindingsApplyToAvatar,BakedControllerSlot slot)
  { // Collect all animation clips from the controller
    var clips = new System.Collections.Generic.HashSet<UnityEngine.AnimationClip>();
    foreach (var layer in controller.layers)
    CollectClips(layer.stateMachine,clips);
    foreach (var clip in clips)
    {
    if (clip == null) continue;
    System.String clipPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    // Must be editable (not a built-in resource)
    if (System.String.IsNullOrEmpty(clipPath) || clipPath.StartsWith("Library/"))
      continue;
    // Get all curve bindings
    var bindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);
    var objBindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clip);
    System.Boolean changed = false;
    foreach (var binding in bindings)
    {
      System.String newPath = RewritePath(binding.path,rewrites,rootBindingsApplyToAvatar);
      if (newPath == null)
      {
      // Delete this curve
      UnityEditor.AnimationUtility.SetEditorCurve(clip,binding,null);
      changed = true; }
      else if (newPath != binding.path)
      {
      // Copy curve data to new binding with rewritten path
      var curve = UnityEditor.AnimationUtility.GetEditorCurve(clip,binding);
      var newBinding = binding;
      newBinding.path = newPath;
      // Remove old curve,add at new binding
      UnityEditor.AnimationUtility.SetEditorCurve(clip,binding,null);
      UnityEditor.AnimationUtility.SetEditorCurve(clip,newBinding,curve);
      changed = true; }
    }
    foreach (var binding in objBindings)
    {
      System.String newPath = RewritePath(binding.path,rewrites,rootBindingsApplyToAvatar);
      if (newPath == null)
      {
      UnityEditor.AnimationUtility.SetObjectReferenceCurve(clip,binding,null);
      changed = true; }
      else if (newPath != binding.path)
      {
      var curve = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip,binding);
      var newBinding = binding;
      newBinding.path = newPath;
      UnityEditor.AnimationUtility.SetObjectReferenceCurve(clip,binding,null);
      UnityEditor.AnimationUtility.SetObjectReferenceCurve(clip,newBinding,curve);
      changed = true; }
    }
    if (changed)
    {
      UnityEditor.EditorUtility.SetDirty(clip);
      UnityEngine.Debug.Log("[FCBaker] [Rewrite] Rewrote paths in " + clip.name); }
    }
  }
  static System.String RewritePath(System.String path,System.Collections.Generic.List<(System.String from,System.String to,System.Boolean delete)> rewrites,System.Boolean rootBindingsApplyToAvatar)
  { foreach (var (from,to,del) in rewrites)
    {
    var f = from ?? "";
    while (f.EndsWith("/")) f = f.Substring(0,f.Length - 1);
    var t = to ?? "";
    while (t.EndsWith("/")) t = t.Substring(0,t.Length - 1);
    if (f == "")
    {
      path = JoinPath(t,path);
      if (del) return null; }
    else if (path.StartsWith(f + "/"))
    {
      path = path.Substring(f.Length + 1);
      path = JoinPath(t,path);
      if (del) return null; }
    else if (path == f)
    {
      path = t;
      if (del) return null; }
    }
    return path; }
  static System.String JoinPath(System.String a,System.String b)
  { if (System.String.IsNullOrEmpty(a)) return b ?? "";
    if (System.String.IsNullOrEmpty(b)) return a;
    return a + "/" + b; }
  static void CollectClips(UnityEditor.Animations.AnimatorStateMachine sm,System.Collections.Generic.HashSet<UnityEngine.AnimationClip> clips)
  { foreach (var state in sm.states)
    {
    if (state.state.motion is UnityEngine.AnimationClip clip)
      clips.Add(clip);
    else if (state.state.motion is UnityEditor.Animations.BlendTree tree)
      CollectClipsFromTree(tree,clips); }
    foreach (var child in sm.stateMachines)
    CollectClips(child.stateMachine,clips); }
  static void CollectClipsFromTree(UnityEditor.Animations.BlendTree tree,System.Collections.Generic.HashSet<UnityEngine.AnimationClip> clips)
  { foreach (var child in tree.children)
    {
    if (child.motion is UnityEngine.AnimationClip clip)
      clips.Add(clip);
    else if (child.motion is UnityEditor.Animations.BlendTree subTree)
      CollectClipsFromTree(subTree,clips); }
  }
  // ── PopulateMenuSlots is now in UnityEditor.Editor/systems/builder/CircleMenu.editor.cs ──
  // ── CopyStates ───────────────────────────────────────────────────
  static void CopyStates(UnityEditor.Animations.AnimatorStateMachine source,UnityEditor.Animations.AnimatorStateMachine dest)
  { foreach (var s in source.states)
    {
    var ns = dest.AddState(s.state.name,s.position);
    ns.motion = s.state.motion;
    ns.writeDefaultValues = s.state.writeDefaultValues;
    ns.speed = s.state.speed;
    ns.timeParameter = s.state.timeParameter;
    ns.cycleOffset = s.state.cycleOffset;
    ns.cycleOffsetParameter = s.state.cycleOffsetParameter;
    ns.mirrorParameter = s.state.mirrorParameter;
    ns.mirrorParameterActive = s.state.mirrorParameterActive;
    ns.speedParameter = s.state.speedParameter;
    ns.tag = s.state.tag; }
  }
  // ── CopyTransitions ──────────────────────────────────────────────
  static void CopyTransitions(UnityEditor.Animations.AnimatorStateMachine source,UnityEditor.Animations.AnimatorStateMachine dest)
  { // Build a name→state lookup on dest
    var destStates = new System.Collections.Generic.Dictionary<System.String,UnityEditor.Animations.AnimatorState>();
    foreach (var s in dest.states)
    if (!destStates.ContainsKey(s.state.name))
      destStates[s.state.name] = s.state;
    // Build a name→state lookup on source (to find original states by name)
    var srcStates = new System.Collections.Generic.Dictionary<System.String,UnityEditor.Animations.AnimatorState>();
    foreach (var s in source.states)
    if (!srcStates.ContainsKey(s.state.name))
      srcStates[s.state.name] = s.state;
    foreach (var s in source.states)
    {
    if (!destStates.TryGetValue(s.state.name,out var destState)) continue;
    foreach (var t in s.state.transitions)
    {
      System.String destName = t.destinationState != null ? t.destinationState.name : null;
      var targetState = destName != null && destStates.ContainsKey(destName) ? destStates[destName] : null;
      var nt = destState.AddTransition(targetState);
      nt.hasExitTime = t.hasExitTime;
      nt.duration = t.duration;
      nt.exitTime = t.exitTime;
      nt.offset = t.offset;
      nt.interruptionSource = t.interruptionSource;
      nt.orderedInterruption = t.orderedInterruption;
      nt.canTransitionToSelf = t.canTransitionToSelf;
      foreach (var c in t.conditions)
      nt.AddCondition(c.mode,c.threshold,c.parameter); }
    }
    // AnyStateTransitions
    foreach (var t in source.anyStateTransitions)
    {
    System.String destName = t.destinationState != null ? t.destinationState.name : null;
    var targetState = destName != null && destStates.ContainsKey(destName) ? destStates[destName] : null;
    var nt = dest.AddAnyStateTransition(targetState);
    nt.hasExitTime = t.hasExitTime;
    nt.duration = t.duration;
    nt.exitTime = t.exitTime;
    foreach (var c in t.conditions)
      nt.AddCondition(c.mode,c.threshold,c.parameter); }
    // EntryTransitions
    foreach (var t in source.entryTransitions)
    {
    System.String destName = t.destinationState != null ? t.destinationState.name : null;
    var targetState = destName != null && destStates.ContainsKey(destName) ? destStates[destName] : null;
    var nt = dest.AddEntryTransition(targetState);
    foreach (var c in t.conditions)
      nt.AddCondition(c.mode,c.threshold,c.parameter); }
  }
  // ── GetGenRoot ──────────────────────────────────────────────────
  // Returns or creates the generator root for an avatar.
  // The root gets an AviLink component referencing the avatar.
  // Naming: agnostic — searches by AviLink reference first,then by name.
  public static UnityEngine.Transform GetGenRoot(System.String avatarName,UnityEngine.GameObject avatarObj = null)
  { // First: find existing AviLink pointing to this avatar
    if (avatarObj != null)
    {
    foreach (var link in UnityEngine.Resources.FindObjectsOfTypeAll<AviLink>())
    {
      if (link != null && link.avatarRoot == avatarObj && link.gameObject.scene == avatarObj.scene)
      return link.transform; }
    }
    // Second: find by name
    System.String genName = "GEN_" + avatarName;
    var existing = UnityEngine.GameObject.Find(genName);
    if (existing != null)
    {
    // Ensure it has an AviLink
    if (existing.GetComponent<AviLink>() == null)
    {
      var link = existing.AddComponent<AviLink>();
      link.avatarRoot = avatarObj ?? existing;
      link.avatarName = avatarName; }
    return existing.transform; }
    // Create new
    var go = new UnityEngine.GameObject(genName);
    go.transform.SetParent(null);
    var al = go.AddComponent<AviLink>();
    al.avatarRoot = avatarObj ?? go;
    al.avatarName = avatarName;
    al.mode = avatarObj != null ? AviLink.LinkMode.Updater : AviLink.LinkMode.Generator;
    return go.transform; }
  // ── FindRootBaker ───────────────────────────────────────────────
  // Finds the root C_AviGenerator (mode=Root) from an UnityEngine.Avatar context.
  public static C_AviGenerator FindRootBaker(UnityEngine.GameObject avatarRoot)
  { if (avatarRoot == null) return null;
    // Check avatarRoot's UnityEngine.Transform hierarchy for a Root-mode HB
    foreach (var hb in avatarRoot.GetComponentsInChildren<C_AviGenerator>(true))
    if (hb.mode == E_AviGeneratorMode.Root) return hb;
    return null; }
  // ── AddToGeneratorMergeSources ───────────────────────────────────
  // Adds a VRCFury controller to the appropriate generator's mergeSources
  // so the new merge system picks it up during re-bake.
  static void AddToGeneratorMergeSources(C_AviGenerator rootHB,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType layerType,UnityEngine.RuntimeAnimatorController ctrl)
  { if (rootHB == null || ctrl == null) return;
    // Map layer type → generator child mode
    E_AviGeneratorMode? genMode = layerType switch
    {
    VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture => E_AviGeneratorMode.GestureGenerator,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Action => E_AviGeneratorMode.ExpressionsGenerator,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX => E_AviGeneratorMode.FxGenerator,_ => null
    };
    if (genMode == null) return;
    // Find generator child
    foreach (UnityEngine.Transform c in rootHB.transform)
    {
    var chb = c.GetComponent<C_AviGenerator>();
    if (chb == null || chb.mode != genMode.Value) continue;
    // Add to mergeSources (avoid duplicates)
    if (!chb.mergeSources.Contains(ctrl))
    {
      chb.mergeSources.Add(ctrl);
      UnityEditor.EditorUtility.SetDirty(chb);
      UnityEngine.Debug.Log("[FCBaker] Added " + ctrl.name + " to " + c.name + ".mergeSources"); }
    break; }
  }
  // ── FindAviRoot ──────────────────────────────────────────────
  // Resolves the actual UnityEngine.Avatar root UnityEngine.GameObject from any context.
  // Checks AviLink references first,then falls back to UnityEngine.GameObject.Find.
  public static UnityEngine.GameObject FindAviRoot(System.String avatarName,UnityEngine.GameObject contextObj = null)
  { // Check if contextObj has an AviLink ancestor
    if (contextObj != null)
    {
    var link = contextObj.GetComponentInParent<AviLink>();
    if (link != null && link.avatarRoot != null) return link.avatarRoot; }
    // Find AviLink by name matching
    foreach (var link in UnityEngine.Resources.FindObjectsOfTypeAll<AviLink>())
    {
    if (link != null && link.avatarName == avatarName && link.avatarRoot != null)
      return link.avatarRoot; }
    // Fallback: UnityEngine.GameObject.Find
    var found = UnityEngine.GameObject.Find(avatarName);
    if (found != null) return found;
    return null; }
  // ── DestroyAllVrcfuryComponents ─────────────────────────────────
  // Destroys every VRCFury-related UnityEngine.MonoBehaviour on a UnityEngine.GameObject.
  // Call after ANY bake to fully remove VRCFury dependency.
  public static void DestroyAllVrcfuryComponents(UnityEngine.GameObject target)
  { if (target == null) return;
    foreach (var comp in target.GetComponents<UnityEngine.MonoBehaviour>())
    {
    if (comp == null) continue;
    var fn = comp.GetType().FullName ?? "";
    if (fn.StartsWith("VF.") || fn.StartsWith("VF.Model") || fn.StartsWith("VF.UnityEngine.Component"))
      UnityEditor.Undo.DestroyObjectImmediate(comp); }
  }
  // ── FxMerger ────────────────────────────────────────────────────
  
  
}
}
}
}
#endif
