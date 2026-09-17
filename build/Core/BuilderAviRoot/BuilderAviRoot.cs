#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class BuilderAviRoot
  {
  /* ── Build the UnityEngine.Avatar root (delegates to AVController) ──────── */
  public static void Build(C_AviGenerator hb)
  { AVController.BakeAviRoot(hb);
    /* ── Pipeline ID resolution ── */
    ResolveAndSetPipeline(hb); }
  /* ── Pipeline resolution — uses self field,then children,then scene-wide search ── */
  static void ResolveAndSetPipeline(C_AviGenerator hb)
  { var pid = hb.pipelineId;
    UnityEngine.Debug.Log("[HB] " + "Pipeline search: self.pipelineId='" + (pid ?? "null") + "'");
    if (System.String.IsNullOrEmpty(pid))
    {
    var pmType = AVController.FindPMType();
    UnityEngine.Debug.Log("[HB] " + "Pipeline search: pmType=" + (pmType != null ? pmType.Name : "null"));
    /* Search children */
    foreach (UnityEngine.Transform c in hb.transform)
    {
      var h = c.GetComponent<C_AviGenerator>();
      if (h != null && (h.mode == E_AviGeneratorMode.AviRootBuilder || h.mode == E_AviGeneratorMode.PipelineID))
      { pid = AVController.ReadPipelineId(c,h,pmType); if (!System.String.IsNullOrEmpty(pid)) break; }
    }
    /* Fallback: scene-wide search */
    if (System.String.IsNullOrEmpty(pid))
    {
      foreach (var h in UnityEngine.Object.FindObjectsOfType<C_AviGenerator>())
      {
      if (h == hb || h.mode != E_AviGeneratorMode.PipelineID) continue;
      pid = AVController.ReadPipelineId(h.transform,h,pmType);
      if (!System.String.IsNullOrEmpty(pid)) break; }
    }
    }
    if (!System.String.IsNullOrEmpty(pid))
    {
    var pmType = AVController.FindPMType();
    if (pmType != null)
    {
      var old = hb.NZKC_GO_AviRoot.GetComponent(pmType);
      if (old != null) UnityEngine.Object.DestroyImmediate(old);
      var pm = hb.NZKC_GO_AviRoot.AddComponent(pmType);
      WriteBlueprintId(pm,pmType,pid);
      UnityEngine.Debug.Log("[HB] " + "Pipeline set: " + pid); }
    else UnityEngine.Debug.LogWarning("[HB] " + "VRC_PipelineManager type not found — can't set blueprint ID."); }
    else
    {
    var pmType = AVController.FindPMType();
    if (pmType != null && hb.NZKC_GO_AviRoot.GetComponent(pmType) == null)
    {
      var pm = hb.NZKC_GO_AviRoot.AddComponent(pmType);
      pid = System.Guid.NewGuid().ToString();
      WriteBlueprintId(pm,pmType,pid);
      UnityEngine.Debug.Log("[HB] " + "PipelineManager added with auto-assigned blueprint ID: " + pid); }
    }
  }
  static System.String ReadBlueprintId(UnityEngine.Object pm,System.Type pmType) => AVController.ReadBlueprintId(pm,pmType);
  static void WriteBlueprintId(UnityEngine.Object pm,System.Type pmType,System.String id) => AVController.WriteBlueprintId(pm,pmType,id); }
}
}
#endif
