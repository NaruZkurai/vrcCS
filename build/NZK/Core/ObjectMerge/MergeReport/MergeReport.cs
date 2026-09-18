#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class ObjectMerge {
public struct MergeReport
    {
      public System.Boolean Success;
      /** Which rr-code describes the outcome. */
      public System.Int32 Code;
      public System.String Message;
      /** How many serialized references were re-pointed. */
      public System.Int32 References;
      /** How many components were moved onto the target. */
      public System.Int32 Components;
      /** How many children were reparented onto the target. */
      public System.Int32 Children;
      /** How many source objects were destroyed after being emptied. */
      public System.Int32 Removed;
    }
}
}
}
#endif
