/*
 * AD = AssetDatabase.  R = Refresh.
 *
 * Was editor/AD.R.cs under its own editor asmdef.  The shorthand tree is one
 * assembly now, so this sits at the root with the rest and the single editor
 * call is guarded instead of moved.
 *
 * AssetDatabase does not exist in a player build, so an unguarded call breaks
 * the VRC upload:
 *
 *   error CS0234: The type or namespace name 'AssetDatabase' does not exist in
 *   the namespace 'UnityEditor'
 *
 * The #else branch is a no-op rather than dropping the member.  Callers live in
 * NZK/Core/MenuItems, which is editor-only today, but keeping R() always
 * callable means a shared code path never has to know which side of the build
 * it is on.  Refreshing an asset database that does not exist is, correctly,
 * nothing at all.
 *
 *     NZK.AD.R()   ->  UnityEditor.AssetDatabase.Refresh()   (editor)
 *     NZK.AD.R()   ->  no-op                                  (player)
 */
namespace NZK{  public static partial class AD{
#if UNITY_EDITOR
  public static void R(){UnityEditor.AssetDatabase.Refresh();return;}
#else
  public static void R(){return;}
#endif
}}
