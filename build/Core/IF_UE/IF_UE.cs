namespace NZK
{
public static partial class Core {
public static partial class IF_UE
  {
#if !UNITY_EDITOR
    /* ── Non-editor stubs (no-ops / safe fallbacks) ──────────────── */
    public static void RegCr(UnityEngine.GameObject go, System.String n) { }
    public static void RecObj(UnityEngine.Object o, System.String n) { }
    public static void SetDirty(UnityEngine.Object o) { }
    public static T Load<T>(System.String path) where T : UnityEngine.Object => null;
    public static T CreateAsset<T>(T obj, System.String path) where T : UnityEngine.Object => obj;
    public static T SaveOverwrite<T>(T src, System.String path) where T : UnityEngine.Object => src;
    public static void SaveAndRefresh() { }
    public static System.Boolean DeleteAsset(System.String path) => false;
    public static System.Boolean CopyAsset(System.String src, System.String dst) => false;
    public static System.String MoveAsset(System.String src, System.String dst) => "";
    public static System.Boolean TryDeleteAsset(System.String path) => false;
    public static void DestroyImmediate(UnityEngine.Object o) { if (o != null) UnityEngine.Object.DestroyImmediate(o); }
    public static void DestroyImmediateGameObject(UnityEngine.GameObject go) { if (go != null) UnityEngine.Object.DestroyImmediate(go); }
    public static void UndoDestroy(UnityEngine.Object o, System.String name = "") { }
    public static void EnsureDir(System.String filePath) { }
#endif
  }
}
}
