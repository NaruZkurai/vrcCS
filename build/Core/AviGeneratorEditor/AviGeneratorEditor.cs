#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.CustomEditor(typeof(C_AviGenerator))]
  public class AviGeneratorEditor : UnityEditor.Editor
  {
  C_AviGenerator hb;
  UnityEditor.SerializedProperty sMd,sAN,sAR,sCl,sReb,sAg,sArm,sMs,sEn,sMg,sSh,sFm,sUfv,sOrig,sAv,sTG,sBS,sIBT;
  UnityEditor.SerializedProperty sBSS,sNAS,sOTS,sUAC,sOffClip,sIsTC;
  UnityEditor.SerializedProperty sUS;
  UnityEditor.SerializedProperty sToggleSourceType,sToggleSourceMode,sToggleDisplayName;
  UnityEditor.SerializedProperty sToggleSourceObjects,sToggleBlendshapeName;
  UnityEditor.SerializedProperty sOnConditions,sOffConditions;
  UnityEditor.SerializedProperty sIgnored;
  static void Sec(System.String l,System.String t = null)
  { UnityEditor.EditorGUILayout.Space(4); UnityEditor.EditorGUILayout.LabelField(l,UnityEditor.EditorStyles.boldLabel);
    if (t != null) UnityEditor.EditorGUILayout.HelpBox(t,UnityEditor.MessageType.Info); UnityEditor.EditorGUILayout.Space(2); }
  static System.Boolean Bt(System.String l,System.String t = null,UnityEngine.Color? c = null,float h = 24f)
  { var p = UnityEngine.GUI.backgroundColor; if (c.HasValue) UnityEngine.GUI.backgroundColor = c.Value;
    System.Boolean r = UnityEngine.GUILayout.Button(new UnityEngine.GUIContent(l,t ?? ""),UnityEngine.GUILayout.Height(h));
    UnityEngine.GUI.backgroundColor = p; return r; }
  void Dk() { IF_UE.SetDirty(hb); }
  void NewPipeline(C_AviGenerator h,int n)
  { var g = new UnityEngine.GameObject("Pipeline_" + n); g.transform.SetParent(h.transform,false);
    var ph = g.AddComponent<C_AviGenerator>(); ph.mode = E_AviGeneratorMode.PipelineID; ph.NZKC_GO_AviRoot = h.NZKC_GO_AviRoot; ph.avatarRootName = h.avatarRootName; }
  void OnEnable()
  { hb = (C_AviGenerator)target;
    sMd = serializedObject.FindProperty("mode"); sAN = serializedObject.FindProperty("avatarRootName");
    sAR = serializedObject.FindProperty("NZKC_GO_AviRoot"); sCl = serializedObject.FindProperty("cloneOriginalArmature");
    sReb = serializedObject.FindProperty("removeEndBones");
    sAg = serializedObject.FindProperty("autoGenerateArmature"); sArm = serializedObject.FindProperty("armatureRoot");
    sMs = serializedObject.FindProperty("meshSources"); sEn = serializedObject.FindProperty("entryName");
    sMg = serializedObject.FindProperty("meshGenSources"); sSh = serializedObject.FindProperty("sharedMaterials");
    sFm = serializedObject.FindProperty("faceMesh"); sUfv = serializedObject.FindProperty("useForVrcadFace");
    sOrig = serializedObject.FindProperty("originalArmatureSource");
    sAv = serializedObject.FindProperty("animatorAvatar");
    sTG = serializedObject.FindProperty("toggleGroup");
    sUS = serializedObject.FindProperty("useUniqueMaterialSlots");
    sBSS = serializedObject.FindProperty("blendshapeSources");
    sNAS = serializedObject.FindProperty("nanimationSources");
    sOTS = serializedObject.FindProperty("objectToggleSources");
    sUAC = serializedObject.FindProperty("userAnimationClips");
    sOffClip = serializedObject.FindProperty("offClip");
    sIsTC = serializedObject.FindProperty("isToggleChild");
    sBS = serializedObject.FindProperty("blendshapeToggle");
    sIBT = serializedObject.FindProperty("isBlendshapeToggle");
    sToggleSourceType = serializedObject.FindProperty("toggleSourceType");
    sToggleSourceMode = serializedObject.FindProperty("toggleSourceMode");
    sToggleDisplayName = serializedObject.FindProperty("toggleDisplayName");
    sToggleSourceObjects = serializedObject.FindProperty("toggleSourceObjects");
    sToggleBlendshapeName = serializedObject.FindProperty("toggleBlendshapeName");
    sOnConditions = serializedObject.FindProperty("onConditions");
    sOffConditions = serializedObject.FindProperty("offConditions");
    sIgnored = serializedObject.FindProperty("ignoredChildren"); }
  public override void OnInspectorGUI()
  { if (hb == null || serializedObject.targetObjects.Length == 0) return; serializedObject.Update();
    if (hb == null || hb.gameObject == null) { UnityEditor.EditorGUILayout.HelpBox("Target destroyed.",UnityEditor.MessageType.Warning); return; }
    UnityEditor.EditorGUILayout.Space(4); UnityEditor.EditorGUILayout.LabelField("Higharchy Baker",UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.Space(2); UnityEditor.EditorGUILayout.PropertyField(sMd,new UnityEngine.GUIContent("Script Mode"));
    UnityEditor.EditorGUILayout.Space(6);
    if (hb.isToggleChild) { DrToggleChild(); serializedObject.ApplyModifiedProperties(); return; }
    if (serializedObject.isEditingMultipleObjects) { UnityEditor.EditorGUILayout.HelpBox("Multi-select: edit shared props above.",UnityEditor.MessageType.Info); }
    else { var mode = (E_AviGeneratorMode)sMd.enumValueIndex;
    if (typeof(E_AviGeneratorMode).IsEnumDefined(mode)) switch (mode)
    { case E_AviGeneratorMode.This: DrThis(); break; case E_AviGeneratorMode.Root: DrRoot(); break;
      case E_AviGeneratorMode.MeshBuilder: DrMeshB(); break;
      case E_AviGeneratorMode.MeshGeneratorRoot: DrMGR(); break;
      case E_AviGeneratorMode.ArmatureBuilder: DrArm(); break;
      case E_AviGeneratorMode.AviRootBuilder: DrAVR(); break;
      case E_AviGeneratorMode.MeshGenerator: DrMG(); break;
      case E_AviGeneratorMode.MeshSplitter: DrMeshSplitter(); break;
      case E_AviGeneratorMode.OriginalState: DrOrigState(); break;
      case E_AviGeneratorMode.PipelineID: DrPID(); break;
      case E_AviGeneratorMode.GestureGenerator: if (hb.cbBlockType == CbBlockType.Custom) hb.cbBlockType = CbBlockType.AController; if (hb.animatorSubMode == E_AnimatorSubMode.Custom) hb.animatorSubMode = E_AnimatorSubMode.Base_Gesture; DrGen("Gesture"); break;
      case E_AviGeneratorMode.AnimatorBuilder: DrAnimatorBuilder(); break;
      case E_AviGeneratorMode.ExpressionsGenerator: DrGen("Expressions"); break;
      case E_AviGeneratorMode.FxGenerator: DrGen("FX"); break;
      case E_AviGeneratorMode.MenuGenerator: DrGen("Menu"); break;
      case E_AviGeneratorMode.AnimationsGenerator: DrGen("Animations"); break;
      case E_AviGeneratorMode.Animation: DrAnim(); break;
      case E_AviGeneratorMode.Expression_Layers: DrLayer(); break;
      case E_AviGeneratorMode.ToggleGenerator: DrToggleGenerator(); break;
      case E_AviGeneratorMode.SpsGenerator: DrSpsGenerator(); break;
      case E_AviGeneratorMode.StateMachine: DrSSM(); break;
      case E_AviGeneratorMode.State: DrSS(); break;
      case E_AviGeneratorMode.Up: DrUp(); break;
      case E_AviGeneratorMode.ControllerBuilder: DrCB(); break; } }
    if (!serializedObject.isEditingMultipleObjects) { hb.DedupeSources(); hb.SyncSharedMats(); } serializedObject.ApplyModifiedProperties(); }
  void DrChildrenList()
  { Sec("Children","Toggle each child as generator (green) or ignored (grey).");
    hb.SyncIgnored();
    if (hb.transform.childCount == 0) { UnityEditor.EditorGUILayout.LabelField("No children."); return; }
    UnityEditor.EditorGUI.BeginChangeCheck();
    foreach (UnityEngine.Transform c in hb.transform)
    { if (c.GetComponent<C_AviGenerator>() == null) continue;
    System.Boolean ign = hb.ignoredChildren.Contains(c.name);
    UnityEditor.EditorGUILayout.BeginHorizontal();
    System.String lbl = c.name + (ign ? " [ignored]" : "");
    var col = ign ? new UnityEngine.Color(0.6f,0.6f,0.6f) : new UnityEngine.Color(0.55f,0.9f,0.55f);
    if (Bt(lbl,"Click to " + (ign ? "include" : "ignore"),col,22))
    { if (ign) hb.ignoredChildren.Remove(c.name);
      else hb.ignoredChildren.Add(c.name);
      Dk(); }
    UnityEditor.EditorGUILayout.EndHorizontal(); }
    if (UnityEditor.EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties(); }
  void DrThis()
  { UnityEditor.EditorGUILayout.HelpBox("Select a mode above.",UnityEditor.MessageType.Info);
    if (UnityEngine.GUILayout.Button("Reload Children Objects",UnityEngine.GUILayout.Height(24))) { hb.ReloadChildren(); Dk(); }
    DrChildrenList(); }
  void DrPID()
  { Sec("Pipeline ID","Stores a blueprint ID for VRC_PipelineManager.");
    var pid = serializedObject.FindProperty("pipelineId");
    UnityEditor.EditorGUILayout.PropertyField(pid,new UnityEngine.GUIContent("Blueprint ID"));
    UnityEditor.EditorGUILayout.LabelField("Child of: " + (hb.transform.parent != null ? hb.transform.parent.name : "(root)")); }
  void DrGen(System.String label)
  { Sec(label,"Placeholder for " + label + " data.");
    UnityEditor.EditorGUILayout.HelpBox("Configure your " + label + " in this child UnityEngine.Object.",UnityEditor.MessageType.Info);
    if (label == "Gesture")
    { var sGRC = serializedObject.FindProperty("gestureReferenceController");
    UnityEditor.EditorGUILayout.PropertyField(sGRC,new UnityEngine.GUIContent("Reference Controller"));
    UnityEditor.EditorGUILayout.BeginHorizontal();
    if (Bt("Copy Controller Setup","Load default gesture layer as reference"))
    { AXController.SetupGesturesDefault(hb); AXController.ReloadGestureChildren(hb); Dk(); }
    if (Bt("Reload Gesture Children")) { AXController.ReloadGestureChildren(hb); Dk(); }
    UnityEditor.EditorGUILayout.EndHorizontal();
    if (Bt("Bake Gesture Controller","Bake from here",new UnityEngine.Color(0.55f,0.9f,0.55f),28))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) AXController.BakeGestureController(hb); }; }
    UnityEditor.EditorGUILayout.Space(4);
    Sec("Gesture UnityEditor.Editor","Add states and layers.");
    UnityEditor.EditorGUILayout.BeginHorizontal();
    if (Bt("+ New Layer","Create a new gesture layer"))
    { var layersFolder = hb.transform.Find("layers") ?? new UnityEngine.GameObject("layers").transform;
      if (layersFolder.parent == null) layersFolder.SetParent(hb.transform,false);
      int lc = layersFolder.childCount;
      var lg = new UnityEngine.GameObject("Layer_" + lc); lg.transform.SetParent(layersFolder,false);
      var lhb = lg.AddComponent<C_AviGenerator>();
      lhb.mode = E_AviGeneratorMode.ControllerBuilder; lhb.cbBlockType = CbBlockType.Layer;
      lhb.avatarRootName = hb.avatarRootName; lhb.NZKC_GO_AviRoot = hb.NZKC_GO_AviRoot; lhb.weight = 1f;
      foreach (var mk in new[] {("Entry",CbBlockType.Entry,true),("AnyState",CbBlockType.Any,true),("Exit",CbBlockType.Exit,false),("Up",CbBlockType.Up,false)})
      { var go = new UnityEngine.GameObject(mk.Item1); go.transform.SetParent(lg.transform,false);
      var mhb = go.AddComponent<C_AviGenerator>(); mhb.mode = E_AviGeneratorMode.ControllerBuilder; mhb.cbBlockType = mk.Item2;
      if (mk.Item3) go.AddComponent<TransitionSet>(); }
      IF_UE.RegCr(lg,"Create Layer"); Dk(); }
    if (Bt("+ New State","Add state to first layer"))
    { var tl = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.Cast<UnityEngine.Transform>(hb.transform), c => { var ch = c.GetComponent<C_AviGenerator>(); return ch != null && ch.cbBlockType == CbBlockType.Layer; });
      if (tl == null) UnityEngine.Debug.LogWarning("[HB] No layer found.");
      else { var sg = new UnityEngine.GameObject("sst_State_" + tl.childCount); sg.transform.SetParent(tl,false);
      var sh = sg.AddComponent<C_AviGenerator>(); sh.mode = E_AviGeneratorMode.State; IF_UE.RegCr(sg,"Create State"); Dk(); } }
    UnityEditor.EditorGUILayout.EndHorizontal(); }
    else if (label == "Expressions")
    { if (Bt("Reload Expression Children")) { hb.ReloadExpressionChildren(); Dk(); }
    if (Bt("Bake Expression Layers","Bake from here",new UnityEngine.Color(0.55f,0.9f,0.55f),28))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeExpressionLayers(); }; } }
    else if (label == "FX")
    { if (Bt("Bake FX Layers","Bake from here",new UnityEngine.Color(0.55f,0.9f,0.55f),28))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeFxLayers(); }; } }
    else if (Bt("Reload Children")) { hb.ReloadChildren(); Dk(); }
    if (label == "Gesture" || label == "Expressions" || label == "FX")
    { if (Bt("Rebuild YAML Blocks")) { hb.BuildYamlBlocks(); Dk(); }
    if (Bt("Sync YAML -> CbBlockType")) { hb.SyncCbBlockTypeFromYaml(); Dk(); }
    if (Bt("Build AX Controller","Build from CbBlockType hierarchy",new UnityEngine.Color(0.55f,0.9f,0.55f),22))
    { hb.BuildAXControllerFromHierarchy(); Dk(); }
    UnityEditor.EditorGUILayout.LabelField("YamlBlocks: " + (hb.transform.Find("YamlBlocks") != null ? "present" : "missing")); }
    UnityEditor.EditorGUILayout.LabelField("Child of: " + (hb.transform.parent != null ? hb.transform.parent.name : "(root)")); }
  void DrAnimatorBuilder()
  { var smp = serializedObject.FindProperty("animatorSubMode");
    var lp = serializedObject.FindProperty("animatorLayers");
    Sec("UnityEngine.Animator Builder","Generates VRChat animation controllers.");
    UnityEditor.EditorGUILayout.PropertyField(smp,new UnityEngine.GUIContent("UnityEngine.Animator System.Type"));
    var sm = (E_AnimatorSubMode)smp.enumValueIndex;
    if (sm == E_AnimatorSubMode.Base_Gesture)
    { var sGRC = serializedObject.FindProperty("gestureReferenceController");
    UnityEditor.EditorGUILayout.PropertyField(sGRC,new UnityEngine.GUIContent("Reference Controller"));
    if (Bt("Copy Default Setup")) { AXController.SetupGesturesDefault(hb); AXController.ReloadGestureChildren(hb); Dk(); } }
    DrYamlBlockRef();
    if (lp != null)
    { Sec("Layers (" + lp.arraySize + ")");
    for (int i = 0; i < lp.arraySize; i++)
    { var e = lp.GetArrayElementAtIndex(i);
      UnityEditor.EditorGUILayout.BeginHorizontal("box");
      UnityEditor.EditorGUILayout.PropertyField(e,new UnityEngine.GUIContent("[" + i + "]"),true);
      if (e.objectReferenceValue != null && Bt("X","Remove",new UnityEngine.Color(0.9f,0.4f,0.4f),18))
      { lp.DeleteArrayElementAtIndex(i); Dk(); }
      UnityEditor.EditorGUILayout.EndHorizontal(); }
    if (Bt("+ Add Layer")) { lp.arraySize++; Dk(); }
    if (Bt("Generate from YAML")) { hb.GenerateLayersFromYaml(hb); Dk(); } }
    if (Bt("Rebuild YAML Blocks")) { hb.BuildYamlBlocks(); Dk(); }
    if (Bt("Sync YAML -> CbBlockType")) { hb.SyncCbBlockTypeFromYaml(); Dk(); }
    if (Bt("Build AX Controller","From CbBlockType",new UnityEngine.Color(0.55f,0.9f,0.55f),22))
    { hb.BuildAXControllerFromHierarchy(); Dk(); }
    if (Bt("Bake Controller","Bake from layers",new UnityEngine.Color(0.55f,0.9f,0.55f),28))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) AXController.BakeGestureController(hb); }; } }
  void DrAnim()
  { Sec("Animation","Clip for this state.");
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClip"),new UnityEngine.GUIContent("Animation Clip"));
    UnityEditor.EditorGUILayout.PropertyField(sBS,new UnityEngine.GUIContent("Blendshape Toggle"));
    UnityEditor.EditorGUILayout.PropertyField(sIBT,new UnityEngine.GUIContent("Is Blendshape Toggle")); }
  void DrLayer()
  { Sec("Expression Layer","FX state machine layer.");
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("mask"),new UnityEngine.GUIContent("UnityEngine.Avatar Mask"));
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"),new UnityEngine.GUIContent("Layer Weight"));
    DrYamlBlockRef(); DrCBTransitionSets(); }
  void DrRoot()
  { Sec("Root Config","1st:Name | 2nd:Root | 3rd:Armature | 4th:UnityEngine.Mesh");
    sAN.stringValue = UnityEditor.EditorGUILayout.TextField("UnityEngine.Avatar Root Name [1st]",sAN.stringValue);
    UnityEditor.EditorGUILayout.PropertyField(sAR,new UnityEngine.GUIContent("UnityEngine.Avatar Root"));
    if (Bt("Set UnityEngine.Avatar Root")) { hb.SetAviRoot(); Dk(); }
    if (Bt("Reload Children")) { hb.ReloadChildren(); Dk(); }
    DrChildrenList();
    Sec("Baking");
    if (Bt("Delete Generated UnityEngine.Avatar","Clean re-test",new UnityEngine.Color(0.9f,0.4f,0.4f)))
    { if (hb != null && hb.NZKC_GO_AviRoot != null) { DestroyImmediate(hb.NZKC_GO_AviRoot); hb.NZKC_GO_AviRoot = null; hb.animatorAvatar = null; } }
    if (Bt("Bake All",null,new UnityEngine.Color(0.55f,0.9f,0.55f),30))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeAll(); }; }
    if (Bt("Bake UnityEngine.Avatar Root [2nd]")) hb.BakeAviRoot();
    if (Bt("Bake Armature [3rd]")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeArmature(); }; }
    if (Bt("Bake UnityEngine.Mesh [4th]")) hb.BakeMesh();
    if (Bt("Bake Gesture Layers")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) AXController.BakeGestureLayers(hb); }; }
    if (Bt("Bake Expression Layers")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeExpressionLayers(); }; }
    if (Bt("Bake FX Layers")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeFxLayers(); }; }
    if (Bt("Bake Toggle Generator")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeToggleGenerator(); }; }
    System.Boolean hG = hb.FindHB(HBChildren.GestureGenerator) != null;
    System.Boolean hE = hb.FindHB(HBChildren.ExpressionsGenerator) != null;
    System.Boolean hF = hb.FindHB(HBChildren.FxGenerator) != null;
    System.Boolean hA = hb.FindHB(HBChildren.AnimatorBuilder) != null;
    if (Bt("Attach All","Create all generators",new UnityEngine.Color(0.55f,0.9f,0.55f),22))
    { if (!hG) { var g = hb.CrCh(HBChildren.Prefix+HBChildren.GestureGenerator,E_AviGeneratorMode.GestureGenerator); var gh = g.GetComponent<C_AviGenerator>(); gh.cbBlockType = CbBlockType.AController; gh.animatorSubMode = E_AnimatorSubMode.Base_Gesture; }
    if (!hE) { hb.CrCh(HBChildren.Prefix+HBChildren.ExpressionsGenerator,E_AviGeneratorMode.ExpressionsGenerator); }
    if (!hF) { hb.CrCh(HBChildren.Prefix+HBChildren.FxGenerator,E_AviGeneratorMode.FxGenerator); }
    if (!hA) { var a = hb.CrCh(HBChildren.Prefix+HBChildren.AnimatorBuilder,E_AviGeneratorMode.AnimatorBuilder); a.GetComponent<C_AviGenerator>().animatorSubMode = E_AnimatorSubMode.Custom; }
    Dk(); }
    UnityEditor.EditorGUILayout.LabelField("Gesture"+(hG?" Y":" N")+" Expressions"+(hE?" Y":" N")+" FX"+(hF?" Y":" N")+" AB"+(hA?" Y":" N"),UnityEditor.EditorStyles.miniLabel); }
  void DrMeshB()
  { Sec("UnityEngine.Mesh Output");
    UnityEditor.EditorGUILayout.PropertyField(sMs,new UnityEngine.GUIContent("UnityEngine.Mesh Sources"),true);
    if (UnityEngine.GUILayout.Button("Reload Children",UnityEngine.GUILayout.Height(22))) { hb.ReloadChildren(); Dk(); } }
  void DrMGR()
  { Sec("UnityEngine.Mesh Generator Root");
    UnityEditor.EditorGUILayout.PropertyField(sSh,new UnityEngine.GUIContent("Shared Materials"),true);
    if (UnityEngine.GUILayout.Button("Add UnityEngine.Mesh Entry (M0...)",UnityEngine.GUILayout.Height(22))) { hb.AddMeshEntry(); Dk(); } }
  void DrArm()
  { Sec("Armature Builder");
    sCl.boolValue = UnityEditor.EditorGUILayout.Toggle("Clone Original",sCl.boolValue);
    sReb.boolValue = UnityEditor.EditorGUILayout.Toggle("Remove End Bones",sReb.boolValue);
    UnityEditor.EditorGUILayout.PropertyField(sArm,new UnityEngine.GUIContent("Armature Root"));
    if (Bt("Add Armature To Bake System.Collections.Generic.List")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) { hb.BakeArmature(); Dk(); } }; } }
  void DrAVR()
  { Sec("UnityEngine.Avatar Root Builder","Linked with Root mode.");
    sAN.stringValue = UnityEditor.EditorGUILayout.TextField("UnityEngine.Avatar Root Name",sAN.stringValue);
    UnityEditor.EditorGUILayout.PropertyField(sAR,new UnityEngine.GUIContent("UnityEngine.Avatar Root"));
    UnityEditor.EditorGUILayout.PropertyField(sAv,new UnityEngine.GUIContent("UnityEngine.Animator UnityEngine.Avatar","Drag an UnityEngine.Avatar here or generate from armature"));
    if (Bt("Generate UnityEngine.Avatar from Armature","Auto-build from armature bones")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) { hb.animatorAvatar = null; hb.BakeAviRoot(); var a = hb.NZKC_GO_AviRoot?.GetComponent<UnityEngine.Animator>(); if (a != null) hb.animatorAvatar = a.avatar; Dk(); } }; }
    /* Pipeline selector — list child PipelineID objects */
    var pipes = new System.Collections.Generic.List<System.String>();
    foreach (UnityEngine.Transform c in hb.transform) { var h = c.GetComponent<C_AviGenerator>(); if (h != null && h.mode == E_AviGeneratorMode.PipelineID) pipes.Add(c.name); }
    if (pipes.Count > 0)
    { var idx = UnityEngine.Mathf.Max(0,pipes.IndexOf(hb.selectedPipeline));
    var newIdx = UnityEditor.EditorGUILayout.Popup("Pipeline",idx,pipes.ToArray());
    if (newIdx != idx) { hb.selectedPipeline = pipes[newIdx];
      var sel = hb.transform.Find(pipes[newIdx]); if (sel != null) hb.pipelineId = hb.ResolvePipelineId(sel); Dk(); }
    UnityEditor.EditorGUILayout.BeginHorizontal();
    if (Bt("Set ID","Apply selected pipeline's blueprint ID")) { var s = hb.transform.Find(pipes[idx]); if (s != null) hb.pipelineId = hb.ResolvePipelineId(s); Dk(); }
    if (Bt("New Pipeline","Creates a PipelineID child")) { NewPipeline(hb,pipes.Count); Dk(); }
    UnityEditor.EditorGUILayout.EndHorizontal(); }
    else if (Bt("New Pipeline","Creates a PipelineID child")) { NewPipeline(hb,0); Dk(); }
    UnityEditor.EditorGUILayout.PropertyField(sFm,new UnityEngine.GUIContent("Face UnityEngine.Mesh","UnityEngine.SkinnedMeshRenderer with blendshapes for visemes"));
    UnityEditor.EditorGUILayout.PropertyField(sUfv,new UnityEngine.GUIContent("Use for VRC.SDK3.Avatars.Components.VRCAvatarDescriptor face?","Auto-fills VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.VisemeSkinnedMesh"));
    if (Bt("Set UnityEngine.Avatar Root")) { hb.SetAviRoot(); Dk(); } }
  void DrMG()
  { Sec("UnityEngine.Mesh Generator Entry");
    UnityEditor.EditorGUILayout.PropertyField(sEn,new UnityEngine.GUIContent("Entry Name"));
    UnityEditor.EditorGUILayout.PropertyField(sMg,new UnityEngine.GUIContent("UnityEngine.Mesh Sources"),true);
    UnityEditor.EditorGUILayout.PropertyField(sUS,new UnityEngine.GUIContent("Use Unique UnityEngine.Material Slots","When checked,each source mesh gets its own material slot instead of sharing with other sources that use the same material."));
    if (Bt("Add UnityEngine.Mesh To Bake System.Collections.Generic.List")) hb.BakeMesh(); }
  void DrMeshSplitter()
  { Sec("UnityEngine.Mesh Splitter");
    if (hb.GetComponent<MeshSplitterDefinition>() == null)
    UnityEditor.EditorGUILayout.HelpBox("Add MeshSplitterDefinition component",UnityEditor.MessageType.Info); }
  void DrSpsGenerator()
  { Sec("SPS Generator");
    UnityEditor.EditorGUILayout.HelpBox("Add SpsConfig component to configure",UnityEditor.MessageType.Info); }
  void DrOrigState()
  { Sec("Original Armature State");
    UnityEditor.EditorGUILayout.PropertyField(sOrig,new UnityEngine.GUIContent("Original Source"));
    if (Bt("Reset Transforms")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) { hb.ResetTransforms(); Dk(); } }; }
    if (Bt("Bake for VRChat")) { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) { hb.BakeArmatureFinal(); Dk(); } }; } }
  void DrToggleGenerator()
  { Sec("Toggle Generator");
    UnityEditor.EditorGUILayout.PropertyField(sBSS,new UnityEngine.GUIContent("Blendshape Sources"),true);
    UnityEditor.EditorGUILayout.PropertyField(sNAS,new UnityEngine.GUIContent("Nanimation Sources"),true);
    UnityEditor.EditorGUILayout.PropertyField(sOTS,new UnityEngine.GUIContent("UnityEngine.Object Toggle Sources"),true);
    if (Bt("Create Toggle Children","Generate child objects",null,22))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) { hb.CreateToggleChildren(); Dk(); } }; }
    if (Bt("Bake Toggle Generator","Generate clips/DBTs",new UnityEngine.Color(0.55f,0.9f,0.55f),28))
    { UnityEditor.EditorApplication.delayCall += () => { if (hb != null) hb.BakeToggleGenerator(); }; } }
  void DrToggleChild()
  { Sec("Toggle: " + hb.gameObject.name);
    UnityEditor.EditorGUILayout.PropertyField(sToggleDisplayName,new UnityEngine.GUIContent("Toggle Name"));
    UnityEditor.EditorGUILayout.PropertyField(sToggleSourceType,new UnityEngine.GUIContent("Source System.Type"));
    UnityEditor.EditorGUILayout.PropertyField(sToggleSourceObjects,new UnityEngine.GUIContent("Source Objects"),true); }
  void DrSSM()
  { Sec("State Machine");
    DrYamlBlockRef(); DrCBTransitionSets(); }
  void DrCBTransitionSets()
  { var tss = hb.GetComponents<TransitionSet>();
    if (tss.Length == 0)
    { if (Bt("+ Add Transition")) { hb.gameObject.AddComponent<TransitionSet>(); Dk(); } return; }
    Sec("Transitions (" + tss.Length + ")");
    for (int i = 0; i < tss.Length; i++)
    { var so = new UnityEditor.SerializedObject(tss[i]);
    UnityEditor.EditorGUILayout.PropertyField(so.FindProperty("target"),new UnityEngine.GUIContent("Target"));
    UnityEditor.EditorGUILayout.PropertyField(so.FindProperty("conditions"),new UnityEngine.GUIContent("Conditions"),true);
    UnityEditor.EditorGUILayout.PropertyField(so.FindProperty("duration"));
    UnityEditor.EditorGUILayout.PropertyField(so.FindProperty("hasExitTime"));
    UnityEditor.EditorGUILayout.PropertyField(so.FindProperty("hasFixedDuration"));
    so.ApplyModifiedProperties(); }
    if (Bt("+ Add")) { hb.gameObject.AddComponent<TransitionSet>(); Dk(); }
    if (tss.Length > 0 && Bt("- Remove Last",null,new UnityEngine.Color(0.9f,0.4f,0.4f)))
    { if (tss[tss.Length - 1] != null) { DestroyImmediate(tss[tss.Length - 1]); } Dk(); } }
  void DrYamlBlockRef()
  { if (System.String.IsNullOrEmpty(hb.gameObject.name)) return;
    var rootHB = hb.transform.root?.GetComponent<C_AviGenerator>() ?? hb.GetComponent<C_AviGenerator>();
    var ctrlRoots = rootHB?.GetAllControllerRoots() ??  new System.Collections.Generic.List<UnityEngine.Transform>();
    int mc = 0;
    foreach (var cr in ctrlRoots)
    { var yp = cr.Find("YamlBlocks");
    if (yp == null) continue;
    foreach (UnityEngine.Transform yc in yp)
    { var yb = yc.GetComponent<YamlBlock>();
      if (yb == null || System.String.IsNullOrEmpty(yb.blockName)) continue;
      if (yb.blockName == hb.gameObject.name) { mc++;
      UnityEditor.EditorGUILayout.LabelField("YB: " + yc.name + " [" + yb.typeTag + "]",UnityEditor.EditorStyles.miniLabel); } } }
    if (mc == 0 && ctrlRoots.Count > 0)
    UnityEditor.EditorGUILayout.LabelField("(no YAML block for \"" + hb.gameObject.name + "\")",UnityEditor.EditorStyles.miniLabel); }
  void SyncLayerOrderToHierarchy()
  { var lp = serializedObject.FindProperty("animatorLayers");
    if (lp == null) return;
    for (int i = 0; i < lp.arraySize; i++)
    { var e = lp.GetArrayElementAtIndex(i);
    var go = e.objectReferenceValue as UnityEngine.GameObject;
    if (go != null && go.transform.parent == hb.transform && go.transform.GetSiblingIndex() != i)
      go.transform.SetSiblingIndex(i); } }
  void DrUp()
  { Sec("Up","Sink — states target this to exit up."); }
  void DrCB()
  { var ct = hb.cbBlockType;
    var label = ct == CbBlockType.Layer ? "(CB) Layer" : ct == CbBlockType.Entry ? "(CB) Entry"
    : ct == CbBlockType.Any ? "(CB) AnyState" : ct == CbBlockType.Exit ? "(CB) Exit"
    : ct == CbBlockType.StateMachine ? "(CB) SM" : ct == CbBlockType.Up ? "(CB) Up"
    : ct == CbBlockType.SubState ? "(CB) SubState" : ct == CbBlockType.Script ? "(CB) Script"
    : ct == CbBlockType.AController ? "(CB) AController" : "(CB) ?";
    UnityEditor.EditorGUILayout.LabelField(label,UnityEditor.EditorStyles.boldLabel);
    if (hb.cbBlockType == CbBlockType.Custom && hb.mode == E_AviGeneratorMode.ControllerBuilder)
    hb.cbBlockType = CbBlockType.AController;
    if (ct == CbBlockType.AController)
    { UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("animatorSubMode"),new UnityEngine.GUIContent("Sub-Mode"));
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("acParams"),new UnityEngine.GUIContent("ACParams"),true);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("acLayers"),new UnityEngine.GUIContent("ACLayers"),true); }
    DrYamlBlockRef();
    if (ct == CbBlockType.Layer || ct == CbBlockType.SubState) DrCBTransitionSets(); }
  void DrLayerCB()
  { UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("mask"),new UnityEngine.GUIContent("UnityEngine.Avatar Mask"));
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"),new UnityEngine.GUIContent("Layer Weight"));
    DrYamlBlockRef(); DrCBTransitionSets(); }
  void DrSS()
  { UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClip"),new UnityEngine.GUIContent("Animation Clip"));
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("writeDefaultValues"),new UnityEngine.GUIContent("Write Defaults"));
    DrYamlBlockRef(); DrCBTransitionSets(); }
  }
}
}
#endif
