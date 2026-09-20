#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshImport {
public struct ImportReport
    {
      /** How many assets were examined. */
      public System.Int32 Inspected;
      /** How many had the setting changed to four. */
      public System.Int32 Changed;
      /** How many already asked for four, so nothing was written. */
      public System.Int32 Already;
      /** How many do not expose either property, so could not be set at all. */
      public System.Int32 Unsupported;
      /** How many were skipped because the importer could not be read. */
      public System.Int32 Skipped;
    }
}
}
}
#endif
