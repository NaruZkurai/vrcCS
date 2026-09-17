namespace NZK{  public static partial class P{
  /// <summary>
  /// Bare file name of an asset path, without its extension.
  ///
  /// NAME: P.Sn = Path.Short Name.
  ///   P  = the path-helper class. Siblings: Pa = project-relative to absolute,
  ///        Sa = sanitized-asset leaf.
  ///   Sn = Short Name, i.e. GetFileNameWithoutExtension. "Sn" rather than
  ///        "Leaf" because Sn pairs with Sa in the two-letter style.
  ///
  /// This is the avatar-name derivation used by every NaNimate entry point
  /// when no explicit avatar name is supplied: the model file's own leaf
  /// name is the fallback identity for the generated output tree.
  /// </summary>
  public static string Sn(string path){return System.IO.Path.GetFileNameWithoutExtension(path);}
  }
  }
