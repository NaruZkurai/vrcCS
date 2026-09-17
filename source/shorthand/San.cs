namespace NZK{  public static partial class San{
  /// <summary>
  /// Make a name safe to use as a Unity asset FILE name.
  ///
  /// Uses an ALLOWLIST, not System.IO.Path.GetInvalidFileNameChars. That API
  /// is platform-dependent and, on Linux, returns only NUL and '/', so
  /// characters Unity itself refuses - notably '|' - passed straight through
  /// and every write failed with:
  ///
  ///   "'...|------------RealPP------------|.asset' is not a valid asset file name."
  ///
  /// Unity's asset-name validation is stricter than the host filesystem's,
  /// and it must hold on every platform, so the rule is defined here instead
  /// of being inherited from the OS.
  ///
  /// Kept: letters, digits, space, dot, dash, underscore, parentheses, brackets.
  /// Everything else - including '|', ':', '*', '?', '"', '&lt;', '&gt;', '/',
  /// '\\', control characters and non-ASCII - becomes '_'.
  ///
  /// Encode a name so it is safe as a Unity asset file name AND so distinct
  /// inputs can never alias to the same output.
  ///
  /// A plain '_' substitution is LOSSY: "Pads_K+E" and "Pads_K_E" are two
  /// different source objects, but both sanitize to "Pads_K_E". That is not
  /// merely cosmetic - it made the generated leaf names order-dependent and
  /// produced a corrupt mesh (RootBoneNameHash: 0) for one of the pair,
  /// because the wrong source was matched back to the written asset.
  ///
  /// So an unsafe character is ENCODED, not replaced (see
  /// <see cref="San.Enc"/>), keeping the mapping injective:
  /// different input names always yield different output names.
  ///
  /// Safe characters are kept VERBATIM, including dots, so names such as
  /// "B Body.001" stay readable and keep their identity.
  /// </summary>
  public static string Sanitize(string name){
    if(string.IsNullOrEmpty(name)) return "unnamed";

    var sb=new System.Text.StringBuilder(name.Length+8);

    for(int i=0;i<name.Length;i++){
      char c=name[i];

      if(Safe(c)){sb.Append(c);continue;}

      Enc(sb,c);
    }

    // Trailing dots and spaces are legal on Linux but are stripped or
    // rejected by other platforms, so remove them for a portable name.
    string result=sb.ToString().Trim().TrimEnd('.',' ').Trim();

    return result.Length==0?"unnamed":result;
  }

  /// <summary>
  /// Append the encoding of one unsafe character.
  ///
  /// Unicode code units above 0x7F and any reserved/illegal ASCII become
  /// '$' followed by the value in hex. '$' is itself unsafe, so it also
  /// encodes to '$24' - which is what makes the mapping unambiguous and
  /// reversible: a literal '$' in the source can never be confused with an
  /// escape sequence.
  /// </summary>
  public static void Enc(System.Text.StringBuilder sb,char c){
    sb.Append('$');
    sb.Append(((int)c).ToString("x2"));
  }

  /// <summary>
  /// True for characters permitted in a Unity asset file name.
  ///
  /// Deliberately conservative: an unnecessary escape is a cosmetic
  /// difference, whereas one rejected character fails the entire write.
  /// '$' is NOT safe - it is reserved as the escape marker.
  /// </summary>
  public static bool Safe(char c){
    if(c>='a'&&c<='z') return true;
    if(c>='A'&&c<='Z') return true;
    if(c>='0'&&c<='9') return true;

    switch(c){
      case ' ':
      case '.':
      case '-':
      case '_':
      case '(':
      case ')':
      case '[':
      case ']':
        return true;
      default:
        return false;
    }
  }
  }
  }
