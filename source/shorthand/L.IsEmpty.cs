namespace NZK{  public static partial class L{
  /// <summary>
  /// True when an array has no usable elements.
  ///
  /// NAME: L = List/collection helpers. IsEmpty/NotEmpty/Unique are spelled
  /// out IN FULL here, unlike Pa/Sa/Sn, because they are predicates read as
  /// sentences at the call site - `L.IsEmpty(a)` - where a two-letter form
  /// would hurt more than it compressed.
  ///
  /// Null and empty are treated identically because the Unity APIs that
  /// produce these arrays (mesh.boneWeights, mesh.bindposes, renderer.bones)
  /// return either depending on whether the channel was ever authored, and
  /// every call site needs the same "nothing to work with" answer for both.
  /// </summary>
  public static bool IsEmpty<T>(T[] a){return a==null||a.Length==0;}

  /// <summary>True when a list has no usable elements. See <see cref="L.IsEmpty{T}"/>.</summary>
  public static bool IsEmpty<T>(System.Collections.Generic.List<T> a){return a==null||a.Count==0;}

  /// <summary>True when a list has usable elements.</summary>
  public static bool NotEmpty<T>(System.Collections.Generic.List<T> a){return !IsEmpty(a);}

  /// <summary>
  /// Claim the next free name in a used-set, appending "_n" until the name is
  /// unique.
  ///
  /// Necessary because Blender emits duplicate object names ("B Body" and
  /// "B Body.001" can both sanitize to an already-used leaf), and two assets
  /// written to one leaf path silently overwrite each other - one source is
  /// then lost with no error at all.
  /// </summary>
  public static string Unique(string baseName,System.Collections.Generic.HashSet<string> used){
    if(used.Add(baseName))
      return baseName;

    int suffix=1;
    string candidate;
    do
    {
      candidate=baseName+"_"+suffix;
      suffix++;
    }
    while(!used.Add(candidate));

    return candidate;
  }
  }
  }
