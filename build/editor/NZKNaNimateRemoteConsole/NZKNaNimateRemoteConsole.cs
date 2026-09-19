#if UNITY_EDITOR
namespace NZK.NaNimate
{
public static partial class NZKNaNimateRemoteConsole {
    /*
     * Remote console for the v6 toolkit, so an automated session can drive the
     * editor without a human clicking menus.
     *
     * WHY: the v6 tree had no command channel. `Assets/NZK toolkit v6/remote_cmd.txt`
     * existed but nothing read it - the only listener was v4's RemoteConsole,
     * bound to v4's own file. Writing "compile" there therefore did nothing at
     * all, which silently wasted several diagnostic rounds.
     *
     * A command line is treated as NEW when it differs from the previous content,
     * so re-issuing the same verb requires writing something different in between
     * (append a counter, or clear the file first).
     *
     * Commands (one per line; '#' comments ignored):
     * ping                              liveness + PID
     * compile                           request a script recompilation
     * generate <modelPath> <avatarName>  run the generator immediately
     * diagnose <prefabPath>             dump real renderer/mesh state
     * log <message>                     echo into the Editor log
     */
    
}
}
#endif
