#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class C_AviGenerator : UnityEngine.MonoBehaviour
  {
  // ============================================================
  // Moved from C_AviGenerator.Yaml.cs — editor-only YAML sync
  // ============================================================
  /** Also auto-detects duplicate values and promotes them to HBFlag entries.</summary> */
  public void BuildYamlBlocks()
  { var ctrlRoots = GetAllControllerRoots();
    if (ctrlRoots.Count == 0) { UnityEngine.Debug.Log("[HB] BuildYamlBlocks: No controller generators found."); return; }
    int totalUpdated = 0;
    foreach (var ctrlRoot in ctrlRoots)
    totalUpdated += BuildYamlBlocksForRoot(ctrlRoot);
    UnityEngine.Debug.Log("[HB] BuildYamlBlocks: updated " + totalUpdated + " blocks across " + ctrlRoots.Count + " controllers.");
    /* Phase 2: sync CbBlockType from YAML types */
    SyncCbBlockTypeFromYaml();
    /* Phase 3: build AXController from CbBlockType hierarchy */
    BuildAXControllerFromHierarchy();
    DetectAndPromoteDuplicates(ctrlRoots); }
  /** <summary>Build YamlBlocks for a single controller root (Gesture/Expressions/FX).</summary> */
  int BuildYamlBlocksForRoot(UnityEngine.Transform ctrlRoot)
  { int updated = 0;
    var ybParent = ctrlRoot.Find("YamlBlocks");
    if (ybParent == null) return 0;
    var flags = ctrlRoot.GetComponent<C_HBFlags>();
    System.String layerName = C_HBFlagUtils.S_GetLayerName(ctrlRoot.GetComponent<C_AviGenerator>());
    /* Walk all YB_ children and update matching hierarchy objects */
    foreach (UnityEngine.Transform ybChild in ybParent)
    {
    var yb = ybChild.GetComponent<YamlBlock>();
    if (yb == null) continue;
    /* Find matching hierarchy object by blockName */
    if (System.String.IsNullOrEmpty(yb.blockName)) continue;
    UnityEngine.GameObject hbMatch = FindYamlBlockMatch(yb,ctrlRoot);
    if (hbMatch == null) continue;
    var matchHB = hbMatch.GetComponent<C_AviGenerator>();
    if (matchHB == null) continue;
    System.Boolean didChange = false;
    /* Update clip reference in YAML if state has animationClip (UnityEditor.Editor-only) */
    if (matchHB.animationClip != null && yb.typeTag == "1102")
    {
      System.String clipPath = UnityEditor.AssetDatabase.GetAssetPath(matchHB.animationClip);
      if (!System.String.IsNullOrEmpty(clipPath))
      {
      System.String clipGuid = UnityEditor.AssetDatabase.AssetPathToGUID(clipPath);
      if (!System.String.IsNullOrEmpty(clipGuid))
      {
        var lines = yb.rawText.Split('§');
        for (int i = 0; i < lines.Length; i++)
        {
        if (lines[i].Trim().StartsWith("m_Motion:"))
        {
          lines[i] = "  m_Motion: {fileID: 7400000,guid: " + clipGuid + ",type: 2}";
          didChange = true;
          break; }
        }
        if (didChange) yb.rawText = System.String.Join("§",lines); }
      }
    }
    /* Sync TransitionSet data → YAML transition blocks */
    if ((yb.typeTag == "1101" || yb.typeTag == "1107") && matchHB != null)
    {
      var ts = matchHB.GetComponent<TransitionSet>();
      if (ts != null)
      {
      var lines = yb.rawText.Split('§');
      System.Boolean changed = false;
      for (int i = 0; i < lines.Length; i++)
      {
        var t = lines[i].Trim();
        if (!t.StartsWith("m_")) continue;
        var fn = t.Split(':')[0].Trim();
        switch (fn)
        {
        case "m_Duration":     var nd = ts.duration.ToString("G"); if (nd != ParseYamlValue(t)) { lines[i] = "  m_Duration: " + nd; changed = true; } break;
        case "m_HasExitTime":    var he = ts.hasExitTime ? "1" : "0"; if (he != ParseYamlValue(t)) { lines[i] = "  m_HasExitTime: " + he; changed = true; } break;
        case "m_HasFixedDuration": var hf = ts.hasFixedDuration ? "1" : "0"; if (hf != ParseYamlValue(t)) { lines[i] = "  m_HasFixedDuration: " + hf; changed = true; } break;
        case "m_ExitTime":     var et = ts.exitTime.ToString("G"); if (et != ParseYamlValue(t)) { lines[i] = "  m_ExitTime: " + et; changed = true; } break;
        case "m_CanTransitionToSelf": var ct = ts.canTransitionToSelf ? "1" : "0"; if (ct != ParseYamlValue(t)) { lines[i] = "  m_CanTransitionToSelf: " + ct; changed = true; } break;
        case "m_IsExit":       var ie = ts.isExit ? "1" : "0"; if (ie != ParseYamlValue(t)) { lines[i] = "  m_IsExit: " + ie; changed = true; } break; }
      }
      if (changed) { yb.rawText = System.String.Join("§",lines); didChange = true; }
      }
    }
    /* Sync controller-level properties (acParams,acLayers,mask,weight) */
    if (yb.typeTag == "91") // UnityEditor.Animations.AnimatorController
    {
      var lines = yb.rawText.Split('§');
      System.Boolean changed = false;
      for (int i = 0; i < lines.Length; i++)
      {
      var t = lines[i].Trim();
      if (!t.StartsWith("m_")) continue;
      var fn = t.Split(':')[0].Trim();
      /* m_WriteDefaultValues is on the controller itself */
      if (fn == "m_WriteDefaultValues" && matchHB != null)
      {
        var nv = matchHB.writeDefaultValues ? "1" : "0";
        if (nv != ParseYamlValue(t)) { lines[i] = "  m_WriteDefaultValues: " + nv; changed = true; }
      }
      }
      if (changed) { yb.rawText = System.String.Join("§",lines); didChange = true; }
    }
    /* Sync for UnityEditor.Animations.AnimatorState (typeTag 1102) — writeDefaultValues + speed */
    if (yb.typeTag == "1102" && matchHB != null)
    {
      var lines = yb.rawText.Split('§');
      System.Boolean changed = false;
      for (int i = 0; i < lines.Length; i++)
      {
      var t = lines[i].Trim();
      if (!t.StartsWith("m_")) continue;
      var fn = t.Split(':')[0].Trim();
      if (fn == "m_WriteDefaultValues")
      {
        var nv = matchHB.writeDefaultValues ? "1" : "0";
        if (nv != ParseYamlValue(t)) { lines[i] = "  m_WriteDefaultValues: " + nv; changed = true; }
      }
      }
      if (changed) { yb.rawText = System.String.Join("§",lines); didChange = true; }
    }
    /* Apply HBFlag overrides to this block */
    if (flags != null)
    {
      var lines = yb.rawText.Split('§');
      System.Boolean changed = false;
      for (int i = 0; i < lines.Length; i++)
      {
      var t = lines[i].Trim();
      if (!t.StartsWith("m_")) continue;
      var fn = t.Split(':')[0].Trim();
      System.String fv = flags.S_Resolve(fn,layerName);
      if (fv != null)
      {
        lines[i] = "  " + fn + ": " + fv;
        changed = true; }
      }
      if (changed) { yb.rawText = System.String.Join("§",lines); didChange = true; }
    }
    if (didChange) { IF_UE.SetDirty(yb); updated++; }
    }
    /* After updating blocks,push changes back to Controller Builder objects */
    SyncYamlBlocksToControllerBuilders(ybParent,ctrlRoot);
    return updated; }
  void SyncYamlBlocksToControllerBuilders(UnityEngine.Transform ybParent,UnityEngine.Transform ctrlRoot)
  { foreach (UnityEngine.Transform ybChild in ybParent)
    {
    var yb = ybChild.GetComponent<YamlBlock>();
    if (yb == null || System.String.IsNullOrEmpty(yb.blockName)) continue;
    UnityEngine.GameObject hbMatch = FindYamlBlockMatch(yb,ctrlRoot);
    if (hbMatch == null) continue;
    var matchHB = hbMatch.GetComponent<C_AviGenerator>();
    if (matchHB == null) continue;
    System.Boolean dirty = false;
    foreach (var line in yb.rawText.Split('§'))
    {
      var tl = line.Trim();
      if (!tl.StartsWith("m_")) continue;
      var parts = tl.Split(':');
      if (parts.Length < 2) continue;
      var fn = parts[0].Trim();
      var val = parts[1].Trim();
      /* Sync m_Motion → animationClip */
      if (fn == "m_Motion" && val.Contains("guid:"))
      {
      int gs = val.IndexOf("guid: ") + 6;
      int ge = val.IndexOf(',',gs);
      if (ge > gs)
      {
        System.String guid = val.Substring(gs,ge - gs).Trim();
        System.String path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
        if (!System.String.IsNullOrEmpty(path))
        {
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
        if (clip != null && matchHB.animationClip != clip)
        { matchHB.animationClip = clip; dirty = true; }
        }
      }
      }
      /* Sync m_WriteDefaultValues */
      else if (fn == "m_WriteDefaultValues")
      {
      System.Boolean bv = val == "1" || val == "true";
      if (matchHB.writeDefaultValues != bv)
      { matchHB.writeDefaultValues = bv; dirty = true; }
      }
      /* Sync m_Weight (layer weight) */
      else if (fn == "m_DefaultWeight" || fn == "m_Weight")
      {
      float fv; if (System.Single.TryParse(val,out fv) && System.Math.Abs(matchHB.weight - fv) > 0.0001f)
      { matchHB.weight = fv; dirty = true; }
      }
    }
    /* Sync TransitionSet values */
    var ts = matchHB.GetComponent<TransitionSet>();
    if (ts != null)
    {
      foreach (var line in yb.rawText.Split('§'))
      {
      var tl = line.Trim();
      if (!tl.StartsWith("m_")) continue;
      var parts = tl.Split(':');
      if (parts.Length < 2) continue;
      var fn = parts[0].Trim();
      var val = parts[1].Trim();
      if (fn == "m_Duration")     { float fv; if (System.Single.TryParse(val,out fv) && System.Math.Abs(ts.duration - fv) > 0.0001f) { ts.duration = fv; dirty = true; } }
      else if (fn == "m_HasExitTime")    { System.Boolean bv = val == "1"; if (ts.hasExitTime != bv) { ts.hasExitTime = bv; dirty = true; } }
      else if (fn == "m_HasFixedDuration") { System.Boolean bv = val == "1"; if (ts.hasFixedDuration != bv) { ts.hasFixedDuration = bv; dirty = true; } }
      else if (fn == "m_ExitTime")     { float fv; if (System.Single.TryParse(val,out fv) && System.Math.Abs(ts.exitTime - fv) > 0.0001f) { ts.exitTime = fv; dirty = true; } }
      else if (fn == "m_CanTransitionToSelf") { System.Boolean bv = val == "1"; if (ts.canTransitionToSelf != bv) { ts.canTransitionToSelf = bv; dirty = true; } }
      else if (fn == "m_IsExit")       { System.Boolean bv = val == "1"; if (ts.isExit != bv) { ts.isExit = bv; dirty = true; } }
      }
    }
    if (dirty) IF_UE.SetDirty(matchHB); }
  }
  }
}
}
#endif
