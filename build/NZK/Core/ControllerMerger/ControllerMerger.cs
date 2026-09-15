#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public class ControllerMerger
  {
  readonly UnityEditor.Animations.AnimatorController _target;
  readonly System.Collections.Generic.HashSet<System.String> _existingParams = new System.Collections.Generic.HashSet<System.String>();
  readonly System.Collections.Generic.HashSet<System.String> _existingLayerNames = new System.Collections.Generic.HashSet<System.String>();
  /** <summary>Create a merger that writes into an existing controller.</summary> */
  public ControllerMerger(UnityEditor.Animations.AnimatorController target)
  { _target = target;
    if (_target != null)
    { foreach (var p in _target.parameters)
      _existingParams.Add(p.name);
    foreach (var l in _target.layers)
      _existingLayerNames.Add(l.name); }
  }
  /** <summary> */
  /** Create a fresh controller at the given path and return a merger for it. */
  /** </summary> */
  public static ControllerMerger Create(System.String path,System.String name = null)
  { System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
    IF_UE.DeleteAsset(path);
    var ctrl = new UnityEditor.Animations.AnimatorController();
    ctrl.name = name ?? System.IO.Path.GetFileNameWithoutExtension(path);
    IF_UE.CreateAsset(ctrl,path);
    /* Remove the default "Base Layer" that Unity auto-creates */
    if (ctrl.layers.Length > 0) ctrl.RemoveLayer(0);
    IF_UE.SetDirty(ctrl);
    IF_UE.SaveAndRefresh();
    return new ControllerMerger(ctrl); }
  /** <summary>Merge all content from source into the target controller.</summary> */
  public void Merge(UnityEditor.Animations.AnimatorController source)
  { if (source == null || _target == null) return;
    MergeParameters(source);
    MergeLayers(source); }
  /** <summary>Merge parameters from source (skips duplicates by name).</summary> */
  public void MergeParameters(UnityEditor.Animations.AnimatorController source)
  { if (source == null) return;
    foreach (var p in source.parameters)
    { if (_existingParams.Contains(p.name)) continue;
    _target.AddParameter(p.name,p.type);
    _existingParams.Add(p.name); }
  }
  /** <summary>Merge all layers from source (skips duplicates by name).</summary> */
  public void MergeLayers(UnityEditor.Animations.AnimatorController source)
  { if (source == null) return;
    foreach (var layer in source.layers)
    { System.String layerName = layer.name;
    if (_existingLayerNames.Contains(layerName))
    { // Append a suffix for duplicate layer names
      int suffix = 2;
      while (_existingLayerNames.Contains(layerName + "_" + suffix))
      suffix++;
      layerName = layerName + "_" + suffix; }
    var sm = CopyStateMachine(layer.stateMachine,layerName);
    UnityEditor.AssetDatabase.AddObjectToAsset(sm,_target);
    _target.AddLayer(new UnityEditor.Animations.AnimatorControllerLayer
    {
      name = layerName,stateMachine = sm,defaultWeight = layer.defaultWeight,blendingMode = layer.blendingMode,iKPass = layer.iKPass,syncedLayerIndex = layer.syncedLayerIndex,syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming,avatarMask = layer.avatarMask
    });
    _existingLayerNames.Add(layerName); }
  }
  /** <summary>Save all pending changes and return the target controller.</summary> */
  public UnityEditor.Animations.AnimatorController Save()
  { IF_UE.SetDirty(_target);
    IF_UE.SaveAndRefresh();
    return _target; }
  /** <summary> */
  /** One-shot: merge multiple source controllers into a controller at targetPath. */
  /** Creates the controller if it doesn't exist,or loads and extends it. */
  /** Returns the merged controller. */
  /** </summary> */
  public static UnityEditor.Animations.AnimatorController MergeControllers(System.String targetPath,params UnityEditor.Animations.AnimatorController[] sources)
  { if (System.String.IsNullOrEmpty(targetPath)) return null;
    var folder = System.IO.Path.GetDirectoryName(targetPath);
    System.IO.Directory.CreateDirectory(folder);
    var existing = IF_UE.Load<UnityEditor.Animations.AnimatorController>(targetPath);
    ControllerMerger merger;
    if (existing != null)
    merger = new ControllerMerger(existing);
    else
    merger = Create(targetPath);
    foreach (var src in sources)
    merger.Merge(src);
    return merger.Save(); }
  /* ── Private helpers ─────────────────────────────────────────── */
  /** <summary>Deep-copy an UnityEditor.Animations.AnimatorStateMachine including all states,transitions,*/
  /** sub-state-machines,and entry/any-state transitions.</summary> */
  static UnityEditor.Animations.AnimatorStateMachine CopyStateMachine(
    UnityEditor.Animations.AnimatorStateMachine source,System.String name)
  { var dest = new UnityEditor.Animations.AnimatorStateMachine();
    dest.name = name;
    dest.anyStatePosition = source.anyStatePosition;
    dest.entryPosition = source.entryPosition;
    dest.exitPosition = source.exitPosition;
    dest.parentStateMachinePosition = source.parentStateMachinePosition;
    /* Copy states */
    var stateMap = new System.Collections.Generic.Dictionary<System.String,UnityEditor.Animations.AnimatorState>();
    foreach (var s in source.states)
    { var ns = dest.AddState(s.state.name,s.position);
    CopyStateProperties(s.state,ns);
    stateMap[s.state.name] = ns; }
    /* Copy sub-state-machines (recursive) */
    foreach (var child in source.stateMachines)
    { var childSM = CopyStateMachine(child.stateMachine,child.stateMachine.name);
    UnityEditor.AssetDatabase.AddObjectToAsset(childSM,dest);
    dest.AddStateMachine(childSM,child.position); }
    /* Copy transitions between states (now that all states exist) */
    foreach (var s in source.states)
    { if (!stateMap.TryGetValue(s.state.name,out var destState)) continue;
    foreach (var t in s.state.transitions)
    { var targetState = ResolveDestState(t,stateMap);
      var nt = destState.AddTransition(targetState);
      CopyTransitionProperties(t,nt); }
    }
    /* Copy any-state transitions */
    foreach (var t in source.anyStateTransitions)
    { var targetState = t.destinationState != null && stateMap.ContainsKey(t.destinationState.name)
      ? stateMap[t.destinationState.name] : null;
    var nt = dest.AddAnyStateTransition(targetState);
    CopyAnyStateTransitionProperties(t,nt); }
    /* Copy entry transitions */
    foreach (var t in source.entryTransitions)
    { var targetState = t.destinationState != null && stateMap.ContainsKey(t.destinationState.name)
      ? stateMap[t.destinationState.name] : null;
    var nt = dest.AddEntryTransition(targetState);
    CopyTransitionConditions(t,nt); }
    return dest; }
  static void CopyStateProperties(UnityEditor.Animations.AnimatorState src,UnityEditor.Animations.AnimatorState dest)
  { dest.motion = src.motion;
    dest.speed = src.speed;
    dest.cycleOffset = src.cycleOffset;
    dest.mirror = src.mirror;
    dest.writeDefaultValues = src.writeDefaultValues;
    dest.tag = src.tag;
    dest.speedParameter = src.speedParameter;
    dest.speedParameterActive = src.speedParameterActive;
    dest.cycleOffsetParameter = src.cycleOffsetParameter;
    dest.cycleOffsetParameterActive = src.cycleOffsetParameterActive;
    dest.mirrorParameter = src.mirrorParameter;
    dest.mirrorParameterActive = src.mirrorParameterActive;
    dest.timeParameter = src.timeParameter;
    dest.timeParameterActive = src.timeParameterActive;
    dest.iKOnFeet = src.iKOnFeet; }
  static UnityEditor.Animations.AnimatorState ResolveDestState(UnityEditor.Animations.AnimatorStateTransition t,System.Collections.Generic.Dictionary<System.String,UnityEditor.Animations.AnimatorState> stateMap)
  { if (t.destinationState != null && stateMap.ContainsKey(t.destinationState.name))
    return stateMap[t.destinationState.name];
    return null; }
  static void CopyTransitionProperties(UnityEditor.Animations.AnimatorStateTransition src,UnityEditor.Animations.AnimatorStateTransition dest)
  { dest.hasExitTime = src.hasExitTime;
    dest.duration = src.duration;
    dest.exitTime = src.exitTime;
    dest.offset = src.offset;
    dest.interruptionSource = src.interruptionSource;
    dest.orderedInterruption = src.orderedInterruption;
    dest.canTransitionToSelf = src.canTransitionToSelf;
    dest.hasFixedDuration = src.hasFixedDuration;
    dest.mute = src.mute;
    dest.solo = src.solo;
    CopyTransitionConditions(src,dest); }
  static void CopyAnyStateTransitionProperties(
    UnityEditor.Animations.AnimatorStateTransition src,UnityEditor.Animations.AnimatorStateTransition dest)
  { dest.hasExitTime = src.hasExitTime;
    dest.duration = src.duration;
    dest.exitTime = src.exitTime;
    dest.offset = src.offset;
    dest.interruptionSource = src.interruptionSource;
    dest.orderedInterruption = src.orderedInterruption;
    dest.canTransitionToSelf = src.canTransitionToSelf;
    dest.hasFixedDuration = src.hasFixedDuration;
    dest.mute = src.mute;
    dest.solo = src.solo;
    CopyTransitionConditions(src,dest); }
  static void CopyTransitionConditions(
    UnityEditor.Animations.AnimatorStateTransition src,UnityEditor.Animations.AnimatorStateTransition dest)
  { foreach (var c in src.conditions)
    dest.AddCondition(c.mode,c.threshold,c.parameter); }
  static void CopyTransitionConditions(
    UnityEditor.Animations.AnimatorTransition src,UnityEditor.Animations.AnimatorTransition dest)
  { foreach (var c in src.conditions)
    dest.AddCondition(c.mode,c.threshold,c.parameter); }
  }
}
}
#endif
