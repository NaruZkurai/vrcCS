#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class NZKNanHookMenu
  {
    /* ── MASTER: disable the whole hook ──────────────────── */
    [UnityEditor.MenuItem("NZK/NaNimation/Disable ALL Repair (upload unmodified)")]
    static void ToggleMaster() { NanBuildHook.Enabled = !NanBuildHook.Enabled; Report(); }
    [UnityEditor.MenuItem("NZK/NaNimation/Disable ALL Repair (upload unmodified)",true)]
    static System.Boolean ToggleMasterV()
    { UnityEditor.Menu.SetChecked("NZK/NaNimation/Disable ALL Repair (upload unmodified)",!NanBuildHook.Enabled); return true; }
    /* ── Pre-process: disable during uploads ─────────────── */
    [UnityEditor.MenuItem("NZK/NaNimation/Disable Preprocess Repair")]
    static void TogglePre() { NanBuildHook.PreEnabled = !NanBuildHook.PreEnabled; Report(); }
    [UnityEditor.MenuItem("NZK/NaNimation/Disable Preprocess Repair",true)]
    static System.Boolean TogglePreV()
    { UnityEditor.Menu.SetChecked("NZK/NaNimation/Disable Preprocess Repair",!NanBuildHook.PreEnabled); return true; }
    /* ── Post-process: disable during uploads ────────────── */
    [UnityEditor.MenuItem("NZK/NaNimation/Disable Postprocess Repair")]
    static void TogglePost() { NanBuildHook.PostEnabled = !NanBuildHook.PostEnabled; Report(); }
    [UnityEditor.MenuItem("NZK/NaNimation/Disable Postprocess Repair",true)]
    static System.Boolean TogglePostV()
    { UnityEditor.Menu.SetChecked("NZK/NaNimation/Disable Postprocess Repair",!NanBuildHook.PostEnabled); return true; }
    /* ── Restore everything ──────────────────────────────── */
    [UnityEditor.MenuItem("NZK/NaNimation/Enable ALL Repair")]
    static void EnableAll()
    { NanBuildHook.Enabled = true;
      NanBuildHook.PreEnabled = true;
      NanBuildHook.PostEnabled = true;
      Report(); }
    /** <summary>Log the full state - master plus both phases - on every change.
     *
     *  Reporting the MASTER switch and the phases together is the point.  A line
     *  that names only the phase just toggled reads as a statement about the
     *  whole hook: "post is disabled" was taken for "the hook is off" while the
     *  pre pass went on rewriting weights on every upload.  Printing all three
     *  makes a half-set pair impossible to mistake.</summary> */
    static void Report()
    { System.String state = NanBuildHook.State();
      if (!NanBuildHook.Enabled)
      { NZK.E.C.w(69,"ALL");
        NZK.E.C.w(70,state + " - the avatar uploads UNMODIFIED"); }
      else if (!NanBuildHook.PreEnabled && !NanBuildHook.PostEnabled)
      { NZK.E.C.w(69,"pre+post");
        NZK.E.C.w(70,state + " - both phases off, the avatar uploads unmodified"); }
      else { NZK.E.C.d(70,state); } }
  }
}
}
#endif
