# NaNimation — how it actually works, and what we were doing wrong

Sources read for this document, cloned to `/nzk/git`:

| repo | path | why |
|---|---|---|
| `d4rkc0d3r/d4rkAvatarOptimizer` | `GO_d4rkAvatarOptimizer/` | the technique's origin |
| `bdunderscore/modular-avatar` | `GO_modular-avatar/` | the reference implementation |

The authoritative implementation is
`GO_modular-avatar/Editor/ReactiveObjects/MeshFiltering/NaNimationFilter.cs`.
Everything below is read off that file, not inferred.

---

## 1. The mechanism, correctly stated

> NaNimation works by animating the scale of bones to NaN, in order to cause any
> primitives including those vertices to be hidden.
> — `NaNimationFilter` class comment

Note the word **primitives**. That is the first thing we got wrong.

The clip is trivial:

```
Armature/NaNimations/NaNimate Stockings   m_LocalScale.x/y/z = NaN
```

A bone whose local scale is NaN produces a NaN skinning matrix. Any vertex that
matrix is applied to gets a NaN position, the GPU discards the triangle, and the
garment is hidden. No shader support required.

**We proved this end to end in play mode.** `Assets/_NAN ONLY.controller` has a
35-state, zero-parameter machine whose default state is
`...NaNimate Body - Rings_1_NaN`. Entering play mode toggles that garment with no
input at all:

| | edit mode | play mode |
|---|---|---|
| `Acc Body - Rings` | VISIBLE, 1176 finite verts | **HIDDEN, 0 finite / 1176 NaN** |
| `NaNimate Body - Rings` bone | scale = 1 | **scale = NaN** |
| `NaNimations` children with NaN | 0 | **1** (the matching one) |

So the runtime mechanism is real and we have it reproduced. The bugs are in how
we **build** the meshes and the bones, not in the clip or the animator.

---

## 2. The thing we were missing: NaNimation is a MESH operation

Our pipeline does this:

```
Blender authors a vertex group at 1e-7
        -> Unity imports the mesh (importer clamps 1e-7 up to minBoneWeight=0.001)
        -> the nanimation bone is used directly as a normal skin bone
        -> clip scales that bone to NaN
```

`NaNimationFilter` does something structurally different:

```
input mesh (vertex groups as authored, ANY weights)
        -> SPLIT vertices per "hide key" (which toggles apply to this primitive)
        -> ADD ONE NEW BONE PER HIDE GROUP
        -> move the selected influences onto those new bones
        -> append the matching bindpose for each new bone
        -> replace the renderer's bones array, appending the new bones
        -> clip scales the NEW bone to NaN
```

**The authored group is never relied on to hold a magic value.** Instead the
weights are *rearranged* so that hiding is possible, and a dedicated bone is
created for each distinct visibility group.

That is why our approach produces garbage: we are driving an **existing armature
bone** with NaN, which drags every vertex weighted to it — and if any of those
weights is a real (non-epsilon) value, the surface is torn toward the group
origin instead of hidden. Modular Avatar never has that problem because the
influence it drives lives on a bone created solely for that purpose, at exactly
the weight it needs.

### The concrete pieces

**Hide key** (`IHideKey` / `SmallHideKey` / `LargeHideKey`) — the set of toggle
shapes that select a given primitive. Up to 64 shapes uses a `ulong` bitmask;
more uses a sorted array. Two primitives can be hidden together iff they share a
hide key.

**Vertex splitting** — a vertex shared between primitives with *different* hide
keys cannot serve both, so it is duplicated, and the index buffer is rewritten to
point each primitive at its own copy. That is what makes "part of this mesh
toggles, part of it does not" work. **We do not do this at all**, and it is the
core reason a garment that is partly toggled comes out shredded.

**Weight extension** (`Phase E`) — bone weights are read with
`mesh.GetAllBoneWeights()` / `mesh.GetBonesPerVertex()`, cloned vertices get their
source vertex's weights appended, then `mesh.SetBoneWeights(...)` writes the
whole thing back.

**Bone creation** (`GenerateNaNimatedBones`) — one new bone per hide group, and
critically:

```csharp
// When we merge armature after generating NaNimated bones, we can end up
// changing the localScale of the nanimated bones, which is a problem, since
// we've baked that scale into our animation curves.
//
// To help avoid this, we add a buffer object - the buffer object will take the
// base scale change, while NaNimated object goes between scale (1,1,1) and
// (NaN, NaN, NaN)
```

