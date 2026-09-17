#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class BlendshapeMenuActions
  {
  public static void CreateBlendshapeToggle()
  { var objs = UnityEditor.Selection.gameObjects;
    if (objs == null || objs.Length == 0)
    { UnityEngine.Debug.LogWarning("[NZK] Generate Blendshape: Select a GameObject with a SkinnedMeshRenderer."); return; }
    CreateBlendshapeToggleSingle(objs[0]); }
  public static void CreateBlendshapeToggleSingle(UnityEngine.GameObject target)
  { if (target == null) return;
    var smr = target.GetComponent<UnityEngine.SkinnedMeshRenderer>() ?? target.GetComponentInChildren<UnityEngine.SkinnedMeshRenderer>();
    if (smr == null)
    { UnityEngine.Debug.LogWarning("[NZK] Generate Blendshape: No SkinnedMeshRenderer found on " + target.name); return; }
    var btd = target.GetComponent<BlendshapeToggleDefinition>() ?? target.AddComponent<BlendshapeToggleDefinition>();
    IF_UE.RegCr(target,"Create BlendshapeToggleDefinition");
    btd.targetRenderer = smr;
    btd.parameterName = BTH.TglPrefix + BTH.S(target.name);
    if (smr.sharedMesh != null && smr.sharedMesh.blendShapeCount > 0)
    btd.blendshapeName = smr.sharedMesh.GetBlendShapeName(0);
    UnityEditor.Selection.activeObject = target; }
  public static void CreateMultiBlendshapeToggle()
  { var objs = UnityEditor.Selection.gameObjects;
    if (objs == null || objs.Length == 0)
    { UnityEngine.Debug.LogWarning("[NZK] Generate Multi Blendshape: Select one or more GameObjects with BlendshapeToggleDefinitions."); return; }
    var parent = objs[0];
    var multi = parent.GetComponent<MultiBlendshapeToggleDefinition>() ?? parent.AddComponent<MultiBlendshapeToggleDefinition>();
    IF_UE.RegCr(parent,"Create MultiBlendshapeToggleDefinition");
    multi.childToggles.Clear();
    foreach (var obj in objs)
    { var btd = obj.GetComponent<BlendshapeToggleDefinition>();
    if (btd != null && !multi.childToggles.Contains(btd)) multi.childToggles.Add(btd); }
    multi.parameterName = BTH.TglPrefix + BTH.S(parent.name);
    UnityEditor.Selection.activeObject = parent; }
  }
}
}
#endif
