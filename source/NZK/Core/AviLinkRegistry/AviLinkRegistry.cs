namespace NZK
{
public static partial class Core {
public static class AviLinkRegistry
  {
    static System.Collections.Generic.Dictionary<int, AviLink> _s = new();
    public static AviLink Get(UnityEngine.GameObject g) { AviLink v; _s.TryGetValue(g.GetInstanceID(), out v); return v; }
    public static AviLink GetOrCreate(UnityEngine.GameObject g) { var id = g.GetInstanceID(); AviLink v; if (!_s.TryGetValue(id, out v)) { v = new AviLink(); _s[id] = v; } return v; }
    public static void Remove(UnityEngine.GameObject g) { _s.Remove(g.GetInstanceID()); }
  }
}
}
