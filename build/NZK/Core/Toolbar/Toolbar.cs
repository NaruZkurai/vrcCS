#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class Toolbar
  {
    /* ── PASTE / MERGE ───────────────────────────────────────────────── */
    [UnityEditor.MenuItem("GameObject/NZK/Paste Component as New", false, 31)]
    public static void PasteComponentAsNew()
    { if (RightClick._nzkCopiedComponent == null ||
          string.IsNullOrEmpty(RightClick._nzkCopiedComponentJson))
      { UnityEngine.Debug.LogWarning("[NZK] No component copied. Right-click a component → NZK Copy first."); return; }
      var targets = UnityEditor.Selection.gameObjects;
      if (targets == null || targets.Length == 0) return;
      var type = System.Type.GetType(RightClick._nzkCopiedComponentType);
      if (type == null) { UnityEngine.Debug.LogError("[NZK] Cannot find type: " + RightClick._nzkCopiedComponentType); return; }
      foreach (var go in targets)
      { if (go == null) continue;
        var existing = go.GetComponent(type);
        if (existing != null) { UnityEngine.Debug.Log("[NZK] " + go.name + " already has " + type.Name + " — skipped."); continue; }
        var added = go.AddComponent(type) as UnityEngine.Component;
        if (added != null) { UnityEditor.EditorJsonUtility.FromJsonOverwrite(RightClick._nzkCopiedComponentJson, added); UnityEditor.EditorUtility.SetDirty(go); }
        UnityEngine.Debug.Log("[NZK] Pasted " + type.Name + " onto " + go.name); } }
    [UnityEditor.MenuItem("GameObject/NZK/Paste Component as New", true)]
    public static System.Boolean ValidatePasteComponentAsNew()
    { return UnityEditor.Selection.activeGameObject != null &&
             RightClick._nzkCopiedComponent != null &&
             !string.IsNullOrEmpty(RightClick._nzkCopiedComponentJson); }
    /* The clipboard the two items above read lives on NZK.Core.RightClick,
       where the CONTEXT/Component/NZK Copy item writes it.  Both halves are
       public because a ban on private members leaves no other way for two files
       to share one clipboard - and the alternative (a second copy of the fields
       here) would give the user two clipboards that disagree. */
    [UnityEditor.MenuItem("GameObject/NZK/Merge without Children to Last Selected Selected", false, 32)]
    public static void MergeToLastSelected()
    { var objs = UnityEditor.Selection.gameObjects;
      if (objs == null || objs.Length < 2) return;
      /* The ACTIVE object is the target, not the last array element: the array's
         order follows the selection-sorting setting, not click order, so reading
         its tail merges into a different object on a different machine. */
      var target = UnityEditor.Selection.activeGameObject;
      if (target == null) target = objs[objs.Length - 1];
      if (target == null) return;
      var sources = new System.Collections.Generic.List<UnityEngine.GameObject>();
      for (int i = 0; i < objs.Length; i++) { if (objs[i] != null && objs[i] != target) sources.Add(objs[i]); }
      if (sources.Count == 0) return;
      ObjectMerge.Merge(sources.ToArray(), target); }
    [UnityEditor.MenuItem("GameObject/NZK/Merge without Children to Last Selected Selected", true)]
    public static System.Boolean ValidateMergeToLastSelected()
    { return UnityEditor.Selection.activeGameObject != null &&
             UnityEditor.Selection.gameObjects != null &&
             UnityEditor.Selection.gameObjects.Length > 1; }
    [UnityEditor.MenuItem("GameObject/NZK/Merge Including Children to Last Selected", false, 31)]
    public static void DeepMergeToLastSelected()
    { var objs = UnityEditor.Selection.gameObjects;
      if (objs == null || objs.Length < 2) return;
      var target = UnityEditor.Selection.activeGameObject;
      if (target == null) target = objs[objs.Length - 1];
      if (target == null) return;
      var sources = new System.Collections.Generic.List<UnityEngine.GameObject>();
      for (int i = 0; i < objs.Length; i++) { if (objs[i] != null && objs[i] != target) sources.Add(objs[i]); }
      if (sources.Count == 0) return;
      Systems.DeepMergeObjectsToTarget(sources.ToArray(), target); }
    [UnityEditor.MenuItem("GameObject/NZK/Merge Including Children to Last Selected", true)]
    public static System.Boolean ValidateDeepMergeToLastSelected()
    { return UnityEditor.Selection.activeGameObject != null &&
             UnityEditor.Selection.gameObjects != null &&
             UnityEditor.Selection.gameObjects.Length > 1; }
    /* ── BAKE ────────────────────────────────────────────────────────── */
    /** <summary>Every Bake submenu item, one dispatcher call each.</summary>
     *
     *  Grouped and written four-per-line because they are IDENTICAL in shape -
     *  the only varying parts are the menu path, the priority and the three
     *  dispatch strings.  Expanded into the previous one-line-per-item form
     *  they were 12 lines that had to be read one at a time to spot the one
     *  whose dispatch string disagreed with its menu path. */
    [UnityEditor.MenuItem("GameObject/NZK/Bake All", false, 40)]
    public static void BakeAllMenu() { Systems.MenuDO("Bake","All","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Bake All", true)]
    public static System.Boolean ValidateBakeAllMenu() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Mesh", false, 41)]
    public static void BakeMeshMenu() { Systems.MenuDO("Bake","Mesh","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Mesh", true)]
    public static System.Boolean ValidateBakeMeshMenu() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Armature", false, 42)]
    public static void BakeArmatureMenu() { Systems.MenuDO("Bake","Armature","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Armature", true)]
    public static System.Boolean ValidateBakeArmatureMenu() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Toggle Generator", false, 43)]
    public static void BakeToggleGenMenu() { Systems.MenuDO("Bake","Toggle","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Toggle Generator", true)]
    public static System.Boolean ValidateBakeToggleGenMenu() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/FX Layers", false, 44)]
    public static void BakeFxMenu() { Systems.MenuDO("Bake","Fx","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/FX Layers", true)]
    public static System.Boolean ValidateBakeFxMenu() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Expression Layers", false, 45)]
    public static void BakeExprMenu() { Systems.MenuDO("Bake","Expression","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Bake/Expression Layers", true)]
    public static System.Boolean ValidateBakeExprMenu() { return UnityEditor.Selection.activeGameObject != null; }
    /* ── MESH / GENERATE ─────────────────────────────────────────────── */
    [UnityEditor.MenuItem("GameObject/NZK/Merge Meshes", false, 47)]
    public static void MergeMeshesMenu() { Systems.MenuDO("Mesh","Merge","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Merge Meshes", true)]
    public static System.Boolean ValidateMergeMeshes() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Mesh/Fix 4 Bone Influences", false, 52)]
    public static void FixInfluencesMenu() { Systems.MenuDO("Mesh","Influences","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Mesh/Fix 4 Bone Influences", true)]
    public static System.Boolean ValidateFixInfluences() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Mesh/Audit (why is it invisible)", false, 53)]
    public static void AuditMeshesMenu() { Systems.MenuDO("Mesh","Audit","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Mesh/Audit (why is it invisible)", true)]
    public static System.Boolean ValidateAuditMeshes() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Generate/Menu", false, 34)]
    public static void GenerateMenu() { Systems.MenuDO("Generate","Menu","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Generate/Menu", true)]
    public static System.Boolean ValidateGenerateMenu() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Generate/DBT", false, 35)]
    public static void GenerateDBT() { Systems.MenuDO("Generate","DBT","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Generate/DBT", true)]
    public static System.Boolean ValidateGenerateDBT() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Generate Toggle/Blendshape", false, 48)]
    public static void GenerateToggleBlendshape() { Systems.MenuDO("Generate","Toggle","Blendshape",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Generate Toggle/Blendshape", true)]
    public static System.Boolean ValidateGenToggleBS() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Generate Toggle/NaNimation", false, 49)]
    public static void GenerateToggleNaN() { Systems.MenuDO("Generate","Toggle","NaN",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Generate Toggle/NaNimation", true)]
    public static System.Boolean ValidateGenToggleNaN() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/Generate Toggle/On Off", false, 50)]
    public static void GenerateToggleOnOff() { Systems.MenuDO("Generate","Toggle","onoff",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/Generate Toggle/On Off", true)]
    public static System.Boolean ValidateGenToggleOnOff() { return UnityEditor.Selection.activeGameObject != null; }
    /* ── VRCFURY ─────────────────────────────────────────────────────── */
    [UnityEditor.MenuItem("GameObject/NZK/VF/Bake SPS Sockets", false, 43)]
    public static void BakeSpsSocketsMenu() { Systems.MenuDO("VF","BakeSps","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/VF/Bake SPS Sockets", true)]
    public static System.Boolean ValidateBakeSpsSockets() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/VF/Bake Armature Link", false, 45)]
    public static void BakeArmatureLinkMenu() { Systems.MenuDO("VF","BakeArmatureLink","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/VF/Bake Armature Link", true)]
    public static System.Boolean ValidateBakeArmatureLink() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/VF/Bake Full Controller", false, 46)]
    public static void BakeFullControllerMenu() { Systems.MenuDO("VF","BakeFullController","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/VF/Bake Full Controller", true)]
    public static System.Boolean ValidateBakeFullController() { return UnityEditor.Selection.activeGameObject != null; }
    /* ── NANIMATION ──────────────────────────────────────────────────── */
    [UnityEditor.MenuItem("GameObject/NZK/NaNimation/Fix Bone SMR NaN Relations", false, 51)]
    public static void RelinkNanimationBonesMenu() { Systems.MenuDO("NaNimation","Relink","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/NaNimation/Fix Bone SMR NaN Relations", true)]
    public static System.Boolean ValidateRelinkNanimationBones() { return UnityEditor.Selection.activeGameObject != null; }
    /* ── SPS (native, no VRCFury required) ───────────────────────────── */
    /** <summary>Create an SPS socket as a child of each selected object.
     *
     *  Parented to the SELECTED object, not to the avatar root, because the
     *  socket's transform is what the contact sender reads - a socket on the
     *  root would sit at the avatar's feet and the hand/plug contacts would
     *  never line up.  Undo.RegisterCreatedObjectUndo is called before the
     *  component is configured so one Ctrl+Z removes the whole socket rather
     *  than leaving a bare GameObject behind.</summary> */
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Create Socket", false, 30)]
    public static void CreateSpsSocket()
    { var objs = UnityEditor.Selection.gameObjects; if (objs == null || objs.Length == 0) return;
      int n = 0;
      foreach (var go in objs) { if (go == null) continue;
        var child = new UnityEngine.GameObject(go.name + "_SPS_Socket");
        child.transform.SetParent(go.transform,false);
        UnityEditor.Undo.RegisterCreatedObjectUndo(child,"Create SPS Socket child");
        var socket = child.AddComponent<C_NzkSpsSocket>();
        socket.name = child.name; socket.enableAuto = true; socket.V_addLight = C_NzkSpsSocket.E_AddLight.Auto;
        RightClick.EnsureSpsGenerator(go);
        SpsSocketBaker.BakeSingle(child); n++;
        UnityEngine.Debug.Log("[NZK] SPS Socket created as child of " + go.name); }
      if (n > 0) { var root = objs[0].transform.root.name;
        UnityEngine.Debug.Log("[NZK] SPS Socket Created: " + n + " socket(s) on \"" + root + "\"."); } }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Create Socket", true)]
    public static System.Boolean ValidateCreateSpsSocket() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Create Plug", false, 31)]
    public static void CreateSpsPlug()
    { var objs = UnityEditor.Selection.gameObjects; if (objs == null || objs.Length == 0) return;
      int n = 0;
      foreach (var go in objs) { if (go == null) continue;
        var child = new UnityEngine.GameObject(go.name + "_SPS_Plug");
        child.transform.SetParent(go.transform,false);
        UnityEditor.Undo.RegisterCreatedObjectUndo(child,"Create SPS Plug child");
        var plug = child.AddComponent<C_NzkSpsSocket>();
        plug.name = child.name; plug.enableAuto = false; plug.V_addLight = C_NzkSpsSocket.E_AddLight.None;
        plug.length = 0.1f; plug.position = new UnityEngine.Vector3(0,0,0.05f);
        RightClick.EnsureSpsGenerator(go);
        SpsSocketBaker.BakeSingle(child); n++;
        UnityEngine.Debug.Log("[NZK] SPS Plug created as child of " + go.name); }
      if (n > 0) { var root = objs[0].transform.root.name;
        UnityEngine.Debug.Log("[NZK] SPS Plug Created: " + n + " plug(s) on \"" + root + "\"."); } }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Create Plug", true)]
    public static System.Boolean ValidateCreateSpsPlug() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Bake", false, 32)]
    public static void BakeSpsNative() { Systems.MenuDO("SPS","Bake","",UnityEditor.Selection.gameObjects); }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Bake", true)]
    public static System.Boolean ValidateBakeSpsNative() { return UnityEditor.Selection.activeGameObject != null; }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Clear All Prefab Caches", false, 33)]
    public static void ClearAllSpsCaches()
    { var generators = UnityEngine.Resources.FindObjectsOfTypeAll<C_SpsSocketGenerator>();
      int n = 0;
      foreach (var gen in generators)
      { if (gen == null || gen.gameObject == null || !gen.gameObject.scene.isLoaded) continue;
        var p = gen.PrefabsRoot; for (int i = p.childCount - 1; i >= 0; i--) { UnityEngine.Object.DestroyImmediate(p.GetChild(i).gameObject); n++; } }
      UnityEngine.Debug.Log("[NZK] Cleared " + n + " SPS prefab cache(s).");
      UnityEngine.Debug.Log("[NZK] SPS Cache: Cleared " + n + " cached prefab(s)."); }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Clear All Prefab Caches", true)]
    public static System.Boolean ValidateClearAllSpsCaches() { return true; }
    /* ── VRCFURY SCRIPT CONVERSION (bulk, multi-select) ──────────────── */
    /** <summary>VRCFury script GUID → feature name, for missing-script recovery.
     *
     *  Used when VRCFury is NOT installed: the components are missing, so their
     *  TYPE cannot be read, but the GUID survives in the serialised scene as
     *  `m_Script: {fileID: 11500000, guid: <here>}`.  That GUID is the only
     *  thing left identifying what the component was, so the table below is
     *  what turns an empty MonoBehaviour slot back into a named feature.
     *  Keys are VRCFury's own script GUIDs and must not be edited.</summary> */
    public static readonly System.Collections.Generic.Dictionary<System.String,System.String> VfGuids = new()
    { {"703106f8586c4f64b5aa39b6b4676684","HapticSocket"},
      {"2d4cd252f8c146639507ecd55bc2b48a","HapticPlug"},
      {"d9e94e501a2d4c95bff3d5601013d923","MainVRCFury"},
      {"f70fb861c8004cd4b5af94fd3fd7da0e","HapticTouchReceiver"},
      {"26089cfa1cdd4901ad7650f312669353","HapticTouchSender"},
      {"c87b57302b5d4e1bb6519405620b239c","VRCFuryComponent"},
      {"8028d33728ef49f5b9d44b564480aeff","GlobalCollider"},
      {"973a3360b5c04a279549138b3d548290","HideGizmo"},
      {"325fd324ef90492c8b08804b6f62cc0d","NoUpdateWhenOffscreen"},
      {"87a8ddd6c72c4860af91d53d67f5955e","PlayComponent"},
      {"a93df76833a04889b400fea362da558c","SocketGizmo"},
      {"62488ce102aa93e4b82c4e864ce2d8f4","SpsGreenScreenFix"} };
    /** <summary>Convert every VRCFury component under the selection to its NZK
     *  equivalent, in three passes.
     *
     *  Three passes because the three cases need different evidence and cannot
     *  be told apart in one walk:
     *    1. VRCFury INSTALLED   - the type is readable, match on name.
     *    2. VRCFury MISSING     - the component is a missing script; the only
     *                             evidence is the GUID inside the serialised
     *                             JSON, so the object is string-matched here.
     *    3. ALREADY BAKED       - a previous run baked SPS contact senders but
     *                             left no component to convert; the socket root
     *                             is found by walking UP from a sender whose
     *                             collisionTags carry an SPS prefix.
     *
     *  Passes 2 and 3 both mutate the hierarchy, so each pass re-reads the
     *  children rather than reusing a list gathered at the start - a list
     *  gathered up front would hold components a previous pass already
     *  destroyed.  `done` is what stops the passes from re-processing the same
     *  object as each new walk sees it.</summary> */
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Convert VRCFury Scripts to NZK", false, 29)]
    public static void ConvertVfScriptsOnSelection()
    { var objs = UnityEditor.Selection.gameObjects; if (objs == null || objs.Length == 0) return;
      int n = 0; var done = new System.Collections.Generic.HashSet<UnityEngine.GameObject>();
      foreach (var go in objs)
      { if (go == null) continue;
        /* Phase 1: Known VF types (VRCFury installed) */
        var allComps = go.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true);
        foreach (var c in allComps)
        { if (c == null) continue; if (done.Contains(c.gameObject)) continue;
          var fn = c.GetType().FullName ?? "";
          if (!fn.Contains("VRCFury") && !fn.StartsWith("VF.")) continue;
          done.Add(c.gameObject);
          if (fn.Contains("HapticSocket") || c.GetType().Name.Contains("HapticSocket"))
          { if (c.gameObject.GetComponent<C_NzkSpsSocket>() == null)
            { var nzk = c.gameObject.AddComponent<C_NzkSpsSocket>();
              RightClick.CopySpsProperties(c,nzk);
              UnityEditor.Undo.RegisterCreatedObjectUndo(nzk,"Convert VF\u2192NZK");
              RightClick.EnsureSpsGenerator(c.gameObject); SpsSocketBaker.BakeSingle(c.gameObject); n++; }
            UnityEditor.Undo.DestroyObjectImmediate(c); }
          else
          { var vfType = c.GetType().Name;
            VfConversion.ConvertOrBackup(c.gameObject,c,vfType);
            n++; } }
        /* Phase 2: Missing scripts (VF not installed) - detect by serialized JSON */
        var allChildren = go.GetComponentsInChildren<UnityEngine.Transform>(true);
        foreach (var t in allChildren)
        { if (t == null || done.Contains(t.gameObject)) continue;
          if (t.gameObject.GetComponent<C_NzkSpsSocket>() != null) continue;
          var comps = t.gameObject.GetComponents<UnityEngine.Component>();
          bool hasMissing = false;
          foreach (var comp in comps) { if (comp == null) { hasMissing = true; break; } }
          if (!hasMissing) continue;
          var json = UnityEditor.EditorJsonUtility.ToJson(t.gameObject);
          if (string.IsNullOrEmpty(json)) continue;
          System.String vfType = null;
          foreach (var kv in VfGuids)
          { if (json.Contains(kv.Key)) { vfType = kv.Value; break; } }
          if (vfType == null) continue;
          UnityEngine.Debug.Log("[NZK] Detected missing VF: " + vfType + " on " + t.gameObject.name);
          if (vfType != "HapticSocket" && vfType != "HapticPlug")
          { var folder = VfConversion.StorageRoot + "/" + Vars.Names.Sanitize(t.root.name) + "/" + VfConversion.VfBackupFolder;
            Systems.Folder.Ensure(folder);
            var safe = Vars.Names.Sanitize(t.gameObject.name) + "_" + vfType;
            var path = folder + "/" + safe + ".json";
            System.String pGuid = "";
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(t.gameObject))
            { var pRoot = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
              if (pRoot != null) { var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(pRoot);
                if (prefab != null) { var pp = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                  pGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(pp).ToString(); } } }
            var header = "# PREFAB_GUID:" + pGuid + "\n# OBJECT:" + t.gameObject.name + "\n# TYPE:" + vfType + "\n";
            System.IO.File.WriteAllText(path,header + json); UnityEditor.AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[NZK] Backed up missing VF: " + vfType + " on " + t.gameObject.name + " → " + path + " (prefab: " + (pGuid != "" ? pGuid : "none") + ")");
            UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject); n++; done.Add(t.gameObject); continue; }
          var nzk = t.gameObject.AddComponent<C_NzkSpsSocket>();
          try
          { if (json.Contains("\"addLight\":")) { var idx = json.IndexOf("\"addLight\":"); var val = json.Substring(idx + 11); val = val.Substring(0,val.IndexOf(',')).Trim(); int.TryParse(val,out int al); if (al>=0&&al<=4) nzk.V_addLight=(C_NzkSpsSocket.E_AddLight)al; }
            if (json.Contains("\"name\":\"")) { var idx=json.IndexOf("\"name\":\""); var val=json.Substring(idx+8); val=val.Substring(0,val.IndexOf('\"')); if(!string.IsNullOrEmpty(val))nzk.name=val; }
            if (json.Contains("\"oscId\":\"")) { var idx=json.IndexOf("\"oscId\":\""); var val=json.Substring(idx+9); val=val.Substring(0,val.IndexOf('\"')); nzk.oscId=val; }
            if (json.Contains("\"length\":")) { var idx=json.IndexOf("\"length\":"); var val=json.Substring(idx+9); val=val.Substring(0,val.IndexOf(',')); float.TryParse(val,out nzk.length); }
            if (json.Contains("\"enableAuto\"")) nzk.enableAuto=true;
            if (json.Contains("\"addMenuItem\"")) nzk.addMenuItem=true; }
          catch { /* JSON parse errors are non-fatal */ }
          if (System.String.IsNullOrEmpty(nzk.name)) nzk.name=t.gameObject.name;
          UnityEditor.Undo.RegisterCreatedObjectUndo(nzk,"Convert missing\u2192NZK");
          UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
          RightClick.EnsureSpsGenerator(t.gameObject); SpsSocketBaker.BakeSingle(t.gameObject); n++; done.Add(t.gameObject); }
        /* Phase 3: Already-baked VF sockets */
        var allChildren3 = go.GetComponentsInChildren<UnityEngine.Transform>(true);
        foreach (var t in allChildren3)
        { if (t == null || done.Contains(t.gameObject)) continue;
          bool hasSpsTag = false;
          foreach (var cs in t.gameObject.GetComponents<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>())
          { if (cs == null || cs.collisionTags == null) continue;
            foreach (var tag in cs.collisionTags)
            { if (!string.IsNullOrEmpty(tag) && (tag.StartsWith("SPSLL_") || tag.StartsWith("TPS_") || tag.StartsWith("DPS_")))
              { hasSpsTag = true; break; } }
            if (hasSpsTag) break; }
          if (!hasSpsTag) continue;
          var socketRoot = t.parent;
          while (socketRoot != null && (socketRoot.name == "Senders" || socketRoot.name == "Lights" || socketRoot.name == "Envelope"))
            socketRoot = socketRoot.parent;
          if (socketRoot == null || done.Contains(socketRoot.gameObject)) continue;
          if (socketRoot.GetComponent<C_NzkSpsSocket>() != null) continue;
          int vfChildCount = 0;
          for (int i = 0; i < socketRoot.childCount; i++)
          { var cn = socketRoot.GetChild(i).name;
            if (cn.Contains("Sender") || cn == "Root" || cn == "Front" || cn == "Length" || cn == "WidthHelper" || cn == "Envelope")
              vfChildCount++; }
          if (vfChildCount < 2) continue;
          UnityEngine.Debug.Log("[NZK] Found baked VF socket root: " + socketRoot.name);
          var nzk = socketRoot.gameObject.AddComponent<C_NzkSpsSocket>();
          nzk.name = socketRoot.name;
          UnityEditor.Undo.RegisterCreatedObjectUndo(nzk,"Detect baked VF\u2192NZK");
          RightClick.EnsureSpsGenerator(socketRoot.gameObject); SpsSocketBaker.BakeSingle(socketRoot.gameObject); n++; done.Add(socketRoot.gameObject); } }
      if (n > 0) UnityEngine.Debug.Log("[NZK] Convert VF Scripts: Converted/removed " + n + " VRCFury component(s).");
      else UnityEngine.Debug.Log("[NZK] No VRCFury SPS sockets found on selection."); }
    [UnityEditor.MenuItem("GameObject/NZK/SPS/Convert VRCFury Scripts to NZK", true)]
    public static System.Boolean ValidateConvertVfScriptsOnSelection() { return UnityEditor.Selection.activeGameObject != null; }
  }
}
}
#endif
