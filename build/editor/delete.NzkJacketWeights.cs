namespace NZK.NaNimate
{
    /// <summary>
    /// Reports the NaNimate-bone skin weight on a named imported mesh.
    ///
    /// Originally written against a "UnityCliConnector" attribute framework,
    /// which does not exist in this project: Assets/NZK toolkit v4/_Folders/
    /// UnityCliConnector contains asmdefs and empty folders but zero .cs files.
    /// The dependency is therefore dropped entirely and the diagnostics run from
    /// a menu item instead.
    ///
    /// No using directives by design - every type is fully qualified.
    /// </summary>
    public static class NzkJacketWeights
    {
        const string DefaultObjectName = "TJacket - Leather Jacket";

        /// <summary>Human-readable report, also returned so callers can log it.</summary>
        public sealed class Report
        {
            public int matchedObjects;
            public System.Text.StringBuilder text = new System.Text.StringBuilder();
        }

        [UnityEditor.MenuItem("Tools/NZK/NaNimate/Report Jacket NaNimate Weights")]
        static void ReportFromMenu()
        {
            UnityEngine.GameObject selected =
                UnityEditor.Selection.activeGameObject;

            string target = selected != null ? selected.name : DefaultObjectName;
            Report report = Inspect(target);

            UnityEngine.Debug.Log(report.text.ToString());
        }

        /// <summary>
        /// Inspect every SkinnedMeshRenderer whose GameObject name matches
        /// <paramref name="objectName"/> and report its NaNimate binding.
        /// </summary>
        public static Report Inspect(string objectName)
        {
            Report report = new Report();

            if (string.IsNullOrEmpty(objectName))
                objectName = DefaultObjectName;

            UnityEngine.SkinnedMeshRenderer[] all =
                UnityEngine.Object.FindObjectsOfType<UnityEngine.SkinnedMeshRenderer>(true);

            System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer> matches =
                new System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>();

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].gameObject.name == objectName)
                    matches.Add(all[i]);
            }

            report.matchedObjects = matches.Count;
            report.text.Append("[NZK JacketWeights] target='").Append(objectName)
                       .Append("'  matches=").Append(matches.Count).Append('\n');

            if (matches.Count == 0)
            {
                // Listing near-miss names is the fastest way to find the real one.
                report.text.Append("  no exact match. Skinned meshes present:\n");
                for (int i = 0; i < all.Length && i < 40; i++)
                {
                    if (all[i] != null)
                        report.text.Append("    - ").Append(all[i].gameObject.name).Append('\n');
                }

                return report;
            }

            for (int m = 0; m < matches.Count; m++)
                InspectOne(matches[m], report);

            return report;
        }

        static void InspectOne(
            UnityEngine.SkinnedMeshRenderer smr, Report report)
        {
            report.text.Append('\n');
            report.text.Append("  object: ").Append(smr.gameObject.name).Append('\n');
            report.text.Append("  activeInHierarchy: ").Append(smr.gameObject.activeInHierarchy).Append('\n');
            report.text.Append("  enabled: ").Append(smr.enabled).Append('\n');

            UnityEngine.Mesh mesh = smr.sharedMesh;
            report.text.Append("  meshNull: ").Append(mesh == null).Append('\n');

            UnityEngine.Transform rootBone = smr.rootBone;
            report.text.Append("  rootBone: ")
                       .Append(rootBone != null ? rootBone.name : "<null>").Append('\n');

            UnityEngine.Transform[] bones = smr.bones;
            report.text.Append("  bonesCount: ").Append(bones != null ? bones.Length : 0).Append('\n');

            if (mesh == null)
                return;

            report.text.Append("  meshName: ").Append(mesh.name).Append('\n');
            report.text.Append("  vertexCount: ").Append(mesh.vertexCount).Append('\n');
            report.text.Append("  blendShapeCount: ").Append(mesh.blendShapeCount).Append('\n');

            // Which bone slots in this renderer are NaNimate groups?
            System.Collections.Generic.List<int> nanimateSlots =
                new System.Collections.Generic.List<int>();

            if (bones != null)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (bones[i] != null &&
                        NZKNaNimateWeightNormalizer.IsNaNimateName(bones[i].name))
                    {
                        nanimateSlots.Add(i);
                    }
                }
            }

            report.text.Append("  nanimateBoneSlots: ").Append(nanimateSlots.Count).Append('\n');

            UnityEngine.BoneWeight[] bw = mesh.boneWeights;
            if (bw == null || bw.Length == 0 || nanimateSlots.Count == 0)
            {
                report.text.Append("  vertsBoundToNaNimateBone: 0\n");
                return;
            }

            long boundVerts = 0;
            float minWeight = float.MaxValue;
            float maxWeight = float.MinValue;
            int negative = 0;
            int samplesTaken = 0;

            for (int v = 0; v < bw.Length; v++)
            {
                UnityEngine.BoneWeight b = bw[v];
                float w = WeightOf(b, nanimateSlots);

                if (w < 0f)
                    continue;

                boundVerts++;
                if (w < minWeight) minWeight = w;
                if (w > maxWeight) maxWeight = w;
                if (w < 0f) negative++;

                if (samplesTaken < 10)
                {
                    report.text.Append("    sample v=").Append(v)
                               .Append(" w=").Append(w).Append('\n');
                    samplesTaken++;
                }
            }

            report.text.Append("  vertsBoundToNaNimateBone: ").Append(boundVerts).Append('\n');

            if (boundVerts > 0)
            {
                report.text.Append("  nanimateWeightMin: ").Append(minWeight).Append('\n');
                report.text.Append("  nanimateWeightMax: ").Append(maxWeight).Append('\n');
                report.text.Append("  nanimateWeightNegative: ").Append(negative).Append('\n');
            }
        }

        /// <summary>
        /// Total weight this vertex places on any NaNimate slot, or -1 when the
        /// vertex touches none.
        /// </summary>
        static float WeightOf(
            UnityEngine.BoneWeight b, System.Collections.Generic.List<int> nanimateSlots)
        {
            float sum = -1f;

            for (int s = 0; s < nanimateSlots.Count; s++)
            {
                sum += SlotWeight(b, nanimateSlots[s]);
            }

            return sum < 0f ? -1f : sum;
        }

        static float SlotWeight(UnityEngine.BoneWeight b, int slot)
        {
            if (b.boneIndex0 == slot) return b.weight0;
            if (b.boneIndex1 == slot) return b.weight1;
            if (b.boneIndex2 == slot) return b.weight2;
            if (b.boneIndex3 == slot) return b.weight3;
            return 0f;
        }
    }
}
