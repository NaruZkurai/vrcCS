namespace NZK
{
public static partial class Core {
public static partial class AXController
  {/* ---- VRChat animation layer indices ------------------------------ */
    public const int VRCBase = 0;
    public const int VRCAdd = 1;
    public const int VRCGest = 2;
    public const int VRCAct = 3;
    public const int VRCFX = 4;
    public const int VRCSit = 0;
    public const int VRCTPose = 1;
    public const int VRCIKP = 2;
    /* ---- default gesture reference controller path ------------------------- */
    public const System.String DefaultGestureRefPath = "Assets/NZK toolkit v4/Dependencies/default nzk Gesture controller.controller";
    /* ---- Helper: st8 type checking ---- */
    public static System.Boolean HasSt8(C_AviGenerator hb, CbBlockType flag)
    {
      if (hb == null) return false;
      return hb.cbBlockType == flag;
    }
    /** <summary>Check if the gesture generator has any non-YamlBlocks children. */
    /** Used to decide whether ReloadGestureChildren needs to run.</summary> */
    public static System.Boolean HasGestureChildren(C_AviGenerator hb)
    { // Check inside layers/ container first
      var layersContainer = hb.transform.Find("layers");
      if (layersContainer != null)
      {
        foreach (UnityEngine.Transform c in layersContainer) return true; // has layers
      }
      /* Fallback: direct children */
      foreach (UnityEngine.Transform c in hb.transform)
        if (c.name != "YamlBlocks" && c.name != "layers") return true;
      return false;
    }
    /* ---- Helper utilities ---- */
    /** <summary>Get layer transforms from a generator root,searching either the */
    /** `layers/` container or direct children for backward compatibility.</summary> */
    static System.Collections.Generic.IEnumerable<UnityEngine.Transform> GetGestureLayers(UnityEngine.Transform gestRoot)
    {
      var layersContainer = gestRoot.Find("layers");
      if (layersContainer != null)
      { foreach (UnityEngine.Transform c in layersContainer) yield return c; }
      else
      {
        foreach (UnityEngine.Transform c in gestRoot)
          if (c.name != "YamlBlocks") yield return c;
      }
    }
    public static System.String ResolveLayerName(System.String raw)
    {
      var n = raw.Replace(HBChildren.LayerPrefix, "").Replace("HB_", "").Trim();
      if (n.Contains("Idle")) n = n.Replace("Idle", "").Trim() + " Additive";
      return n;
    }
  }
}
}
