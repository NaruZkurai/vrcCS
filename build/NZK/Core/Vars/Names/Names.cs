#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Vars {
public static partial class Names {
 public static System.String Sanitize(System.String value) => System.String.IsNullOrEmpty(value) ? "Unnamed" : value.Replace('/','_').Replace('\\','_').Trim();
  
   
}
}
}
}
#endif