so the hierarchy is:

```
<original bone>
  └── NaNimatedBuffer$<guid>        scale = 1, and absorbs any parent scale change
       └── NaNimatedBone for <shape>    animates 1 -> NaN
```

The buffer object exists because our armature scaling would otherwise change the
scale of the animated bone and break the curve that was baked against it. We have
no such buffer.

**Bindpose append** (`Phase F`) — `Array.Resize` on `bindposes` to
`nextBoneIndex`, then for each added bone `bindposes[new] = bindposes[original]`.

**This is the exact inverse of our `bindposes` bug.**

---

## 3. Our concrete defects, ranked

### 3.1 `bindposes` longer than `bones` — this is what makes the jacket vanish

Measured on `nemtest` / `TJacket - Leather Jacket`:

```
bones     = 224
bindposes = 336        <-- 112 extra
```

`bones[224..335]` are the null tail we identified early on and dismissed as
"inert padding". It is not inert when `bindposes` still has 336 entries: a
bindpose exists for an index whose bone does not, so the skinning matrix for that
influence is malformed and the vertices using it come out **NaN**.

Measured consequence, play mode:

```
TJacket - Leather Jacket   baked=1808  finite=1318  nan=490
```

**490 of 1808 vertices (27%) are NaN and get discarded** — the jacket renders as
a partial shell. Every other one of the 39 meshes reports `nan=0`, because their
weights never touch the orphaned tail.

Modular Avatar avoids this class of bug by construction: it resizes `bindposes`
*to the new bone count* and fills each appended slot from its source bone. The
array can never be longer than the bones using it.

**Fix direction:** `bindposes.Length` must equal `bones.Length`; truncate the
mesh's bindposes to the bone count the renderer actually declares, and never
leave slots for bones that are null.

### 3.2 We drive a shared armature bone instead of a dedicated one

Our nanimation influence sits on an armature bone (`NaNimate Leather Jacket`,
index 208) that is also a real skin bone in the rig's bone array. Scaling it to
NaN therefore affects exactly the vertices we want — but *only* if every one of
those weights is at the epsilon pin. Any real weight drags geometry.

Compounding it, the importer lifts authored `1e-7` to its `minBoneWeight`
minimum of `0.001` and **refuses to be configured otherwise** (see §3.4), so
"every weight is at the pin" is not a state we can rely on.

Measured spread across the avatar:

| mesh | nanim weight min | max | verdict |
|---|---|---|---|
| 38 of 39 meshes | ~5.7e-8 | ~1.2e-6 | acceptable |
| `TJacket - Leather Jacket` | **1.100e-3** | **1.102e-3** | 4 orders of magnitude too large |

At `1.1e-3` the influence is no longer "bound but invisible" — it deforms. That
is the reported "jacket weight was higher in Blender and it fails differently":
the magnitude *is* the variable.

Modular Avatar's answer is not to author a magic value at all. It builds the
weights it needs.

### 3.3 No vertex splitting

A primitive (triangle) is the unit that gets hidden, not a vertex. Our pipeline
never splits a vertex that is shared between primitives with different
visibility, so "some of the mesh toggles" cannot be expressed. Modular Avatar's
`Phase B+C` exists purely to do this, including the `vertToSharedKeys` map that
decides when a vertex *must* be duplicated versus can be assigned in place.

This is the largest structural gap and it is why partial garments come out
looking wrong even when the weights are perfect.

### 3.4 The importer clamp is unavoidable — stop trying to configure it

Unity 2022.3 forces `ModelImporter.minBoneWeight = 0.001`. Verified failures:

| attempted write path | result |
|---|---|
| `SerializedObject.FindProperty("minBoneWeight").floatValue = 0` | serialized reads 0, native stays 0.001 |
| native `ModelImporter.minBoneWeight = 0` | 0.001 |
| `SetDirty` + `WriteImportSettingsIfDirty` | 0.001 |
| `ImportAsset(ForceUpdate)` | 0.001 |
| `unity-cli reserialize` on the `.meta` | 0.001 |
| editing `minBoneWeight:` in the `.meta` **by hand** | 0.001, and even a *larger* value is ignored |

