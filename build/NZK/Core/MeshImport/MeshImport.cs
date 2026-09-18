#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class MeshImport {
    /* ── CONSTANTS ───────────────────────────────────────────────────── */
    /** <summary>The serialised names of the importer's skin-weight settings.
     *
     *  MEASURED, not guessed.  Read off this project's actual ModelImporter:
     *      skinWeightsMode    = 1        ("Custom")
     *      maxBonesPerVertex  = 255      <- the setting that has to be capped
     *      minBoneWeight      = 0
     *      optimizeBones      = false    <- the setting that has to strip
     *
     *  An earlier revision of this file looked for `m_SkinWeights` and
     *  `m_MaxBonesPerVertex`, NEITHER OF WHICH EXISTS on this Unity version -
     *  so every model reported "unsupported" and the menu item did nothing at
     *  all while looking like it ran.  The names below are the ones that are
     *  actually present.</summary> */
    public const System.String MaxBonesProperty = "maxBonesPerVertex";
    /** <summary>The strip switch: drop bones no vertex references.</summary> */
    public const System.String OptimizeBonesProperty = "optimizeBones";
    /** <summary>Influences per vertex.  Four is BoneWeight's own slot count and
     *  what VRChat's skinning path expects.</summary> */
    public const System.Int32 MaxInfluences = 4;
    /** <summary>Every top-level serialised property name on an object.
     *
     *  Diagnostic only: it is what turns "unsupported" into a name the setting
     *  can then be looked up by.  Joined into one string because the caller
     *  logs a single line and a list would be truncated by the console.</summary> */
    public static System.String PropertyNames(UnityEditor.SerializedObject so)
    { if (so == null) return "(null)";
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      UnityEditor.SerializedProperty p = so.GetIterator();
      while (p.NextVisible(true))
      { if (sb.Length > 0) sb.Append(", ");
        sb.Append(p.name); }
      return sb.Length > 0 ? sb.ToString() : "(none)"; }
    /* ── RESULT ──────────────────────────────────────────────────────── */
    /** <summary>What an import-settings pass did.
     *
     *  `Changed` and `Inspected` are separate because "found nothing to fix" and
     *  "could not read the setting" are different answers and a single count
     *  cannot tell them apart - the second means the mesh is NOT fixed, and a
     *  silent zero would read as success.</summary> */
    
    /* ── ENTRY POINTS ────────────────────────────────────────────────── */
    /** <summary>Force 4 influences on every model under the given objects.
     *
     *  Paths are resolved from the renderers the objects actually use rather
     *  than by searching the project: an avatar only depends on the models it
     *  references, and touching an unrelated model would reimport an asset no
     *  part of this upload reads.</summary> */
    public static ImportReport ForceFour(UnityEngine.GameObject[] objs)
    { ImportReport report = new ImportReport();
      System.Collections.Generic.HashSet<System.String> paths =
        new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
      if (!NZK.B.mpty.t(objs))
      { foreach (UnityEngine.GameObject go in objs)
        { if (go == null) continue;
          UnityEngine.Transform scope = go.transform.root != null ? go.transform.root : go.transform;
          foreach (UnityEngine.SkinnedMeshRenderer smr in scope.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true))
          { if (smr == null || smr.sharedMesh == null) continue;
            System.String p = UnityEditor.AssetDatabase.GetAssetPath(smr.sharedMesh);
            if (NZK.S.Has(p)) paths.Add(p); } } }
      return ForceFour(paths); }
    /** <summary>Force 4 influences on every model in the current selection.</summary> */
    public static ImportReport ForceFourSelection()
    { return ForceFour(UnityEditor.Selection.gameObjects); }
    /** <summary>Force 4 influences on an explicit set of asset paths.
     *
     *  A path that is not a model (a .asset mesh written by this toolkit, for
     *  instance) has no ModelImporter and is counted as skipped rather than
     *  treated as a failure.</summary> */
    public static ImportReport ForceFour(System.Collections.Generic.HashSet<System.String> paths)
    { ImportReport report = new ImportReport();
      if (NZK.B.mpty.t(paths)) return report;
      System.Collections.Generic.List<System.String> reimport =
        new System.Collections.Generic.List<System.String>();
      foreach (System.String path in paths)
      { if (!NZK.S.Has(path)) continue;
        report.Inspected++;
        UnityEditor.AssetImporter importer = UnityEditor.AssetImporter.GetAtPath(path);
        if (importer == null) { report.Skipped++; continue; }
        UnityEditor.SerializedObject so;
        try { so = new UnityEditor.SerializedObject(importer); }
        catch (System.Exception) { report.Skipped++; continue; }
        UnityEditor.SerializedProperty cap = so.FindProperty(MaxBonesProperty);
        UnityEditor.SerializedProperty strip = so.FindProperty(OptimizeBonesProperty);
        if (cap == null && strip == null)
        { /* Neither setting exists on this importer - the model format may not
             support skin weights at all (.obj), or this Unity version names them
             differently.  Reported WITH the property names actually present,
             because "unsupported" alone leaves the reader unable to tell those
             two cases apart and therefore unable to fix either one. */
          report.Unsupported++;
          NZK.E.C.w(67,path + " - tried " + MaxBonesProperty + " and " + OptimizeBonesProperty +
                        "; has: " + PropertyNames(so));
          continue; }
        /* BOTH HALVES, OR NEITHER WORKS.  Capping the influence count without
           stripping leaves the renderer declaring its FULL bone array - measured
           on the reported avatar: every one of 39 SMRs declares 336 vertex
           groups while using between 2 and 74.  That array is what the uploader
           has to quantise, so a mesh weighted to 2 bones still ships a 336-group
           skeleton.  The cap alone therefore does not fix the upload.
           Stripping alone is not enough either: it removes bones nothing
           references but leaves a vertex carrying up to 255 influences. */
        System.Boolean changed = false;
        if (cap != null && cap.intValue != MaxInfluences)
        { cap.intValue = MaxInfluences;
          changed = true; }
        if (strip != null && !strip.boolValue)
        { strip.boolValue = true;
          changed = true; }
        if (!changed) { report.Already++; continue; }
        so.ApplyModifiedPropertiesWithoutUndo();
        UnityEditor.EditorUtility.SetDirty(importer);
        reimport.Add(path);
        report.Changed++; }
      /* One batched reimport at the end rather than one per asset: each reimport
         is a full asset pipeline run, and doing them inside the loop made the
         edit take minutes on an avatar with a dozen models. */
      if (reimport.Count > 0)
      { UnityEditor.AssetDatabase.StartAssetEditing();
        try
        { foreach (System.String p in reimport)
          { UnityEditor.AssetDatabase.ImportAsset(p,UnityEditor.ImportAssetOptions.ForceUpdate); } }
        finally { UnityEditor.AssetDatabase.StopAssetEditing(); }
        UnityEditor.AssetDatabase.Refresh(); }
      NZK.E.C.d(68,report.Changed + " changed, " + report.Already + " already 4, " +
                    report.Unsupported + " unsupported, " + report.Skipped + " skipped");
      return report; }
    /* ── VERIFY ──────────────────────────────────────────────────────── */
    /** <summary>The most influences any vertex of a mesh carries.
     *
     *  Re-exported from NanRelink's measurement so a caller can verify the
     *  import setting took effect without reaching into that class.  BoneWeight
     *  holds four slots, so this cannot report more than four on an IMPORTED
     *  mesh - the number the importer setting controls is the one Unity reads
     *  from the FILE, which is why the fix is at the importer and not here.</summary> */
    public static System.Int32 CountInfluences(UnityEngine.Mesh mesh)
    { if (mesh == null) return 0;
      UnityEngine.BoneWeight[] w = mesh.boneWeights;
      if (NZK.B.mpty.t(w)) return 0;
      System.Int32 max = 0;
      for (System.Int32 i = 0; i < w.Length; i++)
      { System.Int32 c = 0;
        for (System.Int32 s = 0; s < 4; s++)
        { System.Single weight = s == 0 ? w[i].weight0 : s == 1 ? w[i].weight1 : s == 2 ? w[i].weight2 : w[i].weight3;
          if (weight > 0f) c++; }
        if (c > max) max = c; }
      return max; }
    /** <summary>Every model path an avatar depends on, for inspection.
     *
     *  Public so a diagnostic can list what WOULD be changed before changing
     *  it - a settings pass that reimports a dozen models is worth confirming
     *  against the actual list first.</summary> */
    public static System.Collections.Generic.List<System.String> ModelPathsOf(UnityEngine.GameObject root)
    { System.Collections.Generic.List<System.String> paths =
        new System.Collections.Generic.List<System.String>();
      if (root == null) return paths;
      UnityEngine.Transform scope = root.transform.root != null ? root.transform.root : root.transform;
      foreach (UnityEngine.SkinnedMeshRenderer smr in scope.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true))
      { if (smr == null || smr.sharedMesh == null) continue;
        System.String p = UnityEditor.AssetDatabase.GetAssetPath(smr.sharedMesh);
        if (NZK.S.Has(p) && !paths.Contains(p)) paths.Add(p); }
      foreach (UnityEngine.MeshFilter mf in scope.GetComponentsInChildren<UnityEngine.MeshFilter>(true))
      { if (mf == null || mf.sharedMesh == null) continue;
        System.String p = UnityEditor.AssetDatabase.GetAssetPath(mf.sharedMesh);
        if (NZK.S.Has(p) && !paths.Contains(p)) paths.Add(p); }
      return paths; }
  
}
}
}
#endif
