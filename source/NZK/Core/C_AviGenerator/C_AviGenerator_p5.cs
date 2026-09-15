#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public partial class C_AviGenerator
  {
  public void BuildAvatarFromArmature()
  { AVController.BuildAvatarFromArmature(this); }
  public void BakeMesh()
  { MergeCore.BakeMesh(this); }
  public void BakeAll()
  { AVController.BakeAll(this); }
  public void BakeExpressionLayers()
  { AVController.BakeExpressionLayers(this); }
  public void BakeFxLayers()
  { AVController.BakeFxLayers(this); }
  public void BakeToggleGenerator()
  { AVController.BakeToggleGenerator(this); }
  public void SetAviRoot()
  { AVController.SetAviRoot(this); }
  public void BakeAviRoot()
  { AVController.BakeAviRoot(this); }
  public void BakeArmature()
  { AVController.BakeArmature(this); }
  public void ResetTransforms()
  { AVController.ResetTransforms(this); }
  public void BakeArmatureFinal()
  { AVController.BakeArmatureFinal(this); }
  }
}
}
#endif
