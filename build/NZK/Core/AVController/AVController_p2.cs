#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class AVController
  {/* ================================================================
   *  Top-level bake orchestrator
   * ================================================================ */
  public static void BakeAll(C_AviGenerator hb)
  { UnityEngine.Debug.Log("[HB] BakeAll: starting for " + hb.name + " mode=" + hb.mode);
    System.String title = "Baking " + hb.name;
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Preparing...",0f);
    try {
    EnsureAvatarOnAnimator(hb);
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Baking avatar root...",0.1f);
    BakeAviRoot(hb);
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Armature...",0.2f);
    if (ChildAllowed(hb,HBChildren.ArmatureBuilder)) { BakeArmature(hb); }
    if (ChildAllowed(hb,HBChildren.ArmatureBuilder)) { BuildAvatarFromArmature(hb); }
    System.Boolean meshAllowed = ChildAllowed(hb,HBChildren.MeshBuilder) || ChildAllowed(hb,HBChildren.MeshGeneratorRoot);
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Mesh...",0.3f);
    if (meshAllowed) { MergeCore.BakeMesh(hb); }
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Gestures...",0.4f);
    System.Boolean gestAllowed = ChildAllowed(hb,HBChildren.GestureGenerator);
    if (gestAllowed) { AXController.BakeGestureLayers(hb); }
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Expressions...",0.5f);
    System.Boolean exprAllowed = ChildAllowed(hb,HBChildren.ExpressionsGenerator);
    if (exprAllowed) { BakeExpressionLayers(hb); }
    UnityEditor.EditorUtility.DisplayProgressBar(title,"FX...",0.6f);
    System.Boolean fxAllowed = ChildAllowed(hb,HBChildren.FxGenerator);
    if (fxAllowed) { BakeFxLayers(hb); }
    AssignDefaultExpressionAssets(hb);
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Animator...",0.7f);
    System.Boolean animAllowed = ChildAllowed(hb,HBChildren.AnimatorBuilder);
    if (animAllowed) { AXController.BakeGestureLayers(hb); }
    if (hb.blendshapeSources.Count > 0 || hb.nanimationSources.Count > 0 || hb.objectToggleSources.Count > 0 || hb.toggleGroup != null)
    { UnityEditor.EditorUtility.DisplayProgressBar(title,"Toggles...",0.8f); BakeToggleGenerator(hb); }
    var spsChild = hb.FindHB(HBChildren.SpsBuilder);
    if (spsChild != null)
    { var spsRoot = new UnityEngine.GameObject("SPS_Sockets");
    spsRoot.transform.SetParent(hb.NZKC_GO_AviRoot != null ? hb.NZKC_GO_AviRoot.transform : hb.transform,false); }
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Materials...",0.9f);
    MergeCore.BakeMaterialLinks(hb);
    ConfigureVRCAD(hb);
    UnityEditor.EditorUtility.DisplayProgressBar(title,"Finalizing...",0.95f);
    if (hb.NZKC_GO_AviRoot != null)
    { var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
    if (anim != null && anim.avatar != null && !anim.avatar.isHuman)
      UnityEngine.Debug.LogWarning("[HB] Bake complete but UnityEngine.Avatar is GENERIC. Assign armatureRoot and re-bake for humanoid.");
    else if (anim != null && anim.avatar != null && anim.avatar.isHuman)
      UnityEngine.Debug.Log("[HB] Bake complete with humanoid avatar.");
    else
      UnityEngine.Debug.LogWarning("[HB] Bake complete but NO UnityEngine.Avatar set. Assign armatureRoot and re-bake."); }
    UnityEngine.Debug.Log("[HB] Full bake.");
    } finally { UnityEditor.EditorUtility.ClearProgressBar(); } }
  }
}
}
#endif
