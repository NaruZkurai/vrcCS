namespace NZK
{
public static partial class Core {
public static partial class MergeCore
  {
#if UNITY_EDITOR
  public static UnityEngine.Mesh SaveAsset(UnityEngine.Mesh mesh,System.String path,System.String name)
  { if (mesh == null || System.String.IsNullOrEmpty(path)) return null;
    var dir = System.IO.Path.GetDirectoryName(path);
    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
    mesh.name = name;
    if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path) != null) UnityEditor.AssetDatabase.DeleteAsset(path);
    UnityEditor.AssetDatabase.CreateAsset(mesh,path);
    UnityEditor.EditorUtility.SetDirty(mesh);
    UnityEditor.AssetDatabase.SaveAssets();
    return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path); }
  public static void ApplySMR(UnityEngine.GameObject target,UnityEngine.Mesh mesh,UnityEngine.Material[]  mats,UnityEngine.Transform root,System.Collections.Generic.List<UnityEngine.Transform> bones)
  { if (target == null || mesh == null) return;
    var smr = target.GetComponent<UnityEngine.SkinnedMeshRenderer>();
    if (smr == null) smr = target.AddComponent<UnityEngine.SkinnedMeshRenderer>();
    smr.sharedMesh = mesh; smr.sharedMaterials = mats ?? new UnityEngine.Material[0];
    smr.rootBone = root; smr.bones = bones != null ? bones.ToArray() : new UnityEngine.Transform[0]; }
