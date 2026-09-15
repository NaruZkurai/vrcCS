#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class C_NzkSpsSocket {
public enum CompatibilityMode
    { All,       // SPS1 + TPS + DPS + SPS2 — full VRCFury compat (default)
      SPS1,     // Legacy light-based sockets only
      SPS2,     // Shader-based VRCFury SPS2 only
      TPS,      // The Penetration System only
      DPS }
}
}
}
#endif
