#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class NZKNaNimateBoneHolder {
public class PostImportHook : UnityEditor.AssetPostprocessor
    { public static void OnPostprocessAllAssets(
        System.String[] imported, System.String[] deleted,
        System.String[] moved, System.String[] movedFrom)
      { /* Our own writes.  Re-queueing here IS the infinite loop. */
        if (NZKNaNimateBoneHolder._runDepth > 0) return;
        if (NZKNaNimateBoneHolder._suppressImportRuns) return;
        /* A model may have been added or renamed, so the name index is stale.
         * Cleared HERE rather than inside the run because the run itself writes
         * assets, and invalidating during that would rebuild the index on every
         * one of its own writes. */
        NZKNaNimateBoneHolder._modelByMeshName = null;
        if (!AnyHolderWantsAutoRun()) return;
        UnityEditor.EditorApplication.delayCall += RunAllDeferred; } }
}
}
}
#endif
