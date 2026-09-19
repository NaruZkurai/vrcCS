#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.CustomEditor(typeof(NZKNaNimateBoneHolder))]
  public class NZKNaNimateBoneHolderEditor : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    { NZKNaNimateBoneHolder h = (NZKNaNimateBoneHolder)target;
      serializedObject.Update();
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Target"),
        new UnityEngine.GUIContent("Target Avatar", "Leave empty to use this object's scene root"));
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("SourceModels"),
        new UnityEngine.GUIContent("Source Models", "The .blend/.fbx assets. drag them in by hand, or capture while the renderers still point at the model"), true);
      if (UnityEngine.GUILayout.Button("Capture Source Models From Selection"))
      { h.CaptureSourceModels(); serializedObject.Update(); }
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Bones"),
        new UnityEngine.GUIContent("NaNimation Bones"), true);
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoRun"),
        new UnityEngine.GUIContent("Run Automatically", "On scene load and after imports"));
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Verbose"));
      serializedObject.ApplyModifiedProperties();
      UnityEditor.EditorGUILayout.Space(6);
      System.String why;
      System.Boolean can = h.CanRun(out why);
      if (!can)
        UnityEditor.EditorGUILayout.HelpBox("Will not run: " + why, UnityEditor.MessageType.Warning);
      else
      { UnityEngine.GameObject t = h.ResolveTarget();
        UnityEditor.EditorGUILayout.HelpBox("Ready. Avatar: " + (t != null ? t.name : "(none)") +
                                            "\nBones: " + h.Bones.Length,
                                            UnityEditor.MessageType.Info); }
      if (UnityEngine.GUILayout.Button("Reimport + Generate + Normalize", UnityEngine.GUILayout.Height(28)))
        h.RunNow(); }
  }
}
}
#endif
