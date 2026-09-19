namespace NZK.NaNimate
{
public static class NZKNaNimateDiagnostics{
    /*
     * Read-only diagnostics for a generated prefab.
     *
     * WHY THIS EXISTS: the generated prefab is a YAML asset, and reading it with
     * text tools led to a run of wrong conclusions - a referenced-but-absent
     * `m_BoneWeights` field, an apparently empty `m_Children:` list, and a
     * "first Transform" that was not the root. Unity's own object model is the
     * only correct source for these facts, so this reports through it.
     *
     * This class MUTATES NOTHING. It only loads, inspects and logs.
     */
    [UnityEditor.MenuItem("Tools/NZK/NaNimate/Diagnose Generated Prefab")]
    static void DiagnoseSelected(){
        string prefabPath=SelectedPrefabPath();
        if(NZK.B.NoE(prefabPath)){
            UnityEngine.Debug.LogError(
                "[NZK NaNimate] Select a generated prefab (or its source model) in the Project window first.");
            return;
        }
        Diagnose(prefabPath);
    }
    static string SelectedPrefabPath(){
        string[] guids=UnityEditor.Selection.assetGUIDs;
        for(int i=0;i<guids.Length;i++){
            string path=UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            if(NZK.B.NoE(path)) continue;
            if(NZK.S.EndsWithOIC(path,".prefab")) return path;
            /* A model selected instead: derive the generated prefab for it. */
            if(NZK.S.EndsAnyOIC(path,".blend",".fbx")){
                string avatar=NZK.S.P.Next(path);
                string folder=NZKNaNimateMeshFolder.PrefabFolderFor(path,avatar);
                return folder+"/"+avatar+".prefab";
            }
        }
        return null;
    }
    /*
     * Records the FIRST problem seen and counts one occurrence.
     *
     * Every check below writes through this, so the "first offender" string and
     * each counter are only ever touched in one place.
     */
    static void Note(ref int count,ref string firstOffender,string offender){
        count++;
        if(firstOffender==null) firstOffender=offender;
    }
    /* True when any bone slot is null or sits outside the prefab root, which
     * means the mesh is driven by an object outside the prefab.
     *
     * Returns the offender description, or null when every bone is inside. */
    static string BonesOutsideOffender(UnityEngine.SkinnedMeshRenderer r,UnityEngine.GameObject root){
        UnityEngine.Transform[] bones=r.bones;
        if(NZK.B.mpty.t(bones)) return null;
        /* A bone whose transform is NOT under this prefab root means the
         * mesh is driven by an object outside the prefab. */
        for(int b=0;b<bones.Length;b++){
            UnityEngine.Transform bone=bones[b];
            if(bone==null) return r.name+": null bone slot "+b;
            if(!bone.IsChildOf(root.transform)&&bone!=root.transform)
                return r.name+": bone '"+bone.name+"' is outside the prefab";
        }
        return null;
    }
    /* Report Unity's view of a generated prefab's skinning state. */
    public static void Diagnose(string prefabPath){
        UnityEngine.GameObject root=
            UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabPath);
        if(root==null){
            UnityEngine.Debug.LogError(NZK.S.NZKNaNimatePrefix("Could not load prefab at "+prefabPath));
            return;
        }
        UnityEngine.SkinnedMeshRenderer[] renderers=
            root.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true);
        var lines=new System.Text.StringBuilder();
        lines.Append(NZK.S.NK).Append("DIAGNOSE ").Append(prefabPath).Append('\n');
        lines.Append("  root: ").Append(root.name)
             .Append("  children: ").Append(root.transform.childCount).Append('\n');
        lines.Append("  renderers: ").Append(renderers.Length).Append('\n');
        int noMesh=0;
        int noBones=0;
        int bonesOutsidePrefab=0;
        int noWeights=0;
        int noPoses=0;
        int noMaterials=0;
        string firstOffender=null;
        for(int i=0;i<renderers.Length;i++){
            UnityEngine.SkinnedMeshRenderer r=renderers[i];
            if(r.sharedMesh==null){
                Note(ref noMesh,ref firstOffender,r.name+": no mesh");
                continue;
            }
            if(NZK.B.mpty.t(r.bones)){
                Note(ref noBones,ref firstOffender,r.name+": no bones array");
            }else{
                string boneOffender=BonesOutsideOffender(r,root);
                if(boneOffender!=null) Note(ref bonesOutsidePrefab,ref firstOffender,boneOffender);
            }
            if(NZK.B.mpty.t(r.sharedMesh.boneWeights))
                Note(ref noWeights,ref firstOffender,r.name+": mesh has no bone weights");
            if(NZK.B.mpty.t(r.sharedMesh.bindposes))
                Note(ref noPoses,ref firstOffender,r.name+": mesh has no bind poses");
            if(NZK.B.mpty.t(r.sharedMaterials)||r.sharedMaterials[0]==null)
                Note(ref noMaterials,ref firstOffender,r.name+": no material");
        }
        lines.Append("  --- problems ---\n");
        lines.Append("  no mesh            : ").Append(noMesh).Append('\n');
        lines.Append("  no bones array     : ").Append(noBones).Append('\n');
        lines.Append("  bones outside root : ").Append(bonesOutsidePrefab).Append('\n');
        lines.Append("  no bone weights    : ").Append(noWeights).Append('\n');
        lines.Append("  no bind poses      : ").Append(noPoses).Append('\n');
        lines.Append("  no material        : ").Append(noMaterials).Append('\n');
        if(firstOffender!=null)
            lines.Append("  first offender     : ").Append(firstOffender).Append('\n');
        /* A renderer only draws when all of these hold. Report the ones that
         * are actually visible so the count can be compared against what the
         * viewport shows. */
        int visible=0;
        for(int i=0;i<renderers.Length;i++){
            UnityEngine.SkinnedMeshRenderer r=renderers[i];
            if(!r.enabled||!r.gameObject.activeInHierarchy) continue;
            if(r.sharedMesh==null||!r.sharedMesh.isReadable) continue;
            if(NZK.B.mpty.t(r.bones)) continue;
            if(NZK.B.mpty.t(r.sharedMaterials)||r.sharedMaterials[0]==null) continue;
            visible++;
        }
        lines.Append("  eligible to draw   : ").Append(visible)
             .Append(" / ").Append(renderers.Length).Append('\n');
        /* Bounds matter for culling: an AABB that does not contain the posed
         * mesh gets culled even though the renderer is enabled. */
        if(renderers.Length>0){
            UnityEngine.Bounds b=renderers[0].localBounds;
            lines.Append("  sample localBounds : center=").Append(b.center)
                 .Append(" size=").Append(b.size).Append('\n');
            UnityEngine.Bounds w=renderers[0].bounds;
            lines.Append("  sample worldBounds : center=").Append(w.center)
                 .Append(" size=").Append(w.size).Append('\n');
        }
        UnityEngine.Debug.Log(lines.ToString());
    }
  }
}
