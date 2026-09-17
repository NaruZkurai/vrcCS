#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.CustomEditor(typeof(C_NzkSpsSocket))]
  public class C_NzkSpsSocketEditor : UnityEditor.Editor
  { UnityEditor.SerializedProperty _compatMode,_addLight,_name,_oscId,_enableHandTouch,_length,_unitsInMeters,_addMenuItem,_menuIcon,_enableAuto,_position,_rotation,_depthActions,_activeActions,_useHipAvoidance,_plugLenEnable,_plugLenName,_plugWidEnable,_plugWidName,_prefabPath;
    System.Boolean _showDeformation=true,_showMenu=true,_showDepth=true,_showOgb=true,_showAdvanced=false,_showTags=true;
    void OnEnable()
    { _compatMode=serializedObject.FindProperty("compatMode"); _addLight=serializedObject.FindProperty("V_addLight");
      _name=serializedObject.FindProperty("name"); _oscId=serializedObject.FindProperty("oscId");
      _enableHandTouch=serializedObject.FindProperty("enableHandTouchZone2"); _length=serializedObject.FindProperty("length");
      _unitsInMeters=serializedObject.FindProperty("unitsInMeters"); _addMenuItem=serializedObject.FindProperty("addMenuItem");
      _menuIcon=serializedObject.FindProperty("menuIcon"); _enableAuto=serializedObject.FindProperty("enableAuto");
      _position=serializedObject.FindProperty("position"); _rotation=serializedObject.FindProperty("rotation");
      _depthActions=serializedObject.FindProperty("depthActions2"); _activeActions=serializedObject.FindProperty("activeActions");
      _useHipAvoidance=serializedObject.FindProperty("useHipAvoidance");
      _plugLenEnable=serializedObject.FindProperty("enablePlugLengthParameter"); _plugLenName=serializedObject.FindProperty("plugLengthParameterName");
      _plugWidEnable=serializedObject.FindProperty("enablePlugWidthParameter"); _plugWidName=serializedObject.FindProperty("plugWidthParameterName");
      _prefabPath=serializedObject.FindProperty("_spsPrefabPath"); }
    public override void OnInspectorGUI()
    { serializedObject.Update(); var s=(C_NzkSpsSocket)target;
      UnityEditor.EditorGUILayout.LabelField("SPS Socket / Plug",UnityEditor.EditorStyles.boldLabel);
      UnityEditor.EditorGUILayout.PropertyField(_name,new UnityEngine.GUIContent("Name"));
      /* ── Compatibility Mode (VF-style button row) ── */
      UnityEditor.EditorGUILayout.Space(2);
      UnityEditor.EditorGUILayout.LabelField("Compatibility",UnityEditor.EditorStyles.boldLabel);
      var compatNames=new[]{"All","SPS1","SPS2","TPS","DPS"};
      var compatTips=new[]{"All systems (full VF compat)","Legacy light-based sockets","Shader-based SPS2","The Penetration System","Dynamic Penetration System"};
      UnityEditor.EditorGUILayout.BeginHorizontal();
      for(int i=0;i<compatNames.Length;i++)
      { var sel=s.compatMode==(C_NzkSpsSocket.CompatibilityMode)i;
        var wasOn=sel;
        UnityEngine.GUI.enabled=!sel;
        var style=sel ? UnityEditor.EditorStyles.miniButtonMid : UnityEditor.EditorStyles.miniButton;
        if(UnityEngine.GUILayout.Button(new UnityEngine.GUIContent(compatNames[i],compatTips[i]),style)){ _compatMode.enumValueIndex=i; }
        UnityEngine.GUI.enabled=true; }
      UnityEditor.EditorGUILayout.EndHorizontal();
      /* ── Deformation (SPS) - foldout toggle ── */
      UnityEditor.EditorGUILayout.Space(3);
      _showDeformation=UnityEditor.EditorGUILayout.Foldout(_showDeformation,"Deformation (SPS)",true,UnityEditor.EditorStyles.foldoutHeader);
      if(_showDeformation){ UnityEditor.EditorGUI.indentLevel++;
        UnityEditor.EditorGUILayout.PropertyField(_addLight,new UnityEngine.GUIContent("Mode","Auto/Hole/Ring/None/OneWay"));
      UnityEditor.EditorGUI.indentLevel--; }
      /* ── Menu Toggle - foldout toggle ── */
      UnityEditor.EditorGUILayout.Space(2);
      _showMenu=UnityEditor.EditorGUILayout.Foldout(_showMenu,"Menu Toggle",true,UnityEditor.EditorStyles.foldoutHeader);
      if(_showMenu){ UnityEditor.EditorGUI.indentLevel++;
        UnityEditor.EditorGUILayout.PropertyField(_addMenuItem,new UnityEngine.GUIContent("Add Menu Item"));
        if(s.addMenuItem){ UnityEditor.EditorGUILayout.PropertyField(_enableAuto,new UnityEngine.GUIContent("Auto-select")); UnityEditor.EditorGUILayout.PropertyField(_menuIcon,new UnityEngine.GUIContent("Icon")); }
      UnityEditor.EditorGUI.indentLevel--; }
      /* ── Depth Animations - foldout toggle ── */
      UnityEditor.EditorGUILayout.Space(2);
      _showDepth=UnityEditor.EditorGUILayout.Foldout(_showDepth,"Depth Animations",true,UnityEditor.EditorStyles.foldoutHeader);
      if(_showDepth){ UnityEditor.EditorGUI.indentLevel++;
        UnityEditor.EditorGUILayout.PropertyField(_depthActions,new UnityEngine.GUIContent("Actions"),true);
        UnityEditor.EditorGUILayout.PropertyField(_activeActions,new UnityEngine.GUIContent("Active Animation"),true);
      UnityEditor.EditorGUI.indentLevel--; }
      /* ── OGB Haptics - foldout toggle ── */
      UnityEditor.EditorGUILayout.Space(2);
      _showOgb=UnityEditor.EditorGUILayout.Foldout(_showOgb,"OGB Haptics",true,UnityEditor.EditorStyles.foldoutHeader);
      if(_showOgb){ UnityEditor.EditorGUI.indentLevel++;
        UnityEditor.EditorGUILayout.PropertyField(_oscId,new UnityEngine.GUIContent("OSC ID"));
        UnityEditor.EditorGUILayout.PropertyField(_enableHandTouch,new UnityEngine.GUIContent("Hand Touch Zone"));
        if(s.enableHandTouchZone2!=C_NzkSpsSocket.EnableTouchZone.Off) UnityEditor.EditorGUILayout.PropertyField(_length,new UnityEngine.GUIContent("Touch Depth"));
      UnityEditor.EditorGUI.indentLevel--; }
      /* ── Tags - foldout toggle (VF-style) ── */
      UnityEditor.EditorGUILayout.Space(2);
      _showTags=UnityEditor.EditorGUILayout.Foldout(_showTags,"Tags",true,UnityEditor.EditorStyles.foldoutHeader);
      if(_showTags){ UnityEditor.EditorGUI.indentLevel++;
        UnityEditor.EditorGUILayout.PropertyField(_useHipAvoidance,new UnityEngine.GUIContent("Hip Avoidance"));
      UnityEditor.EditorGUI.indentLevel--; }
      /* ── Advanced (foldout) ── */
      UnityEditor.EditorGUILayout.Space(2);
      _showAdvanced=UnityEditor.EditorGUILayout.Foldout(_showAdvanced,"Advanced",true,UnityEditor.EditorStyles.foldoutHeader);
      if(_showAdvanced){ UnityEditor.EditorGUI.indentLevel++;
        UnityEditor.EditorGUILayout.PropertyField(_position,new UnityEngine.GUIContent("Position"));
        UnityEditor.EditorGUILayout.PropertyField(_rotation,new UnityEngine.GUIContent("Rotation"));
        UnityEditor.EditorGUILayout.PropertyField(_unitsInMeters,new UnityEngine.GUIContent("World-Space Units"));
        UnityEditor.EditorGUILayout.Space(2);
        UnityEditor.EditorGUILayout.LabelField("Plug Parameters",UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.PropertyField(_plugLenEnable,new UnityEngine.GUIContent("Enable Plug Length"));
        if(s.enablePlugLengthParameter) UnityEditor.EditorGUILayout.PropertyField(_plugLenName,new UnityEngine.GUIContent("Length Param"));
        UnityEditor.EditorGUILayout.PropertyField(_plugWidEnable,new UnityEngine.GUIContent("Enable Plug Radius"));
        if(s.enablePlugWidthParameter) UnityEditor.EditorGUILayout.PropertyField(_plugWidName,new UnityEngine.GUIContent("Radius Param"));
        if(_prefabPath!=null && !System.String.IsNullOrEmpty(_prefabPath.stringValue)){ UnityEditor.EditorGUILayout.Space(2); UnityEditor.EditorGUILayout.LabelField("Prefab",_prefabPath.stringValue,UnityEditor.EditorStyles.miniLabel); }
      UnityEditor.EditorGUI.indentLevel--; }
      /* ── Bake button ── */
      UnityEditor.EditorGUILayout.Space(5);
      if(UnityEngine.GUILayout.Button("Bake SPS Socket",UnityEngine.GUILayout.Height(26))){ SpsSocketBaker.Bake(s.gameObject); }
      serializedObject.ApplyModifiedProperties(); }
    void OnSceneGUI()
    { var s=(C_NzkSpsSocket)target; if(s==null) return;
      var wp=s.transform.TransformPoint(s.position);
      var np=UnityEditor.Handles.PositionHandle(wp,s.transform.rotation*UnityEngine.Quaternion.Euler(s.rotation));
      if(UnityEngine.Vector3.Distance(wp,np)>0.001f){ UnityEditor.Undo.RecordObject(s,"Move SPS"); s.position=s.transform.InverseTransformPoint(np); UnityEditor.EditorUtility.SetDirty(s); }
      UnityEditor.Handles.color=UnityEngine.Color.magenta;
      var dir=s.transform.TransformDirection(UnityEngine.Quaternion.Euler(s.rotation)*UnityEngine.Vector3.forward);
      UnityEditor.Handles.DrawDottedLine(wp,wp+dir*(s.length>0.001f?s.length:0.1f),2f); } }
}
}
#endif
