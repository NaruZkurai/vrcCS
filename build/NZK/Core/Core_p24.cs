#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
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
}
}
#endif
