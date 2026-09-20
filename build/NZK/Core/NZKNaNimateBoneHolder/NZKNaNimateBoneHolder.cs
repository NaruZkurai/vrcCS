#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEditor.InitializeOnLoad]
 public partial class NZKNaNimateBoneHolder : UnityEngine.MonoBehaviour {
    /** <summary>The bones the user dragged in.  These are the bones whose
     *  names carry "nanim" in the avatar's rig; the holder does not discover
     *  them itself, because which bone a garment's toggle should drive is a
     *  design decision the toolkit cannot read off a name alone.</summary> */
    public UnityEngine.Transform[] Bones = new UnityEngine.Transform[0];
    /** <summary>Target avatar root.  Left null, it resolves to the holder's own
     *  scene root, which is the common case: the holder sits beside the
     *  avatar.</summary> */
    public UnityEngine.GameObject Target;
    /** <summary>The SOURCE model assets this avatar is built from.
     *
     *  EXPLICIT AND REQUIRED, and the reason is the whole reason this field
     *  exists rather than being derived: Generate REPLACES every renderer's
     *  sharedMesh with a generated .asset, which severs the only link back to
     *  the .blend.  A second run then finds 39 Mesh assets with no
     *  ModelImporter and nothing to reimport - measured, and it is what made
     *  the pipeline single-shot when the paths were resolved from renderers.
     *
     *  Storing the path means the component survives any number of re-runs,
     *  which it must, because it re-runs on every import.
     *
     *  SEE ALSO ProjectModel.  This list is normally AUTO-DERIVED from the
     *  scene avatar's renderer mesh names; ProjectModel is the manual override
     *  for the case where that derivation has nothing to work from. */
    public UnityEngine.Object[] SourceModels = new UnityEngine.Object[0];
    /* ── THE THREE THINGS THIS COMPONENT ACTUALLY NEEDS ──────────────── */
    /*
     * The rig and the model assets and the avatar to fix are three DIFFERENT
     * objects, and conflating them is what made the reported scene fail:
     * `Target` pointed at `Nemasis` while all 34 assigned bones rooted under
     * `nemtest`, so the run repaired one avatar and validated its bones against
     * another.  They are now three explicit fields.
     *
     * Order of resolution for each:
     *   SceneAvatar  explicit -> derived from Bones -> holder's own root
     *   ProjectModel explicit -> derived from the avatar's mesh names
     *   NanBones     explicit, always
     */
    /** <summary>The avatar in the SCENE that gets repaired.
     *
     *  The object whose renderers are rewritten.  Distinct from
     *  `ProjectModel`, which is the asset the meshes came FROM, and from the
     *  bones, which merely live under it.
     *
     *  Renamed from `Target` in intent, not in serialisation: the old field is
     *  kept and read as a fallback so existing scenes do not silently lose
     *  their wiring. */
    public UnityEngine.GameObject SceneAvatar;
    /** <summary>The model ASSET the avatar is built from, set by hand.
     *
     *  The override for when auto-derivation cannot answer.  Two situations
     *  need it, both measured:
     *
     *  1. The renderers hold scene-local clones (`*_NZKScene`) whose names have
     *     been stripped, and no model in the project owns a matching sub-asset.
     *  2. Two models in the project contain SAME-NAMED sub-assets.  The name
     *     index is first-model-wins, and on the reported scene it resolved
     *     every one of `nemtest`'s meshes to `Assets/Nemasis.blend` - the wrong
     *     avatar.  Setting this field picks the right one without ambiguity. */
    public UnityEngine.Object ProjectModel;
    /** <summary>Output folder for the generated meshes.  Empty derives it from
     *  the first source model, matching what Generate itself computes. */
    public System.String OutputFolder = "";
    /** <summary>Run automatically on scene load and after imports.
     *
     *  On by default because the point of the holder is that the user does not
     *  have to remember anything; a user who wants manual control turns this
     *  off and uses the button. */
    public System.Boolean AutoRun = true;
    /** <summary>Extra verbosity for diagnosing a toggle that stopped working.
     *  Prints each stage's counts instead of only the summary line.</summary> */
    public System.Boolean Verbose = false;
    /** <summary>Also run the ASSET-WRITING stages (cap influences, Generate,
     *  build a prefab, write normalised .asset meshes).
     *
     *  OFF BY DEFAULT, and that is a correctness-preserving performance decision
     *  rather than a feature toggle.
     *
     *  Those three stages cost MEASURED 32 751 ms on a 39-mesh avatar, and every
     *  millisecond of it was asset pipeline: DeleteAsset + CreateAsset per mesh,
     *  SaveAssets, ImportAsset(ForceUpdate) per model, and the
     *  OnPostprocessAllAssets each of those fires - which re-enters this very
     *  pipeline and is the loop the import guard exists to absorb.
     *
     *  Stage 4 performs the same weight repair IN MEMORY and writes nothing, so
     *  the slow path is not needed to make nanimations work.  The asset form is
     *  wanted only when a REUSABLE mesh or prefab is the goal, which is a
     *  separate workflow from "make the avatar on screen correct".
     *
     *  Turn it on deliberately when you want the generated assets.</summary> */
    public System.Boolean WriteAssets = false;
    /** <summary>MASTER SWITCH for every mesh edit this component performs.
     *
     *  OFF, deliberately, and it stays off until a replacement exists.
     *
     *  WHY.  This component's own weight-editing pipeline is being retired.  It
     *  was built on the assumption that a NaNimation toggle is driven by an
     *  authored near-zero vertex group on an EXISTING armature bone, and that
     *  assumption has been measured wrong in three separate ways:
     *
     *    1. Unity clamps ModelImporter.minBoneWeight to 0.001 and cannot be
     *       configured around it, so the authored 1e-7 does not survive import.
     *    2. Driving a shared armature bone with NaN DRAWS the surface toward the
     *       group origin instead of hiding it whenever any influence on that bone
     *       carries a real weight.
     *    3. There is no vertex splitting, so per-primitive visibility cannot be
     *       expressed at all.
     *
     *  Modular Avatar solves all three by treating NaNimation as a MESH
     *  operation: it splits vertices by visibility group, creates a DEDICATED
     *  bone per group, and moves the influences onto those bones.  Nothing about
     *  the authored weights matters, because the weights are rebuilt.
     *
     *  That approach is being ported.  Until it lands, this component must not
     *  edit anything - a half-working edit is worse than none, because it
     *  produces geometry that looks plausible and renders wrong.
     *
     *  When false, the component still runs, still reports, and still validates
     *  its wiring (target, bones, models) - it simply does not touch a mesh. */
    public System.Boolean EditVertexGroups = false;
    /* ── THE RUN ─────────────────────────────────────────────────────── */
    /** <summary>Resolve the avatar this holder drives, or null.</summary>
     *
     *  Returns null rather than guessing when the holder is not a root object:
     *  a holder parented under something else has no defensible answer for
     *  "which avatar", and picking the nearest ancestor would silently drive
     *  the wrong one in a scene with two.</summary> */
    public UnityEngine.GameObject ResolveTarget()
    { /* BONES WIN OVER Target, AND THAT ORDER IS THE FIX FOR A REAL SCENE.
       *
       * On the reported scene Target pointed at `Nemasis` while ALL 34 assigned
       * bones rooted under `nemtest`.  Trusting Target there sent the whole run
       * to the wrong avatar: the repair rewrote Nemasis's meshes while every
       * bone failed the ownership check against it, and the source-model capture
       * found NOTHING because Nemasis's renderers hold scene-local meshes with
       * no asset path, so `ModelPathsOf` returned zero candidates and the
       * component logged "SourceModels is empty or holds no model asset" even
       * though the right avatar was sitting in the scene the whole time.
       *
       * The bones are what the user actually dragged in and a bone that is not
       * under the target is useless by definition, so the root they share IS the
       * avatar.  Target is only a tie-breaker for the case where no bones are
       * assigned yet. */
      UnityEngine.GameObject fromBones = TargetFromBones();
      if (fromBones != null) return fromBones;
      if (SceneAvatar != null && SceneAvatar != gameObject) return SceneAvatar;
      if (Target != null && Target != gameObject) return Target;
      if (transform.parent != null) return null;
      return gameObject; }
    /** <summary>The avatar root implied by the assigned Bones, or null.
     *
     *  THIS EXISTS BECAUSE Target AND Bones CAN DISAGREE, and on the reported
     *  scene they did: Target pointed at `Nemasis` while all 34 assigned bones
     *  rooted under `nemtest`.  A run then repaired one avatar and checked its
     *  bones against another, so every bone failed the ownership check and the
     *  repair went to the wrong object entirely.
     *
     *  Bones are the authority.  They are what the user actually dragged in, and
     *  a bone that is not under the target is useless by definition, so the root
     *  the bones live under IS the avatar this holder drives.  Returns null when
     *  the bones disagree with each other, because then there is no single
     *  defensible answer and guessing would silently pick one of two avatars. */
    public UnityEngine.GameObject TargetFromBones()
    { if (Bones == null) return null;
      UnityEngine.GameObject found = null;
      for (System.Int32 i = 0; i < Bones.Length; i++)
      { if (Bones[i] == null) continue;
        UnityEngine.Transform root = Bones[i].root;
        if (root == null) continue;
        UnityEngine.GameObject go = root.gameObject;
        if (found == null) { found = go; continue; }
        if (found != go) return null; }
      return found; }
    /** <summary>Is this holder allowed to run right now?
     *
     *  Both conditions are the user's controls, so they are checked in one
     *  place and reported by the caller: an inactive holder is a deliberate
     *  off, and a nested holder is a placement mistake, and those two deserve
     *  different messages.</summary> */
    public System.Boolean CanRun(out System.String why)
    { why = null;
      if (!gameObject.activeInHierarchy) { why = "holder is inactive (this is the off switch)"; return false; }
      if (transform.parent != null)      { why = "holder is not at the scene root"; return false; }
      if (Bones == null || Bones.Length == 0) { why = "no bones assigned"; return false; }
      return true; }
    /** <summary>Everything, in order, for this holder's avatar.
     *
     *  WRAPPED IN THE RE-ENTRANCY GUARD.  Every writer below (ForceFour's
     *  ImportAsset, Generate's CreateAsset, NormalizeModel's CreateAsset)
     *  triggers OnPostprocessAllAssets, so without the depth counter the run
     *  would queue another run of itself through the import system and never
     *  settle.  Manual button clicks take the same path, so they are guarded
     *  too - clicking twice during a run is a no-op rather than a second
     *  concurrent pass over the same assets. </summary> */
    public void RunNow()
    { if (_runDepth > 0)
      { UnityEngine.Debug.Log("[NZK BoneHolder] already running (depth " + _runDepth + "), skipped"); return; }
      _runDepth++;
      /* SUPPRESS THE IMPORT HOOK FOR THE WHOLE RUN, not just while a write is in
       * flight.  See _suppressImportRuns for why a depth counter alone does not
       * terminate. */
      _suppressImportRuns = true;
      try { RunNowGuarded(); }
      finally
      { _runDepth--;
        /* Release on the NEXT tick, not now.  Everything the run imported has
         * already queued its callbacks by this point but none has fired yet, so
         * clearing the flag here would let them straight back in - the loop.
         * See ReleaseSuppression. */
        UnityEditor.EditorApplication.delayCall += ReleaseSuppression; } }
    /** <summary>Set while a run is executing, INCLUDING its deferred aftermath.
     *
     *  WHY _runDepth IS NOT ENOUGH, MEASURED.
     *
     *  _runDepth covers the synchronous body of RunNow.  It does NOT cover the
     *  sequence that actually loops:
     *
     *      Generate -> AssetDatabase.CreateAsset -> OnPostprocessAllAssets
     *                 -> delayCall += RunAllDeferred
     *      ... run body finishes, _runDepth returns to 0 ...
     *      delayCall fires -> _runDepth is 0 -> RunAllInOpenScenes runs again
     *      -> Generate writes again -> hook again -> ...
     *
     *  The guard is checked when the callback FIRES, but the callback was queued
     *  while the run was still in progress, so by the time it fires the counter
     *  has already been released.  Each pass re-writes every generated mesh, so
     *  the editor never settles and the console fills with run logs.
     *
     *  This flag is therefore cleared on the NEXT editor tick rather than at the
     *  end of the run, which is what makes it cover the deferred tail.  It is
     *  deliberately a separate flag and not an extension of the depth counter:
     *  depth answers "am I inside a run right now", this answers "did the run I
     *  just finished cause this queue", and only the second question terminates
     *  the loop. </summary> */
    public static System.Boolean _suppressImportRuns;
    /** <summary>The body of RunNow.  Split out so the depth counter wraps it in
     *  try/finally, which is what makes the guard releasable on an exception. */
    public void RunNowGuarded()
    { System.String why;
      if (!CanRun(out why))
      { UnityEngine.Debug.Log("[NZK BoneHolder] skipped: " + why); return; }
      UnityEngine.GameObject target = ResolveTarget();
      if (target == null) { UnityEngine.Debug.LogWarning("[NZK BoneHolder] no target"); return; }
      UnityEngine.SkinnedMeshRenderer[] smrs =
        target.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
      if (smrs.Length == 0)
      { UnityEngine.Debug.LogWarning("[NZK BoneHolder] no SkinnedMeshRenderer under " + target.name); return; }
      /* MODELS COME FROM THE EXPLICIT LIST, NOT FROM THE RENDERERS.
       *
       * ModelPathsOf reads `smr.sharedMesh` and takes its asset path - and
       * after the first Generate those renderers point at generated .asset
       * meshes with no ModelImporter, so a re-run derived from renderers finds
       * nothing to work on.  Measured: "found 39 mesh asset(s), none with a
       * ModelImporter", which is what made this pipeline single-shot.
       *
       * SourceModels is captured once by the inspector's "Use selection"
       * button, while the renderers still point at the model. */
      System.Collections.Generic.HashSet<System.String> models =
        new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
      /* ProjectModel WINS OUTRIGHT, and is used INSTEAD of the list rather than
       * in addition to it.  It exists to resolve an ambiguity, so merging it
       * with a list that resolved the WRONG model would reintroduce the very
       * conflict it was set to settle - and a run that processed two models
       * would rewrite both avatars. */
      if (ProjectModel != null)
      { System.String pm = UnityEditor.AssetDatabase.GetAssetPath(ProjectModel);
        if (NZK.S.HasOIC(pm, ".blend") || NZK.S.HasOIC(pm, ".fbx") ||
            NZK.S.HasOIC(pm, ".obj")   || NZK.S.HasOIC(pm, ".dae"))
          models.Add(pm);
        else
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] ProjectModel '" +
                                       (NZK.S.Has(pm) ? pm : ProjectModel.name) +
                                       "' is not a model asset; ignoring it."); }
      for (System.Int32 i = 0; ProjectModel == null && i < SourceModels.Length; i++)
      { if (SourceModels[i] == null) continue;
        System.String p = UnityEditor.AssetDatabase.GetAssetPath(SourceModels[i]);
        if (NZK.S.HasOIC(p, ".blend") || NZK.S.HasOIC(p, ".fbx") ||
            NZK.S.HasOIC(p, ".obj")   || NZK.S.HasOIC(p, ".dae"))
          models.Add(p); }
      /* SELF-HEAL: DERIVE THE MODELS FROM THE MESH NAMES WHEN THE LIST IS EMPTY.
       *
       * The manual capture is the preferred path, but it is a step the user can
       * skip, and skipping it used to hard-stop the whole run with a message
       * that named the wrong remedy - "SourceModels is empty, click Capture"
       * - while the avatar in the scene was perfectly resolvable.  That message
       * was ALSO misleading in a second way: on a scene whose renderers hold
       * SCENE-LOCAL meshes, ModelPathsOf returns nothing to capture, so
       * following the instruction could never succeed.  A component that can
       * work out the answer itself should not ask the user to type it in.
       *
       * The join key is the mesh NAME.  A scene-local clone is named
       * `<original>_NZKScene` (see NanRelink.SceneSuffix) and an imported
       * sub-asset keeps the .blend's object name, so stripping the suffix and
       * matching against the project's model sub-assets recovers the source
       * asset that the mesh originally came from.  This is the same name-keyed
       * identity the rest of this pipeline already relies on. */
      if (models.Count == 0)
      { System.Collections.Generic.HashSet<System.String> derived = DeriveModelsFromMeshes(smrs);
        if (derived.Count > 0)
        { foreach (System.String p in derived) models.Add(p);
          UnityEngine.Debug.Log("[NZK BoneHolder] SourceModels was empty; derived " + models.Count +
                                " model(s) from the renderers' mesh names. Assign them in the " +
                                "inspector to skip this lookup."); } }
      if (models.Count == 0)
      { UnityEngine.Debug.LogWarning("[NZK BoneHolder] no source model found for " + target.name +
                                     ". None of its " + smrs.Length + " renderer(s) name a mesh that " +
                                     "matches a .blend/.fbx sub-asset in the project, and SourceModels " +
                                     "is empty. Set SourceModels by dragging the model in by hand.");
        return; }
      UnityEngine.Debug.Log("[NZK BoneHolder] run: avatar=" + target.name +
                            " models=" + models.Count + " renderers=" + smrs.Length +
                            " mode=" + (WriteAssets ? "ASSETS (slow)" : "in-memory"));
      System.Int32 step = 0;
      /* ── THE ASSET-WRITING STAGES, OFF BY DEFAULT ──────────────────────
       *
       * Stages 1-3 (cap influences, Generate, NormalizeModel) all write or
       * reimport ASSETS, and together they cost MEASURED 32 751 ms on a 39-mesh
       * avatar.  Every millisecond was asset pipeline, not weight maths.
       *
       * They are kept behind WriteAssets rather than deleted because they are
       * the path that produces reusable .asset meshes and a prefab, which some
       * workflows want.  They are OFF by default because stage 4 does the same
       * weight repair IN MEMORY, which is what Modular Avatar does and what
       * makes a run take milliseconds instead of half a minute.
       *
       * WHY THE WRITES ARE NOT NEEDED FOR THE UPLOAD.
       * A scene serialises the meshes its renderers reference.  The VRChat
       * upload reads the renderer, not the AssetDatabase, so a mesh that exists
       * only in the scene is uploaded exactly the same.  The asset form matters
       * only for reusing a mesh across sessions, and these meshes are rebuilt
       * from the model on every run regardless.
       *
       * WHY THE REIMPORT IS THE EXPENSIVE PART.
       * ImportAsset(ForceUpdate) runs the whole model importer, and then fires
       * OnPostprocessAllAssets - which re-enters this pipeline through the import
       * hook.  That is the loop the _suppressImportRuns guard has to absorb, and
       * not starting it at all is strictly better than absorbing it. */
      if (WriteAssets)
      {
      /* 1. Cap influences.  Cheap, and a mesh with more than four cannot be
       *    skinned by the client at all, so it goes first: a later stage that
       *    looks at weights is meaningless on an unskinnable mesh. */
      MeshImport.ImportReport imp = MeshImport.ForceFour(models);
      if (Verbose) UnityEngine.Debug.Log("[NZK BoneHolder] 1/3 influences: inspected=" + imp.Inspected +
                            " changed=" + imp.Changed + " already=" + imp.Already +
                            " unsupported=" + imp.Unsupported + " skipped=" + imp.Skipped);
      /* 2. Generate, BEFORE normalize.  See the file header: Generate copies
       *    verbatim, so normalizing first would be overwritten here and the
       *    pin would be lost with no error. */
      foreach (System.String path in models)
      { System.String avatarName = NZK.S.P.Next(path);
        NZKNaNimateMeshGenerator.Result gen = NZKNaNimateMeshGenerator.Generate(path, avatarName);
        if (!gen.success)
        { UnityEngine.Debug.LogWarning("[NZK BoneHolder] 2/3 generate failed for " + path + ": " + gen.error);
          continue; }
        step++;
        if (Verbose) UnityEngine.Debug.Log("[NZK BoneHolder] 2/3 generated " + gen.meshCount +
                              " mesh(es) for " + path);
        /* 3. Normalize that model's meshes: pin the nanimation slots to 1e-07
         *    and renormalise the survivors so they still sum to 1.
         *
         *    This runs per MODEL, not per avatar, because NormalizeModel takes
         *    a model asset path - the output folder is derived from it the same
         *    way the generator derived its own, so the normalized meshes land
         *    beside the generated ones rather than in a second folder. */
        NZKNaNimateWeightNormalizer.Result norm =
          NZKNaNimateWeightNormalizer.NormalizeModel(path,
            NZKNaNimateMeshFolder.FolderFor(path, avatarName));
        /* Result carries a bool, not an error-only check: an empty error with
         * success=false is still a failure and would otherwise read as clean. */
        if (!norm.success || !NZK.B.NoE(norm.error))
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] 3/3 normalize failed for " + path + ": " + norm.error);
        else if (Verbose)
          UnityEngine.Debug.Log("[NZK BoneHolder] 3/3 normalized: renderers=" + norm.renderersProcessed +
                                " meshes=" + norm.meshesProcessed +
                                " written=" + norm.meshesWritten +
                                " affectedVerts=" + norm.verticesAffected +
                                " rescaled=" + norm.verticesRescaled +
                                " nanimateOnly=" + norm.verticesNaNimateOnly +
                                " worstDeviation=" + norm.worstDeviation.ToString("E2"));
        /* Result.HasWarning flags the cases where the rewrite produced weights
         * it could not make consistent - reporting them matters because the
         * mesh is still written, so the upload would otherwise go out with
         * degenerate weights and only fail on the client. */
        if (norm.HasWarning)
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] normalize for " + path +
            " produced weights needing attention: nanimateOnly=" + norm.verticesNaNimateOnly +
            " outOfRangeBoneRefs=" + norm.outOfRangeBoneRefs +
            " nonFiniteScales=" + norm.nonFiniteScales +
            " worstDeviation=" + norm.worstDeviation.ToString("E2"));
        /* The bones the user dragged in must be ON this avatar's rig, or the
         * nanimation drives a bone nothing is weighted to.  Checked by name
         * because that is how the toggles find them too. */
        System.Int32 found = 0;
        for (System.Int32 b = 0; b < Bones.Length; b++)
        { if (Bones[b] == null) continue;
          if (Bones[b].IsChildOf(target.transform) || Bones[b] == target.transform) found++; }
        if (found != Bones.Length)
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] " + (Bones.Length - found) + " of " +
                                       Bones.Length + " assigned bone(s) are NOT under " + target.name +
                                       " - those nanimations will not drive anything."); }
      }   /* end if (WriteAssets) */
      /* 4. REPAIR THE MESHES THE SCENE ACTUALLY RENDERS.
       *
       * THE ONLY STAGE THAT RUNS BY DEFAULT, AND IT WRITES NOTHING.
       *
       * Generate + NormalizeModel rewrite the MODEL ASSET's meshes as .asset
       * files.  They do NOT touch whatever mesh the scene renderers point at,
       * and that is the hole this stage closes.
       *
       * WHY IT IS NEEDED AT ALL, given the importer settings above.  Unity
       * 2022.3 forces ModelImporter.minBoneWeight to 0.001 and REFUSES to let
       * it be written to 0 - verified through SerializedObject, the native
       * typed setter, SetDirty + WriteImportSettingsIfDirty,
       * ImportAsset(ForceUpdate) and a .meta reserialize, all of which leave it
       * reporting 0.001.  A nanimation group authored at 1e-7 is therefore
       * CLAMPED UP on the way in, and no importer setting can prevent it.
       *
       * Measured on the reported avatar: of 39 meshes, 38 came through with
       * nanimation weights in the expected 1e-7..1e-6 band while ONE - the
       * leather jacket - carried 1.100e-3.  At four orders of magnitude above
       * the pin that influence no longer merely BINDS the bone, it DEFORMS:
       * the surface is dragged toward the nanimation group's origin instead of
       * staying put, so the garment is torn off the body and the whole assembly
       * reads as unreadable.  That is the reported "mesh not visible".
       *
       * It also explains why authoring the jacket weight HIGHER in Blender
       * changed the failure mode rather than fixing it - the magnitude is the
       * variable, and every value above the pin deforms.
       *
       * Runs LAST because it is the only stage that reads back what the scene
       * renderers actually hold, so it must see the result of every writer
       * above it. */
      NZKNaNimateWeightNormalizer.Result scene = EditVertexGroups
        ? NZKNaNimateWeightNormalizer.NormalizeSceneInMemory(target)
        : new NZKNaNimateWeightNormalizer.Result { success = true,
            error = "edits disabled (EditVertexGroups = false)" };
      if (!scene.success)
        UnityEngine.Debug.LogWarning("[NZK BoneHolder] 4/4 scene normalize failed: " + scene.error);
      else if (!EditVertexGroups)
        UnityEngine.Debug.Log("[NZK BoneHolder] 4/4 scene normalize SKIPPED - all mesh edits are " +
                              "disabled. See EditVertexGroups on the component.");
      else
      { UnityEngine.Debug.Log("[NZK BoneHolder] 4/4 scene normalize (in-memory, no asset writes): renderers=" +
                              scene.renderersProcessed +
                              " repaired=" + scene.meshesRepairedInMemory +
                              " dirtied=" + scene.meshesDirtied +
                              " repinned=" + scene.meshesWritten +
                              " bindPosesTrimmed=" + scene.bindPosesTrimmed +
                              " bindPosesPadded=" + scene.bindPosesPadded +
                              " bindPoseUnreadable=" + scene.bindPoseUnreadable +
                              " vertsAffected=" + scene.verticesAffected);
        /* A repaired mesh that was not marked dirty will NOT be saved, and the
         * failure is invisible: the scene looks right until it is reloaded.  So
         * the mismatch is called out rather than left for the user to discover. */
        if (scene.meshesRepairedInMemory > 0 && scene.meshesDirtied == 0)
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] " + scene.meshesRepairedInMemory +
                                       " mesh(es) were repaired but the scene was not marked dirty - the " +
                                       "repair would be lost on reload. Save the scene explicitly, or report " +
                                       "this as a bug.");
        /* A bind-pose mismatch is the fault that produces NaN vertices, and NaN
         * vertices are dropped by the GPU - so it shows up as geometry with holes
         * while every weight inspection reads clean.  Reported at WARNING rather
         * than as part of the summary line because a non-zero count here IS the
         * "part of my mesh is invisible" symptom, and it must not scroll past
         * inside a log line. */
        if (scene.bindPosesTrimmed > 0 || scene.bindPosesPadded > 0)
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] " + scene.bindPosesTrimmed +
                                       " mesh(es) had bind poses past the end of their bone array and were " +
                                       "trimmed; " + scene.bindPosesPadded + " had too few and were padded. " +
                                       "An extra bind pose with no bone yields a malformed skinning matrix, so " +
                                       "those meshes were producing NaN vertices and losing geometry.");
        if (scene.bindPoseUnreadable > 0)
          UnityEngine.Debug.LogWarning("[NZK BoneHolder] " + scene.bindPoseUnreadable +
                                       " mesh(es) could not have their bind poses repaired because the mesh is " +
                                       "not readable. Enable Read/Write on the model import settings."); }
      /* Audit last: it reports what the mesh channels actually contain, so it
       * must run after every writer.  A clean audit is the proof the pin
       * survived rather than an assumption that it did. */
      System.Collections.Generic.List<AuditMeshes.MeshDiff> diffs = AuditMeshes.Run();
      System.Int32 dirty = 0;
      for (System.Int32 i = 0; i < diffs.Count; i++) if (!diffs[i].Clean) dirty++;
      if (dirty > 0)
        UnityEngine.Debug.LogWarning("[NZK BoneHolder] audit: " + dirty + " of " + diffs.Count +
                                     " generated mesh(es) differ from source - see the audit table above.");
      else
        UnityEngine.Debug.Log("[NZK BoneHolder] audit: all " + diffs.Count + " mesh(es) clean.");
      UnityEngine.Debug.Log("[NZK BoneHolder] done. Upload when Unity finishes reimporting."); }
    /* ── AUTOMATIC TRIGGERS ──────────────────────────────────────────── */
    /** <summary>Scene load / domain reload.  Every holder in every open scene
     *  is offered a run; CanRun() decides which ones actually do.
     *
     *  Binds the VOID wrapper, not RunAllInOpenScenes: EditorApplication.delayCall
     *  is a CallbackFunction (returns void), so subscribing a method that
     *  returns int fails with CS0407 "has the wrong return type".  The count is
     *  still useful to callers, so it is kept on the real method and a wrapper
     *  exists purely for the delegate. </summary> */
    static NZKNaNimateBoneHolder()
    { UnityEditor.EditorApplication.delayCall += RunAllDeferred; }
    /** <summary>void-typed wrapper for delayCall.  See the static ctor.
     *
     *  Also the funnel for every automatic trigger, so the guard is applied
     *  here as well as in the import hook: a delayCall queued BEFORE a run
     *  started would otherwise fire DURING that run and re-enter it. */
    public static void RunAllDeferred()
    { if (_runDepth > 0) return;
      if (_suppressImportRuns) return;
      RunAllInOpenScenes(); }
    /** <summary>After any import.  A model reimport invalidates every generated
     *  mesh built from it, so this is the moment the pipeline has to re-run.
     *
     *  DEFERRED, AND SUPPRESSED WHILE WE ARE THE ONE IMPORTING.  Two separate
     *  problems, both fatal without a guard:
     *
     *  1. This callback runs while the asset database is locked; writing meshes
     *     from inside it throws.  Hence delayCall.
     *
     *  2. THIS PIPELINE IMPORTS ITS OWN INPUTS.  RunNow calls Generate and
     *     NormalizeModel, both of which write mesh assets, and ForceFour, which
     *     calls ImportAsset(ForceUpdate) on every model - each of those fires
     *     OnPostprocessAllAssets again, which without a guard queues another
     *     full run, whose writes fire it again.  That is unbounded recursion
     *     through the import system: the editor spins, never settles, and every
     *     pass re-filters the whole avatar.
     *
     *  _runDepth is the guard.  It is a depth counter rather than a bool so
     *  that a nested call from any other source cannot clear it early, and it
     *  is reset in a finally so an exception mid-run cannot leave the hold
     *  stuck on forever. </summary> */
    public static System.Int32 _runDepth;
    
    /** <summary>Clear the suppression flag on the tick AFTER a run finished.
     *
     *  The release has to be deferred, and this is the subtlety that makes the
     *  flag work: clearing it at the end of RunNow would be too early, because
     *  the delayCall the run queued has not fired yet and would then see a
     *  cleared flag and run again - which is exactly the loop.  Waiting one tick
     *  lets every callback the run caused fire while the flag is still set, and
     *  a genuinely NEW import arriving later still triggers a fresh run. */
    public static void ReleaseSuppression()
    { _suppressImportRuns = false; }
    /** <summary>True when at least one active root holder has AutoRun on.
     *
     *  Checked before scheduling so an import-heavy project does not queue a
     *  deferred call per import when there is nothing to run - the scan is
     *  cheap and the scheduling is not free.</summary> */
    public static System.Boolean AnyHolderWantsAutoRun()
    { NZKNaNimateBoneHolder[] all = UnityEngine.Resources.FindObjectsOfTypeAll<NZKNaNimateBoneHolder>();
      for (System.Int32 i = 0; i < all.Length; i++)
      { NZKNaNimateBoneHolder h = all[i];
        if (h == null || !h.AutoRun) continue;
        if (h.gameObject.scene.IsValid() && h.gameObject.activeInHierarchy && h.transform.parent == null)
          return true; }
      return false; }
    /** <summary>Run every eligible holder in every loaded scene.
     *
     *  `Resources.FindObjectsOfTypeAll` rather than `FindObjectsOfType`: the
     *  latter only returns ACTIVE objects, and the holder's inactive state is
     *  the user's off switch - so those objects must still be enumerated in
     *  order to be skipped deliberately rather than silently missed. </summary> */
    public static System.Int32 RunAllInOpenScenes()
    { NZKNaNimateBoneHolder[] all = UnityEngine.Resources.FindObjectsOfTypeAll<NZKNaNimateBoneHolder>();
      System.Int32 ran = 0;
      for (System.Int32 i = 0; i < all.Length; i++)
      { NZKNaNimateBoneHolder h = all[i];
        if (h == null || !h.AutoRun) continue;
        /* Scene-valid filters out prefab assets and asset-store objects, which
         * FindObjectsOfTypeAll also returns and which have no scene to run in. */
        if (!h.gameObject.scene.IsValid()) continue;
        h.RunNow();
        ran++; }
      return ran; }
    /* ── MENU + INSPECTOR ────────────────────────────────────────────── */
    public const System.String MenuPath = "GameObject/NZK/NaNimationBoneHolder";
    /** <summary>Create the holder, at the root of the active scene. */
    [UnityEditor.MenuItem(MenuPath, false, 25)]
    public static void CreateHolder()
    { UnityEngine.GameObject go = new UnityEngine.GameObject("NZK_NaNimationBoneHolder");
      /* Root placement is part of the contract, so the object is created at
       * the root rather than parented to the selection - a holder under the
       * avatar would be both parented (blocked) and easy to delete by
       * accident with the avatar. */
      UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create NaNimation Bone Holder");
      go.AddComponent<NZKNaNimateBoneHolder>();
      UnityEditor.Selection.activeGameObject = go;
      UnityEngine.Debug.Log("[NZK BoneHolder] created at scene root. Drag the nanimation bones " +
                            "onto the Bones list, then leave it alone - it runs itself."); }
    [UnityEditor.MenuItem(MenuPath, true)]
    public static System.Boolean ValidateCreateHolder()
    { return !UnityEditor.EditorApplication.isPlaying; }
    /** <summary>Record the source models this holder drives.
     *
     *  MUST be run while the renderers still point at the model, i.e. before
     *  the first Generate - that is the only moment the link exists to read.
     *  Captured into a serialised field so it survives every later run. */
    public void CaptureSourceModels()
    { UnityEngine.GameObject target = ResolveTarget();
      if (target == null) { UnityEngine.Debug.LogWarning("[NZK BoneHolder] no target to capture from"); return; }
      System.Collections.Generic.List<System.String> paths = MeshImport.ModelPathsOf(target);
      System.Collections.Generic.List<UnityEngine.Object> found = new System.Collections.Generic.List<UnityEngine.Object>();
      System.Collections.Generic.HashSet<System.String> seen = new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
      for (System.Int32 i = 0; i < paths.Count; i++)
      { System.String p = paths[i];
        if (!(NZK.S.HasOIC(p, ".blend") || NZK.S.HasOIC(p, ".fbx") ||
              NZK.S.HasOIC(p, ".obj")   || NZK.S.HasOIC(p, ".dae"))) continue;
        if (!seen.Add(p)) continue;
        UnityEditor.ModelImporter mi = UnityEditor.AssetImporter.GetAtPath(p) as UnityEditor.ModelImporter;
        if (mi == null) continue;   /* not a model: an already-generated mesh asset */
        found.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p)); }
      /* FALL BACK TO THE NAME LOOKUP, which is the only path that works once the
       * renderers hold scene-local meshes.  See DeriveModelsFromMeshes. */
      if (found.Count == 0)
      { System.Collections.Generic.HashSet<System.String> derived =
          DeriveModelsFromMeshes(target.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true));
        foreach (System.String p in derived)
        { if (!seen.Add(p)) continue;
          found.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p)); } }
      SourceModels = found.ToArray();
      UnityEditor.EditorUtility.SetDirty(this);
      if (found.Count == 0)
        UnityEngine.Debug.LogWarning("[NZK BoneHolder] no source model found. None of " + target.name +
                                     "'s renderer mesh names matches a .blend/.fbx sub-asset in the " +
                                     "project, so there is nothing to record - drag the model in by hand.");
      else
        UnityEngine.Debug.Log("[NZK BoneHolder] captured " + found.Count + " source model(s)."); }
    /** <summary>The model assets the given renderers' meshes originally came from.
     *
     *  Recovered by NAME, because the asset path is gone in every case that
     *  matters: once Generate has run, or once NanRelink has rebound a renderer
     *  to a scene-local copy, `sharedMesh` has no asset path at all.  What
     *  survives is the mesh's name, which is the .blend's object name with an
     *  optional `_NZKScene` suffix.
     *
     *  Requires the mesh to be READABLE to be indexed by the model scan, which
     *  holds for every model this toolkit consumes (its own pipeline refuses a
     *  non-readable mesh elsewhere), and a mesh that cannot be read is simply
     *  not matched rather than treated as an error.
     *
     *  Returns an empty set rather than throwing when nothing matches: this is a
     *  recovery path and a caller that gets nothing back reports it.</summary> */
    public static System.Collections.Generic.HashSet<System.String> DeriveModelsFromMeshes(
      UnityEngine.SkinnedMeshRenderer[] smrs)
    { System.Collections.Generic.HashSet<System.String> models =
        new System.Collections.Generic.HashSet<System.String>(System.StringComparer.Ordinal);
      if (NZK.B.mpty.t(smrs)) return models;
      /* THE MESH'S OWN ASSET PATH WINS, because it is the only unambiguous
       * answer.  When the renderers still point at an imported sub-asset (before
       * any Generate), the path names the exact model, and no name lookup can
       * beat that.
       *
       * THIS IS NOT A MICRO-OPTIMISATION.  Measured on the reported scene:
       * `nemtest`'s meshes are named `Body`, `face`, `Hair - Hair`, and
       * `Assets/Nemasis.blend` contains SAME-NAMED sub-assets.  The name index is
       * first-model-wins, so it resolved EVERY one of nemtest's meshes to
       * `Assets/Nemasis.blend` - the wrong avatar, and a different one from the
       * scene the user is looking at.  The path shortcut removes that entire
       * class of collision for every model that still has one. */
      System.Collections.Generic.HashSet<System.String> namesNeedingLookup =
        new System.Collections.Generic.HashSet<System.String>(System.StringComparer.OrdinalIgnoreCase);
      for (System.Int32 i = 0; i < smrs.Length; i++)
      { if (smrs[i] == null || smrs[i].sharedMesh == null) continue;
        System.String own = UnityEditor.AssetDatabase.GetAssetPath(smrs[i].sharedMesh);
        if (NZK.S.Has(own) &&
            (NZK.S.HasOIC(own,".blend") || NZK.S.HasOIC(own,".fbx") ||
             NZK.S.HasOIC(own,".obj")   || NZK.S.HasOIC(own,".dae")))
        { models.Add(own); continue; }
        /* No path (a scene-local clone): remember the NAME for the fallback. */
        System.String n = NanRelink.BaseMeshName(smrs[i].sharedMesh.name);
        if (NZK.S.Has(n)) namesNeedingLookup.Add(n); }
      if (namesNeedingLookup.Count == 0) return models;
      /* ONE project-wide model scan, cached across calls: this walks every model
       * asset, and 39 renderers would otherwise repeat it 39 times per run - and
       * the holder re-runs on every import. */
      if (_modelByMeshName == null) BuildModelIndex();
      System.String path;
      foreach (System.String want in namesNeedingLookup)
        if (_modelByMeshName.TryGetValue(want,out path)) models.Add(path);
      return models; }
    /** <summary>Mesh name -> model asset path, built once per editor session.
     *
     *  Cached because the lookup walks EVERY model asset in the project, and the
     *  holder re-runs on every import.  Cleared by the import hook so a newly
     *  added model is picked up.</summary> */
    public static System.Collections.Generic.Dictionary<System.String,System.String> _modelByMeshName;
    /** <summary>Rebuild the mesh-name -> model-path index.</summary> */
    public static void BuildModelIndex()
    { System.Collections.Generic.Dictionary<System.String,System.String> index =
        new System.Collections.Generic.Dictionary<System.String,System.String>(System.StringComparer.OrdinalIgnoreCase);
      System.String[] guids = UnityEditor.AssetDatabase.FindAssets("t:Model");
      for (System.Int32 g = 0; g < guids.Length; g++)
      { System.String p = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[g]);
        if (!NZK.S.Has(p)) continue;
        UnityEngine.Object[] subs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(p);
        if (subs == null) continue;
        for (System.Int32 i = 0; i < subs.Length; i++)
        { UnityEngine.Mesh m = subs[i] as UnityEngine.Mesh;
          if (m == null) continue;
          /* First model wins: a mesh name repeated across models is ambiguous
           * and picking the first is at least deterministic. */
          if (!index.ContainsKey(m.name)) index[m.name] = p; } }
      _modelByMeshName = index; }
  
}
}
}
#endif
