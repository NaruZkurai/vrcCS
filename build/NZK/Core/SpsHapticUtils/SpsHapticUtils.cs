#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class SpsHapticUtils {
 public const System.String CONTACT_PEN_MAIN = "TPS_Pen_Penetrating";
    public const System.String CONTACT_PEN_WIDTH = "TPS_Pen_Width";
    public const System.String CONTACT_PEN_CLOSE = "TPS_Pen_Close";
    public const System.String CONTACT_PEN_ROOT = "TPS_Pen_Root";
    public const System.String TagTpsOrfRoot = "TPS_Orf_Root";
    public const System.String TagTpsOrfFront = "TPS_Orf_Norm";
    public const System.String TagSpsSocketRoot = "SPSLL_Socket_Root";
    public const System.String TagSpsSocketFront = "SPSLL_Socket_Front";
    public const System.String TagSpsSocketIsRing = "SPSLL_Socket_Ring";
    public const System.String TagSpsSocketIsHole = "SPSLL_Socket_Hole";
    public static readonly System.String[] SelfContacts = { "Hand","Finger","Foot" };
    public static readonly System.String[] BodyContacts = { "Head","Hand","Foot","Finger" };
    static readonly System.Random rand = new();
    
    public static System.String RandomTag() => "TPSVF_" + rand.Next(100_000_000,999_999_999); 
}
}
}
#endif
