#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class NzkFbxExport
  {
  const System.String LogTag = "[NzkFBX]";
  public static void ExportArmatures(UnityEngine.GameObject avatarRoot,System.String avatarName,UnityEngine.Transform armatureArm,UnityEngine.Transform armatureStrip,UnityEngine.Avatar refAvatar,UnityEngine.Avatar fullAvatar)
  { System.String exportDir = Vars.Consts.RootFolder + "/" + avatarName + "/Armatures/";
    System.IO.Directory.CreateDirectory(exportDir);
    if (armatureArm != null)
    ExportOne(armatureArm,exportDir + avatarName + "_Armature_Armature",fullAvatar ?? refAvatar);
    if (armatureStrip != null)
    ExportOne(armatureStrip,exportDir + avatarName + "_Armature_Stripped",refAvatar);
    IF_UE.SaveAndRefresh(); UnityEditor.AssetDatabase.Refresh();
    UnityEngine.Debug.Log(LogTag + " Exported armatures to: " + exportDir); }
  static void ExportOne(UnityEngine.Transform root,System.String basePath,UnityEngine.Avatar avatar)
  { System.String prefabPath = basePath + ".prefab";
    var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(root.gameObject,prefabPath);
    if (prefab != null)
    { System.String avPath = basePath + "_Avatar.asset";
    if (avatar != null && !UnityEditor.AssetDatabase.Contains(avatar))
    { var copy = UnityEngine.Object.Instantiate(avatar); copy.name = avatar.name;
      IF_UE.CreateAsset(copy,avPath); }
    UnityEngine.Debug.Log(LogTag + " Prefab: " + prefabPath); }
    System.String fbxPath = basePath + ".fbx";
    try
    { var meType = System.Type.GetType(
      "UnityEditor.Formats.Fbx.Exporter.ModelExporter,Unity.Formats.Fbx.UnityEditor.Editor");
    if (meType != null)
    { var method = meType.GetMethod("ExportObject",new[] { typeof(System.String),typeof(UnityEngine.GameObject) });
      if (method != null)
      { if (System.IO.File.Exists(fbxPath)) System.IO.File.Delete(fbxPath);
      method.Invoke(null,new object[] { fbxPath,root.gameObject });
      if (System.IO.File.Exists(fbxPath))
      { UnityEditor.AssetDatabase.ImportAsset(fbxPath);
        UnityEngine.Debug.Log(LogTag + " FBX: " + fbxPath); return; }
      }
    }
    UnityEngine.Debug.Log(LogTag + " FBX exporter unavailable — prefab only: " + prefabPath); }
    catch (System.Exception e)
    { UnityEngine.Debug.Log(LogTag + " FBX skipped: " + e.Message); }
  }
  }
}
}
#endif
