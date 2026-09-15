namespace NZK
{
public static partial class Core {
public static class C_HBFlagUtils
  {/** <summary>Find the nearest HBFlags component,walking up to the gesture generator root.</summary> */
    public static C_HBFlags FindFlags(C_AviGenerator hb)
    {
      if (hb == null) return null;
      var flags = hb.GetComponent<C_HBFlags>();
      if (flags != null) return flags;
      /* Walk up to find gesture generator */
      var t = hb.transform;
      while (t != null)
      {
        var phb = t.GetComponent<C_AviGenerator>();
        if (phb != null && (phb.mode == E_AviGeneratorMode.GestureGenerator || phb.mode == E_AviGeneratorMode.AnimatorBuilder))
        {
          flags = phb.GetComponent<C_HBFlags>();
          if (flags != null) return flags;
        }
        t = t.parent;
      }
      return null;
    }
    /** <summary>Get layer name from a C_AviGenerator UnityEngine.GameObject (strips prefix).</summary> */
    public static System.String S_GetLayerName(C_AviGenerator hb)
    {
      if (hb == null) return "";
      var n = hb.gameObject.name;
      if (n.StartsWith("LYR_")) return n.Substring(4);
      if (n.StartsWith("HB_")) return n.Substring(3);
      return n;
    }
  }
}
}
