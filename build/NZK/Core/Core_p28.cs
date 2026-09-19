namespace NZK
{
public static partial class Core {
 
  /* HBFlag.cs
   * Flag-based property override system for YamlBlocks.
   * Allows setting per-property overrides at Global,PerLayer,or PerBlock scope.
   * Flags resolve to YAML values when building blocks from the hierarchy.
   */
  /** <summary>Scope at which a flag override applies.</summary> */
  
  /** <summary>One property override entry.</summary> */
  
  /** <summary>Attach to any C_AviGenerator UnityEngine.GameObject to add flag overrides. */
  /** On GestureGenerator root: global flags. */
  /** On LYR_ layer: per-layer flags.</summary> */
  
  /** <summary>Utility: find HBFlags on a UnityEngine.GameObject or its gesture generator root.</summary> */
  
  /* BoneAssignment.cs
   * Attached to each bone UnityEngine.Transform in Armature_Armature and Armature_Stripped.
   * Stores which humanoid bone slot this UnityEngine.Transform maps to,* plus the generated UnityEngine.Avatar asset reference.
   */
  
  /* Static registry — associates BoneAssignment data with GameObjects without MonoBehaviour */
  
  /* NzkArmatureLinkLog.cs
   * Runtime component storing ArmatureLink bake data.
   * Created as a child of GEN_{name}/HB_ArmatureLinks/{sourceName}
   * Links the moved object to its new parent on the avatar.
   */
  
  /* Static registry — associates C_ArmatureLinkLog data with GameObjects without MonoBehaviour */
  
  /* Static registry — associates AviLink data with GameObjects */
  
  /* Static registry — associates C_AviGenerator data with GameObjects */
  
  
  /* AviLink.cs
   * Root component for the NZK generator hierarchy.
   * Links generated tools (SPS,Toggle,FullController) to an avatar.
   *
   * Generator mode: creates full child hierarchy for building a new avatar.
   * Updater mode: references an existing avatar,only bakes checked generators.
   */
  
  /* BakedVFData.cs — VRCFury dead code, not maintained */
#if false
  /** <summary>Stores baked MaterialSet data (material swaps)</summary> */
  
  /** <summary>Stores baked ObjectToggle data (enable/disable)</summary> */
  
  /** <summary>Stores baked PhysBone/UnityEngine.Collider data</summary> */
  
  /** <summary>Stores baked Constraint data</summary> */
  
  /** <summary>Stores baked Contact Sender/Receiver data</summary> */
  
  /** <summary>Stores baked UnityEditor.MenuItem/MenuInstall data</summary> */
  
  /** <summary>Stores baked LightSource data</summary> */
  
  /** <summary>Stores baked ParameterDriver data</summary> */
  
  /** <summary>Stores baked DepthAnimation data</summary> */
  
/* BakedControllerSlot.cs — active component, not dead code */
#endif
  
  /* MeshSplitter.cs
   * UnityEngine.Mesh splitting by vertex groups.
   * Runtime component storing split configuration.
   */
  /* ---- vertex group split entry ---------------------------- */
  
  
  /* BlendshapeToggle.cs
   * Blendshape toggle components for the NZK pipeline.
   * Runtime: BlendshapeToggleDefinition,MultiBlendshapeToggleDefinition,ToggleGroup
   * UnityEditor.Editor code moved to BlendshapeToggle.editor.cs: BlendShapeToggleGenerator,UnityEditor.CustomEditor,Menu Items
   */
  
  
  
  
  /* ToggleBuilder.cs
   * Multi-toggle builder with link dependency resolution.
   * Holds multiple toggle entries,processes links between them,* and generates all toggles + DBT integration in one Build() call.
   * Runtime: ToggleLinkType,ToggleLink,ToggleBuilder
   * UnityEditor.Editor code moved to ToggleBuilder.editor.cs.
   */
  
  
  
  /* GestureLayerConfig.cs
   * VRChat gesture layer configuration system for NZK toolkit v4.
   * Runtime components + UNITY_EDITOR generator.
   * Layer structure: Layer0=LeftHand,Layer1=RightHand,Layer2=LeftHandIdle,Layer3=RightHandIdle
   * Save path: Assets/NZK_Generated/Controllers/{AvatarName}/Gesture.controller
   */
  /* ---- serializable types -------------------------------------------------- */
                 /* Threshold value */
  /* ---- runtime components ------------------------------------------------- */
        /* Transition duration in seconds */
            /* Layer weight */
  
  /* ProxySync.cs
   * Place under Assets/ (NOT inside an UnityEditor.Editor/ subfolder).
   * Pure data UnityEngine.MonoBehaviour -- no editor API,no #if guards.
   * All swap logic lives in UnityEditor.Editor/ObjectSync.cs.
   *
   * Menu shortcuts (defined in UnityEditor.Editor/ObjectSync.cs):
   *   UnityEngine.GameObject > NZK > Create SceneMesh Swaps Root
   *   UnityEngine.GameObject > NZK > Add Scene UnityEngine.Mesh Swap Entry
   */
  
  
  /* SpsConfig.cs
   * SPS (Stoned Physics System) configuration component.
   * Stores all data needed for SPS generation.
   */
  
  
  
  
  
  /* SpsSocketConfig.cs
   * Per-socket configuration component for the SPS (Socket Positioning System) baking pipeline.
   * Each instance represents one socket (Blowjob,Pussy,Anal,Handjob,Special,Feet,etc.),* extracted from VRCFury DPS Plug components during the bake process.
   */
  /** <summary> */
  /** Per-socket configuration extracted from VRCFury DPS Plug components during SPS baking. */
  /** Stores socket name,menu path,UnityEngine.Transform reference,plug geometry,and generated animation clips. */
  /** </summary> */
  
  /* CircleMenu.runtime.cs
   * Consolidated runtime components for the Circle Menu system:
   *   CircleMenuBuilder — clones source VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu into gen folder
   *   BakedMenuSlot  — stores a single baked circle-menu page
   *   ProxyMenuSlot  — represents a single VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu control entry
   *
   * UnityEditor.Editor counterparts live in:
   *   UnityEditor.Editor/systems/builder/NZK.Core.systems.builder.CircleMenu.editor.cs
   */
  /* ── BakedMenuSlot ────────────────────────────────────────────────── */
  /* Runtime component storing a single baked circle-menu page. */
  /* Created under GEN_{name}/HB_Circle_Menu/{Menu|Next|Next2|...} */
  /* Each slot holds one VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu with up to 8 controls. */
  
  /* ── ProxyMenuSlot ───────────────────────────────────────────────── */
  /* Runtime component representing a single VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu control entry. */
  /* Created as a child of a BakedMenuSlot page under GEN/HB_Circle_Menu/. */
  /* Mirrors the data from VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control without modifying the original. */
  
#if UNITY_EDITOR
  /* ── CircleMenuBuilder ───────────────────────────────────────────── */
  /* UnityEditor.Editor component that clones a source VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu (and all sub-menus) */
  /* into the gen folder's menus/ directory,creating a proxy hierarchy under */
  /* GEN/HB_Circle_Menu/ for visual reference. On re-build,existing clones */
  /* are updated in-place. Circular references are detected via menusBuilt. */
  
#endif
  /* paramiters.runtime.cs
   * Centralized parameter runtime data structures for the NZK Toolkit.
   * All parameter-related structs,constants,and validation methods
   * that are used at runtime (scene objects,components) live here.
   *
   * Note: ACParam and ConditionEntry remain in C_AviGenerator.cs
   * (they're tightly coupled to that component).
   *
   * UnityEditor.Editor-only parameter building/merging code lives in:
   *   UnityEditor.Editor/NZK.Core.builder.paramiters.editor.cs
   */
  /* ── Gesture parameter constants ────────────────────────────────── */
  /* Used by GestureLayerConfig for VRChat standard gesture parameters. */
  
  /* ── NZK path constants (runtime-accessible) ────────────────────── */
  
  /* ── Parameter name sanitization ────────────────────────────────── */
  
  /* NZK.Core.IF_UE.cs
   * Runtime stubs for IF_UE wrappers — no-ops for non-editor builds.
   * Real implementations in NZK.Core.IF_UE.editor.cs (now in same assembly).
   */
  
  /* NZK.Core.IF_UE.editor.cs
   * UnityEditor.Editor-only implementations for IF_UE wrappers.
   */
#if UNITY_EDITOR
  
/* UnityEditor.Editor/ObjectSync.cs
 * UnityEditor.Editor-only companion to ProxySync.cs.
 * Handles all swap logic,hooks,inspector and menu items.
 * Triggers on: scene open,assembly reload,material/mesh re-import.
 */
  /* ======================================================================= */
  /* Swap operations                               */
  /* ======================================================================= */
  
  /* ======================================================================= */
  /* Scene-open and assembly-reload hooks                   */
  /* ======================================================================= */
  
  /* ======================================================================= */
  /* Asset postprocessor: re-trigger on material / mesh re-imports       */
  /* ======================================================================= */
  
  /* ======================================================================= */
  /* Custom inspector                              */
  /* ======================================================================= */
  
  /* ======================================================================= */
  /* Menu items — defined in NZK.Toolkit.cs via MenuDO */
  /* ======================================================================= */
#endif
  //NZK: FZK = True;
  /*rules: no // style comments,*terse code,precise system agnostic fully-qualified names eg: c.foo must always be ns.a.b.c.fooasuming ns.a.b.c.foo doesnt have a longer/more fully qualified in the event another script uses a.b.c.foo to prevent colisions,ternary or arrow logic or shorthand or direct function filling If possible,no single defined variables unless in  NZK.Core.consts.
    pythonic double spaced tabs syntax.
    no lines contain garbage or visually emptytokens. eg: tabs{\n or tabs}\n exclusively,no definition can contain multiword names unless compoundword like Blendtree or blendshape,if a compoundword uses a shorthand like bt = blendtree use it,empty defs r ok encouraged to use english like higharchies in names,asume correctly used,use cs9,no definition can contain more than 4 lines,no more than 3 concepts per function,asume correctly used defs.
    all variables are used as described,complex & concise > noob friendly
    private defs r banned,
   *Exceptions:
   words that dont REQUIRE shorthands:UnityEditor.Animations.BlendTree & VertexGroup,GllC = Get or Create
   menu activation entry points: as many concepts  or lines,paramiters: asmanywordsasneeded to be clear,No magic strings — "generate","toggle","NaNimation","On Off" in MenuDO/Toggle.Create are raw literals; should be consts.
    ?? over null-ternary — implied by the shorthand rule.
    Expression body (=>) preferred over { return ...; } — implied by arrow logic rule.
    Single responsibility per method — implied by 3-concept limit.
    Constants over inline literals for repeated values — implied by having Consts; violated by System.String "/DBT","/BlendTrees","/Animations" scattered inline.
  */
#if UNITY_EDITOR
// Menu items defined in NZK.Toolkit.cs via MenuDO routing
// Menu path constants defined in Vars.Consts and Vars.Names.Get
  /* ==================================================================== */
  /* NZK UI — reusable editor elements                  */
  /* ==================================================================== */
   /* NZK.Core.UI */
  // Populates BakedMenuSlot components on avatar root from generated controls
  
#endif
  /* C_AviGenerator.cs
  **allways update comments for ascii ui first**
   * Live-ish baker for UnityEngine.Avatar building.
   * Modes: This (selector) / Root / MeshBuilder / ArmatureBuilder / AviRootBuilder / MeshGenerator
   * Drag-object arrays (like BoneViewer) use standard Unity array fields.
   */
  
  
  /** <summary>Stores one raw YAML document block from the reference controller. */
  /** Created during import,consumed during bake to reconstruct the .controller file.</summary> */
  
  /* Controller Builder block types (CbBlockType enum) */
  
   /* AnimatorControllerParameterType as int: 1=Float,3=Int,4=Bool,9=Trigger */
   /* 0=Override,1=Additive */
  
  /** <summary>Sub-mode for AnimatorBuilder — which VRChat animator layer this generates.</summary> */
  
  
  
  
  /* AVController.cs
   * UnityEngine.Avatar/generator bake logic extracted from C_AviGenerator.
   * All methods are static — they take a C_AviGenerator hb parameter for context.
   */
  
  /* AXController.cs
   * Gesture/expression/FX controller bake logic extracted from C_AviGenerator.
   * All methods are static — they take a C_AviGenerator hb parameter for context.
   * UnityEditor.Editor-only methods live in AXController.editor.cs.
   */
  
  /* AVController.editor.cs
   * UnityEditor.Editor-only UnityEngine.Avatar controller/generator bake logic.
   * NOTHING OTHER THAN UnityEngine.Avatar CONTROLER LOGIC IS ALLOWED IN THIS FILE.
   * Partial class matching AVController in Core/.
   */
#if UNITY_EDITOR
  
#endif
  /* AXController.editor.cs
   * UnityEditor.Editor-only partial for AXController — methods requiring UnityEditor APIs.
   */
#if UNITY_EDITOR
  
#endif
  /* NZK.Core.bakeAll.editor.cs
   * BakeAll — the top-level bake orchestrator.
   * Extracted from AVController.editor.cs because BakeAll coordinates
   * avatar,armature,mesh,controller,toggle,and VRCAD baking —
   * far more than just UnityEngine.Avatar controller logic.
   *
   * All sub-bake methods remain in their respective files:
   *   AVController  — UnityEngine.Avatar root,armature,expressions,FX,toggles
   *   MergeCore   — mesh merge + material links
   *   AXController  — gesture/FX controller layers
   */
#if UNITY_EDITOR
  
#endif
  /* C_AviGenerator.Yaml.editor.cs
   * UnityEditor.Editor-only YAML sync methods stripped from C_AviGenerator.Yaml.cs.
   * UnityEditor.AssetDatabase operations require UNITY_EDITOR context.
   */
#if UNITY_EDITOR
  
#endif
  /* C_AviGenerator.Yaml2.editor.cs
   * UnityEditor.Editor-only methods stripped from C_AviGenerator.Yaml2.cs.
   * UnityEditor.AssetDatabase and UnityEditor.Animations.AnimatorController references require UNITY_EDITOR context.
   */
#if UNITY_EDITOR
  
#endif
  /* C_AviGenerator.editor.cs
   * Custom inspector for C_AviGenerator — full editor UI.
   * Extracted from C_AviGenerator.cs.
   */
#if UNITY_EDITOR
  
  /* ── C_AviGenerator partial editor methods ─────────────────────── */
  
#endif
  /* NZK.Core.systems.builder.core.cs
   * Builder core utilities — mesh helpers used by merge and split operations.
   */
#if UNITY_EDITOR
  
#endif
  /* NZK.Core.systems.builder.meshes.split.cs
   * UnityEngine.Mesh split operations — extract vertex groups into separate meshes.
   */
#if UNITY_EDITOR
  
#endif
  /* NZK.Core.systems.builder.meshes.merge.cs
   * UnityEngine.Mesh merge logic — combines multiple mesh sources into one mesh,* deduplicating shared materials by default unless UseUniqueMaterial is set.
   */
#if UNITY_EDITOR
  
#endif
  /* NZK.Core.systems.builder.avatarroot.cs
   * UnityEngine.Avatar root builder — creates/updates the UnityEngine.Avatar root UnityEngine.GameObject,* configures VRC.SDK3.Avatars.Components.VRCAvatarDescriptor,UnityEngine.Animator,and resolves the pipeline
   * (blueprint ID) from PipelineID children.
   */
#if UNITY_EDITOR
  
#endif
  /* NZK.Core.baking.cs
   * Centralized baking utilities for ArmatureLink,VRCFury Toggles,and FX merging.
   * Referenced by C_AviGenerator,SpsSocketBaker,and any other baking consumer.
   * All methods live in NZK.Core.Systems.Baking.
   */
#if UNITY_EDITOR
#endif
  /* ControllerMerger.cs
   * Reusable utility for merging multiple Unity AnimatorControllers together.
   * Merges parameters,layers,states,and transitions from source controllers
   * into a target controller.
   *
   * Usage:
   *   var merger = new ControllerMerger(targetCtrl);
   *   merger.Merge(sourceCtrl1);
   *   merger.Merge(sourceCtrl2);
   *   merger.Save();
   *
   * Or one-shot:
   *   ControllerMerger.MergeControllers(targetPath,sourceCtrl1,sourceCtrl2);
   */
#if UNITY_EDITOR
  /** <summary> */
  /** Merges multiple AnimatorControllers into one,combining parameters,*/
  /** layers,states,and transitions from all sources. */
  /** </summary> */
  
#endif
  /* GestureLayerImporter.editor.cs
   * Reads an example UnityEditor.Animations.AnimatorController and recreates its full structure
   * as C_AviGenerator hierarchy objects under the HB_Gesture UnityEngine.GameObject.
   * The existing BakeGestureLayers() can then generate a controller
   * identical to the example.
   */
#if UNITY_EDITOR
  
#endif
#if UNITY_EDITOR
#endif
  /* BlendshapeToggle.editor.cs
   * UnityEditor.Editor-only code for BlendshapeToggle components.
   * Extracted from BlendshapeToggle.cs.
   * Runtime: BlendshapeToggleDefinition,MultiBlendshapeToggleDefinition,ToggleGroup
   * UnityEditor.Editor: BlendShapeToggleGenerator,BlendshapeToggleDefinitionEditor,BlendshapeMenuItems
   */
#if UNITY_EDITOR
  /* ---- local helpers (inline from UnityEditor.Editor-assembly Vars to avoid cross-assembly ref) ---- */
  
  
  
  
#endif
  /* NZKDisableFury.cs
   * Toggle overrides for VRCFury and NDMF in both playmode and uploads.
   * Each toggle is independent via EditorPrefs.
   *
   * Menu structure:
   *   NZK / VRCFury / Disable in playmode
   *   NZK / VRCFury / Disable in uploads
   *   NZK / NDMF  / Disable in playmode
   *   NZK / NDMF  / Disable in uploads
   */
#if UNITY_EDITOR
  
#endif
  /* NZKGraphicsSetup.cs
   * Ensures Vulkan is the primary graphics API on Linux for UnityEditor.Editor and builds.
   * Checks startup rendering API — warns if not Vulkan.
   */
#if UNITY_EDITOR
  
#endif
#if UNITY_EDITOR
#endif
  /* NzkFbxExport.cs
   * Exports the two armatures (Armature_Armature + Armature_Stripped)
   * as FBX files with BoneAssignment components and UnityEngine.Avatar asset references.
   * Uses Autodesk.Fbx SDK for native FBX export.
   */
#if UNITY_EDITOR
  
#endif
#if UNITY_EDITOR
#endif
#if UNITY_EDITOR
 
#endif
  /* NZK.Core.Meshes.cs
   * Generic mesh merging core.
   * Data types + merge logic.
   */
  
  
  
  
#if UNITY_EDITOR
#endif
  /* MeshSplitterOps.cs
   * UnityEngine.Mesh splitting ops — editor-only logic placed in runtime folder
   * so HigharchyBaker's BakeMesh (#if UNITY_EDITOR) can call it.
   */
#if UNITY_EDITOR
  
#endif
  /* ══════════════════════════════════════════════════════════════════════════
   * SPS Socket System — C_NzkSpsSocket,C_SpsSocketGenerator,SpsSocketBaker,* SpsSocketBakeService,SpsHapticUtils,SpsHapticContactsService
   * ══════════════════════════════════════════════════════════════════════════ */
#if UNITY_EDITOR
/* ── C_NzkSpsSocket ─────────────────────────────────────────────────────── */
  
/* ── C_SpsSocketGenerator ───────────────────────────────────────────────── */
  
  
/* ── SpsAddLight ──────────────────────────────────────────────────────── */
  
/* ── SpsSocketBakeResult ──────────────────────────────────────────────── */
  
/* ── SpsHapticUtils ───────────────────────────────────────────────────── */
  
/* ── ReceiverRequest ──────────────────────────────────────────────────── */
  
/* ── SpsHapticContactsService ─────────────────────────────────────────── */
  
/* ── SpsSocketBakeService ─────────────────────────────────────────────── */
  
/* ── SpsSocketBaker ───────────────────────────────────────────────────── */
  
/* ══════════════════════════════════════════════════════════════════════════
 * SPS2 Shader Pipeline — Marker meshes, material property config,
 * tracking manifest, git-friendly change detection.
 * ══════════════════════════════════════════════════════════════════════════ */
/* ── SpsMarkerService ────────────────────────────────────────────────── */
/* Creates tiny marker meshes + materials at socket positions (SPS2 shader markers).
 * Works with Hidden/NZK/SpsSocketMarker shader (user must provide or use fallback).
 * Each marker sits on the avatar's body at the socket location. */
  
/* ── SpsShaderConfigurer ─────────────────────────────────────────────── */
/* Configures material properties on the avatar's SkinnedMeshRenderer
 * at the socket location. Generates animation clips for SPS2 shader props. */
  
/* ── SpsManifest ─────────────────────────────────────────────────────── */
/* Tracks ALL generated objects and shaders in a JSON manifest file.
 * Placed at Assets/!_NZK_Generated/{avatar}/SPS/sps_manifest.json
 * Git-friendly: diff the manifest to detect settings changes. */
  
  
  
/* ── VfConversion ────────────────────────────────────────────────────── */
/* Routes detected VRCFury components to NZK equivalents, or backs them up. */
  
/* ── SpsResolverService ──────────────────────────────────────────────── */
/* Configures the SPS2 resolver + player ID on the avatar's body meshes.
 * This allows SPS2 plugs to detect our sockets via GPU resolver. */
  
/* ── C_NzkSpsSocket Editor (VF-style UI) ───────────────────────────────── */
  
/* ── SpsSocketBakerMenus ──────────────────────────────────────────────── */
#endif
}
}
