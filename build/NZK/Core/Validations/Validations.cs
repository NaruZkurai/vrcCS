namespace NZK
{
public static partial class Core {
public static partial class Validations
{
/*
 * EDITOR-ONLY: every member here reads UnityEditor.Selection or
 * UnityEditor.AssetDatabase, which do not exist in a player build.  The whole
 * body is guarded rather than chosen per member, because there is no runtime
 * use for "what is currently selected" in a shipped avatar.
 *
 * Unguarded, this is what the VRC avatar upload reported:
 *
 *   error CS0234: The type or namespace name 'Selection' does not exist in the
 *   namespace 'UnityEditor' (are you missing an assembly reference?)
 *   Error building Player because scripts had compiler errors
 *   AssetBundle was not built
 *
 * Validated (below) stays OUTSIDE the guard: it is a plain string holder that
 * runtime code reads, so the type must exist in both builds.
 */
#if UNITY_EDITOR
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
#endif
}
}
}
