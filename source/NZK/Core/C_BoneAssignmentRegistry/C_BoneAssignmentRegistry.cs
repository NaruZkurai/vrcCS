namespace NZK
{
public static partial class Core {
public static class C_BoneAssignmentRegistry
  {
    static System.Collections.Generic.Dictionary<int, C_BoneAssignment> _s = new();
    public static C_BoneAssignment Get(UnityEngine.GameObject g) { _s.TryGetValue(g.GetInstanceID(), out var v); return v; }
    public static C_BoneAssignment GetOrCreate(UnityEngine.GameObject g) { var id = g.GetInstanceID(); if (!_s.TryGetValue(id, out var v)) { v = new C_BoneAssignment(); _s[id] = v; } return v; }
    public static void Remove(UnityEngine.GameObject g) { _s.Remove(g.GetInstanceID()); }
    public static int DestroyAllInChildren(UnityEngine.Transform root) { int c = 0; foreach (var t in root.GetComponentsInChildren<UnityEngine.Transform>(true)) if (_s.Remove(t.gameObject.GetInstanceID())) c++; return c; }
  }
}
}
