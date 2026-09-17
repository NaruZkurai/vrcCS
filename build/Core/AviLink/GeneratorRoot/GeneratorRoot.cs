namespace NZK
{
public static partial class Core {
public partial class AviLink {
[System.Serializable]
    public class GeneratorRoot
    { /* The generator UnityEngine.GameObject (child of this AviLink) */
      public UnityEngine.GameObject root;
      /* Display name / type label */
      public System.String label;
      /* Whether this generator should be baked */
      public System.Boolean enabled = true;
      /* Quick Run button — runs this specific generator's bake (editor only) */
      public void Run()
      {
        if (root == null || !enabled) return;
        var hb = root.GetComponent<C_AviGenerator>();
        if (hb == null) { UnityEngine.Debug.LogWarning("[AviLink] No C_AviGenerator on " + root.name); return; }
#if UNITY_EDITOR
      if (label != null && label.Contains("Full Controller")) AVController.BakeAll(hb);
      else if (label != null && label.Contains("SPS")) Systems.SPS.BakeSelected();
      else if (label != null && label.Contains("Toggle")) AVController.BakeToggleGenerator(hb);
      else if (label != null && label.Contains("Armature Link")) Systems.Baking.BakeArmatureLink(root);
      else { UnityEngine.Debug.Log("[AviLink] Running " + label + " on " + root.name); AVController.BakeAll(hb); }
#endif
      }
    }
}
}
}
