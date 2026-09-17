#if false
namespace NZK
{
public static partial class Core {
public partial class BakedContact {
[System.Serializable]
  public class ContactEntry
  { public System.String rootPath;      // System.IO.Path to the root transform
    public System.String type;        // "Sender" or "Receiver"
    public System.String parameter;     // The animation parameter name
    public float radius;       // Contact radius
    public System.String[] collisionTags;   // Tags for collision filtering
  }
}
}
}
#endif
