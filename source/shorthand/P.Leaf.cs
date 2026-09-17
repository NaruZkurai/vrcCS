namespace NZK{  public static partial class P{
  /*
   * The LAST segment of a path, with its extension removed.
   *
   *     "Assets/Models/Avatar.blend"        ->  "Avatar"
   *     "Assets/Gen/X/Meshes/Body_0.asset"  ->  "Body_0"
   *
   * NAME: P.Leaf. P is Path, and this lives here rather than under S
   * (string) because the question it answers is about PATH SHAPE - "what is
   * the last segment" - not about text. S holds Head/Tail, which split on an
   * arbitrary delimiter the caller supplies; P.Leaf knows what a path IS.
   *
   * The member is a full word because it has to be readable at the call
   * site. P.Leaf(path) needs no lookup; a two-letter form would.
   *
   * "Leaf" and not "short name" / "file name": the return value is not
   * necessarily a file name (the path may point at a folder), and "short"
   * describes length rather than identity. The concept is the terminal
   * segment, which is the same sense of "leaf" used by the generated leaf
   * assets elsewhere in this toolkit.
   *
   * This is the avatar-name derivation used by every NaNimate entry point
   * when no explicit avatar name is supplied: the model file's own leaf
   * name is the fallback identity for the generated output tree.
   */
  public static string Leaf(string path){return System.IO.Path.GetFileNameWithoutExtension(path);}
  }
}
