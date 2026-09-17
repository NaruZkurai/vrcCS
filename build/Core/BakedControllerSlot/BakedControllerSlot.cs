namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Baked Controller Slot")]
  public class BakedControllerSlot : UnityEngine.MonoBehaviour
  {/* Which layer type this slot represents (Base,Additive,Gesture,Action,FX,Sitting,TPose,IKPose) */
    public VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType layerType;
    /* The controller assigned after baking — cloned/merged from VRCFury */
    public UnityEngine.RuntimeAnimatorController animatorController;
    /* Optional UnityEngine.Avatar mask for this layer */
    public UnityEngine.AvatarMask mask;
    /* Whether this slot uses the default VRChat controller (no custom override) */
    public System.Boolean isDefault = true;
    /* Whether this layer is enabled */
    public System.Boolean isEnabled = true;
    /* Slot index in the VRC descriptor's array (0-4 for base,0-2 for special) */
    public int slotIndex;
    /* Whether this is a base layer or special layer */
    public System.Boolean isSpecialLayer;
    /* ── System.IO.Path rewriting ──────────────────────────────────── */
    /* When enabled,animation clip paths are rewritten via the VRCFury */
    /* rewriteBindings rules during bake,offsetting paths from the */
    /* VRCFury object hierarchy to the actual UnityEngine.Avatar hierarchy. */
    public System.Boolean pathRewriteEnabled = true;
    /* ── Parameters ──────────────────────────────────────── */
    /* Expression parameters associated with this controller slot. */
    /* These are merged into the avatar's VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters during baking. */
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters parameters;
    /* Names of parameters that are "shared" (used by multiple controllers). */
    /* These won't be prefixed/deduplicated during merge. */
    public System.Collections.Generic.List<System.String> sharedParameterNames = new System.Collections.Generic.List<System.String>();
  }
}
}
