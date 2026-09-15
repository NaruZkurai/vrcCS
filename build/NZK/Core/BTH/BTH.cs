#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class BTH
  { public const System.String TglPrefix = "(b-gt)";
  const System.String Root = NZKPaths.Root;
  public static System.String S(System.String v) => System.String.IsNullOrEmpty(v) ? "Unnamed" : v.Replace('/','_').Replace('\\','_').Trim();
  public static System.String AR(System.String a) => Root + "/" + S(a);
  public static System.String AF(System.String a,System.String c) => AR(a) + "/Animations/" + c;
  public static System.String BF(System.String a,System.String c) => AR(a) + "/BlendTrees/" + c;
  public static System.String IAP(System.String a,System.String c,System.String d,System.String s) => AF(a,c) + "/" + d + s;
  public static System.String IBP(System.String a,System.String c,System.String d) => BF(a,c) + "/" + d + ".asset";
  public static UnityEngine.AnimationClip LCC(System.String path,UnityEngine.AnimationClip src)
  { var e = IF_UE.Load<UnityEngine.AnimationClip>(path);
    if (e != null) return e;
    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
    IF_UE.CreateAsset(src,path); return src; } }
}
}
#endif
