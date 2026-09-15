namespace NZK
{
public static partial class Core {
public static class HBChildren
  {
    public const System.String Prefix = "HB_";
    public const System.String LayerPrefix = "LYR_";
    public const System.String StateMachinePrefix = "sm_";
    public const System.String MeshBuilder = "Mesh Output";
    public const System.String MeshGeneratorRoot = "Mesh Generator Root";
    public const System.String ArmatureBuilder = "Armature Builder";
    public const System.String AviRootBuilder = "Avatar Root Builder";
    public const System.String GestureGenerator = "Gesture";
    public const System.String AnimatorBuilder = "Animator Builder";
    public const System.String ExpressionsGenerator = "Expressions";
    public const System.String MenuGenerator = "Menu";
    public const System.String FxGenerator = "FX";
    public const System.String AnimationsGenerator = "Animations";
    public const System.String ToggleGenerator = "Toggle Generator";
    public const System.String MeshSplitter = "Mesh Splitter";
    public const System.String SpsBuilder = "SPS Builder";
    public const System.String ArmatureLinks = "ArmatureLinks";
    public const System.String BakedControllers = "BakedControllers";
    public const System.String Sources = "Sources";
    public const System.String Toggles = "Toggles";
    public const System.String Generated = "Generated";
    public const System.String AnimPrefix = "anim_";
    public const System.String VisemePrefix = "viseme_";
    public const System.String VisemePrefixUpper = "Viseme_";
    public const System.String SubStatePrefix = "sst_";
    public const System.String TransitionPrefix = "tr_";
    public static System.String[] ChildObjects => new System.String[] { MeshGeneratorRoot, ArmatureBuilder, AviRootBuilder, GestureGenerator, ExpressionsGenerator, FxGenerator, ToggleGenerator, MeshSplitter, SpsBuilder };
  }
}
}
