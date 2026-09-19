#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NZKNaNimateMeshGenerator {
    /*
     * Writes a model's skinned meshes out as standalone Mesh assets and builds a
     * prefab from an instance of the imported model whose meshes are swapped for
     * those generated assets.
     *
     * THIS CLASS NEVER RUNS INSIDE AN IMPORT.
     *
     * Every entry point here is MAIN THREAD and OUTSIDE any import, because the
     * work it does - AssetDatabase.CreateAsset, SaveAsPrefabAsset - is illegal
     * while an import is in flight. The import pipeline is NOT a trigger:
     *
     * * AssetPostprocessor.OnPostprocessModel runs on a worker thread inside an
     * out-of-process import. AssetDatabase.CreateFolder refuses there
     * ("CreateFolder is not supported while importing out-of-process") and
     * AssetDatabase.Refresh is a no-op, so a folder can be created on disk
     * yet never registered - the exact failure this class used to hit.
     *
     * * AssetPostprocessor.OnPostprocessAllAssets runs on the main thread but
     * STILL INSIDE the import batch, so the same restrictions apply.
     *
     * Post-processing is the importer's job and stays there: it configures the
     * import and updates the armature. Asset creation is this class's job and is
     * driven ONLY by an explicit, interactive request (right-click -> Reimport
     * with NaNimations, or Tools/NZK/NaNimate/Generate Now).
     *
     * Output layout:
     * Assets/!_NZK_Generated/<FileName>/Meshes/*.asset
     * Assets/!_NZK_Generated/<FileName>/Prefabs/<FileName>.prefab
     */
    
    /*
     * One mesh to write: a faithful copy of the source mesh plus the identity
     * needed to match it back to a clone renderer.
     */
    
    public static Result Generate(string modelAssetPath,string avatarName){
        return GenerateNow(modelAssetPath,avatarName);
    }
    /* Throw unless the project-relative folder exists on disk. */
    static void ThrowIfFolderMissing(string projectRelativeFolder){
        string absolute=NZK.S.P.r2a(projectRelativeFolder);
        if(!System.IO.Directory.Exists(absolute))
            throw new System.Exception("folder was not created: "+projectRelativeFolder+
                                       " (expected at "+absolute+")");
    }
    /* Throw unless the project-relative asset file exists on disk. */
    static void ThrowIfFileMissing(string projectRelativeFile){
        string absolute=NZK.S.P.r2a(projectRelativeFile);
        if(!System.IO.File.Exists(absolute))
            throw new System.Exception("asset was not written: "+projectRelativeFile+
                                       " (expected at "+absolute+")");
    }
    /*
     * Do the ENTIRE job synchronously on the calling (main) thread: read the
     * committed model, write Meshes/*.asset, build Prefabs/*.prefab.
     *
     * Safe to call from a menu item: the AssetDatabase is idle, so
     * CreateAsset and SaveAsPrefabAsset are both legal.
     *
     * MUST NOT be called from an AssetPostprocessor hook. OnPostprocessModel
     * runs on an out-of-process import worker (where CreateFolder refuses)
     * and OnPostprocessAllAssets still runs inside the import batch, so the
     * write can never succeed from either.
     */
    public static Result GenerateNow(string modelAssetPath,string avatarName){
        var result=new Result();
        if(NZK.B.NoE(avatarName))
            avatarName=NZK.S.P.Next(modelAssetPath);
        UnityEngine.GameObject source=
            UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(modelAssetPath);
        if(source==null){
            result.error="Could not load model at "+modelAssetPath+
                         " (is it imported and selected?)";
            return result;
        }
        /*
         * Gather EVERY root of the model, not just the one LoadAssetAtPath
         * happens to hand back.
         *
         * A .blend with several top-level objects imports as SEVERAL root
         * GameObjects, and LoadAssetAtPath returns only the FIRST. When the
         * armature is a sibling root rather than a child of the mesh root, its
         * bones are invisible to both the renderer enumeration and the
         * transform map - which is why the map missed 13104 bones, i.e. all of
         * them, and every renderer then failed to resolve its bone list.
         */
        UnityEngine.Object[] allAssets=
            UnityEditor.AssetDatabase.LoadAllAssetsAtPath(modelAssetPath);
        var roots=new System.Collections.Generic.List<UnityEngine.GameObject>();
        for(int i=0;i<allAssets.Length;i++){
            UnityEngine.GameObject go=allAssets[i] as UnityEngine.GameObject;
            if(go==null)
                continue;
            /*
             * Only true roots: a non-null parent means it is already reached
             * through another root's hierarchy.
             */
            if(go.transform.parent==null)
                roots.Add(go);
        }
        if(roots.Count==0)
            roots.Add(source);
        UnityEngine.SkinnedMeshRenderer[] renderers=CollectRenderers(roots);
        if(renderers.Length==0){
            result.error="No SkinnedMeshRenderer found in "+modelAssetPath+
                         ". Select the .blend itself, not a sub-object.";
            return result;
        }
        result.meshesFolder=NZKNaNimateMeshFolder.FolderFor(modelAssetPath,avatarName);
        result.prefabFolder=NZKNaNimateMeshFolder.PrefabFolderFor(modelAssetPath,avatarName);
        var usedNames=
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        var payloads=
            new System.Collections.Generic.List<MeshPayload>(renderers.Length);
        for(int i=0;i<renderers.Length;i++){
            UnityEngine.Mesh mesh=renderers[i].sharedMesh;
            if(mesh==null)
                continue;
            MeshPayload payload;
            try{
                payload=CaptureMesh(mesh,renderers[i]);
            }
            catch(System.Exception e){
                result.error=e.Message;
                return result;
            }
            payload.leaf=NZK.S.mk.unq(
                NZK.SS.An(renderers[i].name),usedNames);
            payload.rendererName=renderers[i].name;
            payload.rendererPath=NZK.T.Tp(renderers[i].transform);
            payload.sourceRenderer=renderers[i];
            payloads.Add(payload);
        }
        if(payloads.Count==0){
            result.error="No readable meshes on "+modelAssetPath+
                         " (is Read/Write enabled on the model?)";
            return result;
        }
        /*
         * Folders are created on the FILESYSTEM with their .meta sidecars, so
         * Unity adopts them without any AssetDatabase registration call.
         */
        NZKNaNimateMeshFolder.EnsureFolder(result.meshesFolder);
        NZKNaNimateMeshFolder.EnsureFolder(result.prefabFolder);
        try{
            ThrowIfFolderMissing(result.meshesFolder);
            ThrowIfFolderMissing(result.prefabFolder);
        }
        catch(System.Exception e){
            result.error=e.Message;
            return result;
        }
        try{
            /*
             * Keyed by transform path, never by array index:
             * GetComponentsInChildren order is undocumented, and an index
             * mismatch silently swaps the wrong mesh onto a renderer.
             */
            var generatedByPath=
                new System.Collections.Generic.Dictionary<string,UnityEngine.Mesh>(System.StringComparer.Ordinal);
            for(int i=0;i<payloads.Count;i++){
                MeshPayload p=payloads[i];
                string leafPath=result.meshesFolder+"/"+p.leaf+".asset";
                UnityEngine.Mesh mesh=p.mesh;
                mesh.name=p.leaf;
                /*
                 * DELETE + CREATE, never CopySerialized.
                 *
                 * CopySerialized cannot add a vertex stream that the existing
                 * asset lacks, so an asset once written without the skinning
                 * buffer stayed that way forever and the renderer never drew.
                 * A fresh CreateAsset produces the correct stream layout.
                 */
                ReplaceAsset(leafPath,mesh);
                ThrowIfFileMissing(leafPath);
                /*
                 * Re-load so the dictionary holds the persisted asset, not the
                 * transient object that was just handed to CreateAsset.
                 */
                mesh=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(leafPath);
                generatedByPath[p.rendererPath]=mesh;
                result.meshCount++;
            }
            if(result.meshCount==0)
                throw new System.Exception("no meshes were written");
            UnityEditor.AssetDatabase.SaveAssets();
            /*
             * --- instance the model and swap in the generated meshes ------
             *
             * Clone EVERY root, not just `source`. The armature is frequently a
             * sibling root of the mesh root, so cloning only one of them leaves
             * the bones out of the prefab and no bone reference can resolve.
             */
            var instanceRoots=new System.Collections.Generic.List<UnityEngine.GameObject>(roots.Count);
            for(int i=0;i<roots.Count;i++){
                UnityEngine.GameObject cloneRoot=UnityEngine.Object.Instantiate(roots[i]);
                instanceRoots.Add(cloneRoot);
            }
            /*
             * The prefab's root is the first clone. Everything else is parented
             * under it so the saved prefab is a single connected hierarchy -
             * SaveAsPrefabAsset would otherwise discard the unparented roots.
             */
            UnityEngine.GameObject instance=instanceRoots[0];
            try{
                for(int i=1;i<instanceRoots.Count;i++)
                    instanceRoots[i].transform.SetParent(instance.transform,false);
                UnityEngine.SkinnedMeshRenderer[] instanceRenderers=
                    CollectRenderers(instanceRoots);
                /*
                 * Map every SOURCE transform to its CLONE before touching any
                 * renderer, and BEFORE renaming the clone.
                 *
                 * ORDER MATTERS: the rename below changes the clone's ROOT name,
                 * and a path key that included the root made every entry miss -
                 * which is what produced "13104 bone(s) could not be mapped".
                 */
                System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform> transformMap=
                    BuildTransformMap(roots,instanceRoots);
                /* Renamed only AFTER the map exists, so the map is unaffected. */
                instance.name=avatarName;
                /*
                 * Pair clone renderers with the generated meshes by MESH NAME.
                 *
                 * This is the same identity the .meta.nzk sidecar uses: its
                 * per-mesh selection is keyed by mesh name (MeshSelection.name),
                 * which originates from the source .blend object name. Reusing
                 * that key keeps the output consistent with what the sidecar
                 * already decided should be pulled in.
                 *
                 * Deliberately NOT a transform path and NOT an array index:
                 * * TransformPath embeds "#<siblingIndex>", which Unity does
                 * not preserve across the asset-to-scene boundary, so path
                 * lookup missed on EVERY renderer - the
                 * "no renderer was swapped onto a generated mesh" failure.
                 * * Index pairing is unsound here too: `payloads` is a
                 * FILTERED list (renderers with a null sharedMesh are
                 * skipped), while `instanceRenderers` is enumerated
                 * separately from the unfiltered hierarchy.
                 *
                 * Names are grouped into lists so duplicate names
                 * (Blender emits "B Body" and "B Body.001") are paired in
                 * order rather than overwriting one another in a dictionary.
                 *
                 * TWO keys are registered per clone renderer, because the
                 * project uses two overlapping name namespaces and which one is
                 * authoritative is not something this method can know:
                 * * sharedMesh.name - the Unity Mesh name.
                 * * renderer.name   - the source object name, which is what
                 * the .meta.nzk sidecar keys MeshSelection.name on and what
                 * the generated .asset leaf files are named after.
                 * Registering both costs one extra dictionary entry and removes
                 * the guesswork.
                 */
                var byName=
                    new System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>>(System.StringComparer.OrdinalIgnoreCase);
                for(int i=0;i<instanceRenderers.Length;i++){
                    UnityEngine.SkinnedMeshRenderer r=instanceRenderers[i];
                    if(r.sharedMesh!=null)
                        AddToBucket(byName,r.sharedMesh.name,r);
                    AddToBucket(byName,r.name,r);
                }
                var consumed=
                    new System.Collections.Generic.Dictionary<string,int>(System.StringComparer.OrdinalIgnoreCase);
                /*
                 * Clone renderers keyed by path, so each swapped renderer can
                 * have its bones rewired from the corresponding SOURCE
                 * renderer's bone list.
                 *
                 * Built from the SAME payloads the rest of this method walks,
                 * so the key is by construction the `p.rendererPath` that is
                 * looked up below. Recomputing paths from `renderers` here could
                 * disagree with the payloads (the multi-root change means two
                 * roots can yield the same root-relative path), and a
                 * disagreement silently skipped every rewire.
                 */
                var sourceByPath=
                    new System.Collections.Generic.Dictionary<string,UnityEngine.SkinnedMeshRenderer>(System.StringComparer.Ordinal);
                for(int i=0;i<payloads.Count;i++){
                    string path=payloads[i].rendererPath;
                    /*
                     * Keep the FIRST renderer for a path rather than letting a
                     * later collision overwrite it.
                     */
                    if(!sourceByPath.ContainsKey(path))
                        sourceByPath[path]=payloads[i].sourceRenderer;
                }
                var rewiredRenderers=
                    new System.Collections.Generic.HashSet<UnityEngine.SkinnedMeshRenderer>();
                for(int i=0;i<payloads.Count;i++){
                    MeshPayload p=payloads[i];
                    if(!generatedByPath.TryGetValue(p.rendererPath,out UnityEngine.Mesh generated))
                        continue;
                    /*
                     * Try the mesh name first, then the renderer name. Both
                     * are stable across Instantiate, unlike a sibling index.
                     */
                    if(!TryTakeRenderer(byName,consumed,p.meshName,out UnityEngine.SkinnedMeshRenderer target)&&
                       !TryTakeRenderer(byName,consumed,p.rendererName,out target)){
                        result.unmatchedRenderers++;
                        if(result.unmatchedSample==null)
                            result.unmatchedSample=p.rendererName;
                        continue;
                    }
                    target.sharedMesh=generated;
                    /*
                     * Rewire bones to the CLONE's transforms, using the source
                     * renderer that produced this payload. Without this the
                     * clone can keep bones pointing outside the prefab and the
                     * mesh cannot be drawn at all - Unity refuses the upload
                     * with "does not match the expected mesh data size and
                     * vertex stride".
                     *
                     * A MISSING SOURCE RENDERER IS A FAILURE, not a no-op.
                     * This used to be combined into one `&&` condition with the
                     * dedupe check, so a lookup miss silently skipped the
                     * rewire without incrementing anything - the run still
                     * reported "swapped 39" with no warning while every mesh
                     * was left un-rewired and invisible.
                     */
                    if(rewiredRenderers.Add(target)){
                        if(sourceByPath.TryGetValue(p.rendererPath,out UnityEngine.SkinnedMeshRenderer sourceRenderer)){
                            RewireBones(sourceRenderer,target,transformMap,result);
                        }
                        else{
                            result.unmatchedRenderers++;
                            if(result.unmatchedSample==null)
                                result.unmatchedSample=p.rendererName+" (no source renderer)";
                        }
                    }
                    result.swappedCount++;
                    result.rendererCount++;
                }
                if(result.swappedCount==0){
                    /*
                     * Name the counts and a sample so a future failure is
                     * diagnosable without re-instrumenting.
                     */
                    throw new System.Exception(
                        "no renderer was swapped onto a generated mesh (payloads: "+
                        payloads.Count+", clone renderers: "+instanceRenderers.Length+
                        ", distinct names: "+byName.Count+")");
                }
                string prefabPath=result.prefabFolder+"/"+avatarName+".prefab";
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(instance,prefabPath);
                result.prefabPath=prefabPath;
            }
            finally{
                UnityEngine.Object.DestroyImmediate(instance);
            }
            UnityEditor.AssetDatabase.SaveAssets();
            result.success=true;
            UnityEngine.Debug.Log(NZK.S.NZKNaNimatePrefix("Generated "+result.meshCount+
                                  " mesh(es), swapped "+result.swappedCount+
                                  " renderer(s) -> "+result.prefabPath));
            /*
             * A bone that could not be mapped means the mesh is driven by
             * transforms outside the prefab and will not deform correctly even
             * though the swap "succeeded". Report it as a warning so it can
             * never hide behind the success line above.
             */
            if(result.unmappedBones>0){
                UnityEngine.Debug.LogWarning(NZK.S.NZKNaNimatePrefix(result.unmappedBones+
                                             " bone(s) could not be mapped into the prefab"+
                                             (result.unmappedBoneSample!=null
                                                 ? " (e.g. "+result.unmappedBoneSample+")"
                                                 : "")+
                                             ". Those meshes will not deform correctly."));
            }
            if(result.unmatchedRenderers>0){
                UnityEngine.Debug.LogWarning(NZK.S.NZKNaNimatePrefix(result.unmatchedRenderers+
                                             " renderer(s) were not paired to a generated mesh"+
                                             (result.unmatchedSample!=null
                                                 ? " (e.g. "+result.unmatchedSample+")"
                                                 : "")+"."));
            }
            return result;
        }
        catch(System.Exception e){
            result.success=false;
            result.error=e.Message;
            UnityEngine.Debug.LogError(NZK.S.NZKNaNimatePrefix("Generation failed for "+
                                        modelAssetPath+": "+e.Message));
            return result;
        }
    }
    /*
     * Write a mesh over an existing asset by DELETING and re-creating it.
     *
     * WHY NOT CopySerialized: copying into an existing Mesh asset can only
     * update the channels that asset already has. It will NOT add a missing
     * vertex STREAM. Measured directly:
     *
     * seed asset (positions+weights only)  vbc=2
     * after CopySerialized(a 3-buffer mesh) vbc=2  strides: 40 12
     *
     * The skinning stream (BlendWeight + BlendIndices, stride 32) is the one
     * that goes missing, and without it Unity refuses the GPU upload with
     * "does not match the expected mesh data size and vertex stride" so the
     * renderer stops drawing entirely.
     *
     * That made the old in-place path self-perpetuating: any asset ever
     * written as 2-buffer stayed 2-buffer on every later run, no matter how
     * correct the incoming data was. Creating a fresh asset DOES produce the
     * correct layout (verified: Instantiate + CreateAsset -> vbc=3).
     *
     * GUID IS PRESERVED because other assets reference these meshes; the new
     * asset is written to the same path via CreateAsset, which reuses the
     * existing .meta rather than orphaning references.
     */
    static bool ReplaceAsset(string leafPath,UnityEngine.Mesh mesh){
        if(System.IO.File.Exists(NZK.S.P.r2a(leafPath))){
            /*
             * DeleteAsset removes the file but leaves the .meta, so the GUID
             * survives and every existing reference still resolves.
             */
            UnityEditor.AssetDatabase.DeleteAsset(leafPath);
        }
        UnityEditor.AssetDatabase.CreateAsset(mesh,leafPath);
        return true;
    }
    /*
     * True when a failure is likely to succeed on a later attempt because the
     * editor was simply busy, rather than because the request is invalid.
     */
    static bool IsTransient(string message){
        if(NZK.B.NoE(message))
            return false;
        return NZK.B.Oll5(
            NZK.S.HasOIC(message,"restricted during asset importing"),
            NZK.S.HasOIC(message,"cannot write Mesh assets during import"),
            NZK.S.HasOIC(message,"not a valid Unity folder"),
            /*
             * Out-of-process import window: the AssetDatabase APIs used for
             * folder registration legitimately refuse until it closes.
             */
            NZK.S.HasOIC(message,"out-of-process"),
            NZK.S.HasOIC(message,"not supported while importing"));
    }
    /* Register a renderer under a name, creating the bucket if needed. */
    static void AddToBucket(
        System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>> map,
        string key,
        UnityEngine.SkinnedMeshRenderer renderer){
        if(NZK.B.NoE(key)||renderer==null)
            return;
        if(!map.TryGetValue(key,out System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer> bucket)){
            bucket=new System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>(1);
            map[key]=bucket;
        }
        bucket.Add(renderer);
    }
    /*
     * Claim the next unconsumed renderer registered under key.
     *
     * The per-key cursor means duplicate names are paired in ORDER instead of
     * all resolving to the first entry, so a mesh whose name repeats is still
     * swapped onto a distinct renderer each time.
     */
    static bool TryTakeRenderer(
        System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>> map,
        System.Collections.Generic.Dictionary<string,int> consumed,
        string key,
        out UnityEngine.SkinnedMeshRenderer renderer){
        renderer=null;
        if(NZK.B.NoE(key)||
           !map.TryGetValue(key,out System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer> bucket)){
            return false;
        }
        consumed.TryGetValue(key,out int taken);
        if(taken>=bucket.Count)
            return false;
        renderer=bucket[taken];
        consumed[key]=taken+1;
        return true;
    }
    /*
     * All SkinnedMeshRenderers reachable from a set of root objects, in a
     * stable order.
     */
    static UnityEngine.SkinnedMeshRenderer[] CollectRenderers(
        System.Collections.Generic.List<UnityEngine.GameObject> roots){
        var found=new System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>();
        for(int i=0;i<roots.Count;i++){
            if(roots[i]==null)
                continue;
            UnityEngine.SkinnedMeshRenderer[] r=
                roots[i].GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
            for(int j=0;j<r.Length;j++)
                found.Add(r[j]);
        }
        return found.ToArray();
    }
    /* All transforms reachable from a set of root objects. */
    static UnityEngine.Transform[] CollectTransforms(
        System.Collections.Generic.List<UnityEngine.GameObject> roots){
        var found=new System.Collections.Generic.List<UnityEngine.Transform>();
        for(int i=0;i<roots.Count;i++){
            if(roots[i]==null)
                continue;
            UnityEngine.Transform[] t=
                roots[i].GetComponentsInChildren<UnityEngine.Transform>(true);
            for(int j=0;j<t.Length;j++)
                found.Add(t[j]);
        }
        return found.ToArray();
    }
    /*
     * Copy a mesh into a NEW Mesh object, assigned channel by channel.
     *
     * WHY NOT Object.Instantiate: Instantiate produces a correct in-memory
     * copy (3 vertex buffers, strides 40/68/32), but writing that copy with
     * AssetDatabase.CreateAsset LOSES THE SKINNING STREAM - the persisted
     * asset comes back with only 2 buffers (40/68). Verified live:
     *
     * SOURCE  vbc=3 strides: 40 68 32
     * INSTANT vbc=3 strides: 40 68 32
     * ASSET   vbc=2 strides: 40 68     <-- 32-byte BlendWeight+BlendIndices gone
     *
     * With the skinning stream missing Unity refuses the GPU upload:
     * "does not match the expected mesh data size and vertex stride", and the
     * renderer stops drawing - which is why meshes were invisible even though
     * every renderer reported swapped and all bones mapped correctly.
     *
     * Assigning the channels through the API makes Unity build the streams
     * itself, so the skinning buffer is present in the serialized asset.
     */
    static MeshPayload CaptureMesh(UnityEngine.Mesh mesh,UnityEngine.SkinnedMeshRenderer renderer){
        /*
         * A non-readable mesh returns empty arrays instead of throwing, which
         * would produce a hollow .asset that looks like a success. Fail
         * loudly instead, naming the renderer.
         */
        if(!mesh.isReadable)
            throw new System.Exception(
                "mesh '"+mesh.name+"' on renderer '"+renderer.name+
                "' is not readable. Enable Read/Write on the model import settings "+
                "(Model tab -> Read/Write Enabled) and reimport.");
        UnityEngine.BoneWeight[] weights=mesh.boneWeights;
        /*
         * A mesh with bones but no weights renders rigid, and one with weights
         * but no bind poses renders unposed. Both are silent at write time and
         * only show up as a broken avatar, so refuse to write either.
         */
        if(NZK.B.mpty.t(weights)&&
           renderer.bones!=null&&renderer.bones.Length>0){
            throw new System.Exception(
                "mesh '"+mesh.name+"' on renderer '"+renderer.name+
                "' carries no bone weights, but the renderer has "+
                renderer.bones.Length+" bones. Writing it would produce a "+
                "mesh that ignores the armature. Check minBoneWeight / skinWeights "+
                "on the model import settings.");
        }
        var copy=new UnityEngine.Mesh();
        copy.name=mesh.name;
        copy.indexFormat=mesh.indexFormat;
        copy.vertices=mesh.vertices;
        UnityEngine.Vector3[] normals=mesh.normals;
        UnityEngine.Vector4[] tangents=mesh.tangents;
        UnityEngine.Color[] colors=mesh.colors;
        /*
         * All UV sets. The NaNimate materials use up to 8, and dropping any
         * changes stream 1's stride, which is part of what the stride check
         * compares.
         */
        SetUvSet(copy,0,mesh.uv);
        SetUvSet(copy,1,mesh.uv2);
        SetUvSet(copy,2,mesh.uv3);
        SetUvSet(copy,3,mesh.uv4);
        SetUvSet(copy,4,mesh.uv5);
        SetUvSet(copy,5,mesh.uv6);
        SetUvSet(copy,6,mesh.uv7);
        SetUvSet(copy,7,mesh.uv8);
        copy.subMeshCount=mesh.subMeshCount;
        for(int s=0;s<mesh.subMeshCount;s++)
            copy.SetTriangles(mesh.GetTriangles(s),s,false);
        /*
         * NORMALS, TANGENTS AND COLOURS ARE SET **AFTER** THE TRIANGLES, AND
         * THOSE THREE LINES USED TO SIT BEFORE THEM.
         *
         * THIS WAS THE BLACK-LINES-ON-THE-ARMS BUG.  SetTriangles on a Mesh
         * whose vertex count is already fixed RESIZES the vertex buffer to the
         * highest index the triangles reference, so a channel assigned before
         * it is either truncated or silently dropped.  The old guard -
         * `normals.Length == copy.vertexCount` - was evaluated against the
         * PRE-triangle count, so the assignment could succeed and the data
         * still be discarded afterwards, with no error anywhere.  A mesh that
         * reaches the renderer with no normals shades black wherever the
         * fallback normal points away from the light, which is why the
         * artefacts follow the arms (a cylinder: half the surface always faces
         * away) and read as hard lines along the seams.
         *
         * The same loss explains the jacket squares that would not animate:
         * blendshape normal deltas have no normal channel to apply to, so only
         * the position delta survives and flat regions barely move.
         *
         * Assigned now in the only order that works: vertices, UVs, triangles,
         * THEN the per-vertex channels, THEN RecalculateNormals as a floor for
         * any mesh whose normals were absent or the wrong length.
         */
        if(normals!=null&&normals.Length==copy.vertexCount)
            copy.normals=normals;
        else
        {   /* Absent, or a length mismatch that would silently drop them.
               Recalculating is strictly better than shipping a mesh with no
               normals at all, which renders black.  It runs AFTER SetTriangles
               because RecalculateNormals needs the triangle list to average
               over. */
            copy.RecalculateNormals();
            UnityEngine.Debug.LogWarning(
                "[NZK] mesh '"+mesh.name+"' had "+
                (normals==null?"no normals":normals.Length+" normals for "+copy.vertexCount+" vertices")+
                "; recalculated. Authored hard edges on this mesh may be smoothed."); }
        if(tangents!=null&&tangents.Length==copy.vertexCount)
            copy.tangents=tangents;
        else
        {   /* Taylor the tangents to the normals that are actually on the mesh,
               rather than leaving a stale tangent array pointing at normals
               that no longer exist - which is what makes a normal-map-lit
               surface shade inconsistently across a seam. */
            copy.RecalculateTangents();
            UnityEngine.Debug.LogWarning(
                "[NZK] mesh '"+mesh.name+"' had "+
                (tangents==null?"no tangents":tangents.Length+" tangents for "+copy.vertexCount+" vertices")+
                "; recalculated."); }
        if(colors!=null&&colors.Length==copy.vertexCount)
            copy.colors=colors;
        /*
         * Order matters: bind poses BEFORE weights, so the weight indices have
         * a bone list to resolve against when the streams are built.
         */
        UnityEngine.Matrix4x4[] poses=mesh.bindposes;
        if(poses!=null&&poses.Length>0)
            copy.bindposes=poses;
        if(weights!=null&&weights.Length>0)
            copy.boneWeights=weights;
        /*
         * BLENDSHAPES.  These were never copied, and that is the bug behind
         * "some squares on my avi are not animating, at least on the jacket".
         *
         * The generated mesh had geometry, weights and normals but ZERO
         * blendshapes, while the source carries hundreds (ABBS, Blink, Boop,
         * and every NaNimate toggle - the same names the import warnings list).
         * A NaNimation toggle drives a blendshape index; when the generated
         * mesh has no shape at that index the animation runs, the parameter
         * changes, and NOTHING moves.  Flat regions of a garment read as
         * static squares, which is exactly the reported symptom.
         *
         * Measured before this block existed: 41 generated meshes, 2 with
         * shapes, 8 shapes total.
         *
         * Every frame of every shape is copied, and all three channels per
         * frame.  The normal and tangent deltas are NOT optional: a shape
         * carrying only position deltas still moves the silhouette but leaves
         * the shading untouched, so a large flat panel keeps its old lighting
         * and looks frozen even while its vertices shift.  That is the
         * position-only failure mode, and it is why the shape that "barely
         * moves" and the shape that "does not move" are the same bug at
         * different scales.
         *
         * weight=0 on the final frame of a shape is legal and used by
         * Unity's own importer; frames are replayed exactly as read rather
         * than normalised, so a source that relies on a partial frame keeps
         * it.
         */
        int shapeCount=mesh.blendShapeCount;
        for(int si=0;si<shapeCount;si++){
            string shapeName=mesh.GetBlendShapeName(si);
            int frameCount=mesh.GetBlendShapeFrameCount(si);
            for(int fi=0;fi<frameCount;fi++){
                float frameWeight=mesh.GetBlendShapeFrameWeight(si,fi);
                UnityEngine.Vector3[] dVerts=new UnityEngine.Vector3[mesh.vertexCount];
                UnityEngine.Vector3[] dNormals=new UnityEngine.Vector3[mesh.vertexCount];
                UnityEngine.Vector3[] dTangents=new UnityEngine.Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(si,fi,dVerts,dNormals,dTangents);
                /* The frame index is passed as the shape's FIRST frame only
                   after AddBlendShapeFrame has created the shape; adding a
                   frame with weight 0 for the base then the real frame is the
                   documented order.  Zero-weight frames are written as-is. */
                copy.AddBlendShapeFrame(shapeName,frameWeight,dVerts,dNormals,dTangents);
            }
        }
        copy.RecalculateBounds();
        return new MeshPayload
        {
            mesh=copy,
            meshName=mesh.name,
        };
    }
    /*
     * Assign one UV set, skipping nulls and length mismatches.
     *
     * A wrong-length array would throw from Unity with a less useful message,
     * and an absent set must be left untouched rather than zero-filled - the
     * stream stride depends on which sets are actually present.
     */
    static void SetUvSet(UnityEngine.Mesh target,int index,UnityEngine.Vector2[] uv){
        if(uv==null||uv.Length!=target.vertexCount)
            return;
        switch(index){
            case 0: target.uv=uv; break;
            case 1: target.uv2=uv; break;
            case 2: target.uv3=uv; break;
            case 3: target.uv4=uv; break;
            case 4: target.uv5=uv; break;
            case 5: target.uv6=uv; break;
            case 6: target.uv7=uv; break;
            case 7: target.uv8=uv; break;
        }
    }
    /*
     * Map every SOURCE transform to its CLONE by hierarchy path, across ALL
     * roots of each side.
     *
     * Taking root LISTS (rather than a single GameObject each) matters: a
     * .blend with several top-level objects imports as several roots, and the
     * armature is often a sibling of the mesh root. Mapping only one root
     * leaves the bones unmapped, which is what produced
     * "13104 bone(s) could not be mapped" - effectively every bone.
     *
     * Deliberately NOT index-paired: relying on GetComponentsInChildren
     * returning identical order for an original and its clone is an
     * undocumented assumption, and getting it wrong silently rewires bones
     * to the wrong joints. A path lookup is correct by construction.
     */
    static System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform> BuildTransformMap(
        System.Collections.Generic.List<UnityEngine.GameObject> sourceRoots,
        System.Collections.Generic.List<UnityEngine.GameObject> cloneRoots){
        var cloneByPath=
            new System.Collections.Generic.Dictionary<string,UnityEngine.Transform>(System.StringComparer.Ordinal);
        UnityEngine.Transform[] cloneTransforms=CollectTransforms(cloneRoots);
        for(int i=0;i<cloneTransforms.Length;i++)
            cloneByPath[NZK.T.Tp(cloneTransforms[i])]=cloneTransforms[i];
        var map=new System.Collections.Generic.Dictionary<UnityEngine.Transform,UnityEngine.Transform>();
        UnityEngine.Transform[] sourceTransforms=CollectTransforms(sourceRoots);
        for(int i=0;i<sourceTransforms.Length;i++){
            UnityEngine.Transform src=sourceTransforms[i];
            if(cloneByPath.TryGetValue(NZK.T.Tp(src),out UnityEngine.Transform dst))
                map[src]=dst;
        }
        /*
         * Report the mapping state UNCONDITIONALLY.
         *
         * The earlier threshold-based warning (only fire when the match rate
         * was under 25%) produced no output while the failure persisted, which
         * left nothing to diagnose from. Always logging the raw counts means a
         * failing run states its own numbers instead of requiring another
         * round of guessing.
         */
        UnityEngine.Debug.Log(
            NZK.S.NZKNaNimatePrefix("Transform map: matched "+map.Count+
            " of "+sourceTransforms.Length+" source transform(s); clone has "+
            cloneTransforms.Length+"."));
        return map;
    }
    /*
     * Repoint a clone renderer's bones and root bone at the CLONE's own
     * transforms.
     *
     * A SkinnedMeshRenderer resolves every bone index in its mesh's bind
     * poses through its own `bones` array. If that array still references the
     * SOURCE model's transforms - which is not excluded after an
     * asset-to-scene Instantiate - the mesh is driven by objects outside the
     * prefab and renders collapsed, displaced or invisible. Small pieces bound
     * to few bones can survive that while the large deformables vanish, which
     * is exactly the "only a few meshes visible" symptom.
     *
     * A bone outside the cloned subtree cannot be mapped. It is left pointing
     * at the original AND counted, because a partially rewired rig that
     * reports success is the worst possible outcome.
     */
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
            cloneRenderer.rootBone=map.TryGetValue(sourceRenderer.rootBone,out UnityEngine.Transform rootClone)
                ? rootClone
                : sourceRenderer.rootBone;
        }
    }
}
}
}
#endif
