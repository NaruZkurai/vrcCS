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
   /* mesh-merge family - same generic-object shape as rr20..rr50, so a merge
      failure reads as title "<state>" + message "<offending object>". */
public static string rr52<T>(T u){ return $"Has no mesh: {Nm(u)}";}
public static string rr53<T>(T u){ return $"Has no submeshes: {Nm(u)}";}
public static string rr54<T>(T u){ return $"Has no bones to bind: {Nm(u)}";}
public static string rr55<T>(T u){ return $"Failed to merge: {Nm(u)}";}
public static string rr56<T>(T u){ return $"Has no avatar root to merge into: {Nm(u)}";}
public static string rr57<T>(T u){ return $"Was merged, sources parked: {Nm(u)}";}
   /* post-process family - one code per stage, so the console line names WHICH
      stage died instead of a generic "failed". */
public static string rr58<T>(T u){ return $"Weight repair failed: {Nm(u)}";}
public static string rr59<T>(T u){ return $"Clip fixup failed: {Nm(u)}";}
public static string rr60<T>(T u){ return $"Rewrote zero->NaN scale in {Nm(u)} clip(s)";}
public static string rr61<T>(T u){ return $"Repaired {Nm(u)} vertex/vertices";}
   /* object-merge family.  One code per OUTCOME, because the reported bug was
      that a component move looked fine while the reference rewrite silently
      dropped every reference - the two halves need separate codes to be told
      apart in the log. */
public static string rr62<T>(T u){ return $"Nothing to merge: {Nm(u)}";}
public static string rr63<T>(T u){ return $"Target is not a valid merge destination: {Nm(u)}";}
public static string rr64<T>(T u){ return $"Could not be moved onto the target: {Nm(u)}";}
public static string rr65<T>(T u){ return $"Merged {Nm(u)}";}
public static string rr66<T>(T u){ return $"Unpacked prefab instance: {Nm(u)}";}
   /* mesh-import family - the model's skin-weight cap.  4 influences is what
      an FBX imports as "Unlimited", and the SDK reserialises a mesh carrying
      them into something the client cannot skin: the avatar uploads invisible.
      4 is BoneWeight's own slot count, so nothing usable is lost by capping. */
public static string rr67<T>(T u){ return $"Importer has no skin-weight setting: {Nm(u)}";}
public static string rr68<T>(T u){ return $"Mesh import influences set to 4: {Nm(u)}";}
   /* upload-hook family - the pre/post-process switch, so a bisect run is
      visible in the log and an upload cannot be silently un-repaired. */
public static string rr69<T>(T u){ return $"Upload hook DISABLED for '{Nm(u)}' phase - avatar uploaded unmodified";}
public static string rr70<T>(T u){ return $"Upload hook pre={Nm(u)}";}
   /* relink family - a weight mapping that could not resolve names, which is
      the failure that used to collapse every unmatched influence onto bone 0
      (drag the mesh to the root) instead of reporting a skeleton mismatch. */
public static string rr71<T>(T u){ return $"No shared bone names between source and scene: {Nm(u)}";}
public static string rr72<T>(T u){ return $"Unmapped influences: {Nm(u)}";}
public static string rr73<T>(T u){ return $"Upload hook RUNNING at {Nm(u)}";}
   /* mesh-audit family - READ-ONLY diagnostics.  Each names a condition that
      makes a mesh invisible while the hierarchy still looks perfect, so the
      reader gets a fact instead of a theory. */
public static string rr74<T>(T u){ return $"Audit root: {Nm(u)}";}
public static string rr75<T>(T u){ return $"No SkinnedMeshRenderer under: {Nm(u)}";}
public static string rr76<T>(T u){ return $"Renderer or GameObject DISABLED: {Nm(u)}";}
public static string rr77<T>(T u){ return $"sharedMesh is NULL: {Nm(u)}";}
public static string rr78<T>(T u){ return $"EMPTY mesh: {Nm(u)}";}
public static string rr79<T>(T u){ return $"BOUNDS are NaN/Infinite: {Nm(u)}";}
public static string rr80<T>(T u){ return $"BOUNDS are zero size: {Nm(u)}";}
public static string rr81<T>(T u){ return $"Null material slot or shader: {Nm(u)}";}
public static string rr82<T>(T u){ return $"Mesh not readable, weights unchecked: {Nm(u)}";}
public static string rr83<T>(T u){ return $"NaN vertex data: {Nm(u)}";}
public static string rr84<T>(T u){ return $"Vertices with no weight: {Nm(u)}";}
public static string rr85<T>(T u){ return $"Vertices with more than 4 influences: {Nm(u)}";}
public static string rr86<T>(T u){ return $"Mesh: {Nm(u)}";}
public static string rr87<T>(T u){ return $"NaN SCALE on transform: {Nm(u)}";}
public static string rr88<T>(T u){ return $"ZERO SCALE on transform: {Nm(u)}";}
public static string rr89<T>(T u){ return $"Transforms: {Nm(u)}";}
public static string rr90<T>(T u){ return $"Audit summary: {Nm(u)}";}
   /* bone-array family - the audit used to count bones.Length and stop, which
      cannot see an array of N NULLS (length N, no skeleton).  That is the
      "invisible but all the bones are there" shape exactly. */
public static string rr91<T>(T u){ return $"Bones array holds NULL entries: {Nm(u)}";}
public static string rr92<T>(T u){ return $"ALL bone slots are NULL: {Nm(u)}";}
public static string rr93<T>(T u){ return $"Bones belong to a different root: {Nm(u)}";}
public static string rr94<T>(T u){ return $"bindPoses length != bones length: {Nm(u)}";}
   /* dead-bone-slot family - the cause of an invisible avatar with a perfect
      mesh.  A bone array longer than the bones it holds reports a healthy
      bones.Length; an index written into a NULL slot resolves to nothing on the
      client (the editor still draws it) and collapses the vertex to the origin. */
