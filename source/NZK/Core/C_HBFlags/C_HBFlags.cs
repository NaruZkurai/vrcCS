namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Yaml/HB Flags")]
  public class C_HBFlags : UnityEngine.MonoBehaviour
  {
    public System.Collections.Generic.List<HBFlagEntry> flags = new();
    public E_HBFlagScope defaultScope = E_HBFlagScope.PerBlock;
    /** <summary>Resolve effective value for a property. */
    /** Priority: Global → PerLayer(matching) → PerBlock → null (no override).</summary> */
    public System.String S_Resolve(System.String propertyName, System.String layerName = "")
    { // Global scope first
      foreach (var f in flags)
        if (f.propertyName == propertyName && f.scope == E_HBFlagScope.Global)
          return f.S_flagValue;
      /* PerLayer scope — match on layerName if non-empty */
      foreach (var f in flags)
        if (f.propertyName == propertyName && f.scope == E_HBFlagScope.PerLayer
          && (System.String.IsNullOrEmpty(f.layerName) || f.layerName == layerName))
          return f.S_flagValue;
      /* PerBlock scope — only if specifically set (returns null if not found) */
      foreach (var f in flags)
        if (f.propertyName == propertyName && f.scope == E_HBFlagScope.PerBlock)
          return f.S_flagValue;
      return null; // no override
    }
    /** <summary>Set or update a flag entry.</summary> */
    public void V_SetFlag(System.String propertyName, System.String value, E_HBFlagScope scope, System.String layerName = "")
    { // Remove existing entry for same property+scope+layer
      flags.RemoveAll(f => f.propertyName == propertyName && f.scope == scope
      && (System.String.IsNullOrEmpty(layerName) || f.layerName == layerName));
      flags.Add(new HBFlagEntry
      { propertyName = propertyName, S_flagValue = value, scope = scope, layerName = layerName });
    }
    /** <summary>Remove all flags for a property.</summary> */
    public void V_ClearFlags(System.String propertyName)
    { flags.RemoveAll(f => f.propertyName == propertyName); }
    /** <summary>Remove all flags.</summary> */
    public void V_ClearFlags()
    { flags.Clear(); }
    /** <summary>Get all known YAML System.Boolean property names that can be flagged.</summary> */
    public static readonly System.String[] S_KnownBoolFlags = {
    "m_WriteDefaultValues","m_HasExitTime","m_HasFixedDuration","m_CanTransitionToSelf","m_IKOnFeet","m_LoopTime","m_LoopBlend","m_LoopBlendOrientation","m_LoopBlendPositionXZ","m_LoopBlendPositionY","m_KeepOriginalPositionXZ","m_KeepOriginalPositionY","m_KeepOriginalOrientation","m_Mirror","m_IsActive"
  };
    /** <summary>Get all known YAML float property names that can be flagged.</summary> */
    public static readonly System.String[] S_KnownFloatFlags = {
    "m_Speed","m_CycleOffset","m_Duration","m_ExitTime","m_TransitionDuration","m_TransitionOffset","m_EventTreshold","m_DefaultFloat","m_Weight","m_DefaultWeight"
  };
  }
}
}
