#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class AssetUnpack
{
  // === MENU ITEMS ===
  public static System.Boolean ValidateAssetMenu()
  { var o = UnityEditor.Selection.activeObject;
  if (o is UnityEditor.Animations.AnimatorController) return true;
  if (o is VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu) return true;
  return false; }
  public static void RunAssetMenu()
  { var o = UnityEditor.Selection.activeObject;
  if (o is UnityEditor.Animations.AnimatorController fx) { AssetUnpack.UnpackFX(fx); return; }
  if (o is VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu m) { AssetUnpack.UnpackMenu(m); return; }
  }
  public static System.Boolean ValidateContextCtrl(UnityEditor.MenuCommand c) => c.context is UnityEditor.Animations.AnimatorController;
  public static void RunContextCtrl(UnityEditor.MenuCommand c)
  { if (c.context is UnityEditor.Animations.AnimatorController fx) AssetUnpack.UnpackFX(fx); }
  public static System.Boolean ValidateContextMenu(UnityEditor.MenuCommand c) => c.context is VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
  public static void RunContextMenu(UnityEditor.MenuCommand c)
  { if (c.context is VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu m) AssetUnpack.UnpackMenu(m); }
  // === SHARED ===
  public static System.String Dir(System.String path) => System.IO.Path.GetDirectoryName(path)?.Replace('\\','/');
  public static System.String Name(System.String path) => System.IO.Path.GetFileName(path);
  public static System.String Join(System.String a,System.String b) => a + "/" + b;
  public static System.String SafeName(System.String n,System.String fallback) => System.String.IsNullOrWhiteSpace(n) ? fallback : n;
  public static System.String Sanitize(System.String n) { if (System.String.IsNullOrWhiteSpace(n)) return "_"; System.String s = n.Replace("Copied from ","").Replace("VRCFury FX_","").Replace("Faery 2.0_Toggle","Toggle").Replace("Faery 2.0","").Replace('/','_').Replace('\\','_').Replace(':','_').Replace('*','_').Replace('?','_').Replace('"','_').Replace('<','_').Replace('>','_').Replace('|','_').Replace("__","_").Replace("__","_").Replace("__","_").Replace("__","_").Replace("__","_").Replace("VRCFury FX","").Trim(); return s.Length > 80 ? s.Substring(0,80) : s; }
  public static System.Boolean EnsureFolder(System.String path)
  { if (UnityEditor.AssetDatabase.IsValidFolder(path)) return true;
  System.String p = AssetUnpack.Dir(path); System.String c = AssetUnpack.Name(path);
  if (System.String.IsNullOrEmpty(p) || System.String.IsNullOrEmpty(c)) return false;
  return !System.String.IsNullOrEmpty(UnityEditor.AssetDatabase.CreateFolder(p,c)); }
  public static System.String GllCFolder(System.String parent,System.String child)
  { if (System.String.IsNullOrEmpty(parent) || System.String.IsNullOrEmpty(child)) return null;
  System.String path = AssetUnpack.Join(parent,child); if (UnityEditor.AssetDatabase.IsValidFolder(path)) return path;
  System.String guid = UnityEditor.AssetDatabase.CreateFolder(parent,child); System.String gpath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
  return !System.String.IsNullOrEmpty(gpath) ? gpath : (UnityEditor.AssetDatabase.IsValidFolder(path) ? path : null); }
  // === FX ===
  public static System.String FXClonePath(System.String root,System.String name) => UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(root,UnpackConsts.PrefixUnpacked + name + UnpackConsts.SfxController));
  public static System.String FXValidSrc(UnityEditor.Animations.AnimatorController fx)
  { System.String p = UnityEditor.AssetDatabase.GetAssetPath(fx);
  if (System.String.IsNullOrEmpty(p)) { UnityEngine.Debug.LogError(UnpackConsts.LogBadPath); return null; }
  return p; }
  public static System.String FXRoot(System.String src,System.String name)
  { System.String dir = AssetUnpack.Dir(src); if (System.String.IsNullOrEmpty(dir)) { UnityEngine.Debug.LogError(UnpackConsts.LogBadFolder); return null; }
  System.String root = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(dir,UnpackConsts.PrefixUnpacked + name));
  if (!AssetUnpack.EnsureFolder(root)) { UnityEngine.Debug.LogError(UnpackConsts.LogOutFolder); return null; }
  return root; }
  public static System.String SanitizeLayerName(System.String n)
  { if (System.String.IsNullOrWhiteSpace(n)) return "Default";
  System.String s = AssetUnpack.Sanitize(n);
  return System.String.IsNullOrWhiteSpace(s) ? "Default" : s; }
  public static System.String[] FXLayerSubFolders(System.String root,System.String layerName)
  { System.String layer = AssetUnpack.SanitizeLayerName(layerName);
  System.String layerRoot = AssetUnpack.GllCFolder(root,layer);
  if (System.String.IsNullOrEmpty(layerRoot)) return null;
  System.String anims = AssetUnpack.GllCFolder(layerRoot,UnpackConsts.FolderAnims);
  System.String bts = AssetUnpack.GllCFolder(layerRoot,UnpackConsts.FolderBTs);
  if (System.String.IsNullOrEmpty(anims) || System.String.IsNullOrEmpty(bts)) { UnityEngine.Debug.LogError(UnpackConsts.LogSubFolder); return null; }
  return new[] { anims,bts }; }
  public static System.String FXUnknownLayerRoot(System.String root)
  { return AssetUnpack.GllCFolder(root,UnpackConsts.UnknownLayer); }
  public static UnityEditor.Animations.AnimatorController FXCloneCtrl(System.String src,System.String root,System.String name)
  { System.String dst = AssetUnpack.FXClonePath(root,name); if (!UnityEditor.AssetDatabase.CopyAsset(src,dst)) { UnityEngine.Debug.LogError(UnpackConsts.LogCloneFail + src); return null; }
  UnityEditor.AssetDatabase.ImportAsset(dst,UnityEditor.ImportAssetOptions.ForceUpdate);
  var nfx = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(dst);
  if (nfx == null) UnityEngine.Debug.LogError(UnpackConsts.LogLoadFail); return nfx; }
  public static System.String FXFindCommonPrefix(System.Collections.Generic.List<System.String> names)
  { if (names == null || names.Count < 2) return "";
  // Sanitize all names first and filter out empties
  var cleaned = new System.Collections.Generic.List<System.String>();
  foreach (System.String n in names)
  { System.String s = AssetUnpack.Sanitize(AssetUnpack.SafeName(n,UnpackConsts.NameClip));
    if (s.Length >= 2) cleaned.Add(s); }
  if (cleaned.Count < 2) return "";
  // Find the longest prefix that at least 60% of sanitized names share and ends with _
  int threshold = System.Math.Max(2,cleaned.Count * 60 / 100);
  System.String bestPrefix = "";
  // Try prefix lengths from longest to shortest (max 30 chars to be reasonable)
  int maxCheckLen = 30;
  for (int len = maxCheckLen; len >= 2; len--)
  { var prefixCounts = new System.Collections.Generic.Dictionary<System.String,int>();
    foreach (System.String n in cleaned)
    { if (n.Length < len) continue;
    System.String p = n.Substring(0,len);
    if (!p.EndsWith("_")) continue; // only consider prefixes ending with _
    if (prefixCounts.ContainsKey(p)) prefixCounts[p]++; else prefixCounts[p] = 1; }
    foreach (var kv in prefixCounts)
    { if (kv.Value >= threshold && kv.Key.Length > bestPrefix.Length)
      bestPrefix = kv.Key; }
    if (!System.String.IsNullOrEmpty(bestPrefix)) break; }
  return bestPrefix; }
  public static System.String FXApplyName(System.String rawName,System.String commonPrefix)
  { System.String baseName = AssetUnpack.Sanitize(AssetUnpack.SafeName(rawName,UnpackConsts.NameClip));
  if (System.String.IsNullOrWhiteSpace(baseName) || baseName == "_" || baseName == UnpackConsts.NameClip)
    baseName = "clip"; // ensure non-empty name
  if (!System.String.IsNullOrEmpty(commonPrefix) && baseName.StartsWith(commonPrefix))
    baseName = baseName.Substring(commonPrefix.Length);
  // If stripping the prefix left an empty name,use the original
  if (System.String.IsNullOrWhiteSpace(baseName))
    baseName = AssetUnpack.Sanitize(AssetUnpack.SafeName(rawName,UnpackConsts.NameClip));
  return UnpackConsts.PrefixUnpacked + baseName; }
  public static void FXExtractMotionsByLayer(UnityEditor.Animations.AnimatorController nfx,System.String dst,System.String root)
  { var map = new System.Collections.Generic.Dictionary<UnityEngine.Motion,UnityEngine.Motion>();
  // Detect common prefix from clip names for cleaner output names
  var allClipNames = new System.Collections.Generic.List<System.String>();
  foreach (UnityEngine.Motion m in AssetUnpack.MotionsAtPath(dst))
    if (m is UnityEngine.AnimationClip c) allClipNames.Add(c.name);
  System.String commonPrefix = AssetUnpack.FXFindCommonPrefix(allClipNames);
  // Process each layer independently: all motions go into that layer's own folders
  foreach (UnityEditor.Animations.AnimatorControllerLayer layer in nfx.layers)
  { System.String[] subs = AssetUnpack.FXLayerSubFolders(root,layer.name);
    if (subs == null) continue;
    System.String anims = subs[0]; System.String bts = subs[1];
    // Collect states then process BTs first,then clips — all in the SAME layer folder
    var sts = new System.Collections.Generic.List<UnityEditor.Animations.AnimatorState>();
    AssetUnpack.FXCollectStates(layer.stateMachine,sts);
    foreach (UnityEditor.Animations.AnimatorState st in sts)
    { if (st?.motion == null) continue;
    st.motion = AssetUnpack.FXCloneMotion(st.motion,map,anims,bts,commonPrefix);
    UnityEditor.EditorUtility.SetDirty(st); } }
  UnityEditor.AssetDatabase.SaveAssets(); AssetUnpack.FXTryDetachMotions(dst); }
  public static void FXUnpackMasks(UnityEditor.Animations.AnimatorController nfx,System.String root)
  { System.String dst = UnityEditor.AssetDatabase.GetAssetPath(nfx);
  System.String masks = AssetUnpack.GllCFolder(root,UnpackConsts.FolderMasks);
  if (System.String.IsNullOrEmpty(masks)) return;
  var maskGuid = new System.Collections.Generic.Dictionary<System.String,UnityEngine.AvatarMask>();
  var layers = nfx.layers;
  for (int i = 0; i < layers.Length; i++)
  { var l = layers[i];
    if (l.avatarMask == null) continue;
    // Check if this mask is embedded in the controller (same asset path)
    System.String maskAssetPath = UnityEditor.AssetDatabase.GetAssetPath(l.avatarMask);
    if (maskAssetPath != dst) continue; // already a separate asset,skip
    System.String key = l.avatarMask.name ?? "";
    if (maskGuid.TryGetValue(key,out var existing))
    { l.avatarMask = existing; layers[i] = l; UnityEditor.EditorUtility.SetDirty(nfx); continue; }
    // Clone the embedded mask to a standalone .mask asset
    var nMask = new UnityEngine.AvatarMask();
    UnityEditor.EditorUtility.CopySerialized(l.avatarMask,nMask);
    nMask.name = UnpackConsts.PrefixUnpacked + AssetUnpack.Sanitize(AssetUnpack.SafeName(l.avatarMask.name,"mask"));
    System.String path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(masks,nMask.name + UnpackConsts.SfxMask));
    UnityEditor.AssetDatabase.CreateAsset(nMask,path);
    UnityEditor.EditorUtility.SetDirty(nMask);
    l.avatarMask = nMask; layers[i] = l; UnityEditor.EditorUtility.SetDirty(nfx);
    maskGuid[key] = nMask; }
  UnityEditor.AssetDatabase.SaveAssets(); }
  public static void FXFinalize(UnityEditor.Animations.AnimatorController nfx,System.String name,System.String root)
  {
  AssetUnpack.FXCreateParams(nfx,root);
  UnityEditor.EditorUtility.SetDirty(nfx); UnityEditor.AssetDatabase.SaveAssets(); UnityEditor.AssetDatabase.Refresh();
  UnityEditor.EditorGUIUtility.PingObject(nfx); UnityEditor.Selection.activeObject = nfx; UnityEngine.Debug.Log(UnpackConsts.LogDone + name + UnpackConsts.LogTo + root); }
  public static void UnpackFX(UnityEditor.Animations.AnimatorController fx)
  { System.String src = AssetUnpack.FXValidSrc(fx); System.String root = src != null ? AssetUnpack.FXRoot(src,fx.name) : null; if (root == null) return;
  UnityEditor.AssetDatabase.Refresh();
  UnityEditor.Animations.AnimatorController nfx = AssetUnpack.FXCloneCtrl(src,root,fx.name); if (nfx == null) return;
  AssetUnpack.FXExtractMotionsByLayer(nfx,UnityEditor.AssetDatabase.GetAssetPath(nfx),root);
  AssetUnpack.FXUnpackMasks(nfx,root);
  AssetUnpack.FXFinalize(nfx,fx.name,root); }
  public static void FXCollectStates(UnityEditor.Animations.AnimatorStateMachine sm,System.Collections.Generic.List<UnityEditor.Animations.AnimatorState> sts)
  { if (sm == null) return;
  foreach (UnityEditor.Animations.ChildAnimatorState cs in sm.states) sts.Add(cs.state);
  foreach (UnityEditor.Animations.ChildAnimatorStateMachine cm in sm.stateMachines) AssetUnpack.FXCollectStates(cm.stateMachine,sts); }
  public static System.Collections.Generic.List<UnityEditor.Animations.AnimatorState> FXStates(UnityEditor.Animations.AnimatorController fx)
  { var sts = new System.Collections.Generic.List<UnityEditor.Animations.AnimatorState>();
  foreach (UnityEditor.Animations.AnimatorControllerLayer layer in fx.layers) AssetUnpack.FXCollectStates(layer.stateMachine,sts);
  return sts; }
  public static System.Collections.Generic.List<UnityEngine.Motion> MotionsAtPath(System.String path)
  { var list = new System.Collections.Generic.List<UnityEngine.Motion>();
  foreach (UnityEngine.Object o in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path)) if (o is UnityEngine.Motion m) list.Add(m);
  return list; }
  public static void FXTryDetachMotion(UnityEngine.Motion src)
  { if (src == null) return;
  System.String p = UnityEditor.AssetDatabase.GetAssetPath(src);
  UnityEditor.AssetDatabase.RemoveObjectFromAsset(src);
  if (UnityEditor.AssetDatabase.GetAssetPath(src) == p) UnityEngine.Debug.LogWarning(UnpackConsts.LogMoveFail + src.name); }
  public static void FXTryDetachMotions(System.String path)
  { foreach (UnityEngine.Motion m in AssetUnpack.MotionsAtPath(path)) if (m is UnityEditor.Animations.BlendTree) AssetUnpack.FXTryDetachMotion(m);
  foreach (UnityEngine.Motion m in AssetUnpack.MotionsAtPath(path)) if (m is UnityEngine.AnimationClip) AssetUnpack.FXTryDetachMotion(m);
  if (AssetUnpack.MotionsAtPath(path).Count > 0) UnityEngine.Debug.LogWarning(UnpackConsts.LogMovePassFail); }
  public static UnityEngine.Motion FXCloneMotion(UnityEngine.Motion src,System.Collections.Generic.Dictionary<UnityEngine.Motion,UnityEngine.Motion> map,System.String anims,System.String bts,System.String commonPrefix = "")
  { if (src == null) return null; if (map.TryGetValue(src,out UnityEngine.Motion c)) return c;
  if (src is UnityEngine.AnimationClip clip) return AssetUnpack.FXCloneClip(clip,map,anims,commonPrefix);
  if (src is UnityEditor.Animations.BlendTree bt) return AssetUnpack.FXCloneBT(bt,map,anims,bts,commonPrefix);
  map[src] = src; return src; }
  public static UnityEngine.Motion FXCloneClip(UnityEngine.AnimationClip clip,System.Collections.Generic.Dictionary<UnityEngine.Motion,UnityEngine.Motion> map,System.String anims,System.String commonPrefix = "")
  { if (System.String.IsNullOrEmpty(anims) || !UnityEditor.AssetDatabase.IsValidFolder(anims)) { UnityEngine.Debug.LogError(UnpackConsts.LogAnimPath); return null; }
  UnityEngine.AnimationClip nclip = new UnityEngine.AnimationClip(); UnityEditor.EditorUtility.CopySerialized(clip,nclip);
  nclip.name = AssetUnpack.FXApplyName(clip.name,commonPrefix);
  System.String path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(anims,nclip.name + UnpackConsts.SfxAnim)); if (System.String.IsNullOrEmpty(path)) { UnityEngine.Debug.LogError(UnpackConsts.LogAnimPath); UnityEngine.Object.DestroyImmediate(nclip); return null; }
  UnityEditor.AssetDatabase.CreateAsset(nclip,path); UnityEditor.EditorUtility.SetDirty(nclip); map[clip] = nclip; return nclip; }
  public static void FXRemapChildren(UnityEditor.Animations.BlendTree src,UnityEditor.Animations.BlendTree dst,System.Collections.Generic.Dictionary<UnityEngine.Motion,UnityEngine.Motion> map,System.String anims,System.String bts,System.String commonPrefix = "")
  { UnityEditor.Animations.ChildMotion[] srcs = src.children; UnityEditor.Animations.ChildMotion[] dsts = new UnityEditor.Animations.ChildMotion[srcs.Length];
  for (int i = 0; i < srcs.Length; i++) { dsts[i] = srcs[i]; dsts[i].motion = AssetUnpack.FXCloneMotion(srcs[i].motion,map,anims,bts,commonPrefix); }
  dst.children = dsts; UnityEditor.EditorUtility.SetDirty(dst); }
  public static UnityEngine.Motion FXCloneBT(UnityEditor.Animations.BlendTree bt,System.Collections.Generic.Dictionary<UnityEngine.Motion,UnityEngine.Motion> map,System.String anims,System.String bts,System.String commonPrefix = "")
  { if (System.String.IsNullOrEmpty(bts) || !UnityEditor.AssetDatabase.IsValidFolder(bts)) { UnityEngine.Debug.LogError(UnpackConsts.LogBTPath); return null; }
  UnityEditor.Animations.BlendTree nbt = new UnityEditor.Animations.BlendTree(); nbt.name = AssetUnpack.Sanitize(AssetUnpack.SafeName(bt.name,UnpackConsts.NameBT)); nbt.blendType = bt.blendType; nbt.blendParameter = bt.blendParameter; nbt.blendParameterY = bt.blendParameterY; nbt.minThreshold = bt.minThreshold; nbt.maxThreshold = bt.maxThreshold; nbt.useAutomaticThresholds = bt.useAutomaticThresholds;
  System.String path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(bts,nbt.name + UnpackConsts.SfxAsset)); if (System.String.IsNullOrEmpty(path)) { UnityEngine.Debug.LogError(UnpackConsts.LogBTPath); UnityEngine.Object.DestroyImmediate(nbt); return null; }
  UnityEditor.AssetDatabase.CreateAsset(nbt,path); UnityEditor.EditorUtility.SetDirty(nbt); map[bt] = nbt; AssetUnpack.FXRemapChildren(bt,nbt,map,anims,bts,commonPrefix); return nbt; }
  public static void FXCreateParams(UnityEditor.Animations.AnimatorController fx,System.String root)
  { var ep = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>(); var ps = new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();
  foreach (UnityEngine.AnimatorControllerParameter p in fx.parameters) { if (p.type == UnityEngine.AnimatorControllerParameterType.Trigger) continue; ps.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter { name = p.name,saved = UnpackConsts.VrcSaved,valueType = AssetUnpack.FXToVrcType(p.type),defaultValue = AssetUnpack.FXParamDefault(p) }); }
  ep.parameters = ps.ToArray();
  UnityEditor.AssetDatabase.CreateAsset(ep,UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(root,UnpackConsts.FxParamsAsset))); }
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType FXToVrcType(UnityEngine.AnimatorControllerParameterType type)
  { if (type == UnityEngine.AnimatorControllerParameterType.Int) return VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Int;
  if (type == UnityEngine.AnimatorControllerParameterType.Bool) return VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool;
  return VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Float; }
  public static float FXParamDefault(UnityEngine.AnimatorControllerParameter parameter)
  { if (parameter.type == UnityEngine.AnimatorControllerParameterType.Bool) return parameter.defaultBool ? 1f : 0f;
  if (parameter.type == UnityEngine.AnimatorControllerParameterType.Int) return parameter.defaultInt;
  return parameter.defaultFloat; }
  // === MENU ===
  public static System.String MenuValidSrc(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu m)
  { System.String p = UnityEditor.AssetDatabase.GetAssetPath(m);
  if (System.String.IsNullOrEmpty(p)) { UnityEngine.Debug.LogError(UnpackConsts.LogBadPath); return null; }
  return p; }
  public static System.String MenuRoot(System.String src,System.String name)
  { System.String dir = AssetUnpack.Dir(src); if (System.String.IsNullOrEmpty(dir)) { UnityEngine.Debug.LogError(UnpackConsts.LogBadFolder); return null; }
  System.String root = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(dir,UnpackConsts.PrefixUnpacked + name));
  if (!AssetUnpack.EnsureFolder(root)) { UnityEngine.Debug.LogError(UnpackConsts.LogOutFolder); return null; }
  return root; }
  public static System.String MenuSubFolder(System.String root)
  { System.String menus = AssetUnpack.GllCFolder(root,UnpackConsts.FolderMenus);
  if (System.String.IsNullOrEmpty(menus)) { UnityEngine.Debug.LogError(UnpackConsts.LogSubFolder); return null; }
  return menus; }
  public static System.String MenuClonePath(System.String folder,System.String name)
  { return UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetUnpack.Join(folder,name + UnpackConsts.SfxAsset)); }
  public static System.String MenuApplyName(System.String rawName)
  { System.String baseName = AssetUnpack.Sanitize(AssetUnpack.SafeName(rawName,UnpackConsts.NameMenu));
  return UnpackConsts.PrefixUnpacked + baseName; }
  public static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu MenuClone(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu src,System.Collections.Generic.Dictionary<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu> map,System.String menus)
  { if (src == null) return null; if (map.TryGetValue(src,out VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu c)) return c;
  VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu n = UnityEngine.ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(); UnityEditor.EditorUtility.CopySerialized(src,n);
  n.name = AssetUnpack.MenuApplyName(src.name);
  System.String path = AssetUnpack.MenuClonePath(menus,n.name);
  if (System.String.IsNullOrEmpty(path)) { UnityEngine.Debug.LogError(UnpackConsts.LogMenuPath); UnityEngine.Object.DestroyImmediate(n); return null; }
  UnityEditor.AssetDatabase.CreateAsset(n,path); UnityEditor.EditorUtility.SetDirty(n); map[src] = n;
  AssetUnpack.MenuRemapSubs(n,map,menus); return n; }
  public static void MenuRemapSubs(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu n,System.Collections.Generic.Dictionary<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu> map,System.String menus)
  { var controls = n.controls;
  for (int i = 0; i < controls.Count; i++)
  { var ctrl = controls[i];
    if (ctrl.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu && ctrl.subMenu != null)
    { ctrl.subMenu = AssetUnpack.MenuClone(ctrl.subMenu,map,menus);
    controls[i] = ctrl; } }
  UnityEditor.EditorUtility.SetDirty(n); }
  public static void MenuFinalize(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu n,System.String name,System.String root)
  { UnityEditor.EditorUtility.SetDirty(n); UnityEditor.AssetDatabase.SaveAssets(); UnityEditor.AssetDatabase.Refresh();
  UnityEditor.EditorGUIUtility.PingObject(n); UnityEditor.Selection.activeObject = n; UnityEngine.Debug.Log(UnpackConsts.LogDone + name + UnpackConsts.LogTo + root); }
  public static void UnpackMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu m)
  { System.String src = AssetUnpack.MenuValidSrc(m); System.String root = src != null ? AssetUnpack.MenuRoot(src,m.name) : null; if (root == null) return;
  System.String menus = AssetUnpack.MenuSubFolder(root); if (menus == null) return;
  UnityEditor.AssetDatabase.Refresh();
  var map = new System.Collections.Generic.Dictionary<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
  VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu n = AssetUnpack.MenuClone(m,map,menus); if (n == null) return;
  AssetUnpack.MenuFinalize(n,m.name,root); }
}
}
}
#endif
