#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Menu
  { public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu Get(System.String assetPath) => UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(assetPath);
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu Create(System.String assetPath,System.String menuName) { var m = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(); m.name = menuName; m.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>(); UnityEditor.AssetDatabase.CreateAsset(m,assetPath); return m; }
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu GllC(System.String assetPath) => NaNimate.Menu.Get(assetPath) ?? NaNimate.Menu.Create(assetPath,System.IO.Path.GetFileNameWithoutExtension(assetPath)); }
}
}
}
#endif
