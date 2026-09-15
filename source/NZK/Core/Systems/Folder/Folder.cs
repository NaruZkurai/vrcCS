#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class Folder
  { public static void Ensure(System.String folderPath)
  { folderPath = folderPath.Replace('\\','/').TrimEnd('/'); if (UnityEditor.AssetDatabase.IsValidFolder(folderPath)) return;
    System.String abs = NaNimate.Paths.Abs(folderPath);
    if (!System.IO.Directory.Exists(abs)) System.IO.Directory.CreateDirectory(abs);
    if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath)) UnityEditor.AssetDatabase.ImportAsset(folderPath,UnityEditor.ImportAssetOptions.ForceSynchronousImport); }
  public static void Toggle(System.String a,System.String c) { Systems.Folder.Ensure(Vars.Names.Get.AnimFolderCategory(a,c)); Systems.Folder.Ensure(Vars.Names.Get.BTFolderCategory(a,c)); } }
}
}
}
#endif