public static string rr95<T>(T u){ return $"Bone array holds NULL slots: {Nm(u)}";}
public static string rr96<T>(T u){ return $"No live nanimation bone for this mesh: {Nm(u)}";}
   /* merge-source family - the resolved mesh used to build the CombineInstances
      is chosen BEFORE the combine loop, so these two codes name which mesh the
      merge actually ran against.  96 is the healthy case; 97 means the source
      lookup found nothing and the SCENE mesh is being merged in its place,
      which silently changes the submesh count and the skeleton the result is
      bound to. */
public static string rr97<T>(T u){ return $"No source mesh resolved, falling back to the SCENE mesh (degraded): {Nm(u)}";}
public static string rr98<T>(T u){ return $"Live renderer submesh count differs from the resolved source mesh: {Nm(u)}";}
   /* mesh-import family - the importer pass runs over every asset path behind
      the avatar, so a path that cannot be read must be named rather than
      silently dropped from the report. */
public static string rr99<T>(T u){ return $"Importer could not be read for this asset path: {Nm(u)}";}
public static string rr100<T>(T u){ return $"Importer settings report: {Nm(u)}";}
   /* probe family - each code names one stage of the read-only or repair probe,
      so a dumped line identifies its stage instead of collapsing into a single
      generic message.  Codes 110..129 are the probe; 111/121 are the distinct
      "avatar not found" entries for the diagnostic and Why runs respectively.
      Codes 101..109 are kept CONTIGUOUS with 97..100 rather than being skipped,
      because a HOLE IN THE TABLE IS NOT INERT: BarCodeKiller resolves a code by
      building the name "rr" + value and looking it up on typeof(NZK.E), so a
      gap does not fail safe - it returns "unknown:" while the debug output for
      a SUCCESSFUL call prints only "data":true, which reads as proof the call
      ran. The 44 "unknown:" lines this file used to produce were not a missing
      table, they were a missing RANGE. Keep every integer below the highest
      code defined here present, so any in-range index resolves to a sentence. */
public static string rr101<T>(T u){ return $"Probe argument is empty: {Nm(u)}";}
public static string rr102<T>(T u){ return $"Probe argument flag was not supplied: {Nm(u)}";}
public static string rr103<T>(T u){ return $"Probe found no loaded scene matching that avatar name: {Nm(u)}";}
public static string rr104<T>(T u){ return $"Probe scanned no object of the requested kind: {Nm(u)}";}
public static string rr105<T>(T u){ return $"Probe stage skipped, the previous stage returned null: {Nm(u)}";}
public static string rr106<T>(T u){ return $"Probe measured zero of the condition it was looking for: {Nm(u)}";}
public static string rr107<T>(T u){ return $"Probe could not classify this object, it matched no known case: {Nm(u)}";}
public static string rr108<T>(T u){ return $"Probe report was written with fields still unset: {Nm(u)}";}
public static string rr109<T>(T u){ return $"Probe finished with no verdict: {Nm(u)}";}
public static string rr110<T>(T u){ return $"Avatar root not found by name: {Nm(u)}";}
public static string rr111<T>(T u){ return $"Avatar not found, cannot run the diagnostic: {Nm(u)}";}
public static string rr112<T>(T u){ return $"renderer: {Nm(u)}";}
public static string rr113<T>(T u){ return $"summary: {Nm(u)}";}
public static string rr114<T>(T u){ return $"importer fix result: {Nm(u)}";}
public static string rr115<T>(T u){ return $"Running the nanimation repair for this avatar: {Nm(u)}";}
public static string rr116<T>(T u){ return $"Vertex count changed by the repair: {Nm(u)}";}
public static string rr117<T>(T u){ return $"Repair threw: {Nm(u)}";}
public static string rr118<T>(T u){ return $"verify summary: {Nm(u)}";}
public static string rr119<T>(T u){ return $"verify PASS: {Nm(u)}";}
public static string rr120<T>(T u){ return $"verify FAIL: {Nm(u)}";}
public static string rr121<T>(T u){ return $"Avatar not found for the Why diagnostic: {Nm(u)}";}
public static string rr122<T>(T u){ return $"transform with a zero or NaN scale: {Nm(u)}";}
public static string rr123<T>(T u){ return $"transform scale sweep summary: {Nm(u)}";}
public static string rr124<T>(T u){ return $"renderer bounds/enabled diagnostic: {Nm(u)}";}
public static string rr125<T>(T u){ return $"renderer bounds sweep summary: {Nm(u)}";}
public static string rr126<T>(T u){ return $"avatar descriptor report: {Nm(u)}";}
public static string rr127<T>(T u){ return $"Avatar descriptor lookup threw: {Nm(u)}";}
public static string rr128<T>(T u){ return $"animator report: {Nm(u)}";}
public static string rr129<T>(T u){ return $"invisible-avatar verdict: {Nm(u)}";}
   /* name helper for parameterized codes above needs to be genaricized and add keywords to list of error codes forreusibility*/




/* there is no hard cap to barcodes hoever if there is word or words or formats that are used repeatedly thoes need to be compressed and the errors using thoes need to be converted into using compound error code outputs. eg 
 * : {Nm(u)} is used quite frequently, there should be an error return object in message or something that removes that from it.  the word transform pass fail have a prefix varify that should be its own case string if pass fail etc
 * changed by repair should be replaced with a x b 
 * where an error prefix should be repair vertex count changed by for verbs or something so error code rr150 could be (int a int b int c int f T nmu) takes the args builds the error code a verb_b c. alot of these are just heres a true false statement tbh but in strings with pretty words  
*/
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