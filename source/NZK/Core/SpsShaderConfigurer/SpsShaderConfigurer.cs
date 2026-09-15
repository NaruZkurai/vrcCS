#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class SpsShaderConfigurer
  { public static System.Collections.Generic.List<UnityEngine.AnimationClip> GeneratePropertyClips(
      UnityEngine.GameObject markerObject,SpsMarkerService.MarkerConfig cfg,System.String outputBase)
    { var clips = new System.Collections.Generic.List<UnityEngine.AnimationClip>();
      if (markerObject == null) return clips;
      /* Socket property clip: sets all SPS2 material properties on the marker renderer */
      var propClip = new UnityEngine.AnimationClip(); propClip.name = "SPS2_" + (cfg.socketName ?? "Socket") + "_Props";
      System.String markerPath = UnityEditor.AnimationUtility.CalculateTransformPath(markerObject.transform,markerObject.transform.root);
      var mr = markerObject.GetComponent<UnityEngine.MeshRenderer>();
      if (mr != null)
      { var mat = mr.sharedMaterial; if (mat != null)
        { var mProps = SpsMarkerService.GetMaterialProperties(cfg,markerObject.transform);
          foreach (var (prop,val) in mProps)
          { var binding = new UnityEditor.EditorCurveBinding { path = markerPath,type = typeof(UnityEngine.MeshRenderer),propertyName = "material." + prop };
            UnityEditor.AnimationUtility.SetEditorCurve(propClip,binding,UnityEngine.AnimationCurve.Constant(0f,0f,val)); } } }
      SaveClip(propClip); clips.Add(propClip);
      return clips; }
    /* ── Extend renderer bounds so SPS2 marker is never culled ── */
    public static void ExtendBounds(UnityEngine.SkinnedMeshRenderer smr,UnityEngine.Vector3 markerWorldPos)
    { if (smr == null) return;
      var bounds = smr.localBounds;
      var localPos = smr.transform.InverseTransformPoint(markerWorldPos);
      bounds.Encapsulate(localPos);
      smr.localBounds = bounds; }
    static UnityEngine.AnimationClip SaveClip(UnityEngine.AnimationClip c) { return c; } }
}
}
#endif
