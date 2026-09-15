#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class BlendTrees
  { public static UnityEditor.Animations.ChildMotion Direct(UnityEngine.Motion motion) => new UnityEditor.Animations.ChildMotion { motion = motion,directBlendParameter = Vars.Consts.MainDbtWeightParameter,timeScale = Vars.Consts.BtTimeScale,cycleOffset = Vars.Consts.BtCycleOffset,threshold = Vars.Consts.BtChildThreshold,position = UnityEngine.Vector2.zero,mirror = Vars.Consts.BtMirror }; }
}
}
}
#endif
