#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
[UnityEngine.AddComponentMenu("NZK/Circle Menu Builder")]
  public class CircleMenuBuilder : UnityEngine.MonoBehaviour
  {
  public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu sourceMenu;
  public System.String avatarName = "";
  VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu _prevSourceMenu;
  public const int MaxPerPage = 8;
  public System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu> menusBuilt = new();
  public System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu> menusToBuild = new();
  void OnValidate()
  { if (sourceMenu != null && sourceMenu != _prevSourceMenu)
    { _prevSourceMenu = sourceMenu;
    UnityEditor.EditorApplication.delayCall -= DelayedBuild;
    UnityEditor.EditorApplication.delayCall += DelayedBuild; }
    if (sourceMenu == null) _prevSourceMenu = null; }
  void DelayedBuild()
  { if (this == null || sourceMenu == null) return;
    Build(); }
  public System.String MenusFolder
  { get
    {
    System.String name = System.String.IsNullOrEmpty(avatarName) ? "Avatar" : avatarName;
    return NZKPaths.AviRoot(name) + "/menus/"; }
  }
  public void ClearBuilt()
  { for (int i = transform.childCount - 1; i >= 0; i--)
    { var child = transform.GetChild(i);
    if (child.GetComponent<ProxyMenuSlot>() != null)
      DestroyImmediate(child.gameObject); }
    menusBuilt.Clear();
    menusToBuild.Clear(); }
  public void Build()
  { if (sourceMenu == null)
    { UnityEngine.Debug.LogWarning("[CMBuilder] No source menu assigned."); return; }
    System.String menusFolder = MenusFolder;
    System.String fullDir = System.IO.Path.GetFullPath(menusFolder);
    if (!System.IO.Directory.Exists(fullDir))
    System.IO.Directory.CreateDirectory(fullDir);
    ClearBuilt();
    var cloneMap = new System.Collections.Generic.Dictionary<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
    var toClone = new System.Collections.Generic.Queue<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
    toClone.Enqueue(sourceMenu);
    while (toClone.Count > 0)
    { var src = toClone.Dequeue();
    if (cloneMap.ContainsKey(src)) continue;
    System.String assetPath = menusFolder + src.name + ".asset";
    var existing = IF_UE.Load<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>(assetPath);
    VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu clone;
    if (existing != null)
    { clone = existing;
      clone.controls =  new System.Collections.Generic.List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>(System.Linq.Enumerable.Select(src.controls, c => new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
      {
      name = c.name,type = c.type,parameter = c.parameter,value = c.value,subMenu = c.subMenu,icon = c.icon,labels = c.labels,style = c.style
      }));
      IF_UE.SetDirty(clone); }
    else
    { clone = UnityEngine.ScriptableObject.Instantiate(src);
      clone.name = src.name;
      IF_UE.CreateAsset(clone,assetPath); }
    cloneMap[src] = clone;
    foreach (var ctrl in src.controls)
    { if (ctrl.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu && ctrl.subMenu != null)
      toClone.Enqueue(ctrl.subMenu); }
    }
    IF_UE.SaveAndRefresh();
    foreach (var kvp in cloneMap)
    { var clone = kvp.Value;
    for (int i = 0; i < clone.controls.Count; i++)
    { var ctrl = clone.controls[i];
      if (ctrl.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu && ctrl.subMenu != null)
      { if (cloneMap.TryGetValue(ctrl.subMenu,out var clonedSub))
        ctrl.subMenu = clonedSub; }
    }
    IF_UE.SetDirty(clone); }
    IF_UE.SaveAndRefresh();
    menusToBuild.Add(sourceMenu);
    int pageIndex = 0;
    while (menusToBuild.Count > 0)
    { var currentMenu = menusToBuild[0];
    menusToBuild.RemoveAt(0);
    if (menusBuilt.Contains(currentMenu))
    { UnityEngine.Debug.LogWarning("[CMBuilder] Recursive menu detected: " + currentMenu.name + " — creating marker.");
      var marker = new UnityEngine.GameObject("recursive menu building detected");
      UnityEditor.Undo.RecordObject(transform,"Create recursive marker");
      marker.transform.SetParent(transform,false);
      marker.transform.SetSiblingIndex(0);
      continue; }
    menusBuilt.Add(currentMenu);
    cloneMap.TryGetValue(currentMenu,out var clonedMenu);
    System.String pageName = GetPageName(pageIndex);
    var pageTransform = CreatePage(pageName,clonedMenu);
    pageIndex++;
    var controls = currentMenu.controls;
    int controlCount = UnityEngine.Mathf.Min(controls.Count,MaxPerPage);
    for (int i = 0; i < controlCount; i++)
    { var ctrl = controls[i];
      if (ctrl == null) continue;
      System.String childName = !System.String.IsNullOrEmpty(ctrl.name) ? ctrl.name : "slot_" + i;
      var childGo = new UnityEngine.GameObject(childName);
      UnityEditor.Undo.RecordObject(pageTransform,"Create proxy slot");
      childGo.transform.SetParent(pageTransform,false);
      var proxy = childGo.AddComponent<ProxyMenuSlot>();
      UnityEditor.Undo.RecordObject(childGo,"Add ProxyMenuSlot");
      proxy.CopyFrom(ctrl,i);
      if (ctrl.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu && ctrl.subMenu != null)
      { if (cloneMap.TryGetValue(ctrl.subMenu,out var clonedSub))
        proxy.subMenu = clonedSub;
      else
        proxy.subMenu = ctrl.subMenu;
      menusToBuild.Add(ctrl.subMenu); }
    }
    }
    UnityEngine.Debug.Log("[CMBuilder] Built " + cloneMap.Count + " cloned menu(s)," + pageIndex +
    " page(s) from " + sourceMenu.name + " → " + menusFolder); }
  System.String GetPageName(int index)
  { if (index == 0) return "Menu";
    if (index == 1) return "Next";
    return "Next" + index; }
  UnityEngine.Transform CreatePage(System.String name,VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu clonedMenu)
  { var existing = transform.Find(name);
    UnityEngine.GameObject pageGo;
    if (existing != null)
    { pageGo = existing.gameObject;
    for (int i = existing.childCount - 1; i >= 0; i--)
    { var child = existing.GetChild(i);
      if (child.GetComponent<ProxyMenuSlot>() != null)
      DestroyImmediate(child.gameObject); }
    }
    else
    { pageGo = new UnityEngine.GameObject(name);
    UnityEditor.Undo.RecordObject(transform,"Create menu page " + name);
    pageGo.transform.SetParent(transform,false); }
    var slot = pageGo.GetComponent<BakedMenuSlot>();
    if (slot == null)
    { slot = pageGo.AddComponent<BakedMenuSlot>();
    UnityEditor.Undo.RecordObject(pageGo,"Add BakedMenuSlot"); }
    slot.menu = clonedMenu;
    int pageNum = name == "Menu" ? 0 : (name == "Next" ? 1 : System.Int32.Parse(name.Substring(4)));
    slot.pageNumber = pageNum;
    slot.isRoot = (name == "Menu");
    slot.controlCount = clonedMenu != null ? clonedMenu.controls.Count : 0;
    return pageGo.transform; }
  }
}
}
#endif
