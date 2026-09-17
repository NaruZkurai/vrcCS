/*strings: Error Dialogue
 * usage NZK.S.E1
 *  NZK.S.E2
 *  NZK.S.E1
*/
/*add error code to output class*/
namespace NZK
{ public static partial class E

 { 
  /*default 0 = success*/
   public static string rr0 = "success";
   /**/
   public static string rr1= "ex+21";
   public static string rr2= "ex+21";
   public static string rr3= "ex+22";
   public static string rr4= "ex+3";
   public static string rr6= "ex+5";
   public static string rr5= "ex+5";
   public static string rr7= "ex+6";
   public static string rr8= "ex+7";
   public static string rr9= "ex+8";
   /* selected states */
   public static string rr10= "Nothing Selected";
   public static string rr11= "Empty";
public static string rr12<T>(T u){ string type = u?.GetType().ToString() ?? typeof(T).ToString(); return $"Unexpected Selection: {type}\n{u}";}
public static string rr13<T>(T u){ string type = u?.GetType().ToString() ?? typeof(T).ToString(); return $"Exoected Selection: {type}\n{u}";}
   /* generic guard states */
   public static string rr14= "Operation Cancelled";
   public static string rr15= "Invalid State";
   public static string rr16= "Value Out Of Range";
   public static string rr17= "Type Mismatch";
   public static string rr18= "Missing Component";
   public static string rr19= "Not Supported";
   /* generic object states - parameterized, pass the offending object */
public static string rr20<T>(T u){ return $"Is not an asset: {Nm(u)}";}
public static string rr21<T>(T u){ return $"Is not a valid type: {Nm(u)}";}
public static string rr22<T>(T u){ return $"Is read-only: {Nm(u)}";}
public static string rr23<T>(T u){ return $"Is missing: {Nm(u)}";}
public static string rr24<T>(T u){ return $"Not found on disk: {Nm(u)}";}
public static string rr25<T>(T u){ return $"Could not be read: {Nm(u)}";}
public static string rr26<T>(T u){ return $"Is malformed: {Nm(u)}";}
public static string rr27<T>(T u){ return $"Is corrupted: {Nm(u)}";}
public static string rr28<T>(T u){ return $"Failed to import: {Nm(u)}";}
public static string rr29<T>(T u){ return $"Is already valid: {Nm(u)}";}
public static string rr30<T>(T u){ return $"Has no data: {Nm(u)}";}
public static string rr31<T>(T u){ return $"Path is empty: {Nm(u)}";}
public static string rr32<T>(T u){ return $"Is in an immutable package: {Nm(u)}";}
public static string rr33<T>(T u){ return $"Is in a read-only folder: {Nm(u)}";}
public static string rr34<T>(T u){ return $"Write permission denied: {Nm(u)}";}
public static string rr35<T>(T u){ return $"Version mismatch: {Nm(u)}";}
public static string rr36<T>(T u){ return $"Is not supported: {Nm(u)}";}
public static string rr37<T>(T u){ return $"Is out of range: {Nm(u)}";}
public static string rr38<T>(T u){ return $"Is constant zero: {Nm(u)}";}
public static string rr39<T>(T u){ return $"Has no keyframes: {Nm(u)}";}
public static string rr40<T>(T u){ return $"Is locked by another process: {Nm(u)}";}
public static string rr41<T>(T u){ return $"Has no component of that type: {Nm(u)}";}
public static string rr42<T>(T u){ return $"Belongs to a different scene: {Nm(u)}";}
public static string rr43<T>(T u){ return $"Is a prefab asset: {Nm(u)}";}
public static string rr44<T>(T u){ return $"Is a folder, not a file: {Nm(u)}";}
public static string rr45<T>(T u){ return $"Has no selection: {Nm(u)}";}
public static string rr46<T>(T u){ return $"Contains multiple objects: {Nm(u)}";}
public static string rr47<T>(T u){ return $"Operation failed: {Nm(u)}";}
public static string rr48<T>(T u){ return $"Operation not permitted: {Nm(u)}";}
public static string rr49<T>(T u){ return $"Backup is missing: {Nm(u)}";}
public static string rr50<T>(T u){ return $"Unknown error: {Nm(u)}";}
public static string rr51= "51";
   /* name helper for parameterized codes above */
public static string Nm<T>(T u){ return u == null ? typeof(T).Name : u.ToString();}
    /*--------- Success (2xx) ----------*/
    public static string rr200  = "200 OK";
    public static string rr201  = "201 Created";
    public static string rr202  = "202 Accepted";
    public static string rr204  = "204 No Content";

    /*---------- Redirection (3xx) ----------*/
    public static string rr301  = "301 Moved Permanently";
    public static string rr302  = "302 Found";
    public static string rr303  = "304 Not Modified";

    /*---------- Client Errors (4xx) ----------*/
    public static string rr400 = "400 Bad Request";
    public static string rr401 = "401 Unauthorized";
    public static string rr403 = "403 Forbidden";
    public static string rr404 = "404 Not Found";
    public static string rr429 = "429 Too Many Requests";

    /*---------- Server Errors (5xx) ----------*/
    public static string rr500 = "500 Internal Server Error";
    public static string rr502 = "502 Bad Gateway";
    public static string rr503 = "503 Service Unavailable";
    public static string rr504 = "504 Gateway Timeout";
}}