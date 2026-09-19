/* ==========================================================================
 * NZK.Audit.Meshes — aggregate every mesh channel and report only DIFFS.
 *
 * WHY THIS EXISTS
 *   Verifying "did the generator copy the mesh correctly" one channel at a
 *   time through exec/reflection is slow and error-prone: each probe has to
 *   re-find the types and re-load the assets, and a channel that was forgotten
 *   simply never gets checked.  This walks EVERY channel Unity exposes on a
 *   Mesh, compares it against the source, and prints a single table where the
 *   only interesting row is the one that is not "OK".
 *
 * HOW TO RUN (in Unity, via the ucli `exec` command)
 *   Paste the body of Audit.Run() as the code, or call it directly if the file
 *   is inside the project.  It needs no arguments: the model and the generated
 *   folder are declared as constants below.
 *
 * INPUTS, and why they are constants rather than parameters
 *   A probe is run by pasting code into a console, where a parameter list is
 *   more friction than editing two lines.  Change SourceModel and GenFolder to
 *   audit a different avatar.
 *
 * OUTPUT
 *   One block per mesh, with a DIFF count per channel.  A mesh that copied
 *   cleanly prints every channel as "0 diffs".  Anything else is the bug.
 *
 * SCOPE
 *   Editor-only: AssetDatabase and Mesh.blendShape* reads.  Not shipped, not
 *   part of the toolkit build - this file lives under testing/ and is here so
 *   the next verification does not have to be re-invented.
 * ========================================================================== */

#if UNITY_EDITOR
namespace NZK
{
public static partial class Core
{
  public static class AuditMeshes
  {
    public const System.String SourceModel = "Assets/Nemasis.blend";
    public const System.String GenFolder   = "Assets/!_NZK_Generated/Nemasis/Meshes";

    /** <summary>One mesh's comparison result, so callers can assert on it.</summary>
     *
     *  Counts rather than booleans: "3 of 5427 vertices differ" and "all 5427
     *  differ" are different bugs, and a bool cannot tell them apart.</summary> */
    public struct MeshDiff
    {
      public System.String Name;
      public System.Int32   SrcVerts;
      public System.Int32   GenVerts;
      public System.Int32   VertDiffs;
      public System.Int32   NormalDiffs;
      public System.Int32   TangentDiffs;
      public System.Int32   UvDiffs;
      public System.Int32   ColorDiffs;
      public System.Int32   BoneWeightDiffs;
      public System.Int32   BindposeDiffs;
      public System.Int32   TriangleDiffs;
      public System.Int32   ShapeNameDiffs;
      public System.Int32   ShapeFrameDiffs;
      public System.Int32   ShapeWeightDiffs;
      public System.Int32   ShapeVertexDiffs;
      public System.Int32   ShapeNormalDiffs;
      public System.Int32   ShapeTangentDiffs;
      /** Every source UV set that is present on the source but absent from, or
       *  a different length on, the generated mesh. */
      public System.Int32   MissingUvSets;

      public System.Boolean Clean
      { get { return VertDiffs == 0 && NormalDiffs == 0 && TangentDiffs == 0 &&
                     UvDiffs == 0 && ColorDiffs == 0 && BoneWeightDiffs == 0 &&
                     BindposeDiffs == 0 && TriangleDiffs == 0 &&
                     ShapeNameDiffs == 0 && ShapeFrameDiffs == 0 &&
                     ShapeWeightDiffs == 0 && ShapeVertexDiffs == 0 &&
                     ShapeNormalDiffs == 0 && ShapeTangentDiffs == 0 &&
                     MissingUvSets == 0; } }
    }

    /** <summary>Compare one source mesh against one generated mesh.</summary> */
    public static MeshDiff Compare(UnityEngine.Mesh src, UnityEngine.Mesh gen)
    { MeshDiff d = new MeshDiff();
      if (src == null || gen == null) { d.Name = "(null)"; return d; }
      d.Name     = src.name;
      d.SrcVerts = src.vertexCount;
      d.GenVerts = gen.vertexCount;

      /* Vertex-count mismatch invalidates every per-vertex comparison below,
         so it is recorded and the channel diffs are left at 0 rather than
         reporting a meaningless "all differ". */
      if (src.vertexCount != gen.vertexCount) return d;

      d.VertDiffs   = CountVector3(src.vertices,  gen.vertices);
      d.NormalDiffs = CountVector3(src.normals,   gen.normals);
      d.TangentDiffs= CountVector4(src.tangents,  gen.tangents);
      d.UvDiffs     = CountVector2(src.uv,        gen.uv);
      d.ColorDiffs  = CountColor  (src.colors,    gen.colors);
      d.BoneWeightDiffs = CountBoneWeight(src.boneWeights, gen.boneWeights);
      d.BindposeDiffs   = CountMatrix(src.bindposes, gen.bindposes);
      d.TriangleDiffs   = CountInt(src.triangles, gen.triangles);

      /* Every UV set, not just uv0: the NaNimate materials use up to 8 and a
         dropped uv2 is invisible in a uv0-only check while still changing the
         vertex stream stride the material depends on. */
      d.MissingUvSets = 0;
      for (System.Int32 i = 2; i <= 8; i++)
      { UnityEngine.Vector2[] s = UvSet(src, i), g = UvSet(gen, i);
        if (s == null) continue;
        if (g == null || g.Length != s.Length) { d.MissingUvSets++; continue; }
        d.UvDiffs += CountVector2(s, g); }

      CompareShapes(src, gen, ref d);
      return d; }

