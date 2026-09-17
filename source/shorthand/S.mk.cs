namespace NZK{  public static partial class S{
  /*
   * NAME-MAKING helpers that return a string.
   *
   *     S        = String, the return type.
   *     .mk      = make.  The verb, so the call site reads as an action:
   *                S.mk.unq(used) is "string, make, unique".
   *     .unq     = unique.  Vowel dropped in the standard telegraphic form
   *                already used by S.mk's siblings and by B.mpty - "uq" would
   *                be unreadable, "unique" is four characters longer for no
   *                information.
   *
   * So the name carries result type, verb and property, in that order, and a
   * reader never has to open the file to learn what came back.
   *
   * REPLACES L.Unique.  That name described the RESULT ("the string is unique")
   * while saying nothing about what the call DOES - it claims a name, which is
   * a mutation of `used`.  It also sat in L, "List/collection helpers", which
   * described the secondary parameter rather than the return.
   */
  public static partial class mk
  {
    /*
     * Claim the next free name in a used-set, appending "_n" until the name is
     * unique, and MUTATE `used` as the claim.
     *
     * Necessary because Blender emits duplicate object names ("B Body" and
     * "B Body.001" can both sanitize to an already-used leaf), and two assets
     * written to one leaf path silently overwrite each other - one source is
     * then lost with no error at all.
     *
     * The return value is the caller's to keep, but `used` is what makes it
     * work: the second call with the same base name sees the first and takes
     * "_1".  A caller that throws the HashSet away between calls gets the same
     * name back twice, so the set must outlive the loop.
     *
     * `used.Add` returns false when the entry was already present, which is why
     * the do/while tests it directly - the loop condition IS the uniqueness
     * test, so there is no separate "does it exist" scan.
     */
    public static string unq(string baseName,System.Collections.Generic.HashSet<string> used){
      if(used.Add(baseName))
        return baseName;

      int suffix=1;
      string candidate;
      do
      {
        candidate=baseName+"_"+suffix;
        suffix++;
      }
      while(!used.Add(candidate));

      return candidate;
    }
  }
}}
