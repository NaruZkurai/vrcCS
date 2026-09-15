#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
class ProxySyncPostprocessor : UnityEditor.AssetPostprocessor
  {   static void OnPostprocessAllAssets(
      System.String[] imported,System.String[] _d,System.String[] _m,System.String[] _f)
    {   foreach (var p in imported)
      {   if (p.EndsWith(".mat",System.StringComparison.OrdinalIgnoreCase) ||
          p.EndsWith(".fbx",System.StringComparison.OrdinalIgnoreCase) ||
          p.EndsWith(".asset",System.StringComparison.OrdinalIgnoreCase))
        {   ProxySyncHooks.TriggerAll(); return;   }   }   }   }
}
}
#endif
