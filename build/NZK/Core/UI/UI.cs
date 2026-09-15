#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class UI
  {/* Renders a section header with an optional tooltip */
  public static void Section(System.String label,System.String tooltip = null)
  { UnityEditor.EditorGUILayout.Space(4);
    UnityEditor.EditorGUILayout.LabelField(label,UnityEditor.EditorStyles.boldLabel);
    if (!System.String.IsNullOrEmpty(tooltip))
    UnityEditor.EditorGUILayout.HelpBox(tooltip,UnityEditor.MessageType.Info);
    UnityEditor.EditorGUILayout.Space(2); }
  /* Bone-viewer-style drag-and-drop array field */
  public static void DropArray(UnityEditor.SerializedProperty prop,System.String label,System.String tooltip = null)
  { if (prop == null) return;
    UnityEditor.EditorGUILayout.PropertyField(prop,new UnityEngine.GUIContent(label,tooltip ?? ""),true);
    var rect = UnityEditor.EditorGUILayout.GetControlRect(false,UnityEditor.EditorGUIUtility.singleLineHeight);
    rect = UnityEditor.EditorGUI.IndentedRect(rect);
    var ev = UnityEngine.Event.current;
    if (rect.Contains(ev.mousePosition) &&
      (ev.type == UnityEngine.EventType.DragUpdated ||
       ev.type == UnityEngine.EventType.DragPerform))
    { if (ev.type == UnityEngine.EventType.DragUpdated)
    { UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
      ev.Use(); }
    else if (ev.type == UnityEngine.EventType.DragPerform)
    { UnityEditor.DragAndDrop.AcceptDrag();
      ev.Use();
      var dragged = new System.Collections.Generic.List<UnityEngine.Transform>();
      foreach (var obj in UnityEditor.DragAndDrop.objectReferences)
      { if (obj is UnityEngine.GameObject go && go.transform != null)
        dragged.Add(go.transform);
      else if (obj is UnityEngine.Transform t)
        dragged.Add(t); }
      if (dragged.Count > 0)
      { prop.ClearArray(); prop.arraySize = dragged.Count;
      for (int i = 0; i < dragged.Count; i++)
        prop.GetArrayElementAtIndex(i).objectReferenceValue = dragged[i];
      prop.serializedObject.ApplyModifiedProperties(); }
      UnityEditor.EditorGUIUtility.ExitGUI(); } }
    var hintRect = rect;
    hintRect.height = UnityEditor.EditorGUIUtility.singleLineHeight;
    UnityEditor.EditorGUI.LabelField(hintRect,"  \u25BC Drag objects here",new UnityEngine.GUIStyle(UnityEditor.EditorStyles.miniLabel)
    { normal = { textColor = UnityEngine.Color.grey } }); }
  /* Renders a button with optional color tint. Returns true when clicked. */
  public static System.Boolean Button(System.String label,System.String tooltip = null,UnityEngine.Color? color = null,float height = 24f)
  { var prev = UnityEngine.GUI.backgroundColor;
    if (color.HasValue) UnityEngine.GUI.backgroundColor = color.Value;
    System.Boolean clicked = UnityEngine.GUILayout.Button(
    new UnityEngine.GUIContent(label,tooltip ?? ""),UnityEngine.GUILayout.Height(height));
    UnityEngine.GUI.backgroundColor = prev;
    return clicked; }
  /* Renders a text field with label. Returns the modified System.String. */
  public static System.String TextField(System.String label,System.String value,System.String tooltip = null)
  { return UnityEditor.EditorGUILayout.TextField(
    new UnityEngine.GUIContent(label,tooltip ?? ""),value); }
  /* Renders a toggle/checkbox with label. Returns the modified value. */
  public static System.Boolean Toggle(System.String label,System.Boolean value,System.String tooltip = null)
  { return UnityEditor.EditorGUILayout.Toggle(
    new UnityEngine.GUIContent(label,tooltip ?? ""),value); }
  }
}
}
#endif
