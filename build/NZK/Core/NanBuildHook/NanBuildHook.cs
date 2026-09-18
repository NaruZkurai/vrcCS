#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public class NanBuildHook : VRC.SDKBase.Editor.BuildPipeline.IVRCSDKPreprocessAvatarCallback,
                              VRC.SDKBase.Editor.BuildPipeline.IVRCSDKPostprocessAvatarCallback
  {
    /** <summary>Lowest order, so the reference is captured before any other
     *  pre-process callback can rebuild the hierarchy out from under us.</summary> */
    public System.Int32 callbackOrder => System.Int32.MinValue;
    /** <summary>The clone VRChat is uploading, carried from pre to post.
     *
     *  Static because the post-process callback is handed nothing, so there is
     *  no instance to read it from.  Null-checked and cleared on every use.</summary> */
    static UnityEngine.GameObject _pending;
    /** <summary>Capture the avatar and repair it.
     *
     *  The repair runs HERE rather than in post-process: this is the phase where
     *  the object is known-alive and its meshes are known-writable, and it is
     *  the phase the menu-driven path already proved works.  Post-process is a
     *  safety net, not the primary trigger.</summary> */
    public System.Boolean OnPreprocessAvatar(UnityEngine.GameObject avatarGameObject)
    { _pending = avatarGameObject;
      Repair(avatarGameObject);
      return true; }
    /** <summary>Safety net: re-run the repair in case a later package rebuilt a
     *  mesh after the pre-process pass.  The repair is idempotent, so a second
     *  run over already-correct weights is a no-op.</summary> */
    public void OnPostprocessAvatar()
    { var target = _pending;
      _pending = null;                       /* clear FIRST: never re-enter on a stale clone */
      Repair(target); }
    /** <summary>Run the repair, swallowing and logging every failure.
     *
     *  One helper for both phases so the error handling cannot differ between
     *  them.  Nothing may escape: a throw here would abort the upload from
     *  pre-process and be silently discarded from post-process.</summary> */
    static void Repair(UnityEngine.GameObject target)
    { if (target == null) return;
      try
      { NZK.Core.Nan.PostProcess(target); }
      catch (System.Exception e)
      { UnityEngine.Debug.LogError("[NanBuildHook] nanimation repair failed: " + e); } }
  }
}
}
#endif
