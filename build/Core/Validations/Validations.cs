namespace NZK
{
public static partial class Core {
public static partial class Validations
{
  public static bool HasSelectedAnimations()
  { string[] guids = UnityEditor.Selection.assetGUIDs;
    if (guids == null || guids.Length == 0) return false;
    foreach (string guid in guids)
    {
      string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
      UnityEngine.AnimationClip clip =
        UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
      if (clip != null) return true;
    }
    return false;
  }
  public static bool HasSelectedZeroScaleAnimations()
  { string[] guids = UnityEditor.Selection.assetGUIDs;
    if (guids == null || guids.Length == 0) return false;
    foreach (string guid in guids)
    { string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
      UnityEngine.AnimationClip clip =
        UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
      if (clip != null && ClipHasZeroScale(clip)) return true;
    }
    return false;
  }
  public static bool ClipHasZeroScale(UnityEngine.AnimationClip clip)
  { if (clip == null) return false;
    if (!HasValidClipFilePath(clip)) return false;
    return NZK.Core.Nan.ClipHasZeroScale(
      System.IO.File.ReadAllText(NZK.Core.Validated.FilePath));
  }
  public static bool HasValidClipFilePath(UnityEngine.AnimationClip clip)
  { if (clip == null) return false;
    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    if (System.String.IsNullOrEmpty(assetPath)) return false;
    NZK.Core.Validated.FilePath = NZK.S.P.C(
      NZK.Core.MenuItems.ApplicationDataPath,
      assetPath.Substring("Assets/".Length));
    return System.IO.File.Exists(NZK.Core.Validated.FilePath);
  }
}
}
}
