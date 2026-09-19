namespace NZK.NaNimate
{
public static partial class NZKNaNimateMeshGenerator {
public sealed class Result
    {
        public bool success;
        public string error;
        public string meshesFolder;
        public string prefabFolder;
        public string prefabPath;
        public int meshCount;
        public int rendererCount;
        public int swappedCount;
        public int unmatchedRenderers;
        public string unmatchedSample;
        /*
         * Bones that could not be mapped into the cloned hierarchy and so
         * still point at the SOURCE model's transforms.
         *
         * Tracked separately from unmatchedRenderers because
         * the two are unrelated failures and merging them makes a bone-rewire
         * problem look like a renderer-pairing problem. Non-zero here means
         * at least one mesh is driven by objects outside the prefab, so it
         * will not deform correctly.
         */
        public int unmappedBones;
        public string unmappedBoneSample;
    }
}
}
