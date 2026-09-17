namespace NZK
{
public static partial class Core {
public class SpsBoneEntry
  {
    public UnityEngine.Transform boneTransform;
    public System.String chainName;
    public float radius = 0.05f;
    public float stiffness = 0.2f;
    public float pull = 0.2f;
    public float grabMovement = 0.2f;
    public float maxAngleX = 45f;
    public float maxAngleZ = 45f;
    public float maxAngleY = 0f;
    public System.Boolean isGrabbable = true;
    public System.Boolean isPoseable = true;
  }
}
}
