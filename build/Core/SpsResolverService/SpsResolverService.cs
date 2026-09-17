#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class SpsResolverService
  { private const int MaxPlayerIdComponent = (1 << 16) - 1;
    private static System.UInt32 _playerIdCounter = 1;
    private static UnityEngine.AnimationClip _playerIdLowClip;
    private static UnityEngine.AnimationClip _playerIdHighClip;
    public static void ResetPlayerIds() { _playerIdCounter = 1; _playerIdLowClip = null; _playerIdHighClip = null; }
    public static void EnsurePlayerIdClips(System.String outputBase,System.String avatarName,UnityEditor.Animations.AnimatorController fxController)
    { if (_playerIdLowClip != null) return;
      _playerIdLowClip = new UnityEngine.AnimationClip(); _playerIdLowClip.name = "SpsPlayerIdLow";
      _playerIdHighClip = new UnityEngine.AnimationClip(); _playerIdHighClip.name = "SpsPlayerIdHigh";
      /* Add random-driven int params */
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "SPS_PID_LR")) fxController.AddParameter("SPS_PID_LR",UnityEngine.AnimatorControllerParameterType.Int);
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "SPS_PID_HR")) fxController.AddParameter("SPS_PID_HR",UnityEngine.AnimatorControllerParameterType.Int);
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "SPS_PID_L")) fxController.AddParameter("SPS_PID_L",UnityEngine.AnimatorControllerParameterType.Float);
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "SPS_PID_H")) fxController.AddParameter("SPS_PID_H",UnityEngine.AnimatorControllerParameterType.Float);
      /* Create player ID layer */
      var layer = new UnityEditor.Animations.AnimatorControllerLayer { name = "SPS - Player ID",stateMachine = new UnityEditor.Animations.AnimatorStateMachine(),defaultWeight = 1f,blendingMode = UnityEditor.Animations.AnimatorLayerBlendingMode.Override };
      var entry = layer.stateMachine.AddState("Entry",new UnityEngine.Vector3(200,0,0)); entry.writeDefaultValues = true;
      var rand = layer.stateMachine.AddState("Randomize",new UnityEngine.Vector3(400,0,0)); rand.writeDefaultValues = true;
      var et = entry.AddTransition(rand); et.hasExitTime = false; et.hasFixedDuration = true; et.duration = 0;
      /* No direct random support in standard API, so use copy with random default values */
      rand.motion = _playerIdLowClip;
      var ls = System.Linq.Enumerable.ToList(fxController.layers); ls.Add(layer); fxController.layers = ls.ToArray(); }
    public static void RegisterRenderer(UnityEngine.Component component,UnityEditor.Animations.AnimatorController fxController,System.String outputBase,System.String avatarName)
    { if (component == null) return;
      EnsurePlayerIdClips(outputBase,avatarName,fxController);
      _playerIdLowClip.SetCurve("",typeof(UnityEngine.MeshRenderer),"material._SPS_PlayerIdLow",UnityEngine.AnimationCurve.Constant(0f,0f,(float)_playerIdCounter));
      _playerIdHighClip.SetCurve("",typeof(UnityEngine.MeshRenderer),"material._SPS_PlayerIdHigh",UnityEngine.AnimationCurve.Constant(0f,0f,(float)(_playerIdCounter >> 16)));
      _playerIdCounter = (_playerIdCounter + 1) % (MaxPlayerIdComponent + 1);
      if (_playerIdCounter == 0) _playerIdCounter = 1; }
    public static void ConfigureResolverOnBody(UnityEngine.GameObject avatarRoot,UnityEditor.Animations.AnimatorController fxController,System.String outputBase,System.String avatarName)
    { /* Find all SkinnedMeshRenderers on the avatar body */
      var renderers = avatarRoot.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
      foreach (var smr in renderers)
      { if (smr == null || smr.sharedMaterials == null || smr.sharedMaterials.Length == 0) continue;
        /* Check if this renderer already has SPS materials */
        System.Boolean hasSps = false;
        foreach (var m in smr.sharedMaterials)
        { if (m != null && m.shader != null && m.shader.name != null && m.shader.name.Contains("SPS")) { hasSps = true; break; } }
        if (hasSps) continue;
        /* We don't modify the body mesh materials - instead we ensure the socket markers
         * have the correct render order (Background-948) so the resolver can find them.
         * The resolver on the body is for PLUG-side detection. For our sockets, the
         * markers already have the correct shader and properties. */
        UnityEngine.Debug.Log("[SPS2] Body renderer: " + smr.name + " (" + smr.sharedMaterials.Length + " mats)"); } } }
}
}
#endif
