#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Vertex Groups")]
  public class C_NZKVertexGroups : UnityEngine.MonoBehaviour
  {
    /** <summary>Which renderer to report on.  Null means "the one on this object".</summary> */
    public UnityEngine.SkinnedMeshRenderer NZKVG_Target;
    /** <summary>Emit the full per-slot table, not just the counts.
     *  A 336-slot renderer prints 336 lines, so the table is opt-in and the
     *  counts are always printed.</summary> */
    public System.Boolean NZKVG_ListEveryGroup = true;
    /** <summary>Emit a line per BONE in the avatar that no slot on this renderer
     *  refers to.  Those are the groups a caller usually means when they say a
     *  group is "missing": present on the rig, absent from this renderer. */
    public System.Boolean NZKVG_ListUnusedRendererBones = true;
    /** <summary>Only print slots with a non-zero vertex count.
     *  This is the short answer - "what is actually skinning this mesh". */
    public System.Boolean NZKVG_OnlyWeighted;
    /* ── the report ─────────────────────────────────────────────────── */
    /** <summary>Every vertex group on the target renderer, as text.
     *
     *  The counts are produced by one pass over the weight array, so a caller
     *  can afford to call this on every renderer of an avatar.</summary> */
    public System.String Report()
    { UnityEngine.SkinnedMeshRenderer smr = Resolve();
      if (smr == null) return "(no SkinnedMeshRenderer)";
      UnityEngine.Mesh mesh = smr.sharedMesh;
      UnityEngine.Transform[] bones = smr.bones;
      if (mesh == null) return "(renderer has no sharedMesh)";
      System.Int32 slotCount = bones != null ? bones.Length : 0;
      UnityEngine.BoneWeight[] weights = null;
      try { weights = mesh.boneWeights; } catch (System.Exception) { weights = null; }
      System.Int32 weightCount = weights != null ? weights.Length : 0;
      /* Per-slot tallies.  Indexed by slot, so slotCount entries even when most
         of them stay zero - a slot that exists and is unused must be visible. */
      System.Int32[] usedBy = new System.Int32[slotCount];
      System.Single[] heaviest = new System.Single[slotCount];
      System.Int32 over4 = 0;
      System.Int32 sumBad = 0;
      for (System.Int32 v = 0; v < weightCount; v++)
      { UnityEngine.BoneWeight w = weights[v];
        System.Int32 nonzero = 0;
        System.Single sum = 0f;
        for (System.Int32 s = 0; s < 4; s++)
        { System.Single wv = NZK.Core.NanRelink.WeightAt(w,s);
          if (wv <= 0f) continue;
          nonzero++;
          sum += wv;
          System.Int32 bi = NZK.Core.NanRelink.BoneIndexAt(w,s);
          if (bi >= 0 && bi < slotCount)
          { usedBy[bi]++;
            if (wv > heaviest[bi]) heaviest[bi] = wv; } }
        if (nonzero > 4) over4++;
        if (nonzero > 0 && System.Math.Abs(sum - 1f) > 0.001f) sumBad++; }
      System.Int32 emptySlots = 0;
      System.Int32 nullSlots = 0;
      System.Int32 unusedSlots = 0;
      System.Int32 nanimSlots = 0;
      for (System.Int32 i = 0; i < slotCount; i++)
      { if (bones[i] == null) nullSlots++;
        if (usedBy[i] == 0) unusedSlots++;
        if (bones[i] == null || !NZK.S.Has(bones[i].name)) emptySlots++;
        if (NZK.Core.NanRelink.IsNanimBone(bones[i] != null ? bones[i].name : null)) nanimSlots++; }
      var sb = new System.Text.StringBuilder();
      sb.Append("VERTEX GROUPS on '").Append(smr.name).Append("'");
      sb.Append("  mesh=").Append(mesh.name);
      sb.Append("  verts=").Append(mesh.vertexCount);
      sb.Append("  boneSlots=").Append(slotCount);
      sb.Append("  boneWeights=").Append(weightCount);
      sb.Append("  readable=").Append(mesh.isReadable);
      sb.Append("  submeshes=").Append(mesh.subMeshCount);
      sb.Append("  blendShapes=").Append(mesh.blendShapeCount);
      sb.Append("  bindposes=").Append(mesh.bindposes != null ? mesh.bindposes.Length : 0);
      sb.Append('\n');
      sb.Append("  slots: used=").Append(slotCount - unusedSlots);
      sb.Append(" unused=").Append(unusedSlots);
      sb.Append(" null=").Append(nullSlots);
      sb.Append(" unnamed=").Append(emptySlots);
      sb.Append(" nanimation=").Append(nanimSlots);
      sb.Append('\n');
      sb.Append("  weights: vertsOver4=").Append(over4);
      sb.Append(" vertsNotSummingToOne=").Append(sumBad);
      sb.Append('\n');
      if (NZKVG_ListEveryGroup)
      { sb.Append("  ── per slot (index | name | live | verts | heaviest) ──\n");
        for (System.Int32 i = 0; i < slotCount; i++)
        { if (NZKVG_OnlyWeighted && usedBy[i] == 0) continue;
          UnityEngine.Transform b = bones[i];
          System.String nm = b != null ? b.name : "<NULL SLOT>";
          sb.Append("  ").Append(i.ToString().PadLeft(4));
          sb.Append(" | ").Append(nm);
          if (NZK.Core.NanRelink.IsNanimBone(nm)) sb.Append("   [nanimation]");
          sb.Append(" | live=").Append(b != null ? "yes" : "NO");
          sb.Append(" | verts=").Append(usedBy[i]);
          sb.Append(" | heaviest=").Append(heaviest[i].ToString("0.######"));
          sb.Append('\n'); } }
      if (NZKVG_ListUnusedRendererBones && bones != null)
      { /* Bones reachable on the RIG but absent from this renderer's slots.
           Printed last because it is the "what is missing" half, and a reader
           has usually already seen the "what is here" half. */
        var onRig = new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
        for (System.Int32 i = 0; i < slotCount; i++)
          if (bones[i] != null && NZK.S.Has(bones[i].name)) onRig.Add(bones[i].name);
        UnityEngine.Transform root = smr.transform.root;
        if (root != null)
        { System.Int32 missing = 0;
          var lines = new System.Text.StringBuilder();
          UnityEngine.Transform[] all = root.GetComponentsInChildren<UnityEngine.Transform>(true);
          for (System.Int32 i = 0; i < all.Length; i++)
          { UnityEngine.Transform t = all[i];
            if (t == null || !NZK.S.Has(t.name)) continue;
            if (onRig.Contains(t.name)) continue;
            if (!NZK.Core.NanRelink.IsNanimBone(t.name)) continue;
            missing++;
            if (lines.Length < 2000) lines.Append("    ").Append(t.name).Append('\n'); }
          sb.Append("  ── nanimation bones ON THE RIG but NOT in this renderer's slots: ")
            .Append(missing).Append(" ──\n").Append(lines.ToString()); } }
      return sb.ToString(); }
    /** <summary>The renderer this component reports on.
     *  Explicit target wins; otherwise the renderer on this object.</summary> */
    UnityEngine.SkinnedMeshRenderer Resolve()
    { if (NZKVG_Target != null) return NZKVG_Target;
      return GetComponent<UnityEngine.SkinnedMeshRenderer>(); }
    /* ── UnityEngine plumbing ───────────────────────────────────────── */
    void Reset()
    { NZKVG_Target = GetComponent<UnityEngine.SkinnedMeshRenderer>(); }
    void Start() { Emit(false); }
    /** <summary>Print the report to the console.
     *
     *  `onlyNames` collapses it to the slot names, which is what a quick
     *  "does this mesh have group X" check actually needs. */
    public void Emit(System.Boolean onlyNames)
    { UnityEngine.SkinnedMeshRenderer smr = Resolve();
      if (smr == null)
      { UnityEngine.Debug.LogWarning("[NZK VertexGroups] " + name + ": no SkinnedMeshRenderer to report on."); return; }
      if (!onlyNames) { UnityEngine.Debug.Log(Report()); return; }
      UnityEngine.Transform[] bones = smr.bones;
      if (NZK.B.mpty.t(bones)) { UnityEngine.Debug.LogWarning("[NZK VertexGroups] " + name + ": renderer has no bones."); return; }
      var sb = new System.Text.StringBuilder();
      sb.Append("[NZK VertexGroups] ").Append(smr.name).Append(": ");
      for (System.Int32 i = 0; i < bones.Length; i++)
      { if (i > 0) sb.Append(", ");
        sb.Append(bones[i] != null ? bones[i].name : "<NULL>"); }
      UnityEngine.Debug.Log(sb.ToString()); }
#if UNITY_EDITOR
    /* ── editor conveniences ────────────────────────────────────────── */
    /** <summary>Menu item: report every vertex-group-bearing renderer under the
     *  selected objects' roots.  Reads only, like everything else here.</summary> */
    [UnityEditor.MenuItem("GameObject/NZK/Mesh/Report Vertex Groups",false,229)]
    public static void ReportSelection()
    { UnityEngine.GameObject[] sel = UnityEditor.Selection.gameObjects;
      if (NZK.B.mpty.t(sel))
      { NZK.E.C.w(110,"nothing selected for a vertex-group report"); return; }
      var seen = new System.Collections.Generic.HashSet<UnityEngine.SkinnedMeshRenderer>();
      System.Int32 n = 0;
      for (System.Int32 i = 0; i < sel.Length; i++)
      { UnityEngine.GameObject go = sel[i];
        if (go == null) continue;
        UnityEngine.Transform scope = go.transform.root != null ? go.transform.root : go.transform;
        UnityEngine.SkinnedMeshRenderer[] smrs = scope.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
        for (System.Int32 k = 0; k < smrs.Length; k++)
        { UnityEngine.SkinnedMeshRenderer smr = smrs[k];
          if (smr == null || smr.sharedMesh == null) continue;
          if (!seen.Add(smr)) continue;
          UnityEngine.Debug.Log(new C_NZKVertexGroups { NZKVG_Target = smr }.Report());
          n++; } }
      NZK.E.C.d(112,"vertex-group report written for " + n + " renderer(s)"); }
#endif
  }
}
}
#endif
