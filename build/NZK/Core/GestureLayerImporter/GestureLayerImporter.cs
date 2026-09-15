#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class GestureLayerImporter
  {/** <summary> */
  /** Rebuild the HB_Gesture hierarchy from an example UnityEditor.Animations.AnimatorController. */
  /** Clears all existing children,then creates LYR_ layers,sm_ sub-SMs,*/
  /** sst_ states with clips,TransitionSets,and Entry/Exit/AnyState markers. */
  /** Also stores all example controller parameters on rootBaker.importParameterNames/types. */
  /** </summary> */
  public static void ImportFromExample(C_AviGenerator rootBaker,UnityEditor.Animations.AnimatorController example)
  { if (rootBaker == null) { UnityEngine.Debug.LogError("[GL Importer] rootBaker is null."); return; }
    if (example == null) { UnityEngine.Debug.LogError("[GL Importer] example controller is null."); return; }
    var log =  new System.Collections.Generic.List<System.String>();
    /* ---- 1. Clear existing children ----------------------------------- */
    var toDelete =  new System.Collections.Generic.List<UnityEngine.GameObject>();
    foreach (UnityEngine.Transform c in rootBaker.transform) toDelete.Add(c.gameObject);
    foreach (var d in toDelete) IF_UE.DestroyImmediateGameObject(d);
    LogAppend(log,"Cleared " + toDelete.Count + " existing children.");
    /* ---- 2. Store all example parameters as ACParam entries ---------- */
    rootBaker.acParams.Clear();
    foreach (var p in example.parameters)
    rootBaker.acParams.Add(new ACParam { name = p.name,type = (int)p.type,value = p.defaultFloat });
    LogAppend(log,"Stored " + example.parameters.Length + " ACParams.");
    /* ---- 3. Store all example layers as ACLayer entries --------------- */
    rootBaker.acLayers.Clear();
    foreach (var l in example.layers)
    rootBaker.acLayers.Add(new ACLayer { name = l.name,weight = l.defaultWeight,blendMode = (int)l.blendingMode });
    LogAppend(log,"Stored " + example.layers.Length + " ACLayers.");
    /* ---- Lookups: state→sst_GO,sm→marker GOs ------------------------ */
    var stateToGO = new System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,UnityEngine.GameObject>();    var smNameToGO = new System.Collections.Generic.Dictionary<System.String,UnityEngine.GameObject>();
    /* Map state → original entry transition conditions (for compound entries like Thumbs up) */
    var stateToEntryConds = new System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,System.Collections.Generic.List<(System.String param,int mode,float threshold)>>();
    /* Per-SM info: (layerGO,subSM_GO?,entryGO,exitGO,anyStateGO,smRef) */
    var smInfos =  new System.Collections.Generic.List<(UnityEditor.Animations.AnimatorStateMachine sm,UnityEngine.GameObject layerGO,UnityEngine.GameObject smGO,UnityEngine.GameObject entryGO,UnityEngine.GameObject upGO,UnityEngine.GameObject anyStateGO,UnityEngine.GameObject exitGO)>();
    /* Create a layers container (no LYR_ prefix — use actual names from YAML) */
    var layersContainer = new UnityEngine.GameObject("layers");
    layersContainer.transform.SetParent(rootBaker.transform,false);
    /* ================================================================== */
    /* PASS 1: Create ALL objects — layers,sub-SMs,states,markers */
    /* ================================================================== */
    for (int li = 0; li < example.layers.Length; li++)
    { var exLayer = example.layers[li];
    var layerGO = new UnityEngine.GameObject(exLayer.name);
    layerGO.transform.SetParent(layersContainer.transform,false);
    var sm = exLayer.stateMachine;
    if (sm == null) continue;
    System.Boolean hasSubSMs = sm.stateMachines != null && sm.stateMachines.Length > 0;
    /* Layer markers: Entry,AnyState,Exit */
    var layerEntry = CreateMarker("Entry",layerGO.transform);
    var layerAnyState = CreateMarker("AnyState",layerGO.transform);
    var layerExit = CreateMarker("Exit",layerGO.transform);
    LogAppend(log,"Layer[" + li + "] \"" + exLayer.name + "\"" + (hasSubSMs ? " (has sub-SMs)" : ""));
      foreach (var childSubSM in sm.stateMachines)
      { var subSM = childSubSM.stateMachine;
      if (subSM == null) continue;
      var smGO = new UnityEngine.GameObject("sm_" + subSM.name);
      smGO.transform.SetParent(layerGO.transform,false);
      smNameToGO[subSM.name] = smGO;
      /* Sub-SM markers: Entry,Up,AnyState,Exit */
      var smEntry = CreateMarker("Entry",smGO.transform);
      var smUp = CreateMarker("Up",smGO.transform);
      var smAny = CreateMarker("AnyState",smGO.transform);
      var smExit = CreateMarker("Exit",smGO.transform);
      smInfos.Add((subSM,layerGO,smGO,smEntry,smUp,smAny,smExit));
      int sstCount = 0;
      foreach (var childState in subSM.states)
      { if (childState.state == null) continue;
        var sstGO = CreateStateObject(childState.state,smGO.transform);
        stateToGO[childState.state] = sstGO; sstCount++; }
      LogAppend(log,"  sm_" + subSM.name + " (" + sstCount + " states: " + System.String.Join(",",System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(subSM.states, s => s.state != null), s => s.state.name)) + ")"); }
      smInfos.Add((sm,layerGO,null,layerEntry,null,layerAnyState,layerExit));
      /* Capture parent SM's entry transition conditions per state */
      if (sm.entryTransitions != null)
      { foreach (var entry in sm.entryTransitions)
      { if (entry.destinationState != null && entry.conditions != null && entry.conditions.Length > 0)
        { var conds =  new System.Collections.Generic.List<(System.String,int,float)>();
        foreach (var c in entry.conditions) conds.Add((c.parameter,(int)c.mode,c.threshold));
        stateToEntryConds[entry.destinationState] = conds; } } }
    else
    { smInfos.Add((sm,layerGO,null,layerEntry,null,layerAnyState,layerExit));
      int sstCount = 0;
      foreach (var childState in sm.states)
      { if (childState.state == null) continue;
      var sstGO = CreateStateObject(childState.state,layerGO.transform);
      stateToGO[childState.state] = sstGO; sstCount++; }
      if (sstCount > 0) LogAppend(log,"  (" + sstCount + " direct states)"); }
    }
    /* ================================================================== */
    /* PASS 2: Resolve targets on all TransitionSets (conditions/timing already on) */
    /* ================================================================== */
    foreach (var (sm,layerGO,smGO,entryGO,upGO,anyStateGO,exitGO) in smInfos)
    { System.String ctx = smGO != null ? "  " + smGO.name + "/" : "  " + layerGO.name + "/";
    var layerHB = layerGO.GetComponent<C_AviGenerator>();
    if (layerHB == null) { layerHB = layerGO.AddComponent<C_AviGenerator>();
      layerHB.mode = E_AviGeneratorMode.Expression_Layers; }
    layerHB.mode = E_AviGeneratorMode.ControllerBuilder;
    layerHB.cbBlockType = CbBlockType.Layer;
    /* Sub-SM setup */
    if (smGO != null)
    { var smHB = smGO.GetComponent<C_AviGenerator>();
      if (smHB == null) { smHB = smGO.AddComponent<C_AviGenerator>();
      smHB.mode = E_AviGeneratorMode.ControllerBuilder;
      smHB.cbBlockType = CbBlockType.StateMachine; }
      var smTS = smGO.GetComponent<TransitionSet>() ?? smGO.AddComponent<TransitionSet>();
      smTS.target = ResolveSMDefaultTarget(sm,smGO.transform,stateToGO,smNameToGO);
      LogAppend(log,ctx + "SM → " + (smTS.target != null ? smTS.target.name : "(none)")); }
    /* Wire Entry marker */
    WireEntryToDefault(entryGO,sm,smGO != null ? smGO.transform : layerGO.transform,stateToGO,smNameToGO);
    var entryTS = entryGO.GetComponent<TransitionSet>();
    LogAppend(log,ctx + "Entry → " + (entryTS?.target != null ? entryTS.target.name : "(none)"));
    /* Resolve targets for all TransitionSets on state objects (lockstep with original transitions) */
    UnityEngine.Transform searchRoot = smGO != null ? smGO.transform : layerGO.transform;
    foreach (var childState in sm.states)
    { if (childState.state == null) continue;
      if (!stateToGO.TryGetValue(childState.state,out var sstGO)) continue;
      var tss = sstGO.GetComponents<TransitionSet>();
      var origTrans = childState.state.transitions;
      int count = System.Math.Min(tss.Length,origTrans?.Length ?? 0);
      for (int i = 0; i < count; i++)
      { tss[i].target = ResolveTarget(origTrans[i],upGO,searchRoot,stateToGO,smNameToGO);
      LogAppend(log,ctx + childState.state.name + "[" + i + "] → " + (tss[i].target?.name ?? (tss[i].isExit ? "exit" : "(none)"))); }
      /* Capture original entry conditions from parent SM for this state. */
      /* Store on the first TransitionSet with Equals/NotEqual conditions (the "main" one). */
      if (tss.Length > 0 && stateToEntryConds.TryGetValue(childState.state,out var entryConds))
      { for (int j = 0; j < tss.Length; j++)
      { if (System.Linq.Enumerable.Any(tss[j].conditions, c => c.mode == 6 || c.mode == 7))
        { tss[j].entryConditions.Clear();
        foreach (var (param,mode,threshold) in entryConds)
          tss[j].entryConditions.Add(new ConditionEntry { parameter = param,mode = mode,threshold = threshold });
        break; } } } }
    /* Wire AnyState marker */
    if (anyStateGO != null && sm.anyStateTransitions != null && sm.anyStateTransitions.Length > 0)
    { var anyTS = anyStateGO.GetComponent<TransitionSet>();
      if (anyTS != null)
      { var firstAny = sm.anyStateTransitions[0];
      anyTS.target = ResolveTarget(firstAny,upGO,searchRoot,stateToGO,smNameToGO);
      anyTS.conditions.Clear();
      if (firstAny.conditions != null)
        foreach (var c in firstAny.conditions)
        anyTS.conditions.Add(new ConditionEntry { parameter = c.parameter,mode = (int)c.mode,threshold = c.threshold });
      LogAppend(log,ctx + "AnyState (" + sm.anyStateTransitions.Length + " transitions) → " + (anyTS.target?.name ?? "(none)")); } }
    }
    /* ---- Post-pass: apply layer properties from example layers -------- */
    for (int li = 0; li < example.layers.Length; li++)
    { var exLayer = example.layers[li];
    var layersFolder = rootBaker.transform.Find("layers");
    var layerGO = layersFolder != null ? layersFolder.Find(exLayer.name) : rootBaker.transform.Find("LYR_" + exLayer.name);
    if (layerGO == null) continue;
    var hb = layerGO.GetComponent<C_AviGenerator>();
    if (hb == null) continue;
    hb.weight = exLayer.defaultWeight;
    if (exLayer.avatarMask != null) hb.mask = exLayer.avatarMask;
    LogAppend(log,"  Layer[" + li + "] weight=" + exLayer.defaultWeight
      + (exLayer.avatarMask != null ? " mask=" + exLayer.avatarMask.name : "")); }
    /* ---- Store raw YAML blocks as scene objects --------------------------- */
    var existingBlocks = rootBaker.transform.Find("YamlBlocks");
    if (existingBlocks != null) IF_UE.DestroyImmediateGameObject(existingBlocks.gameObject);
    System.String refPath = UnityEditor.AssetDatabase.GetAssetPath(example);
    if (!System.String.IsNullOrEmpty(refPath) && System.IO.File.Exists(refPath))
    { System.String rawYaml = System.IO.File.ReadAllText(refPath);
    var blocksGO = new UnityEngine.GameObject("YamlBlocks");
    blocksGO.transform.SetParent(rootBaker.transform,false);
    int order = 0,pos = 0;
    /* Store YAML header (lines before the first "--- !u!" block) */
    /* e.g. "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n" */
    int firstBlock = rawYaml.IndexOf("\n--- !u!");
    if (firstBlock > 0)
    { System.String header = rawYaml.Substring(0,firstBlock + 1); // include the leading \n
      var hdrGO = new UnityEngine.GameObject("YB_header");
      hdrGO.transform.SetParent(blocksGO.transform,false);
      var hdrYB = hdrGO.AddComponent<YamlBlock>();
      hdrYB.rawText = header;
      hdrYB.sortOrder = order++; }
    while (pos < rawYaml.Length)
    { int blockStart = rawYaml.IndexOf("--- !u!",pos);
      if (blockStart < 0) break;
      int nextBlock = rawYaml.IndexOf("--- !u!",blockStart + 6);
      int blockEnd = nextBlock >= 0 ? nextBlock : rawYaml.Length;
      System.String blockText = rawYaml.Substring(blockStart,blockEnd - blockStart);
      /* Extract type tag and fileID from "--- !u!TYPE &FILEID" */
      var blockGO = new UnityEngine.GameObject("YB_" + order);
      blockGO.transform.SetParent(blocksGO.transform,false);
      var yb = blockGO.AddComponent<YamlBlock>();
      yb.rawText = blockText.TrimEnd() + "§";
      yb.sortOrder = order++;
      /* Parse type tag */
      int typeStart = blockStart + 6; // past "--- !u!"
      int typeEnd = rawYaml.IndexOf(' ',typeStart);
      if (typeEnd > typeStart) yb.typeTag = rawYaml.Substring(typeStart,typeEnd - typeStart);
      /* Parse fileID */
      int fidStart = rawYaml.IndexOf('&',typeStart);
      if (fidStart >= 0)
      { fidStart++;
      int fidEnd = rawYaml.IndexOf('\n',fidStart);
      long.TryParse(rawYaml.Substring(fidStart,fidEnd - fidStart),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out yb.fileId);
      /* Try to extract m_Name for debugging */
      int namePos = blockText.IndexOf("m_Name: ");
      if (namePos >= 0)
      { int nameEnd = blockText.IndexOf('\n',namePos + 8);
        yb.blockName = blockText.Substring(namePos + 8,nameEnd - namePos - 8).Trim(); } }
      pos = blockEnd; }
    LogAppend(log,"Stored " + order + " YAML blocks as scene objects."); }
    else LogAppend(log,"Cannot read reference YAML — no blocks stored.");
    UnityEngine.Debug.Log("[GL Importer] Import complete: " + example.layers.Length + " layers," + stateToGO.Count + " states."
    + "§" + System.String.Join("§",System.Linq.Enumerable.Select(log, l => "  " + l))); }
  /* ---- helpers --------------------------------------------------------- */
  static void LogAppend(System.Collections.Generic.List<System.String> log,System.String msg) { if (log != null) log.Add(msg); }
  /** <summary>Create a state UnityEngine.GameObject named after the controller state (no prefix).</summary> */
  static UnityEngine.GameObject CreateStateObject(UnityEditor.Animations.AnimatorState state,UnityEngine.Transform parent)
  { var sstGO = new UnityEngine.GameObject(state.name);
    sstGO.transform.SetParent(parent,false);
    var sstHB = sstGO.AddComponent<C_AviGenerator>();
    sstHB.mode = E_AviGeneratorMode.State;
    sstHB.writeDefaultValues = state.writeDefaultValues;
    /* Copy animation clip — use UnityEditor.AssetDatabase to get a proper reference */
    if (state.motion is UnityEngine.AnimationClip clip)
    { System.String clipPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    sstHB.animationClip = !System.String.IsNullOrEmpty(clipPath)
      ? IF_UE.Load<UnityEngine.AnimationClip>(clipPath) : clip; }
    else if (state.motion is UnityEditor.Animations.BlendTree blendTree)
    UnityEngine.Debug.Log("[GL Importer] State \"" + state.name + "\" uses UnityEditor.Animations.BlendTree motion — clipping skipped.");
    /* Copy ALL transitions as multiple TransitionSet components (not just first) */
    foreach (var trans in state.transitions)
    { var ts = sstGO.AddComponent<TransitionSet>();
    ts.duration = trans.duration;
    ts.hasExitTime = trans.hasExitTime;
    ts.hasFixedDuration = trans.hasFixedDuration;
    ts.exitTime = trans.exitTime;
    ts.canTransitionToSelf = trans.canTransitionToSelf;
    ts.isExit = trans.isExit;
    if (trans.conditions != null)
      foreach (var c in trans.conditions)
      ts.conditions.Add(new ConditionEntry { parameter = c.parameter,mode = (int)c.mode,threshold = c.threshold }); }
    return sstGO; }
  /** <summary>Create a marker UnityEngine.GameObject. Exit/Up get no TransitionSet (sinks).</summary> */
  static UnityEngine.GameObject CreateMarker(System.String name,UnityEngine.Transform parent)
  { var go = new UnityEngine.GameObject(name);
    go.transform.SetParent(parent,false);
    var hb = go.GetComponent<C_AviGenerator>() ?? go.AddComponent<C_AviGenerator>();
    hb.mode = E_AviGeneratorMode.ControllerBuilder;
    SetSt8(hb,name);
    /* Exit and Up are sinks — no TransitionSet needed */
    if (name != "Exit" && name != "Up" && go.GetComponent<TransitionSet>() == null)
    go.AddComponent<TransitionSet>();
    return go; }
  static void SetSt8(C_AviGenerator hb,System.String name)
  { hb.cbBlockType = name switch
    { "Entry" => CbBlockType.Entry,"AnyState" => CbBlockType.Any,"Exit" => CbBlockType.Exit,"Up" => CbBlockType.Up,_ => CbBlockType.Custom }; }
  /** <summary>Wire an Entry marker's TransitionSet to mirror the example */
  /** controller's Entry transitions. Copies the first Entry transition's */
  /** conditions + target (state or sub-state-machine). Falls back to */
  /** default state if no Entry transitions exist. Leaves target null if */
  /** no valid target is found — Entry with null target means "no entry point."</summary> */
  static void WireEntryToDefault(UnityEngine.GameObject entryGO,UnityEditor.Animations.AnimatorStateMachine sm,UnityEngine.Transform searchRoot,System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,UnityEngine.GameObject> stateToObject,System.Collections.Generic.Dictionary<System.String,UnityEngine.GameObject> smNameToGO)
  { if (entryGO == null || sm == null) return;
    var ts = entryGO.GetComponent<TransitionSet>();
    if (ts == null) return;
    /* Priority 0: if SM has sub-SMs,Entry → first sub-SM */
    if (sm.stateMachines != null && sm.stateMachines.Length > 0)
    { var firstSM = sm.stateMachines[0].stateMachine;
    if (firstSM != null && smNameToGO.TryGetValue(firstSM.name,out var smGO))
    { ts.target = smGO; return; } }
    /* Priority 1: copy the first Entry transition from the example controller */
    if (sm.entryTransitions != null && sm.entryTransitions.Length > 0)
    { var firstEntry = sm.entryTransitions[0];
    ts.conditions.Clear();
    if (firstEntry.conditions != null)
    { foreach (var c in firstEntry.conditions)
      ts.conditions.Add(new ConditionEntry { parameter = c.parameter,mode = (int)c.mode,threshold = c.threshold }); }
    if (firstEntry.destinationState != null)
    { if (stateToObject.TryGetValue(firstEntry.destinationState,out var tgt))
      ts.target = tgt;
      else { var f = searchRoot.Find(firstEntry.destinationState.name);
      if (f != null) ts.target = f.gameObject; } }
    else if (firstEntry.destinationStateMachine != null)
    { if (smNameToGO.TryGetValue(firstEntry.destinationStateMachine.name,out var smFound))
      ts.target = smFound; }
    return; }
    /* Priority 2: fall back to the SM's default state */
    var defaultState = sm.defaultState;
    if (defaultState == null && sm.states.Length > 0)
    defaultState = sm.states[0].state;
    if (defaultState != null)
    { if (stateToObject.TryGetValue(defaultState,out var target))
      ts.target = target;
    else { var found = searchRoot.Find(defaultState.name);
      if (found != null) ts.target = found.gameObject; } }
    /* No valid target found — Entry stays null */
  }
  /** <summary>Ensure an Exit marker has no target — it is a sink that */
  /** other states target when they want to leave this state machine.</summary> */
  static void EnsureExitNoTarget(UnityEngine.GameObject exitGO)
  { if (exitGO == null) return;
    var ts = exitGO.GetComponent<TransitionSet>();
    if (ts != null) ts.target = null; }
  /** <summary>Find the default target for a state machine: first sub-SM,*/
  /** default state,first state,or Exit (for empty SMs).</summary> */
  /** <summary>Find the default target for a state machine at its own level: */
  /** sub-SMs → first sub-SM; root SM with states → null (states are children); */
  /** empty SM → null.</summary> */
  static UnityEngine.GameObject ResolveSMDefaultTarget(UnityEditor.Animations.AnimatorStateMachine sm,UnityEngine.Transform smRoot,System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,UnityEngine.GameObject> stateToGO,System.Collections.Generic.Dictionary<System.String,UnityEngine.GameObject> smNameToGO)
  { // Has sub-SMs → first sub-SM (peer level)
    if (sm.stateMachines != null && sm.stateMachines.Length > 0
    && sm.stateMachines[0].stateMachine != null
    && smNameToGO.TryGetValue(sm.stateMachines[0].stateMachine.name,out var subGO))
    return subGO;
    /* States are children,not peers — SM points nowhere */
    return null; }
  /** <summary>Copy all transitions from a state into a TransitionSet on its sst_ UnityEngine.Object. */
  /** Stores the STATE IDENTITY condition (inverted from the transition condition),*/
  /** so BakeGestureLayers can use it for entry transitions (Equals mode).</summary> */
  static UnityEngine.GameObject CopyTransitions(
    UnityEditor.Animations.AnimatorState state,UnityEngine.GameObject sstGO,UnityEngine.GameObject upGO,UnityEngine.Transform smTransform,System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,UnityEngine.GameObject> stateToObject,System.Collections.Generic.Dictionary<System.String,UnityEngine.GameObject> smNameToGO = null)
  { if (state.transitions == null || state.transitions.Length == 0) return null;
    /* Use the first transition for the TransitionSet (BakeGestureLayers reads one) */
    var firstTrans = state.transitions[0];
    var ts = sstGO.GetComponent<TransitionSet>();
    if (ts == null) ts = sstGO.AddComponent<TransitionSet>();
    /* Determine target UnityEngine.GameObject (where this transition goes — used as fallback destination) */
    var tgt = ResolveTarget(firstTrans,upGO,smTransform,stateToObject,smNameToGO);
    ts.target = tgt;
    /* Copy conditions INVERTED: the transition condition is "when to LEAVE",*/
    /* but the TransitionSet should store "when to BE IN" this state (state identity). */
    ts.conditions.Clear();
    foreach (var cond in firstTrans.conditions)
    { ts.conditions.Add(new ConditionEntry
    {
      parameter = cond.parameter,mode = (int)InvertConditionMode(cond.mode),threshold = cond.threshold
    }); }
    return tgt; }
  /** <summary>Invert an UnityEditor.Animations.AnimatorConditionMode for state identity.</summary> */
  static UnityEditor.Animations.AnimatorConditionMode InvertConditionMode(UnityEditor.Animations.AnimatorConditionMode m) => m switch
  { UnityEditor.Animations.AnimatorConditionMode.Equals => UnityEditor.Animations.AnimatorConditionMode.NotEqual,UnityEditor.Animations.AnimatorConditionMode.NotEqual => UnityEditor.Animations.AnimatorConditionMode.Equals,UnityEditor.Animations.AnimatorConditionMode.Greater => UnityEditor.Animations.AnimatorConditionMode.Less,UnityEditor.Animations.AnimatorConditionMode.Less => UnityEditor.Animations.AnimatorConditionMode.Greater,_ => m };
  /** <summary>Copy anyState transitions onto the AnyState marker UnityEngine.GameObject.</summary> */
  static void CopyAnyStateTransitions(
    UnityEditor.Animations.AnimatorStateTransition[] anyStateTransitions,UnityEngine.GameObject anyStateGO,UnityEngine.Transform smTransform,UnityEngine.GameObject upGO,System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,UnityEngine.GameObject> stateToObject,System.Collections.Generic.Dictionary<System.String,UnityEngine.GameObject> smNameToGO = null)
  { if (anyStateTransitions == null || anyStateTransitions.Length == 0) return;
    var ts = anyStateGO.GetComponent<TransitionSet>();
    if (ts == null) ts = anyStateGO.AddComponent<TransitionSet>();
    /* We use the first anyState transition for the marker (BakeGestureLayers uses one TransitionSet per object) */
    var first = anyStateTransitions[0];
    ts.target = ResolveTarget(first,upGO,smTransform,stateToObject,smNameToGO);
    ts.conditions.Clear();
    foreach (var cond in first.conditions)
    { ts.conditions.Add(new ConditionEntry
    {
      parameter = cond.parameter,mode = (int)cond.mode,threshold = cond.threshold
    }); }
  }
  /** <summary>Resolve the target UnityEngine.GameObject for a transition. */
  /** Returns upGO for exit/up transitions (go to parent SM). */
  /** Priority: destinationState → name lookup → destinationStateMachine → upGO.</summary> */
  static UnityEngine.GameObject ResolveTarget(
    UnityEditor.Animations.AnimatorStateTransition trans,UnityEngine.GameObject upGO,UnityEngine.Transform smTransform,System.Collections.Generic.Dictionary<UnityEditor.Animations.AnimatorState,UnityEngine.GameObject> stateToObject,System.Collections.Generic.Dictionary<System.String,UnityEngine.GameObject> smNameToGO = null)
  { // No destination or explicit exit → go UP
    if (trans.isExit || trans.destinationState == null)
    return upGO;
    /* Direct reference lookup */
    if (stateToObject.TryGetValue(trans.destinationState,out var targetGO))
    return targetGO;
    /* Name-based fallback */
    foreach (var kv in stateToObject)
    if (kv.Key.name == trans.destinationState.name) return kv.Value;
    /* UnityEngine.Transform search */
    var found = smTransform.Find(trans.destinationState.name);
    if (found != null) return found.gameObject;
    /* Destination state machine */
    if (trans.destinationStateMachine != null && smNameToGO != null
    && smNameToGO.TryGetValue(trans.destinationStateMachine.name,out var smGO))
    return smGO;
    /* Not found — go UP */
    return upGO; }
  }
}
}
#endif
