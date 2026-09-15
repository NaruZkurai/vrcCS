namespace NZK
{
public static partial class Core {
public partial class C_ArmatureLinkLog {
    
    public TDestinationMode destinationMode = TDestinationMode.Object;
    public UnityEngine.GameObject targetObject;  // the propBone that was moved
    /* Used only when destinationMode == UnityEngine.Object */
    public UnityEngine.Transform targetParent;   // the bone/transform it was parented to
    /* Used only when destinationMode == AvatarDefinition */
    public int targetBoneIndex = -1; // index into UnityEngine.HumanBodyBones (0-24)
    /* Used only when destinationMode == System.IO.Path */
    public System.String targetPath;    // cached path for System.String-based lookups
  
}
}
}
