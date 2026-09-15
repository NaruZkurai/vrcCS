#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class IF_UE
  {/* ── UnityEditor.Undo wrappers ────────────────────────────────────────────── */
  public static void RegCr(UnityEngine.GameObject go,System.String n) => UnityEditor.Undo.RegisterCreatedObjectUndo(go,n);
  public static void RecObj(UnityEngine.Object o,System.String n) => UnityEditor.Undo.RecordObject(o,n);
  public static void SetDirty(UnityEngine.Object o) => UnityEditor.EditorUtility.SetDirty(o);
  /* ── Asset helpers ────────────────────────────────────────────── */
  /** <summary>Load an asset at path,or null if missing/invalid.</summary> */
  public static T Load<T>(System.String path) where T : UnityEngine.Object =>
    System.String.IsNullOrEmpty(path) ? null : UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
  /** <summary>Create an asset,set it dirty,and return it.</summary> */
  public static T CreateAsset<T>(T obj,System.String path) where T : UnityEngine.Object {
    UnityEditor.AssetDatabase.CreateAsset(obj,path);
    IF_UE.SetDirty(obj);
    return obj; }
  /** <summary>Create or overwrite an asset via UnityEditor.EditorUtility.CopySerialized.</summary> */
  public static T SaveOverwrite<T>(T src,System.String path) where T : UnityEngine.Object {
    var existing = Load<T>(path);
    if (existing != null) { UnityEditor.EditorUtility.CopySerialized(src,existing); IF_UE.SetDirty(existing); return existing; }
    return CreateAsset(src,path); }
  /** <summary>Save all pending asset changes and refresh.</summary> */
  public static void SaveAndRefresh() { UnityEditor.AssetDatabase.SaveAssets(); UnityEditor.AssetDatabase.Refresh(); }
  /** <summary>Delete an asset at path. Safe to call on missing paths.</summary> */
  public static System.Boolean DeleteAsset(System.String path) {
    if (System.String.IsNullOrEmpty(path)) return false;
    return UnityEditor.AssetDatabase.DeleteAsset(path); }
  /** <summary>Copy an asset from src to dst path.</summary> */
  public static System.Boolean CopyAsset(System.String src,System.String dst) {
    if (System.String.IsNullOrEmpty(src) || System.String.IsNullOrEmpty(dst)) return false;
    return UnityEditor.AssetDatabase.CopyAsset(src,dst); }
  /** <summary>Move/rename an asset.</summary> */
  public static System.String MoveAsset(System.String src,System.String dst) {
    if (System.String.IsNullOrEmpty(src) || System.String.IsNullOrEmpty(dst)) return "";
    return UnityEditor.AssetDatabase.MoveAsset(src,dst); }
  /** <summary>Delete an asset if it exists at path.</summary> */
  public static System.Boolean TryDeleteAsset(System.String path) {
    if (System.String.IsNullOrEmpty(path)) return false;
    var exists = Load<UnityEngine.Object>(path);
    if (exists != null) { UnityEditor.AssetDatabase.DeleteAsset(path); return true; }
    return false; }
  /** <summary>Destroy a UnityEngine.Object immediately (editor-only).</summary> */
  public static void DestroyImmediate(UnityEngine.Object o) {
    if (o != null) UnityEngine.Object.DestroyImmediate(o); }
  /** <summary>Destroy a UnityEngine.GameObject immediately (editor-only).</summary> */
  public static void DestroyImmediateGameObject(UnityEngine.GameObject go) {
    if (go != null) UnityEngine.Object.DestroyImmediate(go); }
  /** <summary>Record an UnityEditor.Undo DestroyObject operation.</summary> */
  public static void UndoDestroy(UnityEngine.Object o,System.String name = "") {
    if (o != null) UnityEditor.Undo.DestroyObjectImmediate(o); }
  /* ── System.IO.Directory helpers ────────────────────────────────────────── */
  /** <summary>Ensure a directory exists for a given file path.</summary> */
  public static void EnsureDir(System.String filePath) {
    var dir = System.IO.Path.GetDirectoryName(filePath);
    if (!System.String.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir); }
  }
}
}
#endif
