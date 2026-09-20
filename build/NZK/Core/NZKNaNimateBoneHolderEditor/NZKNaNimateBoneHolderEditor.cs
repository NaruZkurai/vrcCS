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
      /* THE THREE THINGS, GROUPED AND LABELLED BY WHAT THEY ARE.
       *
       * They were previously one field plus a silent derivation, and the
       * inspector is where that ambiguity became invisible: the component said
       * "Target Avatar" while its bones belonged to a different avatar, and
       * nothing on screen showed the disagreement.  Naming all three makes the
       * mismatch readable before a run rather than after.
       *
       * All three are optional in the common case - each derives itself from
       * the bones and the mesh names - so they are shown as overrides. */
      UnityEditor.EditorGUILayout.LabelField("Scene", UnityEditor.EditorStyles.boldLabel);
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneAvatar"),
        new UnityEngine.GUIContent("Scene Avatar", "The avatar in this scene whose renderers get repaired. " +
                                                   "Derived from the bones when left empty."));
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Bones"),
        new UnityEngine.GUIContent("NaNimation Bones", "The bones the toggles drive. These decide which " +
                                                       "avatar is repaired, so they are the authority."), true);
      UnityEditor.EditorGUILayout.Space(4);
      UnityEditor.EditorGUILayout.LabelField("Source Model", UnityEditor.EditorStyles.boldLabel);
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectModel"),
        new UnityEngine.GUIContent("Project Model", "The .blend/.fbx the avatar is built from. Set this " +
                                                    "ONLY to break a tie between models that share mesh names."));
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("SourceModels"),
        new UnityEngine.GUIContent("Source Models", "Derived automatically from the renderers' mesh names. " +
                                                    "Drag models here to pin them explicitly."), true);
      if (UnityEngine.GUILayout.Button("Capture Source Models From Selection"))
      { h.CaptureSourceModels(); serializedObject.Update(); }
      UnityEditor.EditorGUILayout.Space(4);
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoRun"),
        new UnityEngine.GUIContent("Run Automatically", "On scene load and after imports"));
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Verbose"));
      UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("WriteAssets"),
        new UnityEngine.GUIContent("Write Asset Files (slow)",
          "OFF by default. The repair runs in memory and writes nothing, which takes milliseconds. " +
          "Turning this on also generates reusable .asset meshes + a prefab and reimports every model, " +
          "which was measured at ~33 seconds on a 39-mesh avatar."));
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
