#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class Systems {
public static class SpsLayerBuilderStub
  { public static void BuildAll(SpsConfig config)
    { if (config == null) return;
      var avatarRoot = config.gameObject.transform.root.gameObject;
      var vrcad = avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
      if (vrcad == null) return;
      /* Find or create the SPS controller path */
      System.String ctrlPath = "Assets/!_NZK_Generated/" + Vars.Names.Sanitize(config.avatarName) + "/SPS/SPS.controller";
      Systems.Folder.Ensure(System.IO.Path.GetDirectoryName(ctrlPath).Replace('\\','/'));
      /* Generate PhysBone components from bone entries */
      foreach (var be in config.boneEntries)
      { if (be == null || string.IsNullOrEmpty(be.chainName)) continue;
        var chain = avatarRoot.transform.Find(be.chainName);
        if (chain == null) continue;
        var pb = chain.gameObject.GetComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
        if (pb == null) pb = chain.gameObject.AddComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
        pb.stiffness = be.stiffness; pb.pull = be.pull; pb.grabMovement = be.grabMovement;
        pb.maxAngleX = be.maxAngleX; pb.maxAngleZ = be.maxAngleZ;
        pb.allowGrabbing = be.isGrabbable ? VRC.Dynamics.VRCPhysBoneBase.AdvancedBool.True : VRC.Dynamics.VRCPhysBoneBase.AdvancedBool.False;
        pb.allowPosing = be.isPoseable ? VRC.Dynamics.VRCPhysBoneBase.AdvancedBool.True : VRC.Dynamics.VRCPhysBoneBase.AdvancedBool.False; }
      /* Generate contact receivers */
      foreach (var cr in config.contactReceivers)
      { if (cr == null || cr.rootTransform == null) continue;
        var contact = cr.rootTransform.gameObject.GetComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
        if (contact == null) contact = cr.rootTransform.gameObject.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
        contact.radius = cr.radius; contact.parameter = cr.parameter;
        contact.allowSelf = cr.allowSelf; contact.allowOthers = cr.allowOthers;
        if (cr.collisionTags != null) contact.collisionTags = new System.Collections.Generic.List<System.String>(cr.collisionTags); }
      UnityEngine.Debug.Log("[SPS] Built layers for " + config.avatarName + ": " + config.boneEntries.Count + " bones, " + config.contactReceivers.Count + " contacts"); } }
}
}
}
#endif
