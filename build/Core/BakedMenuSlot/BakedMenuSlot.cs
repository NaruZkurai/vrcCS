namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Baked Menu Slot")]
  public class BakedMenuSlot : UnityEngine.MonoBehaviour
  {/* The baked sub-menu asset (holds up to 8 controls) */
    public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu;
    /* Page number: 0 = Menu (primary),1 = Next,2 = Next2,etc. */
    public int pageNumber;
    /* Whether this is the root menu assigned to VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.expressionsMenu */
    public System.Boolean isRoot;
    /* Number of controls in this menu (for display purposes) */
    public int controlCount;
    /* If true,use the existing menu asset to populate proxy children */
    /* without cloning. Changes to the source menu are reflected immediately. */
    public System.Boolean useExistingMenu;
    /* Ordered array of child ProxyMenuSlot objects under this page. */
    /* Updated by the Regen Children / Update Order buttons. */
    public System.Collections.Generic.List<ProxyMenuSlot> childSlots = new();
  }
}
}
