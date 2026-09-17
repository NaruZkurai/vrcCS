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

## v6 — NaNimate generator

The `source/v6/` set is the NaNimate mesh pipeline, converted from the live
project's `Assets/NZK toolkit v6/Editor/*.cs`. Ten sources:

`NZKNaNimateMeshGenerator`, `NZKNaNimateMeshFolder`, `NZKNaNimateWeightImporter`,
`NZKNaNimateWeightNormalizer`, `NZKNaNimateDiagnostics`, `NZKNaNimateRemoteConsole`,
`NZKNaNimateGroupManifest`, `NZKNaNimateGroupScanner`, `NZKNaNimateGroupWindow`,
`NZKNaNimateMeshFreezer`.

### Split and deploy

```bash
cd /nzk/git/GP_CS_SPLITTER
cp /nzk/git/vrcCS/source/v6/*.cs.nzk .
cp /nzk/git/vrcCS/source/shorthand/*.cs .
# -bc = one file per type, -d first pass wipes dest, -u appends later passes
for f in NZKNaNimate*.cs.nzk; do
  convert.py "$f" -bc -d -o /tmp/nzkv6   # (first pass only)
done
```

Output path derives from the **top-level type**, never the filename. Then
deploy into the project:

| what | from | to |
|---|---|---|
| split leaves (19) | `/tmp/nzkv6/**/*.cs` | `Assets/NZK toolkit v6/Editor/**` |
| shorthand (23) | `source/shorthand/*.cs` | `Assets/NZK toolkit v6/vrcCS/build/shorthand/` |

Originals are backed up to `Editor/_bak/*.cs.bak` before replacement.

### Dialect rules

- Shape: `namespace NZK{  public static partial class X{...}}` — two spaces
  after `namespace NZK{`, no newline before the class.
- No `using` directives. Everything fully qualified.
- Rationale/XML kept **verbatim** in meaning, but as `/* */` blocks only.
  A literal `*/` must never appear inside a comment body.
- Trivial bodies reuse shorthand; prefer a shorthand helper over a buried local.
- **Moderately complicated functions should be simple 1-liners.** A body that
  is one expression, or one call plus a return, collapses onto the signature
  line. Multi-step bodies (loops, several statements) stay expanded, but each
  step should itself be a single line.
- If a 1-liner would repeat an expression used elsewhere, it becomes a new
  shorthand file instead — see `P.La` and `M.By`, extracted exactly because
  two files needed the same logic.
- `ll<N>` is the counted-or family (`B.ll2`…`B.ll5`, `B.llAny`, `B.llNll`).

### Shorthand added during v6

| file | member | purpose |
|---|---|---|
| `P.La.cs` | `P.La(folder,sanitizedName,i)` | one frozen-mesh leaf path: `Combine(folder, sanitizedName+"_"+i+".asset")` |
| `M.By.cs` | `M.Vb(mesh)` | estimated mesh bytes (`vertexCount * 48`) |
| `M.By.cs` | `M.By(bytes)` | human-readable size, one decimal on KB/MB |
| `M.By.cs` | `M.Mb(mesh)` | `M.By(M.Vb(mesh))` |

`M.BytesPerVertex48` is the shared per-vertex budget, so the generator and the
freezer cannot drift on what "48 bytes per vertex" means.

### Shorthand must not reference `NZK.<SubNamespace>`

Shorthand files declare `namespace NZK{ ... }`. Inside that namespace the
identifier `NZK` first resolves to the **current** namespace, so a reference to
`NZK.NaNimate.NZKNaNimateMeshFolder` is read as `NZK.NZK.NaNimate...` and the
project build fails with:

```
P.La.cs(10,28): error CS0234: The type or namespace name 'NaNimate'
does not exist in the namespace 'NZK'
```

Shorthand is the lowest layer — it must not depend on the v6 types at all.
Push the coupling to the **caller**: `P.La` takes an already-sanitized name, and
`LeafAssetPath` — which lives in `NZK.NaNimate` and can see `Sanitize` —
does the sanitizing.

### Verified

- Deployed tree: 19 split leaves + `delete.NzkJacketWeights.cs` = 20 `.cs`.
- All repeated outer types (`NZKNaNimateMeshGenerator`, `…GroupScanner`,
  `…WeightNormalizer`, `…RemoteConsole`, `…MeshFreezer`) are `partial` and merge.
- Shorthand in the project: 25 files (`vrcCS/build/shorthand/`).
- `unity-cli console --type error` → **`[]`**.

### The `CreateFolder` errors are gone — here is the proof

Eleven `CreateFolder is not supported while importing out-of-process` lines used
to appear in the console. They are **stale buffer replay**, not live failures:

| step | result |
|---|---|
| `grep AssetDatabase.CreateFolder` across all of `Assets/**/*.cs` | **4 hits, all comments — zero real call sites** |
| console cleared | `[]` |
| `AssetDatabase.Refresh(ForceUpdate)` issued, full reimport | — |
| `CreateFolder` errors after reimport | **0** |
| console errors after reimport | **`[]`** |

