#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshAudit {
public struct AuditReport
    {
      public System.Int32 Renderers;
      public System.Int32 Enabled;
      public System.Int32 Readable;
      public System.Int32 EmptyMeshes;
      public System.Int32 ZeroBounds;
      public System.Int32 NaNBounds;
      public System.Int32 Unweighted;
      public System.Int32 OverInfluenced;
      public System.Int32 NaNVertices;
      public System.Int32 BadScales;
      public System.Int32 MissingMaterials;
      /** Renderers whose bones array holds at least one NULL entry. */
      public System.Int32 NullBones;
      /** Renderers with bones from a different hierarchy root. */
      public System.Int32 OffRigBones;
      /** Renderers where bindposes.Length != bones.Length. */
      public System.Int32 BindPoseMismatch;
    }
}
}
}
#endif
