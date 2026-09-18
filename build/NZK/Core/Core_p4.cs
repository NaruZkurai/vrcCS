#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
namespace NZK
{
public static partial class Core {
  /** <summary>Upload-time trigger for the nanimation weight repair.
   *
   *  The repair runs in PRE-process (avatar alive, meshes writable), and the
   *  post-process callback re-runs it as a safety net for anything that rebuilt
   *  a mesh after us.  Both phases are required: see the header.</summary> */
  
  /** <summary>Menu surface for the upload-hook switches.
   *
   *  Same shape as NZKDisableFury (the toolkit's existing disable switch):
   *  EditorPrefs for the state, an NZK/<Tool>/<Action> menu path, and a
   *  validate callback that only calls SetChecked.  Matching it means the two
   *  switches are found and read the same way, instead of this one inventing a
   *  second convention for the same idea.
   *
   *  A "disable" switch rather than an "enable" one for the same reason the
   *  other package switches are: the default state has to be ACTIVE for a fresh
   *  checkout, and a checkbox labelled "Disabled" that starts unchecked says
   *  that directly.  An "Enabled" box that starts checked is the same state with
   *  an extra mental inversion.</summary> */
  
}
}
#endif