If nothing in the source calls `CreateFolder`, nothing can raise it. The fix is
`EnsureFolder` → `System.IO.Directory.CreateDirectory` + hand-written `.meta`
sidecars, because `AssetDatabase.CreateFolder` is main-thread-only and
`AssetDatabase.Refresh` is a silent no-op during import.

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

# .cs.nzk shorthand format — spec for v6 conversion (2026-09-17)

## Layout
Flat file, NOT partial-leaf per type. One file per TOP-LEVEL TYPE:
`namespace NZK{ public static partial class X{ ...MEMBERS... } }`

## Compression rules in /nzk/git/vrcCS/source/shorthand/
1. `namespace NZK{` — no newline after `{`; type opens on the SAME line.
   Canonical opener: `namespace NZK{  public static partial class X{`
2. ALL types fully qualified, NO `using` directives:
   `System.Int32`, `System.String`, `UnityEngine.Mesh`, `UnityEditor.AssetDatabase`.
   This is deliberate — see "No using directives by design" in the v6 sources.
3. `public static` (not `internal static`), even for `static` helpers.
4. Return types spelled fully qualified: `System.Int32`, `System.Boolean`.
5. `return X;` written on the SAME lines as the signature where short — often
   one line per member: `public static bool NoE(string a){return string.IsNullOrEmpty(a);}`
6. Two-space indent inside `namespace`, braces often closed on one line:
   `}}}` or `  }` + `  }`.
7. Comments preserved when they carry meaning (see E.D.cs, ToINT.cs which keeps a
   commented-out member).
8. Generic helpers keep constraints: `where T : class`.
9. `<T>` helpers are used to AVOID overload duplication.

## Big-function/chunk guidance (user directive)
Break large functions and `if` statements into REUSABLE CHUNKS held in VARIABLES,
so the same expression is not repeated and each chunk can be referenced by name.
Prefer a named local/helper over duplicating an expression.

## USER RULES — comments and trivial bodies (2026-09-17)
1. **COMMENTS: `/*style*/` ONLY.** No `///` XML doc blocks, no `//` line comments.
   Convert XML docs to `/* ... */`, preserving the text verbatim.
2. **TRIVIAL BODIES MUST REUSE SHORTHAND.** Do not re-implement what a shorthand file
   already provides. Example the user gave:
       return !string.IsNullOrEmpty(name)&&name.IndexOf(P,System.StringComparison.OrdinalIgnoreCase)>=0;
   must become `NZK.S.HasOIC(name,P)`. If a body is one expression of framework calls,
   look for a shorthand equivalent first.

## SHORTHAND CATALOG (reuse before inventing)
    NZK.B.NoE(string) / B.NllE(string) / B.Eq<T>(T,T) / B.EC<T> / B.I3eeI3<T>
    NZK.S.Has(string)                  non-empty string
    NZK.S.HasOIC(string,string)        case-insensitive Contains          (NEW)
    NZK.S.StartsOIC(string,string)     case-insensitive StartsWith        (NEW)
    NZK.S.EqOIC(string,string)         case-insensitive Equals, null-safe (NEW)
    NZK.S.EndsWithOIC(string,string)   case-insensitive EndsWith
    NZK.S.EndsAnyOIC(string,params string[])  any-of suffixes             (NEW)
    NZK.S.NZKNaNimatePrefix(string)    "[NZK NaNimate] " + message
    NZK.S.Head(string,char) / S.Tail(string,char)                        (NEW)
    NZK.E.PC(string,string)            Path.Combine
    NZK.P.Pa(string)                   absolute from project-relative
    NZK.P.Sn(string)                   leaf name without extension
    NZK.San.Sanitize/Safe/Enc          asset-name safety
    NZK.T.Tp(Transform)/Tpr(Transform) transform path root-exclusive/inclusive
    NZK.L.IsEmpty/NotEmpty/Unique      collection + name helpers
    NZK.E.D.Lg/LgErr/LgWarn/LgIf       rr-code console logging
    NZK.E.D.OK/OK<T>/NerrOK            rr-code dialogs

Filename = `<Type>.<Member>.cs`, or `<Type>Name.cs` when the file holds a whole small type.
Verified mapping:
    AD.R.cs            -> class AD, member R
    B.NoE.cs           -> class B, member NoE
    B.Eq.cs            -> class B, member Eq
    B.I3eeI3.cs        -> class B, member I3eeI3
    E.D.cs             -> class E, nested class D
    E.Dd.cs            -> class E, member Dd
    S.pc.cs            -> class E, member PC   (note: name chosen for the CONCEPT)
    ToINT.cs           -> class I, several To* methods (whole type in one file)
    M.ec.cs            -> class B  (misnamed, pre-existing)
Types are grouped by FIRST LETTER of purpose: B=Bool, S=String, E=Error, AD=AssetDatabase,
I=Int/convert, M=Misc. A new utility type should pick a free letter and one file per member.

## file.nzk format: Prefer NEW SHORTHAND FILES over buried locals
Repeated non-trivial logic that could serve ANY toolkit file must become a NEW file in
`/nzk/git/vrcCS/source/shorthand/`, named per the convention above — NOT inlined as a
private local helper inside one file. Trivial 1-liners may stay inline.

```
namespace NZK{  public static partial class B{public static bool NoE(string a){return string.IsNullOrEmpty(a);}}
  }
```

