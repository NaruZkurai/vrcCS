/*
 * AD = AssetDatabase.  R = Refresh.
 *
 * EDITOR-ONLY, which is why this file lives in editor/.  AssetDatabase does
 * not exist in a player build, so a root-level copy would break the VRC upload:
 *
 *   error CS0234: The type or namespace name AD does not exist in NZK
 *
 * The assembly split is declared by editor/NZK.Shorthand.Editor.asmdef, which
 * sets includePlatforms to Editor and references NZK.Shorthand.Runtime.  The
 * root of shorthand/ and runtime/ compile into that runtime assembly, so any
 * UnityEditor touch belongs under editor/ and nowhere else.
 *
 *     NZK.AD.R()   ->  UnityEditor.AssetDatabase.Refresh()
 */
namespace NZK{  public static partial class AD{
  public static void R(){UnityEditor.AssetDatabase.Refresh();return;}
}}
