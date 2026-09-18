#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
 /* //TODO: meshes.cs - right click split meshes by material,save to storage + master file (same system as NaNimate) */
  public static void ForEach(System.Collections.Generic.IEnumerable<System.String> names,System.Action<System.String> fn) { foreach (System.String n in names) fn(n); }
  /* ── Merge multiple GameObjects into a single target ────────── */
  public static void MergeObjectsToTarget(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
  { if (sources == null || target == null || sources.Length == 0) return;
    if (!MergePrefabCheck(sources,target)) return;
    int childEstimate = 0; foreach (var s in sources) if (s != null) childEstimate += s.transform.childCount;
    if (childEstimate > 500) UnityEngine.Debug.Log("[NZK] Large Merge: " + childEstimate + " children — proceeding.");
    UnityEditor.Undo.SetCurrentGroupName("Merge to Last Selected");
    int group = UnityEditor.Undo.GetCurrentGroup();
    UnityEditor.Undo.RegisterFullObjectHierarchyUndo(target,"Merge target");
    var targetTransform = target.transform;
    foreach (var src in sources)
    { if (src == null || src == target) continue;
      UnityEditor.Undo.RegisterFullObjectHierarchyUndo(src,"Merge source");
      var srcT = src.transform;
      while (srcT.childCount > 0)
      { var child = srcT.GetChild(0);
        UnityEditor.Undo.SetTransformParent(child,targetTransform,"Move child"); } }
    MergeReplaceReferences(sources,target);
    MergeMoveComponents(sources,target);
    foreach (var src in sources)
    { if (src == null || src == target) continue;
      UnityEditor.Undo.DestroyObjectImmediate(src); }
    UnityEditor.Undo.CollapseUndoOperations(group); }
  /* Deep merge aka Merge Including Children: recursively merges children with matching names in the target hierarchy */
  public static void DeepMergeObjectsToTarget(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
  { if (sources == null || target == null || sources.Length == 0) return;
    if (!MergePrefabCheck(sources,target)) return;
    int childEstimate = 0; foreach (var s in sources) if (s != null) CountDeep(s.transform,ref childEstimate);
    if (childEstimate > 500) UnityEngine.Debug.Log("[NZK] Large Deep Merge: " + childEstimate + " nodes — proceeding.");
    UnityEditor.Undo.SetCurrentGroupName("Deep Merge to Last Selected");
    int group = UnityEditor.Undo.GetCurrentGroup();
    UnityEditor.Undo.RegisterFullObjectHierarchyUndo(target,"Deep merge target");
    foreach (var src in sources)
    { if (src == null || src == target) continue;
      UnityEditor.Undo.RegisterFullObjectHierarchyUndo(src,"Deep merge source");
      DeepMergeNode(src.transform,target.transform); }
    MergeReplaceReferences(sources,target);
    foreach (var src in sources)
    { if (src == null || src == target) continue;
      UnityEditor.Undo.DestroyObjectImmediate(src); }
    UnityEditor.Undo.CollapseUndoOperations(group); }
  static void CountDeep(UnityEngine.Transform t,ref int count) { count++; for (int i = 0; i < t.childCount; i++) CountDeep(t.GetChild(i),ref count); }
  static System.Boolean MergePrefabCheck(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
  { foreach (var s in sources) { if (s == null) continue;
      if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(s))
      { var root = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(s);
        if (root == null) root = s;
        UnityEngine.Debug.Log("[NZK] Prefab Instance Detected: " + s.name + " is part of a prefab, unpacking root " + root.name + ".");
        UnityEditor.PrefabUtility.UnpackPrefabInstance(root,UnityEditor.PrefabUnpackMode.Completely,UnityEditor.InteractionMode.UserAction); } }
    /* ── Target keeps its prefab link untouched — no unpacking, no applying ── */
    return true; }
  static void DeepMergeNode(UnityEngine.Transform src,UnityEngine.Transform dst)
  { /* ── Merge components from src to dst ── */
    MergeComponents(src,dst);
    /* ── BFS through all descendants of src, merge by relative path into dst ── */
    var allDesc = new System.Collections.Generic.List<UnityEngine.Transform>();
    var queue = new System.Collections.Generic.Queue<UnityEngine.Transform>(); queue.Enqueue(src);
    while (queue.Count > 0) { var n = queue.Dequeue(); for (int i = 0; i < n.childCount; i++) { var c = n.GetChild(i); allDesc.Add(c); queue.Enqueue(c); } }
    var merged = new System.Collections.Generic.HashSet<UnityEngine.Transform>();
    foreach (var desc in allDesc)
    { if (desc == null || merged.Contains(desc)) continue;
      var relPath = UnityEditor.AnimationUtility.CalculateTransformPath(desc,src);
      var dstMatch = dst.Find(relPath);
      if (dstMatch != null)
      { MergeComponents(desc,dstMatch); merged.Add(desc);
        /* Children of this matched node that don't have matches should move to dstMatch */
        var descKids = new System.Collections.Generic.List<UnityEngine.Transform>();
        for (int i = 0; i < desc.childCount; i++) descKids.Add(desc.GetChild(i));
        foreach (var dk in descKids)
        { var dkRel = UnityEditor.AnimationUtility.CalculateTransformPath(dk,src);
          if (dst.Find(dkRel) == null) { UnityEditor.Undo.SetTransformParent(dk,dstMatch,"Move unmatched child"); merged.Add(dk); } } } }
    /* ── Move every unmatched node to its closest matched ancestor's dst counterpart ── */
    foreach (var desc in allDesc)
    { if (desc == null || merged.Contains(desc)) continue;
      /* Walk up to find first merged ancestor */
      var cur = desc.parent; UnityEngine.Transform mergeParent = null;
      while (cur != null) { if (cur == src) { mergeParent = dst; break; } if (merged.Contains(cur)) { var cp = UnityEditor.AnimationUtility.CalculateTransformPath(cur,src); mergeParent = dst.Find(cp); break; } cur = cur.parent; }
      if (mergeParent != null) { UnityEditor.Undo.SetTransformParent(desc,mergeParent,"Move unmatched subtree"); merged.Add(desc); } }
    /* ── Final safety: move any stragglers ── */
    for (int i = src.childCount - 1; i >= 0; i--) { var r = src.GetChild(i); if (r != null && !merged.Contains(r)) { UnityEditor.Undo.SetTransformParent(r,dst,"Move straggler"); } } }
  static void MergeComponents(UnityEngine.Transform src,UnityEngine.Transform dst)
  { var comps = src.GetComponents<UnityEngine.Component>();
    foreach (var c in comps)
    { if (c == null || c is UnityEngine.Transform) continue;
      var ct = c.GetType(); var ex = dst.GetComponent(ct);
      if (ex == null) { var nc = dst.gameObject.AddComponent(ct); UnityEditor.Undo.RegisterCreatedObjectUndo(nc,"Add merged component"); UnityEditor.EditorUtility.CopySerialized(c,nc); }
      else { UnityEditor.Undo.RecordObject(ex,"Update component"); UnityEditor.EditorUtility.CopySerialized(c,ex); }
      UnityEngine.Object.DestroyImmediate(c); } }
  static void MergeReplaceReferences(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
  { /* Delegated to ObjectMerge.  The version that lived here tested
       `objectReferenceValue is UnityEngine.GameObject`, so a reference to a
       COMPONENT was never rewritten and kept pointing at the component this
       merge was about to destroy - physbones, constraints, material links and
       toggles all reference components, not objects.  ObjectMerge rewrites
       both kinds and re-points a component reference at the target's component
       of the SAME TYPE, because a typed serialized field will not accept a
       GameObject in its place. */
    var work = new System.Collections.Generic.List<UnityEngine.GameObject>();
    if (!NZK.B.mpty.t(sources))
    { foreach (var s in sources) { if (s != null && s != target && !work.Contains(s)) work.Add(s); } }
    if (work.Count == 0) return;
    NZK.Core.ObjectMerge.MergeReferencesFor(work,target); }
  static void MergeMoveComponents(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
  { /* Delegated to ObjectMerge, which copies BEFORE destroying.  The version
       that lived here destroyed unconditionally, so a component the target
       could not accept was deleted with nothing replacing it and the object
       came out of the merge holding less than it went in. */
    if (NZK.B.mpty.t(sources)) return;
    foreach (var src in sources) { if (src != null) NZK.Core.ObjectMerge.MoveComponents(src,target); } }
  public static void MenuDO(System.String cat1,System.String cat2,System.String fn,UnityEngine.GameObject[] objs,System.Boolean silent = false)
  { /* ── Pre-validate: filter nulls, fallback to active selection ── */
    if (objs == null || objs.Length == 0)
    { if (UnityEditor.Selection.activeGameObject != null) objs = new[] { UnityEditor.Selection.activeGameObject };
      else { Systems.Warnings.NoObjects(cat1 + "/" + cat2,silent); return; } }
    var valid = new System.Collections.Generic.List<UnityEngine.GameObject>(objs.Length);
    foreach (var o in objs) if (o != null) valid.Add(o);
    if (valid.Count == 0) { Systems.Warnings.NoObjects(cat1 + "/" + cat2,silent); return; }
    objs = valid.ToArray();
    /* ── Route table ───────────────────────────────────────────── */
    if (cat1 == "Generate" && cat2 == "Toggle") { Systems.Toggle.Create(fn,objs); Systems.MenuSuccess("Generate Toggle","Toggle(s) created for " + System.String.Join(", ",System.Array.ConvertAll(objs,o => o.name)) + "."); return; }
    if (cat1 == "Generate" && cat2 == "Menu") { var avatars = Systems.Avatar.From(objs); foreach (var a in avatars) NaNimate.Update.Sync(a); Systems.MenuSuccess("Generate Menu","Regenerated menu for " + System.String.Join(", ",avatars) + "."); return; }
    if (cat1 == "Generate" && cat2 == "DBT") { var avatars = Systems.Avatar.From(objs); foreach (var a in avatars) NaNimate.Update.DBTs(a); Systems.MenuSuccess("Generate DBT","Rebuilt blend tree hierarchy for " + System.String.Join(", ",avatars) + "."); return; }
    if (cat1 == "Bake" && cat2 == "All") { int n = 0; foreach (var go in objs) { var hb = AviGeneratorRegistry.Get(go) ?? go.GetComponentInChildren<C_AviGenerator>(); if (hb != null) { AVController.BakeAll(hb); n++; } } Systems.MenuSuccess("Bake All","Baked " + n + " avatar(s)."); return; }
    if (cat1 == "Bake" && cat2 == "Mesh") { int n = 0; foreach (var go in objs) { var hb = AviGeneratorRegistry.Get(go) ?? go.GetComponentInChildren<C_AviGenerator>(); if (hb != null) { MergeCore.BakeMesh(hb); n++; } } Systems.MenuSuccess("Bake Mesh","Baked mesh for " + n + " avatar(s)."); return; }
    if (cat1 == "Bake" && cat2 == "Armature") { int n = 0; foreach (var go in objs) { var hb = AviGeneratorRegistry.Get(go) ?? go.GetComponentInChildren<C_AviGenerator>(); if (hb != null) { AVController.BakeArmatureFinal(hb); n++; } } Systems.MenuSuccess("Bake Armature","Baked armature for " + n + " avatar(s)."); return; }
    if (cat1 == "Bake" && cat2 == "Toggle") { int n = 0; foreach (var go in objs) { var hb = AviGeneratorRegistry.Get(go) ?? go.GetComponentInChildren<C_AviGenerator>(); if (hb != null) { AVController.BakeToggleGenerator(hb); n++; } } Systems.MenuSuccess("Bake Toggle Generator","Generated toggles for " + n + " avatar(s)."); return; }
    if (cat1 == "Bake" && cat2 == "Fx") { int n = 0; foreach (var go in objs) { var hb = AviGeneratorRegistry.Get(go) ?? go.GetComponentInChildren<C_AviGenerator>(); if (hb != null) { AVController.BakeFxLayers(hb); n++; } } Systems.MenuSuccess("Bake FX Layers","Baked FX for " + n + " avatar(s)."); return; }
    if (cat1 == "Bake" && cat2 == "Expression") { int n = 0; foreach (var go in objs) { var hb = AviGeneratorRegistry.Get(go) ?? go.GetComponentInChildren<C_AviGenerator>(); if (hb != null) { AVController.BakeExpressionLayers(hb); n++; } } Systems.MenuSuccess("Bake Expression Layers","Baked expressions for " + n + " avatar(s)."); return; }
    if (cat1 == "Mesh" && cat2 == "Merge") { var mr = MeshesMerge.MergeSelection(); if (mr.Success) { NZK.E.C.d(57,mr.Merged.Count); Systems.MenuSuccess("Merge Meshes","Merged " + mr.Merged.Count + " mesh(es)" + (mr.Skipped > 0 ? " (" + mr.Skipped + " skipped)" : "") + "."); } else { NZK.E.C.e(52,mr.ErrorMessage); } return; }
    if (cat1 == "Mesh" && cat2 == "Influences") { var ir = MeshImport.ForceFour(objs); Systems.MenuSuccess("Fix 4 Bone Influences","models=" + ir.Inspected + " changed=" + ir.Changed + " already4=" + ir.Already + (ir.Unsupported > 0 ? " unsupported=" + ir.Unsupported : "") + "."); return; }
    if (cat1 == "Mesh" && cat2 == "Audit") { var ar = MeshAudit.Run(objs); Systems.MenuSuccess("Mesh Audit","renderers=" + ar.Renderers + " empty=" + ar.EmptyMeshes + " zeroBounds=" + ar.ZeroBounds + " nanBounds=" + ar.NaNBounds + " nanVerts=" + ar.NaNVertices + " unweighted=" + ar.Unweighted + " badScale=" + ar.BadScales + ". See the Console for the per-mesh detail."); return; }
    if (cat1 == "SPS" && cat2 == "Bake") { int n = 0; foreach (var go in objs) { SpsSocketBaker.Bake(go); n++; } Systems.MenuSuccess("SPS Bake","Baked " + n + " socket(s)."); return; }
    if (cat1 == "SPS" && cat2 == "Sockets") { int n = 0; foreach (var go in objs) { SpsSocketBaker.Bake(go); n++; } Systems.MenuSuccess("SPS Sockets","Baked " + n + " socket(s)."); return; }
    if (cat1 == "VF" && cat2 == "BakeToggle") { int n = 0; foreach (var go in objs) { Systems.Baking.BakeToggleFromVrcfury(go); n++; } Systems.MenuSuccess("Bake Toggle (VF)","Processed " + n + " object(s)."); return; }
    if (cat1 == "VF" && cat2 == "BakeArmatureLink") { int n = 0; foreach (var go in objs) { Systems.Baking.BakeArmatureLink(go,true); n++; } Systems.MenuSuccess("Bake Armature Link","Processed " + n + " object(s)."); return; }
    if (cat1 == "VF" && cat2 == "BakeFullController") { int n = 0; foreach (var go in objs) { Systems.Baking.BakeFullController(go); n++; } Systems.MenuSuccess("Bake Full Controller","Processed " + n + " object(s)."); return; }
    if (cat1 == "NaNimation" && cat2 == "Relink") { int n = NZK.Core.NanRelink.RelinkSelection(objs); Systems.MenuSuccess("Fix Bone SMR NaN Relations","Rebuilt nanimation weight relations for " + n + " vertex/vertices."); return; }
    if (cat1 == "VF" && cat2 == "BakeSps") { int n = 0; foreach (var go in objs) { SpsSocketBaker.Bake(go); n++; } Systems.MenuSuccess("Bake SPS (VF)","Baked " + n + " socket(s)."); return; }
    UnityEngine.Debug.LogWarning("[MenuDO] Unknown command: " + cat1 + "/" + cat2 + " (" + fn + ")"); }
  
  public static void MenuSuccess(System.String title,System.String msg) { UnityEngine.Debug.Log("[NZK] " + title + ": " + msg); }
  // ── Shared playable layer assignment ──────────────────────────────
  // Used by both C_AviGenerator and the FC baker to assign controllers
  // to VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.baseAnimationLayers[] and specialAnimationLayers[].
  // CustomAnimLayer is a struct — this utility handles the struct copy properly.
  
  public static void CreateFileObject(System.String path)
  { if (System.String.IsNullOrEmpty(path)) return;
    System.String abs = NaNimate.Paths.Abs(path);
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(abs));
    if (!System.IO.File.Exists(abs)) { System.IO.File.WriteAllText(abs,""); }
    if (!UnityEditor.AssetDatabase.IsValidFolder(System.IO.Path.GetDirectoryName(path).Replace('\\','/')))
    { UnityEditor.AssetDatabase.ImportAsset(System.IO.Path.GetDirectoryName(path).Replace('\\','/'),UnityEditor.ImportAssetOptions.ForceSynchronousImport); } }
  
  
  
  
  
  
  public static void RegenerateFxParamsMenuSelection() { foreach (System.String a in Systems.Avatar.From(UnityEditor.Selection.gameObjects)) NaNimate.Update.Sync(a); UnityEditor.AssetDatabase.SaveAssets(); }
  
  
  
  
  
  // Runtime stub — real impl in UnityEditor.Editor assembly
  
  
  
}
}
}
#endif
