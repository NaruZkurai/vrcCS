#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MA {
    /** <summary>A bone created for one visibility group.
     *
     *  `originalBoneIndex` names the bone whose influence was moved; the new
     *  bone inherits its bind pose and its position in the hierarchy, so the
     *  geometry does not move when the influence is reassigned.</summary> */
    
    /** <summary>One visibility group: the vertices it owns, and the bone to drive.
     *
     *  PLURAL BONES, because one group can need more than one.  A vertex carries
     *  at most four influences; if all four are real and none belongs to the
     *  group's first bone, the group has to claim a second bone to reach that
     *  vertex.  The original has the same property - `ComputeNaNPlanForShape`
     *  loops until `remainingVertices` is empty, adding a bone per pass. */
    
    /* ── RESULT ──────────────────────────────────────────────────────── */
    /** <summary>What a nanimation pass did.</summary> */
    
    /* ── ENTRY POINT ─────────────────────────────────────────────────── */
    /** <summary>Apply every group's nanimation to one renderer's mesh, in memory.
     *
     *  Groups are keyed by the source bone index they should hide; the caller
     *  supplies which vertices belong to which group (that is the Blender-side
     *  authoring, and this file deliberately does not care how it was decided).
     *
     *  RetURNS the created bones grouped by name, so the caller can hand them to
     *  the animation side.  Nothing is written to disk.
     *
     *  SHARED MESHES.  If two renderers point at the same mesh, each needs its
     *  own copy once it has its own bones - the bone assignment lives on the
     *  RENDERER, not the mesh, so a shared mesh would otherwise be driven by
     *  whichever renderer ran last.  The caller passes a mesh it owns; this
     *  method replaces `renderer.sharedMesh` and never mutates the input. */
    public static Report Apply(
      UnityEngine.SkinnedMeshRenderer renderer,
      System.Collections.Generic.List<Group> groups)
    { Report report = new Report();
      if (renderer == null) { report.error = "renderer is null"; return report; }
      UnityEngine.Mesh source = renderer.sharedMesh;
      if (source == null) { report.error = "renderer has no mesh"; return report; }
      if (groups == null || groups.Count == 0) { report.error = "no groups given"; return report; }
      report.MeshesInspected = 1;
      /* Only vertices that actually need splitting force a rebuild.  A mesh with
       * no group assignment at all is left alone, which is what keeps a mesh the
       * user did not opt in from being rewritten. */
      System.Boolean anySelection = false;
      for (System.Int32 g = 0; g < groups.Count; g++)
        if (groups[g] != null && groups[g].vertices.Count > 0) { anySelection = true; break; }
      if (!anySelection) { report.MeshesSkipped = 1; return report; }
      System.String err = null;
      System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<UnityEngine.GameObject>> made =
        Rebuild(renderer,source,groups,report,out err);
      if (err != null) { report.error = err; return report; }
      report.MeshesRebuilt = 1;
      return report; }
    /* ── THE SURGERY ─────────────────────────────────────────────────── */
    /** <summary>Rebuild the mesh and create the group bones.
     *
     *  THE ORDER IS THE ALGORITHM.  Each step depends on the previous:
     *    1. classify every vertex by which groups select it  -> hide keys
     *    2. decide, per primitive, whether its vertices can stay shared or must
     *       be duplicated so the primitive can be hidden independently
     *    3. build the new mesh with the duplicated vertices and remapped indices
     *    4. append the bone weights for the duplicated vertices
     *    5. allocate bones, move influences onto them, append bind poses
     *    6. build the bone GameObjects behind a buffer object per parent
     *
     *  Doing 5 before 3 breaks the bind poses (they are indexed by the OLD
     *  vertex layout); doing 6 before 5 leaks bones nothing references. */
    static System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<UnityEngine.GameObject>> Rebuild(
      UnityEngine.SkinnedMeshRenderer renderer,
      UnityEngine.Mesh mesh,
      System.Collections.Generic.List<Group> groups,
      Report report,
      out System.String error)
    { error = null;
      System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<UnityEngine.GameObject>> result =
        new System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<UnityEngine.GameObject>>(System.StringComparer.Ordinal);
      if (!mesh.isReadable)
      { error = "mesh '" + mesh.name + "' is not readable; enable Read/Write on the model";
        return result; }
      UnityEngine.Transform[] bones = renderer.bones;
      if (bones == null || bones.Length == 0)
      { error = "renderer '" + renderer.name + "' declares no bones"; return result; }
      /* ── 1. HIDE KEY PER VERTEX ────────────────────────────────────
       *
       * A bitmask over groups is enough and is what the original uses for up to
       * 64 groups (`SmallHideKey`).  We cap at 64 and report rather than
       * silently mis-group beyond that - a wrong hide key produces a garment
       * that hides with the wrong toggle, which is far worse than an error. */
      if (groups.Count > 64)
      { error = groups.Count + " groups exceeds the 64-group limit of this implementation";
        return result; }
      System.Int32 vertCount = mesh.vertexCount;
      System.UInt64[] hideKey = new System.UInt64[vertCount];
      for (System.Int32 gi = 0; gi < groups.Count; gi++)
      { Group g = groups[gi];
        if (g == null) continue;
        System.UInt64 bit = 1UL << gi;
        foreach (System.Int32 v in g.vertices)
          if (v >= 0 && v < vertCount) hideKey[v] |= bit; }
      /* ── 2. VERTEX SPLITTING ───────────────────────────────────────
       *
       * A vertex used by two primitives with DIFFERENT hide keys cannot serve
       * both: whichever key it carries, the other primitive hides wrongly.  Such
       * a vertex is duplicated and each primitive is pointed at its own copy.
       *
       * `newToOrig[newIdx] = origIdx`, and it starts as the identity - the
       * original's layout, with duplicates appended. */
      System.Collections.Generic.List<System.Int32> newToOrig =
        new System.Collections.Generic.List<System.Int32>(vertCount);
      System.Collections.Generic.List<System.UInt64> newHideKey =
        new System.Collections.Generic.List<System.UInt64>(vertCount);
      for (System.Int32 v = 0; v < vertCount; v++) { newToOrig.Add(v); newHideKey.Add(hideKey[v]); }
      System.Int32 subMeshCount = mesh.subMeshCount;
      System.Int32[][] indices = new System.Int32[subMeshCount][];
      for (System.Int32 sm = 0; sm < subMeshCount; sm++)
        indices[sm] = mesh.GetTriangles(sm);
      /* cloneMap[(origVert, hideKey)] -> the duplicate that carries that key. */
      System.Collections.Generic.Dictionary<System.Int64,System.Int32> cloneMap =
        new System.Collections.Generic.Dictionary<System.Int64,System.Int32>();
      for (System.Int32 sm = 0; sm < subMeshCount; sm++)
      { System.Int32[] tri = indices[sm];
        /* Every 3 indices form one primitive.  The primitive's hide key is the
         * AND of its vertices' keys: a primitive is hidden only by a group that
         * selects ALL of its vertices.  Intersecting rather than taking the
         * first vertex's key is what stops a group from dragging in a triangle
         * that merely touches one selected vertex. */
        for (System.Int32 p = 0; p + 2 < tri.Length; p += 3)
        { System.UInt64 key = hideKey[tri[p]] & hideKey[tri[p+1]] & hideKey[tri[p+2]];
          if (key == 0) continue;   /* not selected by any group: leave shared */
          for (System.Int32 k = 0; k < 3; k++)
          { System.Int32 ov = tri[p+k];
            if (newHideKey[ov] == key) continue;   /* already carries this key */
            System.Int64 mapKey = ((System.Int64)ov << 32) | (System.Int64)(System.UInt32)key;
            if (!cloneMap.TryGetValue(mapKey,out System.Int32 dup))
            { dup = newToOrig.Count;
              newToOrig.Add(ov);
              newHideKey.Add(key);
              cloneMap[mapKey] = dup;
              report.VerticesSplit++; }
            tri[p+k] = dup; } } }
      /* ── 3. NEW MESH ──────────────────────────────────────────────── */
      UnityEngine.Mesh rebuilt = new UnityEngine.Mesh();
      rebuilt.name = mesh.name;
      rebuilt.indexFormat = newToOrig.Count > 65535
        ? UnityEngine.Rendering.IndexFormat.UInt32
        : mesh.indexFormat;
      System.Int32[] newToOrigArray = newToOrig.ToArray();
      TransferVertexData(rebuilt,mesh,newToOrigArray);
      rebuilt.bindposes = mesh.bindposes;
      TransferShapes(rebuilt,mesh,newToOrigArray);
      rebuilt.subMeshCount = subMeshCount;
      for (System.Int32 sm = 0; sm < subMeshCount; sm++)
        rebuilt.SetTriangles(indices[sm],sm,false);
      /* ── 4. WEIGHT EXTENSION ───────────────────────────────────────
       *
       * `GetAllBoneWeights` is a flat array plus a per-vertex count; the
       * duplicated vertices need the SOURCE vertex's weights appended in the
       * same encoding before anything can be moved onto a new bone.
       *
       * THESE ARE NativeArray AT THE SOURCE, and Unity's API only accepts
       * NativeArray back on `SetBoneWeights`.  Copying into a managed array
       * would round-trip the whole skinning dataset twice on a 39-mesh avatar
       * for no benefit, so the data is converted ONCE into managed arrays here
       * and converted back once at the write - the algorithm needs indexed
       * read/write, which a managed array gives without the safety-handle
       * ceremony. */
      Unity.Collections.NativeArray<UnityEngine.BoneWeight1> srcWeights = mesh.GetAllBoneWeights();
      Unity.Collections.NativeArray<System.Byte> srcPerVertex = mesh.GetBonesPerVertex();
      UnityEngine.BoneWeight1[] allWeights = new UnityEngine.BoneWeight1[srcWeights.Length];
      srcWeights.CopyTo(allWeights);
      System.Byte[] bonesPerVertex = new System.Byte[srcPerVertex.Length];
      srcPerVertex.CopyTo(bonesPerVertex);
      System.Int32 origCount = vertCount;
      System.Int32 newCount = newToOrig.Count;
      System.Int32 cloneCount = newCount - origCount;
      System.Int32[] firstBone = new System.Int32[origCount];
      { System.Int32 run = 0;
        for (System.Int32 v = 0; v < origCount; v++) { firstBone[v] = run; run += bonesPerVertex[v]; } }
      System.Int32 extra = 0;
      for (System.Int32 c = 0; c < cloneCount; c++)
        extra += bonesPerVertex[newToOrig[origCount + c]];
      UnityEngine.BoneWeight1[] extW = new UnityEngine.BoneWeight1[allWeights.Length + extra];
      System.Array.Copy(allWeights,extW,allWeights.Length);
      System.Byte[] extB = new System.Byte[newCount];
      System.Array.Copy(bonesPerVertex,extB,origCount);
      { System.Int32 w = allWeights.Length;
        for (System.Int32 c = 0; c < cloneCount; c++)
        { System.Int32 ov = newToOrig[origCount + c];
          System.Int32 n = bonesPerVertex[ov];
          extB[origCount + c] = (System.Byte)n;
          System.Int32 baseIdx = firstBone[ov];
          for (System.Int32 b = 0; b < n; b++) extW[w++] = allWeights[baseIdx + b]; } }
      /* Recompute the offsets over the EXTENDED vertex set: every later step
       * indexes weights by new vertex, and using the original offsets here
       * reads the wrong influences for every duplicated vertex. */
      System.Int32[] firstBoneNew = new System.Int32[newCount];
      { System.Int32 run = 0;
        for (System.Int32 v = 0; v < newCount; v++) { firstBoneNew[v] = run; run += extB[v]; } }
      /* ── 5. ALLOCATE BONES AND MOVE INFLUENCES ─────────────────────
       *
       * One bone at a time: take the group's vertices still needing a bone,
       * pick the influence they share most often, and repoint it at a NEW bone.
       * Repeat until the group's vertices are all covered - the loop exists
       * because a vertex with four real influences may need a fourth-candidate
       * bone before one of its slots can be donated. */
      System.Int32 nextBone = mesh.bindposes == null ? 0 : mesh.bindposes.Length;
      for (System.Int32 gi = 0; gi < groups.Count; gi++)
      { Group g = groups[gi];
        if (g == null || g.vertices.Count == 0) continue;
        System.UInt64 bit = 1UL << gi;
        System.Collections.Generic.List<System.Int32> remaining =
          new System.Collections.Generic.List<System.Int32>();
        for (System.Int32 v = 0; v < newCount; v++)
          if ((newHideKey[v] & bit) != 0) remaining.Add(v);
        while (remaining.Count > 0)
        { /* Count which bones the remaining vertices already use, most common
           * first: repointing the most-shared influence covers the most
           * vertices per new bone, which keeps the bone count down. */
          System.Collections.Generic.Dictionary<System.Int32,System.Int32> freq =
            new System.Collections.Generic.Dictionary<System.Int32,System.Int32>();
          for (System.Int32 i = 0; i < remaining.Count; i++)
          { System.Int32 v = remaining[i];
            for (System.Int32 b = 0; b < extB[v]; b++)
            { UnityEngine.BoneWeight1 bw = extW[firstBoneNew[v] + b];
              if (bw.weight == 0f) continue;
              if (bw.boneIndex < 0) continue;
              freq.TryGetValue(bw.boneIndex,out System.Int32 n);
              freq[bw.boneIndex] = n + 1; } }
          if (freq.Count == 0)
          { /* Vertices selected but carrying no influence at all.  They cannot
             * be hidden by a bone, so they are dropped from the group; the
             * alternative is an infinite loop choosing a bone that does not
             * exist. */
            remaining.Clear();
            break; }
          System.Int32 target = -1; System.Int32 best = -1;
          foreach (System.Collections.Generic.KeyValuePair<System.Int32,System.Int32> kv in freq)
            if (kv.Value > best) { best = kv.Value; target = kv.Key; }
          AddedBone added = new AddedBone { originalBoneIndex = target, newBoneIndex = nextBone++ };
          g.bones.Add(added);
          report.BonesCreated++;
          for (System.Int32 i = remaining.Count - 1; i >= 0; i--)
          { System.Int32 v = remaining[i];
            System.Boolean claimed = false;
            for (System.Int32 b = 0; b < extB[v]; b++)
            { System.Int32 at = firstBoneNew[v] + b;
              UnityEngine.BoneWeight1 bw = extW[at];
              if (bw.weight == 0f || bw.boneIndex != target) continue;
              bw.boneIndex = added.newBoneIndex;
              extW[at] = bw;
              claimed = true;
              break; }
            if (claimed) remaining.RemoveAt(i); } } }
      /* ── 5b. BIND POSES ───────────────────────────────────────────
       *
       * A new bone needs its own bind pose or the vertices it just claimed
       * collapse.  It INHERITS the original bone's pose, which is what keeps the
       * geometry exactly where it was - the influence moved, the transform did
       * not.  Resizing to `nextBone` also guarantees the array is never longer
       * than the bones that use it, which is the mismatch that produced NaN
       * vertices in the previous implementation. */
      UnityEngine.Matrix4x4[] poses = mesh.bindposes;
      System.Int32 oldPoseCount = poses == null ? 0 : poses.Length;
      System.Array.Resize(ref poses,nextBone);
      for (System.Int32 gi = 0; gi < groups.Count; gi++)
      { Group g = groups[gi];
        if (g == null) continue;
        for (System.Int32 b = 0; b < g.bones.Count; b++)
        { AddedBone ab = g.bones[b];
          poses[ab.newBoneIndex] = ab.originalBoneIndex >= 0 && ab.originalBoneIndex < oldPoseCount
            ? mesh.bindposes[ab.originalBoneIndex]
            : UnityEngine.Matrix4x4.identity; } }
      rebuilt.bindposes = poses;
      /* SetBoneWeights takes NativeArray, so the managed result is handed back
       * once here - see the note in section 4 for why the round trip is done
       * deliberately rather than threaded through the whole algorithm. */
      using (Unity.Collections.NativeArray<System.Byte> outB =
               new Unity.Collections.NativeArray<System.Byte>(extB.Length,Unity.Collections.Allocator.Temp))
      using (Unity.Collections.NativeArray<UnityEngine.BoneWeight1> outW =
               new Unity.Collections.NativeArray<UnityEngine.BoneWeight1>(extW.Length,Unity.Collections.Allocator.Temp))
      { outB.CopyFrom(extB);
        outW.CopyFrom(extW);
        rebuilt.SetBoneWeights(outB,outW); }
      /* ── 6. BONE OBJECTS ──────────────────────────────────────────
       *
       * EVERY NEW BONE HANGS OFF A BUFFER OBJECT, one per distinct parent:
       *
       *     <original bone>
       *       └── NZK_NanimBuffer_<n>       scale stays 1, absorbs parent scale
       *            └── <group name>         animates 1 -> NaN
       *
       * WHY THE BUFFER EXISTS.  The animation curve is baked against the bone's
       * local scale being 1.  Anything that rescales the RIG (a merge step, a
       * user scaling the armature) would change that bone's scale and the curve
       * would no longer reach NaN - the toggle would silently stop working.  The
       * buffer takes the scale change instead, so the animated bone stays at
       * exactly 1.  This mirrors the original's `NaNimatedBuffer` and the bug it
       * cites (bdunderscore/modular-avatar#1869). */
      UnityEngine.Transform[] newBones = new UnityEngine.Transform[nextBone];
      System.Array.Copy(renderer.bones,newBones,System.Math.Min(renderer.bones.Length,nextBone));
      System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform> bufferFor =
        new System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform>();
      for (System.Int32 gi = 0; gi < groups.Count; gi++)
      { Group g = groups[gi];
        if (g == null || g.bones.Count == 0) continue;
        System.Collections.Generic.List<UnityEngine.GameObject> created =
          new System.Collections.Generic.List<UnityEngine.GameObject>();
        for (System.Int32 b = 0; b < g.bones.Count; b++)
        { AddedBone ab = g.bones[b];
          UnityEngine.Transform parentBone =
            ab.originalBoneIndex >= 0 && ab.originalBoneIndex < renderer.bones.Length
              ? renderer.bones[ab.originalBoneIndex] : null;
          if (parentBone == null) continue;
          if (!bufferFor.TryGetValue(parentBone,out UnityEngine.Transform buffer))
          { UnityEngine.GameObject bufferObj = new UnityEngine.GameObject("NZK_NanimBuffer_" + report.BuffersCreated);
            buffer = bufferObj.transform;
            buffer.SetParent(parentBone,false);
            buffer.localPosition = UnityEngine.Vector3.zero;
            buffer.localRotation = UnityEngine.Quaternion.identity;
            buffer.localScale = UnityEngine.Vector3.one;
            bufferFor[parentBone] = buffer;
            report.BuffersCreated++; }
          /* A SUFFIX ON THE GROUP NAME, because one group can need several bones.
           * Naming them identically would make the animation side unable to tell
           * which bone a clip should drive. */
          UnityEngine.GameObject boneObj = new UnityEngine.GameObject(
            g.name + (g.bones.Count > 1 ? "_" + b : ""));
          UnityEngine.Transform boneT = boneObj.transform;
          boneT.SetParent(buffer,false);
          boneT.localPosition = UnityEngine.Vector3.zero;
          boneT.localRotation = UnityEngine.Quaternion.identity;
          boneT.localScale = UnityEngine.Vector3.one;
          newBones[ab.newBoneIndex] = boneT;
          created.Add(boneObj); }
        if (created.Count > 0) result[g.name] = created; }
      renderer.bones = newBones;
      renderer.sharedMesh = rebuilt;
      return result; }
    /* ── VERTEX STREAM COPY ────────────────────────────────────────── */
    /** <summary>Copy every vertex attribute stream, duplicating as indexed.
     *
     *  `newToOrig[newIdx] = origIdx`, duplicates allowed.
     *
     *  THIS EXISTS RATHER THAN `Mesh.Instantiate` OR A CHANNEL-BY-CHANNEL COPY,
     *  and the reason is measured: writing a mesh through the per-channel API
     *  (`copy.vertices = ...`, `copy.uv = ...`) can lose the SKINNING stream
     *  when the result is serialised, and a mesh without it is refused by the
     *  GPU upload.  Reading and writing the raw vertex BUFFERS preserves the
     *  exact stream layout the source had, including any channel we do not know
     *  about by name.
     *
     *  This is the original `MeshVertexCopyUtil.TransferVertexData`, unchanged
     *  in behaviour.</summary> */
    public static void TransferVertexData(UnityEngine.Mesh dst,UnityEngine.Mesh src,System.Int32[] newToOrig)
    { UnityEngine.Rendering.VertexAttributeDescriptor[] attrs = src.GetVertexAttributes();
      dst.SetVertexBufferParams(newToOrig.Length,attrs);
      for (System.Int32 stream = 0; stream < 4; stream++)
      { System.Int32 stride = src.GetVertexBufferStride(stream);
        if (stride == 0) continue;
        UnityEngine.GraphicsBuffer srcBuf = src.GetVertexBuffer(stream);
        System.Byte[] origData = new System.Byte[stride * src.vertexCount];
        srcBuf.GetData(origData);
        System.Byte[] newData = new System.Byte[stride * newToOrig.Length];
        for (System.Int32 v = 0; v < newToOrig.Length; v++)
          System.Array.Copy(origData,newToOrig[v] * stride,newData,v * stride,stride);
        dst.SetVertexBufferData(newData,0,0,newData.Length,stream); } }
    /** <summary>Copy every blendshape frame, duplicating vertices as indexed.
     *
     *  Shapes must be re-emitted because the vertex count changed: a shape frame
     *  is one delta per vertex, so a duplicated vertex needs its own entry or
     *  the shape would address the wrong vertices.
     *
     *  All three channels are copied.  A shape carrying only position deltas
     *  still moves the silhouette but leaves the shading behind, which reads as
     *  a region that moves without lighting correctly.
     *
     *  This is the original `MeshVertexCopyUtil.TransferShapes`.</summary> */
    public static void TransferShapes(UnityEngine.Mesh dst,UnityEngine.Mesh src,System.Int32[] newToOrig)
    { dst.ClearBlendShapes();
      System.Int32 newCount = newToOrig.Length;
      UnityEngine.Vector3[] oPos = new UnityEngine.Vector3[src.vertexCount];
      UnityEngine.Vector3[] nPos = new UnityEngine.Vector3[newCount];
      UnityEngine.Vector3[] oNrm = new UnityEngine.Vector3[src.vertexCount];
      UnityEngine.Vector3[] nNrm = new UnityEngine.Vector3[newCount];
      UnityEngine.Vector3[] oTan = new UnityEngine.Vector3[src.vertexCount];
      UnityEngine.Vector3[] nTan = new UnityEngine.Vector3[newCount];
      for (System.Int32 s = 0; s < src.blendShapeCount; s++)
      { System.String name = src.GetBlendShapeName(s);
        System.Int32 frames = src.GetBlendShapeFrameCount(s);
        for (System.Int32 f = 0; f < frames; f++)
        { src.GetBlendShapeFrameVertices(s,f,oPos,oNrm,oTan);
          for (System.Int32 i = 0; i < newCount; i++)
          { nPos[i] = oPos[newToOrig[i]];
            nNrm[i] = oNrm[newToOrig[i]];
            nTan[i] = oTan[newToOrig[i]]; }
          dst.AddBlendShapeFrame(name,src.GetBlendShapeFrameWeight(s,f),nPos,nNrm,nTan); } } }
  
}
}
}
#endif
