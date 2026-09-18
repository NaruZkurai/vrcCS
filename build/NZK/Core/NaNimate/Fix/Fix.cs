#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Fix
  { /** <summary>Patch a generated .anim on disk so its zero scale values become NaN.
     *
     *  This is the FILE half of the nanimation fixup; the VALUE half lives in
     *  NZK.Core.Nan and is the single authority on what counts as zero.  The two
     *  used to be duplicated here, and the local copy was wrong:
     *
     *    - ReplaceVectorScaleValue matched with "[-0-9.eE]*0", which also matches
     *      any number ENDING in 0.  A scale of 10, 100 or -20 therefore had its
     *      vector replaced with NaN, hiding geometry that was never meant to be
     *      hidden.  Nan.ScaleVectorsZeroToNaN parses each axis as a float and
     *      converts only when all three are genuinely zero.
     *    - FixEditorCurveScale re-derived the same decision with 40 lines of
     *      index arithmetic, and only inside an "m_EditorCurves:" block, so the
     *      m_ScaleCurves block was left to the buggy vector path above.
     *
     *  Routing both through Nan means one definition of "zero" and one place to
     *  fix.  Returns true when the file changed on disk.</summary> */
  public static System.Boolean FixAnimationClipScale(UnityEngine.AnimationClip clip)
  { if (clip == null) return false;
    System.String assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    if (System.String.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/",System.StringComparison.Ordinal)) return false;
    System.String fullPath = NaNimate.Paths.Abs(assetPath);
    if (!System.IO.File.Exists(fullPath)) return false;
    System.String content = System.IO.File.ReadAllText(fullPath);
    /* NaN only the AnimationClip documents, and only values that are exactly
       zero.  Nan returns null when nothing changed, which is the signal to skip
       the write and the reimport. */
    System.String updated = NZK.Core.Nan.ClipScaleToNaN(content);
    if (System.String.IsNullOrEmpty(updated) || System.String.Equals(updated,content)) return false;
    System.IO.File.WriteAllText(fullPath,updated);
    UnityEditor.AssetDatabase.ImportAsset(assetPath,UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate);
    return true; }
  }
}
}
}
#endif
