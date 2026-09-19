namespace NZK
{
public static partial class Core
{
/* NaNimate import pipeline hook.
 *
 * PLACEMENT: inside NZK.Core, matching every other toolkit type.  It used to
 * declare `namespace NZK.Core` directly, which is the SAME namespace the
 * generated build/ tree uses but a DIFFERENT assembly, and that produced the
 * CS0435/CS0234 pair when this file tried to reach NZK.Core.MeshImport: inside
 * `namespace NZK.Core`, the name `NZK.Core` resolves to the local namespace
 * before it resolves to the assembly type `Core`, so the path searched for a
 * namespace `Core` inside `NZK.Core` and found nothing.  Nested in `Core`, a
 * reference is just `MeshImport.MaxInfluences` and the shadowing cannot occur.
 *
 * NOTE ON HOOK ACCESSIBILITY: OnPreprocessModel / OnPostprocessModel are
 * discovered by Unity BY NAME and invoked through the AssetPostprocessor base.
 * They are `public` here to satisfy the no-private-members rule, which is safe
 * for this pair because Unity resolves them by reflection over the type's
 * methods; the visibility is not part of the contract.  If a future Unity
 * version stops calling them, suspect that first. */
public sealed class NZKNaNimateWeightImporter : UnityEditor.AssetPostprocessor
{
    public const string GroupName = "NaNimate";
    public const string MenuPath = "Tools/NZK/NaNimate/Reimport Selected Blend Assets";
    public const string NormalizeMenuPath = "Tools/NZK/NaNimate/Normalize NaNimate Weights On Selected";
    /* Import hook for NaNimate rigs.
     *
     * DIVISION OF RESPONSBILITY, deliberately strict:
     *
     *   THIS CLASS (import pipeline) - configure the import and update the
     *     armature. It NEVER creates assets. AssetDatabase.CreateAsset,
     *     CreateFolder and SaveAsPrefabAsset are all illegal here: this hook runs
     *     on an out-of-process import worker, and the follow-up
     *     OnPostprocessAllAssets still runs inside the import batch.
     *
     *   NZKNaNimateMeshGenerator (main thread, interactive) - writes
     *     Meshes/*.asset and Prefabs/*.prefab. Triggered ONLY by right-click ->
     *     Reimport with NaNimations, or Tools/NZK/NaNimate/Generate Now.
     */
    public static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        /* NO-OP BY DESIGN.
         *
         * This hook exists in the AssetPostprocessor contract, so it is declared
         * explicitly to document that nothing happens here. It previously drove a
         * deferred asset-creation flush, which could never succeed: the batch is
         * still importing, so AssetDatabase.CreateFolder refuses with
         * "CreateFolder is not supported while importing out-of-process" and
         * AssetDatabase.Refresh is a no-op. Asset creation belongs to the
         * generator, on the main thread, outside any import. */
    }
    /* When true, imported meshes have every NaNimate influence pinned to
     * exactly NZK.NaNimate.NZKNaNimateWeightNormalizer.NaNimateWeight (1e-07) and all other
     * influences rescaled proportionally so they still sum to 1.
     *
     * Stored in EditorPrefs so it survives domain reloads and is not confused
     * with per-asset import settings.
     */
    public const string NormalizePrefsKey = "NZK.NaNimate.NormalizeWeightsOnImport";
    public static bool NormalizeOnImport
    {
        get => UnityEditor.EditorPrefs.GetBool(NormalizePrefsKey, true);
        set
        {
            UnityEditor.EditorPrefs.SetBool(NormalizePrefsKey, value);
            UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Normalize-on-import " +
                                  (value ? "enabled." : "disabled.")));
        }
    }
    [UnityEditor.MenuItem("Tools/NZK/NaNimate/Normalize Weights On Import")]
    public static void ToggleNormalizeOnImport()
    {
        NormalizeOnImport = !NormalizeOnImport;
        UnityEditor.Menu.SetChecked(
            "Tools/NZK/NaNimate/Normalize Weights On Import", NormalizeOnImport);
    }
    [UnityEditor.MenuItem("Tools/NZK/NaNimate/Normalize Weights On Import", true)]
    public static bool ValidateToggleNormalizeOnImport()
    {
        UnityEditor.Menu.SetChecked(
            "Tools/NZK/NaNimate/Normalize Weights On Import", NormalizeOnImport);
        return true;
    }
    /* Four influences per vertex.
     *
     * A LITERAL, and the reason is assembly boundaries, not laziness.
     * MeshImport lives in the generated build/ tree and is compiled into
     * Assembly-CSharp; this file is in Assets/NZK toolkit v6/Editor/ and lands
     * in Assembly-CSharp-Editor. The two assemblies do not reference each
     * other, so `MeshImport.MaxInfluences` is CS0103 "does not exist in the
     * current context" no matter how the namespaces are arranged.
     *
     * Moving this file into source/ as a .cs.nzk is what would make it one
     * source of truth: it would then be generated into the same tree and the
     * reference would resolve as a plain sibling. Until that migration lands,
     * the value is restated here and MUST be kept equal to
     * NZK.Core.MeshImport.MaxInfluences. */
    public const int MaxInfluences = 4;

    public void OnPreprocessModel()
    {
        if (!NZK.S.EndsWithOIC(assetPath, ".blend"))
            return;
        UnityEditor.ModelImporter importer = (UnityEditor.ModelImporter)assetImporter;
        importer.minBoneWeight = 0f;
        /* FOUR influences, not 255.
         *
         * 255 here is "Unlimited", and it was the reason every fix applied
         * from outside this hook reverted on the next import: this hook runs on
         * EVERY model import and overwrote the cap before the mesh was built.
         * The Inspector would read Four Bones while maxBonesPerVertex wrote 255
         * back, and whichever pipeline ran last won.
         *
         * The 255 was never load-bearing. It was added as belt-and-braces
         * alongside skinWeights=Custom, on the assumption that Custom means
         * "no cap". Custom does NOT mean that: maxBonesPerVertex is still
         * honoured in Custom mode, which is the whole reason minBoneWeight
         * below works. Capping at 4 keeps the sub-threshold NaNimation group
         * (minBoneWeight = 0 preserves it) while giving VRChat the four
         * influences its skinning path is built around. */
        importer.maxBonesPerVertex = MaxInfluences;
        /* skinWeights must be Custom. In Standard, Unity ignores
         * maxBonesPerVertex/minBoneWeight and silently drops influences, which
         * unbinds the NaNimate bones so the skinned meshes render wrong or not
         * at all. Only Custom honours the weight settings above. */
        importer.skinWeights = UnityEditor.ModelImporterSkinWeights.Custom;
        /* Read/Write must be enabled or the imported meshes carry no CPU-side
         * vertex/weight data, and reading them to generate standalone Mesh assets
         * yields empty files. This is a read-only requirement: it only asks Unity
         * to keep a readable copy in memory, it does not modify the source asset. */
        importer.isReadable = true;
    }
    public void OnPostprocessModel(UnityEngine.GameObject root)
    {
        if (!NZK.S.EndsWithOIC(assetPath, ".blend"))
            return;
        int meshCount = 0;
        int nanimateMeshCount = 0;
        UnityEngine.SkinnedMeshRenderer[] renderers =
            root.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
        /* POST-PROCESS ONLY. No asset creation happens here, and nothing in this
         * method touches the AssetDatabase. The armature is validated below and
         * the mesh/prefab output is produced later, on the main thread, by
         * NZKNaNimateMeshGenerator.GenerateNow when the user asks for it. */
        for (int r = 0; r < renderers.Length; r++)
        {
            UnityEngine.SkinnedMeshRenderer renderer = renderers[r];
            meshCount++;
            if (!HasNaNimateBone(renderer))
                continue;
            nanimateMeshCount++;
            if (!NormalizeOnImport)
            {
                UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Preserved sub-threshold weights on " +
                                      renderer.name + " (normalization disabled)."));
                continue;
            }
            /* REPORT ONLY - never rewrite the imported mesh.
             *
             * Swapping a normalised mesh in via renderer.sharedMesh mutated the
             * model THIS PROJECT RENDERS FROM, leaving a scene instance holding
             * meshes that ceased to exist once the import was discarded. The
             * source model and its .meta are strictly read-only; any rewritten
             * mesh is written out as a separate asset by the generator instead. */
            NZK.NaNimate.NZKNaNimateWeightNormalizer.Result result =
                new NZK.NaNimate.NZKNaNimateWeightNormalizer.Result();
            string[] boneNames = NZK.NaNimate.NZKNaNimateWeightNormalizer.BoneNamesOf(renderer);
            UnityEngine.Mesh mesh = renderer.sharedMesh;
            if (mesh == null)
                continue;
            /* Analyse without applying: pass a copy so the original cannot be
             * touched even if the normalizer mutates its input. */
            UnityEngine.Mesh probe = UnityEngine.Object.Instantiate(mesh);
            try
            {
                NZK.NaNimate.NZKNaNimateWeightNormalizer.Normalize(probe, boneNames, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(probe);
            }
            UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Would pin " + result.verticesAffected +
                                  " vertex/vertices to " + NZK.NaNimate.NZKNaNimateWeightNormalizer.NaNimateWeight +
                                  " on " + renderer.name +
                                  " (rescaled " + result.verticesRescaled +
                                  ", worst deviation " + result.worstDeviation.ToString("E2") +
                                  ") from " + assetPath + " [source left untouched]"));
        }
        UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Processed " + meshCount +
                              " skinned mesh(es), including " + nanimateMeshCount +
                              " NaNimate mesh(es), from " + assetPath));
    }
    public static bool HasNaNimateBone(UnityEngine.SkinnedMeshRenderer renderer)
    {
        UnityEngine.Transform[] bones = renderer.bones;
        if (bones == null)
            return false;
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null &&
                NZK.NaNimate.NZKNaNimateWeightNormalizer.IsNaNimateName(bones[i].name))
            {
                return true;
            }
        }
        return false;
    }
    /* Rewrite NaNimate weights on the selected blend assets without a full
     * reimport. Useful when normalization is toggled on after import.
     */
    [UnityEditor.MenuItem(NormalizeMenuPath, true)]
    public static bool ValidateNormalizeSelected()
    {
        return SelectedBlendAssetPaths().Length > 0;
    }
    [UnityEditor.MenuItem(NormalizeMenuPath)]
    public static void NormalizeSelected()
    {
        string[] paths = SelectedBlendAssetPaths();
        int totalMeshes = 0;
        long totalVerts = 0;
        for (int p = 0; p < paths.Length; p++)
        {
            string path = paths[p];
            string folder = System.IO.Path.GetDirectoryName(path)
                .Replace('\\', '/');
            NZK.NaNimate.NZKNaNimateWeightNormalizer.Result result =
                NZK.NaNimate.NZKNaNimateWeightNormalizer.NormalizeModel(
                    path, NZK.S.P.C(folder, NZK.S.P.Next(path) + ".meshes"));
            if (!result.success)
            {
                UnityEngine.Debug.LogError(NZK.S.NZKNaNimatePrefix("Normalize failed for " +
                                           path + ": " + result.error));
                continue;
            }
            totalMeshes += result.meshesWritten;
            totalVerts += result.verticesAffected;
            UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Normalized " + result.meshesWritten +
                                  " mesh(es) from " + path + ": " +
                                  result.verticesAffected + " vertex/vertices pinned, " +
                                  result.verticesRescaled + " rescaled, worst deviation " +
                                  result.worstDeviation.ToString("E2")));
        }
        UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Normalize complete: " + totalMeshes +
                              " mesh(es), " + totalVerts + " vertex/vertices pinned."));
    }
    [UnityEditor.MenuItem(MenuPath, true)]
    public static bool ValidateReimportSelectedBlendAssets()
    {
        foreach (string path in SelectedBlendAssetPaths())
            return true;
        return false;
    }
    [UnityEditor.MenuItem(MenuPath)]
    public static void ReimportSelectedBlendAssets()
    {
        string[] paths = SelectedBlendAssetPaths();
        foreach (string path in paths)
        {
            UnityEditor.ModelImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.ModelImporter;
            if (importer == null)
                continue;
            importer.minBoneWeight = 0f;
            importer.maxBonesPerVertex = MaxInfluences;
            importer.SaveAndReimport();
        }
        UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Reimported " + paths.Length + " selected Blend asset(s) with minBoneWeight = 0."));
    }
    /* Project-relative paths of the selected assets that are .blend files,
     * de-duplicated. Shared by every menu entry point above so the selection
     * rule (filter to .blend, drop repeats) is written once.
     */
    public static string[] SelectedBlendAssetPaths()
    {
        UnityEngine.Object[] selected =
            UnityEditor.Selection.GetFiltered<UnityEngine.Object>(UnityEditor.SelectionMode.Assets);
        System.Collections.Generic.List<string> paths =
            new System.Collections.Generic.List<string>();
        System.Collections.Generic.HashSet<string> seen =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < selected.Length; i++)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(selected[i]);
            if (NZK.B.NoE(path))
                continue;
            if (!NZK.S.EndsWithOIC(path, ".blend"))
                continue;
            if (seen.Add(path))
                paths.Add(path);
        }
        return paths.ToArray();
    }
}
}
}
