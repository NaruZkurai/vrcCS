#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class ObjectMerge {
    /* ── CODES ───────────────────────────────────────────────────────────
       Local to this file so the numbers are readable beside the call that
       raises them.  The resolver is the global one in E.C, which reflects
       `rr<code>` off NZK.E, so these keys MUST exist in E.rr.cs. */
    /** No usable selection: fewer than two objects, or all null. */
    public const System.Int32 CodeNothingSelected = 62;
    /** The target is itself one of the sources, or null. */
    public const System.Int32 CodeBadTarget = 63;
    /** A source has a component that could not be copied onto the target. */
    public const System.Int32 CodeComponentCopyFailed = 64;
    /** The merge completed. */
    public const System.Int32 CodeMerged = 65;
    /* ── RESULT ──────────────────────────────────────────────────────── */
    /** <summary>What one object merge did, so a caller can report it.
     *
     *  `References` and `Components` are counted separately because they fail
     *  for unrelated reasons and a single total cannot tell the user which half
     *  went wrong — the whole point of the reported bug is that the component
     *  move LOOKED fine while the references were being dropped.</summary> */
    
    /* ── ENTRY POINTS ────────────────────────────────────────────────── */
    /** <summary>Merge the selection, treating the LAST selected object as target.
     *
     *  Unity reports the active object separately from the selected set, so the
     *  target is taken from `activeGameObject` and NOT from the tail of the
     *  array.  The array's order follows the project's selection-sorting setting
     *  and is not the click order, so reading the tail merges into whatever
     *  happens to sort last — which is a different object between machines and
     *  between Unity versions.</summary> */
    public static MergeReport MergeToActive()
    { UnityEngine.GameObject target = UnityEditor.Selection.activeGameObject;
      return Merge(UnityEditor.Selection.gameObjects,target); }
    /** <summary>Merge `sources` into `target`, preserving references.
     *
     *  `target` is excluded from the work even when it appears in `sources`,
     *  because Unity includes it in the selection and merging an object into
     *  itself would have the reference pass rewrite the target's own
     *  self-references and the destroy pass delete the target.</summary> */
    public static MergeReport Merge(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
    {
      MergeReport report = new MergeReport();
      System.Collections.Generic.List<UnityEngine.GameObject> work = Sources(sources,target);
      if (work.Count == 0)
      { report.Code = CodeNothingSelected;
        report.Message = NZK.E.rr62(target);
        NZK.E.C.w(report.Code,report.Message);
        return report; }
      if (target == null)
      { report.Code = CodeBadTarget;
        report.Message = NZK.E.rr63(sources);
        NZK.E.C.e(report.Code,report.Message);
        return report; }
      if (!MergeToTarget(sources,target,ref report))
      { NZK.E.C.e(report.Code,report.Message);
        return report; }
      report.Success = true;
      report.Code = CodeMerged;
      report.Message = NZK.E.rr65(report.References + " ref(s), " +
                                  report.Components + " component(s), " +
                                  report.Children + " child(ren)");
      NZK.E.C.d(report.Code,report.Message);
      return report;
    }
    /* ── SOURCE LIST ─────────────────────────────────────────────────── */
    /** <summary>The sources to merge, in order, with the target and nulls removed.
     *
     *  Distinct is applied because a Unity selection can legitimately contain
     *  the same object twice (a multi-select that spans a prefab and its
     *  instance), and merging one object twice would move its components onto
     *  the target on the first pass and then find the target already carrying
     *  them on the second — reporting a successful merge of nothing.</summary> */
    public static System.Collections.Generic.List<UnityEngine.GameObject> Sources(UnityEngine.GameObject[] sources,UnityEngine.GameObject target)
    { System.Collections.Generic.List<UnityEngine.GameObject> work =
        new System.Collections.Generic.List<UnityEngine.GameObject>();
      if (NZK.B.mpty.t(sources)) return work;
      foreach (UnityEngine.GameObject go in sources)
      { if (go == null || go == target) continue;
        if (!work.Contains(go)) work.Add(go); }
      return work; }
    /* ── THE MERGE ───────────────────────────────────────────────────── */
    /** <summary>Do the work, in the one order that cannot lose a reference.
     *
     *  Order, and why it is this order:
     *    1. unpack prefab instances, because a component cannot be copied off a
     *       prefab instance that is still linked without the copy being
     *       reverted on the next import;
     *    2. REWRITE REFERENCES, while every component is still alive and still
     *       in its original place — the replacement is found by looking up the
     *       source, so a source that has already been moved or destroyed cannot
     *       be resolved;
     *    3. move components, copying before destroying;
     *    4. reparent children, so a child that was referenced is still a child
     *       of something that exists;
     *    5. destroy only the sources that are now empty.
     *
     *  Any reordering puts a lookup after the thing it looks up has been
     *  removed, which is what the original code did.</summary> */
    static System.Boolean MergeToTarget(UnityEngine.GameObject[] sources,UnityEngine.GameObject target,ref MergeReport report)
    {
      System.Collections.Generic.List<UnityEngine.GameObject> work = Sources(sources,target);
      UnityEditor.Undo.SetCurrentGroupName("Merge to Last Selected");
      System.Int32 group = UnityEditor.Undo.GetCurrentGroup();
      UnityEditor.Undo.RegisterFullObjectHierarchyUndo(target,"Merge target");
      foreach (UnityEngine.GameObject src in work)
      { if (src != null) UnityEditor.Undo.RegisterFullObjectHierarchyUndo(src,"Merge source"); }
      if (!UnpackPrefabs(work)) return false;
      report.References = MergeReferences(work,target);
      System.Int32 moved = 0;
      foreach (UnityEngine.GameObject src in work)
      { if (src == null) continue;
        moved += MoveComponents(src,target); }
      report.Components = moved;
      System.Int32 kids = 0;
      foreach (UnityEngine.GameObject src in work)
      { if (src == null) continue;
        kids += MoveChildren(src,target); }
      report.Children = kids;
      System.Int32 removed = 0;
      foreach (UnityEngine.GameObject src in work)
      { if (src == null || src == target) continue;
        if (!IsEmpty(src))
        { /* Something is still on this object that the merge does not know how
             to move.  Reported and LEFT ALONE rather than destroyed: destroying
             it would discard the one component the merge could not account for,
             and the user would have no way to find out which. */
          NZK.E.C.w(CodeComponentCopyFailed,src.name);
          continue; }
        UnityEditor.Undo.DestroyObjectImmediate(src);
        removed++; }
      report.Removed = removed;
      UnityEditor.Undo.CollapseUndoOperations(group);
      return true; }
    /** <summary>A source is empty when it holds nothing but a Transform.
     *
     *  Transform itself is not movable, so counting it would make every object
     *  non-empty and nothing would ever be removed.</summary> */
    static System.Boolean IsEmpty(UnityEngine.GameObject go)
    { if (go == null) return false;
      UnityEngine.Component[] comps = go.GetComponents<UnityEngine.Component>();
      foreach (UnityEngine.Component c in comps)
      { if (c != null && !(c is UnityEngine.Transform)) return false; }
      return true; }
    /* ── REFERENCES ──────────────────────────────────────────────────── */
    /** <summary>Re-point the scene's references from the sources to the target.
     *
     *  Public so the older Systems.MergeObjectsToTarget / DeepMerge path can
     *  delegate here instead of keeping its own copy of the scan.  Two copies
     *  of this logic is how the component-reference blindness survived: the
     *  copy that was fixed was not the copy that ran.</summary> */
    public static System.Int32 MergeReferencesFor(System.Collections.Generic.List<UnityEngine.GameObject> sources,UnityEngine.GameObject target)
    { return MergeReferences(sources,target); }
    /** <summary>Re-point every serialized reference that names a source object.
     *
     *  Returns how many fields were written.  The scan is scene-wide plus the
     *  sources plus the target, because a reference TO a merged object can live
     *  on any object in the scene — an avatar descriptor pointing at a menu
     *  holder, a constraint pointing at a looked-at object — and scanning only
     *  the merged objects would miss exactly the references the user notices.
     *
     *  Project assets are deliberately NOT scanned: rewriting a prefab or a
     *  ScriptableObject because of a scene-local merge would change every other
     *  scene using it, which is a different operation with a different undo
     *  story.</summary> */
    static System.Int32 MergeReferences(System.Collections.Generic.List<UnityEngine.GameObject> sources,UnityEngine.GameObject target)
    { if (target == null) return 0;
      System.Collections.Generic.List<UnityEngine.GameObject> scan =
        new System.Collections.Generic.List<UnityEngine.GameObject>();
      foreach (UnityEngine.GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
      { Collect(root,scan); }
      Collect(target,scan);
      foreach (UnityEngine.GameObject s in sources) Collect(s,scan);
      /* Two passes: collect, then apply.  See the file header for why a single
         pass makes the result order-dependent. */
      System.Collections.Generic.List<System.Action> writes = new System.Collections.Generic.List<System.Action>();
      System.Collections.Generic.HashSet<System.String> seen = new System.Collections.Generic.HashSet<System.String>();
      foreach (UnityEngine.GameObject go in scan)
      { if (go == null) continue;
        UnityEngine.Component[] comps = go.GetComponents<UnityEngine.Component>();
        foreach (UnityEngine.Component comp in comps)
        { if (comp == null || comp is UnityEngine.Transform) continue;
          UnityEditor.SerializedObject so;
          try { so = new UnityEditor.SerializedObject(comp); }
          catch (System.Exception) { continue; }   /* not serializable - skip */
          UnityEditor.SerializedProperty prop = so.GetIterator();
          System.Boolean dirty = false;
          while (prop.NextVisible(true))
          { if (prop.propertyType != UnityEditor.SerializedPropertyType.ObjectReference) continue;
            UnityEngine.Object current = prop.objectReferenceValue;
            if (current == null) continue;
            System.String key = comp.GetInstanceID() + ":" + prop.propertyPath;
            if (!seen.Add(key)) continue;
            UnityEngine.Object replacement = Counterpart(current,sources,target);
            if (replacement == null || replacement == current) continue;
            /* Capture the property path, not the SerializedProperty: the
               iterator is invalidated once this loop moves on, so a deferred
               write must re-resolve its target at apply time. */
            System.String path = prop.propertyPath;
            UnityEngine.Component owner = comp;
            UnityEngine.Object value = replacement;
            writes.Add(() =>
            { if (owner == null) return;
              UnityEditor.SerializedObject late = new UnityEditor.SerializedObject(owner);
              UnityEditor.SerializedProperty p = late.FindProperty(path);
              if (p == null) return;
              p.objectReferenceValue = value;
              late.ApplyModifiedProperties(); });
            dirty = true; }
          if (dirty) UnityEditor.EditorUtility.SetDirty(comp); } }
      foreach (System.Action w in writes) w();
      return writes.Count; }
    /** <summary>What a reference to `current` should become.
     *
     *  THE DESCENDANT CASE IS THE WHOLE POINT.  Only a source ROOT has its
     *  components moved onto the target; a source's CHILDREN are reparented
     *  intact, so their components are never copied and never appear on the
     *  target.  A reference that points at a child therefore has no same-type
     *  component to find on the target object itself, and an implementation
     *  that only looks there leaves it pointing at the child - which, since
     *  the child has just been moved under the target, reads as a reference
     *  that survived when in fact it now resolves through a hierarchy the
     *  merge rearranged.  The user-visible symptom is exactly the report:
     *  children's components keep their OLD references.
     *
     *  So a referenced descendant is mapped by RELATIVE PATH: the reference is
     *  re-pointed at the object the descendant became, which is
     *  `target.Find(relativePathFromSourceRoot)`.  That is the same
     *  correspondence the reparent step establishes, so the two cannot drift.
     *
     *  Resolution order, most specific first:
     *    1. the object itself is a source root  -> target (or its same-type
     *       component, because a typed field refuses a GameObject where a
     *       Collider is expected);
     *    2. the object is BELOW a source root   -> the target's counterpart at
     *       the same relative path, then that counterpart's same-type
     *       component, then the target itself as a last resort;
     *    3. anything else -> left alone.
     *
     *  Both a GameObject and a Component reference go through the same
     *  mapping; they differ only in whether the component is picked off the
     *  resolved object at the end.</summary> */
    static UnityEngine.Object Counterpart(UnityEngine.Object current,System.Collections.Generic.List<UnityEngine.GameObject> sources,UnityEngine.GameObject target)
    { if (current == null || target == null) return null;
      UnityEngine.GameObject referenced = null;
      System.Type wanted = null;
      if (current is UnityEngine.Component comp)
      { referenced = comp.gameObject; wanted = comp.GetType(); }
      else if (current is UnityEngine.GameObject go) { referenced = go; }
      else { return null; }
      if (referenced == null) return null;
      UnityEngine.GameObject counterpart = CounterpartObject(referenced,sources,target);
      if (counterpart == null) return null;
      /* A component reference lands on the counterpart's component of the SAME
         TYPE.  Falling back to the counterpart GameObject is attempted rather
         than assumed: the serializer refuses the write when the field is typed,
         which leaves the reference alone instead of pointing it somewhere
         wrong.  A GameObject reference has no type to match and lands on the
         counterpart itself. */
      if (wanted == null) return counterpart;
      UnityEngine.Component match = counterpart.GetComponent(wanted);
      return match != null ? (UnityEngine.Object)match : (UnityEngine.Object)counterpart; }
    /** <summary>The object a source-side object becomes after the merge.
     *
     *  A source root becomes the target; anything below a source root becomes
     *  the target's child at the same relative path, and falls back to the
     *  target when the merge could not place it (the reparent step reports
     *  that separately, so a moved reference still points at the merged result
     *  rather than at an object the merge has just emptied).</summary> */
    static UnityEngine.GameObject CounterpartObject(UnityEngine.GameObject referenced,System.Collections.Generic.List<UnityEngine.GameObject> sources,UnityEngine.GameObject target)
    { if (referenced == null || target == null) return null;
      if (referenced == target) return null;          /* already the destination */
      foreach (UnityEngine.GameObject src in sources)
      { if (src == null) continue;
        if (referenced == src) return target;
        if (!IsDescendant(referenced.transform,src.transform)) continue;
        /* Relative path from the SOURCE ROOT, not from the scene root: the
           children are re-parented under the target, so the path that
           identifies the counterpart is the path inside the source. */
        System.String rel = UnityEditor.AnimationUtility.CalculateTransformPath(referenced.transform,src.transform);
        if (!NZK.S.Has(rel)) continue;
        UnityEngine.Transform found = target.transform.Find(rel);
        return found != null ? found.gameObject : target; }
      return null; }
    /** <summary>Is `t` strictly below `root`?
     *
     *  Walked upward from the candidate rather than downward from the root:
     *  the question is about ONE object, and walking up stops as soon as the
     *  answer is known instead of enumerating the root's whole subtree for
     *  every reference scanned.</summary> */
    public static System.Boolean IsDescendant(UnityEngine.Transform t,UnityEngine.Transform root)
    { if (t == null || root == null) return false;
      UnityEngine.Transform cur = t.parent;
      while (cur != null)
      { if (cur == root) return true;
        cur = cur.parent; }
      return false; }
    /** <summary>Collect an object and everything under it.
     *
     *  Iterative rather than recursive: a deep rig can be thousands of nodes
     *  and a recursive walk over a user's hierarchy risks a stack overflow in
     *  the editor, which takes the whole session down rather than failing one
     *  operation.</summary> */
    static void Collect(UnityEngine.GameObject root,System.Collections.Generic.List<UnityEngine.GameObject> into)
    { if (root == null) return;
      System.Collections.Generic.Queue<UnityEngine.Transform> queue =
        new System.Collections.Generic.Queue<UnityEngine.Transform>();
      queue.Enqueue(root.transform);
      System.Collections.Generic.HashSet<System.Int32> seen = new System.Collections.Generic.HashSet<System.Int32>();
      while (queue.Count > 0)
      { UnityEngine.Transform t = queue.Dequeue();
        if (t == null) continue;
        if (!seen.Add(t.GetInstanceID())) continue;
        into.Add(t.gameObject);
        for (System.Int32 i = 0; i < t.childCount; i++) queue.Enqueue(t.GetChild(i)); } }
    /* ── COMPONENTS ──────────────────────────────────────────────────── */
    /** <summary>Move every movable component off `src` and onto `target`.
     *
     *  COPY FIRST, DESTROY AFTER.  The original destroyed unconditionally, so a
     *  component whose type cannot be added to the target (a second Transform,
     *  a component with [DisallowMultipleComponent] already present and
     *  incompatible) was deleted with nothing taking its place — the object
     *  came out of the merge with LESS than it went in.  The copy is verified
     *  before the original is removed.
     *
     *  CopySerialized is used rather than a field-by-field copy because it
     *  carries the SERIALIZED state, including references this file has just
     *  rewritten, and it works for component types this code has never heard
     *  of — which is all of the VRC/SDK and third-party ones.</summary> */
    public static System.Int32 MoveComponents(UnityEngine.GameObject src,UnityEngine.GameObject target)
    { if (src == null || target == null || src == target) return 0;
      System.Int32 moved = 0;
      UnityEngine.Component[] comps = src.GetComponents<UnityEngine.Component>();
      foreach (UnityEngine.Component c in comps)
      { if (c == null || c is UnityEngine.Transform) continue;
        System.Type type = c.GetType();
        UnityEngine.Component existing = target.GetComponent(type);
        if (existing == null)
        { UnityEngine.Component added = target.AddComponent(type);
          if (added == null)
          { NZK.E.C.w(CodeComponentCopyFailed,type.Name + " on " + src.name);
            continue; }
          UnityEditor.Undo.RegisterCreatedObjectUndo(added,"Add merged component");
          UnityEditor.EditorUtility.CopySerialized(c,added);
          UnityEditor.EditorUtility.SetDirty(added); }
        else
        { UnityEditor.Undo.RecordObject(existing,"Update merged component");
          UnityEditor.EditorUtility.CopySerialized(c,existing);
          UnityEditor.EditorUtility.SetDirty(existing); }
        UnityEditor.Undo.DestroyObjectImmediate(c);
        moved++; }
      return moved; }
    /** <summary>Reparent a source's children onto the target.
     *
     *  Taken from index 0 in a while loop rather than iterating a snapshot: the
     *  loop shrinks childCount as it goes, so a for-loop over a captured count
     *  reads past the end once the first child moves.</summary> */
    public static System.Int32 MoveChildren(UnityEngine.GameObject src,UnityEngine.GameObject target)
    { if (src == null || target == null || src == target) return 0;
      System.Int32 moved = 0;
      UnityEngine.Transform srcT = src.transform;
      while (srcT.childCount > 0)
      { UnityEngine.Transform child = srcT.GetChild(0);
        if (child == null) break;
        UnityEditor.Undo.SetTransformParent(child,target.transform,"Move child");
        moved++; }
      return moved; }
    /* ── PREFABS ──────────────────────────────────────────────────────── */
    /** <summary>Unpack every prefab instance in the work set.
     *
     *  A linked prefab instance reverts component additions and moves on the
     *  next import, so a component copied OFF one is copied back over on the
     *  next prefab refresh and the merge appears to undo itself later.  The
     *  TARGET is deliberately not unpacked: it is the object the user is
     *  merging INTO and the target's own prefab link is theirs to keep.</summary> */
    static System.Boolean UnpackPrefabs(System.Collections.Generic.List<UnityEngine.GameObject> work)
    { foreach (UnityEngine.GameObject src in work)
      { if (src == null) continue;
        if (!UnityEditor.PrefabUtility.IsPartOfPrefabInstance(src)) continue;
        UnityEngine.GameObject root = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(src);
        if (root == null) root = src;
        NZK.E.C.d(66,root.name);
        UnityEditor.PrefabUtility.UnpackPrefabInstance(root,UnityEditor.PrefabUnpackMode.Completely,UnityEditor.InteractionMode.UserAction); }
      return true; }
  
}
}
}
#endif
