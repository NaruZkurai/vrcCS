#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshesMerge {
public struct MergeOutcome
    {
      public System.Boolean Success;
      /** The rr-code that describes the failure, or -1 when there was none.
       *  Carried alongside the text so a caller can re-report the SAME code
       *  through E.C / E.D instead of inventing a second spelling of it. */
      public System.Int64 ErrorCode;
      public System.String ErrorMessage;
      /** The merged objects, in the order they were produced. */
      public System.Collections.Generic.List<UnityEngine.GameObject> Merged;
      /** The sources that were parked, in the same order. */
      public System.Collections.Generic.List<UnityEngine.GameObject> Sources;
      /** How many sources had no mesh and were skipped. */
      public System.Int32 Skipped;
    }
}
}
}
#endif
