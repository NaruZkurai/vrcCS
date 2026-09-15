namespace NZK
{
public static partial class Core {
public partial class C_BoneAssignment {
/* ── Slot definition ────────────────────────────────────── */
    /* Which human bone does THIS UnityEngine.Transform represent? */
    /* Uses the same indexing as UnityEngine.HumanBodyBones (0-24) plus extra slots. */
    
    public E_BoneSlot slot = C_BoneAssignment.E_BoneSlot.None;
    /* ── Assignment metadata ────────────────────────────────── */
    /* The human-readable name of the slot (e.g. "LeftUpperArm") */
    public System.String slotName = "";
    /* Whether this bone was confidently matched during UnityEngine.Avatar build */
    public System.Boolean isConfirmed;
    /* The UnityEngine.Avatar that was generated from this armature */
    public UnityEngine.Avatar generatedAvatar;
    /* The armature this bone belongs to (Armature_Armature or Armature_Stripped) */
    public System.String armatureType = "";
    /* ── Bone's world-space UnityEngine.Transform at bake time (for verification) ── */
    public UnityEngine.Vector3 bakePosition;
    public UnityEngine.Quaternion bakeRotation;
    public UnityEngine.Vector3 bakeScale;
    /* ── Helper ─────────────────────────────────────────────── */
    public void RecordBakeTransform(UnityEngine.Transform t)
    { bakePosition = t.position; bakeRotation = t.rotation; bakeScale = t.lossyScale; }
    public static System.String SlotToHumanName(E_BoneSlot slot)
    { if (slot == E_BoneSlot.None) return "None"; if (slot == E_BoneSlot.Extra) return "Extra"; return slot.ToString(); }
    public static E_BoneSlot HumanNameToSlot(System.String humanName)
    {
      if (System.String.IsNullOrEmpty(humanName)) return C_BoneAssignment.E_BoneSlot.None;
      if (System.Enum.TryParse<C_BoneAssignment.E_BoneSlot>(humanName, true, out var result)) return result;
      return C_BoneAssignment.E_BoneSlot.Extra;
    }
  
}
}
}
