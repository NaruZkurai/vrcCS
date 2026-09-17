#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class C_AviGenerator : UnityEngine.MonoBehaviour
  {
  // ============================================================
  // Moved from C_AviGenerator.Yaml2.cs — editor-only methods
  // ============================================================
  public void BuildAXControllerFromHierarchy()
  { var ctrlRoots = GetAllControllerRoots();
    foreach (var ctrl in ctrlRoots)
    { var ctrlHB = ctrl.GetComponent<C_AviGenerator>();
    if (ctrlHB == null) continue;
    /* Clear existing controller data to rebuild fresh */
    ctrlHB.acParams.Clear();
    ctrlHB.acLayers.Clear();
    /* Import parameters from reference controller if available (UnityEditor.Editor-only) */
    var refCtrl = ctrlHB.gestureReferenceController as UnityEditor.Animations.AnimatorController;
    if (refCtrl != null)
    { foreach (var p in refCtrl.parameters)
      { if (!System.Linq.Enumerable.Any(ctrlHB.acParams, x => x.name == p.name))
        ctrlHB.acParams.Add(new ACParam { name = p.name,type = (int)p.type,value = p.defaultFloat }); }
    }
    /* Helper to process children of a given UnityEngine.Transform (skips YamlBlocks/layers containers) */
    System.Action<UnityEngine.Transform> ProcessChildren = null;
    ProcessChildren = (parent) =>
    {
      foreach (UnityEngine.Transform child in parent)
      { if (child.name == "YamlBlocks" || child.name == "layers") continue;
      var childHB = child.GetComponent<C_AviGenerator>();
      if (childHB == null) continue;
      switch (childHB.cbBlockType)
      { case CbBlockType.AController:
        /* Root controller — collect params from children */
        /* Walk sub-states */
        foreach (UnityEngine.Transform sub in child)
        { var subHB = sub.GetComponent<C_AviGenerator>();
          if (subHB == null) continue;
          if (subHB.cbBlockType == CbBlockType.SubState && subHB.animationClip != null)
        break; }
        break; }
      }
    };
    /* Process direct children; also look inside layers/ container */
    ProcessChildren(ctrl);
    var layersFolder = ctrl.Find("layers");
    if (layersFolder != null) ProcessChildren(layersFolder);
    IF_UE.SetDirty(ctrlHB); }
    UnityEngine.Debug.Log("[HB] AXController built from " + ctrlRoots.Count + " controller roots."); }
  public static void SyncYamlToHierarchy(YamlBlock yb)
  { if (yb == null || System.String.IsNullOrEmpty(yb.blockName)) return;
    UnityEngine.Transform ctrlRoot = FindNearestControllerRoot(yb);
    if (ctrlRoot == null) return;
    var ctrlHB = ctrlRoot.GetComponent<C_AviGenerator>();
    if (ctrlHB == null) return;
    var match = FindYamlBlockMatchStatic(yb,ctrlRoot);
    if (match == null) return;
    var matchHB = match.GetComponent<C_AviGenerator>();
    if (matchHB == null) return;
    var ts = matchHB.GetComponent<TransitionSet>();
    foreach (var line in yb.rawText.Split('§'))
    { var tl = line.Trim();
    if (!tl.StartsWith("m_")) continue;
    var parts = tl.Split(':');
    if (parts.Length < 2) continue;
    var fn = parts[0].Trim();
    var val = parts[1].Trim();
    if (fn == "m_Motion" && val.Contains("guid:"))
    { int gs = val.IndexOf("guid: ") + 6;
      int ge = val.IndexOf(',',gs);
      if (ge > gs)
      { System.String guid = val.Substring(gs,ge - gs).Trim();
      System.String path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
      if (!System.String.IsNullOrEmpty(path))
      { var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
        if (clip != null) matchHB.animationClip = clip; }
      }
    }
    else if (ts != null && (fn == "m_HasExitTime" || fn == "m_HasFixedDuration"
      || fn == "m_CanTransitionToSelf" || fn == "m_IsExit"))
    { System.Boolean bv = val == "1" || val == "true";
      if (fn == "m_HasExitTime") ts.hasExitTime = bv;
      else if (fn == "m_HasFixedDuration") ts.hasFixedDuration = bv;
      else if (fn == "m_CanTransitionToSelf") ts.canTransitionToSelf = bv;
      else if (fn == "m_IsExit") ts.isExit = bv; }
    else if (ts != null && (fn == "m_Duration" || fn == "m_ExitTime" || fn == "m_TransitionDuration" || fn == "m_TransitionOffset"))
    { float fv; System.Single.TryParse(val,out fv);
      if (fn == "m_Duration") ts.duration = fv;
      else if (fn == "m_ExitTime") ts.exitTime = fv; }
    else if (fn == "m_WriteDefaultValues")
    { matchHB.writeDefaultValues = val == "1" || val == "true"; }
    else if (fn == "m_DefaultWeight" || fn == "m_Weight")
    { float fv; if (System.Single.TryParse(val,out fv)) matchHB.weight = fv; }
    }
  }
  }
}
}
#endif
