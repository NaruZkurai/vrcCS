namespace NZK{  public static partial class P{
  /// <summary>
  /// Bare file name of an asset path, without its extension.
  ///
  /// This is the avatar-name derivation used by every NaNimate entry point
  /// when no explicit avatar name is supplied: the model file's own leaf
  /// name is the fallback identity for the generated output tree.
  /// </summary>
  public static string Sn(string path){return System.IO.Path.GetFileNameWithoutExtension(path);}
  }
  }
