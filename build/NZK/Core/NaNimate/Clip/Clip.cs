#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Clip
  { public static UnityEngine.AnimationClip Scale(System.String objectPath,UnityEngine.Vector3 scale)
  { var clip = new UnityEngine.AnimationClip();
    Overloads.Curve.Set(clip,objectPath,"m_LocalScale.x",scale.x); Overloads.Curve.Set(clip,objectPath,"m_LocalScale.y",scale.y); Overloads.Curve.Set(clip,objectPath,"m_LocalScale.z",scale.z);
    return clip; }
  public static UnityEngine.AnimationClip Active(System.String objectPath,System.Boolean isActive)
  { var clip = new UnityEngine.AnimationClip(); Overloads.Curve.Set(clip,objectPath,typeof(UnityEngine.GameObject),"m_IsActive",isActive ? 1f : 0f); return clip; }
  public static UnityEngine.AnimationClip Get(System.String p)
  { var c = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(p); if (c != null) return c;
    if (System.IO.File.Exists(NaNimate.Paths.Abs(p))) { UnityEditor.AssetDatabase.ImportAsset(p,UnityEditor.ImportAssetOptions.ForceSynchronousImport); return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(p); }
    return null; }
  public static UnityEngine.AnimationClip Create(System.String p,UnityEngine.AnimationClip src)
    { UnityEditor.AssetDatabase.CreateAsset(src,p); return src; }
  /* uber important function GllC get or create file :D*/
  public static UnityEngine.AnimationClip GllC(System.String p,UnityEngine.AnimationClip src) => NaNimate.Clip.Get(p) ?? NaNimate.Clip.Create(p,src); }
}
}
}
#endif
