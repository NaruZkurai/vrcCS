#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static partial class FX {
public static class States { public static UnityEditor.Animations.AnimatorState Find(UnityEditor.Animations.AnimatorStateMachine sm,System.String name) { foreach (var s in sm.states) if (s.state.name == name) return s.state; return null; } }
}
}
}
}
#endif
