#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshesMerge {
    /* ── CONSTANTS ───────────────────────────────────────────────────── */
    /** <summary>Name of the object the merged results are parented under.</summary> */
    public const System.String MergedRootName = "NZK_Merged";
    /** <summary>Name of the object the sources are parked under, switched off.</summary> */
    public const System.String SourceRootName = "NKZ_Source_Meshes";
    /** <summary>Prefix each merged object carries so a re-merge can find it.
     *
     *  A merge run twice must not treat its own previous output as an input, so
     *  the prefix is both the recognition rule and the reason the name is not
     *  simply the source's name.</summary> */
    public const System.String MergedPrefix = "Merged_";
    /** <summary>VRChat's hard limit on influences per vertex.  A vertex carrying
     *  more than this is silently truncated on upload, which unlinks bones in a
     *  way that is invisible until a toggle stops working.</summary> */
    public const System.Int32 MaxInfluences = 4;
    /** <summary>Folder every merged mesh asset is written under.</summary> */
    public const System.String OutputFolder = "Assets/NZK_Generated/Meshes";
    /* ── RESULT ──────────────────────────────────────────────────────── */
    /** <summary>What one merge run produced.
     *
     *  Failures are REPORTED rather than thrown and rather than silently
     *  returning a half-built object: a merge that cannot save its asset must
     *  leave the scene as it found it, so callers check `Success` before they
     *  use anything else here.</summary> */
    
    /* ── ENTRY POINT ─────────────────────────────────────────────────── */
    /** <summary>Merge the selected objects, one source at a time.
     *
     *  Each iteration pops exactly one source off the pending list, merges that
     *  single source, and only then removes it - so the pending list shrinks by
     *  one per produced object and the final count matches the input count.</summary> */
    public static MergeOutcome MergeSelection()
    {
      UnityEngine.GameObject[] sel = UnityEditor.Selection.gameObjects;
      return MergeObjects(sel,UnityEditor.Selection.activeTransform);
    }
    /** <summary>Merge an explicit array, one source at a time.
     *
     *  `anchor` is the transform the merged root and the source parking root are
     *  parented under.  It is passed explicitly rather than read from the
     *  selection so this can be driven from a bake step or a test with no
     *  selection at all.</summary> */
    public static MergeOutcome MergeObjects(UnityEngine.GameObject[] sources,UnityEngine.Transform anchor)
    {
      MergeOutcome outcome = new MergeOutcome();
      outcome.Merged = new System.Collections.Generic.List<UnityEngine.GameObject>();
      outcome.Sources = new System.Collections.Generic.List<UnityEngine.GameObject>();
      outcome.Skipped = 0;
      outcome.Success = false;
      /* The work queue.  Distinct preserves selection ORDER, which matters
         because the merged objects are named from their sources and a user
         re-running the merge expects the same order. */
      System.Collections.Generic.List<UnityEngine.GameObject> pending =
        NZK.B.mpty.t(sources)
          ? new System.Collections.Generic.List<UnityEngine.GameObject>()
          : System.Linq.Enumerable.ToList(
              System.Linq.Enumerable.Distinct(
                System.Linq.Enumerable.Where(sources,s => s != null)));
      if (pending.Count == 0)
      { outcome.ErrorCode = 10;
        outcome.ErrorMessage = NZK.E.rr10;
        NZK.E.C.e(outcome.ErrorCode,outcome.ErrorMessage);
        return outcome; }
      UnityEngine.Transform root = ResolveAnchor(anchor,pending[0]);
      if (root == null)
      { outcome.ErrorCode = 56;
        outcome.ErrorMessage = NZK.E.rr56(anchor != null ? (System.Object)anchor : (System.Object)sources);
        NZK.E.C.e(outcome.ErrorCode,outcome.ErrorMessage);
        return outcome; }
      UnityEngine.Transform mergedRoot = EnsureChild(root,MergedRootName);
      UnityEngine.Transform sourceRoot = EnsureChild(root,SourceRootName);
      UnityEditor.Undo.SetCurrentGroupName("Merge Meshes");
      int undoGroup = UnityEditor.Undo.GetCurrentGroup();
      /* ── ONE AT A TIME ─────────────────────────────────────────────── */
      while (pending.Count > 0)
      {
        /* Always take index 0 and remove it only AFTER it has been read.  Taking
           the LAST element would reverse the order; taking 0 and forgetting to
           remove it would loop forever, so the removal is deliberately the next
           statement rather than something the loop body may skip. */
        UnityEngine.GameObject src = pending[0];
        pending.RemoveAt(0);
        if (src == null) { continue; }
        UnityEngine.Mesh mesh = MergeCore.GetMesh(src);
        if (mesh == null)
        { outcome.Skipped++;
          NZK.E.C.w(52,src.name);
          continue; }
        System.String name = MergedName(src);
        UnityEngine.Mesh merged = MergeOne(src,mesh);
        if (merged == null)
        { outcome.Skipped++;
          continue; }
        UnityEngine.Transform mergedParent = ParentFor(src,mergedRoot);
        UnityEngine.GameObject go = new UnityEngine.GameObject(name);
        go.transform.SetParent(mergedParent,false);
        UnityEditor.Undo.RegisterCreatedObjectUndo(go,"Create merged mesh");
        UnityEngine.SkinnedMeshRenderer smr = go.AddComponent<UnityEngine.SkinnedMeshRenderer>();
        smr.sharedMesh = merged;
        smr.sharedMaterials = ResolveRendererMaterials(src,mesh);
        UnityEngine.Transform boneRoot = FirstBone(src);
        UnityEngine.Transform[] bones = SourceBones(src);
        if (!NZK.B.mpty.t(bones))
        { smr.bones = bones;
          smr.rootBone = (boneRoot != null) ? boneRoot : bones[0]; }
        /* Park the SOURCE: switch it off, leave it whole.  Nothing is destroyed
           (see the file header) and the object keeps its own name, so the
           hierarchy still reads as the rig the user built. */
        src.transform.SetParent(sourceRoot,true);
        src.SetActive(false);
        outcome.Sources.Add(src);
        outcome.Merged.Add(go);
      }
      UnityEditor.Undo.CollapseUndoOperations(undoGroup);
      outcome.Success = outcome.Merged.Count > 0;
      if (!outcome.Success)
      { outcome.ErrorCode = 52;
        outcome.ErrorMessage = NZK.E.rr52(sources);
        NZK.E.C.e(outcome.ErrorCode,outcome.ErrorMessage);
        return outcome; }
      NZK.E.C.d(57,outcome.Merged.Count);
      if (outcome.Skipped > 0) { NZK.E.C.w(30,outcome.Skipped); }
      return outcome;
    }
    /* ── ONE SOURCE ──────────────────────────────────────────────────── */
    /** <summary>Merge a single source into one mesh.
     *
     *  This is the whole of the per-iteration work, which is the point of the
     *  rewrite: one source in, one mesh out, no other source consulted.  A
     *  source with no submeshes is reported rather than producing an empty mesh,
     *  because an empty mesh renders as nothing and reads as a silent failure.
     *
     *  The SOURCE mesh is preferred over the live scene mesh: the scene mesh has
     *  had its sub-0.01 vertex groups stripped by the importer/optimizer, so it
     *  is not the authority for which groups a vertex belongs to.</summary> */
    public static UnityEngine.Mesh MergeOne(UnityEngine.GameObject src,UnityEngine.Mesh mesh)
    {
      if (src == null || mesh == null) return null;
      /* Prefer the imported .blend sub-asset: the scene mesh has already lost
         its near-zero vertex groups, so it cannot tell us which groups a vertex
         belongs to.  Fall back to the scene mesh when nothing resolves.
         This resolution happens FIRST because the resolved mesh is the mesh the
         CombineInstances below are built against, so it - not the live mesh - is
         the authority for the submesh count.  Reading the count off the LIVE
         mesh lets a source asset with FEWER submeshes than the scene mesh drive
         the loop past the source's last submesh, and CombineMeshes then throws
         ArgumentException: the merge is caught, reported as code 55, and the
         source is silently counted as skipped. */
      UnityEngine.Mesh sourceMesh = (src != null)
        ? (NZK.Core.NanRelink.FindSourceMesh(src.GetComponent<UnityEngine.SkinnedMeshRenderer>()) ?? mesh)
        : mesh;
      if (sourceMesh != mesh) { NZK.E.C.d(96,src.name); }
      else { NZK.E.C.w(97,src.name); }
      /* The submesh count AND the loop bound both come from the RESOLVED mesh,
         so the failure path below describes the mesh actually being combined. */
      int subCount = sourceMesh.subMeshCount;
      if (subCount <= 0)
      { System.String msg = NZK.E.rr53(src.name);
        NZK.E.C.e(53,msg);
        return null; }
      /* The merged renderer's slots are sized from the SOURCE submesh count, so
         a live mesh carrying extra material slots (a renderer slot with no
         submesh behind it) would leave the material array longer than the mesh
         it is assigned to.  Reported rather than silently misindexed. */
      if (mesh.subMeshCount != sourceMesh.subMeshCount) { NZK.E.C.w(98,src.name); }
      /* The source is MERGED IN PLACE (identity transform) - it is not being
         re-parented into the merged object, so baking its world matrix here
         would offset every merged mesh by the source's own position. */
      UnityEngine.CombineInstance[] cis = new UnityEngine.CombineInstance[subCount];
      for (System.Int32 s = 0; s < subCount; s++)
      { cis[s] = new UnityEngine.CombineInstance
        { mesh = sourceMesh,subMeshIndex = s,transform = UnityEngine.Matrix4x4.identity }; }
      UnityEngine.Mesh outMesh = new UnityEngine.Mesh { name = MergedMeshName(src) };
      if (sourceMesh.vertexCount > 65535) outMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
      /* SUBMESHES ARE KEPT SEPARATE (mergeSubMeshes:false) so one output submesh
         exists per source submesh.  ResolveRendererMaterials returns one material
         per source submesh in submesh order, so that array keeps indexing the
         same submeshes after the combine; merging them would fuse two submeshes
         that merely shared a material. */
      try { outMesh.CombineMeshes(cis,false,true); }
      catch (System.ArgumentException ex)
      { NZK.E.C.e(55,src.name + " - " + ex.Message);
        return null; }
      CopyUvChannels(sourceMesh,outMesh,sourceMesh.vertexCount);
      CopyBones(src,outMesh);
      return outMesh;
    }
    /* ── MATERIALS ───────────────────────────────────────────────────── */
    /** <summary>The per-submesh material array for the merged renderer.
     *
     *  One material per submesh, in SUBMESH ORDER, with a source that has fewer
     *  materials than submeshes extended by repeating its LAST material rather
     *  than by a null.  Repeating the last material is what Unity itself does
     *  when a submesh has no entry: a null there renders the submesh with the
     *  default purple material, which looks like corruption rather than a
     *  missing assignment.
     *
     *  `mesh` is the mesh the returned array is assigned to, so its submesh
     *  count is the RENDERER's slot count.  The caller passes the LIVE renderer
     *  mesh deliberately: the live slots are what the user configured, and the
     *  merged mesh inherits their submesh count whenever the resolved source and
     *  the live mesh agree.  The array is TRIMMED when the renderer has more
     *  material slots than submeshes, so the result always satisfies the promise
     *  above (Length == submesh count) instead of handing the renderer an array
     *  with slots no submesh indexes.</summary> */
    public static UnityEngine.Material[] ResolveRendererMaterials(UnityEngine.GameObject src,UnityEngine.Mesh mesh)
    {
      UnityEngine.Material[] srcMats = MergeCore.GetMats(src);
      int subCount = mesh != null ? mesh.subMeshCount : 0;
      if (subCount <= 0) return srcMats ?? new UnityEngine.Material[0];
      if (NZK.B.mpty.t(srcMats)) return new UnityEngine.Material[subCount];
      if (srcMats.Length == subCount) return srcMats;
      if (srcMats.Length > subCount)
      { /* Extra slots: keep the FIRST subCount entries, which are the ones the
           submeshes address, and drop the rest. */
        UnityEngine.Material[] trimmed = new UnityEngine.Material[subCount];
        System.Array.Copy(srcMats,trimmed,subCount);
        return trimmed; }
      UnityEngine.Material[] padded = new UnityEngine.Material[subCount];
      System.Array.Copy(srcMats,padded,srcMats.Length);
      UnityEngine.Material last = srcMats[srcMats.Length - 1];
      for (System.Int32 s = srcMats.Length; s < subCount; s++) padded[s] = last;
      return padded;
    }
    /** <summary>Append the UV channels CombineMeshes drops.
     *
     *  CombineMeshes copies uv/uv2 but NOT uv3/uv4, so a merged mesh silently
     *  loses the third and fourth channel and every shader that reads them
     *  (lilToon's decal mask, Poiyomi's UV3 packing) renders wrong with no
     *  error.  The channel is copied only when its length matches the merged
     *  vertex count, so a source that never authored it does not shift every
     *  later vertex's UV by its own size.</summary> */
    public static void CopyUvChannels(UnityEngine.Mesh src,UnityEngine.Mesh dst,System.Int32 vertexCount)
    {
      if (src == null || dst == null) return;
      if (dst.vertexCount != vertexCount) return;
      if (src.uv3 != null && src.uv3.Length == vertexCount) dst.uv3 = src.uv3;
      if (src.uv4 != null && src.uv4.Length == vertexCount) dst.uv4 = src.uv4;
    }
    /* ── BONES ───────────────────────────────────────────────────────── */
    /** <summary>Carry the source mesh's boneWeights onto the merged mesh.
     *
     *  The weights are copied from the source mesh VERBATIM because the merged
     *  mesh has the SAME vertex count (one source, identity transform), so its
     *  vertex ordering is the source's.  A vertex with no weights is given a
     *  single influence on bone 0: leaving an empty weight is what makes Unity
     *  unlink the vertex and drop the mesh to the origin on import.
     *
     *  BIND POSES: DELIBERATELY NOT REBUILT.  The imported bindpose array was
     *  already carried over by CombineMeshes from the same mesh the vertices
     *  came from and is correct by construction.  Deriving it from each bone's
     *  LIVE world transform makes the rest pose depend on where the bones are
     *  standing at merge time, so the client skins the mesh against a foreign
     *  rest pose and the avatar collapses to a point or flies apart with nothing
     *  in the log.</summary> */
    public static void CopyBones(UnityEngine.GameObject src,UnityEngine.Mesh mesh)
    {
      if (src == null || mesh == null) return;
      UnityEngine.SkinnedMeshRenderer smr = src.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr == null || NZK.B.mpty.t(smr.bones)) return;
      UnityEngine.BoneWeight[] srcWeights = mesh.boneWeights;
      System.Int32 count = mesh.vertexCount;
      UnityEngine.BoneWeight[] weights = new UnityEngine.BoneWeight[count];
      if (NZK.B.mpty.t(srcWeights) || srcWeights.Length < count)
      { for (System.Int32 v = 0; v < count; v++)
        { weights[v] = new UnityEngine.BoneWeight { boneIndex0 = 0,weight0 = 1f }; } }
      else
      { System.Array.Copy(srcWeights,weights,count); }
      mesh.boneWeights = weights;
    }
    /** <summary>The bones the merged renderer binds to.
     *
     *  Returned VERBATIM and UNFILTERED, holes included: the mesh's own
     *  boneIndex0..3 address THIS array positionally, so handing the renderer a
     *  shorter array with the nulls squeezed out shifts every index past the
     *  first hole onto the wrong bone (or out of range, which the client
     *  resolves to null and collapses the vertex onto the origin).</summary> */
    public static UnityEngine.Transform[] SourceBones(UnityEngine.GameObject src)
    {
      if (src == null) return new UnityEngine.Transform[0];
      UnityEngine.SkinnedMeshRenderer smr = src.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr == null || NZK.B.mpty.t(smr.bones)) return new UnityEngine.Transform[0];
      return smr.bones;
    }
    /** <summary>The transform the merged renderer's root bone is set from.
     *
     *  Preferred in order: the renderer's own rootBone, then the armature's
     *  Hips.  A root bone is what the skinning is measured against, so guessing
     *  the object itself (the old behaviour) makes a merged mesh collapse to its
     *  own origin instead of following the rig.</summary> */
    public static UnityEngine.Transform FirstBone(UnityEngine.GameObject src)
    {
      if (src == null) return null;
      UnityEngine.SkinnedMeshRenderer smr = src.GetComponent<UnityEngine.SkinnedMeshRenderer>();
      if (smr != null && smr.rootBone != null) return smr.rootBone;
      UnityEngine.Transform root = src.transform.root;
      if (root != null)
      { UnityEngine.Transform arm = root.Find("Armature");
        if (arm != null)
        { UnityEngine.Transform hips = arm.Find("Hips");
          if (hips != null) return hips;
          return arm; } }
      return null;
    }
    /* ── NAMING + PLACEMENT ──────────────────────────────────────────── */
    /** <summary>The scene name for a merged object.
     *
     *  Always prefixed, so a second run can tell its own output from a source
     *  and never merges a merged object into itself.</summary> */
    public static System.String MergedName(UnityEngine.GameObject src)
    { if (src == null) return MergedPrefix + "Mesh";
      return MergedPrefix + src.name; }
    /** <summary>The asset name for a merged mesh, sanitised for the filesystem.
     *
     *  The scene OBJECT keeps the source's spacing because that is what a human
     *  reads; the ASSET name cannot, because a slash or a quote in it would
     *  either create a directory or break the path outright.</summary> */
    public static System.String MergedMeshName(UnityEngine.GameObject src)
    { if (src == null) return "MergedMesh";
      return Meshes.Vars.Names.Build.SanitizeBoneName(MergedName(src)); }
    /** <summary>Where a merged object goes: BESIDE its source, not under one root.
     *
     *  The source's own parent is used so a merge performed on a child of the
     *  armature does not teleport the result to the avatar root, and the merged
     *  object keeps the sibling relationship the source had - which is what
     *  animation paths and constraints are written against.</summary> */
    public static UnityEngine.Transform ParentFor(UnityEngine.GameObject src,UnityEngine.Transform fallback)
    { if (src == null) return fallback;
      UnityEngine.Transform p = src.transform.parent;
      return (p != null) ? p : fallback; }
    /** <summary>Resolve the transform the merge roots are parented under.
     *
     *  The ARM ROOT of the anchor is used rather than the anchor itself: the
     *  anchor is usually one of the selected meshes, and parenting the merged
     *  root under a source parks the results inside the very object that is
     *  about to be switched off.</summary> */
    public static UnityEngine.Transform ResolveAnchor(UnityEngine.Transform anchor,UnityEngine.GameObject first)
    { if (anchor != null) return anchor.root;
      if (first != null) return first.transform.root;
      return null; }
    /** <summary>Find or create a child by name.
     *
     *  A child is CREATED at the world origin with an identity local transform,
     *  so it does not inherit the position of whatever it happens to be created
     *  under in a way that offsets the meshes parented to it afterwards.</summary> */
    public static UnityEngine.Transform EnsureChild(UnityEngine.Transform parent,System.String name)
    { if (parent == null) return null;
      UnityEngine.Transform found = parent.Find(name);
      if (found != null) return found;
      UnityEngine.GameObject go = new UnityEngine.GameObject(name);
      go.transform.SetParent(parent,false);
      go.transform.localPosition = UnityEngine.Vector3.zero;
      go.transform.localRotation = UnityEngine.Quaternion.identity;
      go.transform.localScale = UnityEngine.Vector3.one;
      return go.transform; }
    /* ── MESH ASSET ──────────────────────────────────────────────────── */
    /** <summary>Save a merged mesh beside the avatar's other generated meshes.
     *
     *  The folder is derived from the MESH's own name, not from the source
     *  object, so the asset path and the asset's contents agree - a mismatch
     *  there is what makes two merges of different sources overwrite each
     *  other's asset and leave the scene pointing at the wrong geometry.</summary> */
    public static UnityEngine.Mesh SaveMergedAsset(UnityEngine.Mesh merged,System.String avatarName)
    { if (merged == null) return null;
      System.String folder = OutputFolder + "/" + NZK.Core.Vars.Names.Sanitize(avatarName);
      Systems.Folder.Ensure(folder);
      System.String path = folder + "/" + merged.name + ".asset";
      UnityEngine.Mesh existing = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path);
      if (existing != null) UnityEditor.AssetDatabase.DeleteAsset(path);
      UnityEditor.AssetDatabase.CreateAsset(merged,path);
      UnityEditor.EditorUtility.SetDirty(merged);
      UnityEditor.AssetDatabase.SaveAssets();
      return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path); }
  
}
}
}
#endif
