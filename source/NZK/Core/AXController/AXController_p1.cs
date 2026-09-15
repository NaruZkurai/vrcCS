#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class AXController
  {/* ---- Setup / Import ---- */
  public static void SetupGesturesDefault(C_AviGenerator hb)
  { if (hb.mode != E_AviGeneratorMode.GestureGenerator && hb.mode != E_AviGeneratorMode.ControllerBuilder) return;
    if (hb.gestureReferenceController != null) { UnityEngine.Debug.Log("[HB] Gesture reference already set: " + hb.gestureReferenceController.name); return; }
    var def = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.RuntimeAnimatorController>(DefaultGestureRefPath);
    if (def != null) { hb.gestureReferenceController = def; UnityEditor.EditorUtility.SetDirty(hb);
    UnityEngine.Debug.Log("[HB] SetupGesturesDefault: loaded " + DefaultGestureRefPath); }
    else UnityEngine.Debug.LogWarning("[HB] SetupGesturesDefault: default not found at " + DefaultGestureRefPath); }
  /* ---- Bake Gesture Controller (entry point from UI) ---- */
  public static void BakeGestureController(C_AviGenerator hb)
  { var root = hb.transform.parent?.GetComponent<C_AviGenerator>();
    if (root != null) { BakeGestureLayers(root); UnityEngine.Debug.Log("[HB] Gesture controller baked from GestureGenerator."); }
    else UnityEngine.Debug.LogWarning("[HB] No root C_AviGenerator parent — cannot bake gesture controller."); }
  /* ---- Bake Gesture Layers ---- */
  public static void BakeGestureLayers(C_AviGenerator hb) => BakeGestureLayersToPath(hb,NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Controllers) + "/Gesture.controller");
  public static void BakeGestureLayersToPath(C_AviGenerator hb,System.String ctrlPath)
  { UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: enter ctrlPath=" + ctrlPath);
    /* Try all three possible modes for the generator object */
    var gest = hb.FindHB(HBChildren.GestureGenerator) ??
         hb.FindHB(HBChildren.AnimatorBuilder) ??
         hb.FindHBByMode(E_AviGeneratorMode.ControllerBuilder);
    if (gest == null) { UnityEngine.Debug.LogWarning("[HB] No Gesture/UnityEngine.Animator/Controller Builder found."); return; }
    var gestHB = gest.GetComponent<C_AviGenerator>();
    if (gestHB == null) { UnityEngine.Debug.LogWarning("[HB] No C_AviGenerator on controller generator."); return; }
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: gest=" + gest.name + " mode=" + gestHB.mode);
    var ctrlDir = System.IO.Path.GetDirectoryName(ctrlPath);
    System.IO.Directory.CreateDirectory(ctrlDir);
    var preLoad = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(ctrlPath);
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: pre-existing controller at ctrlPath=" + (preLoad != null ? preLoad.name : "null"));
    if (preLoad != null)
    { UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: deleting old controller " + preLoad.name);
    UnityEditor.AssetDatabase.DeleteAsset(ctrlPath);
    UnityEditor.AssetDatabase.SaveAssets();
    UnityEditor.AssetDatabase.Refresh();
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: deleted + saved"); }
    /* Ensure reference controller is set and hierarchy children exist */
    System.Boolean needSetup = gestHB.gestureReferenceController == null;
    System.Boolean needReload = !HasGestureChildren(gestHB);
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: needSetup=" + needSetup + " needReload=" + needReload);
    if (needSetup) SetupGesturesDefault(gestHB);
    if (needSetup || needReload) ReloadGestureChildren(gestHB);
    /* Copy reference controller directly (preserves all PPtr references) */
    System.String refPath = UnityEditor.AssetDatabase.GetAssetPath(gestHB.gestureReferenceController);
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: refPath=" + refPath + " exists=" + System.IO.File.Exists(refPath));
    if (System.String.IsNullOrEmpty(refPath) || !System.IO.File.Exists(refPath))
    { UnityEngine.Debug.LogError("[HB] Cannot copy controller — reference path invalid: " + (refPath ?? "null")); return; }
    /* Use UnityEditor.AssetDatabase.CopyAsset to keep ADB fully in sync.
     * CopyAsset copies .meta (preserving GUID from reference) but the controller
     * is a template,not a shared asset,so GUID deduplication is fine.
     * RenameAsset renames the main object to match the filename. */
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: CopyAsset " + refPath + " -> " + ctrlPath);
    if (!UnityEditor.AssetDatabase.CopyAsset(refPath,ctrlPath))
    { UnityEngine.Debug.LogError("[HB] BakeGestureLayersToPath: CopyAsset failed"); return; }
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: CopyAsset done,renaming...");
    System.String ctrlName = System.IO.Path.GetFileNameWithoutExtension(ctrlPath);
    System.String renameResult = UnityEditor.AssetDatabase.RenameAsset(ctrlPath,ctrlName);
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: RenameAsset result='" + (renameResult ?? "") + "'");
    var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(ctrlPath);
    UnityEngine.Debug.Log("[HB] BakeGestureLayersToPath: Load result=" + (controller != null ? controller.name : "NULL"));
                                                          if (controller == null) { UnityEngine.Debug.LogError("[HB] Failed to load copied controller."); return; }
    /* === Clip replacement via UnityEditor.Animations.AnimatorController API === */
    foreach (UnityEngine.Transform layer in GetGestureLayers(gest.transform))
    { var lhb = layer.GetComponent<C_AviGenerator>();
    if (lhb == null || (lhb.mode != E_AviGeneratorMode.Expression_Layers
      && lhb.mode != E_AviGeneratorMode.ControllerBuilder)) continue;
    System.Boolean isIdle = layer.name.Contains("Idle") || layer.name.Contains("Additive");
    if (isIdle)
    { foreach (UnityEngine.Transform sub in layer.transform)
      { var shb = sub.GetComponent<C_AviGenerator>();
      if (shb?.mode != E_AviGeneratorMode.State || shb.animationClip == null) continue;
      System.String stateName = layer.name.Contains("Left") ? "Left Idle" : "Right Idle";
      foreach (var sm in controller.layers)
        if (sm.stateMachine != null)
        foreach (var s in sm.stateMachine.states)
          if (s.state.name == stateName) { s.state.motion = shb.animationClip; break; } } }
    else
    { foreach (UnityEngine.Transform finger in layer.transform)
      { var fhb = finger.GetComponent<C_AviGenerator>();
      if (fhb == null || !HasSt8(fhb,CbBlockType.StateMachine) && fhb.mode != E_AviGeneratorMode.StateMachine) continue;
      foreach (UnityEngine.Transform sub in finger.transform)
      { var shb = sub.GetComponent<C_AviGenerator>();
        if (shb?.mode != E_AviGeneratorMode.State || shb.animationClip == null) continue;
        foreach (var sm in controller.layers)
        if (sm.stateMachine != null)
          foreach (var s in sm.stateMachine.states)
          if (s.state.name == sub.name) { s.state.motion = shb.animationClip; break; } } } } }
    /* === Parameter sync === */
    if (gestHB.acParams != null && gestHB.acParams.Count > 0)
    { var toRemove =  new System.Collections.Generic.List<System.String>();
    foreach (var cp in controller.parameters)
    { System.Boolean found = false;
      foreach (var ap in gestHB.acParams) if (ap.name == cp.name) { found = true; break; }
      if (!found) toRemove.Add(cp.name); }
    foreach (var name in toRemove) controller.RemoveParameter(System.Linq.Enumerable.First(controller.parameters, p => p.name == name));
    foreach (var ap in gestHB.acParams)
    { System.Boolean exists = false;
      foreach (var cp in controller.parameters) if (cp.name == ap.name) { exists = true; break; }
      if (!exists) controller.AddParameter(ap.name,(UnityEngine.AnimatorControllerParameterType)ap.type); }
    UnityEditor.EditorUtility.SetDirty(controller); }
    /* === Layer settings sync === */
    for (int li = 0; li < controller.layers.Length; li++)
    { var cl = controller.layers[li];
    UnityEngine.Transform match = null;
    foreach (UnityEngine.Transform layer in GetGestureLayers(gest.transform))
    { var lhb = layer.GetComponent<C_AviGenerator>();
      if (lhb == null) continue;
      System.String ln = ResolveLayerName(layer.name);
      if (ln == cl.name) { match = layer; break; } }
    if (match == null) continue;
    var hlhb = match.GetComponent<C_AviGenerator>();
    System.Boolean isIdle = match.name.Contains("Idle") || match.name.Contains("Additive");
    UnityEngine.AvatarMask mask = hlhb != null ? hlhb.mask : null;
    if (mask == null && isIdle)
    { System.String maskFile = match.name.Contains("Left") ? "L_Hand.mask" : "R_Hand.mask";
      mask = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AvatarMask>(
      NZKPaths.DepRoot + "/GestureDefaults/" + maskFile); }
    cl.avatarMask = mask;
    cl.defaultWeight = hlhb != null ? hlhb.weight : (isIdle ? 0.33f : 1f);
    cl.blendingMode = isIdle ? UnityEditor.Animations.AnimatorLayerBlendingMode.Additive
                 : UnityEditor.Animations.AnimatorLayerBlendingMode.Override;
    controller.layers[li] = cl; }
    UnityEditor.EditorUtility.SetDirty(controller);
    UnityEditor.AssetDatabase.SaveAssets();
    UnityEditor.AssetDatabase.Refresh();
    UnityEngine.Debug.Log("[HB] Gesture layers baked (scene YAML): " + System.IO.Path.GetFileName(ctrlPath));
    /* === Sync hierarchy → YamlBlocks === */
    hb.BuildYamlBlocks();
    UnityEngine.Debug.Log("[HB] YamlBlocks synced from hierarchy.");
    /* === Save generator hierarchy as prefabs (non-fatal) === */
    System.String prefabBase = NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Controllers) + "/base/gestures";
    try
    { System.IO.Directory.CreateDirectory(prefabBase);
    foreach (UnityEngine.Transform layer in GetGestureLayers(gest.transform))
    { System.String prefabPath = prefabBase + "/" + layer.name + ".prefab";
      try { UnityEditor.PrefabUtility.SaveAsPrefabAsset(layer.gameObject,prefabPath); }
      catch (System.Exception ex) { UnityEngine.Debug.Log("[HB] Cannot save prefab " + prefabPath + ": " + ex.Message); }
      foreach (UnityEngine.Transform sub in layer.transform)
      { if (sub.name == "Entry" || sub.name == "AnyState" || sub.name == "Exit" || sub.name == "Up") continue;
      System.String subPath = prefabBase + "/" + layer.name + "/" + sub.name + ".prefab";
      var subDir = System.IO.Path.GetDirectoryName(subPath);
      System.IO.Directory.CreateDirectory(subDir);
      try { UnityEditor.PrefabUtility.SaveAsPrefabAsset(sub.gameObject,subPath); }
      catch (System.Exception ex) { UnityEngine.Debug.Log("[HB] Cannot save prefab " + subPath + ": " + ex.Message); } } }
    UnityEditor.AssetDatabase.Refresh();
    UnityEngine.Debug.Log("[HB] Gesture generator hierarchy saved as prefabs to " + prefabBase); }
    catch (System.Exception ex) { UnityEngine.Debug.LogWarning("[HB] Prefab save skipped: " + ex.Message); }
    /* === Merge additional sources into the generated controller === */
    var mergedCtrl = MergeControllerSources(gestHB,controller,ctrlPath);
    if (hb.NZKC_GO_AviRoot != null)
    VRCAD.Set.ControllerSlot(hb,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Gesture,mergedCtrl);
    /* Store on the generator for inspector visibility */
    gestHB.generatedController = mergedCtrl;
    UnityEditor.EditorUtility.SetDirty(gestHB); }
  /** <summary>Merge all mergeSources into the baked controller. */
  /** Returns the merged controller (same reference if no sources).</summary> */
  public static UnityEditor.Animations.AnimatorController MergeControllerSources(C_AviGenerator gestHB,UnityEditor.Animations.AnimatorController bakedCtrl,System.String ctrlPath)
  { if (gestHB.mergeSources == null || gestHB.mergeSources.Count == 0)
    return bakedCtrl;
    /* Filter to actual UnityEditor.Animations.AnimatorController sources */
    var sources = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Cast<UnityEditor.Animations.AnimatorController>(System.Linq.Enumerable.Where(gestHB.mergeSources, s => s != null && s is UnityEditor.Animations.AnimatorController)));
    if (sources.Length == 0) return bakedCtrl;
    UnityEngine.Debug.Log("[HB] Merging " + sources.Length + " source controller(s) into " +
    System.IO.Path.GetFileName(ctrlPath));
    /* Load or create the merger against the baked controller path */
    System.String mergedPath = System.IO.Path.GetDirectoryName(ctrlPath) + "/" +
    System.IO.Path.GetFileNameWithoutExtension(ctrlPath) + "_Merged.controller";
    var merged = ControllerMerger.MergeControllers(mergedPath,sources);
    if (merged != null)
    { UnityEngine.Debug.Log("[HB] Merge complete: " + mergedPath);
    return merged; }
    return bakedCtrl; }
  /* ---- Reload / Reset Gesture Children ---- */
  public static void ReloadGestureChildren(C_AviGenerator hb)
  { if (hb.mode != E_AviGeneratorMode.GestureGenerator && hb.mode != E_AviGeneratorMode.AnimatorBuilder && hb.mode != E_AviGeneratorMode.ControllerBuilder) return;
    /* Save YamlBlocks before destroying children */
    UnityEngine.Transform yamlBlocks = hb.transform.Find("YamlBlocks");
    DestroyAllGestureChildren(hb);
    SetupGesturesDefault(hb);
    if (hb.gestureReferenceController is UnityEditor.Animations.AnimatorController ac)
    { UnityEngine.Debug.Log("[HB] Importing gesture hierarchy from reference: " + ac.name);
    GestureLayerImporter.ImportFromExample(hb,ac);
    UnityEditor.EditorUtility.SetDirty(hb); return; }
    /* Fallback: generate from YAML blocks if available */
    if (yamlBlocks != null)
    { yamlBlocks.SetParent(hb.transform,false); // re-attach
    UnityEngine.Debug.Log("[HB] No reference controller — generating layers from existing YAML blocks.");
    hb.GenerateLayersFromYaml(hb);
    hb.SyncCbBlockTypeFromYaml();
    hb.BuildAXControllerFromHierarchy();
    UnityEditor.EditorUtility.SetDirty(hb);
    return; }
    UnityEngine.Debug.LogWarning("[HB] No gesture reference controller and no YAML blocks — cannot reload gesture children."); }
  static void DestroyAllGestureChildren(C_AviGenerator hb)
  { var preserve = new System.Collections.Generic.HashSet<System.String> { "YamlBlocks","HB_Generated","HB_Sources" };
    var toDelete =  new System.Collections.Generic.List<UnityEngine.GameObject>();
    foreach (UnityEngine.Transform c in hb.transform)
    if (!preserve.Contains(c.name)) toDelete.Add(c.gameObject);
    foreach (var d in toDelete) UnityEngine.Object.DestroyImmediate(d); }
  static UnityEditor.Animations.AnimatorConditionMode InvertAnimatorConditionMode(UnityEditor.Animations.AnimatorConditionMode mode)
  { switch (mode)
    { case UnityEditor.Animations.AnimatorConditionMode.Equals:   return UnityEditor.Animations.AnimatorConditionMode.NotEqual;
    case UnityEditor.Animations.AnimatorConditionMode.NotEqual: return UnityEditor.Animations.AnimatorConditionMode.Equals;
    case UnityEditor.Animations.AnimatorConditionMode.Greater:  return UnityEditor.Animations.AnimatorConditionMode.Less;
    case UnityEditor.Animations.AnimatorConditionMode.Less:   return UnityEditor.Animations.AnimatorConditionMode.Greater;
    default: return mode; } }
  static void AddControllerLayer(UnityEditor.Animations.AnimatorController ctrl,System.String n,UnityEditor.Animations.AnimatorStateMachine sm,float w,UnityEditor.Animations.AnimatorLayerBlendingMode bm,UnityEngine.AvatarMask m)
  { var layer = new UnityEditor.Animations.AnimatorControllerLayer
    { name = n,stateMachine = sm ?? new UnityEditor.Animations.AnimatorStateMachine(),defaultWeight = w,avatarMask = m,blendingMode = bm };
    ctrl.AddLayer(layer); }
  }
}
}
#endif