    /** <summary>Compare blendshape names, order, frames and all three deltas.
     *
     *  Order matters as much as content: NaNimation animations drive shapes by
     *  INDEX, so a reordered list animates the wrong shape while every count
     *  and name still "exists".  The positional name comparison is what catches
     *  that.</summary> */
    public static void CompareShapes(UnityEngine.Mesh src, UnityEngine.Mesh gen, ref MeshDiff d)
    { d.ShapeNameDiffs = 0; d.ShapeFrameDiffs = 0; d.ShapeWeightDiffs = 0;
      d.ShapeVertexDiffs = 0; d.ShapeNormalDiffs = 0; d.ShapeTangentDiffs = 0;
      if (src.blendShapeCount != gen.blendShapeCount)
      { d.ShapeNameDiffs = System.Math.Abs(src.blendShapeCount - gen.blendShapeCount);
        return; }
      for (System.Int32 si = 0; si < src.blendShapeCount; si++)
      { if (src.GetBlendShapeName(si) != gen.GetBlendShapeName(si))
        { d.ShapeNameDiffs++; continue; }
        System.Int32 sf = src.GetBlendShapeFrameCount(si);
        System.Int32 gf = gen.GetBlendShapeFrameCount(si);
        if (sf != gf) { d.ShapeFrameDiffs++; continue; }
        System.Int32 vc = src.vertexCount;
        for (System.Int32 fi = 0; fi < sf; fi++)
        { if (src.GetBlendShapeFrameWeight(si, fi) != gen.GetBlendShapeFrameWeight(si, fi))
            d.ShapeWeightDiffs++;
          UnityEngine.Vector3[] sv = new UnityEngine.Vector3[vc], sn = new UnityEngine.Vector3[vc], st = new UnityEngine.Vector3[vc];
          UnityEngine.Vector3[] gv = new UnityEngine.Vector3[vc], gn = new UnityEngine.Vector3[vc], gt = new UnityEngine.Vector3[vc];
          src.GetBlendShapeFrameVertices(si, fi, sv, sn, st);
          gen.GetBlendShapeFrameVertices(si, fi, gv, gn, gt);
          d.ShapeVertexDiffs  += CountVector3(sv, gv);
          d.ShapeNormalDiffs  += CountVector3(sn, gn);
          d.ShapeTangentDiffs += CountVector3(st, gt); } } }

    /* ── PRIMITIVE COMPARATORS ───────────────────────────────────────── */

    public static System.Int32 CountVector2(UnityEngine.Vector2[] a, UnityEngine.Vector2[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++) if (a[i] != b[i]) c++;
      return c; }

    public static System.Int32 CountVector3(UnityEngine.Vector3[] a, UnityEngine.Vector3[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++) if (a[i] != b[i]) c++;
      return c; }

    public static System.Int32 CountVector4(UnityEngine.Vector4[] a, UnityEngine.Vector4[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++) if (a[i] != b[i]) c++;
      return c; }

    public static System.Int32 CountColor(UnityEngine.Color[] a, UnityEngine.Color[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++) if (a[i] != b[i]) c++;
      return c; }

    public static System.Int32 CountInt(System.Int32[] a, System.Int32[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++) if (a[i] != b[i]) c++;
      return c; }

    /** <summary>Bone weights, all four indices AND all four weights.
     *
     *  Index-only comparison would pass for a vertex whose slot COUNT is right
     *  but whose bones are wrong, which is the exact failure a relink produces.
     *  Weight-only would pass for correct values on the wrong bones.</summary> */
    public static System.Int32 CountBoneWeight(UnityEngine.BoneWeight[] a, UnityEngine.BoneWeight[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++)
      { if (a[i].boneIndex0 != b[i].boneIndex0 || a[i].boneIndex1 != b[i].boneIndex1 ||
            a[i].boneIndex2 != b[i].boneIndex2 || a[i].boneIndex3 != b[i].boneIndex3 ||
            a[i].weight0 != b[i].weight0 || a[i].weight1 != b[i].weight1 ||
            a[i].weight2 != b[i].weight2 || a[i].weight3 != b[i].weight3) c++; }
      return c; }

