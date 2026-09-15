namespace NZK
{
public static partial class Core {
[System.Serializable]
  public class GestureCondition
  {
    public System.String parameter;               /* Parameter name (GestureLeft,GestureRight...) */
    public int mode;                  /* UnityEditor.Animations.AnimatorConditionMode as int: 1=If,2=IfNot,3=Greater,4=Less,6=Equals,7=NotEqual */
    public float threshold;
  }
}
}
