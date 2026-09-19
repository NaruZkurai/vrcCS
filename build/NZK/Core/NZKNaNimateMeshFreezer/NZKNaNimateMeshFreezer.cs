#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NZKNaNimateMeshFreezer {
    /*
     * Copies an imported model into a frozen, self-contained set of UnityEngine.Mesh assets
     * plus a prefab whose hierarchy is fully duplicated.
     *
     * Why duplicate the hierarchy: the whole point is to stop Unity's
     * UnityEditor.ModelImporter from rewriting the rig on every reimport. A prefab that
     * still points at the importer's Transforms is still hostage to the
     * importer, so we clone every UnityEngine.Transform and rewire all bones/rootBone to
     * the clones. Nothing in the frozen prefab references the source model.
     *
     * UnityEngine.Mesh data is copied verbatim - vertices, normals, tangents, UVs, colours,
     * bone weights (all influences), bindposes and blend shapes. No
     * reconstruction, so there is nothing to get subtly wrong.
     */
    
    /*
     * Freeze a model's skinned meshes into standalone assets and a prefab.
     *
     * modelAssetPath: Project-relative source model path.
     * avatarName: Avatar folder name; defaults to the model name.
     */
    public static Result Freeze(string modelAssetPath,string avatarName){
        var result=new Result();
        UnityEngine.GameObject source=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(modelAssetPath);
        if(source==null){
            result.error="Could not load model at "+modelAssetPath;
            return result;
        }
        UnityEngine.SkinnedMeshRenderer[] sourceRenderers=
            source.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
        if(sourceRenderers.Length==0){
            result.error="No UnityEngine.SkinnedMeshRenderer found in "+modelAssetPath;
            return result;
        }
        string folder=NZKNaNimateMeshFolder.FolderFor(modelAssetPath,avatarName);
        NZKNaNimateMeshFolder.EnsureFolder(folder);
        result.folder=folder;
        /* Declared outside try so the finally block can always clean up. */
        UnityEngine.GameObject clone=null;
        bool assetsEditing=false;
        try{
            UnityEditor.AssetDatabase.StartAssetEditing();
            assetsEditing=true;
            /* Clone the whole hierarchy so the frozen prefab owns its bones. */
            clone=UnityEngine.Object.Instantiate(source);
            clone.name=source.name+" (Frozen)";
            /*
             * Map every source UnityEngine.Transform to its clone so bone references can
             * be rewritten without relying on name matching.
             */
            System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform> transformMap=BuildTransformMap(source,clone);
            UnityEngine.SkinnedMeshRenderer[] cloneRenderers=
                clone.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
            /*
             * Pair source renderer -> clone renderer by HIERARCHY PATH, not by
             * array index. GetComponentsInChildren order is undocumented and
             * a mismatch silently writes the wrong mesh (or none at all, when
             * the mismatched source slot has no sharedMesh) into the clone,
             * which is how the prefab ended up with empty renderers and the
             * Meshes folder stayed empty. Same reasoning as BuildTransformMap.
             */
            var cloneRendererByPath=
                new System.Collections.Generic.Dictionary<string,UnityEngine.SkinnedMeshRenderer>(System.StringComparer.Ordinal);
            for(int i=0;i<cloneRenderers.Length;i++)
                cloneRendererByPath[NZK.T.Tpr(cloneRenderers[i].transform)]=cloneRenderers[i];
            /*
             * Meshes are cached by source mesh so shared meshes are not
             * duplicated once per renderer that uses them.
             */
            var frozenBySource=new System.Collections.Generic.Dictionary<UnityEngine.Mesh,UnityEngine.Mesh>();
            var usedNames=new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for(int i=0;i<sourceRenderers.Length;i++){
                UnityEngine.SkinnedMeshRenderer sourceRenderer=sourceRenderers[i];
                if(!cloneRendererByPath.TryGetValue(NZK.T.Tpr(sourceRenderer.transform),out UnityEngine.SkinnedMeshRenderer cloneRenderer)){
                    result.unmappedRenderers++;
                    if(result.unmappedRendererSample==null)
                        result.unmappedRendererSample=sourceRenderer.name;
                    continue;
                }
                UnityEngine.Mesh sourceMesh=sourceRenderer.sharedMesh;
                if(sourceMesh==null)
                    continue;
                if(!frozenBySource.TryGetValue(sourceMesh,out UnityEngine.Mesh frozen)){
                    string leafName=NZK.S.mk.unq(
                        NZK.SS.An(sourceRenderer.name),
                        usedNames);
                    frozen=FreezeMesh(sourceMesh,leafName);
                    if(frozen==null)
                        continue;
                    string leafPath=folder+"/"+leafName+".asset";
                    UnityEditor.AssetDatabase.CreateAsset(frozen,leafPath);
                    frozenBySource[sourceMesh]=frozen;
                    result.meshCount++;
                    result.approxBytes+=NZK.U.Bfll2.VC(sourceMesh);
                }
                cloneRenderer.sharedMesh=frozen;
                cloneRenderer.materials=sourceRenderer.sharedMaterials;
                cloneRenderer.localBounds=sourceRenderer.localBounds;
                cloneRenderer.updateWhenOffscreen=sourceRenderer.updateWhenOffscreen;
                RewireBones(sourceRenderer,cloneRenderer,transformMap,result);
                result.rendererCount++;
            }
            UnityEditor.AssetDatabase.StopAssetEditing();
            assetsEditing=false;
            /*
             * Nothing was written: report it as a failure rather than saving
             * an empty prefab into the project.
             */
            if(result.meshCount==0||result.rendererCount==0){
                UnityEngine.Object.DestroyImmediate(clone);
                clone=null;
                result.error=
                    "Freeze produced no meshes ("+result.meshCount+" mesh(es), "+
                    result.rendererCount+" renderer(s)) from "+modelAssetPath+
                    ". Source renderers: "+sourceRenderers.Length+
                    ", unmatched: "+result.unmappedRenderers+
                    (result.unmappedRendererSample!=null?" (e.g. "+result.unmappedRendererSample+")":"")+".";
                return result;
            }
            string prefabPath=NZKNaNimateMeshFolder.PrefabPath(folder,avatarName);
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(clone,prefabPath);
            UnityEngine.Object.DestroyImmediate(clone);
            clone=null;
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            result.prefabPath=prefabPath;
            result.success=true;
            return result;
        }
        catch(System.Exception e){
            result.error=e.Message;
            return result;
        }
        finally{
            /*
             * StopAssetEditing must always run. Leaving the AssetDatabase in
             * an editing block makes later imports appear to do nothing.
             */
            if(assetsEditing)
                UnityEditor.AssetDatabase.StopAssetEditing();
            /* The success path already destroyed the clone before saving. */
            if(clone!=null)
                UnityEngine.Object.DestroyImmediate(clone);
        }
    }
    /*
     * Map every source UnityEngine.Transform to its clone by hierarchy path.
     *
     * Deliberately NOT index-paired: relying on GetComponentsInChildren
     * returning identical order for an original and its clone is an
     * undocumented assumption, and getting it wrong silently rewires bones
     * to the wrong joints. System.IO.Path lookup is correct by construction.
     */
    static System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform> BuildTransformMap(UnityEngine.GameObject source,UnityEngine.GameObject clone){
        var cloneByPath=new System.Collections.Generic.Dictionary<string,UnityEngine.Transform>(System.StringComparer.Ordinal);
        UnityEngine.Transform[] cloneTransforms=clone.GetComponentsInChildren<UnityEngine.Transform>(true);
        for(int i=0;i<cloneTransforms.Length;i++)
            cloneByPath[NZK.T.Tpr(cloneTransforms[i])]=cloneTransforms[i];
        var map=new System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform>();
        UnityEngine.Transform[] sourceTransforms=source.GetComponentsInChildren<UnityEngine.Transform>(true);
        for(int i=0;i<sourceTransforms.Length;i++){
            UnityEngine.Transform src=sourceTransforms[i];
            if(cloneByPath.TryGetValue(NZK.T.Tpr(src),out UnityEngine.Transform dst))
                map[src]=dst;
        }
        return map;
    }
    static void RewireBones(
        UnityEngine.SkinnedMeshRenderer sourceRenderer,
        UnityEngine.SkinnedMeshRenderer cloneRenderer,
        System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform> map,
        Result result){
        UnityEngine.Transform[] bones=sourceRenderer.bones;
        var rewired=new UnityEngine.Transform[bones.Length];
        for(int i=0;i<bones.Length;i++){
            UnityEngine.Transform bone=bones[i];
            if(bone==null){
                rewired[i]=null;
                continue;
            }
            /*
             * A bone outside the cloned subtree cannot be mapped. Left
             * pointing at the original AND counted, because a partially
             * rewired rig that reports success is the worst outcome.
             */
            if(map.TryGetValue(bone,out UnityEngine.Transform cloned)){
                rewired[i]=cloned;
            }
            else{
                rewired[i]=bone;
                result.unmappedBones++;
                if(result.unmappedBoneSample==null)
                    result.unmappedBoneSample=bone.name;
            }
        }
        cloneRenderer.bones=rewired;
        if(sourceRenderer.rootBone!=null){
            if(map.TryGetValue(sourceRenderer.rootBone,out UnityEngine.Transform rootClone))
                cloneRenderer.rootBone=rootClone;
            else
                cloneRenderer.rootBone=sourceRenderer.rootBone;
        }
    }
    /*
     * Copy a mesh verbatim. Uses Instantiate so every channel Unity knows
     * about is carried over without enumerating them by hand.
     */
    static UnityEngine.Mesh FreezeMesh(UnityEngine.Mesh sourceMesh,string name){
        UnityEngine.Mesh copy=UnityEngine.Object.Instantiate(sourceMesh);
        copy.name=name;
        /*
         * Keep the full influence count; a NaNimate group is by definition
         * made of sub-threshold weights that a reduced format would discard.
         */
        copy.indexFormat=sourceMesh.indexFormat;
        return copy;
    }
  
}
}
}
#endif
