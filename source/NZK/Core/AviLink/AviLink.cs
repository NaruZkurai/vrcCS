namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/UnityEngine.Avatar Link")]
 public partial class AviLink : UnityEngine.MonoBehaviour {
    
    
    public LinkMode mode = LinkMode.Updater;
    public UnityEngine.GameObject avatarRoot;
    /* Updater: which generators to bake */
    public System.Boolean bakeSPS = true;
    public System.Boolean bakeToggles = true;
    public System.Boolean bakeFullController = true;
    public System.Boolean bakeArmatureLinks = true;
    /* Generator: sub-generator children */
    public System.Boolean hasSpsGenerator;
    public System.Boolean hasToggleGenerator;
    public System.Boolean hasFullController;
    /* Auto-populated list of generator roots (children with generator components) */
    public System.Collections.Generic.List<GeneratorRoot> generatorRoots = new();
    /* UnityEngine.Avatar info (auto-filled in updater mode) */
    public System.String avatarName;
    public System.String avatarId; // optional persistent ID
    void Reset()
    { // Auto-find UnityEngine.Avatar root from parent hierarchy
      if (avatarRoot == null)
      {
        var t = transform;
        while (t.parent != null) t = t.parent;
        avatarRoot = t.gameObject;
      }
    }
    /* Scan children and populate generatorRoots list */
    public void ReloadGenerators()
    {
      generatorRoots.Clear();
      /* Scan ALL children (not just immediate) for known generator types */
      var allChildren = GetComponentsInChildren<UnityEngine.Transform>(true);
      foreach (var child in allChildren)
      {
        if (child == transform) continue; // skip self
        /* Already added via a parent match? skip */
        System.Boolean alreadyAdded = false;
        foreach (var existing in generatorRoots)
        {
          if (existing.root == child.gameObject) { alreadyAdded = true; break; }
          /* If an ancestor is already a generator root,skip children of that ancestor */
          var parent = child.parent;
          while (parent != null && parent != transform)
          {
            if (existing.root == parent.gameObject) { alreadyAdded = true; break; }
            parent = parent.parent;
          }
          if (alreadyAdded) break;
        }
        if (alreadyAdded) continue;
        var hb = child.GetComponent<C_AviGenerator>();
        if (hb != null)
        {
          System.String label = hb.mode.ToString() + " [" + child.parent.name + "/" + child.name + "]";
          generatorRoots.Add(new GeneratorRoot { root = child.gameObject, label = label });
          continue;
        }
        /* Fallback: check by name conventions,auto-add missing scripts */
        System.String name = child.name;
        System.String fullPath = child.parent != transform ? child.parent.name + "/" + name : name;
        if (name.Contains("BakedControllers"))
        { generatorRoots.Add(new GeneratorRoot { root = child.gameObject, label = "Full Controller Baker [" + fullPath + "]" }); }
        else if (name.Contains("SPS") || name.Contains("Socket"))
          generatorRoots.Add(new GeneratorRoot { root = child.gameObject, label = "SPS Socket Baker [" + fullPath + "]" });
        else if (name.Contains("Toggle"))
          generatorRoots.Add(new GeneratorRoot { root = child.gameObject, label = "Toggle Baker [" + fullPath + "]" });
        else if (name.Contains("ArmatureLink"))
          generatorRoots.Add(new GeneratorRoot { root = child.gameObject, label = "Armature Link Baker [" + fullPath + "]" });
      }
      UnityEngine.Debug.Log("[AviLink] Reloaded generators: found " + generatorRoots.Count + " root(s)");
    }
  
}
}
}
