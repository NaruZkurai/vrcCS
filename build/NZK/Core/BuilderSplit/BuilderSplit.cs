#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class BuilderSplit {
  
  static int DominantBone(ref UnityEngine.BoneWeight w)
  { float m = w.weight0; int bi = w.boneIndex0;
    if (w.weight1 > m) { m = w.weight1; bi = w.boneIndex1; }
    if (w.weight2 > m) { m = w.weight2; bi = w.boneIndex2; }
    if (w.weight3 > m) bi = w.boneIndex3;
    return bi; }
  static void CopyVertexData(UnityEngine.Mesh src,int[] srcVerts,UnityEngine.Mesh dst)
  { var pos = src.vertices; var dPos = new UnityEngine.Vector3[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++) dPos[i] = pos[srcVerts[i]];
    dst.vertices = dPos;
    var nrm = src.normals;
    if (nrm != null && nrm.Length > 0)
    { var dNrm = new UnityEngine.Vector3[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++) dNrm[i] = nrm[srcVerts[i]];
    dst.normals = dNrm; }
    var tan = src.tangents;
    if (tan != null && tan.Length > 0)
    { var dTan = new UnityEngine.Vector4[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++) dTan[i] = tan[srcVerts[i]];
    dst.tangents = dTan; }
    var uv = src.uv;
    if (uv != null && uv.Length > 0)
    { var dUv = new UnityEngine.Vector2[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++) dUv[i] = uv[srcVerts[i]];
    dst.uv = dUv; }
    var uv2 = src.uv2;
    if (uv2 != null && uv2.Length > 0)
    { var dUv2 = new UnityEngine.Vector2[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++) dUv2[i] = uv2[srcVerts[i]];
    dst.uv2 = dUv2; }
    var col = src.colors;
    if (col != null && col.Length > 0)
    { var dCol = new UnityEngine.Color[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++) dCol[i] = col[srcVerts[i]];
    dst.colors = dCol; } }
  static UnityEngine.BoneWeight[] RemapBoneWeights(UnityEngine.Mesh src,int[] srcVerts,System.Collections.Generic.List<UnityEngine.Transform> srcBones,System.Collections.Generic.List<UnityEngine.Transform> outBones,out System.Collections.Generic.List<UnityEngine.Transform> usedBones)
  { var nameMap = new System.Collections.Generic.Dictionary<System.String,int>();
    usedBones =  new System.Collections.Generic.List<UnityEngine.Transform>();
    for (int i = 0; i < outBones.Count; i++)
    { var b = outBones[i]; if (b != null && !nameMap.ContainsKey(b.name)) { nameMap[b.name] = usedBones.Count; usedBones.Add(b); } }
    var srcBW = src.boneWeights;
    if (srcBW == null || srcBW.Length == 0) { usedBones = outBones; return null; }
    var r = new UnityEngine.BoneWeight[srcVerts.Length];
    for (int i = 0; i < srcVerts.Length; i++)
    { var w = srcBW[srcVerts[i]];
    r[i] = new UnityEngine.BoneWeight
    { boneIndex0 = RemapBone(w.boneIndex0,srcBones,nameMap),weight0 = w.weight0,boneIndex1 = RemapBone(w.boneIndex1,srcBones,nameMap),weight1 = w.weight1,boneIndex2 = RemapBone(w.boneIndex2,srcBones,nameMap),weight2 = w.weight2,boneIndex3 = RemapBone(w.boneIndex3,srcBones,nameMap),weight3 = w.weight3 }; }
    return r; }
  static int RemapBone(int srcIdx,System.Collections.Generic.List<UnityEngine.Transform> srcBones,System.Collections.Generic.Dictionary<System.String,int> nameMap)
  { if (srcIdx < 0 || srcIdx >= srcBones.Count || srcBones[srcIdx] == null) return 0;
    return nameMap.TryGetValue(srcBones[srcIdx].name,out int idx) ? idx : 0; }
  static void CopyBlendShapes(UnityEngine.Mesh src,int[] srcVerts,UnityEngine.Mesh dst)
  { if (src.blendShapeCount == 0) return;
    for (int bs = 0; bs < src.blendShapeCount; bs++)
    { var name = src.GetBlendShapeName(bs);
    var fc = src.GetBlendShapeFrameCount(bs);
    for (int f = 0; f < fc; f++)
    { var w = src.GetBlendShapeFrameWeight(bs,f);
      var sV = new UnityEngine.Vector3[src.vertexCount]; var sN = new UnityEngine.Vector3[src.vertexCount]; var sT = new UnityEngine.Vector3[src.vertexCount];
      src.GetBlendShapeFrameVertices(bs,f,sV,sN,sT);
      var dV = new UnityEngine.Vector3[srcVerts.Length]; var dN = new UnityEngine.Vector3[srcVerts.Length]; var dT = new UnityEngine.Vector3[srcVerts.Length];
      for (int i = 0; i < srcVerts.Length; i++)
      { dV[i] = sV[srcVerts[i]]; dN[i] = sN[srcVerts[i]]; dT[i] = sT[srcVerts[i]]; }
      dst.AddBlendShapeFrame(name,w,dV,dN,dT); } } }
  public static SplitResult Split(MeshSplitterDefinition config,UnityEngine.Transform avatarRoot)
  { var r = new SplitResult { success = false };
    if (config.sourceSMR == null) { r.errorMessage = "No source SMR"; return r; }
    var srcMesh = config.sourceSMR.sharedMesh;
    if (srcMesh == null) { r.errorMessage = "No shared mesh on source SMR"; return r; }
    var bones = config.sourceSMR.bones;
    if (bones == null || bones.Length == 0) { r.errorMessage = "No bones on source SMR"; return r; }
    var selected = new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(config.splitGroups, g => g.extract), g => g.groupName));
    if (selected.Count == 0) { r.errorMessage = "No vertex groups selected"; return r; }
    var bw = srcMesh.boneWeights;
    if (bw == null || bw.Length == 0) { r.errorMessage = "No bone weights on mesh"; return r; }
    var tVerts =  new System.Collections.Generic.List<int>(); var rVerts =  new System.Collections.Generic.List<int>();
    for (int i = 0; i < srcMesh.vertexCount; i++)
    { var bi = DominantBone(ref bw[i]);
    var bn = bi >= 0 && bi < bones.Length && bones[bi] != null ? bones[bi].name : null;
    if (bn != null && selected.Contains(bn)) tVerts.Add(i); else rVerts.Add(i); }
    if (tVerts.Count == 0) { r.errorMessage = "No vertices match selected groups"; return r; }
    if (rVerts.Count == 0) { r.errorMessage = "All vertices match selected groups — nothing to remain"; return r; }
    var mats = config.sourceSMR.sharedMaterials;
    r.materials = mats;
    var boneList = System.Linq.Enumerable.ToList(bones);
    if (config.createTargetMesh) BuildOutput(srcMesh,tVerts,boneList,out r.targetMesh,out r.targetBones);
    if (config.createRemainingMesh) BuildOutput(srcMesh,rVerts,boneList,out r.remainingMesh,out r.remainingBones);
    r.success = true;
    return r; }
  static void BuildOutput(UnityEngine.Mesh src,System.Collections.Generic.List<int> verts,System.Collections.Generic.List<UnityEngine.Transform> allBones,out UnityEngine.Mesh m,out System.Collections.Generic.List<UnityEngine.Transform> usedBones)
  { m = new UnityEngine.Mesh();
    if (verts.Count > 65535) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    var va = verts.ToArray();
    CopyVertexData(src,va,m);
    m.subMeshCount = src.subMeshCount;
    var srcVSet = new System.Collections.Generic.HashSet<int>(verts);
    for (int sm = 0; sm < src.subMeshCount; sm++)
    { var tris = src.GetTriangles(sm);
    var outTris =  new System.Collections.Generic.List<int>();
    for (int ti = 0; ti < tris.Length; ti += 3)
    { if (srcVSet.Contains(tris[ti]) && srcVSet.Contains(tris[ti + 1]) && srcVSet.Contains(tris[ti + 2]))
      { outTris.Add(verts.IndexOf(tris[ti]));
      outTris.Add(verts.IndexOf(tris[ti + 1]));
      outTris.Add(verts.IndexOf(tris[ti + 2])); } }
    m.SetTriangles(outTris.ToArray(),sm); }
    var sb = src.boneWeights;
    if (sb != null && sb.Length > 0)
    { var bw = RemapBoneWeights(src,va,allBones,allBones,out usedBones);
    m.boneWeights = bw ?? new UnityEngine.BoneWeight[va.Length];
    var bindposes = new UnityEngine.Matrix4x4[usedBones.Count];
    for (int i = 0; i < usedBones.Count; i++)
      bindposes[i] = usedBones[i].worldToLocalMatrix * (usedBones[0]?.root?.localToWorldMatrix ?? UnityEngine.Matrix4x4.identity);
    m.bindposes = bindposes; }
    else { usedBones =  new System.Collections.Generic.List<UnityEngine.Transform>(); m.bindposes = new UnityEngine.Matrix4x4[0]; }
    CopyBlendShapes(src,va,m); }
  public static void SaveAndApply(SplitResult result,MeshSplitterDefinition config,UnityEngine.Transform avatarRoot)
  { if (!result.success) { UnityEngine.Debug.LogError("Split failed: " + result.errorMessage); return; }
    var dir = config.outputFolder;
    System.IO.Directory.CreateDirectory(dir);
    var rootBone = config.sourceSMR.rootBone ?? avatarRoot;
    if (result.targetMesh != null && config.createTargetMesh)
    { var tn = System.String.IsNullOrEmpty(config.targetMeshName) ? config.sourceSMR.gameObject.name + "_Target" : config.targetMeshName;
    var tp = dir + "/" + tn + ".asset";
    BuilderCore.SaveAsset(result.targetMesh,tp,tn);
    var tgo = new UnityEngine.GameObject(tn);
    tgo.transform.SetParent(config.sourceSMR.transform.parent,false);
    BuilderCore.ApplySMR(tgo,result.targetMesh,config.preserveMaterials ? result.materials : null,rootBone,result.targetBones); }
    if (result.remainingMesh != null && config.createRemainingMesh)
    { var rn = System.String.IsNullOrEmpty(config.remainingMeshName) ? config.sourceSMR.gameObject.name + "_Remaining" : config.remainingMeshName;
    var rp = dir + "/" + rn + ".asset";
    BuilderCore.SaveAsset(result.remainingMesh,rp,rn);
    var rgo = new UnityEngine.GameObject(rn);
    rgo.transform.SetParent(config.sourceSMR.transform.parent,false);
    BuilderCore.ApplySMR(rgo,result.remainingMesh,config.preserveMaterials ? result.materials : null,rootBone,result.remainingBones); }
    IF_UE.SaveAndRefresh(); }
  
}
}
}
#endif
