#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NZKNaNimateWeightNormalizer {
public sealed class Result{
        public bool success;
        public string error;
        public int meshesProcessed;
        public int meshesWritten;
        public int renderersProcessed;
        /* Vertices where at least one NaNimate influence was pinned. */
        public long verticesAffected;
        /* Vertices whose ordinary weights were rescaled. */
        public long verticesRescaled;
        /* Vertices holding only NaNimate influences (cannot sum to 1). */
        public long verticesNaNimateOnly;
        /* Bones referenced by weights but absent from the name list. */
        public long outOfRangeBoneRefs;
        /*
         * Rescaled weights that came out NaN/Infinity/negative and were
         * zeroed. Non-zero here means the input mesh had degenerate weights;
         * Unity would otherwise report "Invalid AABB".
         */
        public long nonFiniteScales;
        /* Largest observed |sum(weights) - 1| after rewriting. */
        public float worstDeviation;
        public long approxBytesBefore;
        public long approxBytesAfter;
        public bool HasWarning=>NZK.B.Oll4(verticesNaNimateOnly>0,outOfRangeBoneRefs>0,nonFiniteScales>0,worstDeviation>1e-3f);
    }
}
}
}
#endif
