#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.CustomEditor(typeof(C_NZKToolkit))]
public class C_NZKToolkitEditor : UnityEditor.Editor
{C_NZKToolkit _t;
UnityEditor.SerializedProperty _spBaker;
UnityEditor.Editor _bakerEditor;
void OnEnable()
{_t = (C_NZKToolkit)target;
_spBaker = serializedObject.FindProperty("NZKTK_ScriptableComponent");}
void EnsureBakerEditor()
{if (_t.NZKTK_ScriptableComponent == null) { _bakerEditor = null; return; }
if (_bakerEditor == null || _bakerEditor.target != _t.NZKTK_ScriptableComponent)
{_bakerEditor = null;
UnityEditor.Editor.CreateCachedEditor(_t.NZKTK_ScriptableComponent,null,ref _bakerEditor);}}
public override void OnInspectorGUI()
{if (_t == null) return;
serializedObject.Update();
EnsureBakerEditor();
/* ── Avatar Root ── */
UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("NZKTK_AviRoot"),new UnityEngine.GUIContent("Avatar Root"));
/* ── Baker component ── */
UnityEditor.EditorGUILayout.BeginHorizontal();
UnityEditor.EditorGUILayout.PropertyField(_spBaker,new UnityEngine.GUIContent("Higharchy Baker"));
if (UnityEngine.GUILayout.Button("Auto-Create",UnityEngine.GUILayout.Width(85)))
{_t.EnsureBaker();UnityEditor.EditorUtility.SetDirty(_t);EnsureBakerEditor();}
UnityEditor.EditorGUILayout.EndHorizontal();
/* ── Draw the full C_AviGenerator inspector (cached editor) ── */
if (_bakerEditor != null)
{UnityEditor.EditorGUILayout.Space(4);
_bakerEditor.OnInspectorGUI();}
else
{UnityEditor.EditorGUILayout.HelpBox("No Higharchy Baker. Click Auto-Create to add one.",UnityEditor.MessageType.Warning);}
/* ── Bake toggles ── */
UnityEditor.EditorGUILayout.Space(4);
UnityEditor.EditorGUILayout.LabelField("── Bake Options ──",UnityEditor.EditorStyles.boldLabel);
UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeAviRoot"),new UnityEngine.GUIContent("Bake Avatar Root"));
UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeMesh"),new UnityEngine.GUIContent("Bake Mesh"));
UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeToggles"),new UnityEngine.GUIContent("Bake Toggles"));
UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeArmature"),new UnityEngine.GUIContent("Bake Armature"));
UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeControllers"),new UnityEngine.GUIContent("Bake Controllers"));
serializedObject.ApplyModifiedProperties();}}
}
}
#endif
