namespace NZK
{
public static partial class Core {
[System.Serializable]
  public struct HBFlagEntry
  {
    public System.String propertyName; // e.g. "m_WriteDefaultValues"
    public System.String S_flagValue;  // serialized as System.String for YAML compat
    public E_HBFlagScope scope;
    public System.String layerName;  // filters to layer when scope=PerLayer,empty=all
  }
}
}