#endif
    public static UnityEngine.Mesh GetMesh(UnityEngine.GameObject obj)
    {
      if (obj == null) return null;
      var smr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr != null) return smr.sharedMesh;
      var mf = obj.GetComponent<UnityEngine.MeshFilter>();
      return mf != null ? mf.sharedMesh : null;
    }
    public static UnityEngine.Material[] GetMats(UnityEngine.GameObject obj)
    {
      if (obj == null) return new UnityEngine.Material[0];
      var smr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr != null) return smr.sharedMaterials;
      var mr = obj.GetComponent<UnityEngine.MeshRenderer>();
      return mr != null ? mr.sharedMaterials : new UnityEngine.Material[0];
    }
    static UnityEngine.Material[] CombineMats(System.Collections.Generic.List<MeshEntry> entries)
    {
      var r = new System.Collections.Generic.List<UnityEngine.Material>();
      foreach (var e in entries) if (e.Materials != null)
        foreach (var m in e.Materials) if (m != null) r.Add(m);
      return r.Count > 0 ? r.ToArray() : new UnityEngine.Material[] { new UnityEngine.Material(UnityEngine.Shader.Find("Standard")) };
    }
    static System.Collections.Generic.List<UnityEngine.Transform> SetupBones(UnityEngine.Mesh mesh, System.Collections.Generic.List<MeshEntry> entries, System.Collections.Generic.List<UnityEngine.CombineInstance> cis)
    { /* Find armature root from the first entry's object */
      UnityEngine.Transform root = null;
      foreach (var e in entries)
      { if (e.SourceObject != null) { root = e.SourceObject.transform.root; break; } }
      if (root == null) return new System.Collections.Generic.List<UnityEngine.Transform>();
      var arm = root.Find("Armature");
      var armBones = new System.Collections.Generic.List<UnityEngine.Transform>();
      var nameMap = new System.Collections.Generic.Dictionary<System.String, int>();
      if (arm != null) { AddBones(arm, armBones, nameMap); }
      if (armBones.Count == 0) { armBones.Add(root); nameMap[root.name] = 0; }
      /* Build a lookup from mesh instance → entry for bone data */
      var meshToEntry = new System.Collections.Generic.Dictionary<UnityEngine.Mesh, MeshEntry>();
      foreach (var e in entries)
        if (e.SourceMesh != null && !meshToEntry.ContainsKey(e.SourceMesh))
          meshToEntry[e.SourceMesh] = e;
      var bw = new UnityEngine.BoneWeight[mesh.vertexCount];
      int vOff = 0;
      foreach (var ci in cis)
      {
        var m = ci.mesh; if (m == null || m.vertexCount == 0) { vOff += (m != null ? m.vertexCount : 0); continue; }
        int vc = m.vertexCount;
        var e = meshToEntry.TryGetValue(m, out var entry) ? entry : null;
        if (e == null) { vOff += vc; continue; }
        var src = e.SourceMesh;
        var smr = e.SourceObject?.GetComponent<UnityEngine.SkinnedMeshRenderer>();
        var srcBones = smr != null ? smr.bones : new UnityEngine.Transform[0];
        var srcBW = src.boneWeights;
        if (srcBW == null || srcBW.Length == 0)
        {
          var def = new UnityEngine.BoneWeight { boneIndex0 = 0, weight0 = 1f };
          for (int v = 0; v < vc && vOff + v < mesh.vertexCount; v++) bw[vOff + v] = def;
        }
        else for (int v = 0; v < vc && vOff + v < mesh.vertexCount; v++)
        {
          int sv = v % src.vertexCount;
          var w = sv < srcBW.Length ? srcBW[sv] : new UnityEngine.BoneWeight { boneIndex0 = 0, weight0 = 1f };
          bw[vOff + v] = new UnityEngine.BoneWeight
          { boneIndex0 = Remap(w.boneIndex0, srcBones, nameMap, armBones), weight0 = w.weight0, boneIndex1 = Remap(w.boneIndex1, srcBones, nameMap, armBones), weight1 = w.weight1, boneIndex2 = Remap(w.boneIndex2, srcBones, nameMap, armBones), weight2 = w.weight2, boneIndex3 = Remap(w.boneIndex3, srcBones, nameMap, armBones), weight3 = w.weight3 };
        }
        vOff += vc;
      }
      mesh.boneWeights = bw;
      var bindposes = new UnityEngine.Matrix4x4[armBones.Count];
      for (int i = 0; i < armBones.Count; i++) bindposes[i] = armBones[i].worldToLocalMatrix * root.localToWorldMatrix;
      mesh.bindposes = bindposes;
      return armBones;
    }
    static void AddBones(UnityEngine.Transform t, System.Collections.Generic.List<UnityEngine.Transform> list, System.Collections.Generic.Dictionary<System.String, int> map)
    {
      if (!map.ContainsKey(t.name)) { map[t.name] = list.Count; list.Add(t); }
      for (int i = 0; i < t.childCount; i++) AddBones(t.GetChild(i), list, map);
    }
    static int Remap(int srcIdx, UnityEngine.Transform[] srcBones, System.Collections.Generic.Dictionary<System.String, int> nameMap, System.Collections.Generic.List<UnityEngine.Transform> armBones)
    {
      if (srcIdx < 0 || srcIdx >= srcBones.Length || srcBones[srcIdx] == null) return 0;
      return nameMap.TryGetValue(srcBones[srcIdx].name, out int idx) ? idx : 0;
    }
    public static MergeResult Merge(System.Collections.Generic.List<MeshEntry> entries, UnityEngine.Material defaultMat = null)
    {
      if (entries == null || entries.Count == 0)
        return new MergeResult { Success = false, ErrorMessage = "No entries" };
      var valid = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(entries, e => e != null && e.SourceMesh != null));
      if (valid.Count == 0) return new MergeResult { Success = false, ErrorMessage = "No valid meshes" };
      var finalMats = new System.Collections.Generic.List<UnityEngine.Material>();
      /* ── Build combine instances (one per submesh for material separation) ── */
      var allGroupedCis = new System.Collections.Generic.List<UnityEngine.CombineInstance>();
      /* Track source mesh and UnityEngine.Transform for each CI for blend shape copying */
      var ciSrcMesh = new System.Collections.Generic.List<UnityEngine.Mesh>();
      var ciXform = new System.Collections.Generic.List<UnityEngine.Matrix4x4>();
      foreach (var e in valid)
      {
        var m = e.SourceMesh; if (m == null || m.vertexCount == 0) continue;
        var x = e.SourceObject != null ? e.SourceObject.transform.localToWorldMatrix : UnityEngine.Matrix4x4.identity;
        var mats = e.Materials;
        if (mats == null || mats.Length == 0) mats = MergeCore.GetMats(e.SourceObject);
        for (int s = 0; s < m.subMeshCount; s++)
        {
          var mat = (s < mats.Length && mats[s] != null) ? mats[s] : null;
          finalMats.Add(mat);
          allGroupedCis.Add(new UnityEngine.CombineInstance { mesh = m, subMeshIndex = s, transform = x });
          ciSrcMesh.Add(m); ciXform.Add(x);
        }
      }
      if (allGroupedCis.Count == 0)
        return new MergeResult { Success = false, ErrorMessage = "No submeshes to merge" };
      /* ── Incremental: merge geometry + accumulate BS one CI at a time ── */
      var cm = new UnityEngine.Mesh { name = "CombinedMesh" };
      var bsAccum = new System.Collections.Generic.Dictionary<System.String, (UnityEngine.Vector3[] dV, UnityEngine.Vector3[] dN, UnityEngine.Vector3[] dT, float w)>();
      int totalVc = 0;
      for (int ciIdx = 0; ciIdx < allGroupedCis.Count; ciIdx++)
      {
        var ci = allGroupedCis[ciIdx];
        var srcMesh = ciSrcMesh[ciIdx];
        var xf = ciXform[ciIdx];
        /* Build standalone mesh from this one CI */
        var temp = new UnityEngine.Mesh();
        if (ci.mesh.vertexCount > 65535) temp.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        temp.CombineMeshes(new[] { ci }, false, true);
        int vc = temp.vertexCount;
        /* Merge into cm */
        if (ciIdx == 0)
        { cm = temp; }
        else
        {
          int prevVc = cm.vertexCount;
          var combined = new UnityEngine.Mesh();
          if (prevVc + vc > 65535) combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
          combined.CombineMeshes(new[] {
      new UnityEngine.CombineInstance { mesh = cm,subMeshIndex = 0,transform = UnityEngine.Matrix4x4.identity },new UnityEngine.CombineInstance { mesh = temp,subMeshIndex = 0,transform = UnityEngine.Matrix4x4.identity }
      }, false, true);
          cm = combined;
        }
        /* ── Accumulate blend shapes at offset totalVc ── */
        if (srcMesh.blendShapeCount > 0)
        {
          var r = xf.rotation; var sc = xf.lossyScale; int svc = srcMesh.vertexCount;
          for (int bs = 0; bs < srcMesh.blendShapeCount; bs++)
          {
            var n = srcMesh.GetBlendShapeName(bs);
            int fc = srcMesh.GetBlendShapeFrameCount(bs);
            for (int f = 0; f < fc; f++)
            {
              float w = srcMesh.GetBlendShapeFrameWeight(bs, f);
              if (!bsAccum.ContainsKey(n))
                bsAccum[n] = (new UnityEngine.Vector3[cm.vertexCount], new UnityEngine.Vector3[cm.vertexCount], new UnityEngine.Vector3[cm.vertexCount], w);
              else
              {
                var (dV, dN, dT, _) = bsAccum[n];
                if (dV.Length < cm.vertexCount)
                {
                  System.Array.Resize(ref dV, cm.vertexCount);
                  System.Array.Resize(ref dN, cm.vertexCount);
                  System.Array.Resize(ref dT, cm.vertexCount);
                  bsAccum[n] = (dV, dN, dT, bsAccum[n].w);
                }
              }
              var (accV, accN, accT, _) = bsAccum[n];
              var sV = new UnityEngine.Vector3[svc]; var sN = new UnityEngine.Vector3[svc]; var sT = new UnityEngine.Vector3[svc];
              srcMesh.GetBlendShapeFrameVertices(bs, f, sV, sN, sT);
              for (int v = 0; v < svc && totalVc + v < cm.vertexCount; v++)
              {
                accV[totalVc + v] += r * UnityEngine.Vector3.Scale(sV[v], sc);
                accN[totalVc + v] += r * sN[v];
                accT[totalVc + v] += r * sT[v];
              }
            }
          }
        }
        totalVc += vc;
      }
      /* ── Apply accumulated blend shapes to final cm ── */
      int cmVc = cm.vertexCount;
      foreach (var kv in bsAccum)
      {
        var (dV, dN, dT, w) = kv.Value;
        if (dV.Length != cmVc)
        {
          var ndV = new UnityEngine.Vector3[cmVc]; var ndN = new UnityEngine.Vector3[cmVc]; var ndT = new UnityEngine.Vector3[cmVc];
          int copy = dV.Length < cmVc ? dV.Length : cmVc;
          System.Array.Copy(dV, ndV, copy); System.Array.Copy(dN, ndN, copy); System.Array.Copy(dT, ndT, copy);
          dV = ndV; dN = ndN; dT = ndT;
        }
        cm.AddBlendShapeFrame(kv.Key, w, dV, dN, dT);
      }
      /* ── Copy UV channels ── */
      {
        var uv2 = new System.Collections.Generic.List<UnityEngine.Vector2>(); var uv3 = new System.Collections.Generic.List<UnityEngine.Vector2>(); var uv4 = new System.Collections.Generic.List<UnityEngine.Vector2>();
        for (int ci = 0; ci < allGroupedCis.Count; ci++)
        {
          var m = allGroupedCis[ci].mesh; if (m == null) continue;
          int vc = m.vertexCount;
          var u2 = (m.uv2 != null && m.uv2.Length > 0) ? m.uv2 : new UnityEngine.Vector2[vc];
          var u3 = (m.uv3 != null && m.uv3.Length > 0) ? m.uv3 : new UnityEngine.Vector2[vc];
          var u4 = (m.uv4 != null && m.uv4.Length > 0) ? m.uv4 : new UnityEngine.Vector2[vc];
          uv2.AddRange(u2); uv3.AddRange(u3); uv4.AddRange(u4);
        }
        if (uv2.Count == cm.vertexCount) cm.uv2 = uv2.ToArray();
        if (uv3.Count == cm.vertexCount) cm.uv3 = uv3.ToArray();
        if (uv4.Count == cm.vertexCount) cm.uv4 = uv4.ToArray();
      }
      return new MergeResult { Success = true, MergedMesh = cm, Materials = finalMats.ToArray(), GeneratedBones = new System.Collections.Generic.List<UnityEngine.Transform>() };
    }
