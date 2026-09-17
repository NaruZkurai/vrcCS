namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Higharchy Baker")]
  public partial class C_AviGenerator : UnityEngine.MonoBehaviour {
    public E_AviGeneratorMode mode = E_AviGeneratorMode.This;
    public E_AnimatorSubMode animatorSubMode = E_AnimatorSubMode.Custom;
    public System.Collections.Generic.List<UnityEngine.GameObject> animatorLayers = new();
    public System.String avatarRootName = "Avi Root Root";
    public UnityEngine.GameObject NZKC_GO_AviRoot;
    public System.Boolean cloneOriginalArmature = true;
    public System.Boolean autoGenerateArmature;
    public System.Boolean removeEndBones = true;
    public UnityEngine.Transform armatureRoot;
    public System.Collections.Generic.List<UnityEngine.GameObject> meshSources = new();
    public System.Collections.Generic.List<UnityEngine.Material> sharedMaterials = new();
    public System.String entryName = "";
    public System.Collections.Generic.List<UnityEngine.GameObject> meshGenSources = new();
    public System.Boolean useUniqueMaterialSlots;
    public UnityEngine.GameObject faceMesh;
    public System.Boolean useForVrcadFace;
    public UnityEngine.Transform originalArmatureSource;
    public UnityEngine.Avatar animatorAvatar;
    public System.String selectedPipeline;
    public System.String pipelineId;
    /* Animation mode fields */
    public UnityEngine.AvatarMask mask;
    public float weight = 1f;
    public System.Boolean writeDefaultValues;
    public UnityEngine.AnimationClip animationClip;
    public System.Collections.Generic.List<System.String> conditionParams = new();
    public System.Collections.Generic.List<float> conditionThresholds = new();
    public System.Collections.Generic.List<ConditionEntry> subStateConditions = new();
    public UnityEngine.GameObject subStateTransitionTarget;
    /* Reference controller for importing FX layers into GestureGenerator hierarchy */
    public UnityEngine.RuntimeAnimatorController gestureReferenceController;
    /* ToggleGenerator mode — source arrays */
    public System.Collections.Generic.List<UnityEngine.GameObject> blendshapeSources = new();
    public System.Collections.Generic.List<UnityEngine.GameObject> nanimationSources = new();
    public System.Collections.Generic.List<UnityEngine.GameObject> objectToggleSources = new();
    public System.Collections.Generic.List<UnityEngine.AnimationClip> userAnimationClips = new();
    /* Per-child fields for toggles created by CreateToggleChildren */
    public UnityEngine.AnimationClip offClip;
    public System.Boolean isToggleChild;
    public System.String toggleParameterName;
    /* ToggleGroup (legacy) */
    public ToggleGroup toggleGroup;
    /* BlendShape toggle integration for Animation mode */
    public BlendshapeToggleDefinition blendshapeToggle;
    public System.Boolean isBlendshapeToggle;
    public CbBlockType cbBlockType = CbBlockType.Custom; // replaces System.String cbType
    /* Per-toggle child configuration fields */
    
    
    public TToggleSourceType toggleSourceType = TToggleSourceType.Blendshape;
    public TToggleSourceMode toggleSourceMode = TToggleSourceMode.Single;
    public System.String toggleDisplayName;
    public System.Collections.Generic.List<UnityEngine.GameObject> toggleSourceObjects = new();
    public System.String toggleBlendshapeName;
    public System.Collections.Generic.List<System.String> onConditions = new();
    public System.Collections.Generic.List<System.String> offConditions = new();
    /* Imported parameters from GestureLayerImporter */
    public System.Collections.Generic.List<ACParam> acParams = new();
    /* Imported layers from GestureLayerImporter */
    public System.Collections.Generic.List<ACLayer> acLayers = new();
    /* Merge sources: additional controllers to merge into generated output */
    public System.Collections.Generic.List<UnityEngine.RuntimeAnimatorController> mergeSources = new();
    /* Generated controller output (set after merge during bake) */
    public UnityEngine.RuntimeAnimatorController generatedController;
    /* Children to skip during baking — names of first-level transforms */
    public System.Collections.Generic.List<System.String> ignoredChildren = new();
    /* ---- helpers ------------------------------------------------------ */
    public System.Boolean IsIgnored(System.String childName) => ignoredChildren.Contains(childName);
    public System.Boolean IsIgnored(UnityEngine.Transform t) => t != null && ignoredChildren.Contains(t.name);
    public void SyncIgnored()
    {
      foreach (UnityEngine.Transform c in transform)
      {
        if (!ignoredChildren.Contains(c.name)) continue;
        var hb = c.GetComponent<C_AviGenerator>();
        if (hb == null) ignoredChildren.Remove(c.name);
      }
    }
    static System.String HbPf => HBChildren.Prefix;
    UnityEngine.Transform FindChild(System.String n)
    { foreach (UnityEngine.Transform c in transform) if (c.name == n) return c; return null; }
    public UnityEngine.GameObject CrCh(System.String n, E_AviGeneratorMode m)
    {
      var NZKC_go_CrCh_Param = new UnityEngine.GameObject(n); NZKC_go_CrCh_Param.transform.SetParent(transform, false);
      NZKC_go_CrCh_Param.AddComponent<C_AviGenerator>();
      var hb = NZKC_go_CrCh_Param.GetComponent<C_AviGenerator>(); hb.mode = m;
      hb.avatarRootName = avatarRootName; hb.NZKC_GO_AviRoot = NZKC_GO_AviRoot;
      IF_UE.RegCr(NZKC_go_CrCh_Param, "Create " + n);
      return NZKC_go_CrCh_Param;
    }
    public UnityEngine.GameObject GllC(System.String n, E_AviGeneratorMode m) => FindChild(n)?.gameObject ?? CrCh(n, m);
    public int MgCount() => System.Linq.Enumerable.Count(GetComponentsInChildren<C_AviGenerator>(), h => h.mode == E_AviGeneratorMode.MeshGenerator && h != this);
    public void SetEntry(UnityEngine.GameObject go, System.String name) { go.GetComponent<C_AviGenerator>().entryName = name; }
    public void EnsureCBMarkers(UnityEngine.Transform t)
    {
      var markers = new[] {
    ("Entry",CbBlockType.Entry,true),("AnyState",CbBlockType.Any,true),("Exit",CbBlockType.Exit,false),("Up",CbBlockType.Up,false) };
      foreach (var (name, blockType, needsTransition) in markers)
      {
        var existing = t.Find(name);
        if (existing != null) continue;
        var go = new UnityEngine.GameObject(name);
        go.transform.SetParent(t, false);
        var hb = go.AddComponent<C_AviGenerator>();
        hb.mode = E_AviGeneratorMode.ControllerBuilder;
        hb.cbBlockType = blockType;
        hb.avatarRootName = avatarRootName;
        hb.NZKC_GO_AviRoot = NZKC_GO_AviRoot;
        if (needsTransition) go.AddComponent<TransitionSet>();
        IF_UE.RegCr(go, "Create CB marker " + name);
      }
    }
    /* ---- core --------------------------------------------------------- */
    public void ReloadChildren()
    {
      if (mode == E_AviGeneratorMode.MeshBuilder)
      { GllC(HbPf + HBChildren.MeshGeneratorRoot, E_AviGeneratorMode.MeshGeneratorRoot); return; }
      if (mode == E_AviGeneratorMode.MeshGeneratorRoot)
      {
        if (MgCount() == 0) for (int e = 0; e < 3; e++) SetEntry(CrCh("M" + e, E_AviGeneratorMode.MeshGenerator), "M" + e);
        GllC(HbPf + HBChildren.MeshSplitter, E_AviGeneratorMode.MeshSplitter); return;
      }
      if (mode == E_AviGeneratorMode.ToggleGenerator)
      {
        GllC(HbPf + HBChildren.Sources, E_AviGeneratorMode.Sources);
        GllC(HbPf + HBChildren.Toggles, E_AviGeneratorMode.Animation);
        GllC(HbPf + HBChildren.Generated, E_AviGeneratorMode.Generated); return;
      }
      if (mode == E_AviGeneratorMode.SpsGenerator)
      {
        GllC(HbPf + "Config", E_AviGeneratorMode.This);
        GllC(HbPf + "Materials", E_AviGeneratorMode.This);
        GllC(HbPf + "Layers", E_AviGeneratorMode.This);
        GllC(HbPf + "Generated", E_AviGeneratorMode.Generated); return;
      }
      if (mode == E_AviGeneratorMode.ArmatureLinks)
      { GllC(HbPf + "Sources", E_AviGeneratorMode.Sources); GllC(HbPf + "Generated", E_AviGeneratorMode.Generated); return; }
      if (mode == E_AviGeneratorMode.BakedControllers)
      { GllC(HbPf + "Sources", E_AviGeneratorMode.Sources); GllC(HbPf + "Generated", E_AviGeneratorMode.Generated); return; }
      /* ControllerBuilder/AnimatorBuilder: ensure markers on self and layers */
      if (mode == E_AviGeneratorMode.ControllerBuilder || mode == E_AviGeneratorMode.AnimatorBuilder)
      {
        EnsureCBMarkers(transform);
        /* Also ensure markers on all layer children (objects with cbBlockType == Layer) */
        foreach (UnityEngine.Transform c in transform)
        {
          var chb = c.GetComponent<C_AviGenerator>();
          if (chb != null && chb.cbBlockType == CbBlockType.Layer)
            EnsureCBMarkers(c);
        }
        return;
      }
      foreach (var s in HBChildren.ChildObjects) GllC(HbPf + s, ModeFromName(s));
      /* Auto-populate generator children + ensure Generated/Sources children */
      foreach (UnityEngine.Transform c in transform)
      {
        var chb = c.GetComponent<C_AviGenerator>();
        if (chb == null) continue;
        if (chb.mode == E_AviGeneratorMode.ExpressionsGenerator) chb.ReloadExpressionChildren();
        /* Each controller generator gets a Generated child (for baked output) and Sources child */
        if (chb.mode == E_AviGeneratorMode.GestureGenerator || chb.mode == E_AviGeneratorMode.FxGenerator
          || chb.mode == E_AviGeneratorMode.ExpressionsGenerator || chb.mode == E_AviGeneratorMode.AnimatorBuilder)
        {
          GllC(HbPf + HBChildren.Generated, E_AviGeneratorMode.Generated);
          GllC(HbPf + HBChildren.Sources, E_AviGeneratorMode.Sources);
        }
      }
    }
    /* Gesture methods moved to AXController */
    /* Gesture child methods moved to AXController */
    public void ReloadExpressionChildren()
    {
      if (mode != E_AviGeneratorMode.ExpressionsGenerator) return;
      for (int i = 0; i < 14; i++)
        GllC(HbPf + HBChildren.VisemePrefix + i, E_AviGeneratorMode.Animation);
    }
    public void AddMeshEntry()
    { var n = "M" + MgCount(); SetEntry(CrCh(n, E_AviGeneratorMode.MeshGenerator), n); }
    public static E_AviGeneratorMode ModeFromName(System.String n) => n == HBChildren.MeshBuilder ? E_AviGeneratorMode.MeshBuilder : n == HBChildren.MeshGeneratorRoot ? E_AviGeneratorMode.MeshGeneratorRoot : n == HBChildren.ArmatureBuilder ? E_AviGeneratorMode.ArmatureBuilder : n == HBChildren.AviRootBuilder ? E_AviGeneratorMode.AviRootBuilder : n == HBChildren.GestureGenerator ? E_AviGeneratorMode.GestureGenerator : n == HBChildren.ExpressionsGenerator ? E_AviGeneratorMode.ExpressionsGenerator : n == HBChildren.FxGenerator ? E_AviGeneratorMode.FxGenerator : n == HBChildren.MenuGenerator ? E_AviGeneratorMode.MenuGenerator : n == HBChildren.AnimationsGenerator ? E_AviGeneratorMode.AnimationsGenerator : n == HBChildren.ToggleGenerator ? E_AviGeneratorMode.ToggleGenerator : n == HBChildren.MeshSplitter ? E_AviGeneratorMode.MeshSplitter : n == HBChildren.SpsBuilder ? E_AviGeneratorMode.SpsGenerator : n == HBChildren.ArmatureLinks ? E_AviGeneratorMode.ArmatureLinks : n == HBChildren.BakedControllers ? E_AviGeneratorMode.BakedControllers : n.StartsWith(HBChildren.StateMachinePrefix) ? E_AviGeneratorMode.StateMachine : n.StartsWith(HBChildren.SubStatePrefix) ? E_AviGeneratorMode.State : n.StartsWith(HBChildren.TransitionPrefix) ? E_AviGeneratorMode.Conditions : n.StartsWith(HBChildren.AnimPrefix) ? E_AviGeneratorMode.Animation : n.StartsWith(HBChildren.VisemePrefix) ? E_AviGeneratorMode.Animation : n == HBChildren.Sources ? E_AviGeneratorMode.Sources : n == HBChildren.Toggles ? E_AviGeneratorMode.Animation : n == HBChildren.Generated ? E_AviGeneratorMode.Generated : n == "Entry" ? E_AviGeneratorMode.This : n == "Exit" ? E_AviGeneratorMode.This : n == "Up" ? E_AviGeneratorMode.Up : n == "AnyState" ? E_AviGeneratorMode.This : E_AviGeneratorMode.This;
    /* MapHumanoid and related skeleton/armature methods moved to AVController */
    public void Ac(System.Collections.Generic.List<C_AviGenerator> l)
    {
      foreach (UnityEngine.Transform c in transform)
      {
        var h = c.GetComponent<C_AviGenerator>();
        if (h == null) { continue; }
        if (h.mode == E_AviGeneratorMode.MeshGenerator) { l.Add(h); }
        else if (h.mode is E_AviGeneratorMode.MeshGeneratorRoot or E_AviGeneratorMode.MeshBuilder) { h.Ac(l); }
      }
    }
    public void SyncSharedMats()
    {
      if (mode != E_AviGeneratorMode.MeshGeneratorRoot) return;
      foreach (UnityEngine.Transform c in transform)
      {
        var h = c.GetComponent<C_AviGenerator>();
        if (h != null && h.mode == E_AviGeneratorMode.MeshGenerator && h.sharedMaterials != sharedMaterials)
          h.sharedMaterials = sharedMaterials != null ? System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(sharedMaterials, m => m != null)) : new System.Collections.Generic.List<UnityEngine.Material>();
      }
    }
    public System.String EntryDisplayName => System.String.IsNullOrEmpty(entryName) ? gameObject.name : entryName;
    public void DedupeSources()
    {
      if (meshSources != null) meshSources = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(meshSources, s => s != null)));
      if (meshGenSources != null) meshGenSources = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(meshGenSources, s => s != null)));
      if (sharedMaterials != null) sharedMaterials = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(sharedMaterials, m => m != null)));
      if (blendshapeSources != null) blendshapeSources = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(blendshapeSources, s => s != null)));
      if (nanimationSources != null) nanimationSources = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(nanimationSources, s => s != null)));
      if (objectToggleSources != null) objectToggleSources = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(objectToggleSources, s => s != null)));
      if (userAnimationClips != null) userAnimationClips = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(userAnimationClips, c => c != null)));
      if (ignoredChildren != null) ignoredChildren = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(ignoredChildren, n => !System.String.IsNullOrEmpty(n))));
    }
    public void CreateToggleChildren()
    {
      if (mode == E_AviGeneratorMode.ToggleGenerator)
      {
        GllC(HbPf + HBChildren.Sources, E_AviGeneratorMode.This);
        GllC(HbPf + HBChildren.Toggles, E_AviGeneratorMode.This);
        GllC(HbPf + HBChildren.Generated, E_AviGeneratorMode.This);
      }
    }
    public UnityEngine.GameObject CreateToggle(TToggleSourceType type, UnityEngine.GameObject src)
    {
      var suffix = type == TToggleSourceType.Blendshape ? "BS" : type == TToggleSourceType.NaNimation ? "NaN" : "OT";
      var tc = GllC(HbPf + HBChildren.Toggles, E_AviGeneratorMode.This).transform;
      var n = AVController.SanitizeName(src.name) + "_" + suffix;
      var exist = tc.Find(n); if (exist != null) return exist.gameObject;
      var go = new UnityEngine.GameObject(n); go.transform.SetParent(tc, false);
      var hb = go.AddComponent<C_AviGenerator>();
      hb.mode = E_AviGeneratorMode.Animation; hb.isToggleChild = true;
      hb.toggleSourceType = type; hb.toggleSourceObjects = new System.Collections.Generic.List<UnityEngine.GameObject> { src };
      hb.toggleParameterName = "(b-gt)" + AVController.SanitizeParamName(src.name);
      IF_UE.RegCr(go, "Create Toggle " + n); return go;
    }
    public UnityEngine.Transform FindHB(System.String childName)
    {
      if (IsIgnored(HBChildren.Prefix + childName)) return null;
      foreach (UnityEngine.Transform c in transform)
      {
        var h = c.GetComponent<C_AviGenerator>();
        if (h != null && h.mode == ModeFromName(childName)) return c;
      }
      return null;
    }
    public UnityEngine.Transform FindHBByMode(E_AviGeneratorMode mode)
    {
      foreach (UnityEngine.Transform c in transform)
      {
        var h = c.GetComponent<C_AviGenerator>();
        if (h != null && h.mode == mode) return c;
      }
      return null;
    }
    public System.String ResolvePipelineId(UnityEngine.Transform child)
    {
      if (child == null) return null;
      var hb = child.GetComponent<C_AviGenerator>();
      return AVController.ReadPipelineId(child, hb, AVController.FindPMType());
    }
    /* ---- YAML sync -------------------------------------------------- */
  
}
}
}
