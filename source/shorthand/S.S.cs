namespace NZK{  public static partial class S{
  /*
   * Sanitizing helpers that RETURN A STRING.
   *
   * NAMING:
   *     S       = the return type: this returns a System.String.
   *     .S      = the CONCEPT: Sanitize. A safe asset name.
   *     .An     = what you get back: an ANonymized/sanitized name.
   *
   *   So S.S.An reads as "String, Sanitized, name" - 4 + 4 characters at the
   *   call site, against the 7 of the San.Sanitize it replaces. The old form
   *   was also self-contradicting: a class named San whose member repeated
   *   "Sanitize" spent seven characters saying one thing twice.
   *
   * The 'S' concept segment also carries the two sibling members below
   * (AssetPath), so everything about safe names is found under one prefix.
   */
  public static partial class SS{
    /*
     * Make a name safe to use as a Unity asset FILE name.
     *
     * Uses an ALLOWLIST, not System.IO.Path.GetInvalidFileNameChars.
     * That API is platform-dependent and, on Linux, returns only NUL and '/',
     * so characters Unity itself refuses - notably '|' - passed straight
     * through and every write failed with:
     *
     *   "'...|------------RealPP------------|.asset' is not a valid asset file name."
     *
     * Unity's asset-name validation is stricter than the host filesystem's,
     * and it must hold on every platform, so the rule is defined here instead
     * of being inherited from the OS.
     *
     * Kept: letters, digits, space, dot, dash, underscore, parentheses.
     * Everything else - including '|', ':', '*', '?', '"', '<', '>', '/',
     * '\\', control characters and non-ASCII - is ENCODED, not replaced.
     *
     * A plain '_' substitution is LOSSY: "Pads_K+E" and "Pads_K_E" are two
     * different source objects, but both sanitize to "Pads_K_E". That is not
     * merely cosmetic - it made the generated leaf names order-dependent and
     * produced a corrupt mesh (RootBoneNameHash: 0) for one of the pair,
     * because the wrong source was matched back to the written asset.
     *
     * So an unsafe character is ENCODED (see Enc), keeping the mapping
     * injective: different input names always yield different output names.
     * Safe characters are kept VERBATIM, including dots, so names such as
     * "B Body.001" stay readable and keep their identity.
     */
    public static string An(string name){
      if(NZK.B.NoE(name)) return "unnamed";

      var sb=new System.Text.StringBuilder(name.Length+8);

      for(int i=0;i<name.Length;i++){
        char c=name[i];
        if(Safe(c)){sb.Append(c);continue;}
        Enc(sb,c);
      }

      /*
       * Trailing dots and spaces are legal on Linux but are stripped or
       * rejected by other platforms, so remove them for a portable name.
       */
      string result=sb.ToString().Trim().TrimEnd('.',' ').Trim();
      return result.Length==0?"unnamed":result;
    }

    /*
     * Append the encoding of one unsafe character.
     *
     * Unicode code units above 0x7F and any reserved/illegal ASCII become
     * '$' followed by the value in hex. '$' is itself unsafe, so it also
     * encodes to '$24' - which is what makes the mapping unambiguous and
     * reversible: a literal '$' in the source can never be confused with an
     * escape sequence.
     */
    static void Enc(System.Text.StringBuilder sb,char c){
      sb.Append('$');
      sb.Append(((int)c).ToString("x2"));
    }

    /*
     * True for characters permitted in a Unity asset file name.
     *
     * Deliberately conservative: an unnecessary escape is a cosmetic
     * difference, whereas one rejected character fails the entire write.
     * '$' is NOT safe - it is reserved as the escape marker.
     */
    static bool Safe(char c){
      if((c>='a'&&c<='z')||(c>='A'&&c<='Z')||(c>='0'&&c<='9')) return true;
      return c==' '||c=='.'||c=='-'||c=='_'||c=='('||c==')'||c=='['||c==']';
    }

    /*
     * Project-relative path of ONE generated asset leaf:
     *
     *     <folder>/<sanitizedName>_<index>.asset
     *
     * NAME: S.S.AssetPath - String, Sanitized, AssetPath.
     *
     * WHY IT SITS BESIDE An AND NOT IN ITS OWN FILE:
     *   It encodes the same invariant: the leaf name is only safe because An
     *   ran first. Keeping them together is what stops a caller passing a raw
     *   renderer name and writing an invalid path.
     *
     * WHY THE CALLER SANITIZES AND THIS METHOD DOES NOT:
     *   Taking the raw name and calling An in here would collapse the call
     *   site to one argument, and it was tried - it BROKE:
     *
     *     error CS0234: The type or namespace name 'NaNimate' does not exist
     *     in the namespace 'NZK'
     *
     *   Reaching a NaNimate type from inside namespace NZK needs the
     *   global-qualified "global::NZK.NaNimate.X", because a bare
     *   "NZK.NaNimate.X" resolves relative to the enclosing NZK and is read
     *   as NZK.NZK.NaNimate.X.
     *
     *   Rather than reach up into the feature layer, sanitizing stays at the
     *   call site. Shorthand is the FOUNDATION layer and must never depend on
     *   the feature that consumes it, or the dependency runs in a circle.
     */
    public static string AssetPath(string folder,string sanitizedName,int subMeshIndex){
      return System.IO.Path.Combine(folder,sanitizedName+"_"+subMeshIndex+".asset");
    }
  }
  }
}
