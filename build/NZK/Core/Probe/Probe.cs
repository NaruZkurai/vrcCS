#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class Probe
  {
    /** <summary>Value of a command-line flag, or null.</summary> */
    public static System.String Arg(System.String flag)
    { if (NZK.B.N.ll.e(flag)) return null;
      System.String[] args = System.Environment.GetCommandLineArgs();
      if (NZK.B.mpty.t(args)) return null;
      for (System.Int32 i = 0; i < args.Length - 1; i++)
      { if (args[i] == flag) return args[i + 1]; }
      return null; }
    /** <summary>The first loaded GameObject whose ROOT name matches, or null.</summary> */
    public static UnityEngine.GameObject FindAvatar(System.String name)
    { UnityEngine.Transform[] all =
        UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Transform>();
      if (NZK.B.mpty.t(all)) return null;
      for (System.Int32 i = 0; i < all.Length; i++)
      { UnityEngine.Transform t = all[i];
        if (t == null) continue;
        if (t.gameObject == null) continue;
        if (t.gameObject.scene == null) continue;
        if (!t.gameObject.scene.isLoaded) continue;
        if (t.parent != null) continue;
        if (NZK.S.EqOIC(t.name, name)) return t.gameObject; }
      NZK.E.C.w(110, name);
      return null; }
    /** <summary>Dump, per renderer, everything the nanimation repair depends on.</summary>
     *  <para>Called as: -executeMethod NZK.Core.Probe.Dump --avatar nemtest</para> */
    public static void Dump()
    { System.String name = Arg("--avatar");
      if (NZK.B.N.ll.e(name)) name = "nemtest";
      UnityEngine.GameObject avatar = FindAvatar(name);
      if (avatar == null)
      { NZK.E.C.e(111, name); return; }
      UnityEngine.SkinnedMeshRenderer[] smrs =
        avatar.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
      System.Int32 renderers = 0;
      System.Int32 named = 0;
      System.Int32 withSource = 0;
      System.Int32 withNanimGroups = 0;
      if (smrs != null)
      { for (System.Int32 i = 0; i < smrs.Length; i++)
        { UnityEngine.SkinnedMeshRenderer smr = smrs[i];
          if (smr == null) continue;
          renderers++;
          UnityEngine.Mesh mesh = smr.sharedMesh;
          System.String meshName = mesh != null ? mesh.name : "<null>";
          System.Int32 verts = mesh != null ? mesh.vertexCount : -1;
          System.String isReadable = mesh != null ? mesh.isReadable.ToString() : "n/a";
          System.Int32 bones = smr.bones != null ? smr.bones.Length : -1;
          System.Int32 deadSlots = NZK.Core.NanRelink.DeadBoneSlots(smr);
          System.Int32 bindposes = -1;
          if (mesh != null && mesh.bindposes != null) bindposes = mesh.bindposes.Length;
          System.String boneWeightsReadable = "throw";
          try
          { UnityEngine.BoneWeight[] bw = mesh != null ? mesh.boneWeights : null;
            if (NZK.B.mpty.t(bw)) boneWeightsReadable = "empty";
            else boneWeightsReadable = "yes"; }
          catch (System.Exception ex)
          { boneWeightsReadable = "throw"; }
          UnityEngine.Mesh src = NZK.Core.NanRelink.FindSourceMesh(smr);
          System.String srcName = src != null ? src.name : "<null>";
          System.Int32 srcVerts = src != null ? src.vertexCount : -1;
          System.String srcReadable = src != null ? src.isReadable.ToString() : "n/a";
          System.Int32 srcBindposes = -1;
          if (src != null)
          { try
            { srcBindposes = src.bindposes != null ? src.bindposes.Length : -1; }
            catch (System.Exception ex)
            { srcBindposes = -1; } }
          System.Int32 srcWeights = -1;
          if (src != null)
          { try
            { srcWeights = src.boneWeights != null ? src.boneWeights.Length : -1; }
            catch (System.Exception ex)
            { srcWeights = -1; } }
          System.String srcIsSameAsSceneMesh = (src != null && mesh != null && src == mesh) ? "yes" : "no";
          System.Int32 nanimBone = NZK.Core.NanRelink.FirstNanimBone(smr.bones);
          System.Int32 nanimGroups = -1;
          if (src != null)
          { try
            { nanimGroups = NZK.Core.NanRelink.SourceNanGroupNames(src, smr.bones).Count; }
            catch (System.Exception ex)
            { nanimGroups = -1; } }
          System.Int32 nanimOnVerts = NZK.Core.NanRelink.CountNanimBound(smr);
          System.Int32 unitSumViolations = NZK.Core.NanRelink.CountUnitSumViolations(smr);
          System.Int32 maxInfluences = NZK.Core.NanRelink.MaxInfluencesIn(mesh);
          if (mesh != null) named++;
          if (src != null) withSource++;
          if (nanimGroups > 0) withNanimGroups++;
          NZK.E.C.d(112,
            "renderer=" + smr.name +
            " mesh=" + meshName +
            " verts=" + verts +
            " isReadable=" + isReadable +
            " bones=" + bones +
            " deadSlots=" + deadSlots +
            " bindposes=" + bindposes +
            " boneWeightsReadable=" + boneWeightsReadable +
            " src=" + srcName +
            " srcVerts=" + srcVerts +
            " srcReadable=" + srcReadable +
            " srcBindposes=" + srcBindposes +
            " srcWeights=" + srcWeights +
            " srcIsSameAsSceneMesh=" + srcIsSameAsSceneMesh +
            " nanimBone=" + nanimBone +
            " nanimGroups=" + nanimGroups +
            " nanimOnVerts=" + nanimOnVerts +
            " unitSumViolations=" + unitSumViolations +
            " maxInfluences=" + maxInfluences); } }
      NZK.E.C.d(113,
        "renderers=" + renderers +
        " named=" + named +
        " withSource=" + withSource +
        " withNanimGroups=" + withNanimGroups); }
    /** <summary>Run the importer fix (4 influences, strip unreferenced bones, lowest
     *  weight threshold) over the avatar's models and report what it measured.
     *  <para>-executeMethod NZK.Core.Probe.FixImports --avatar nemtest</para> */
    public static void FixImports()
    { System.String name = Arg("--avatar");
      if (NZK.B.N.ll.e(name)) name = "nemtest";
      UnityEngine.GameObject avatar = FindAvatar(name);
      if (avatar == null)
      { NZK.E.C.e(111, name); return; }
      NZK.Core.MeshImport.ImportReport r =
        NZK.Core.MeshImport.ForceFour(new UnityEngine.GameObject[] { avatar });
      NZK.E.C.d(114,
        "inspected=" + r.Inspected +
        " changed=" + r.Changed +
        " already=" + r.Already +
        " unsupported=" + r.Unsupported +
        " skipped=" + r.Skipped);
      NZK.Core.MeshImport.ReportImpacts(new UnityEngine.GameObject[] { avatar }); }
    /** <summary>Run the nanimation weight repair over the avatar.
     *  <para>-executeMethod NZK.Core.Probe.Fix --avatar nemtest</para> */
    public static void Fix()
    { System.String name = Arg("--avatar");
      if (NZK.B.N.ll.e(name)) name = "nemtest";
      UnityEngine.GameObject avatar = FindAvatar(name);
      if (avatar == null)
      { NZK.E.C.e(111, name); return; }
      NZK.E.C.d(115, name);
      try
      { System.Int32 n = NZK.Core.Nan.PostProcess(avatar);
        NZK.E.C.d(116, n); }
      catch (System.Exception e)
      { NZK.E.C.e(117, e.Message); } }
    /** <summary>Report whether the avatar meets the four-influence unit-sum rule.
     *  <para>-executeMethod NZK.Core.Probe.Verify --avatar nemtest</para> */
    public static void Verify()
    { System.String name = Arg("--avatar");
      if (NZK.B.N.ll.e(name)) name = "nemtest";
      UnityEngine.GameObject avatar = FindAvatar(name);
      if (avatar == null)
      { NZK.E.C.e(111, name); return; }
      UnityEngine.SkinnedMeshRenderer[] smrs =
        avatar.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
      System.Int32 renderers = 0;
      System.Int32 withNanimBound = 0;
      System.Int32 totalUnitSumViolations = 0;
      if (smrs != null)
      { for (System.Int32 i = 0; i < smrs.Length; i++)
        { UnityEngine.SkinnedMeshRenderer smr = smrs[i];
          if (smr == null) continue;
          renderers++;
          if (NZK.Core.NanRelink.CountNanimBound(smr) > 0) withNanimBound++;
          totalUnitSumViolations += NZK.Core.NanRelink.CountUnitSumViolations(smr); } }
      NZK.E.C.d(118,
        "renderers=" + renderers +
        " withNanimBound=" + withNanimBound +
        " totalUnitSumViolations=" + totalUnitSumViolations);
      if (totalUnitSumViolations == 0 && withNanimBound > 0)
      { NZK.E.C.d(119, "PASS - every vertex has <=4 influences summing to 1"); }
      else
      { NZK.E.C.e(120,
          "FAIL - totalUnitSumViolations=" + totalUnitSumViolations +
          " withNanimBound=" + withNanimBound); } }
    /** <summary>Full-string path of a transform, walking parents up to the root.
     *  <para>Capped at 12 levels so a malformed hierarchy cannot loop forever.</para> */
    public static System.String PathOf(UnityEngine.Transform t)
    { if (t == null) return "<null>";
      System.String path = t.name;
      UnityEngine.Transform p = t.parent;
      System.Int32 depth = 1;
      while (p != null && depth < 12)
      { path = p.name + "/" + path;
        p = p.parent;
        depth++; }
      return path; }
    /** <summary>Why is the avatar invisible?  Sweeps bone scales, renderer bounds,
     *  the avatar descriptor and the animators, then prints a verdict.
     *  <para>-executeMethod NZK.Core.Probe.Why --avatar nemtest</para> */
    public static void Why()
    { System.String name = Arg("--avatar");
      if (NZK.B.N.ll.e(name)) name = "nemtest";
      UnityEngine.GameObject avatar = FindAvatar(name);
      if (avatar == null)
      { NZK.E.C.e(121, name); return; }
      /* ---- A. bone scale sweep ---- */
      UnityEngine.Transform[] transforms =
        avatar.GetComponentsInChildren<UnityEngine.Transform>(true);
      System.Int32 transformCount = 0;
      System.Int32 zeroScale = 0;
      System.Int32 nanScale = 0;
      System.Boolean zeroScaleOnActiveBone = false;
      System.Boolean nanScaleOnActiveBone = false;
      if (transforms != null)
      { for (System.Int32 i = 0; i < transforms.Length; i++)
        { UnityEngine.Transform t = transforms[i];
          if (t == null) continue;
          transformCount++;
          UnityEngine.Vector3 s = t.localScale;
          System.Boolean isZero = s.x == 0f || s.y == 0f || s.z == 0f;
          System.Boolean isNan = System.Single.IsNaN(s.x)
                              || System.Single.IsNaN(s.y)
                              || System.Single.IsNaN(s.z);
          if (!isZero && !isNan) continue;
          if (isZero) zeroScale++;
          if (isNan) nanScale++;
          System.Boolean active = t.gameObject != null && t.gameObject.activeInHierarchy;
          if (isZero && active) zeroScaleOnActiveBone = true;
          if (isNan && active) nanScaleOnActiveBone = true;
          NZK.E.C.w(122,
            "path=" + PathOf(t) +
            " localScale=" + s.ToString("R") +
            " active=" + active.ToString().ToLowerInvariant() +
            (isZero ? " ZERO-SCALE" : "") +
            (isNan ? " NAN-SCALE" : "")); } }
      NZK.E.C.d(123,
        "transforms=" + transformCount +
        " zeroScale=" + zeroScale +
        " nanScale=" + nanScale);
      /* ---- B. renderer bounds sweep ---- */
      UnityEngine.SkinnedMeshRenderer[] smrs =
        avatar.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
      System.Int32 renderers = 0;
      System.Int32 disabled = 0;
      System.Int32 offscreenUpdateOff = 0;
      System.Int32 zeroBoundsSize = 0;
      System.Boolean zeroBoundsSkinnedMesh = false;
      if (smrs != null)
      { for (System.Int32 i = 0; i < smrs.Length; i++)
        { UnityEngine.SkinnedMeshRenderer smr = smrs[i];
          if (smr == null) continue;
          renderers++;
          UnityEngine.Bounds b = smr.localBounds;
          UnityEngine.Vector3 bs = b.size;
          System.Boolean badSize = bs.x == 0f || bs.y == 0f || bs.z == 0f
                                || System.Single.IsNaN(bs.x)
                                || System.Single.IsNaN(bs.y)
                                || System.Single.IsNaN(bs.z);
          System.Boolean active = smr.gameObject != null && smr.gameObject.activeInHierarchy;
          if (!smr.enabled) disabled++;
          if (!smr.updateWhenOffscreen) offscreenUpdateOff++;
          if (badSize) zeroBoundsSize++;
          if (badSize && smr.enabled && active) zeroBoundsSkinnedMesh = true;
          System.String rootBone = smr.rootBone != null ? smr.rootBone.name : "<null>";
          System.String meshName = smr.sharedMesh != null ? smr.sharedMesh.name : "<null>";
          NZK.E.C.d(124,
            "renderer=" + smr.name +
            " enabled=" + smr.enabled +
            " activeInHierarchy=" + active +
            " updateWhenOffscreen=" + smr.updateWhenOffscreen +
            " localBoundsCenter=" + b.center.ToString("R") +
            " localBoundsSize=" + bs.ToString("R") +
            " rootBone=" + rootBone +
            " meshName=" + meshName +
            " lossyScale=" + smr.transform.lossyScale.ToString("R")); } }
      NZK.E.C.d(125,
        "renderers=" + renderers +
        " disabled=" + disabled +
        " offscreenUpdateOff=" + offscreenUpdateOff +
        " zeroBoundsSize=" + zeroBoundsSize);
      /* ---- C. avatar descriptor ---- */
      System.Boolean descriptorFound = false;
      try
      { VRC.SDK3.Avatars.Components.VRCAvatarDescriptor vrcad =
          avatar.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
        if (vrcad == null)
        { vrcad = avatar.GetComponentInChildren<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(true); }
        System.Int32 baseLayers = 0;
        System.Int32 specialLayers = 0;
        System.Boolean customize = false;
        System.Boolean hasMenu = false;
        System.Boolean hasParams = false;
        System.String viewPos = "<null>";
        if (vrcad != null)
        { descriptorFound = true;
          if (vrcad.baseAnimationLayers != null) baseLayers = vrcad.baseAnimationLayers.Length;
          if (vrcad.specialAnimationLayers != null) specialLayers = vrcad.specialAnimationLayers.Length;
          customize = vrcad.customizeAnimationLayers;
          hasMenu = vrcad.expressionsMenu != null;
          hasParams = vrcad.expressionParameters != null;
          viewPos = vrcad.ViewPosition.ToString("R"); }
        NZK.E.C.d(126,
          "found=" + descriptorFound +
          " viewPosition=" + viewPos +
          " baseAnimationLayers=" + baseLayers +
          " specialAnimationLayers=" + specialLayers +
          " customizeAnimationLayers=" + customize +
          " expressionsMenu=" + hasMenu +
          " expressionParameters=" + hasParams); }
      catch (System.Exception e)
      { NZK.E.C.e(127, e.Message); }
      /* ---- D. animator check ---- */
      UnityEngine.Animator[] animators =
        avatar.GetComponentsInChildren<UnityEngine.Animator>(true);
      System.Boolean noAnimatorController = true;
      System.Boolean noHumanAvatar = true;
      System.Int32 animatorCount = 0;
      if (animators != null)
      { for (System.Int32 i = 0; i < animators.Length; i++)
        { UnityEngine.Animator a = animators[i];
          if (a == null) continue;
          animatorCount++;
          System.String avatarName = a.avatar != null ? a.avatar.name : "<null>";
          System.String ctrlName = a.runtimeAnimatorController != null
            ? a.runtimeAnimatorController.name : "<null>";
          if (a.runtimeAnimatorController != null) noAnimatorController = false;
          if (a.avatar != null && a.isHuman) noHumanAvatar = false;
          NZK.E.C.d(128,
            "path=" + PathOf(a.transform) +
            " enabled=" + a.enabled +
            " avatar=" + avatarName +
            " isHuman=" + a.isHuman +
            " runtimeAnimatorController=" + ctrlName +
            " applyRootMotion=" + a.applyRootMotion); } }
      /* ---- E. verdict ---- */
      System.Boolean noAvatarDescriptor = !descriptorFound;
      System.String verdict = "";
      if (zeroScaleOnActiveBone) verdict += " zeroScaleOnActiveBone";
      if (nanScaleOnActiveBone) verdict += " nanScaleOnActiveBone";
      if (zeroBoundsSkinnedMesh) verdict += " zeroBoundsSkinnedMesh";
      if (noAvatarDescriptor) verdict += " noAvatarDescriptor";
      if (noAnimatorController) verdict += " noAnimatorController";
      if (noHumanAvatar) verdict += " noHumanAvatar";
      if (NZK.B.N.ll.e(verdict))
      { verdict = "NO-OBVIOUS-CAUSE - the hierarchy looks structurally sound"; }
      NZK.E.C.d(129, verdict); }
    /** <summary>Fix imports, run the repair, then verify - one call.
     *  <para>-executeMethod NZK.Core.Probe.All --avatar nemtest</para> */
    public static void All()
    { FixImports();
      Dump();
      Fix();
      Verify();
      Why(); }
  }
}
}
#endif
