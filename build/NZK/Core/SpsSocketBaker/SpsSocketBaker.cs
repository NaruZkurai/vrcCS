#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class SpsSocketBaker {
 public const System.String DpsPlugGuid = "703106f8586c4f64b5aa39b6b4676684";
    public const System.String VrcfuryComponentGuid = "d9e94e501a2d4c95bff3d5601013d923";
    public static System.String[] HumanBoneNames => Systems.Baking.HumanBoneNames;
    
    public static void Bake(UnityEngine.GameObject spsRoot)
    { if (spsRoot == null) { Vars.Errors.Log(Vars.Errors.E_NoObjects,"SPS Bake: no target"); return; }
      var avatarName = Vars.Names.Sanitize(spsRoot.transform.root.name);
      var avatarRoot = Systems.Baking.FindAviRoot(avatarName,spsRoot) ?? spsRoot.transform.root.gameObject;
      /* ── Validate avatar root ── */
      if (avatarRoot == null) { Vars.Errors.Log(Vars.Errors.E_NoAviRootForSps,"No avatar root for " + spsRoot.name); UnityEngine.Debug.LogWarning("[NZK] " + Vars.Errors.E_NoAviRootForSps + " - SPS Bake: Select a GameObject inside an avatar hierarchy."); return; }
      var vrcad = avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
      if (vrcad == null) { Vars.Errors.Log(Vars.Errors.E_NoAviRootForSps,"No VRCAvatarDescriptor on " + avatarRoot.name); UnityEngine.Debug.LogWarning("[NZK] " + Vars.Errors.E_NoAviRootForSps + " - SPS Bake: The avatar root must have a VRCAvatarDescriptor component."); return; }
      var sockets = FindDpsSockets(spsRoot);
      if (sockets == null || sockets.Count == 0) { Vars.Errors.Log(Vars.Errors.E_NoSpsSockets,"No SPS sockets found on " + spsRoot.name); UnityEngine.Debug.LogWarning("[NZK] " + Vars.Errors.E_NoSpsSockets + " - SPS Bake: No SPS sockets found on " + spsRoot.name + ". Use GameObject/NZK/SPS/Create Socket first."); return; }
      /* ── Warn if prefab instance ── */
      if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(spsRoot))
      { UnityEngine.Debug.Log("[SPS Baker] Prefab instance detected, unpacking " + spsRoot.name + ".");
        UnityEditor.PrefabUtility.UnpackPrefabInstance(spsRoot,UnityEditor.PrefabUnpackMode.Completely,UnityEditor.InteractionMode.UserAction); }
      System.String outputBase = "Assets/!_NZK_Generated";
      var genRoot = Systems.Baking.GetGenRoot(avatarName,avatarRoot);
      var spsChild = genRoot.Find("SPS GENERATOR");
      /* ── Preserve existing configs on rebake ── */
      var existingConfigs = new System.Collections.Generic.Dictionary<System.String,SpsSocketConfig>();
      if (spsChild != null) { foreach (UnityEngine.Transform c in spsChild) { var cfg = c.GetComponent<SpsSocketConfig>(); if (cfg != null && cfg.socketTransform != null) existingConfigs[cfg.socketName] = cfg; else UnityEngine.Object.DestroyImmediate(c.gameObject); } }
      if (spsChild == null) { var g = new UnityEngine.GameObject("SPS GENERATOR"); g.transform.SetParent(genRoot,false); spsChild = g.transform; }
      var genGo = spsChild.gameObject;
      var generator = genGo.GetComponent<C_SpsSocketGenerator>();
      if (generator == null) { generator = genGo.AddComponent<C_SpsSocketGenerator>(); }
      generator.avatarName = avatarName; generator.outputFolder = outputBase; genGo.tag = "EditorOnly";
      foreach (var socketData in sockets)
      { /* ── Deduplicate: skip if this socket was already processed ── */
        System.Boolean alreadyDone = false;
        if (existingConfigs.TryGetValue(socketData.socketName,out var existingCfg))
        { if (existingCfg.socketTransform == socketData.gameObject.transform) alreadyDone = true; }
        if (!alreadyDone) ProcessSocket(spsRoot,avatarRoot,generator,socketData);
        var socketObj = socketData.gameObject;
        if (socketObj != null) { foreach (var light in socketObj.GetComponentsInChildren<UnityEngine.Light>(true)) if (light != null && light.color == UnityEngine.Color.black && light.range < 1f) UnityEditor.Undo.DestroyObjectImmediate(light.gameObject); Systems.Baking.DestroyAllVrcfuryComponents(socketObj); } }
      generator.RefreshSockets(); GenerateAll(generator);
      /* ── Assign generated menu and params to VRCAD ── */
      if (vrcad != null)
      { System.String menuDir2 = "Assets/!_NZK_Generated/" + avatarName + "/Menus";
        var spsMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(menuDir2 + "/" + avatarName + "_SPS_Menu.asset");
        if (spsMenu != null) { vrcad.expressionsMenu = spsMenu; vrcad.customExpressions = true; UnityEditor.EditorUtility.SetDirty(vrcad); }
        /* Create/find SPS params from the SPS controller */
        var spsCtrlPath2 = "Assets/!_NZK_Generated/" + avatarName + "/controllers/" + avatarName + "_SPS.controller";
        var spsCtrl2 = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(spsCtrlPath2);
        if (spsCtrl2 != null)
        { var paramPath = menuDir2 + "/" + avatarName + "_SPS_Params.asset";
          var prm = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(paramPath);
          if (prm == null) { prm = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(); UnityEditor.AssetDatabase.CreateAsset(prm,paramPath); }
          var prmList = new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();
          foreach (var p in spsCtrl2.parameters)
          { var vt = p.type == UnityEngine.AnimatorControllerParameterType.Bool ? VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool
              : p.type == UnityEngine.AnimatorControllerParameterType.Int ? VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Int
              : VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Float;
            prmList.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter { name = p.name,valueType = vt,defaultValue = 0f,saved = true,networkSynced = true }); }
          prm.parameters = prmList.ToArray(); UnityEditor.EditorUtility.SetDirty(prm);
          vrcad.expressionParameters = prm; UnityEditor.EditorUtility.SetDirty(vrcad); } }
      var cnt = sockets.Count; UnityEngine.Debug.Log("[NZK] SPS Bake: Baked " + cnt + " SPS socket" + (cnt == 1 ? "" : "s") + ". Output: " + outputBase + "/" + avatarName + "/" + (generator != null ? "Socket_NZK_" + (generator.sockets?.Count ?? 0) : "SPS")); }
    public static System.Collections.Generic.List<SocketData> FindDpsSockets(UnityEngine.GameObject root)
    { var results =  new System.Collections.Generic.List<SocketData>();
      /* ── Phase 1: Scan baked hierarchy for pre-existing baked sockets ── */
      var avatarName = Vars.Names.Sanitize(root.transform.root.name);
      var genRoot = Systems.Baking.GetGenRoot(avatarName,root);
      if (genRoot != null)
      { var spsGen = genRoot.Find("SPS GENERATOR");
        if (spsGen != null) foreach (UnityEngine.Transform child in spsGen)
        { if (child.name.EndsWith(" Config") && child.GetComponent<SpsSocketConfig>() != null)
          { var cfg = child.GetComponent<SpsSocketConfig>();
            if (cfg.socketTransform != null)
            { UnityEngine.Debug.Log("[SPS Baker] Found pre-existing config: " + cfg.socketName);
              results.Add(new SocketData { gameObject = cfg.socketTransform.gameObject,
                socketName = cfg.socketName,menuPath = cfg.menuPath,V_addLight = cfg.V_addLight,
                enableAuto = cfg.enableAuto,position = cfg.position,rotation = cfg.rotation,
                length = cfg.length,enableHandTouchZone2 = cfg.enableHandTouchZone2 ? 1 : 0,
                parameterOverride = cfg.parameterOverride,boneLinkIndex = cfg.boneLinkIndex,
                compatMode = cfg.compatMode }); } } } }
      /* ── Phase 2: Scan all MonoBehaviours for socket components ── */
      var allComponents = root.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true);
      System.String dpsPlugGuidPath = !System.String.IsNullOrEmpty(DpsPlugGuid) ? UnityEditor.AssetDatabase.GUIDToAssetPath(DpsPlugGuid) : null;
      System.String vrcfuryGuidPath = !System.String.IsNullOrEmpty(VrcfuryComponentGuid) ? UnityEditor.AssetDatabase.GUIDToAssetPath(VrcfuryComponentGuid) : null;
      var vrcfuryMap = new System.Collections.Generic.Dictionary<UnityEngine.GameObject,UnityEngine.MonoBehaviour>();
      foreach (var comp in allComponents)
      { if (comp == null) continue; var compType = comp.GetType(); System.String fullName = compType.FullName ?? ""; System.String typeName = compType.Name;
        /* Detect by fully qualified name or GUID (VRCFury installed) */
        System.Boolean isVrcfury = fullName == "VF.Model.VRCFury" || fullName.Contains("VF.Model");
        if (!isVrcfury && !System.String.IsNullOrEmpty(vrcfuryGuidPath)) { var script = UnityEditor.MonoScript.FromMonoBehaviour(comp); if (script != null) isVrcfury = UnityEditor.AssetDatabase.GetAssetPath(script) == vrcfuryGuidPath; }
        if (isVrcfury && !vrcfuryMap.ContainsKey(comp.gameObject)) vrcfuryMap[comp.gameObject] = comp;
        /* Detect missing-script components (VRCFury uninstalled) by serialized data */
        if (!isVrcfury && typeName == "MonoBehaviour")
        { var so = new UnityEditor.SerializedObject(comp); var sp = so.FindProperty("m_Script");
          if (sp != null && sp.objectReferenceValue == null)
          { /* Missing script — check serialized data for known VF socket fields */
            var nameProp = so.FindProperty("name"); var oscProp = so.FindProperty("oscId");
            var lightProp = so.FindProperty("V_addLight"); var autoProp = so.FindProperty("enableAuto");
            if (nameProp != null || oscProp != null || lightProp != null)
            { /* This looks like a VRCFury socket component — extract what we can */
              var sd = new SocketData { gameObject = comp.gameObject,
                socketName = nameProp?.stringValue ?? comp.gameObject.name,
                menuPath = nameProp?.stringValue ?? comp.gameObject.name,
                V_addLight = lightProp?.intValue ?? 0,
                enableAuto = autoProp?.boolValue ?? false,
                position = so.FindProperty("position")?.vector3Value ?? UnityEngine.Vector3.zero,
                rotation = so.FindProperty("rotation")?.vector3Value ?? UnityEngine.Vector3.zero,
                length = so.FindProperty("length")?.floatValue ?? 0f,
                enableHandTouchZone2 = so.FindProperty("enableHandTouchZone2")?.intValue ?? 0,
                parameterOverride = oscProp?.stringValue ?? null,
                boneLinkIndex = -1,compatMode = 0 };
              if (!AlreadyExists(results,sd.gameObject)) { results.Add(sd); UnityEngine.Debug.Log("[SPS Baker] Found missing-script socket on " + comp.gameObject.name); } } } }
        /* Detect by type name for C_NzkSpsSocket */
        if (comp is C_NzkSpsSocket nzkSocket)
        { if (!AlreadyExists(results,comp.gameObject)) results.Add(new SocketData { gameObject = comp.gameObject,socketName = nzkSocket.name ?? comp.gameObject.name,menuPath = nzkSocket.name ?? comp.gameObject.name,V_addLight = (int)nzkSocket.V_addLight,enableAuto = nzkSocket.enableAuto,position = nzkSocket.position,rotation = nzkSocket.rotation,length = nzkSocket.length,enableHandTouchZone2 = (int)nzkSocket.enableHandTouchZone2,parameterOverride = nzkSocket.oscId,boneLinkIndex = -1,compatMode = (int)nzkSocket.compatMode }); continue; } }
      /* ── Phase 3: Detect VRCFury HapticSocket components by type name ── */
      foreach (var comp in allComponents)
      { if (comp == null) continue; if (AlreadyExists(results,comp.gameObject)) continue;
        var compType = comp.GetType(); System.String fullName = compType.FullName ?? ""; System.String name = compType.Name;
        System.Boolean isDpsPlug = fullName == "VF.Component.VRCFuryHapticSocket" || fullName.Contains("VRCFuryHapticSocket");
        if (!isDpsPlug && !System.String.IsNullOrEmpty(dpsPlugGuidPath)) { var script = UnityEditor.MonoScript.FromMonoBehaviour(comp); if (script != null) isDpsPlug = UnityEditor.AssetDatabase.GetAssetPath(script) == dpsPlugGuidPath; }
        isDpsPlug = isDpsPlug || name == "VRCFuryHapticSocket"; if (!isDpsPlug) continue;
        var so = new UnityEditor.SerializedObject(comp); var data = new SocketData { gameObject = comp.gameObject,socketName = so.FindProperty("name")?.stringValue ?? comp.gameObject.name,menuPath = so.FindProperty("name")?.stringValue ?? comp.gameObject.name,V_addLight = so.FindProperty("V_addLight")?.intValue ?? 0,enableAuto = so.FindProperty("enableAuto")?.boolValue ?? false,position = so.FindProperty("position")?.vector3Value ?? UnityEngine.Vector3.zero,rotation = so.FindProperty("rotation")?.vector3Value ?? UnityEngine.Vector3.zero,length = so.FindProperty("length")?.floatValue ?? 0f,enableHandTouchZone2 = so.FindProperty("enableHandTouchZone2")?.intValue ?? 0,parameterOverride = so.FindProperty("parameterOverride")?.stringValue ?? null };
        if (vrcfuryMap.TryGetValue(comp.gameObject,out var vrcfuryComp)) { var furySo = new UnityEditor.SerializedObject(vrcfuryComp); data.boneLinkIndex = ExtractBoneLinkIndex(furySo); }
        results.Add(data); }
      return results; }
    static System.Boolean AlreadyExists(System.Collections.Generic.List<SocketData> list,UnityEngine.GameObject go)
    { if (go == null) return true; foreach (var d in list) if (d.gameObject == go) return true; return false; }
    public static int ExtractBoneLinkIndex(UnityEditor.SerializedObject furySo)
    { var target = furySo.targetObject; if (target == null) return -1; var t = target.GetType();
      try { var getAllMethod = t.GetMethod("GetAllFeatures",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (getAllMethod != null) { var features = getAllMethod.Invoke(target,null) as System.Collections.IList; if (features != null) { foreach (var feature in features) { if (feature == null) continue; var ft = feature.GetType(); if (ft.Name == "ArmatureLink" || (ft.FullName ?? "").Contains("ArmatureLink")) { var linkToField = ft.GetField("linkTo",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (linkToField != null) { var linkTo = linkToField.GetValue(feature) as System.Collections.IList; if (linkTo != null && linkTo.Count > 0) { var boneField = linkTo[0].GetType().GetField("bone",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (boneField != null) return (int)boneField.GetValue(linkTo[0]); } } } } } }
        var contentField = t.GetField("content",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (contentField != null) { var content = contentField.GetValue(target); if (content != null) { var contentType = content.GetType(); if (contentType.Name == "ArmatureLink" || (contentType.FullName ?? "").Contains("ArmatureLink")) { var linkToField = contentType.GetField("linkTo",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (linkToField != null) { var linkTo = linkToField.GetValue(content) as System.Collections.IList; if (linkTo != null && linkTo.Count > 0) { var boneField = linkTo[0].GetType().GetField("bone",System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (boneField != null) return (int)boneField.GetValue(linkTo[0]); } } } } } }
      catch (System.Exception _ex) { UnityEngine.Debug.LogError("[NZK] Unhandled exception: " + _ex.Message); }
      try { var refs = furySo.FindProperty("m_References"); if (refs != null && refs.isArray && refs.arraySize > 0) { for (int i = 0; i < refs.arraySize; i++) { var refId = refs.GetArrayElementAtIndex(i); var typeProp = refId.FindPropertyRelative("type"); if (typeProp == null) continue; System.String typeStr = typeProp.stringValue ?? ""; if (!typeStr.Contains("ArmatureLink")) continue; var dataProp = refId.FindPropertyRelative("data"); if (dataProp == null) continue; var linkTo = dataProp.FindPropertyRelative("linkTo"); if (linkTo != null && linkTo.isArray && linkTo.arraySize > 0) { var firstLink = linkTo.GetArrayElementAtIndex(0); if (firstLink != null) { var boneProp = firstLink.FindPropertyRelative("bone"); if (boneProp != null) return boneProp.intValue; } } } } }
      catch (System.Exception _ex) { UnityEngine.Debug.LogWarning("[AvatarValidator] ParseInt error: " + _ex.Message); } return -1; }
    public static void ProcessSocket(UnityEngine.GameObject spsRoot,UnityEngine.GameObject avatarRoot,C_SpsSocketGenerator generator,SocketData socketData)
    { var configGo = new UnityEngine.GameObject(socketData.socketName + " Config"); configGo.transform.SetParent(generator.transform,false); UnityEditor.Undo.RegisterCreatedObjectUndo(configGo,"Create Socket Config"); configGo.tag = "EditorOnly";
      var config = configGo.AddComponent<SpsSocketConfig>(); config.socketName = socketData.socketName; config.menuPath = socketData.menuPath; config.V_addLight = socketData.V_addLight; config.enableAuto = socketData.enableAuto; config.position = socketData.position; config.rotation = socketData.rotation; config.length = socketData.length; config.enableHandTouchZone2 = socketData.enableHandTouchZone2 != 0; config.parameterOverride = socketData.parameterOverride; config.boneLinkIndex = socketData.boneLinkIndex; config.compatMode = socketData.compatMode; config.socketTransform = socketData.gameObject.transform; config.toggleParamName = "(b-gt)SPS_" + Vars.Names.Sanitize(socketData.socketName);
      if (socketData.boneLinkIndex >= 0 && socketData.boneLinkIndex < HumanBoneNames.Length) Systems.Baking.ReparentToBone(socketData.gameObject,avatarRoot,socketData.boneLinkIndex,config); }
    public static void GenerateAll(C_SpsSocketGenerator generator)
    { System.String avatarName = Vars.Names.Sanitize(generator.avatarName); System.String socketCount = "Socket_NZK_" + (generator.sockets?.Count ?? 0); System.String spsFolder = generator.outputFolder + "/" + avatarName + "/" + socketCount; System.String ctrlDir = generator.outputFolder + "/" + avatarName + "/controllers"; System.String menuFolder = generator.outputFolder + "/" + avatarName + "/Menus"; generator.generatedFolder = spsFolder;
      Systems.Folder.Ensure(spsFolder); Systems.Folder.Ensure(spsFolder + "/Animations"); Systems.Folder.Ensure(ctrlDir); Systems.Folder.Ensure(menuFolder);
      generator.RefreshSockets(); if (generator.sockets.Count == 0) return;
      var avatarObj = Systems.Baking.FindAviRoot(generator.avatarName,generator.gameObject) ?? UnityEngine.GameObject.Find(generator.avatarName);
      if (avatarObj == null) { UnityEngine.Debug.LogError("[SPS Baker] Could not find UnityEngine.Avatar root UnityEngine.GameObject for " + generator.avatarName); return; }
      var vrcad = avatarObj.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
      foreach (var socket in generator.sockets) BuildSocketAnimations(socket,spsFolder,avatarName);
      if (generator.generateFXLayer || generator.generateToggle || generator.generateMenu)
      { System.String spsCtrlPath = ctrlDir + "/" + avatarName + "_SPS.controller"; var spsController = CreateSpsOnlyController(spsCtrlPath);
        if (spsController == null) { UnityEngine.Debug.LogError("[SPS Baker] Could not create SPS-only controller"); return; }
        { System.String spsBtDir = generator.outputFolder + "/" + avatarName + "/blendtrees/sockets"; Systems.Folder.Ensure(spsBtDir); }
        SpsSocketBakeService.BakeAllSockets(generator,spsController,vrcad,spsFolder,avatarName);
        Systems.Baking.FxMerger.MergeAndAssign(ctrlDir,avatarName,avatarObj,vrcad,new[] { spsController });
        /* ── Configure SPS2 resolver on avatar body meshes ── */
        SpsResolverService.ConfigureResolverOnBody(avatarObj,spsController,spsFolder,avatarName); }
      RegisterSpsTogglesInToggleGenerator(generator,spsFolder,avatarName); UnityEditor.AssetDatabase.SaveAssets(); UnityEngine.Debug.Log("[SPS Baker] Generation complete for " + avatarName + " -> " + spsFolder); }
    public static void RegisterSpsTogglesInToggleGenerator(C_SpsSocketGenerator generator,System.String outputBase,System.String avatarName)
    { var genRoot = Systems.Baking.GetGenRoot(avatarName); var tgTr = genRoot.Find("HB_ToggleGenerator");
      if (tgTr == null) { UnityEngine.Debug.Log("[SPS Baker] No Toggle Generator found. Clips in: " + outputBase + "/Animations/"); return; }
      var tgBaker = tgTr.GetComponent<C_AviGenerator>(); if (tgBaker == null) return; tgBaker.CreateToggleChildren();
      var togglesContainer = tgBaker.transform.Find("HB_Toggles"); if (togglesContainer == null) return;
      var prebaked = togglesContainer.Find("Prebaked Toggles"); if (prebaked == null) prebaked = CreateChild(togglesContainer,"Prebaked Toggles");
      var settings = prebaked.Find("Settings"); if (settings == null) settings = CreateChild(prebaked,"Settings");
      System.String tgAnimFolder = "Assets/NZK_Generated/ToggleGenerator/" + avatarName + "/Animations/"; Systems.Folder.Ensure(tgAnimFolder);
      foreach (var socket in generator.sockets) { if (socket == null || System.String.IsNullOrEmpty(socket.socketName)) continue;
        System.String toggleName = new System.String(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(socket.socketName, c => !System.Linq.Enumerable.Contains(System.IO.Path.GetInvalidFileNameChars(), c) && c != ' '))) + "_SPS";
        if (settings.Find(toggleName) != null) continue;
        var toggleGo = new UnityEngine.GameObject(toggleName); toggleGo.transform.SetParent(settings,false);
        var toggleHb = toggleGo.AddComponent<C_AviGenerator>(); toggleHb.mode = E_AviGeneratorMode.Animation; toggleHb.isToggleChild = true; toggleHb.toggleSourceType = C_AviGenerator.TToggleSourceType.ObjectToggle; toggleHb.toggleSourceObjects = new System.Collections.Generic.List<UnityEngine.GameObject>(); if (socket.socketTransform != null) toggleHb.toggleSourceObjects.Add(socket.socketTransform.gameObject); toggleHb.toggleParameterName = socket.toggleParamName; toggleHb.toggleDisplayName = socket.socketName; toggleHb.avatarRootName = avatarName; toggleHb.animationClip = socket.onClip; toggleHb.offClip = socket.offClip;
        UnityEditor.Undo.RegisterCreatedObjectUndo(toggleGo,"Create SPS Toggle"); UnityEngine.Debug.Log($"[SPS Baker] Created toggle '{toggleName}' under Prebaked Toggles/Settings with param {socket.toggleParamName}"); }
      UnityEngine.Debug.Log("[SPS Baker] Registered " + generator.sockets.Count + " SPS toggles in Toggle Generator."); }
    public static UnityEngine.Transform CreateChild(UnityEngine.Transform parent,System.String name) { var go = new UnityEngine.GameObject(name); go.transform.SetParent(parent,false); return go.transform; }
    public static void BakeToggleFromVrcfury(UnityEngine.GameObject target) => Systems.Baking.BakeToggleFromVrcfury(target);
    public static void BakeArmatureLink(UnityEngine.GameObject target) => Systems.Baking.BakeArmatureLink(target);
    public static void BuildSocketAnimations(SpsSocketConfig socket,System.String outputBase,System.String avatarName)
    { if (socket.socketTransform == null) { UnityEngine.Debug.LogWarning("[SPS Baker] Socket " + socket.socketName + " has null socketTransform,skipping."); return; }
      System.String socketPath = UnityEditor.AnimationUtility.CalculateTransformPath(socket.socketTransform,socket.socketTransform.root);
      var onClip = new UnityEngine.AnimationClip(); onClip.name = avatarName + "_" + Vars.Names.Sanitize(socket.socketName) + "_On";
      UnityEditor.AnimationUtility.SetEditorCurve(onClip,UnityEditor.EditorCurveBinding.FloatCurve(socketPath,typeof(UnityEngine.GameObject),"m_IsActive"),UnityEngine.AnimationCurve.Constant(0f,0f,1f));
      var offClip = new UnityEngine.AnimationClip(); offClip.name = avatarName + "_" + Vars.Names.Sanitize(socket.socketName) + "_Off";
      UnityEditor.AnimationUtility.SetEditorCurve(offClip,UnityEditor.EditorCurveBinding.FloatCurve(socketPath,typeof(UnityEngine.GameObject),"m_IsActive"),UnityEngine.AnimationCurve.Constant(0f,0f,0f));
      socket.onClip = SaveClip(onClip,outputBase + "/Animations/" + onClip.name + ".anim"); socket.offClip = SaveClip(offClip,outputBase + "/Animations/" + offClip.name + ".anim"); }
    public static UnityEngine.AnimationClip SaveClip(UnityEngine.AnimationClip clip,System.String path)
    { var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path); if (existing != null) { UnityEditor.EditorUtility.CopySerialized(clip,existing); UnityEditor.EditorUtility.SetDirty(existing); return existing; } UnityEditor.AssetDatabase.CreateAsset(clip,path); return clip; }
    public static UnityEditor.Animations.AnimatorController CreateSpsOnlyController(System.String path)
    { Systems.Folder.Ensure(System.IO.Path.GetDirectoryName(path)); UnityEditor.AssetDatabase.DeleteAsset(path); var ctrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path); if (ctrl.layers.Length > 0) ctrl.RemoveLayer(0); return ctrl; }
    /* ── BakeSingle: bake/rebake a single socket instantly, saves as prefab ── */
    public static void BakeSingle(UnityEngine.GameObject socketObj)
    { if (socketObj == null) return;
      var socket = socketObj.GetComponent<C_NzkSpsSocket>();
      if (socket == null) return;
      var avatarName = Vars.Names.Sanitize(socketObj.transform.root.name);
      var avatarRoot = Systems.Baking.FindAviRoot(avatarName,socketObj) ?? socketObj.transform.root.gameObject;
      if (avatarRoot == null) return;
      /* Ensure generator exists */
      var genRoot = Systems.Baking.GetGenRoot(avatarName,avatarRoot);
      if (genRoot == null) return;
      var spsChild = genRoot.Find("SPS GENERATOR");
      if (spsChild == null) { var g = new UnityEngine.GameObject("SPS GENERATOR"); g.transform.SetParent(genRoot,false); g.tag = "EditorOnly"; spsChild = g.transform; }
      var generator = spsChild.GetComponent<C_SpsSocketGenerator>();
      if (generator == null) { generator = spsChild.gameObject.AddComponent<C_SpsSocketGenerator>(); generator.avatarName = avatarName; generator.outputFolder = "Assets/!_NZK_Generated"; spsChild.gameObject.tag = "EditorOnly"; }
      /* Ensure SocketGenerator child */
      var sgChild = spsChild.Find("SocketGenerator");
      if (sgChild == null) { var sg = new UnityEngine.GameObject("SocketGenerator"); sg.transform.SetParent(spsChild,false); sg.tag = "EditorOnly"; sgChild = sg.transform; }
      /* Find or create config by socketTransform (survives rename) */
      SpsSocketConfig config = null;
      foreach (UnityEngine.Transform c in spsChild)
      { var cfg = c.GetComponent<SpsSocketConfig>();
        if (cfg != null && cfg.socketTransform == socketObj.transform) { config = cfg; break; } }
      if (config == null)
      { var cg = new UnityEngine.GameObject(socketObj.name + " Config"); cg.transform.SetParent(spsChild,false); cg.tag = "EditorOnly";
        config = cg.AddComponent<SpsSocketConfig>(); }
      config.socketName = socketObj.name; config.socketTransform = socketObj.transform;
      config.menuPath = socketObj.name; config.V_addLight = (int)socket.V_addLight;
      config.enableAuto = socket.enableAuto; config.position = socket.position;
      config.rotation = socket.rotation; config.length = socket.length;
      config.enableHandTouchZone2 = socket.enableHandTouchZone2 == C_NzkSpsSocket.EnableTouchZone.On;
      config.parameterOverride = socket.oscId; config.compatMode = (int)socket.compatMode;
      config.toggleParamName = "(b-gt)SPS_" + Vars.Names.Sanitize(socketObj.name);
      generator.RefreshSockets();
      /* Generate output folders */
      System.String safeName = Vars.Names.Sanitize(socketObj.name);
      System.String socketCount = "Socket_NZK_" + (generator.sockets?.Count ?? 0);
      System.String spsFolder = generator.outputFolder + "/" + avatarName + "/" + socketCount;
      System.String ctrlDir = generator.outputFolder + "/" + avatarName + "/controllers";
      System.String btDir = generator.outputFolder + "/" + avatarName + "/blendtrees/sockets/" + safeName;
      Systems.Folder.Ensure(spsFolder); Systems.Folder.Ensure(spsFolder + "/Animations"); Systems.Folder.Ensure(spsFolder + "/Prefabs");
      Systems.Folder.Ensure(ctrlDir); Systems.Folder.Ensure(btDir);
      /* Delete old prefab if name changed */
      var prefabPath = spsFolder + "/Prefabs/" + safeName + ".prefab";
      if (!System.String.IsNullOrEmpty(socket._spsPrefabPath) && socket._spsPrefabPath != prefabPath)
      { var oldAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(socket._spsPrefabPath);
        if (oldAsset != null) UnityEditor.AssetDatabase.DeleteAsset(socket._spsPrefabPath); }
      /* Remove old baked children and SocketGenerator prefab instance (collect then destroy) */
      var toDestroy = new System.Collections.Generic.List<UnityEngine.GameObject>();
      foreach (var child in socketObj.transform.GetComponentsInChildren<UnityEngine.Transform>(true))
        if (child != socketObj.transform && child.parent == socketObj.transform && (child.name == "OneSpace" || child.name.StartsWith("SPS_Marker")))
          toDestroy.Add(child.gameObject);
      foreach (var go in toDestroy) if (go != null) UnityEngine.Object.DestroyImmediate(go);
      for (int i = sgChild.childCount - 1; i >= 0; i--) { var c = sgChild.GetChild(i); if (c != null) UnityEngine.Object.DestroyImmediate(c.gameObject); }
      /* Find or create SPS controller */
      System.String ctrlPath = ctrlDir + "/" + avatarName + "_SPS.controller";
      var spsController = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(ctrlPath);
      if (spsController == null) spsController = CreateSpsOnlyController(ctrlPath);
      /* Build animations and bake */
      BuildSocketAnimations(config,spsFolder,avatarName);
      var usedOscIds = new System.Collections.Generic.HashSet<System.String>(System.StringComparer.OrdinalIgnoreCase);
      var autoSockets = new System.Collections.Generic.List<(System.String,System.String,System.String)>();
      var exclusive = new System.Collections.Generic.List<(System.String,System.String)>();
      var result = SpsSocketBakeService.BakeSocket(config,spsController,spsFolder,btDir,usedOscIds,null,null,ref autoSockets,ref exclusive,generator);
      /* Unwrap: move children from BakedSpsSocket direct to socketObj, save as prefab, instance in SocketGenerator */
      if (result != null && result.bakeRoot != null)
      { result.bakeRoot.transform.SetParent(socketObj.transform,false);
        var bakeChildren = new System.Collections.Generic.List<UnityEngine.Transform>();
        for (int i = 0; i < result.bakeRoot.transform.childCount; i++) bakeChildren.Add(result.bakeRoot.transform.GetChild(i));
        /* Save as prefab asset (auto-replaces scene bakeRoot with instance) */
        var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(result.bakeRoot,prefabPath);
        /* Move children directly under socketObj (no wrapper) */
        foreach (var child in bakeChildren) if (child != null) child.SetParent(socketObj.transform,true);
        /* Delete the empty wrapper */
        if (result.bakeRoot != null) UnityEngine.Object.DestroyImmediate(result.bakeRoot);
        if (prefab != null)
        { socket._spsPrefabPath = prefabPath;
          /* Instance prefab in SocketGenerator for tracking */
          var genInst = UnityEditor.PrefabUtility.InstantiatePrefab(prefab,sgChild) as UnityEngine.GameObject;
          if (genInst != null) { genInst.name = "BakedSpsSocket"; } } }
      /* Merge into FX if VRCAD exists */
      var vrcad = avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
      if (vrcad != null)
      { Systems.Baking.FxMerger.MergeAndAssign(ctrlDir,avatarName,avatarRoot,vrcad,new[] { spsController });
        /* Assign generated menu and params to VRCAD */
        System.String menuDir2 = "Assets/!_NZK_Generated/" + avatarName + "/Menus";
        var spsMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(menuDir2 + "/" + avatarName + "_SPS_Menu.asset");
        if (spsMenu != null) { vrcad.expressionsMenu = spsMenu; vrcad.customExpressions = true; UnityEditor.EditorUtility.SetDirty(vrcad); }
        var spsCtrlPath2 = ctrlDir + "/" + avatarName + "_SPS.controller";
        var spsCtrl2 = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(spsCtrlPath2);
        if (spsCtrl2 != null)
        { var paramPath = menuDir2 + "/" + avatarName + "_SPS_Params.asset";
          var prm = UnityEditor.AssetDatabase.LoadAssetAtPath<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(paramPath);
          if (prm == null) { prm = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(); UnityEditor.AssetDatabase.CreateAsset(prm,paramPath); }
          var prmList = new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();
          foreach (var p in spsCtrl2.parameters)
          { var vt = p.type == UnityEngine.AnimatorControllerParameterType.Bool ? VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool
              : p.type == UnityEngine.AnimatorControllerParameterType.Int ? VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Int
              : VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Float;
            prmList.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter { name = p.name,valueType = vt,defaultValue = 0f,saved = true,networkSynced = true }); }
          prm.parameters = prmList.ToArray(); UnityEditor.EditorUtility.SetDirty(prm);
          vrcad.expressionParameters = prm; UnityEditor.EditorUtility.SetDirty(vrcad); } }
      UnityEditor.AssetDatabase.SaveAssets();
      UnityEngine.Debug.Log("[SPS Baker] Auto-baked " + socketObj.name); }
  
}
}
}
#endif
