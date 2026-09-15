#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class BuilderMerge
  {/* ── High-level merge entry point for the builder pipeline ── */
  public static MergeResult Merge(System.Collections.Generic.List<MeshEntry> entries,UnityEngine.Transform avatarRoot)
  { if (entries == null || entries.Count == 0)
    return new MergeResult { Success = false,ErrorMessage = "No entries" };
    var valid = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(entries, e => e != null && e.SourceMesh != null));
    if (valid.Count == 0) return new MergeResult { Success = false,ErrorMessage = "No valid meshes" };
    var vr =  new System.Collections.Generic.List<VRange>(); int vo = 0;
    var xforms =  new System.Collections.Generic.List<UnityEngine.Matrix4x4>();
    var allGroupedCis =  new System.Collections.Generic.List<UnityEngine.CombineInstance>();
    var finalMats =  new System.Collections.Generic.List<UnityEngine.Material>();
    foreach (var e in valid)
    { var m = e.SourceMesh;
    if (m == null) continue;
    var x = e.SourceObject != null ? e.SourceObject.transform.localToWorldMatrix : UnityEngine.Matrix4x4.identity;
    var mats = e.Materials;
    if (mats == null || mats.Length == 0)
      mats = BuilderCore.GetMats(e.SourceObject);
    if (e.UseUniqueMaterial)
    {
      /* Keep each submesh separate (no dedup with others) */
      for (int s = 0; s < m.subMeshCount; s++)
      { var mat = (s < mats.Length && mats[s] != null) ? mats[s] : null;
      allGroupedCis.Add(new UnityEngine.CombineInstance { mesh = m,subMeshIndex = s,transform = x });
      if (mat != null && !finalMats.Contains(mat)) finalMats.Add(mat); }
    }
    else
    {
      /* Group by material across entries to deduplicate shared materials */
      for (int s = 0; s < m.subMeshCount; s++)
      { var mat = (s < mats.Length && mats[s] != null) ? mats[s] : null;
      int existing = finalMats.IndexOf(mat);
      if (existing < 0)
      { finalMats.Add(mat);
        allGroupedCis.Add(new UnityEngine.CombineInstance { mesh = m,subMeshIndex = s,transform = x }); }
      else
      { /* Merge into existing submesh slot */
        var existingCI = allGroupedCis[existing];
        if (existingCI.mesh == m && existingCI.subMeshIndex == s) continue;
        var temp = new UnityEngine.Mesh();
        if (vo > 65535) temp.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        temp.CombineMeshes(new UnityEngine.CombineInstance[] { existingCI,new UnityEngine.CombineInstance { mesh = m,subMeshIndex = s,transform = x } },true,true);
        allGroupedCis[existing] = new UnityEngine.CombineInstance { mesh = temp,subMeshIndex = 0,transform = UnityEngine.Matrix4x4.identity }; }
      }
    }
    vr.Add(new VRange { SV = vo,VC = m.vertexCount,SS = 0,SC = m.subMeshCount });
    xforms.Add(x); vo += m.vertexCount; }
    if (allGroupedCis.Count == 0)
    return new MergeResult { Success = false,ErrorMessage = "No submeshes after material grouping" };
    /* Final combine: keep submeshes separate per material */
    var cm = new UnityEngine.Mesh { name = "CombinedMesh" };
    if (vo > 65535) cm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    try { cm.CombineMeshes(allGroupedCis.ToArray(),false,true); }
    catch (System.ArgumentException ex)
    { if (ex.Message.Contains("index format"))
    { cm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
      cm.CombineMeshes(allGroupedCis.ToArray(),false,true); }
    else return new MergeResult { Success = false,ErrorMessage = ex.Message }; }
    /* Copy all UV channels (CombineMeshes only copies UV1) */
    {
    var uv2 =  new System.Collections.Generic.List<UnityEngine.Vector2>(); var uv3 =  new System.Collections.Generic.List<UnityEngine.Vector2>(); var uv4 =  new System.Collections.Generic.List<UnityEngine.Vector2>();
    foreach (var e in valid)
    { var m = e.SourceMesh;
      if (m.uv2 != null && m.uv2.Length > 0) uv2.AddRange(m.uv2); else uv2.AddRange(new UnityEngine.Vector2[m.vertexCount]);
      if (m.uv3 != null && m.uv3.Length > 0) uv3.AddRange(m.uv3); else uv3.AddRange(new UnityEngine.Vector2[m.vertexCount]);
      if (m.uv4 != null && m.uv4.Length > 0) uv4.AddRange(m.uv4); else uv4.AddRange(new UnityEngine.Vector2[m.vertexCount]); }
    if (uv2.Count == cm.vertexCount) cm.uv2 = uv2.ToArray();
    if (uv3.Count == cm.vertexCount) cm.uv3 = uv3.ToArray();
    if (uv4.Count == cm.vertexCount) cm.uv4 = uv4.ToArray(); }
    /* Merge blend shapes */
    var bsMap = new System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<(int vOff,int vc,UnityEngine.Mesh src,int bsIdx,UnityEngine.Matrix4x4 x)>>();
    for (int i = 0; i < valid.Count; i++)
    { var src = valid[i].SourceMesh; if (src.blendShapeCount == 0) continue;
    int vOff = vr[i].SV; int vc = src.vertexCount; var x = xforms[i];
    for (int bs = 0; bs < src.blendShapeCount; bs++)
    { var n = src.GetBlendShapeName(bs);
      if (!bsMap.ContainsKey(n)) bsMap[n] =  new System.Collections.Generic.List<(int,int,UnityEngine.Mesh,int,UnityEngine.Matrix4x4)>();
      bsMap[n].Add((vOff,vc,src,bs,x)); } }
    foreach (var kv in bsMap)
    { var dV = new UnityEngine.Vector3[cm.vertexCount]; var dN = new UnityEngine.Vector3[cm.vertexCount]; var dT = new UnityEngine.Vector3[cm.vertexCount];
    float weight = 0;
    foreach (var (vOff,vc,src,bsIdx,x) in kv.Value)
    { var fc = src.GetBlendShapeFrameCount(bsIdx); var r = x.rotation; var s = x.lossyScale;
      for (int f = 0; f < fc; f++)
      { weight = src.GetBlendShapeFrameWeight(bsIdx,f);
      var sV = new UnityEngine.Vector3[vc]; var sN = new UnityEngine.Vector3[vc]; var sT = new UnityEngine.Vector3[vc];
      src.GetBlendShapeFrameVertices(bsIdx,f,sV,sN,sT);
      for (int v = 0; v < vc; v++)
      { dV[vOff + v] += r * UnityEngine.Vector3.Scale(sV[v],s);
        dN[vOff + v] += r * sN[v];
        dT[vOff + v] += r * sT[v]; } } }
    cm.AddBlendShapeFrame(kv.Key,weight,dV,dN,dT); }
    var bones = BuilderCore.SetupBones(avatarRoot);
    return new MergeResult { Success = true,MergedMesh = cm,Materials = finalMats.ToArray(),GeneratedBones = bones }; }
  }
}
}
#endif
