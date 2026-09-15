#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class SpsSocketBakeService
  { public const System.String ParamAutoMode = "autoMode"; public const System.String ParamStealth = "stealth"; public const System.String ParamDualMode = "multi";
    public const System.String LayerExclusive = "SPS - Socket Exclusivity"; public const System.String LayerAutoCompare = "SPS - Auto Socket Comparison";
    public const float ContactSenderRootRadius = 0.001f; public const float ContactSenderFrontOffset = 0.01f;
    static UnityEngine.AnimationClip _autoClip;
    public static System.Collections.Generic.List<SpsSocketBakeResult> BakeAllSockets(C_SpsSocketGenerator generator,UnityEditor.Animations.AnimatorController fxController,VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar,System.String outputBase,System.String avatarName)
    { var results =  new System.Collections.Generic.List<SpsSocketBakeResult>();
      if (generator == null || fxController == null || avatar == null) { UnityEngine.Debug.LogError("[SpsSocketBakeService] Missing required references."); return results; }
      ResetCachedState(); generator.RefreshSockets();
      var sockets = generator.sockets;
      if (sockets.Count == 0) { UnityEngine.Debug.Log("[SpsSocketBakeService] No sockets."); return results; }
      Systems.Folder.Ensure(outputBase); Systems.Folder.Ensure(outputBase + "/Animations");
      int menuCount = System.Linq.Enumerable.Count(sockets, s => !System.String.IsNullOrEmpty(s.menuPath));
      System.Boolean enableAuto = System.Linq.Enumerable.Count(sockets, s => s.enableAuto) >= 2;
      System.Boolean enableStealth = menuCount >= 1; System.Boolean enableMulti = menuCount >= 2;
      System.String autoP = enableAuto ? ParamAutoMode : null; System.String stealthP = enableStealth ? ParamStealth : null; System.String dualP = enableMulti ? ParamDualMode : null;
      if (autoP != null) EnsureBoolParam(fxController,autoP);
      if (stealthP != null) EnsureBoolParam(fxController,stealthP);
      if (dualP != null) EnsureBoolParam(fxController,dualP);
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "IsLocal")) fxController.AddParameter("IsLocal",UnityEngine.AnimatorControllerParameterType.Bool);
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "(f)Weight")) fxController.AddParameter("(f)Weight",UnityEngine.AnimatorControllerParameterType.Float);
      if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "_One")) fxController.AddParameter("_One",UnityEngine.AnimatorControllerParameterType.Float);
      var autoSockets =  new System.Collections.Generic.List<(System.String oscId,System.String toggleP,System.String distP)>();
      var exclusive =  new System.Collections.Generic.List<(System.String oscId,System.String toggleP)>();
      var usedOscIds = new System.Collections.Generic.HashSet<System.String>(System.StringComparer.OrdinalIgnoreCase);
      foreach (var socket in sockets)
      { try { System.String safeName = Vars.Names.Sanitize(socket.socketName); System.String btDir = System.IO.Path.GetDirectoryName(outputBase.TrimEnd('/')) + "/blendtrees/sockets/" + safeName; Systems.Folder.Ensure(btDir); var r = BakeSocket(socket,fxController,outputBase,btDir,usedOscIds,stealthP,autoP,ref autoSockets,ref exclusive,generator); if (r != null) results.Add(r); }
        catch (System.Exception e) { UnityEngine.Debug.LogError($"[SpsSocketBakeService] Failed '{socket.socketName}': {e.Message}"); } }
      if (exclusive.Count >= 2) CreateExclusiveLayer(fxController,exclusive,stealthP,dualP);
      if (autoP != null && autoSockets.Count > 0) CreateAutoModeLayer(fxController,autoP,autoSockets);
      System.String menuDir = System.IO.Path.GetDirectoryName(outputBase.TrimEnd('/')) + "/Menus";
      System.String spsMenuPath = "SPS"; System.String optionsPath = spsMenuPath + "/<b>Options";
      System.String menuAssetPath = menuDir + "/" + avatarName + "_SPS_Menu.asset";
      Systems.Folder.Ensure(menuDir);
      var rootMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(menuAssetPath);
      if (rootMenu == null) { rootMenu = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(); rootMenu.name = avatarName + " SPS Menu"; rootMenu.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>(); UnityEditor.AssetDatabase.CreateAsset(rootMenu,menuAssetPath); }
      rootMenu.controls.Clear();
      System.Boolean hasOptions = autoP != null || stealthP != null || dualP != null;
      VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu optionsMenu = null;
      if (hasOptions)
      { System.String optPath = menuDir + "/" + avatarName + "_SPS_Options.asset"; optionsMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(optPath);
        if (optionsMenu == null) { optionsMenu = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(); optionsMenu.name = avatarName + " SPS Options"; optionsMenu.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>(); UnityEditor.AssetDatabase.CreateAsset(optionsMenu,optPath); }
        optionsMenu.controls.Clear();
        if (autoP != null) optionsMenu.controls.Add(MakeToggle(optionsPath + "/<b>Auto Mode<b>\\n<size=20>Activates hole nearest to a VRCFury plug",autoP));
        if (stealthP != null) optionsMenu.controls.Add(MakeToggle(optionsPath + "/<b>Stealth Mode<\\/b>\\n<size=20>Only local haptics,\\nInvisible to others",stealthP));
        if (dualP != null)
        { System.String dualPath = menuDir + "/" + avatarName + "_SPS_Dual.asset"; var dualMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(dualPath);
          if (dualMenu == null) { dualMenu = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(); dualMenu.name = avatarName + " SPS Dual"; dualMenu.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>(); UnityEditor.AssetDatabase.CreateAsset(dualMenu,dualPath); }
          dualMenu.controls.Clear(); dualMenu.controls.Add(MakeToggle(optionsPath + "/<b>Dual Mode<\\/b>\\n<size=20>Allows 2 active sockets",dualP));
          dualMenu.controls.Add(MakeButton(optionsPath + "/<b>WARNING<\\/b>\\n<size=20>Everyone else must use SPS or TPS - NO DPS!"));
          dualMenu.controls.Add(MakeButton(optionsPath + "/<b>WARNING<\\/b>\\n<size=20>Nobody else can use a hole at the same time"));
          dualMenu.controls.Add(MakeButton(optionsPath + "/<b>WARNING<\\/b>\\n<size=20>DO NOT ENABLE MORE THAN 2"));
          optionsMenu.controls.Add(MakeSub(optionsPath + "/<b>Dual Mode",dualMenu)); }
        rootMenu.controls.Add(MakeSub(spsMenuPath + "/Options",optionsMenu)); }
      foreach (var r in results)
      { if (System.String.IsNullOrEmpty(r.toggleParamName)) continue;
        System.String displayName = r.sourceConfig != null ? r.sourceConfig.socketName : r.oscId;
        if (!System.String.IsNullOrEmpty(r.sourceConfig?.menuPath)) { var parts = r.sourceConfig.menuPath.Split('/'); displayName = parts[parts.Length - 1]; }
        rootMenu.controls.Add(MakeToggle(spsMenuPath + "/" + displayName,r.toggleParamName)); }
      UnityEditor.EditorUtility.SetDirty(rootMenu); if (optionsMenu != null) UnityEditor.EditorUtility.SetDirty(optionsMenu);
      /* ── Update manifest ── */
      var manifest = SpsManifestManager.Load(avatarName);
      manifest.buildDate = System.DateTime.Now.ToString("O");
      foreach (var r in results)
      { if (r.sourceConfig == null) continue;
        var assetPaths = new System.Collections.Generic.List<System.String>();
        if (r.bakeRoot != null) { var p = UnityEditor.AssetDatabase.GetAssetPath(r.bakeRoot); if (!System.String.IsNullOrEmpty(p)) assetPaths.Add(p); }
        manifest.sockets.Add(new ManifestEntry { name = r.sourceConfig.socketName,type = "socket",
          compatMode = r.sourceConfig.compatMode,configHash = generator?.ConfigHash(r.sourceConfig) ?? "",
          markerId = r.oscId,assetPaths = assetPaths }); }
      manifest.shaders = new System.Collections.Generic.List<System.String> { SpsMarkerService.SpsSocketShader };
      SpsManifestManager.Save(manifest,avatarName);
      UnityEditor.AssetDatabase.SaveAssets(); UnityEngine.Debug.Log($"[SpsSocketBakeService] Baked {results.Count} sockets + menu at {menuAssetPath}."); return results; }
    public static SpsSocketBakeResult BakeSocket(SpsSocketConfig socket,UnityEditor.Animations.AnimatorController fxController,System.String outputBase,System.String blendTreeDir,System.Collections.Generic.HashSet<System.String> usedOscIds,System.String stealthParamName,System.String autoParamName,ref System.Collections.Generic.List<(System.String,System.String,System.String)> autoSockets,ref System.Collections.Generic.List<(System.String,System.String)> exclusive,C_SpsSocketGenerator generator = null)
    { if (socket.socketTransform == null) return null; var st = socket.socketTransform;
      System.String rawId = !System.String.IsNullOrEmpty(socket.parameterOverride) ? socket.parameterOverride : socket.socketName;
      System.String oscId = MakeUniqueId(usedOscIds,SanitizeForParameter(rawId));
      /* ── Prefab cache: skip rebuild if hash matches ── */
      if (generator != null)
      { var cachedHash = generator.GetCachedHash(socket); var curHash = generator.ConfigHash(socket);
        if (cachedHash == curHash)
        { var cachedRoot = generator.GetCachedBakeRoot(socket.socketName);
          if (cachedRoot != null)
          { var cachedToggleP = !System.String.IsNullOrEmpty(socket.menuPath) ? oscId : null;
            if (cachedToggleP != null) { EnsureBoolParam(fxController,cachedToggleP); exclusive.Add((oscId,cachedToggleP)); }
            var os = cachedRoot.Find("OneSpace"); var ws = os != null ? os.Find("WorldSpace") : cachedRoot.Find("WorldSpace"); var s = ws != null ? ws.Find("Senders") : null; var lt = ws != null ? ws.Find("Lights") : null;
            UnityEngine.Debug.Log("[SPS Cache] HIT: " + socket.socketName + " (unchanged, skipped rebuild)");
            return new SpsSocketBakeResult { bakeRoot = cachedRoot.gameObject,worldSpace = ws != null ? ws.gameObject : null,senders = s != null ? s.gameObject : null,lights = lt != null ? lt.gameObject : null,oscId = oscId,toggleParamName = cachedToggleP,menuPath = socket.menuPath,sourceConfig = socket }; } } }
      /* ── Full rebuild ── */
      int lightType = socket.V_addLight;
      if (lightType == SpsAddLight.Auto) { var cur = st; System.Boolean isHole = false; while (cur != null) { System.String n = cur.name.ToLowerInvariant(); if (n.Contains("head") || n.Contains("jaw") || n.Contains("hips")) { isHole = true; break; } cur = cur.parent; } lightType = isHole ? SpsAddLight.Hole : SpsAddLight.Ring; }
      var bakeRoot = new UnityEngine.GameObject("BakedSpsSocket"); bakeRoot.transform.SetParent(st,false); bakeRoot.transform.localPosition = socket.position; bakeRoot.transform.localRotation = UnityEngine.Quaternion.Euler(socket.rotation);
      /* ── OneSpace: normalizes parent scale so WorldSpace contacts aren't distorted ── */
      var oneSpace = new UnityEngine.GameObject("OneSpace"); oneSpace.transform.SetParent(bakeRoot.transform,false);
      oneSpace.transform.localPosition = UnityEngine.Vector3.zero; oneSpace.transform.localRotation = UnityEngine.Quaternion.identity;
      var bs = bakeRoot.transform.lossyScale;
      oneSpace.transform.localScale = new UnityEngine.Vector3(1f/bs.x,1f/bs.y,1f/bs.z);
      var worldSpace = new UnityEngine.GameObject("WorldSpace"); worldSpace.transform.SetParent(oneSpace.transform,false); worldSpace.transform.localPosition = UnityEngine.Vector3.zero; worldSpace.transform.localRotation = UnityEngine.Quaternion.identity; worldSpace.transform.localScale = UnityEngine.Vector3.one;
      var wsCon = worldSpace.AddComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>(); wsCon.IsActive = true; wsCon.Locked = true;
      var senders = new UnityEngine.GameObject("Senders"); senders.transform.SetParent(worldSpace.transform,false); CreateContactSenders(senders,lightType,socket.compatMode);
      UnityEngine.GameObject lights = null; if (lightType != SpsAddLight.None) lights = CreateDeformationLights(worldSpace,lightType);
      /* ── SPS2 shader marker (SPS2 or All compat mode) with material properties ── */
      UnityEngine.GameObject sps2Marker = null;
      if (socket.compatMode == 0 || socket.compatMode == 2)
      { var avatarRoot = st.root; var anim = avatarRoot != null ? avatarRoot.GetComponent<UnityEngine.Animator>() : null;
        var markerCfg = new SpsMarkerService.MarkerConfig
        { socketId = SpsMarkerService.NewMarkerId(),lightType = lightType,compatMode = socket.compatMode,
          socketName = socket.socketName,useRadiusOffset = false };
        sps2Marker = SpsMarkerService.CreateSocketMarker(oneSpace.transform,UnityEngine.Vector3.zero,UnityEngine.Quaternion.identity,markerCfg);
        /* Set material properties on the marker renderer */
        var mr = sps2Marker.GetComponent<UnityEngine.MeshRenderer>();
        if (mr != null)
        { var mat = mr.sharedMaterial;
          if (mat != null)
          { var mProps = SpsMarkerService.GetMaterialProperties(markerCfg,sps2Marker.transform);
            foreach (var (prop,val) in mProps)
            { if (mat.HasProperty(prop)) mat.SetFloat(prop,val); }
            /* ── Generate animation clip for material properties ── */
            var propClip = new UnityEngine.AnimationClip(); propClip.name = "SPS2_" + (socket.socketName ?? "Socket") + "_Props";
            System.String markerPath = UnityEditor.AnimationUtility.CalculateTransformPath(sps2Marker.transform,sps2Marker.transform.root);
            foreach (var (prop,val) in mProps)
            { var binding = new UnityEditor.EditorCurveBinding { path = markerPath,type = typeof(UnityEngine.MeshRenderer),propertyName = "material." + prop };
              UnityEditor.AnimationUtility.SetEditorCurve(propClip,binding,UnityEngine.AnimationCurve.Constant(0f,0f,val)); }
            propClip = SaveClip(propClip,$"{outputBase}/Animations/{propClip.name}.anim");
            /* Wire prop clip into the FX controller as a direct blend tree entry */
            var tdName = oscId + "_Props";
            var td = NewDirect(tdName); td.AddChild(propClip); td.children[0].directBlendParameter = "(f)Weight";
            SaveBT(td,$"{blendTreeDir}/{tdName}.asset");
            /* Add float params for blend trees */
            if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "(f)Weight"))
              fxController.AddParameter("(f)Weight",UnityEngine.AnimatorControllerParameterType.Float);
            if (!System.Linq.Enumerable.Any(fxController.parameters, p => p.name == "_One"))
              fxController.AddParameter("_One",UnityEngine.AnimatorControllerParameterType.Float);
            /* Create a layer for the material properties */
            var propLayer = new UnityEditor.Animations.AnimatorControllerLayer
            { name = "SPS2_Props_" + oscId,stateMachine = new UnityEditor.Animations.AnimatorStateMachine(),
              defaultWeight = 1f,blendingMode = UnityEditor.Animations.AnimatorLayerBlendingMode.Override };
            var propState = propLayer.stateMachine.AddState("Props",new UnityEngine.Vector3(200,0,0));
            propState.motion = td; propState.writeDefaultValues = true;
            var ls = System.Linq.Enumerable.ToList(fxController.layers); ls.Add(propLayer); fxController.layers = ls.ToArray();
            /* ── Register player ID on the marker renderer ── */
            SpsResolverService.RegisterRenderer(mr,fxController,outputBase,Vars.Names.Sanitize(st.root.name)); } }
        /* Extend nearest SkinnedMeshRenderer bounds so marker isn't culled */
        if (anim != null) { var nearestBone = FindNearestBone(anim,st.position);
          if (nearestBone != null) { var smr = nearestBone.GetComponentInChildren<UnityEngine.SkinnedMeshRenderer>();
            if (smr != null) SpsShaderConfigurer.ExtendBounds(smr,st.position); } } }
      System.String toggleP = null; if (!System.String.IsNullOrEmpty(socket.menuPath)) { toggleP = oscId; EnsureBoolParam(fxController,toggleP); exclusive.Add((oscId,toggleP)); }
      UnityEngine.GameObject hapticsObj = null;
      if (toggleP != null)
      { st.gameObject.SetActive(true); bakeRoot.SetActive(false); senders.SetActive(false); if (hapticsObj != null) hapticsObj.SetActive(false); if (lights != null) lights.SetActive(false); if (sps2Marker != null) sps2Marker.SetActive(false);
        var localClip = new UnityEngine.AnimationClip { name = $"{oscId} (Local)" }; foreach (var go in new[] { bakeRoot,senders,hapticsObj,lights,sps2Marker }) { if (go != null) SetActive(localClip,go,true); } localClip = SaveClip(localClip,$"{outputBase}/Animations/{localClip.name}.anim");
        var remoteClip = new UnityEngine.AnimationClip { name = $"{oscId} (Remote)" }; foreach (var go in new[] { bakeRoot,senders,lights,sps2Marker }) { if (go != null) SetActive(remoteClip,go,true); } remoteClip = SaveClip(remoteClip,$"{outputBase}/Animations/{remoteClip.name}.anim");
        var stealthClip = new UnityEngine.AnimationClip { name = $"{oscId} (Stealth)" }; foreach (var go in new[] { bakeRoot,hapticsObj }) { if (go != null) SetActive(stealthClip,go,true); } stealthClip = SaveClip(stealthClip,$"{outputBase}/Animations/{stealthClip.name}.anim");
        System.String sw = $"{oscId}/_SW"; EnsureFloatParam(fxController,sw,0f); System.String nsw = $"{oscId}/_NSW"; EnsureFloatParam(fxController,nsw,1f);
        System.String iw = $"{oscId}/_IW"; EnsureFloatParam(fxController,iw,0f); System.String niw = $"{oscId}/_NIW"; EnsureFloatParam(fxController,niw,1f);
        SaveBT(Make1D(stealthParamName ?? "(f)Weight",sw,0f,0f,1f),$"{blendTreeDir}/{oscId}_SW.asset");
        SaveBT(Make1D(sw,nsw,1f,0f,0f),$"{blendTreeDir}/{oscId}_NSW.asset");
        var lt = NewDirect($"{oscId}/LTree"); lt.AddChild(stealthClip); lt.children[0].directBlendParameter = sw; lt.AddChild(localClip); lt.children[1].directBlendParameter = nsw; SaveBT(lt,$"{blendTreeDir}/{oscId}_LTree.asset");
        var rt = NewDirect($"{oscId}/RTree"); rt.AddChild(remoteClip); rt.children[0].directBlendParameter = nsw; SaveBT(rt,$"{blendTreeDir}/{oscId}_RTree.asset");
        SaveBT(Make1D("IsLocal",iw,0f,0f,1f),$"{blendTreeDir}/{oscId}_IW.asset");
        SaveBT(Make1D(iw,niw,1f,0f,0f),$"{blendTreeDir}/{oscId}_NIW.asset");
        var ot = NewDirect($"{oscId}/OTree"); ot.AddChild(lt); ot.children[0].directBlendParameter = iw; ot.AddChild(rt); ot.children[1].directBlendParameter = niw; SaveBT(ot,$"{blendTreeDir}/{oscId}_OTree.asset");
        var td = NewDirect($"{oscId} - Toggle"); td.AddChild(ot); td.children[0].directBlendParameter = toggleP; SaveBT(td,$"{blendTreeDir}/{oscId}_Toggle.asset");
        if (socket.enableAuto && autoParamName != null)
        { var ar = new UnityEngine.GameObject("AutoDistance"); ar.transform.SetParent(worldSpace.transform,false);
          System.String dp = $"{oscId}/AutoD"; SpsHapticContactsService.AddReceiver(new ReceiverRequest { obj = ar,paramName = dp,objName = "R",radius = 0.3f,tags = new[] { SpsHapticUtils.CONTACT_PEN_MAIN },party = SpsHapticUtils.ReceiverParty.Others,useHipAvoidance = true });
          ar.SetActive(false); var ac = GetOrCreateAutoClip($"{outputBase}/Animations"); SetActive(ac,bakeRoot,true); SetActive(ac,ar,true); autoSockets.Add((oscId,toggleP,dp)); } }
      if (generator != null) { generator.SaveToPrefabCache(socket.socketName,bakeRoot); generator.SetCachedHash(socket); }
      return new SpsSocketBakeResult { bakeRoot = bakeRoot,worldSpace = worldSpace,senders = senders,lights = lights,oscId = oscId,toggleParamName = toggleP,menuPath = socket.menuPath,sourceConfig = socket }; }
    public static void CreateContactSenders(UnityEngine.GameObject parent,int lightType,int compatMode = 0)
    { var rootTags =  new System.Collections.Generic.List<System.String>();
      /* Always add SPS2 tags if All or SPS2 mode */
      if (compatMode == 0 || compatMode == 2)
      { rootTags.Add(SpsHapticUtils.TagSpsSocketRoot); }
      /* Add SPS1/DPS/TPS light-based tags if All, SPS1, TPS, or DPS mode */
      if (compatMode != 2)
      { rootTags.Add(SpsHapticUtils.TagTpsOrfRoot);
        if (lightType != SpsAddLight.None)
        { if (lightType == SpsAddLight.Ring) rootTags.Add(SpsHapticUtils.TagSpsSocketIsRing);
          else if (lightType == SpsAddLight.RingOneWay) { rootTags.Add(SpsHapticUtils.TagSpsSocketIsRing); rootTags.Add(SpsHapticUtils.TagSpsSocketIsHole); }
          else rootTags.Add(SpsHapticUtils.TagSpsSocketIsHole); } }
      MakeSender(parent,"Root",UnityEngine.Vector3.zero,ContactSenderRootRadius,rootTags.ToArray());
      var frontTags = new System.Collections.Generic.List<System.String>();
      if (compatMode == 0 || compatMode == 2) frontTags.Add(SpsHapticUtils.TagSpsSocketFront);
      if (compatMode != 2) frontTags.Add(SpsHapticUtils.TagTpsOrfFront);
      MakeSender(parent,"Front",UnityEngine.Vector3.forward * ContactSenderFrontOffset,ContactSenderRootRadius,frontTags.ToArray()); }
    public static UnityEngine.GameObject MakeSender(UnityEngine.GameObject parent,System.String name,UnityEngine.Vector3 pos,float radius,System.String[] tags)
    { var go = new UnityEngine.GameObject(name); go.transform.SetParent(parent.transform,false); go.transform.localPosition = pos;
      var s = go.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>(); s.position = pos; s.radius = radius; s.collisionTags = System.Linq.Enumerable.ToList(tags); return go; }
    public static UnityEngine.GameObject CreateDeformationLights(UnityEngine.GameObject ws,int lt)
    { var lights = new UnityEngine.GameObject("Lights"); lights.transform.SetParent(ws.transform,false);
      var main = new UnityEngine.GameObject("Root"); main.transform.SetParent(lights.transform,false);
      var ml = main.AddComponent<UnityEngine.Light>(); ml.type = UnityEngine.LightType.Point; ml.color = UnityEngine.Color.black; ml.range = (lt == SpsAddLight.Ring || lt == SpsAddLight.RingOneWay) ? 0.4206f : 0.4106f; ml.shadows = UnityEngine.LightShadows.None; ml.renderMode = UnityEngine.LightRenderMode.ForceVertex;
      var front = new UnityEngine.GameObject("Front"); front.transform.SetParent(lights.transform,false); front.transform.localPosition = UnityEngine.Vector3.forward * ContactSenderFrontOffset;
      var fl = front.AddComponent<UnityEngine.Light>(); fl.type = UnityEngine.LightType.Point; fl.color = UnityEngine.Color.black; fl.range = 0.4506f; fl.shadows = UnityEngine.LightShadows.None; fl.renderMode = UnityEngine.LightRenderMode.ForceVertex; return lights; }
    public static void SetActive(UnityEngine.AnimationClip c,UnityEngine.GameObject o,System.Boolean v)
    { if (o == null) return; System.String p = UnityEditor.AnimationUtility.CalculateTransformPath(o.transform,o.transform.root);
      UnityEditor.AnimationUtility.SetEditorCurve(c,UnityEditor.EditorCurveBinding.FloatCurve(p,typeof(UnityEngine.GameObject),"m_IsActive"),UnityEngine.AnimationCurve.Constant(0f,0f,v ? 1f : 0f)); }
    public static UnityEngine.AnimationClip SaveClip(UnityEngine.AnimationClip c,System.String path)
    { var x = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path); if (x != null) { UnityEditor.EditorUtility.CopySerialized(c,x); return x; } UnityEditor.AssetDatabase.CreateAsset(c,path); return c; }
    public static void EnsureBoolParam(UnityEditor.Animations.AnimatorController c,System.String n) { if (!System.Linq.Enumerable.Any(c.parameters, p => p.name == n)) c.AddParameter(n,UnityEngine.AnimatorControllerParameterType.Bool); }
    public static void EnsureFloatParam(UnityEditor.Animations.AnimatorController c,System.String n,float d = 0f)
    { if (System.Linq.Enumerable.Any(c.parameters, p => p.name == n)) return; c.AddParameter(n,UnityEngine.AnimatorControllerParameterType.Float);
      var ps = c.parameters; for (int i = 0; i < ps.Length; i++) { if (ps[i].name == n) { var p = ps[i]; p.defaultFloat = d; ps[i] = p; break; } } }
    public static UnityEditor.Animations.BlendTree NewDirect(System.String n) => new UnityEditor.Animations.BlendTree { name = n,blendType = UnityEditor.Animations.BlendTreeType.Direct,useAutomaticThresholds = false,blendParameter = "(f)Weight" };
    public static UnityEditor.Animations.BlendTree Make1D(System.String cp,System.String op,float v0,float v1,float v2)
    { var t = new UnityEditor.Animations.BlendTree { name = $"{op}_1D",blendType = UnityEditor.Animations.BlendTreeType.Simple1D,blendParameter = cp,useAutomaticThresholds = false };
      t.AddChild(SetClip(op,v0),0f); t.AddChild(SetClip(op,v1),0f); t.AddChild(SetClip(op,v2),1f); return t; }
    public static UnityEngine.AnimationClip SetClip(System.String p,float v)
    { var c = new UnityEngine.AnimationClip { name = p.Replace('/','_') + "_S" }; UnityEditor.AnimationUtility.SetEditorCurve(c,UnityEditor.EditorCurveBinding.FloatCurve("",typeof(UnityEngine.Animator),p),UnityEngine.AnimationCurve.Constant(0f,0f,v)); c.wrapMode = UnityEngine.WrapMode.Clamp; return c; }
    public static void SaveBT(UnityEditor.Animations.BlendTree t,System.String path) { var x = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.BlendTree>(path); if (x != null) UnityEditor.AssetDatabase.DeleteAsset(path); UnityEditor.AssetDatabase.CreateAsset(t,path); }
    public static void CreateExclusiveLayer(UnityEditor.Animations.AnimatorController ctrl,System.Collections.Generic.List<(System.String,System.String)> triggers,System.String stealthP,System.String dualP)
    { var l = GetOrAddLayer(ctrl,LayerExclusive); var sm = l.stateMachine; var start = sm.AddState("Start",new UnityEngine.Vector3(200,0,0)); start.writeDefaultValues = true;
      for (int i = 0; i < triggers.Count; i++) { var (name,on) = triggers[i]; var st = sm.AddState(name,new UnityEngine.Vector3(200,100 + i * 80,0)); st.writeDefaultValues = true;
        var e = sm.AddEntryTransition(st); e.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,on); e.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"IsLocal");
        if (stealthP != null) e.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot,0,stealthP); if (dualP != null) e.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot,0,dualP);
        var dc = new UnityEngine.AnimationClip { name = $"{name}_Drive" }; for (int j = 0; j < triggers.Count; j++) { if (i == j) continue; var (_,otherOn) = triggers[j]; UnityEditor.AnimationUtility.SetEditorCurve(dc,UnityEditor.EditorCurveBinding.FloatCurve("",typeof(UnityEngine.Animator),otherOn),UnityEngine.AnimationCurve.Constant(0f,0f,0f)); } st.motion = dc; }
      sm.defaultState = start; }
    public static void CreateAutoModeLayer(UnityEditor.Animations.AnimatorController ctrl,System.String autoP,System.Collections.Generic.List<(System.String,System.String,System.String)> sockets)
    { var l = GetOrAddLayer(ctrl,LayerAutoCompare); var sm = l.stateMachine;
      var rt = sm.AddState("Remote trap",new UnityEngine.Vector3(200,0,0)); var sd = sm.AddState("Stopped",new UnityEngine.Vector3(300,0,0));
      var ss = sm.AddState("Start",new UnityEngine.Vector3(400,0,0)); var sp = sm.AddState("Stop",new UnityEngine.Vector3(500,0,0));
      var t1 = rt.AddTransition(sd); t1.hasExitTime = false; t1.duration = 0; t1.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"IsLocal");
      var t2 = sd.AddTransition(ss); t2.hasExitTime = false; t2.duration = 0; t2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,autoP);
      var t3 = ss.AddTransition(sp); t3.hasExitTime = false; t3.duration = 0; t3.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot,0,autoP);
      var sdc = new UnityEngine.AnimationClip { name = "AutoStop" }; foreach (var (_,tp,_) in sockets) UnityEditor.AnimationUtility.SetEditorCurve(sdc,UnityEditor.EditorCurveBinding.FloatCurve("",typeof(UnityEngine.Animator),tp),UnityEngine.AnimationCurve.Constant(0f,0f,0f)); sp.motion = sdc;
      var t4 = sp.AddTransition(sd); t4.hasExitTime = false; t4.duration = 0; t4.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"_One");
      System.String vsP = $"{autoP}/Cmp"; EnsureFloatParam(ctrl,vsP,0f);
      var states = new System.Collections.Generic.Dictionary<(int,int),UnityEditor.Animations.AnimatorState>();
      for (int i = 0; i < sockets.Count; i++) { var (aN,aE,aD) = sockets[i]; var to = sm.AddState($"On {aN}",new UnityEngine.Vector3(100,200 + i * 140,0)); to.writeDefaultValues = true;
        var toc = new UnityEngine.AnimationClip { name = $"AutoOn_{aN}" }; UnityEditor.AnimationUtility.SetEditorCurve(toc,UnityEditor.EditorCurveBinding.FloatCurve("",typeof(UnityEngine.Animator),aE),UnityEngine.AnimationCurve.Constant(0f,0f,1f)); to.motion = toc; states[(i,-1)] = to;
        var tf = sm.AddState($"Off {aN}",new UnityEngine.Vector3(100,200 + i * 140 + 60,0)); tf.writeDefaultValues = true;
        var tfc = new UnityEngine.AnimationClip { name = $"AutoOff_{aN}" }; UnityEditor.AnimationUtility.SetEditorCurve(tfc,UnityEditor.EditorCurveBinding.FloatCurve("",typeof(UnityEngine.Animator),aE),UnityEngine.AnimationCurve.Constant(0f,0f,0f)); tf.motion = tfc;
        var t5 = tf.AddTransition(ss); t5.hasExitTime = false; t5.duration = 0; t5.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"_One"); states[(i,-2)] = tf;
        for (int j = 0; j < sockets.Count; j++) { if (i == j) continue; var (bN,bE,bD) = sockets[j];
          var vs = sm.AddState($"{aN}v{bN}",new UnityEngine.Vector3(300 + j * 200,200 + i * 140,0));
          var dbt = NewDirect($"{aN}_v_{bN}"); dbt.AddChild(SetClip(vsP,1f)); dbt.children[0].directBlendParameter = bD; dbt.AddChild(SetClip(vsP,-1f)); dbt.children[1].directBlendParameter = aD; vs.motion = dbt; states[(i,j)] = vs; } }
      for (int i = 0; i < sockets.Count; i++) { var (name,enabled,dist) = sockets[i]; var trOn = states[(i,-1)]; var trOff = states[(i,-2)];
        int fj = i == 0 ? 1 : 0; states.TryGetValue((i,fj), out var fc);
        if (fc != null) { var ts = ss.AddTransition(fc); ts.hasExitTime = false; ts.duration = 0; ts.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,enabled); }
        if (fc != null && trOn != null) { var ts2 = trOn.AddTransition(fc); ts2.hasExitTime = false; ts2.duration = 0; ts2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"_One"); }
        for (int j = 0; j < sockets.Count; j++) { if (i == j) continue; var cur = states[(i,j)]; var oa = states[(j,-1)];
          if (cur != null && oa != null) { var toa = cur.AddTransition(oa); toa.hasExitTime = false; toa.duration = 0; toa.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater,0,vsP); }
          int nj = j + 1; if (nj == i) nj++; if (nj >= sockets.Count) { if (cur != null) { var tof = cur.AddTransition(trOff); tof.hasExitTime = false; tof.duration = 0; tof.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less,0,dist); tof.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less,0.001f,vsP); var ts3 = cur.AddTransition(ss); ts3.hasExitTime = false; ts3.duration = 0; ts3.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"_One"); } }
          else { states.TryGetValue((i,nj), out var nx); if (cur != null && nx != null) { var tn = cur.AddTransition(nx); tn.hasExitTime = false; tn.duration = 0; tn.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"_One"); } } } }
      if (sockets.Count > 0) { var f = sockets[0]; var fTo = states[(0,-1)]; int fcIdx = 0 == 0 ? 1 : 0; states.TryGetValue((0,fcIdx), out var fCmp);
        if (fTo != null) { var tf2 = ss.AddTransition(fTo); tf2.hasExitTime = false; tf2.duration = 0; tf2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot,0,f.Item2); tf2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater,0,f.Item3); }
        if (fCmp != null) { var tf3 = ss.AddTransition(fCmp); tf3.hasExitTime = false; tf3.duration = 0; tf3.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,"_One"); } }
      sm.defaultState = sd; }
    public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control MakeToggle(System.String name,System.String param) { return new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control { name = name,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle,parameter = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter { name = param },icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1 }; }
    public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control MakeButton(System.String name) { return new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control { name = name,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Button,icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1 }; }
    public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control MakeSub(System.String name,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu sub) { return new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control { name = name,type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu,subMenu = sub,icon = null,labels = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Label[0],style = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Style.Style1 }; }
    public static UnityEditor.Animations.AnimatorControllerLayer GetOrAddLayer(UnityEditor.Animations.AnimatorController c,System.String n)
    { foreach (var l in c.layers) if (l.name == n) return l; var sm = new UnityEditor.Animations.AnimatorStateMachine { name = n }; sm.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
      System.String ap = UnityEditor.AssetDatabase.GetAssetPath(c); if (!System.String.IsNullOrEmpty(ap)) UnityEditor.AssetDatabase.AddObjectToAsset(sm,c);
      var layer = new UnityEditor.Animations.AnimatorControllerLayer { name = n,stateMachine = sm,defaultWeight = 1f,blendingMode = UnityEditor.Animations.AnimatorLayerBlendingMode.Override };
      var ls = System.Linq.Enumerable.ToList(c.layers); ls.Add(layer); c.layers = ls.ToArray(); return layer; }
    public static UnityEngine.AnimationClip GetOrCreateAutoClip(System.String folder)
    { if (_autoClip != null) return _autoClip; _autoClip = new UnityEngine.AnimationClip { name = "Enable SPS Auto Contacts" }; return SaveClip(_autoClip,$"{folder}/{_autoClip.name}.anim"); }
    public static void ResetCachedState() { _autoClip = null; }
    public static System.String MakeUniqueId(System.Collections.Generic.HashSet<System.String> used,System.String p) { for (int i = 0; ; i++) { System.String n = p + (i == 0 ? "" : i.ToString()); if (!used.Contains(n)) { used.Add(n); return n; } } }
    public static System.String SanitizeForParameter(System.String n)
    { if (System.String.IsNullOrEmpty(n)) return "Socket"; var sb = new System.Text.StringBuilder();
      foreach (char c in n) { if (System.Char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.') sb.Append(c); else if (c == '/' || c == '\\' || c == ' ') sb.Append('_'); }
      System.String r = sb.ToString(); return System.String.IsNullOrEmpty(r) ? "Socket" : r; }
    /* ── Find nearest bone to a world position ── */
    public static UnityEngine.Transform FindNearestBone(UnityEngine.Animator anim,UnityEngine.Vector3 worldPos)
    { if (anim == null || !anim.isHuman) return null;
      UnityEngine.Transform nearest = null; float minDist = float.MaxValue;
      foreach (UnityEngine.HumanBodyBones bone in System.Enum.GetValues(typeof(UnityEngine.HumanBodyBones)))
      { if (bone == UnityEngine.HumanBodyBones.LastBone) continue;
        var t = anim.GetBoneTransform(bone); if (t == null) continue;
        float d = UnityEngine.Vector3.Distance(worldPos,t.position);
        if (d < minDist) { minDist = d; nearest = t; } }
      return nearest; }
  }
}
}
#endif
