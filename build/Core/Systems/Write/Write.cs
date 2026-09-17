#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class Write
  { public static void TodoFile(System.String toggleType,UnityEngine.GameObject[] selectedObjects)
  { int ci = System.String.Equals(toggleType,"On Off",System.StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    var groups = new System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<UnityEngine.GameObject>>(System.StringComparer.Ordinal);
    foreach (var sel in selectedObjects) { if (sel == null) continue; System.String a = Vars.Names.Get.Avatar(sel); if (!groups.TryGetValue(a,out var l)) { l = new System.Collections.Generic.List<UnityEngine.GameObject>(); groups[a] = l; } l.Add(sel); }
    foreach (var kvp in groups) Systems.Write.AvatarTodo(kvp.Key,kvp.Value,ci); }
  public static void AvatarTodo(System.String avatarName,System.Collections.Generic.List<UnityEngine.GameObject> objects,int categoryIndex)
  { Systems.Folder.Ensure(Vars.Names.Get.AviRoot(avatarName)); System.String tp = Vars.Names.Get.AviRoot(avatarName) + "/ToggleGenerationTodo.txt";
    var lines = new System.Collections.Generic.List<System.String> { "Catagory:[0,nan toggle]" };
    if (categoryIndex == 0) Systems.Write.AppendPaths(lines,objects); lines.Add("catagory:[1,onoff toggle]"); if (categoryIndex == 1) Systems.Write.AppendPaths(lines,objects); lines.Add("--done--");
    System.IO.File.WriteAllLines(NaNimate.Paths.Abs(tp),lines,System.Text.Encoding.UTF8); UnityEditor.AssetDatabase.ImportAsset(tp,UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate); }
  public static void AppendPaths(System.Collections.Generic.List<System.String> lines,System.Collections.Generic.List<UnityEngine.GameObject> objects)
  { int i = 0; foreach (var sel in objects) { lines.Add(i.ToString("D1") + ":" + UnityEditor.AnimationUtility.CalculateTransformPath(sel.transform,sel.transform.root)); i++; } } }
}
}
}
#endif