#if UNITY_EDITOR
  /* ================================================================
   *  UnityEditor.Editor-only mesh baking orchestration
   *  Moved into runtime file so MergeCore partial class isn't split
   *  across assemblies (UnityEditor.Editor-folder = UnityEditor.Editor assembly = invisible
   *  from root-level files).
   * ================================================================ */
  public static void BakeMesh(C_AviGenerator hb)
  { if (hb.NZKC_GO_AviRoot == null) { UnityEngine.Debug.LogWarning("[HB] No UnityEngine.Avatar root."); return; }
    /* ── Reset stale shared materials ────────────────────────────── */
    foreach (UnityEngine.Transform c in hb.transform)
    { var rootHb = c.GetComponent<C_AviGenerator>();
    if (rootHb == null || rootHb.mode != E_AviGeneratorMode.MeshGeneratorRoot) continue;
    rootHb.sharedMaterials =  new System.Collections.Generic.List<UnityEngine.Material>();
    foreach (UnityEngine.Transform child in rootHb.transform)
    { var childHb = child.GetComponent<C_AviGenerator>();
      if (childHb != null && childHb.mode == E_AviGeneratorMode.MeshGenerator)
      childHb.sharedMaterials =  new System.Collections.Generic.List<UnityEngine.Material>(); }
    }
    var all =  new System.Collections.Generic.List<C_AviGenerator>(); hb.Ac(all);
    int idx = 0;
    foreach (var e in System.Linq.Enumerable.Where(all, e => (e.meshGenSources?.Count ?? 0) > 0))
    { var entries = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(e.meshGenSources, s => s != null), s => new MeshEntry
    { SourceObject = s,SourceMesh = MergeCore.GetMesh(s),Materials = MergeCore.GetMats(s),UseUniqueMaterial = e.useUniqueMaterialSlots }), m => m.SourceMesh != null));
    if (entries.Count == 0) { UnityEngine.Debug.LogWarning("[HB] Entry " + e.EntryDisplayName + " has no valid meshes."); continue; }
    var result = MergeCore.Merge(entries);
    if (!result.Success) { UnityEngine.Debug.LogError("[HB] Merge failed for " + e.EntryDisplayName + ": " + result.ErrorMessage); continue; }
    var arm = hb.NZKC_GO_AviRoot.transform.Find("Armature");
    if (arm == null) { var a = new UnityEngine.GameObject("Armature"); a.transform.SetParent(hb.NZKC_GO_AviRoot.transform,false); arm = a.transform; }
    var path = MergeCore.SaveAsset(result.MergedMesh,NZKPaths.AvatarPath(hb.avatarRootName,NZKPaths.Meshes) + "/" + e.EntryDisplayName + ".asset",e.EntryDisplayName);
    var existing = hb.NZKC_GO_AviRoot.transform.Find(e.EntryDisplayName);
    var go = existing != null ? existing.gameObject : new UnityEngine.GameObject(e.EntryDisplayName);
    go.transform.SetParent(hb.NZKC_GO_AviRoot.transform,false);
    var avatarArmBones =  new System.Collections.Generic.List<UnityEngine.Transform>();
    if (arm != null) { AddBones(arm,avatarArmBones,new System.Collections.Generic.Dictionary<System.String,int>()); }
    MergeCore.ApplySMR(go,path,result.Materials,arm,avatarArmBones);
    idx++; }
    UnityEngine.Debug.Log("[HB] Baked " + idx + " meshes to " + hb.avatarRootName);
    if (idx > 0)
    { foreach (UnityEngine.Transform c in hb.transform)
    { var rootHb = c.GetComponent<C_AviGenerator>();
      if (rootHb == null || rootHb.mode != E_AviGeneratorMode.MeshGeneratorRoot) continue;
      var bakedMats =  new System.Collections.Generic.List<UnityEngine.Material>();
      foreach (UnityEngine.Transform child in rootHb.transform)
      { var childHb = child.GetComponent<C_AviGenerator>();
      if (childHb == null || childHb.mode != E_AviGeneratorMode.MeshGenerator) continue;
      var smrObj = hb.NZKC_GO_AviRoot.transform.Find(childHb.EntryDisplayName);
      if (smrObj == null) continue;
      var smr = smrObj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr == null || smr.sharedMesh == null) continue;
      foreach (var m in smr.sharedMaterials)
        if (m != null && !bakedMats.Contains(m)) bakedMats.Add(m);
      childHb.sharedMaterials =  new System.Collections.Generic.List<UnityEngine.Material>(); }
      /* Don't update rootHb.sharedMaterials — avoids SyncSharedMats corruption */ } }
    foreach (UnityEngine.Transform c in hb.transform)
    { var h = c.GetComponent<C_AviGenerator>();
    if (h != null && h.mode == E_AviGeneratorMode.MeshSplitter)
    { var splitter = h.GetComponent<MeshSplitterDefinition>();
      if (splitter != null && splitter.sourceSMR != null)
      { var result = MeshSplitterOps.Split(splitter,hb.NZKC_GO_AviRoot.transform);
      if (result.success)
        MeshSplitterOps.SaveAndApply(result,splitter,hb.NZKC_GO_AviRoot.transform);
      else
        UnityEngine.Debug.LogError("[HB] UnityEngine.Mesh split failed: " + result.errorMessage); } } }
    if (hb.NZKC_GO_AviRoot != null)
    { var vrcad = hb.NZKC_GO_AviRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
    if (vrcad != null && vrcad.VisemeSkinnedMesh == null)
    { var smrs = hb.NZKC_GO_AviRoot.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>();
      UnityEngine.SkinnedMeshRenderer pick = null;
      foreach (var s in smrs)
      { var n = s.name.ToLower();
      if (n.Contains("face")) { pick = s; break; }
      if (pick == null && n.Contains("body")) pick = s;
      if (pick == null && s.sharedMesh != null && s.sharedMesh.blendShapeCount > 0) pick = s; }
      if (pick != null && pick.sharedMesh != null)
      { VRCAD.Set.VisemeMesh(vrcad,pick);
      var vNames = AVController.VisemeNames;
      var bs =  new System.Collections.Generic.List<System.String>();
      for (int i = 0; i < pick.sharedMesh.blendShapeCount; i++)
        bs.Add(pick.sharedMesh.GetBlendShapeName(i).ToLower());
      var mapped = new System.String[15];
      for (int i = 0; i < 15; i++)
      { var found = false;
        foreach (var b in bs)
        { var parts = b.Split('_'); var last = parts.Length > 0 ? parts[parts.Length - 1] : "";
        if (last == vNames[i]) { mapped[i] = b; found = true; break; } }
        if (!found) mapped[i] = "-none-"; }
      VRCAD.Set.VisemeBlendShapes(vrcad,mapped);
      int matched = System.Linq.Enumerable.Count(mapped, s => s != "-none-");
      UnityEngine.Debug.Log("[HB] Auto-assigned face SMR: " + pick.name + " srcBS=" + pick.sharedMesh.blendShapeCount + " matchedVisemes=" + matched + "/15 unmatched=" + (15 - matched)); } } } }
  /* ================================================================
   *  UnityEngine.Material link baking
   * ================================================================ */
  public static void BakeMaterialLinks(C_AviGenerator hb)
  { var links = UnityEngine.Object.FindObjectsOfType<MaterialLink>();
    if (links == null || links.Length == 0) return;
    UnityEngine.Debug.Log("[HB] Processing " + links.Length + " MaterialLink component(s)...");
    int matCount = 0;
    int propCount = 0;
    foreach (var link in links)
    { if (link.defaultMaterials != null)
    { foreach (var mat in link.defaultMaterials)
      { if (mat == null) continue;
      if (!IsPOIMat(mat)) continue;
      matCount++;
      var allProps = UnityEditor.MaterialEditor.GetMaterialProperties(new[] { mat });
      if (allProps == null) continue;
      foreach (var p in allProps)
      { if (p == null) continue;
        if (mat.shader.name.StartsWith("Hidden/Locked/")) { propCount++; }
        else { System.String tag = mat.GetTag(p.name + "Animated",false,"");
        if (tag == "1" || tag == "2") propCount++; } } } }
    if (link.targetObjects != null)
    { foreach (var obj in link.targetObjects)
      { if (obj == null) continue;
      var renderers = obj.GetComponentsInChildren<UnityEngine.Renderer>(true);
      foreach (var renderer in renderers)
      { var mats = renderer.sharedMaterials;
        if (mats == null) continue;
        for (int s = 0; s < mats.Length; s++)
        { var mat = mats[s];
        if (mat == null) continue;
        if (!IsPOIMat(mat)) continue;
        matCount++;
        var allProps = UnityEditor.MaterialEditor.GetMaterialProperties(new[] { mat });
        if (allProps == null) continue;
        foreach (var p in allProps)
        { if (p == null) continue;
          if (mat.shader.name.StartsWith("Hidden/Locked/"))
          { propCount++; }
          else
          { System.String tag = mat.GetTag(p.name + "Animated",false,"");
          if (tag == "1" || tag == "2") propCount++; } } } } } } }
    UnityEngine.Debug.Log("[HB] MaterialLink bake done: " + matCount + " POI materials," + propCount + " animated properties verified."); }
  static System.Boolean IsPOIMat(UnityEngine.Material mat)
  { if (mat == null || mat.shader == null) return false;
    System.String n = mat.shader.name;
    return n.Contains(".poiyomi") || n.Contains("Poiyomi"); }
#endif
  }
}
}
