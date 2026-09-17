namespace NZK{  public static partial class T{
  /// <summary>
  /// Stable path identity for a UnityEngine.Transform, RELATIVE TO THE ROOT.
  ///
  /// NAME: T.Tp = Transform.TPath. T is the Transform-helper class
  /// (sibling: Tpr, Transform Path Root-inclusive). Tp/Tpr differ only by
  /// whether the root segment is included.
  ///
  /// The root's own name is deliberately EXCLUDED. A clone's root is renamed
  /// after instantiation, and the source root's name comes from the .blend
  /// import, so including it made every source/clone path pair mismatch at
  /// the first segment - 13104 unmapped bones, i.e. effectively all of them.
  /// Dropping the root name makes the key depend only on structure, which is
  /// what both sides genuinely share.
  ///
  /// The sibling index IS kept: duplicate names under one parent are common
  /// (Blender emits "B Body", "B Body.001") and would otherwise collide.
  ///
  /// The root itself contributes an empty path; give it a stable marker so
  /// it is still a valid non-null key rather than an empty string.
  /// </summary>
  public static string Tp(UnityEngine.Transform t){
    var parts=new System.Collections.Generic.List<string>(8);

    // Walk up but stop BEFORE the outermost transform, so exactly one
    // segment - the root - is omitted from the key.
    while(t!=null&&t.parent!=null){
      parts.Add(t.name+"#"+t.GetSiblingIndex());
      t=t.parent;
    }

    parts.Reverse();

    return parts.Count==0?"<root>":string.Join("/",parts);
  }

  /// <summary>
  /// Stable path identity for a UnityEngine.Transform, ROOT-INCLUSIVE,
  /// e.g. "Armature/NaNimations/NaNimate Bra".
  ///
  /// Sibling index is included so duplicate names cannot collide.
  ///
  /// Kept as a SEPARATE key from Tp on purpose: the two are NOT
  /// interchangeable. A root-inclusive key is used where source and clone
  /// roots genuinely share a name and the whole hierarchy must match;
  /// a root-exclusive key is used where the root is renamed or comes from a
  /// differently-named import, and a mismatch at the first segment would
  /// silently unmapped every bone beneath it.
  /// </summary>
  public static string Tpr(UnityEngine.Transform t){
    var parts=new System.Collections.Generic.List<string>(8);

    while(t!=null){
      parts.Add(t.name+"#"+t.GetSiblingIndex());
      t=t.parent;
    }

    parts.Reverse();
    return string.Join("/",parts);
  }
  }
  }
