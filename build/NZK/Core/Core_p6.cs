namespace NZK
{
public static partial class Core {
#if UNITY_EDITOR
#endif
/* Expected User flow *\
* multiselect two objects right click, merge with children
* - unpacks first selected objects (if required),
* - replaces all references to target object's in the scene with the current ones
* - if there is any animations that target  that object specifically it duplicates thoes animations and regargets them
* - all script components get coppied as new, then updated, if theres a conflict it should ask the user to override or add as a new component with references. (should still turn all references to that object regardless)
*
* Select any object with VF sockets, right-click → NZK → SPS → Convert VRCFury Sockets to NZK, and each one gets:
*
*
* Component converted (VF → C_NzkSpsSocket with all serialized data)
* Generator created/connected (GEN_{avatar} → SPS GENERATOR)
* Config created under the generator
* Baked hierarchy + prefab generated
* FX controller merged if has linked controller
*
*
*
*
*
*
*
*
*
*/
}
}
