#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.InitializeOnLoad]
  public static class NZKDisableFury
  {
  const System.String PrefVrcPlay   = "NZK.VRCFury.Disabled";
  const System.String PrefVrcUpload = "NZK.VRCFury.UploadDisabled";
  const System.String PrefNdmfPlay   = "NZK.NDMF.PlayDisabled";
  const System.String PrefNdmfUpload = "NZK.NDMF.UploadDisabled";
  static NZKDisableFury() { UnityEditor.EditorApplication.delayCall += Apply; }
  /* ── VRCFury: Disable in playmode ────────────────────── */
  [UnityEditor.MenuItem("NZK/VRCFury/Disable in playmode")]
  static void ToggleVrcPlay()  { TogglePref(PrefVrcPlay); }
  [UnityEditor.MenuItem("NZK/VRCFury/Disable in playmode",true)]
  static System.Boolean ToggleVrcPlayV()
  { UnityEditor.Menu.SetChecked("NZK/VRCFury/Disable in playmode",UnityEditor.EditorPrefs.GetBool(PrefVrcPlay,false)); return true; }
  /* ── VRCFury: Disable in uploads ─────────────────────── */
  [UnityEditor.MenuItem("NZK/VRCFury/Disable in uploads")]
  static void ToggleVrcUpload() { TogglePref(PrefVrcUpload); }
  [UnityEditor.MenuItem("NZK/VRCFury/Disable in uploads",true)]
  static System.Boolean ToggleVrcUploadV()
  { UnityEditor.Menu.SetChecked("NZK/VRCFury/Disable in uploads",UnityEditor.EditorPrefs.GetBool(PrefVrcUpload,false)); return true; }
  /* ── NDMF: Disable in playmode ───────────────────────── */
  [UnityEditor.MenuItem("NZK/NDMF/Disable in playmode")]
  static void ToggleNdmfPlay()  { TogglePref(PrefNdmfPlay); }
  [UnityEditor.MenuItem("NZK/NDMF/Disable in playmode",true)]
  static System.Boolean ToggleNdmfPlayV()
  { UnityEditor.Menu.SetChecked("NZK/NDMF/Disable in playmode",UnityEditor.EditorPrefs.GetBool(PrefNdmfPlay,false)); return true; }
  /* ── NDMF: Disable in uploads ────────────────────────── */
  [UnityEditor.MenuItem("NZK/NDMF/Disable in uploads")]
  static void ToggleNdmfUpload() { TogglePref(PrefNdmfUpload); }
  [UnityEditor.MenuItem("NZK/NDMF/Disable in uploads",true)]
  static System.Boolean ToggleNdmfUploadV()
  { UnityEditor.Menu.SetChecked("NZK/NDMF/Disable in uploads",UnityEditor.EditorPrefs.GetBool(PrefNdmfUpload,false)); return true; }
  /* ── Common toggle helper ────────────────────────────── */
  static void TogglePref(System.String key)
  { UnityEditor.EditorPrefs.SetBool(key,!UnityEditor.EditorPrefs.GetBool(key,false));
    Apply(); }
  /* ── Apply all overrides ─────────────────────────────── */
  static void Apply()
  { System.Boolean vrcPlayOff   = UnityEditor.EditorPrefs.GetBool(PrefVrcPlay,false);
    System.Boolean vrcUploadOff = UnityEditor.EditorPrefs.GetBool(PrefVrcUpload,false);
    System.Boolean ndmfPlayOff  = UnityEditor.EditorPrefs.GetBool(PrefNdmfPlay,false);
    System.Boolean ndmfUpOff  = UnityEditor.EditorPrefs.GetBool(PrefNdmfUpload,false);
    /* VRCFury playmode */
    UnityEditor.EditorPrefs.SetBool("com.vrcfury.playMode",!vrcPlayOff);
    /* VRCFury upload */
    UnityEditor.SessionState.SetBool("com.vrcfury.useInUpload",!vrcUploadOff);
    /* NDMF playmode */
    SetNdmfProperty("ApplyOnPlay",!ndmfPlayOff);
    /* NDMF upload */
    SetNdmfProperty("ApplyOnBuild",!ndmfUpOff);
    /* Log summary */
    var log = "[NZK]";
    log += vrcPlayOff   ? " VF:play=OFF"   : " VF:play=ON";
    log += vrcUploadOff ? " upload=OFF"  : " upload=ON";
    log += ndmfPlayOff  ? " | NDMF:play=OFF" : " | NDMF:play=ON";
    log += ndmfUpOff  ? " upload=OFF"  : " upload=ON";
    UnityEngine.Debug.Log(log); }
  /* ── Set a single NDMF property via reflection ───────── */
  static void SetNdmfProperty(System.String propName,System.Boolean value)
  { var asm = System.AppDomain.CurrentDomain.GetAssemblies();
    foreach (var a in asm)
    { if (!a.GetName().Name.Contains("nadena.dev.ndmf")) continue;
    var cfgType = a.GetType("nadena.dev.ndmf.config.Config");
    if (cfgType == null) continue;
    var prop = cfgType.GetProperty(propName,System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    prop?.SetValue(null,value);
    break; }
  }
  }
}
}
#endif
