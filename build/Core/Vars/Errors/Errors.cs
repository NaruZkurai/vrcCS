#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Vars {
public static class Errors
  { public static void Log(System.String code,System.String message,UnityEngine.Object ctx = null)
    { var msg = "[NZK:" + code + "] " + message;
      if (ctx != null) UnityEngine.Debug.LogError(msg,ctx); else UnityEngine.Debug.LogError(msg); }
    public static void Warn(System.String code,System.String message,UnityEngine.Object ctx = null)
    { var msg = "[NZK:" + code + "] " + message;
      if (ctx != null) UnityEngine.Debug.LogWarning(msg,ctx); else UnityEngine.Debug.LogWarning(msg); }
    public static System.String Fix(System.String code)
    { switch (code)
      { case E_NoObjects: return "Select GameObjects in the Hierarchy first.";
        case E_NoAviGenerator: return "Add an C_AviGenerator component or use Ensure Baker.";
        case E_NoAviRoot: return "Set the Avatar Root field on the C_AviGenerator.";
        case E_NullClip: return "Check that the source object has valid animations.";
        case E_NoMainDbt: return "Generate toggles first, then run Generate/DBT.";
        case E_NoBaker: return "Click Auto-Create or add a Higharchy Baker component.";
        case E_NoSpsSockets: return "Add a C_NzkSpsSocket component or VRCFury HapticSocket first.";
        case E_NoAviRootForSps: return "Select a GameObject inside the avatar hierarchy.";
        default: return "No additional fix information available."; } }
    public static System.String Dialog(System.String code,System.String message)
    { return "[" + code + "] " + message + "\n\n" + Fix(code); }
    public const System.String E_NoObjects      = "E001";
    public const System.String E_NoAviGenerator = "E002";
    public const System.String E_NoAviRoot      = "E003";
    public const System.String E_NullClip       = "E004";
    public const System.String E_NoMainDbt      = "E005";
    public const System.String E_NoBaker        = "E006";
    public const System.String E_NoSpsSockets   = "E007";
    public const System.String E_NoAviRootForSps = "E008"; }
}
}
}
#endif
