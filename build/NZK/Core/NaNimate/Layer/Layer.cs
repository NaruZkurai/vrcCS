#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Layer
  { public static UnityEditor.Animations.AnimatorStateMachine Sm(System.String n,UnityEditor.Animations.AnimatorController c)
    { var sm = new UnityEditor.Animations.AnimatorStateMachine { name = n + " SM" }; UnityEditor.AssetDatabase.AddObjectToAsset(sm,c); return sm; }
  public static UnityEditor.Animations.AnimatorControllerLayer Make(System.String n,UnityEditor.Animations.AnimatorStateMachine sm)
    => new UnityEditor.Animations.AnimatorControllerLayer { name = n,stateMachine = sm,defaultWeight = Vars.Consts.LayerWeight,blendingMode = Vars.Consts.LayerBlendMode,iKPass = Vars.Consts.LayerIKPass,syncedLayerIndex = Vars.Consts.LayerSyncedIndex,syncedLayerAffectsTiming = Vars.Consts.LayerSyncedTiming };
  public static UnityEditor.Animations.AnimatorControllerLayer Get(UnityEditor.Animations.AnimatorController c,System.String n,out System.Boolean f)
    { foreach (var l in c.layers) { if (l.name == n) { f = true; return l; } } f = false; return default(UnityEditor.Animations.AnimatorControllerLayer); }
  public static UnityEditor.Animations.AnimatorControllerLayer Create(UnityEditor.Animations.AnimatorController c,System.String n)
    { c.AddLayer(NaNimate.Layer.Make(n,NaNimate.Layer.Sm(n,c))); return c.layers[c.layers.Length - 1]; }
  public static UnityEditor.Animations.AnimatorControllerLayer GllC(UnityEditor.Animations.AnimatorController c,System.String n)
    { System.Boolean f; var l = NaNimate.Layer.Get(c,n,out f); return f ? l : NaNimate.Layer.Create(c,n); }
  public static UnityEditor.Animations.AnimatorState State(UnityEditor.Animations.AnimatorStateMachine sm,System.String stateName)
  { foreach (var cs in sm.states) { if (cs.state != null && cs.state.name == stateName) return cs.state; }
    return sm.AddState(stateName); }
  public static void Set(UnityEditor.Animations.AnimatorController controller,UnityEditor.Animations.AnimatorControllerLayer updatedLayer)
  { var layers = controller.layers;
    for (int i = 0; i < layers.Length; i++) { if (layers[i].name == updatedLayer.name) { layers[i] = updatedLayer; controller.layers = layers; return; } } } }
}
}
}
#endif
