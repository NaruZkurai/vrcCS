#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class Overloads {
public static class Curve
  { public static void Set(UnityEngine.AnimationClip clip,System.String path,System.String prop,float value) => Overloads.Curve.Set(clip,path,typeof(UnityEngine.Transform),prop,value);
  public static void Set(UnityEngine.AnimationClip clip,System.String path,System.Type t,System.String prop,float value) { UnityEditor.AnimationUtility.SetEditorCurve(clip,UnityEditor.EditorCurveBinding.FloatCurve(path,t,prop),new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f,value))); } }
}
}
}
#endif
