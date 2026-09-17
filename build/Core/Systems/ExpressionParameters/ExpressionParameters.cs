#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class ExpressionParameters { public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters Get(System.String path) => UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(path); }
}
}
}
#endif
