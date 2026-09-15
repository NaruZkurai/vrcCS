#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.InitializeOnLoad]
  public static class NZKGraphicsSetup
  { static NZKGraphicsSetup()
    { UnityEditor.EditorApplication.delayCall += Diagnose;}
    public static void Diagnose()
    { var gfx = UnityEngine.SystemInfo.graphicsDeviceType;
      if (gfx == UnityEngine.Rendering.GraphicsDeviceType.Vulkan) {return;}
      UnityEngine.Debug.LogWarning(
        "[NZK] UnityEditor.Editor is on " + gfx + " (Vulkan preferred for performance).\n" +
        "  Add these flags to Unity Hub → UnityEditor.Editor → Advanced command line:\n" +
        "    -force-vulkan -force-vulkan-layers -force-gfx-direct\n" +
        "  Or create a desktop shortcut with:\n" +
        "    unity-editor %F -force-vulkan -force-vulkan-layers -force-gfx-direct"); }
  }
}
}
#endif
