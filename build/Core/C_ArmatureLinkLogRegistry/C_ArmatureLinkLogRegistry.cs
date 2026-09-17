namespace NZK
{
public static partial class Core {
public static class C_ArmatureLinkLogRegistry
  {
    static System.Collections.Generic.Dictionary<int, C_ArmatureLinkLog> _s = new();
    public static C_ArmatureLinkLog Get(UnityEngine.GameObject g) { _s.TryGetValue(g.GetInstanceID(), out var v); return v; }
    public static C_ArmatureLinkLog GetOrCreate(UnityEngine.GameObject g) { var id = g.GetInstanceID(); if (!_s.TryGetValue(id, out var v)) { v = new C_ArmatureLinkLog(); _s[id] = v; } return v; }
    public static void Remove(UnityEngine.GameObject g) { _s.Remove(g.GetInstanceID()); }
  }
}
}
