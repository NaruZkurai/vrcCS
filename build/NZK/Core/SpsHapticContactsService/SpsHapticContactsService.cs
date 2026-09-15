#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static class SpsHapticContactsService
  { public static System.String AddReceiver(ReceiverRequest req)
    { var child = new UnityEngine.GameObject(req.objName ?? "Contact");
      child.transform.SetParent(req.obj.transform,false);
      child.transform.localPosition = req.pos;
      if (req.rotation != UnityEngine.Vector3.zero) child.transform.localEulerAngles = req.rotation;
      var cr = child.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
      cr.radius = req.radius; cr.position = req.pos;
      cr.rotation = req.rotation != UnityEngine.Vector3.zero ? UnityEngine.Quaternion.Euler(req.rotation) : UnityEngine.Quaternion.identity;
      cr.collisionTags = req.tags != null ? System.Linq.Enumerable.ToList(req.tags) :  new System.Collections.Generic.List<System.String>();
      cr.allowSelf = req.party == SpsHapticUtils.ReceiverParty.Self;
      cr.allowOthers = req.party == SpsHapticUtils.ReceiverParty.Others;
      cr.localOnly = req.localOnly; cr.receiverType = req.type;
      if (req.height > 0) { cr.height = req.height; cr.shapeType = VRC.Dynamics.ContactBase.ShapeType.Capsule; }
      if (req.localOnly) { var tag = SpsHapticUtils.RandomTag(); cr.parameter = tag; return tag; }
      else { cr.parameter = req.paramName; return req.paramName; } }
  }
}
}
#endif
