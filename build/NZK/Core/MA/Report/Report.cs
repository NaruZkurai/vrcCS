#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MA {
public struct Report
    {
      /** Meshes examined. */
      public System.Int32 MeshesInspected;
      /** Meshes actually rebuilt (some vertices split and/or bones added). */
      public System.Int32 MeshesRebuilt;
      /** Vertices duplicated to give primitives independent visibility. */
      public System.Int32 VerticesSplit;
      /** Bones created across all meshes. */
      public System.Int32 BonesCreated;
      /** Buffer objects created (one per distinct parent bone). */
      public System.Int32 BuffersCreated;
      /** Meshes skipped because they had no group assignment. */
      public System.Int32 MeshesSkipped;
      /** Non-null when the pass could not proceed at all. */
      public System.String error;
    }
}
}
}
#endif
