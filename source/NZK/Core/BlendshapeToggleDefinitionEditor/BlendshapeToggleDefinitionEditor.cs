#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.CustomEditor(typeof(BlendshapeToggleDefinition))]
  public class BlendshapeToggleDefinitionEditor : UnityEditor.Editor
  {
  UnityEditor.SerializedProperty targetRendererProp,blendshapeNameProp,onValueProp,offValueProp,parameterNameProp,isMultiProp;
  void OnEnable()
  { targetRendererProp = serializedObject.FindProperty("targetRenderer");
    blendshapeNameProp = serializedObject.FindProperty("blendshapeName");
    onValueProp = serializedObject.FindProperty("onValue");
    offValueProp = serializedObject.FindProperty("offValue");
    parameterNameProp = serializedObject.FindProperty("parameterName");
    isMultiProp = serializedObject.FindProperty("isMulti"); }
  public override void OnInspectorGUI()
  { serializedObject.Update();
    var btd = (BlendshapeToggleDefinition)target;
    UnityEditor.EditorGUILayout.PropertyField(targetRendererProp,new UnityEngine.GUIContent("Target UnityEngine.Renderer"));
    if (btd.targetRenderer != null)
    { var mesh = btd.targetRenderer.sharedMesh;
    if (mesh != null && mesh.blendShapeCount > 0)
    { var names = new System.String[mesh.blendShapeCount];
      for (int i = 0; i < mesh.blendShapeCount; i++) names[i] = mesh.GetBlendShapeName(i);
      var idx = System.Array.IndexOf(names,btd.blendshapeName);
      var sel = UnityEditor.EditorGUILayout.Popup("Blend Shape",idx < 0 ? 0 : idx,names);
      if (sel >= 0 && sel < names.Length) blendshapeNameProp.stringValue = names[sel]; }
    else UnityEditor.EditorGUILayout.HelpBox("No blend shapes on this mesh.",UnityEditor.MessageType.Info); }
    else UnityEditor.EditorGUILayout.PropertyField(blendshapeNameProp,new UnityEngine.GUIContent("Blend Shape Name"));
    onValueProp.floatValue = UnityEditor.EditorGUILayout.Slider("On Value",onValueProp.floatValue,0f,100f);
    offValueProp.floatValue = UnityEditor.EditorGUILayout.Slider("Off Value",offValueProp.floatValue,0f,100f);
    UnityEditor.EditorGUILayout.PropertyField(parameterNameProp,new UnityEngine.GUIContent("Parameter Name"));
    UnityEditor.EditorGUILayout.PropertyField(isMultiProp,new UnityEngine.GUIContent("Is Multi"));
    if (UnityEngine.GUILayout.Button("Generate Toggle"))
    { var avatarName = BTH.S(btd.gameObject.transform.root.name);
    var result = BlendShapeToggleGenerator.GenerateToggle(btd.gameObject,avatarName,null);
    if (!System.String.IsNullOrEmpty(result)) { IF_UE.SetDirty(btd); IF_UE.SaveAndRefresh(); NaNimate.Update.DBTs(avatarName); NaNimate.Update.Sync(avatarName); UnityEngine.Debug.Log("Blendshape toggle generated: " + result); }
    }
    serializedObject.ApplyModifiedProperties(); }
  }
}
}
#endif
