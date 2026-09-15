#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class BuilderCore
  {/* ── UnityEngine.Mesh extraction ── */
  public static UnityEngine.Mesh GetMesh(UnityEngine.GameObject obj)
  { if (obj == null) return null;
    var smr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
    if (smr != null) return smr.sharedMesh;
    var mf = obj.GetComponent<UnityEngine.MeshFilter>();
    return mf != null ? mf.sharedMesh : null; }
  public static UnityEngine.Material[] GetMats(UnityEngine.GameObject obj)
  { if (obj == null) return new UnityEngine.Material[0];
    var smr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
    if (smr != null) return smr.sharedMaterials;
    var mr = obj.GetComponent<UnityEngine.MeshRenderer>();
    return mr != null ? mr.sharedMaterials : new UnityEngine.Material[0]; }
  /* ── Asset persistence ── */
  public static UnityEngine.Mesh SaveAsset(UnityEngine.Mesh mesh,System.String path,System.String name)
  { if (mesh == null || System.String.IsNullOrEmpty(path)) return null;
    var dir = System.IO.Path.GetDirectoryName(path);
    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
    UnityEditor.AssetDatabase.Refresh();
    mesh.name = name;
    if (IF_UE.Load<UnityEngine.Mesh>(path) != null) IF_UE.DeleteAsset(path);
    IF_UE.CreateAsset(mesh,path); IF_UE.SaveAndRefresh();
    return IF_UE.Load<UnityEngine.Mesh>(path); }
  public static void ApplySMR(UnityEngine.GameObject target,UnityEngine.Mesh mesh,UnityEngine.Material[]  mats,UnityEngine.Transform root,System.Collections.Generic.List<UnityEngine.Transform> bones)
  { if (target == null || mesh == null) return;
    var smr = target.GetComponent<UnityEngine.SkinnedMeshRenderer>();
    if (smr == null) smr = target.AddComponent<UnityEngine.SkinnedMeshRenderer>();
    smr.sharedMesh = mesh;
    smr.sharedMaterials = mats ?? new UnityEngine.Material[0];
    smr.rootBone = root;
    smr.bones = bones != null ? bones.ToArray() : new UnityEngine.Transform[0]; }
  /* ── Bone setup ── */
  public static System.Collections.Generic.List<UnityEngine.Transform> SetupBones(UnityEngine.Transform root)
  { var arm = root.Find("Armature");
    var armBones =  new System.Collections.Generic.List<UnityEngine.Transform>();
    var nameMap = new System.Collections.Generic.Dictionary<System.String,int>();
    if (arm != null) { AddBones(arm,armBones,nameMap); }
    return armBones; }
  static void AddBones(UnityEngine.Transform t,System.Collections.Generic.List<UnityEngine.Transform> list,System.Collections.Generic.Dictionary<System.String,int> map)
  { if (!map.ContainsKey(t.name)) { map[t.name] = list.Count; list.Add(t); }
    for (int i = 0; i < t.childCount; i++) AddBones(t.GetChild(i),list,map); }
  }
}
}
#endif