The last row is the decisive one: `maxBonesPerVertex: 2` in the `.meta` also
reads back as `4`. Unity ignores both fields and hard-codes them. Only
`optimizeBones` is honoured from the `.meta`.

**Any code that sets `minBoneWeight` and reports success is reporting a lie.**
`MeshImport.ForceFour` does this today, and it also probes `weightThreshold`,
which does not exist on this importer at all.

Modular Avatar never touches these settings — because it does not need the
authored value to survive. It re-derives the weights after import.

---

## 4. Performance — 32.7 s per run, and where it goes

Measured: a full `NZKNaNimateBoneHolder.RunNow()` takes **32 751 ms**.

The causes, in the order they cost:

1. **`MeshImport.ForceFour` calls `ImportAsset(ForceUpdate)` on every model.**
   A forced reimport is the most expensive operation in the editor, and it runs
   even for models already reported `Already` (nothing changed). The reimport
   also re-triggers `OnPostprocessAllAssets`, which is the infinite-recursion
   path we already had to guard.
2. **`Generate` writes every mesh asset on every run**, via delete +
   `CreateAsset`, then `SaveAssets()`.
3. **`NormalizeModel` writes every mesh a second time**, so each mesh is
   serialised twice per run.
4. `AssetDatabase.Refresh()` is called on top of `StopAssetEditing()`, which
   already flushes.

`NaNimationFilter` uses `NativeArray` + `JobHandle` for the primitive-mask phase
and operates entirely **in memory** — no asset writes, no reimports — then hands
one finished mesh to the caller.

**Fix direction:** do the weight work in memory on the scene mesh (which is what
`NormalizeScene` already does, and it was the one fast stage), skip writes when
nothing changed, and never `ImportAsset(ForceUpdate)` a model that already
satisfies the cap.

---

## 5. What to change

Ordered by how much it unblocks.

1. **Truncate `bindposes` to `bones.Length` on every mesh we touch.** Fixes the
   490 NaN vertices on the jacket and any other mesh with an orphaned tail. This
   is a change in `NZKNaNimateWeightNormalizer.NormalizeScene` (or a new pass),
   and it is cheap — it is one array resize and a `mesh.bindposes = ...` write,
   in memory.
2. **Stop relying on the authored 1e-7.** Either
   - create dedicated nanimation bones and move the influences onto them
     (Modular Avatar's approach, correct but a large change), or
   - at minimum, re-pin and renormalise **after** import on the scene mesh, and
     assert the result rather than assuming the importer preserved it.
   We already do the second; the missing part is the assertion and the vertex
   splitting.
3. **Split vertices by hide key** if partial-garment toggling is required. This is
   `Phase B+C`; without it, per-primitive visibility is not expressible.
4. **Add the buffer object** between the original bone and the animated bone, so
   armature scaling cannot change the scale the clip was baked against.
5. **Kill the forced reimports.** Drop `ImportAsset(ForceUpdate)` for models
   already at the cap, drop the redundant `Refresh()`, and skip mesh writes when
   the normalised result equals the source. Expect the run to go from ~33 s to
   under a second, since the only remaining cost is in-memory weight maths.
6. **Delete the `weightThreshold` probe** in `MeshImport`; the property does not
   exist.

---

## 6. Test evidence already gathered

| file | content |
|---|---|
| `testing/clip_probe.txt` | all 35 `_NAN ONLY` clip paths resolve in the `nemtest` rig (`resolved=35 missing=0`) |
| `testing/play_result.txt` | play mode: `Acc Body - Rings` HIDDEN, 1176/1176 NaN, bone scale NaN |
| `testing/weight_audit.txt` | all 39 meshes: 0 zero-weight vertices, 0 bad weight sums |
| `testing/null_audit.txt` | 0 dangling bone references |
| `Assets/testing/NaNimPlayTest.cs` | the harness that produced the above |
| `Assets/testing/NanimProof.cs` | earlier harness; its manual `localScale = NaN` phase is INVALID — Unity rejects the write (`transform.localScale assign attempt ... is not valid`) so that phase measured an unchanged transform |

That last point is worth restating because it cost a full round trip: **you
cannot test a nanimation by assigning `localScale = NaN` from script.** Unity's
setter validates and discards the value. Only the animation system, which does
not go through that setter, can write NaN. Any test that writes it by hand will
report a false negative.
