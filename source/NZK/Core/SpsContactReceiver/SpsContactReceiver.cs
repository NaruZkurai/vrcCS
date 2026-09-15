namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class SpsContactReceiver
  {
    public System.String receiverName;
    public UnityEngine.Transform rootTransform;
    public float radius = 0.1f;
    public System.String parameter;
    public System.Boolean allowSelf = true;
    public System.Boolean allowOthers = true;
    public System.String[] collisionTags;
  }
}
}
