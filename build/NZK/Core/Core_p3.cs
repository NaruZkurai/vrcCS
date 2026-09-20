#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
  /** <summary>Scene marker: the nanimation bones for the avatar in this scene.
   *
   *  One per scene, at the root.  See the file header for why the placement
   *  and active-state rules exist.</summary> */
  
  /** <summary>Inspector: the Bones list, a Run button, and the reason it will
   *  not run when there is one.
   *
   *  The reason is shown IN the inspector rather than only logged, because the
   *  failure mode this whole component exists to remove is "I clicked it and
   *  nothing happened, with no idea why".</summary> */
  
}
}
#endif
