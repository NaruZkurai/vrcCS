#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static partial class FX {
public static class Layers { public static UnityEditor.Animations.AnimatorControllerLayer Get(UnityEditor.Animations.AnimatorController c,System.String n) { foreach (var l in c.layers) if (l.name == n) return l; return null; } }
}
}
}
}
#endif
