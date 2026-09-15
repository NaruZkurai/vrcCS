#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class ProxySyncOps
  { public static void EnforceChildrenDisabled(ProxySync s)
    {   foreach (UnityEngine.Transform child in s.transform)
      {   if (child.gameObject.activeSelf)
          child.gameObject.SetActive(false);   }   }
    /* Applies swap-source (B) to target. Returns true if a swap ran. */
    public static System.Boolean PerformSwap(ProxySync s)
    {   if (s.targetObject == null) return false;
      var swapSMR   = s.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      var targetSMR = s.targetObject.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (swapSMR == null || targetSMR == null) return false;
      if (SMRMatches(targetSMR,swapSMR)) return false;
      if (!s.HasOriginal)
      {   s.SetOriginalData(
          targetSMR.sharedMesh,targetSMR.sharedMaterials);
        IF_UE.SetDirty(s);   }
      UnityEditor.Undo.RecordObject(targetSMR,"Proxy Sync Swap");
      CopySMR(swapSMR,targetSMR);
      IF_UE.SetDirty(targetSMR);
      return true;   }
    /* Re-captures target's current data as stored original (A).
     * Refuses when the target already carries the swap (B) values. */
    public static void RecaptureOriginal(ProxySync s)
    {   if (s.targetObject == null) return;
      var targetSMR = s.targetObject.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      var swapSMR   = s.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (targetSMR == null || swapSMR == null) return;
      if (SMRMatches(targetSMR,swapSMR))
      {   UnityEngine.Debug.LogWarning("[NZK] Proxy Sync: Target already has swap (B) values applied, cannot re-capture original.");
        return;   }
      s.SetOriginalData(targetSMR.sharedMesh,targetSMR.sharedMaterials);
      IF_UE.SetDirty(s);   }
    internal static System.Boolean SMRMatches(
      UnityEngine.SkinnedMeshRenderer a,UnityEngine.SkinnedMeshRenderer b)
    {   if (a.sharedMesh != b.sharedMesh) return false;
      var am = a.sharedMaterials;
      var bm = b.sharedMaterials;
      if (am.Length != bm.Length) return false;
      for (int i = 0; i < am.Length; i++)
      { if (am[i] != bm[i]) return false; }
      return true;   }
    static void CopySMR(UnityEngine.SkinnedMeshRenderer src,UnityEngine.SkinnedMeshRenderer dst)
    {   dst.sharedMesh    = src.sharedMesh;
      dst.sharedMaterials = (UnityEngine.Material[])src.sharedMaterials.Clone();   }
  }
}
}
#endif
