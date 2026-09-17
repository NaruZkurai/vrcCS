#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static partial class Baking {
public static class FxMerger
  { public static void MergeAndAssign(
    System.String ctrlDir,System.String avatarName,UnityEngine.GameObject avatarObj,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad,UnityEditor.Animations.AnimatorController[] sourceControllers)
    {
    System.String preMergePath = ctrlDir + "/" + avatarName + "_FX_PreMerge.controller";
    System.String finalPath = ctrlDir + "/" + avatarName + "_SPS_FX.controller";
    Systems.Folder.Ensure(ctrlDir);
    UnityEditor.Animations.AnimatorController originalFx = null;
    System.String originalPath = null;
    if (vrcad != null && vrcad.baseAnimationLayers != null && vrcad.baseAnimationLayers.Length > 4)
    {
      originalFx = vrcad.baseAnimationLayers[4].animatorController as UnityEditor.Animations.AnimatorController;
      originalPath = originalFx != null ? UnityEditor.AssetDatabase.GetAssetPath(originalFx) : null; }
    if (originalFx != null && !System.String.IsNullOrEmpty(originalPath) && System.IO.File.Exists(originalPath))
    { UnityEditor.AssetDatabase.DeleteAsset(preMergePath);
      if (UnityEditor.AssetDatabase.CopyAsset(originalPath,preMergePath))
        UnityEditor.AssetDatabase.ImportAsset(preMergePath);
      UnityEditor.AssetDatabase.DeleteAsset(finalPath);
      if (!UnityEditor.AssetDatabase.CopyAsset(originalPath,finalPath))
      { UnityEngine.Debug.LogWarning("[FxMerger] Could not clone original (" + originalPath + "), creating fresh."); } }
    else if (originalFx != null && !System.String.IsNullOrEmpty(originalPath))
    { UnityEngine.Debug.LogWarning("[FxMerger] Original FX controller asset not found on disk at " + originalPath + ", creating fresh."); }
    var final = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(finalPath);
    if (final == null)
    { final = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(finalPath);
      if (final.layers.Length > 0) final.RemoveLayer(0); }
    var existingParams = new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(final.parameters, p => p.name));
    foreach (var src in sourceControllers)
    {
      if (src == null) continue;
      foreach (var p in src.parameters)
      if (!existingParams.Contains(p.name))
      { final.AddParameter(p.name,p.type); existingParams.Add(p.name); }
    }
    /* Remove any old SPS layers so fresh ones replace them */
    var finalLayers = new System.Collections.Generic.List<UnityEditor.Animations.AnimatorControllerLayer>(final.layers);
    finalLayers.RemoveAll(l => l.name != null && (l.name.StartsWith("SPS2_") || l.name.StartsWith("SPS - ")));
    final.layers = finalLayers.ToArray();
    var existingLayers = new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(final.layers, l => l.name));
    foreach (var src in sourceControllers)
    {
      if (src == null) continue;
      foreach (var layer in src.layers)
      {
      if (existingLayers.Contains(layer.name)) continue;
      if (layer.stateMachine == null) continue;
      var sm = new UnityEditor.Animations.AnimatorStateMachine { name = layer.stateMachine.name + "_Merged" };
      foreach (var state in layer.stateMachine.states)
      {
        var ns = sm.AddState(state.state.name,state.position);
        ns.motion = state.state.motion;
        ns.writeDefaultValues = state.state.writeDefaultValues;
        ns.speed = state.state.speed;
        foreach (var t in state.state.transitions)
        {
        var nt = ns.AddTransition(t.destinationState);
        nt.hasExitTime = t.hasExitTime; nt.duration = t.duration;
        nt.exitTime = t.exitTime; nt.offset = t.offset;
        nt.interruptionSource = t.interruptionSource;
        nt.orderedInterruption = t.orderedInterruption;
        nt.canTransitionToSelf = t.canTransitionToSelf;
        foreach (var c in t.conditions)
          nt.AddCondition(c.mode,c.threshold,c.parameter); }
      }
      foreach (var t in layer.stateMachine.anyStateTransitions)
      {
        var nt = sm.AddAnyStateTransition(t.destinationState);
        nt.hasExitTime = t.hasExitTime; nt.duration = t.duration;
        nt.exitTime = t.exitTime;
        foreach (var c in t.conditions)
        nt.AddCondition(c.mode,c.threshold,c.parameter); }
      foreach (var t in layer.stateMachine.entryTransitions)
      {
        var nt = sm.AddEntryTransition(t.destinationState);
        foreach (var c in t.conditions)
        nt.AddCondition(c.mode,c.threshold,c.parameter); }
      UnityEditor.AssetDatabase.AddObjectToAsset(sm,final);
      final.AddLayer(new UnityEditor.Animations.AnimatorControllerLayer
      {
        name = layer.name,stateMachine = sm,defaultWeight = layer.defaultWeight,blendingMode = layer.blendingMode,iKPass = layer.iKPass,syncedLayerIndex = layer.syncedLayerIndex,syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming
      });
      existingLayers.Add(layer.name); }
    }
    UnityEditor.EditorUtility.SetDirty(final);
    UnityEditor.AssetDatabase.SaveAssets();
    if (vrcad != null && vrcad.baseAnimationLayers != null && vrcad.baseAnimationLayers.Length > 4)
    {
      VRCAD.Set.BaseLayerSlot(vrcad,4,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX,final);
      UnityEditor.EditorUtility.SetDirty(vrcad);
      UnityEngine.Debug.Log("[FxMerger] Assigned merged FX (" + final.layers.Length + " layers) → " + finalPath); }
    }
  }
}
}
}
}
#endif
