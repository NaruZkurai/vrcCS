namespace NZK
{
public static partial class Core {
public static partial class MenuItems
{
  [UnityEditor.MenuItem("Tools/NZK Toolkit/Fix Zero Scale Animations", false, 30)]
  public static void ValidateZeroScaleAnimations()
  { UnityEngine.Object selected = UnityEditor.Selection.activeObject;
    if (NZK.E.D.NerrOK(selected, 45, 10, selected)) {return;}
    UnityEngine.AnimationClip clip = selected as UnityEngine.AnimationClip;
    if (NZK.E.D.NerrOK<UnityEngine.Object>(clip, 21, 13, selected)) {return;}
    FixZeroScaleAnimations(clip);
  }
  public static void FixZeroScaleAnimations(UnityEngine.AnimationClip clip)
 {
    if (NZK.E.D.NerrOK<UnityEngine.Object>(clip, 23, 13, clip)) {return;}
    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    if (NZK.E.D.NerrOK<System.String>(
          System.String.IsNullOrEmpty(assetPath) ? null : assetPath,
          31, 24, assetPath)) {return;}
    string fullPath = NZK.E.PC(NZK.Core.ApplicationDataPath, assetPath.Substring("Assets/".Length));
    if (NZK.E.D.NerrOK<System.String>(
          System.IO.File.Exists(fullPath) ? fullPath : null,
          24, 25, fullPath)) {return;}
    string content = System.IO.File.ReadAllText(fullPath);
    bool hasZero = content.Contains("value: {x: 0") ||
                   content.Contains("value:\\s*\\{x:\\s*0") ||
                   content.Contains(": 0, y: 0, z: 0");
    if (!hasZero)
    { NZK.E.D.OK(clip, 30, 29);
      return; }
    if (FixAnimationClipScale(clip))
    { NZK.AD.R();
      NZK.E.D.OK(clip, 29, 29); }
    else
    { NZK.E.D.OK(clip, 50, 47); }
  }
  [UnityEditor.MenuItem("Tools/NZK Toolkit/Check Zero Scale Animations", false, 31)]
  public static void CheckZeroScaleAnimations()
  {
    UnityEngine.Object selected = UnityEditor.Selection.activeObject;
    if (NZK.E.D.NerrOK(selected, 45, 10, selected)) {return;}
    UnityEngine.AnimationClip clip = selected as UnityEngine.AnimationClip;
    if (NZK.E.D.NerrOK<UnityEngine.Object>(clip, 21, 13, selected)) {return;}
    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    if (NZK.E.D.NerrOK<System.String>(
          System.String.IsNullOrEmpty(assetPath) ? null : assetPath,
          31, 24, assetPath)) {return;}
    string fullPath = NZK.E.PC(NZK.Core.ApplicationDataPath, assetPath.Substring("Assets/".Length));
    if (NZK.E.D.NerrOK<System.String>(
          System.IO.File.Exists(fullPath) ? fullPath : null,
          24, 25, fullPath)) {return;}
    string content = System.IO.File.ReadAllText(fullPath);
    bool hasZero = content.Contains("value: {x: 0") ||
                   content.Contains("value:\\s*\\{x:\\s*0") ||
                   content.Contains(": 0, y: 0, z: 0");
    if (hasZero)
    { NZK.E.D.OK(clip, 38, 30); }
    else
    { NZK.E.D.OK(clip, 29, 30); }
  }
}
}
}
