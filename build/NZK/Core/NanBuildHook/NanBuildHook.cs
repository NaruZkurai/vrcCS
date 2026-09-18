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
    /** <summary>MASTER SWITCH: turn the whole hook off, both phases at once.
     *
     *  Exists because setting the two phase switches individually is easy to
     *  half-do and impossible to verify from the log - the report line names a
     *  phase, so "post is off" reads as "the hook is off" when the pre pass is
     *  still rewriting weights.  One switch that means "do nothing at all" is
     *  what a bisect actually needs.
     *
     *  Checked by BOTH phases, and checked FIRST, so it cannot be partially
     *  honoured by a later edit to one of them.</summary> */
    public static System.Boolean Enabled
    { get { return UnityEditor.EditorPrefs.GetBool(PrefEnabled,true); }
      set { UnityEditor.EditorPrefs.SetBool(PrefEnabled,value); } }
    /** <summary>Whether the PRE-process repair runs on upload.
     *
     *  Off means the avatar uploads with whatever weights the scene already has,
     *  so an upload can be compared against one where the repair ran.  That
     *  comparison is the whole reason this switch exists: the repair is the
     *  first thing to suspect when an avatar uploads wrong but looks correct in
     *  play mode, and "suspect" is not the same as "know".</summary> */
    public static System.Boolean PreEnabled
    { get { return Enabled && UnityEditor.EditorPrefs.GetBool(PrefPre,true); }
      set { UnityEditor.EditorPrefs.SetBool(PrefPre,value); } }
    /** <summary>Whether the POST-process repair runs on upload.
     *
     *  Separate from the pre switch on purpose.  The two phases do different
     *  work at different times - pre runs while the meshes are still writable,
     *  post re-runs after every other package has had its turn - so a fault in
     *  one is invisible if both are toggled together.  Turning them off one at a
     *  time is what isolates the phase.</summary> */
    public static System.Boolean PostEnabled
    { get { return Enabled && UnityEditor.EditorPrefs.GetBool(PrefPost,true); }
      set { UnityEditor.EditorPrefs.SetBool(PrefPost,value); } }
    /** <summary>True when BOTH phases are off, so nothing this hook does can
     *  affect an upload.  A one-line question with a name, rather than the
     *  caller spelling out the pair.</summary> */
    public static System.Boolean FullyDisabled
    { get { return !PreEnabled && !PostEnabled; } }
    /** <summary>EditorPrefs keys.  Namespaced by tool so they cannot collide
     *  with another package's preference, and stable so a restart does not
     *  silently re-enable a disabled phase.</summary> */
    public const System.String PrefPre  = "NZK.NanBuildHook.PreEnabled";
    public const System.String PrefPost = "NZK.NanBuildHook.PostEnabled";
    /** Master switch key.  Defaults true when absent, so a fresh checkout is
     *  active and an existing session that never touched the menu is too. */
    public const System.String PrefEnabled = "NZK.NanBuildHook.Enabled";
    /** <summary>Capture the avatar and repair it, unless the phase is off.
     *
     *  The repair runs HERE rather than in post-process: this is the phase where
     *  the object is known-alive and its meshes are known-writable, and it is
     *  the phase the menu-driven path already proved works.  Post-process is a
     *  safety net, not the primary trigger.</summary> */
    public System.Boolean OnPreprocessAvatar(UnityEngine.GameObject avatarGameObject)
    {
      /* _pending is set even when the phase is off: post-process reads it, and a
         user disabling PRE but not POST is explicitly asking for the post pass,
         which needs the clone.  Skipping the assignment would make that
         combination a silent no-op. */
      _pending = avatarGameObject;
      if (!PreEnabled)
      { NZK.E.C.w(69,"pre"); return true; }
      NZK.E.C.d(73,"pre - repairing nanimation weights on the upload clone");
      Repair(avatarGameObject);
      return true; }
    /** <summary>Safety net: re-run the repair in case a later package rebuilt a
     *  mesh after the pre-process pass.  The repair is idempotent, so a second
     *  run over already-correct weights is a no-op.</summary> */
    public void OnPostprocessAvatar()
    { var target = _pending;
      _pending = null;                       /* clear FIRST: never re-enter on a stale clone */
      if (!PostEnabled)
      { NZK.E.C.w(69,"post"); return; }
      NZK.E.C.d(73,"post - repairing nanimation weights on the upload clone");
      Repair(target); }
    /** <summary>One-line state, for a diagnostic or a menu click.
     *
     *  Names the MASTER switch and both phases, because the failure mode this
     *  reports is a half-set pair: "post is disabled" was read as "the hook is
     *  off" while the pre pass kept rewriting weights on every upload, and a
     *  line that only mentions the phase that changed cannot show that.</summary> */
    public static System.String State()
    { return "hook=" + (Enabled ? "ON" : "OFF") +
             " pre=" + (PreEnabled ? "ON" : "OFF") +
             " post=" + (PostEnabled ? "ON" : "OFF"); }
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
