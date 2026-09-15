#How to add to vrchat
in unity go to window>package manager>+ button at the top left>add package from git url
then paste this:
```bash
https://github.com/NaruZkurai/vrcCS.git?path=/build
```
# vrcCS — NZK Toolkit working mirror

Read-only mirror of the Unity project's NZK toolkit, plus the test harness.
Nothing here is a source of truth: everything is produced by
`NZK-Toolkit/sync.sh` inside the Unity project.

```
vrcCS/
├── source/   # the .cs.nzk inputs + hand-written leaves (what gets compiled)
├── build/    # sync output — this is what ships to Unity
│   ├── NZK/          # generated from the five .cs.nzk inputs
│   ├── shorthand/    # pre-leafed, hand-written, mirrored verbatim
│   └── rctoan_menuItem.cs.nzk
└── testig/   # test harness + compile script (nothing ships from here)
```

## Regenerating

Run sync from the Unity project; it writes `source/` and `build/` for you:

```bash
cd "/nzk/unity/blank project/Assets/NZK-Toolkit"
./sync.sh
```

`sync.sh` does five `convert.py -bc -u` passes, then mirrors out:

| pass | input | output segment |
|---|---|---|
| 1 | `nzktoolkit_monolith_compilable.cs.nzk` (`-d`) | `NZK/Core/Core.cs` |
| 2 | `rctonan.cs.nzk` | `NZK/Core/Core_p1.cs` |
| 3 | `NZK.Toolkit.cs.nzk` | `NZK/Core/Core_p2.cs` |
| 4 | `Yaml.cs.nzk` | `NZK/Core/Core_p3.cs` |
| 5 | `nan.cs.nzk` | `NZK/Core/Core_p4.cs` |

`shorthand/` is **pre-leafed** — hand-written, one member per file, never
generated. It is mirrored as-is (`S.pc.cs` -> `NZK.E.PC`, `E.D.cs` -> nested
`NZK.E.D`, `AD.R.cs` -> `NZK.AD.R`, `ToINT.cs` -> `NZK.I.To`).

## Split of duties

- **`Yaml.cs.nzk`** — how a Unity YAML document is *built*: markers, block
  spans, type tags, line/field IO, `YBlock`, `Span`. Also the YAML/Unity
  **value literals** any system might need: `NaN`, `VecNaN`, `FlagsKey`,
  `FlagsScale`, `AttrScale`, `IsZero`, `IsNumber`, `SetFlags`, `Group`.
- **`nan.cs.nzk`** — the NaN *policy* only: is this scale zero, turn zeroes
  into NaN, force any numeric scale to NaN. It asks `Yaml` for both the block
  boundaries and the literals; it only decides *when* a value becomes NaN.

Neither knows the other's job. Keep it that way.

## Compiling

```bash
/nzk/git/vrcCS/testig/nzkbuild.sh
```

Reads only `build/` (so it cannot compile a `.cs.nzk` sync input by accident),
runs `sync.sh` first, and writes `testig/nzk.dll`. Needs no .NET SDK — it uses
Unity's bundled Roslyn (`DotNetSdkRoslyn/csc.dll`) and the VRChat SDK
assemblies from `/nzk/unity/vrc/!_CC_Kiga 4/Packages`.

### Known-good baseline
On local testing without unity:
3 errors, all pre-existing and unrelated to the toolkit sources:

```
NZK/Core/Systems/AvatarValidator/AvatarValidator.cs  (VRC.SDK3.Validation, VRCSdkControlPanel)
NZK/Core/AVController/AVController_p1.cs             (VRC.SDK3.Validation)
```

These are a missing-assembly artefact of the offline harness, not real
failures. Anything else is a genuine regression.

## Rules

1. **Never edit anything in `build/` or `source/`.** Both are overwritten by
   `sync.sh`. Edit the `.cs.nzk` in the Unity project.
2. `testig/` is scratch space — delete and recreate freely.
3. Only synced output is compiled; never compile a `.cs.nzk` sync input
   alongside its own generated output (that yields CS0111/CS0102 duplicates).
4. Use **bash**, not zsh, for these commands — zsh autocorrect mangles
   heredocs and filenames.
# Other notes
obviously this readme is more for ai than humans 
yes this project has some in it
does it mater which ones i use?
idk i swapp around alot. stuff like deepseek and qwen or other local stuff.
i prefer 9b > anything coz its a good balence of fast to able to use tools