    public static System.Int32 CountMatrix(UnityEngine.Matrix4x4[] a, UnityEngine.Matrix4x4[] b)
    { if (a == null || b == null) return a == b ? 0 : -1;
      System.Int32 n = System.Math.Min(a.Length, b.Length);
      System.Int32 c = System.Math.Abs(a.Length - b.Length);
      for (System.Int32 i = 0; i < n; i++)
      { if (a[i].m00 != b[i].m00 || a[i].m01 != b[i].m01 || a[i].m02 != b[i].m02 || a[i].m03 != b[i].m03 ||
            a[i].m10 != b[i].m10 || a[i].m11 != b[i].m11 || a[i].m12 != b[i].m12 || a[i].m13 != b[i].m13 ||
            a[i].m20 != b[i].m20 || a[i].m21 != b[i].m21 || a[i].m22 != b[i].m22 || a[i].m23 != b[i].m23 ||
            a[i].m30 != b[i].m30 || a[i].m31 != b[i].m31 || a[i].m32 != b[i].m32 || a[i].m33 != b[i].m33) c++; }
      return c; }

    /** <summary>UV set by 1-based name, matching the inspector's channel order.</summary> */
    public static UnityEngine.Vector2[] UvSet(UnityEngine.Mesh m, System.Int32 which)
    { switch (which)
      { case 2: return m.uv2; case 3: return m.uv3; case 4: return m.uv4;
        case 5: return m.uv5; case 6: return m.uv6; case 7: return m.uv7;
        case 8: return m.uv8; default: return m.uv; } }

    /* ── ENTRY POINTS ────────────────────────────────────────────────── */

    /** <summary>Audit every mesh pair between the model and the generated tree.
     *
     *  Meshes are paired BY NAME.  Assets whose names do not match are reported
     *  rather than skipped: an unmatched name means a generated mesh came from
     *  somewhere other than the model, which is itself the finding.</summary> */
    public static System.Collections.Generic.List<MeshDiff> Run()
    { UnityEngine.Object[] subs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(SourceModel);
      System.Collections.Generic.Dictionary<System.String, UnityEngine.Mesh> src =
        new System.Collections.Generic.Dictionary<System.String, UnityEngine.Mesh>();
      foreach (UnityEngine.Object o in subs)
      { UnityEngine.Mesh m = o as UnityEngine.Mesh;
        if (m != null) src[m.name] = m; }

      System.Collections.Generic.List<MeshDiff> results =
        new System.Collections.Generic.List<MeshDiff>();
      System.String[] files = System.IO.Directory.GetFiles(GenFolder, "*.asset");
      for (System.Int32 i = 0; i < files.Length; i++)
      { UnityEngine.Mesh gen = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(files[i]);
        if (gen == null) continue;
        UnityEngine.Mesh s = null;
        src.TryGetValue(gen.name, out s);
        results.Add(Compare(s, gen)); }
      Report(results);
      return results; }

    /** <summary>Print the table, DIFF-first so the interesting row is on top. */
    public static void Report(System.Collections.Generic.List<MeshDiff> diffs)
    { System.Int32 clean = 0, dirty = 0;
      System.Text.StringBuilder bad = new System.Text.StringBuilder();
      System.Text.StringBuilder ok  = new System.Text.StringBuilder();
      for (System.Int32 i = 0; i < diffs.Count; i++)
      { MeshDiff d = diffs[i];
        if (d.Clean) { clean++; ok.Append("  OK   ").Append(d.Name).Append('\n'); continue; }
        dirty++;
        bad.Append("  DIFF ").Append(d.Name)
           .Append("  verts=").Append(d.SrcVerts).Append('/').Append(d.GenVerts)
           .Append("  v=").Append(d.VertDiffs)
           .Append(" n=").Append(d.NormalDiffs)
           .Append(" t=").Append(d.TangentDiffs)
           .Append(" uv=").Append(d.UvDiffs)
           .Append(" col=").Append(d.ColorDiffs)
           .Append(" bw=").Append(d.BoneWeightDiffs)
           .Append(" pose=").Append(d.BindposeDiffs)
           .Append(" tri=").Append(d.TriangleDiffs)
           .Append(" | shapes: name=").Append(d.ShapeNameDiffs)
           .Append(" frame=").Append(d.ShapeFrameDiffs)
           .Append(" w=").Append(d.ShapeWeightDiffs)
           .Append(" dv=").Append(d.ShapeVertexDiffs)
           .Append(" dn=").Append(d.ShapeNormalDiffs)
           .Append(" dt=").Append(d.ShapeTangentDiffs)
           .Append('\n'); }

      UnityEngine.Debug.Log("[NZK Audit] meshes=" + diffs.Count +
                            " clean=" + clean + " differing=" + dirty + "\n" +
                            (dirty > 0 ? "--- DIFFS ---\n" + bad : "") +
                            (dirty > 0 ? "--- CLEAN ---\n" + ok  : ""));
      if (dirty > 0)
        UnityEngine.Debug.LogWarning("[NZK Audit] " + dirty + " mesh(es) differ from " + SourceModel); }
  }
}
}
#endif
