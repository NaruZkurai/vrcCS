#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
namespace NZK
{
public static partial class Core {
  /** <summary>Upload-time trigger for the nanimation weight repair.
   *
   *  The repair runs in PRE-process (avatar alive, meshes writable), and the
   *  post-process callback re-runs it as a safety net for anything that rebuilt
   *  a mesh after us.  Both phases are required: see the header.</summary> */
  
}
}
#endif
