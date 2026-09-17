namespace NZK
{
public static partial class Core {
public partial class C_AviGenerator : UnityEngine.MonoBehaviour
  {
    public System.Collections.Generic.List<UnityEngine.Transform> GetAllControllerRoots()
    {
      var roots = new System.Collections.Generic.List<UnityEngine.Transform>();
      var modes = new System.Collections.Generic.HashSet<E_AviGeneratorMode> {
    E_AviGeneratorMode.GestureGenerator,E_AviGeneratorMode.AnimatorBuilder,E_AviGeneratorMode.ControllerBuilder,E_AviGeneratorMode.ExpressionsGenerator,E_AviGeneratorMode.FxGenerator };
      if (modes.Contains(mode)) roots.Add(transform);
      foreach (UnityEngine.Transform c in transform)
      {
        var hb = c.GetComponent<C_AviGenerator>();
        if (hb != null && modes.Contains(hb.mode)) roots.Add(c);
      }
      return roots;
    }
    /** become a single flag instead of N duplicate values.</summary> */
    void DetectAndPromoteDuplicates(System.Collections.Generic.List<UnityEngine.Transform> ctrlRoots)
    {
      /* Collect all (propertyName,value) pairs across all blocks */
      var propValues = new System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.HashSet<System.String>>();
      var propBlocks = new System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.List<(YamlBlock yb, System.String line, int lineIdx)>>();
      foreach (var root in ctrlRoots)
      {
        var flags = root.GetComponent<C_HBFlags>();
        if (flags == null) continue;
        var ybParent = root.Find("YamlBlocks");
        if (ybParent == null) continue;
        System.String layerName = C_HBFlagUtils.S_GetLayerName(root.GetComponent<C_AviGenerator>());
        foreach (UnityEngine.Transform ybChild in ybParent)
        {
          var yb = ybChild.GetComponent<YamlBlock>();
          if (yb == null) continue;
          var lines = yb.rawText.Split('§');
          for (int i = 0; i < lines.Length; i++)
          {
            var t = lines[i].Trim();
            if (!t.StartsWith("m_") || t.Contains("{") || t.Contains("[") || t.Contains("&")) continue;
            var colon = t.IndexOf(':');
            if (colon < 0) continue;
            var fn = t.Substring(0, colon).Trim();
            var val = t.Substring(colon + 1).Trim();
            if (System.String.IsNullOrEmpty(val) || val.Length > 60) continue; // skip long/complex values
            if (!propValues.ContainsKey(fn)) propValues[fn] = new System.Collections.Generic.HashSet<System.String>();
            propValues[fn].Add(val);
            if (!propBlocks.ContainsKey(fn)) propBlocks[fn] = new System.Collections.Generic.List<(YamlBlock, System.String, int)>();
            propBlocks[fn].Add((yb, t, i));
          }
        }
      }
      /* Properties with exactly 1 unique value across all blocks → promote to Global flag */
      int promoted = 0;
      foreach (var kvp in propValues)
      {
        if (kvp.Value.Count != 1) continue; // not a duplicate (unique or varied)
        if (!propBlocks.TryGetValue(kvp.Key, out var blocks)) continue;
        if (blocks.Count < 2) continue; // need at least 2 blocks to be "duplicate"
        System.String uniformValue = System.Linq.Enumerable.First(kvp.Value);
        /* Promote to flag on each controller root that has this property */
        foreach (var root in ctrlRoots)
        {
          var flags = root.GetComponent<C_HBFlags>();
          if (flags == null) continue;
          System.String layerName = C_HBFlagUtils.S_GetLayerName(root.GetComponent<C_AviGenerator>());
          /* Check if this root actually has blocks with this property */
          System.Boolean rootHas = false;
          foreach (var (yb, _, _) in blocks)
          {
            var t = yb.transform;
            while (t != null) { if (t == root) { rootHas = true; break; } t = t.parent; }
            if (rootHas) break;
          }
          if (!rootHas) continue;
          /* Set flag (only if not already set) */
          if (flags.S_Resolve(kvp.Key, layerName) == null)
          {
            flags.V_SetFlag(kvp.Key, uniformValue, E_HBFlagScope.Global, "");
            promoted++;
          }
        }
        /* Remove the now-flagged value from all YAML block texts */
        foreach (var (yb, oldLine, li) in blocks)
        {
          var lines = yb.rawText.Split('§');
          if (li < lines.Length && lines[li].Trim() == oldLine.Trim())
          {
            lines[li] = "  " + kvp.Key + ": " + uniformValue + " # flagged";
            yb.rawText = System.String.Join("§", lines);
            IF_UE.SetDirty(yb);
          }
        }
      }
      if (promoted > 0) UnityEngine.Debug.Log("[HB] Promoted " + promoted + " duplicate values to HBFlags.");
    }
  }
}
}
