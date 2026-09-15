#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Paths
  { public static System.String Abs(System.String assetPath) => assetPath.StartsWith("Assets/",System.StringComparison.Ordinal) ? System.IO.Path.Combine(UnityEngine.Application.dataPath,assetPath.Substring(7).Replace('/',System.IO.Path.DirectorySeparatorChar)) : System.IO.Path.GetFullPath(assetPath); }
}
}
}
#endif
