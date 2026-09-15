#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class FX
  { public static UnityEditor.Animations.AnimatorController Get(System.String path)
    => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
  public static UnityEditor.Animations.AnimatorController Create(System.String path)
    => UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
  public static UnityEditor.Animations.AnimatorController GllC(System.String path)
    => NaNimate.FX.Get(path) ?? NaNimate.FX.Create(path); }
}
}
}
#endif
