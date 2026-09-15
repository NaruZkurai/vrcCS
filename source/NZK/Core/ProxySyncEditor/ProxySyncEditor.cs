#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.CustomEditor(typeof(ProxySync))]
  class ProxySyncEditor : UnityEditor.Editor
  {   ProxySync _s;
    System.Boolean    _showOriginal;
    void OnEnable()
    {   _s = (ProxySync)target;
      if (_s != null) ProxySyncOps.EnforceChildrenDisabled(_s);   }
    public override void OnInspectorGUI()
    {   if (_s == null) return;
      serializedObject.Update();
      /* ---- Swap Now button ----------------------------------------- */
      var prevBg = UnityEngine.GUI.backgroundColor;
      UnityEngine.GUI.backgroundColor = new UnityEngine.Color(0.55f,0.9f,0.55f);
      if (UnityEngine.GUILayout.Button("▶  Swap Now",UnityEngine.GUILayout.Height(30)))
      {   System.Boolean ok = ProxySyncOps.PerformSwap(_s);
        if (!ok)
          UnityEngine.Debug.LogWarning("[NZK] Proxy Sync: No swap performed. Target may already have swap (B) values, or a required component is missing.");   }
      UnityEngine.GUI.backgroundColor = prevBg;
      UnityEditor.EditorGUILayout.Space(6);
      /* ---- Target & component type ---------------------------------- */
      UnityEditor.EditorGUILayout.PropertyField(
        serializedObject.FindProperty("targetObject"),new UnityEngine.GUIContent("Target Object"));
      UnityEditor.EditorGUILayout.PropertyField(
        serializedObject.FindProperty("componentType"),new UnityEngine.GUIContent("Component Type"));
      UnityEditor.EditorGUILayout.Space(4);
      /* ---- Swap source (B) preview ---------------------------------- */
      var swapSMR = _s.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (swapSMR == null)
      {   UnityEditor.EditorGUILayout.HelpBox(
          "Add a UnityEngine.SkinnedMeshRenderer to this UnityEngine.GameObject with the " +
          "desired (B) values.  This script never modifies it.",UnityEditor.MessageType.Warning);   }
      else
      {   UnityEditor.EditorGUILayout.LabelField(
          "Swap Source  (B — never modified by script)",UnityEditor.EditorStyles.boldLabel);
        { var ds__ = new UnityEditor.EditorGUI.DisabledScope(true); try
        {   UnityEditor.EditorGUILayout.ObjectField(
            "UnityEngine.Mesh",swapSMR.sharedMesh,typeof(UnityEngine.Mesh),false);
          var mats = swapSMR.sharedMaterials;
          for (int i = 0; i < mats.Length; i++)
            UnityEditor.EditorGUILayout.ObjectField(
              "UnityEngine.Material " + i,mats[i],typeof(UnityEngine.Material),false); }
        finally { ((System.IDisposable)ds__).Dispose(); } }   }
      /* ---- Stored original (A) -------------------------------------- */
      UnityEditor.EditorGUILayout.Space(4);
      if (_s.HasOriginal)
      {   _showOriginal = UnityEditor.EditorGUILayout.Foldout(
          _showOriginal,"Stored Original  (A)");
        if (_showOriginal)
        { var ds__ = new UnityEditor.EditorGUI.DisabledScope(true); try
          {   UnityEditor.EditorGUILayout.ObjectField(
              "UnityEngine.Mesh",_s.OriginalMesh,typeof(UnityEngine.Mesh),false);
            var om = _s.OriginalMats;
            if (om != null)
              for (int i = 0; i < om.Length; i++)
                UnityEditor.EditorGUILayout.ObjectField(
                  "UnityEngine.Material " + i,om[i],typeof(UnityEngine.Material),false); }
          finally { ((System.IDisposable)ds__).Dispose(); }
          UnityEditor.EditorGUILayout.Space(2);
          if (UnityEngine.GUILayout.Button("Re-capture Original from Target"))
            ProxySyncOps.RecaptureOriginal(_s);   }   }
      else
      {   UnityEditor.EditorGUILayout.HelpBox(
          "Original (A) not yet stored — captured on first swap.",UnityEditor.MessageType.Info);   }
      serializedObject.ApplyModifiedProperties();   }   }
}
}
#endif
