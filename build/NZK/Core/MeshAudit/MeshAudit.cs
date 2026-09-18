#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshAudit {
    /* ── REPORT ──────────────────────────────────────────────────────── */
    /** <summary>What one audit found.  Counted so a caller can decide whether
     *  anything was wrong without parsing the log.</summary> */
    
    /* ── ENTRY ───────────────────────────────────────────────────────── */
    /** <summary>Audit the root of each selected object.</summary> */
    public static AuditReport RunSelection()
    { return Run(UnityEditor.Selection.gameObjects); }
    /** <summary>Audit every renderer under the root of each given object.
     *
     *  Rooted like every other tool in this toolkit, for the same reason: the
     *  thing being uploaded is the avatar, and a user selects a mesh inside it.
     *  Auditing only the selection would miss the mesh that is actually
     *  broken. </summary> */
    public static AuditReport Run(UnityEngine.GameObject[] objs)
    {
      AuditReport r = new AuditReport();
      if (NZK.B.mpty.t(objs)) return r;
      System.Collections.Generic.HashSet<UnityEngine.GameObject> roots =
        new System.Collections.Generic.HashSet<UnityEngine.GameObject>();
      foreach (UnityEngine.GameObject go in objs)
      { if (go == null) continue;
        UnityEngine.Transform t = go.transform.root;
        roots.Add(t != null ? t.gameObject : go); }
      foreach (UnityEngine.GameObject root in roots)
      { if (root == null) continue;
        NZK.E.C.d(74,root.name);
        var smrs = root.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
        if (NZK.B.mpty.t(smrs))
        { NZK.E.C.w(75,root.name + " - no SkinnedMeshRenderer under this root at all");
          continue; }
        foreach (UnityEngine.SkinnedMeshRenderer smr in smrs)
        { if (smr == null) continue;
          AuditRenderer(smr,ref r); }
        /* Static renderers too: an avatar's accessories are often MeshRenderers,
           and "invisible" can mean the static half is what disappeared. */
        var mrs = root.GetComponentsInChildren<UnityEngine.MeshRenderer>(true);
        foreach (UnityEngine.MeshRenderer mr in mrs)
        { if (mr == null) continue;
          AuditStatic(mr,ref r); }
        /* The skeleton, checked separately: bones present is exactly what the
           report says, so a bone that is PRESENT but driving the mesh to zero
           is the case worth naming. */
        AuditBones(root,ref r); }
      Summarise(r);
      return r; }
    /* ── ONE SKINNED RENDERER ────────────────────────────────────────── */
    /** <summary>Audit one skinned renderer and fold the findings into `r`.</summary> */
    public static void AuditRenderer(UnityEngine.SkinnedMeshRenderer smr,ref AuditReport r)
    { if (smr == null) return;
      r.Renderers++;
      if (!smr.enabled || !smr.gameObject.activeInHierarchy)
      { NZK.E.C.w(76,smr.name + " - renderer or GameObject is DISABLED");
        return; }
      r.Enabled++;
      UnityEngine.Mesh m = smr.sharedMesh;
      if (m == null)
      { NZK.E.C.w(77,smr.name + " - sharedMesh is NULL");
        return; }
      /* Geometry presence.  A mesh with no triangles draws nothing however
         healthy the rest of it looks. */
      System.Int32 verts = m.vertexCount;
      System.Int32 tris = m.triangles != null ? m.triangles.Length / 3 : 0;
      if (verts == 0 || tris == 0)
      { r.EmptyMeshes++;
        NZK.E.C.w(78,smr.name + " - EMPTY: verts=" + verts + " tris=" + tris); }
      /* Bounds.  zero size or a NaN component both make the renderer fail its
         cull test on the client while the editor still draws it. */
      UnityEngine.Bounds b = smr.localBounds;
      System.Single size = b.size.magnitude;
      if (System.Single.IsNaN(size) || System.Single.IsInfinity(size) ||
          System.Single.IsNaN(b.center.x) || System.Single.IsNaN(b.center.y) || System.Single.IsNaN(b.center.z))
      { r.NaNBounds++;
        NZK.E.C.w(79,smr.name + " - BOUNDS ARE NaN/INF: center=" + b.center + " size=" + b.size); }
      else if (size <= 0f)
      { r.ZeroBounds++;
        NZK.E.C.w(80,smr.name + " - BOUNDS ARE ZERO SIZE: center=" + b.center); }
      /* Materials.  Present geometry with no shader renders as nothing, and a
         null material is invisible rather than pink when the shader is missing
         from the build. */
      var mats = smr.sharedMaterials;
      System.Int32 missing = 0;
      if (NZK.B.mpty.t(mats)) missing = 1;
      else foreach (UnityEngine.Material mat in mats)
      { if (mat == null || mat.shader == null) missing++; }
      if (missing > 0)
      { r.MissingMaterials++;
        NZK.E.C.w(81,smr.name + " - " + missing + " null material slot(s)/shader(s)"); }
      /* Weights.  Needs a readable mesh; every imported mesh is readable by
         default, so the branch below is for meshes deliberately marked
         non-readable to save memory, where there is nothing to inspect. */
      if (!m.isReadable)
      { NZK.E.C.w(82,smr.name + " - mesh is NOT READABLE, weights unchecked");
        return; }
      r.Readable++;
      UnityEngine.BoneWeight[] w = m.boneWeights;
      System.Int32 unweighted = 0;
      System.Int32 over = 0;
      System.Int32 nanv = 0;
      if (!NZK.B.mpty.t(w))
      { for (System.Int32 i = 0; i < w.Length; i++)
        { System.Single sum = w[i].weight0 + w[i].weight1 + w[i].weight2 + w[i].weight3;
          if (System.Single.IsNaN(sum))
          { nanv++; continue; }
          if (sum <= 0f) unweighted++;
          System.Int32 c = 0;
          for (System.Int32 s = 0; s < 4; s++)
          { System.Single sw = s == 0 ? w[i].weight0 : s == 1 ? w[i].weight1 : s == 2 ? w[i].weight2 : w[i].weight3;
            if (sw > 0f) c++; }
          if (c > 4) over++; } }
      /* Vertex positions.  A single NaN poisons the mesh's computed bounds. */
      UnityEngine.Vector3[] vs = m.vertices;
      System.Int32 nanpos = 0;
      if (!NZK.B.mpty.t(vs))
      { for (System.Int32 i = 0; i < vs.Length; i++)
        { if (System.Single.IsNaN(vs[i].x) || System.Single.IsNaN(vs[i].y) || System.Single.IsNaN(vs[i].z)) nanpos++; } }
      if (nanpos > 0 || nanv > 0)
      { r.NaNVertices++;
        NZK.E.C.w(83,smr.name + " - NaN DATA: NaN positions=" + nanpos + "/" + vs.Length +
                     " NaN weights=" + nanv + "/" + w.Length +
                     " - NaN bounds make the client cull the mesh entirely"); }
      if (unweighted > 0)
      { r.Unweighted++;
        NZK.E.C.w(84,smr.name + " - " + unweighted + "/" + w.Length +
                     " vertices have NO WEIGHT AT ALL (collapse to origin)"); }
      if (over > 0)
      { r.OverInfluenced++;
        NZK.E.C.w(85,smr.name + " - " + over + " vertices carry more than 4 influences"); }
      /* BONES ARRAY CONTENTS.
         Counting smr.bones.Length proves nothing: an array of 336 NULLS has a
         length of 336.  That is the exact shape of an "invisible but the bones
         are all there" bug - the hierarchy is intact, the renderer lists 336
         slots, and not one of them points at a bone, so the mesh has no
         transform to skin against and vanishes.  It is invisible to an audit
         that stops at the count, and invisible in the inspector unless the
         user expands a 336-entry list and scrolls. */
      UnityEngine.Transform[] bn = smr.bones;
      System.Int32 nullBones = 0;
      System.Int32 offRigBones = 0;
      if (!NZK.B.mpty.t(bn))
      { UnityEngine.Transform rootT = smr.transform.root;
        for (System.Int32 i = 0; i < bn.Length; i++)
        { if (bn[i] == null) { nullBones++; continue; }
          if (rootT != null) { UnityEngine.Transform rt = bn[i].root; if (rt != null && rt != rootT) offRigBones++; } } }
      if (nullBones > 0)
      { r.NullBones++;
        NZK.E.C.w(91,smr.name + " - bones[" + bn.Length + "] contains " + nullBones +
                     " NULL entries - the renderer has no skeleton to skin against, it draws NOTHING");
        if (nullBones == bn.Length)
          NZK.E.C.w(92,smr.name + " - ALL " + bn.Length + " bone slots are NULL (every entry unassigned)"); }
      if (offRigBones > 0)
      { r.OffRigBones++;
        NZK.E.C.w(93,smr.name + " - " + offRigBones + " bone(s) belong to a DIFFERENT hierarchy root"); }
      /* The bind poses must be the same LENGTH as the bone array.  A shorter
         array is silently indexed out of range by the skinning path, which
         again draws nothing - and it is the condition the removed
         RebuildBindPoses was meant to address, reported here read-only. */
      UnityEngine.Matrix4x4[] bp = m.bindposes;
      System.Int32 bpc = NZK.B.mpty.t(bp) ? 0 : bp.Length;
      System.Int32 bn2 = NZK.B.mpty.t(bn) ? 0 : bn.Length;
      if (bn2 > 0 && bpc != bn2)
      { r.BindPoseMismatch++;
        NZK.E.C.w(94,smr.name + " - bindPoses=" + bpc + " but bones=" + bn2 +
                     " - skinning indexes out of range and draws nothing"); }
      NZK.E.C.d(86,smr.name + " verts=" + verts + " tris=" + tris +
                    " bounds=" + b.size + " bones=" + bn2 +
                    " nullBones=" + nullBones +
                    " root=" + (smr.rootBone != null ? smr.rootBone.name : "NONE") +
                    " bindPoses=" + bpc +
                    " mats=" + (mats != null ? mats.Length : 0) +
                    " bs=" + m.blendShapeCount +
                    " nanPos=" + nanpos + " unweighted=" + unweighted); }
    /* ── ONE STATIC RENDERER ─────────────────────────────────────────── */
    /** <summary>Audit a plain MeshRenderer: geometry and bounds only.
     *
     *  Static renderers are usually accessories rather than the body, so this is
     *  brief - but "the avatar is invisible" has to include the case where the
     *  body is fine and everything else is what vanished. </summary> */
    public static void AuditStatic(UnityEngine.MeshRenderer mr,ref AuditReport r)
    { if (mr == null) return;
      r.Renderers++;
      if (!mr.enabled || !mr.gameObject.activeInHierarchy) return;
      r.Enabled++;
      UnityEngine.MeshFilter mf = mr.GetComponent<UnityEngine.MeshFilter>();
      UnityEngine.Mesh m = mf != null ? mf.sharedMesh : null;
      if (m == null)
      { NZK.E.C.w(77,mr.name + " - sharedMesh is NULL (MeshRenderer with no mesh)");
        return; }
      System.Single size = mr.localBounds.size.magnitude;
      if (System.Single.IsNaN(size) || size <= 0f)
      { r.NaNBounds++;
        NZK.E.C.w(80,mr.name + " - static renderer bounds are NaN or zero"); } }
    /* ── BONES ───────────────────────────────────────────────────────── */
    /** <summary>Check every transform under the root for a degenerate scale.
     *
     *  "All the bones are there" is the report, and a bone that is present, named
     *  correctly, and SCALED TO ZERO satisfies it while collapsing everything
     *  weighted to it - which is invisible and leaves the hierarchy looking
     *  perfect in the inspector.  A NaN scale does the same and additionally
     *  poisons any bind pose derived from it. </summary> */
    public static void AuditBones(UnityEngine.GameObject root,ref AuditReport r)
    { if (root == null) return;
      var all = root.GetComponentsInChildren<UnityEngine.Transform>(true);
      System.Int32 zero = 0;
      System.Int32 nan = 0;
      System.Int32 bones = 0;
      foreach (UnityEngine.Transform t in all)
      { if (t == null) continue;
        bones++;
        UnityEngine.Vector3 s = t.localScale;
        System.Boolean anyNaN = System.Single.IsNaN(s.x) || System.Single.IsNaN(s.y) || System.Single.IsNaN(s.z);
        if (anyNaN) { nan++;
          NZK.E.C.w(87,Describe(t) + " - NAN SCALE " + s); continue; }
        if (UnityEngine.Mathf.Approximately(s.x,0f) || UnityEngine.Mathf.Approximately(s.y,0f) || UnityEngine.Mathf.Approximately(s.z,0f))
        { zero++;
          NZK.E.C.w(88,Describe(t) + " - ZERO SCALE " + s + " (collapses everything weighted to it)"); } }
      if (zero > 0 || nan > 0) { r.BadScales += zero + nan; }
      NZK.E.C.d(89,"transforms=" + bones + " zeroScale=" + zero + " nanScale=" + nan); }
    /** <summary>Full path of a transform, so a reported bone can be FOUND.
     *
     *  A bare object name is useless on a rig where every second bone shares a
     *  name across limbs; the path is what makes the report actionable.</summary> */
    public static System.String Describe(UnityEngine.Transform t)
    { if (t == null) return "(null)";
      System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
      UnityEngine.Transform p = t.parent;
      System.Int32 depth = 0;
      while (p != null && depth < 8)
      { sb.Insert(0,p.name + "/");
        p = p.parent;
        depth++; }
      return sb.ToString(); }
    /* ── SUMMARY ─────────────────────────────────────────────────────── */
    /** <summary>One verdict-free line naming every count that is non-zero.
     *
     *  Deliberately does NOT say "this is the cause".  The counts are what is
     *  known; which of them explains a given upload is for the reader, because
     *  the same numbers appear on a healthy avatar - a rig legitimately has
     *  zero-weight vertices, and a static accessory legitimately has one
     *  material.  A summary that ranked them would be guessing on the reader's
     *  behalf, which is how the last three theories went wrong.</summary> */
    static void Summarise(AuditReport r)
    { System.Text.StringBuilder sb = new System.Text.StringBuilder();
      sb.Append("renderers=").Append(r.Renderers);
      sb.Append(" enabled=").Append(r.Enabled);
      sb.Append(" readable=").Append(r.Readable);
      if (r.EmptyMeshes > 0) sb.Append(" EMPTY=").Append(r.EmptyMeshes);
      if (r.ZeroBounds > 0) sb.Append(" ZEROBOUNDS=").Append(r.ZeroBounds);
      if (r.NaNBounds > 0) sb.Append(" NANBOUNDS=").Append(r.NaNBounds);
      if (r.NaNVertices > 0) sb.Append(" NANVERTS=").Append(r.NaNVertices);
      if (r.Unweighted > 0) sb.Append(" UNWEIGHTED=").Append(r.Unweighted);
      if (r.OverInfluenced > 0) sb.Append(" OVER4=").Append(r.OverInfluenced);
      if (r.NullBones > 0) sb.Append(" NULLBONES=").Append(r.NullBones);
      if (r.OffRigBones > 0) sb.Append(" OFFRIG=").Append(r.OffRigBones);
      if (r.BindPoseMismatch > 0) sb.Append(" BINDMISMATCH=").Append(r.BindPoseMismatch);
      if (r.BadScales > 0) sb.Append(" BADSCALE=").Append(r.BadScales);
      if (r.MissingMaterials > 0) sb.Append(" NOMAT=").Append(r.MissingMaterials);
      NZK.E.C.d(90,sb.ToString()); }
  
}
}
}
#endif
