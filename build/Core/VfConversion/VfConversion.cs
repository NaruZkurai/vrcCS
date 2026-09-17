#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class VfConversion
  { public const System.String StorageRoot = "Assets/!_NZK_Storage";
    public const System.String VfBackupFolder = "VF_Backups";
    public static System.String GetStorageFolder(UnityEngine.GameObject avatarRoot)
    { var an = Vars.Names.Sanitize(avatarRoot.transform.root.name);
      return StorageRoot + "/" + an + "/" + VfBackupFolder; }
    /* ── Convert a known VF component to NZK equivalent, or back it up ── */
    public static System.Boolean ConvertOrBackup(UnityEngine.GameObject go,UnityEngine.Component vfComp,System.String vfTypeName)
    { if (go == null || vfComp == null) return false;
      try { switch (vfTypeName)
        { case "HapticSocket":
          case "HapticPlug":
            if (go.GetComponent<C_NzkSpsSocket>() == null)
            { /* Already handled by the caller — just return true */ }
            return true;
          case "MainVRCFury":
            /* Main VRCFury component — try to bake each feature, then remove */
            Systems.Baking.BakeAllVf(go);
            Systems.Baking.DestroyAllVrcfuryComponents(go);
            return true;
          case "HapticTouchReceiver":
          case "HapticTouchSender":
            /* Touch contacts — NZK handles these through SPS baking, just remove VF */
            UnityEditor.Undo.DestroyObjectImmediate(vfComp);
            return true;
          default:
            /* Unknown VF feature — back up to storage */
            BackupVfComponent(go,vfComp,vfTypeName);
            UnityEditor.Undo.DestroyObjectImmediate(vfComp);
            return false; } }
      catch (System.Exception ex) { UnityEngine.Debug.LogWarning("[NZK] VF conversion failed for " + vfTypeName + ": " + ex.Message);
        BackupVfComponent(go,vfComp,vfTypeName); return false; } }
    /* ── Back up a VF component's serialized data + scene reference ── */
    static void BackupVfComponent(UnityEngine.GameObject go,UnityEngine.Component vfComp,System.String vfTypeName)
    { try
      { var avatarRoot = go.transform.root.gameObject;
        var folder = GetStorageFolder(avatarRoot);
        Systems.Folder.Ensure(folder);
        var safe = Vars.Names.Sanitize(go.name) + "_" + vfTypeName;
        var path = folder + "/" + safe + ".json";
        /* Collect prefab GUID if part of a prefab instance */
        System.String prefabGuid = "";
        if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(go))
        { var root = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(go);
          if (root != null) { var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(root);
            if (prefab != null) { var pPath = UnityEditor.AssetDatabase.GetAssetPath(prefab);
              prefabGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(pPath).ToString(); } } }
        /* Save JSON with embedded prefab GUID header */
        var json = UnityEditor.EditorJsonUtility.ToJson(vfComp);
        var header = "# PREFAB_GUID:" + prefabGuid + "\n# OBJECT:" + go.name + "\n# TYPE:" + vfTypeName + "\n";
        System.IO.File.WriteAllText(path,header + json);
        if (!System.IO.File.Exists(path)) UnityEditor.AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[NZK] Backed up " + vfTypeName + " on " + go.name + " → " + path + " (prefab: " + (prefabGuid != "" ? prefabGuid : "none") + ")"); }
      catch (System.Exception ex) { UnityEngine.Debug.LogWarning("[NZK] Backup failed for " + vfTypeName + ": " + ex.Message); } } }
}
}
#endif
