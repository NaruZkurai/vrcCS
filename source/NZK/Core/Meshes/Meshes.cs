#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Meshes {
  
  
  
  
  public static UnityEngine.Mesh GetMeshFromGameObject(UnityEngine.GameObject obj)
  { if (obj == null) { return null; }
  UnityEngine.SkinnedMeshRenderer smr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
  if (smr != null) { return smr.sharedMesh; }
  UnityEngine.MeshFilter mf = obj.GetComponent<UnityEngine.MeshFilter>();
  if (mf != null) { return mf.sharedMesh; }
  return null; }
  public static UnityEngine.Material[]  GetMaterialsFromGameObject(UnityEngine.GameObject obj)
  { if (obj == null) { return System.Array.Empty<UnityEngine.Material>(); }
  UnityEngine.SkinnedMeshRenderer smr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
  if (smr != null) { return smr.sharedMaterials; }
  UnityEngine.MeshRenderer mr = obj.GetComponent<UnityEngine.MeshRenderer>();
  if (mr != null) { return mr.sharedMaterials; }
  return System.Array.Empty<UnityEngine.Material>(); }
  public static UnityEngine.Transform AssignNewBone(UnityEngine.GameObject meshObject,UnityEngine.Transform parent,System.String boneName,float weight = -1f)
  { if (meshObject == null || parent == null) { return null; }
  if (weight < 0) { weight = Meshes.Vars.Consts.DefaultNearZeroVertexWeight; }
  weight = UnityEngine.Mathf.Clamp01(weight);
  UnityEngine.GameObject boneGO = new UnityEngine.GameObject(boneName);
  UnityEngine.Transform boneTransform = boneGO.transform;
  boneTransform.SetParent(parent,false);
  boneTransform.localPosition = UnityEngine.Vector3.zero;
  boneTransform.localRotation = UnityEngine.Quaternion.identity;
  boneTransform.localScale = UnityEngine.Vector3.one;
  return boneTransform; }
  public static void CreateNearZeroWeightVertexGroup(UnityEngine.Mesh mesh,int boneIndex,UnityEngine.Transform rootBone,System.Collections.Generic.List<UnityEngine.Transform> allBones)
  { if (mesh == null || allBones == null || allBones.Count == 0) { return; }
  UnityEngine.BoneWeight[] boneWeights = new UnityEngine.BoneWeight[mesh.vertexCount];
  for (int i = 0; i < mesh.vertexCount; i++)
  { boneWeights[i] = new UnityEngine.BoneWeight { boneIndex0 = boneIndex,weight0 = Meshes.Vars.Consts.DefaultNearZeroVertexWeight,boneIndex1 = 0,weight1 = 1f - Meshes.Vars.Consts.DefaultNearZeroVertexWeight }; }
  mesh.boneWeights = boneWeights;
  UnityEngine.Matrix4x4[] bindposes = new UnityEngine.Matrix4x4[allBones.Count];
  for (int i = 0; i < allBones.Count; i++)
  { if (allBones[i] != null) { bindposes[i] = allBones[i].worldToLocalMatrix * rootBone.localToWorldMatrix; } }
  mesh.bindposes = bindposes; }
  public static UnityEngine.Transform GetOrCreateMergeBoneHierarchy(UnityEngine.Transform avatarRoot)
  { if (avatarRoot == null) { return null; }
  UnityEngine.Transform armature = avatarRoot.Find("Armature");
  if (armature == null)
  { UnityEngine.GameObject armatureGO = new UnityEngine.GameObject("Armature");
    armatureGO.transform.SetParent(avatarRoot,false);
    armature = armatureGO.transform; }
  UnityEngine.Transform hips = armature.Find("Hips");
  if (hips == null)
  { UnityEngine.GameObject hipsGO = new UnityEngine.GameObject("Hips");
    hipsGO.transform.SetParent(armature,false);
    hips = hipsGO.transform; }
  UnityEngine.Transform gBones = hips.Find("G_bones");
  if (gBones == null)
  { UnityEngine.GameObject gBonesGO = new UnityEngine.GameObject("G_bones");
    gBonesGO.transform.SetParent(hips,false);
    gBones = gBonesGO.transform; }
  return gBones; }
  public static UnityEngine.Transform GetOrCreateNanimentBoneParent(UnityEngine.Transform avatarRoot)
  { if (avatarRoot == null) { return null; }
  UnityEngine.Transform armature = avatarRoot.Find("Armature");
  if (armature == null)
  { UnityEngine.GameObject armatureGO = new UnityEngine.GameObject("Armature");
    armatureGO.transform.SetParent(avatarRoot,false);
    armature = armatureGO.transform; }
  UnityEngine.Transform hips = armature.Find("Hips");
  if (hips == null)
  { UnityEngine.GameObject hipsGO = new UnityEngine.GameObject("Hips");
    hipsGO.transform.SetParent(armature,false);
    hips = hipsGO.transform; }
  UnityEngine.Transform naniBones = hips.Find("NaNim_bones");
  if (naniBones == null)
  { UnityEngine.GameObject naniBonesGO = new UnityEngine.GameObject("NaNim_bones");
    naniBonesGO.transform.SetParent(hips,false);
    naniBones = naniBonesGO.transform; }
  return naniBones; }
  public static MergeResult MergeMeshes(System.Collections.Generic.List<MeshEntry> entries,UnityEngine.Transform avatarRoot)
  { if (entries == null || entries.Count == 0) { return new MergeResult { Success = false,ErrorMessage = "No mesh entries provided" }; }
  System.Collections.Generic.List<MeshEntry> validEntries = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(entries, e => e != null && e.SourceMesh != null));
  if (validEntries.Count == 0) { return new MergeResult { Success = false,ErrorMessage = "No valid meshes in entries" }; }
  System.Collections.Generic.List<UnityEngine.CombineInstance> combineInstances =  new System.Collections.Generic.List<UnityEngine.CombineInstance>();
  System.Collections.Generic.List<VertexRange> vertexRanges =  new System.Collections.Generic.List<VertexRange>();
  int vertexOffset = 0;
  int submeshOffset = 0;
  foreach (MeshEntry entry in validEntries)
  { UnityEngine.Mesh mesh = entry.SourceMesh;
    int submeshCount = mesh.subMeshCount;
    for (int s = 0; s < submeshCount; s++)
    { combineInstances.Add(new UnityEngine.CombineInstance { mesh = mesh,subMeshIndex = s,transform = entry.SourceObject != null ? entry.SourceObject.transform.localToWorldMatrix : UnityEngine.Matrix4x4.identity }); }
    vertexRanges.Add(new VertexRange { StartVertex = vertexOffset,VertexCount = mesh.vertexCount,StartSubmesh = submeshOffset,SubmeshCount = submeshCount,BoneIndex = -1,BoneWeight = entry.BoneWeight });
    vertexOffset += mesh.vertexCount;
    submeshOffset += submeshCount; }
    UnityEngine.Mesh combinedMesh = new UnityEngine.Mesh {name = "CombinedMesh"};
    if (vertexOffset > 65535) { combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; }
  try { combinedMesh.CombineMeshes(combineInstances.ToArray(),false,true); }
  catch (System.ArgumentException ex)
  { if (ex.Message.Contains("index format"))
    { combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    combinedMesh.CombineMeshes(combineInstances.ToArray(),false,true); }
    else { return new MergeResult { Success = false,ErrorMessage = "Failed to combine meshes: " + ex.Message }; } }
  System.Collections.Generic.List<UnityEngine.Transform> generatedBones = null;
  if (System.Linq.Enumerable.Any(entries, e => e.CreateNewBone))
  { generatedBones = Meshes.SetupBoneWeights(combinedMesh,validEntries,vertexRanges,avatarRoot); }
 UnityEngine.Material[]  combinedMaterials = Meshes.CombineMaterials(validEntries);
  return new MergeResult { Success = true,MergedMesh = combinedMesh,Materials = combinedMaterials,GeneratedBones = generatedBones,ErrorMessage = "" }; }
  static System.Collections.Generic.List<UnityEngine.Transform> SetupBoneWeights(UnityEngine.Mesh mesh,System.Collections.Generic.List<MeshEntry> entries,System.Collections.Generic.List<VertexRange> ranges,UnityEngine.Transform avatarRoot)
  { System.Collections.Generic.List<UnityEngine.Transform> bones =  new System.Collections.Generic.List<UnityEngine.Transform>();
  UnityEngine.BoneWeight[] boneWeights = new UnityEngine.BoneWeight[mesh.vertexCount];
  UnityEngine.Transform hips = avatarRoot != null ? avatarRoot : null;
  if (hips == null && avatarRoot != null) { hips = avatarRoot.root; }
  bones.Add(hips);
  int rootBoneIndex = 0;
  UnityEngine.Transform genericBonesParent = hips != null ? Meshes.GetOrCreateMergeBoneHierarchy(hips) : null;
  UnityEngine.Transform naniBoneParent = hips != null ? Meshes.GetOrCreateNanimentBoneParent(hips) : null;
  int boneIndex = 1;
  System.Collections.Generic.Dictionary<MeshEntry,int> entryToBoneIndex = new System.Collections.Generic.Dictionary<MeshEntry,int>();
  foreach (MeshEntry entry in entries)
  { if (entry.CreateNewBone)
    { UnityEngine.Transform boneParent = UnityEngine.Mathf.Approximately(entry.BoneWeight,Meshes.Vars.Consts.DefaultNearZeroVertexWeight) ? naniBoneParent : genericBonesParent;
    System.String boneName = Meshes.Vars.Names.Build.BoneName(entry.BoneName,entry.BoneWeight);
    UnityEngine.Transform newBone = Meshes.AssignNewBone(entry.SourceObject,boneParent,boneName,entry.BoneWeight);
    if (newBone != null)
    { bones.Add(newBone);
      entryToBoneIndex[entry] = boneIndex;
      boneIndex++; } } }
  for (int i = 0; i < ranges.Count; i++)
  { VertexRange range = ranges[i];
    MeshEntry entry = entries[i];
    int assignedBoneIndex = rootBoneIndex;
    if (entryToBoneIndex.TryGetValue(entry,out int bidx)) { assignedBoneIndex = bidx; }
    for (int v = range.StartVertex; v < range.StartVertex + range.VertexCount; v++)
    { float boneWeight = entry.CreateNewBone ? entry.BoneWeight : 0f;
    if (boneWeight <= 0f)
    { boneWeights[v] = new UnityEngine.BoneWeight { boneIndex0 = rootBoneIndex,weight0 = 1f }; }
    else
    { boneWeights[v] = new UnityEngine.BoneWeight { boneIndex0 = assignedBoneIndex,weight0 = boneWeight,boneIndex1 = rootBoneIndex,weight1 = 1f - boneWeight }; } } }
  mesh.boneWeights = boneWeights;
  if (hips != null)
  { UnityEngine.Matrix4x4[] bindposes = new UnityEngine.Matrix4x4[bones.Count];
    for (int i = 0; i < bones.Count; i++)
    { if (bones[i] != null) { bindposes[i] = bones[i].worldToLocalMatrix * hips.localToWorldMatrix; } }
    mesh.bindposes = bindposes; }
  return bones; }
  static UnityEngine.Material[]  CombineMaterials(System.Collections.Generic.List<MeshEntry> entries)
  { System.Collections.Generic.List<UnityEngine.Material> combinedMaterials =  new System.Collections.Generic.List<UnityEngine.Material>();
  System.Collections.Generic.HashSet<UnityEngine.Material> seen = new System.Collections.Generic.HashSet<UnityEngine.Material>();
  foreach (MeshEntry entry in entries)
  { if (entry.Materials != null)
    { foreach (UnityEngine.Material mat in entry.Materials)
    { if (mat != null && !seen.Contains(mat))
      { combinedMaterials.Add(mat);
      seen.Add(mat); } } } }
  return combinedMaterials.Count > 0 ? combinedMaterials.ToArray() : new UnityEngine.Material[]  { new UnityEngine.Material(UnityEngine.Shader.Find("Standard")) }; }
  public static UnityEngine.Mesh SaveMeshAsset(UnityEngine.Mesh mesh,System.String assetPath,System.String meshName)
  { if (mesh == null || System.String.IsNullOrEmpty(assetPath)) { return null; }
  System.String folderPath = System.IO.Path.GetDirectoryName(assetPath);
  Systems.Folder.Ensure(folderPath.Replace('\\','/'));
  mesh.name = meshName;
  if (IF_UE.Load<UnityEngine.Mesh>(assetPath) != null)
  { IF_UE.DeleteAsset(assetPath); }
  IF_UE.CreateAsset(mesh,assetPath);
  IF_UE.SaveAndRefresh();
  return IF_UE.Load<UnityEngine.Mesh>(assetPath); }
  public static System.String MergedMeshAssetPath(UnityEngine.GameObject[] selectedObjects)
  { if (selectedObjects == null || selectedObjects.Length == 0) { return null; }
  System.String avatarName = selectedObjects[0].transform.root != null ? selectedObjects[0].transform.root.name : selectedObjects[0].name;
  System.String parentNames = System.String.Join("_",System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(selectedObjects, o => o != null), o => o.transform.parent != null ? NZK.Core.Vars.Names.Sanitize(o.transform.parent.name) : NZK.Core.Vars.Names.Sanitize(o.name)), n => !System.String.IsNullOrEmpty(n))));
  if (System.String.IsNullOrEmpty(parentNames)) { parentNames = "MergedMesh"; }
  return Meshes.Vars.Consts.MeshOutputFolder + "/" + NZK.Core.Vars.Names.Sanitize(avatarName) + "/" + Meshes.Vars.Names.Build.MergedMeshName(avatarName,parentNames) + ".asset"; }
  public static UnityEngine.Mesh SaveMergedMeshAsset(UnityEngine.Mesh mesh,UnityEngine.GameObject[] selectedObjects)
  { System.String assetPath = Meshes.MergedMeshAssetPath(selectedObjects);
  if (System.String.IsNullOrEmpty(assetPath)) { return mesh; }
  return Meshes.SaveMeshAsset(mesh,assetPath,System.IO.Path.GetFileNameWithoutExtension(assetPath)); }
  public static void ApplyMergedMesh(UnityEngine.GameObject targetObject,UnityEngine.Mesh mesh,UnityEngine.Material[]  materials)
  { if (targetObject == null || mesh == null) { return; }
  UnityEngine.SkinnedMeshRenderer smr = targetObject.GetComponent<UnityEngine.SkinnedMeshRenderer>();
  if (smr != null)
  { smr.sharedMesh = mesh;
    if (materials != null && materials.Length > 0) { smr.sharedMaterials = materials; }
    return; }
  UnityEngine.MeshFilter mf = targetObject.GetComponent<UnityEngine.MeshFilter>();
  if (mf != null)
  { mf.sharedMesh = mesh;
    UnityEngine.MeshRenderer mr = targetObject.GetComponent<UnityEngine.MeshRenderer>();
    if (mr == null) { mr = targetObject.AddComponent<UnityEngine.MeshRenderer>(); }
    if (materials != null && materials.Length > 0) { mr.sharedMaterials = materials; } } }
  public static MergeResult MergeSelectedMeshes(UnityEngine.GameObject[] selectedObjects,UnityEngine.Transform avatarRoot)
  { if (selectedObjects == null || selectedObjects.Length == 0)
  { UnityEngine.Debug.LogWarning("[NZK] Merge Meshes: Please select one or more GameObjects with meshes.");
    return new MergeResult { Success = false,ErrorMessage = "No meshes selected." }; }
  System.Collections.Generic.List<MeshEntry> entries =  new System.Collections.Generic.List<MeshEntry>();
  foreach (UnityEngine.GameObject obj in selectedObjects)
  { UnityEngine.Mesh mesh = Meshes.GetMeshFromGameObject(obj);
    if (mesh == null) { continue; }
    entries.Add(new MeshEntry { SourceObject = obj,SourceMesh = mesh,Materials = Meshes.GetMaterialsFromGameObject(obj),CreateNewBone = false,BoneName = obj.name,BoneWeight = Meshes.Vars.Consts.DefaultNearZeroVertexWeight,GroupKey = obj.name }); }
  MergeResult result = Meshes.MergeMeshes(entries,avatarRoot);
  if (!result.Success)
  { UnityEngine.Debug.LogError("UnityEngine.Mesh merge failed: " + result.ErrorMessage);
    return result; }
  UnityEngine.Debug.Log("Successfully merged " + entries.Count + " meshes");
  return result; }
  public static System.Boolean ValidateMergeSelectedObjects() { return UnityEditor.Selection.gameObjects != null && UnityEditor.Selection.gameObjects.Length > 0; }
  public static void MergeSelectedObjectsFromSelection()
  { UnityEngine.GameObject[] selectedObjects = UnityEditor.Selection.gameObjects;
  if (selectedObjects == null || selectedObjects.Length == 0)
  { UnityEngine.Debug.LogWarning("[NZK] Merge Meshes: Please select one or more GameObjects with meshes.");
    return; }
  UnityEngine.Transform avatarRoot = UnityEditor.Selection.activeTransform != null ? UnityEditor.Selection.activeTransform.root : null;
  MergeResult result = Meshes.MergeSelectedMeshes(selectedObjects,avatarRoot);
  if (!result.Success || result.MergedMesh == null)
  { return; }
  result.MergedMesh = Meshes.SaveMergedMeshAsset(result.MergedMesh,selectedObjects);
  if (result.MergedMesh == null)
  { UnityEngine.Debug.LogError("Failed to save merged mesh asset.");
    return; }
  UnityEngine.GameObject mergedObject = new UnityEngine.GameObject("Merged UnityEngine.Mesh");
  UnityEngine.Transform mergedParent = selectedObjects.Length > 0 && selectedObjects[0] != null ? selectedObjects[0].transform.parent : null;
  if (mergedParent != null)
  { mergedObject.transform.SetParent(mergedParent,false); }
  if (result.GeneratedBones == null || result.GeneratedBones.Count == 0)
  { UnityEngine.Mesh mesh = result.MergedMesh;
    if (mesh != null)
    { System.Collections.Generic.List<UnityEngine.Transform> bones =  new System.Collections.Generic.List<UnityEngine.Transform>();
    UnityEngine.Transform rootBone = new UnityEngine.GameObject("MergedMesh_RootBone").transform;
    rootBone.SetParent(mergedObject.transform,false);
    rootBone.localPosition = UnityEngine.Vector3.zero;
    rootBone.localRotation = UnityEngine.Quaternion.identity;
    rootBone.localScale = UnityEngine.Vector3.one;
    bones.Add(rootBone);
    UnityEngine.BoneWeight[] boneWeights = new UnityEngine.BoneWeight[mesh.vertexCount];
    for (int i = 0; i < boneWeights.Length; i++)
    { boneWeights[i] = new UnityEngine.BoneWeight { boneIndex0 = 0,weight0 = 1f }; }
    mesh.boneWeights = boneWeights;
    mesh.bindposes = new UnityEngine.Matrix4x4[] { UnityEngine.Matrix4x4.identity };
    result.GeneratedBones = bones; }
  }
  UnityEngine.SkinnedMeshRenderer smr = mergedObject.AddComponent<UnityEngine.SkinnedMeshRenderer>();
  smr.sharedMesh = result.MergedMesh;
  if (result.Materials != null && result.Materials.Length > 0) { smr.sharedMaterials = result.Materials; }
  if (result.GeneratedBones != null && result.GeneratedBones.Count > 0)
  { smr.bones = result.GeneratedBones.ToArray();
    smr.rootBone = result.GeneratedBones[0]; }
  UnityEngine.GameObject originalRoot = new UnityEngine.GameObject("Original Meshes");
  originalRoot.SetActive(false);
  if (mergedParent != null)
  { originalRoot.transform.SetParent(mergedParent,false); }
  foreach (UnityEngine.GameObject obj in selectedObjects)
  { if (obj == null) { continue; }
    obj.transform.SetParent(originalRoot.transform,true); }
  foreach (UnityEngine.GameObject obj in selectedObjects)
  { if (obj == null) { continue; }
    UnityEngine.SkinnedMeshRenderer oldSmr = obj.GetComponent<UnityEngine.SkinnedMeshRenderer>();
    if (oldSmr != null)
    { IF_UE.DestroyImmediate(oldSmr);
    continue; }
    UnityEngine.MeshFilter oldMf = obj.GetComponent<UnityEngine.MeshFilter>();
    if (oldMf != null) { IF_UE.DestroyImmediate(oldMf); }
    UnityEngine.MeshRenderer oldMr = obj.GetComponent<UnityEngine.MeshRenderer>();
    if (oldMr != null) { IF_UE.DestroyImmediate(oldMr); }
  }
  UnityEditor.Selection.activeGameObject = mergedObject; }
  
}
}
}
#endif
