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
        if (!AnyHolderWantsAutoRun()) return;
        UnityEditor.EditorApplication.delayCall += RunAllDeferred; } }
}
}
}
#endif
