namespace NZK
{
public static partial class Core {
public static class AviGeneratorRegistry
  {
    static System.Collections.Generic.Dictionary<int, C_AviGenerator> _s = new();
    public static C_AviGenerator Get(UnityEngine.GameObject g) { C_AviGenerator v; _s.TryGetValue(g.GetInstanceID(), out v); return v; }
    public static C_AviGenerator Get(UnityEngine.Transform t) { C_AviGenerator v; _s.TryGetValue(t.gameObject.GetInstanceID(), out v); return v; }
    public static C_AviGenerator GetOrCreate(UnityEngine.GameObject g) { var id = g.GetInstanceID(); C_AviGenerator v; if (!_s.TryGetValue(id, out v)) { v = new C_AviGenerator(); _s[id] = v; } return v; }
    public static System.Collections.Generic.List<C_AviGenerator> GetAllInChildren(UnityEngine.Transform root) { var r = new System.Collections.Generic.List<C_AviGenerator>(); foreach (var t in root.GetComponentsInChildren<UnityEngine.Transform>(true)) { C_AviGenerator v; if (_s.TryGetValue(t.gameObject.GetInstanceID(), out v)) r.Add(v); } return r; }
    public static void Remove(UnityEngine.GameObject g) { _s.Remove(g.GetInstanceID()); }
  }
}
}
