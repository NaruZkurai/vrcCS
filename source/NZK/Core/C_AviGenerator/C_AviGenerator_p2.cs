namespace NZK
{
public static partial class Core {
public partial class C_AviGenerator : UnityEngine.MonoBehaviour
  {
    UnityEngine.GameObject FindYamlBlockMatch(YamlBlock yb, UnityEngine.Transform gestRoot)
    {
      if (System.String.IsNullOrEmpty(yb.blockName)) return null;
      /* Determine which container holds layer objects */
      UnityEngine.Transform layersContainer = gestRoot.Find("layers");
      if (layersContainer == null) layersContainer = gestRoot; // fallback to root
      /* Search all layers (either under layers/ container,or direct children) */
      foreach (UnityEngine.Transform layer in layersContainer)
      {
        if (layer.name == "YamlBlocks" || layer.name == "layers") continue;
        /* Check if this layer's name matches */
        if (yb.blockName == layer.name)
        {
          var lhb = layer.GetComponent<C_AviGenerator>();
          if (lhb != null) return layer.gameObject;
        }
        /* Search sub-SMs and states within this layer */
        foreach (UnityEngine.Transform sub in layer)
        {
          if (sub.name == "Entry" || sub.name == "Exit" || sub.name == "Up" || sub.name == "AnyState") continue;
          if (yb.blockName == sub.name) return sub.gameObject;
          /* Search deeper (states within sub-SMs) */
          foreach (UnityEngine.Transform state in sub)
          {
            if (state.name == "Entry" || state.name == "Exit" || state.name == "Up" || state.name == "AnyState") continue;
            if (yb.blockName == state.name) return state.gameObject;
          }
        }
      }
      return null;
    }
    /** <summary>Parse the value part of a YAML line (after ": ").</summary> */
    /** <summary>Walk up from a YamlBlock UnityEngine.Transform to find the nearest controller */
    /** generator root (GestureGenerator,ExpressionsGenerator,or FxGenerator).</summary> */
    /** <summary>Sync YAML block changes back to the matching hierarchy UnityEngine.Object. */
    /** Called from YamlBlockEditor after any edit to keep the hierarchy in sync. */
    /** Works with any controller type (Gesture,Expressions,FX).</summary> */
    public void SyncCbBlockTypeFromYaml()
    {
      var ctrlRoots = GetAllControllerRoots();
      foreach (var ctrl in ctrlRoots)
      {
        var ybParent = ctrl.Find("YamlBlocks");
        if (ybParent == null) continue;
        foreach (UnityEngine.Transform ybChild in ybParent)
        {
          var yb = ybChild.GetComponent<YamlBlock>();
          if (yb == null || System.String.IsNullOrEmpty(yb.blockName)) continue;
          UnityEngine.GameObject match = FindYamlBlockMatchStatic(yb, ctrl);
          if (match == null) continue;
          var mhb = match.GetComponent<C_AviGenerator>();
          if (mhb == null) continue;
          var inferred = YamlTagToCbBlockType(yb.typeTag, yb.rawText);
          if (inferred != CbBlockType.Custom && inferred != mhb.cbBlockType)
          { mhb.cbBlockType = inferred; IF_UE.SetDirty(mhb); }
        }
      }
    }
    void _Dummy() { }
    public void GenerateLayersFromYaml(C_AviGenerator rootHB)
    {
      if (rootHB == null) rootHB = this;
      var ybParent = rootHB.transform.Find("YamlBlocks");
      if (ybParent == null) { UnityEngine.Debug.LogWarning("[HB] GenerateLayersFromYaml: No YamlBlocks found."); return; }
      /* Get or create the layers container */
      var layersContainer = rootHB.transform.Find("layers");
      if (layersContainer == null)
      {
        layersContainer = new UnityEngine.GameObject("layers").transform;
        layersContainer.SetParent(rootHB.transform, false);
      }
      /* Collect all distinct blockNames that could be layers (larger structural blocks) */
      var layerNames = new System.Collections.Generic.HashSet<System.String>();
      var blockMap = new System.Collections.Generic.Dictionary<System.String, YamlBlock>();
      foreach (UnityEngine.Transform ybChild in ybParent)
      {
        var yb = ybChild.GetComponent<YamlBlock>();
        if (yb == null || System.String.IsNullOrEmpty(yb.blockName)) continue;
        /* Skip known marker/header blocks */
        if (yb.blockName.StartsWith("YB_") || yb.blockName == "header") continue;
        /* Collect block name -> first YamlBlock mapping */
        if (!blockMap.ContainsKey(yb.blockName))
          blockMap[yb.blockName] = yb;
        /* Collect as potential layer if it looks like a top-level name */
        if (yb.typeTag == "1107" || yb.typeTag == "1101" || yb.typeTag == "1102"
          || yb.typeTag == "91" || yb.typeTag == "1113")
          layerNames.Add(yb.blockName);
      }
      /* Clear existing animatorLayers list */
      rootHB.animatorLayers.Clear();
      /* Create layer GOs inside layers/ container for each blockName */
      int created = 0;
      foreach (var name in layerNames)
      { // Skip Entry/Exit/Up/AnyState — those are markers,not layers
        if (name == "Entry" || name == "Exit" || name == "Up" || name == "AnyState") continue;
        /* Check if already exists inside layers/ container */
        System.Boolean exists = layersContainer.Find(name) != null;
        if (exists) continue;
        var layerGO = new UnityEngine.GameObject(name);
        layerGO.transform.SetParent(layersContainer, false);
        var lhb = layerGO.GetComponent<C_AviGenerator>() ?? layerGO.AddComponent<C_AviGenerator>();
        lhb.mode = E_AviGeneratorMode.AnimatorBuilder;
        lhb.animatorSubMode = rootHB.animatorSubMode;
        /* Add to the animatorLayers list */
        rootHB.animatorLayers.Add(layerGO);
        IF_UE.RegCr(layerGO, "Create Layer " + name);
        created++;
      }
      /* Now create child states for each layer based on more specific YAML blocks */
      foreach (var layerGO in rootHB.animatorLayers)
      {
        if (layerGO == null) continue;
        System.String layerName = layerGO.name;
        /* Find YAML blocks that are children of this layer's context */
        foreach (var kvp in blockMap)
        {
          if (kvp.Key == layerName) continue; // skip self
          if (kvp.Key == "Entry" || kvp.Key == "Exit" || kvp.Key == "Up" || kvp.Key == "AnyState") continue;
          /* Check if this block could be a state within this layer */
          System.Boolean childExists = layerGO.transform.Find(kvp.Key) != null;
          if (childExists) continue;
          /* Create as state object */
          var stateGO = new UnityEngine.GameObject(kvp.Key);
          stateGO.transform.SetParent(layerGO.transform, false);
          var shb = stateGO.GetComponent<C_AviGenerator>() ?? stateGO.AddComponent<C_AviGenerator>();
          shb.mode = E_AviGeneratorMode.State;
          IF_UE.RegCr(stateGO, "Create State " + kvp.Key);
        }
        /* Ensure Entry/Exit/Up/AnyState markers exist */
        foreach (var marker in new[] { "Entry", "AnyState", "Exit", "Up" })
        {
          if (layerGO.transform.Find(marker) != null) continue;
          var mgo = new UnityEngine.GameObject(marker);
          mgo.transform.SetParent(layerGO.transform, false);
          var mhb = mgo.AddComponent<C_AviGenerator>();
          mhb.mode = E_AviGeneratorMode.ControllerBuilder;
          mhb.cbBlockType = marker == "Entry" ? CbBlockType.Entry : marker == "AnyState" ? CbBlockType.Any : marker == "Exit" ? CbBlockType.Exit : CbBlockType.Up;
          if (marker != "Exit" && marker != "Up") mgo.AddComponent<TransitionSet>();
        }
      }
      IF_UE.SetDirty(rootHB);
      UnityEngine.Debug.Log("[HB] GenerateLayersFromYaml: created " + created + " layers in 'layers/' container from " + layerNames.Count + " YAML block names.");
    }
    CbBlockType YamlTagToCbBlockType(System.String typeTag, System.String rawText)
    {
      switch (typeTag)
      {
        case "91": case "1113": return CbBlockType.AController;   // UnityEditor.Animations.AnimatorController / UnityEngine.AnimatorOverrideController
        case "1107":                        // UnityEditor.Animations.AnimatorStateMachine
          if (rawText.Contains("m_EntryTransitions:") || rawText.Contains("m_AnyStateTransitions:"))
            return CbBlockType.StateMachine;
          return CbBlockType.Layer;
        case "1102": return CbBlockType.SubState;           // UnityEditor.Animations.AnimatorState
        case "1101":                        // UnityEditor.Animations.AnimatorTransition (entry/any → state)
          if (rawText.Contains("m_DstStateMachine:")) return CbBlockType.StateMachine;
          if (rawText.Contains("m_Conditions:"))
          { if (rawText.Contains("GestureLeft") || rawText.Contains("GestureRight")) return CbBlockType.Any; }
          return CbBlockType.Entry;
        case "1114": return CbBlockType.Exit;             // UnityEditor.Animations.AnimatorTransition (exit marker)
        default: return CbBlockType.Custom;
      }
    }
    static void CollectACParams(UnityEngine.Transform t, C_AviGenerator targetHB)
    {
      var tss = t.GetComponents<TransitionSet>();
      foreach (var ts in tss)
      {
        foreach (var c in ts.conditions)
        {
          if (!System.Linq.Enumerable.Any(targetHB.acParams, p => p.name == c.parameter))
            targetHB.acParams.Add(new ACParam { name = c.parameter, type = 1 });
        }
        foreach (var c in ts.entryConditions)
        {
          if (!System.Linq.Enumerable.Any(targetHB.acParams, p => p.name == c.parameter))
            targetHB.acParams.Add(new ACParam { name = c.parameter, type = 1 });
        }
      }
      foreach (var c in targetHB.subStateConditions)
      {
        if (!System.Linq.Enumerable.Any(targetHB.acParams, p => p.name == c.parameter))
          targetHB.acParams.Add(new ACParam { name = c.parameter, type = 1 });
      }
    }
    static System.String ParseYamlValue(System.String line)
    { var c = line.IndexOf(':'); return c >= 0 ? line.Substring(c + 1).Trim() : ""; }
    static UnityEngine.Transform FindNearestControllerRoot(YamlBlock yb)
    {
      var controllerModes = new System.Collections.Generic.HashSet<E_AviGeneratorMode> {
    E_AviGeneratorMode.GestureGenerator,E_AviGeneratorMode.AnimatorBuilder,E_AviGeneratorMode.ControllerBuilder,E_AviGeneratorMode.ExpressionsGenerator,E_AviGeneratorMode.FxGenerator };
      var t = yb.transform;
      while (t != null)
      {
        var phb = t.GetComponent<C_AviGenerator>();
        if (phb != null && controllerModes.Contains(phb.mode))
          return t;
        t = t.parent;
      }
      return null;
    }
    public static UnityEngine.GameObject FindYamlBlockMatchStatic(YamlBlock yb, UnityEngine.Transform ctrlRoot)
    {
      if (System.String.IsNullOrEmpty(yb.blockName)) return null;
      UnityEngine.Transform layersContainer = ctrlRoot.Find("layers");
      if (layersContainer == null) layersContainer = ctrlRoot;
      foreach (UnityEngine.Transform layer in layersContainer)
      {
        if (layer.name == "YamlBlocks" || layer.name == "layers") continue;
        if (yb.blockName == layer.name) return layer.gameObject;
        foreach (UnityEngine.Transform sub in layer)
        {
          if (sub.name == "Entry" || sub.name == "Exit" || sub.name == "Up" || sub.name == "AnyState") continue;
          if (yb.blockName == sub.name) return sub.gameObject;
          foreach (UnityEngine.Transform state in sub)
          {
            if (state.name == "Entry" || state.name == "Exit" || state.name == "Up" || state.name == "AnyState") continue;
            if (yb.blockName == state.name) return state.gameObject;
          }
        }
      }
      return null;
    }
  }
}
}
