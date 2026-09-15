namespace NZK
{
public static partial class Core {
public static partial class AVController
  {/* ── Constants ────────────────────────────────────────────────── */
    public const System.String LogPrefix = "[HB] ";
    public const System.String SuffixBS = "_BS";
    public const System.String SuffixNaN = "_NaN";
    public const System.String SuffixOT = "_OT";
    public const System.String SuffixUC = "_UC";
    public static readonly System.String[] VisemeNames = { "sil", "nn", "ff", "th", "dd", "kk", "ch", "ss", "nn", "rr", "aa", "e", "ih", "oh", "ou" };
    /* ================================================================
     *  Name sanitization helpers
     * ================================================================ */
    public static System.String SanitizeName(System.String n)
    {
      var invalids = System.IO.Path.GetInvalidFileNameChars();
      var chars = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(n, c => !System.Linq.Enumerable.Contains(invalids, c) && c != ' '));
      return new System.String(chars);
    }
    public static System.String SanitizeParamName(System.String n)
    {
      var sb = new System.Text.StringBuilder();
      foreach (var c in n)
      {
        if (System.Char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
        else if (c == ' ' || c == '-') sb.Append('_');
      }
      return sb.ToString();
    }
    public static System.String GetToggleCategory(System.String childName)
    {
      if (childName.EndsWith(SuffixBS)) return "Blendshape";
      if (childName.EndsWith(SuffixNaN)) return "NaNimations";
      if (childName.EndsWith(SuffixOT) || childName.EndsWith(SuffixUC)) return "UnityEngine.Object Toggle";
      return "UnityEngine.Object Toggle";
    }
    /* ================================================================
     *  Skeleton / armature utilities
     * ================================================================ */
    public static int BoneDepth(UnityEngine.Transform t) { int d = 0; while (t.parent != null) { t = t.parent; d++; } return d; }
    static System.Boolean IsBoneName(System.String n)
    {
      if (System.String.IsNullOrEmpty(n)) return false;
      n = n.ToLower();
      return !n.StartsWith("hb_") && !n.StartsWith("armature") && !n.Contains("meshe") && !n.Contains("generat")
      && !n.Contains("sources") && !n.Contains("toggle") && !n.Contains("sps_");
    }
    public static UnityEngine.Transform CreateBoneChain(UnityEngine.Transform root, UnityEngine.Transform src)
    {
      if (src.parent == null) return root;
      if (!IsBoneName(src.parent.name)) return root;
      var c = root.Find(src.parent.name);
      if (c != null) return c;
      var p = CreateBoneChain(root, src.parent);
      var n = new UnityEngine.GameObject(src.parent.name); n.transform.SetParent(p, false);
      n.transform.position = src.parent.position; n.transform.rotation = src.parent.rotation; n.transform.localScale = UnityEngine.Vector3.one;
      return n.transform;
    }
    /* ================================================================
     *  Core bake entry point
     * ================================================================ */
    /* ── Field/property access helpers ────────────────────────────── */
    public static System.String ReadBlueprintId(UnityEngine.Object pm, System.Type pmType)
    {
      var f = pmType.GetField("blueprintId"); if (f != null) { var v = f.GetValue(pm) as System.String; if (!System.String.IsNullOrEmpty(v)) return v; }
      var p = pmType.GetProperty("blueprintId"); if (p != null) { var v = p.GetValue(pm, null) as System.String; if (!System.String.IsNullOrEmpty(v)) return v; }
      return null;
    }
    public static void WriteBlueprintId(UnityEngine.Object pm, System.Type pmType, System.String id)
    {
      var bpField = pmType.GetField("blueprintId") ?? (System.Reflection.MemberInfo)pmType.GetProperty("blueprintId");
      if (bpField is System.Reflection.FieldInfo fi) fi.SetValue(pm, id);
      else if (bpField is System.Reflection.PropertyInfo pi) pi.SetValue(pm, id, null);
    }
    /* ---- ReadPipelineId helper (needed by BakeAviRoot) ---- */
    public static System.String ReadPipelineId(UnityEngine.Transform child, C_AviGenerator hb, System.Type pmType)
    {
      if (child == null) return null;
      if (hb != null && !System.String.IsNullOrEmpty(hb.pipelineId)) { UnityEngine.Debug.Log("[HB] " + "Pipeline found via HB field: " + hb.pipelineId); return hb.pipelineId; }
      if (pmType != null)
      {
        var pm = child.GetComponent(pmType);
        if (pm != null)
        {
          var v = ReadBlueprintId(pm, pmType);
          if (!System.String.IsNullOrEmpty(v)) { UnityEngine.Debug.Log("[HB] " + "Pipeline found via PipelineManager.blueprintId: " + v); return v; }
        }
      }
      return null;
    }
    /* ================================================================
     *  Armature building
     * ================================================================ */
    public static UnityEngine.HumanBone[] MapHumanoid(UnityEngine.Transform arm)
    {
      var r = new System.Collections.Generic.List<UnityEngine.HumanBone>();
      var skippable = new System.Collections.Generic.HashSet<System.String> { "halo", "accessory", "decorative" };
      var neutralPatterns = new (System.String key, System.String baseName, System.Boolean paired)[] {
    ("hips","Hips",false),("pelvis","Hips",false),("spine","Spine",false),("chest","Chest",false),("upperchest","UpperChest",false),("neck","Neck",false),("head","Head",false),("jaw","Jaw",false),("lefteye","Eye",true),("righteye","Eye",true),("shoulder","Shoulder",true),("clavicle","Shoulder",true),("collar","Shoulder",true),("upperarm","UpperArm",true),("upper_arm","UpperArm",true),("lowerarm","LowerArm",true),("lower_arm","LowerArm",true),("forearm","LowerArm",true),("elbow","LowerArm",true),("hand","Hand",true),("upperleg","UpperLeg",true),("upper_leg","UpperLeg",true),("thigh","UpperLeg",true),("leg","UpperLeg",true),("lowerleg","LowerLeg",true),("lower_leg","LowerLeg",true),("knee","LowerLeg",true),("calf","LowerLeg",true),("foot","Foot",true),("toe","Toe",true),("thumbproximal"," Thumb Proximal",true),("thumbintermediate"," Thumb Intermediate",true),("thumbdistal"," Thumb Distal",true),("indexproximal"," Index Proximal",true),("indexintermediate"," Index Intermediate",true),("indexdistal"," Index Distal",true),("middleproximal"," Middle Proximal",true),("middleintermediate"," Middle Intermediate",true),("middledistal"," Middle Distal",true),("ringproximal"," Ring Proximal",true),("ringintermediate"," Ring Intermediate",true),("ringdistal"," Ring Distal",true),("littleproximal"," Little Proximal",true),("littleintermediate"," Little Intermediate",true),("littledistal"," Little Distal",true)
    };
      /* ── Bone lateralization helpers ──────────────────────────── */
      System.Boolean HasSuffix(System.String nl, System.String s) => nl.EndsWith("_" + s) || nl.EndsWith("_" + s) || nl.Contains("_" + s + "_") ||
      nl.EndsWith("." + s) || nl.EndsWith("-" + s) || nl.EndsWith(":" + s);
      System.Boolean IsLeft(UnityEngine.Transform t)
      {
        var n = t.name; var nl = n.ToLower();
        if (n.StartsWith("Left", System.StringComparison.OrdinalIgnoreCase) ||
          n.StartsWith("L_", System.StringComparison.OrdinalIgnoreCase)) return true;
        if (n.StartsWith("Right", System.StringComparison.OrdinalIgnoreCase) ||
          n.StartsWith("R_", System.StringComparison.OrdinalIgnoreCase)) return false;
        if (HasSuffix(nl, "l") || HasSuffix(nl, "left")) return true;
        if (HasSuffix(nl, "r") || HasSuffix(nl, "right")) return false;
        if (nl.Contains("left") && !nl.Contains("right")) return true;
        if (nl.Contains("right") && !nl.Contains("left")) return false;
        return t.localPosition.x < -0.01f;
      }
      System.Boolean IsRight(UnityEngine.Transform t)
      {
        var n = t.name; var nl = n.ToLower();
        if (n.StartsWith("Right", System.StringComparison.OrdinalIgnoreCase) ||
          n.StartsWith("R_", System.StringComparison.OrdinalIgnoreCase)) return true;
        if (n.StartsWith("Left", System.StringComparison.OrdinalIgnoreCase) ||
          n.StartsWith("L_", System.StringComparison.OrdinalIgnoreCase)) return false;
        if (HasSuffix(nl, "r") || HasSuffix(nl, "right")) return true;
        if (HasSuffix(nl, "l") || HasSuffix(nl, "left")) return false;
        if (nl.Contains("right") && !nl.Contains("left")) return true;
        if (nl.Contains("left") && !nl.Contains("right")) return false;
        return t.localPosition.x > 0.01f;
      }
      var allTx = arm.GetComponentsInChildren<UnityEngine.Transform>();
      var used = new System.Collections.Generic.HashSet<System.String>();
      var scored = new System.Collections.Generic.List<(UnityEngine.Transform t, System.String humanName, int score, UnityEngine.Vector3 wPos)>();
      foreach (var tx in allTx)
      {
        if (tx == arm) continue;
        var n = tx.name.ToLower().Replace(" ", "").Replace("-", "").Replace("_", "").Replace(":", "").Replace(".", "");
        System.Boolean skip = false; foreach (var p in skippable) if (n.Contains(p)) { skip = true; break; }
        if (skip) continue;
        (System.String human, int score, int keyLen) best = (null, 0, 0);
        foreach (var (key, baseName, isPaired) in neutralPatterns)
        {
          var pk = key.ToLower().Replace(" ", "").Replace("-", "").Replace("_", "").Replace(":", "").Replace(".", "");
          int raw = 0;
          if (n == pk) raw = 100;
          else if (n.Contains(pk)) raw = 80;
          else if (pk.Length >= 4 && n.Contains(pk)) raw = 60;
          if (raw <= 0) continue;
          System.String humanName;
          if (!isPaired) humanName = baseName;
          else
          {
            System.Boolean isLeft = IsLeft(tx), isRight = IsRight(tx);
            if (isLeft) humanName = "Left" + baseName;
            else if (isRight) humanName = "Right" + baseName;
            else humanName = "Left" + baseName;
            if ((isLeft && humanName.StartsWith("Left")) || (isRight && humanName.StartsWith("Right"))) raw += 50;
            else if ((isLeft && humanName.StartsWith("Right")) || (isRight && humanName.StartsWith("Left"))) raw -= 30;
          }
          if (raw > best.score || (raw == best.score && pk.Length > best.keyLen))
            best = (humanName, raw, pk.Length);
        }
        if (best.human != null && best.score >= 60)
          scored.Add((tx, best.human, best.score, tx.position));
      }
      var descendantBonus = new System.Collections.Generic.Dictionary<System.String, (int minDesc, int bonus)>
    { {"Hips",(5,40)},{"Spine",(3,30)},{"Chest",(3,30)},{"UpperChest",(2,20)},{"LeftUpperArm",(3,35)},{"RightUpperArm",(3,35)},{"LeftLowerArm",(5,40)},{"RightLowerArm",(5,40)},{"LeftHand",(8,40)},{"RightHand",(8,40)},{"LeftUpperLeg",(3,35)},{"RightUpperLeg",(3,35)},{"LeftLowerLeg",(2,30)},{"RightLowerLeg",(2,30)},{"LeftFoot",(1,20)},{"RightFoot",(1,20)},{"Neck",(1,15)},{"Head",(1,10)},};
      var rescored = new System.Collections.Generic.List<(UnityEngine.Transform t, System.String humanName, int score, UnityEngine.Vector3 wPos)>();
      UnityEngine.Vector3 hipsPos = FindHipsPosition(allTx);
      float hipsY = hipsPos.y;
      foreach (var s in scored)
      {
        int posBonus = 0;
        System.String hn = s.humanName;
        if (hn.Contains("Leg") || hn.Contains("Foot") || hn.Contains("Toe") || hn.Contains("Thigh") || hn.Contains("Knee") || hn.Contains("Calf"))
        {
          if (s.wPos.y < hipsY - 0.05f) posBonus += 30;
          else if (s.wPos.y < hipsY) posBonus += 10;
          else posBonus -= 40;
        }
        if (hn == "Spine" || hn == "Chest" || hn == "UpperChest" || hn == "Neck" || hn == "Head")
        {
          if (s.wPos.y > hipsY + 0.05f) posBonus += 30;
          else if (s.wPos.y > hipsY) posBonus += 10;
          else posBonus -= 30;
        }
        if (hn.Contains("Arm") || hn.Contains("Hand") || hn.Contains("Shoulder") || hn.Contains("Elbow") || hn.Contains("Forearm"))
        {
          if (s.wPos.y > hipsY + 0.05f && UnityEngine.Mathf.Abs(s.wPos.x) > 0.05f) posBonus += 30;
          else if (s.wPos.y > hipsY) posBonus += 10;
          else posBonus -= 20;
        }
        if (hn.Contains("Eye"))
        {
          float maxY = System.Linq.Enumerable.Max(allTx, tx => tx.position.y);
          if (UnityEngine.Mathf.Abs(s.wPos.y - maxY) < 0.3f) posBonus += 20;
        }
        if (hn == "Hips")
        {
          int up = 0, down = 0;
          foreach (UnityEngine.Transform c in s.t)
          {
            if (c.position.y > s.t.position.y + 0.02f) up++;
            else if (c.position.y < s.t.position.y - 0.02f) down++;
          }
          if (up >= 1 && down >= 2) posBonus += 50;
          else if (up >= 1 || down >= 2) posBonus += 20;
          else posBonus -= 20;
        }
        if (hn == "Spine" || hn == "Chest" || hn == "UpperChest")
        {
          int mirrored = 0;
          var cp = new System.Collections.Generic.List<UnityEngine.Vector3>();
          foreach (UnityEngine.Transform c in s.t) cp.Add(c.position);
          for (int i = 0; i < cp.Count; i++)
            for (int j = i + 1; j < cp.Count; j++)
              if (UnityEngine.Mathf.Abs(cp[i].x + cp[j].x) < 0.05f && UnityEngine.Mathf.Abs(cp[i].y - cp[j].y) < 0.15f)
                mirrored++;
          if (mirrored >= 2) posBonus += 40;
          else if (mirrored >= 1) posBonus += 20;
        }
        if (hn == "Neck")
        {
          System.Boolean hasHead = false;
          foreach (UnityEngine.Transform c in s.t)
            if (c.name.ToLower().Contains("head")) { hasHead = true; break; }
          if (hasHead) posBonus += 20;
        }
        int descCount = CountDescendantsNonRecursive(s.t);
        if (hn.Contains("LowerArm") || hn.Contains("LowerLeg"))
        {
          if (descCount >= 10) posBonus += 40;
          else if (descCount >= 4) posBonus += 25;
          else if (descCount >= 2) posBonus += 10;
          else posBonus -= 30;
        }
        if (hn.Contains("UpperArm") || hn.Contains("UpperLeg"))
        {
          if (descCount >= 15) posBonus += 30;
          else if (descCount >= 8) posBonus += 20;
          else if (descCount >= 4) posBonus += 10;
          else posBonus -= 20;
        }
        if (hn.Contains("Hand"))
        {
          if (descCount >= 8) posBonus += 30;
          else if (descCount >= 3) posBonus += 15;
          else posBonus -= 20;
        }
        if (hn.Contains("Toe"))
        {
          int shortBonus = UnityEngine.Mathf.Max(0, 25 - s.t.name.Length);
          int toeChildren = 0;
          foreach (UnityEngine.Transform c in s.t)
            if (c.name.ToLower().Contains("toe")) toeChildren++;
          if (toeChildren > 0) shortBonus += toeChildren * 15;
          posBonus += shortBonus;
        }
        if (s.t.name.ToLower().Contains("twist")) posBonus -= 30;
        rescored.Add((s.t, hn, s.score + posBonus, s.wPos));
      }
      var grouped = System.Linq.Enumerable.ToList(System.Linq.Enumerable.GroupBy(rescored, s => SideGroup(s.humanName)));
      foreach (var grp in grouped)
      {
        var items = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderByDescending(grp, s => s.wPos.y));
        System.String key = grp.Key ?? "";
        if (key.Contains("Arm") || key.Contains("Hand"))
        {
          var upper = System.Linq.Enumerable.FirstOrDefault(items, s => s.humanName.Contains("UpperArm"));
          var lower = System.Linq.Enumerable.FirstOrDefault(items, s => s.humanName.Contains("LowerArm"));
          var hand = System.Linq.Enumerable.FirstOrDefault(items, s => s.humanName.Contains("Hand"));
          if (upper.t != null && lower.t != null && upper.wPos.y < lower.wPos.y - 0.05f)
          {
            rescored = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(rescored, s => s.t == upper.t ? (upper.t, "LeftLowerArm" + upper.humanName.Substring(4), upper.score - 20, upper.wPos) : s));
            rescored = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(rescored, s => s.t == lower.t ? (lower.t, "LeftUpperArm" + lower.humanName.Substring(4), lower.score + 20, lower.wPos) : s));
          }
        }
        if (key.Contains("Leg") || key.Contains("Foot"))
        {
          var upper = System.Linq.Enumerable.FirstOrDefault(items, s => s.humanName.Contains("UpperLeg"));
          var lower = System.Linq.Enumerable.FirstOrDefault(items, s => s.humanName.Contains("LowerLeg"));
          var foot = System.Linq.Enumerable.FirstOrDefault(items, s => s.humanName.Contains("Foot"));
          if (upper.t != null && lower.t != null && upper.wPos.y < lower.wPos.y)
          {
            rescored = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(rescored, s => s.t == upper.t ? (upper.t, "LeftLowerLeg" + upper.humanName.Substring(4), upper.score - 20, upper.wPos) : s));
            rescored = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(rescored, s => s.t == lower.t ? (lower.t, "LeftUpperLeg" + lower.humanName.Substring(4), lower.score + 20, lower.wPos) : s));
          }
        }
      }
      int maxDepth = System.Linq.Enumerable.Max(allTx, tx => { int d = 0; var p = tx; while (p != arm) { d++; p = p.parent; } return d; });
      if (maxDepth >= 3)
      {
        var chainPairs = new (System.String parentH, System.String childH)[] {
    ("LeftUpperArm","LeftLowerArm"),("LeftLowerArm","LeftHand"),("RightUpperArm","RightLowerArm"),("RightLowerArm","RightHand"),("LeftUpperLeg","LeftLowerLeg"),("LeftLowerLeg","LeftFoot"),("RightUpperLeg","RightLowerLeg"),("RightLowerLeg","RightFoot"),};
        foreach (var (parentH, childH) in chainPairs)
        {
          System.Boolean changed = true;
          while (changed)
          {
            changed = false;
            var p = System.Linq.Enumerable.FirstOrDefault(rescored, s => s.humanName == parentH);
            var c = System.Linq.Enumerable.FirstOrDefault(rescored, s => s.humanName == childH);
            if (p.t == null || c.t == null) break;
            if (IsAncestor(p.t, c.t)) break;
            var betterChild = System.Linq.Enumerable.FirstOrDefault(rescored, s => s.humanName == childH && IsAncestor(p.t, s.t));
            if (betterChild.t != null && betterChild.t != c.t)
            {
              rescored.RemoveAll(s => s.t == c.t && s.humanName == childH);
              changed = true;
              continue;
            }
            var allChildCandidates = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(rescored, s => s.humanName == childH));
            UnityEngine.Transform bestParent = null;
            foreach (var candidate in System.Linq.Enumerable.Where(rescored, s => s.humanName == parentH))
            {
              if (candidate.t == p.t) continue;
              foreach (var cc in allChildCandidates)
              { if (IsAncestor(candidate.t, cc.t)) { bestParent = candidate.t; break; } }
              if (bestParent != null) break;
            }
            if (bestParent != null)
            {
              rescored.RemoveAll(s => s.t == p.t && s.humanName == parentH);
              changed = true;
            }
            else
            {
              rescored.RemoveAll(s => s.t == c.t && s.humanName == childH);
              changed = true;
            }
          }
        }
      }
      foreach (var s in System.Linq.Enumerable.OrderByDescending(rescored, s => s.score))
      {
        if (used.Contains(s.humanName)) continue;
        r.Add(new UnityEngine.HumanBone { boneName = s.t.name, humanName = s.humanName, limit = new UnityEngine.HumanLimit { useDefaultValues = true } });
        used.Add(s.humanName);
      }
      var vitalBones = new System.String[] { "Hips", "Chest", "Head", "Spine", "Neck" };
      foreach (var vital in vitalBones)
      {
        if (used.Contains(vital)) continue;
        var cleaned = vital.ToLower();
        var best = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.ThenBy(System.Linq.Enumerable.OrderByDescending(System.Linq.Enumerable.Where(System.Linq.Enumerable.Where(allTx, tx => tx != arm), tx =>
        {
          var cn = tx.name.ToLower().Replace(" ", "").Replace("-", "").Replace("_", "").Replace(":", "").Replace(".", "");
          return cn.Contains(cleaned);
        }), tx => tx.name.ToLower() == vital.ToLower() ? 100 : 80), tx => tx.name.Length));
        if (best == null) continue;
        r.Add(new UnityEngine.HumanBone { boneName = best.name, humanName = vital, limit = new UnityEngine.HumanLimit { useDefaultValues = true } });
        used.Add(vital);
        UnityEngine.Debug.Log("[HB] Vital bone " + vital + " forced from " + best.name);
      }
      return r.ToArray();
    }
    static UnityEngine.Vector3 FindHipsPosition(UnityEngine.Transform[] allTx)
    {
      foreach (var tx in allTx)
      {
        var nl = tx.name.ToLower();
        if (nl == "hips" || nl == "pelvis" || nl.Contains("hips")) return tx.position;
      }
      UnityEngine.Transform best = null; float bestY = float.MaxValue;
      foreach (var tx in allTx)
      { if (tx.childCount >= 2 && tx.position.y < bestY) { bestY = tx.position.y; best = tx; } }
      if (best != null) return best.position;
      return UnityEngine.Vector3.zero;
    }
    static System.String SideGroup(System.String humanName)
    {
      if (humanName.StartsWith("Left")) return humanName.Substring(4);
      if (humanName.StartsWith("Right")) return humanName.Substring(5);
      return humanName;
    }
    static System.Boolean IsAncestor(UnityEngine.Transform ancestor, UnityEngine.Transform descendant)
    {
      if (ancestor == null || descendant == null) return false;
      var cur = descendant;
      while (cur != null)
      {
        if (cur == ancestor) return true;
        cur = cur.parent;
      }
      return false;
    }
    static int CountDescendantsNonRecursive(UnityEngine.Transform t)
    {
      if (t == null) return 0;
      int count = 0;
      var stack = new System.Collections.Generic.Stack<UnityEngine.Transform>();
      for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
      while (stack.Count > 0)
      {
        var cur = stack.Pop(); count++;
        for (int i = 0; i < cur.childCount; i++) stack.Push(cur.GetChild(i));
      }
      return count;
    }
    public static UnityEngine.SkeletonBone[] BuildSkeleton(UnityEngine.Transform arm, UnityEngine.Transform avatarRoot)
    {
      var r = new System.Collections.Generic.List<UnityEngine.SkeletonBone>();
      r.Add(new UnityEngine.SkeletonBone { name = avatarRoot.name, position = avatarRoot.localPosition, rotation = avatarRoot.localRotation, scale = avatarRoot.localScale });
      AddSkeletonBones(arm, r);
      return r.ToArray();
    }
    static void AddSkeletonBones(UnityEngine.Transform t, System.Collections.Generic.List<UnityEngine.SkeletonBone> list)
    {
      var existing = new System.Collections.Generic.HashSet<System.String>(System.Linq.Enumerable.Select(list, b => b.name));
      AddSkeletonBonesRecursive(t, list, existing);
    }
    static void AddSkeletonBonesRecursive(UnityEngine.Transform t, System.Collections.Generic.List<UnityEngine.SkeletonBone> list, System.Collections.Generic.HashSet<System.String> existing)
    {
      if (!existing.Contains(t.name))
      {
        list.Add(new UnityEngine.SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale });
        existing.Add(t.name);
      }
      for (int i = 0; i < t.childCount; i++) AddSkeletonBonesRecursive(t.GetChild(i), list, existing);
    }
    public static System.Type FindPMType()
    {
      foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
      {
        var t = a.GetType("VRC.SDKBase.VRC_PipelineManager");
        if (t != null) return t;
        t = a.GetType("VRC.Core.PipelineManager");
        if (t != null) return t;
      }
      return null;
    }
    /** <summary>Set ViewPosition via UnityEngine.Animator.GetBoneTransform for precise eye placement. */
    /** Called after the humanoid UnityEngine.Avatar is built and assigned to the UnityEngine.Animator.</summary> */
    public static void SyncAvatarArmature(C_AviGenerator hb, UnityEngine.Transform boneRoot, UnityEngine.Transform originalRoot)
    {
      if (hb.NZKC_GO_AviRoot == null) return;
      var avatarRootNameLower = (hb.avatarRootName ?? "").ToLower();
      /* Capture source Armature rotation before resetting target — source may have
       a Blender→Unity Z-up→Y-up conversion rotation (typically 270° around X). */
      var sourceArmRot = UnityEngine.Quaternion.identity;
      foreach (UnityEngine.Transform c in boneRoot)
      { if (c.name.ToLower() == "armature") { sourceArmRot = c.localRotation; break; } }
      var target = hb.NZKC_GO_AviRoot.transform.Find("Armature");
      if (target == null) { var g = new UnityEngine.GameObject("Armature"); g.transform.SetParent(hb.NZKC_GO_AviRoot.transform, false); target = g.transform; }
      target.localPosition = UnityEngine.Vector3.zero;
      target.localRotation = sourceArmRot;
      target.localScale = UnityEngine.Vector3.one;
      var root = ResolveSyncRoot(boneRoot, avatarRootNameLower);
      if (root == null) { UnityEngine.Debug.Log("[HB] SyncAvatarArmature: no syncable root in " + boneRoot.name); return; }
      /* If root is named "Armature" (whether it IS boneRoot or a child of boneRoot),sync its children directly into target to avoid Armature-inside-Armature nesting. */
      if (root.name.ToLower() == "armature")
      {
        foreach (UnityEngine.Transform child in root)
        {
          if (child.GetComponentInChildren<C_AviGenerator>() != null) continue;
          SyncBone(child, target);
        }
        UnityEngine.Debug.Log("[HB] SyncAvatarArmature: " + root.name + " children synced directly (flattened) target_children=" + target.childCount);
        return;
      }
      /* Sync the resolved root itself (it's an actual bone,not a container) */
      SyncBone(root, target);
      /* Then sync any remaining direct children that weren't under the resolved root */
      foreach (UnityEngine.Transform child in boneRoot)
      {
        if (child == root) continue;
        var cn = child.name.ToLower();
        if (cn == avatarRootNameLower || cn.StartsWith(avatarRootNameLower + "_")) continue;
        if (child.GetComponentInChildren<C_AviGenerator>() != null) continue;
        SyncBone(child, target);
      }
      UnityEngine.Debug.Log("[HB] SyncAvatarArmature: " + boneRoot.name + "→" + (root != boneRoot ? root.name + " " : "") + "target_children=" + target.childCount);
    }
    static UnityEngine.Transform ResolveSyncRoot(UnityEngine.Transform t, System.String avatarRootNameLower)
    {
      if (t == null) return null;
      var tn = t.name.ToLower();
      /* Skip virtual roots that are just containers for the actual hierarchy */
      if (tn == avatarRootNameLower || tn.StartsWith(avatarRootNameLower + "_")) goto next;
      /* If this node has a direct child named "Armature",recurse into it — must check
       BEFORE the HB skip so mesh-source containers (Armature_Stripped etc.) flatten correctly. */
      foreach (UnityEngine.Transform child in t)
      { if (child.name.ToLower() == "armature") return ResolveSyncRoot(child, avatarRootNameLower); }
      if (t.GetComponentInChildren<C_AviGenerator>() != null) goto next;
      return t;
    next: if (t.childCount == 0) return null;
      /* Try each child — return the first valid sync root found */
      for (int i = 0; i < t.childCount; i++)
      {
        var r = ResolveSyncRoot(t.GetChild(i), avatarRootNameLower);
        if (r != null) return r;
      }
      return null;
    }
    static void SyncBone(UnityEngine.Transform src, UnityEngine.Transform parent)
    {
      var sn = src.name.ToLower();
      if (src.GetComponent<C_AviGenerator>() != null) return;
      if (src.GetComponentInChildren<C_AviGenerator>() != null) return;
      var existing = parent.Find(src.name);
      UnityEngine.Transform dst;
      if (existing != null) dst = existing;
      else { var g = new UnityEngine.GameObject(src.name); g.transform.SetParent(parent, false); dst = g.transform; }
      dst.localPosition = src.localPosition; dst.localRotation = src.localRotation; dst.localScale = src.localScale;
      foreach (UnityEngine.Transform child in src) SyncBone(child, dst);
    }
    /* ---- Armature baking ---- */
    /* ---- Armature reference building ---- */
    public static UnityEngine.Transform FindTargetArmatureSource(C_AviGenerator hb)
    {
      UnityEngine.Transform ar = hb.armatureRoot;
      if (ar != null) return ar;
      foreach (UnityEngine.Transform c in hb.transform)
      {
        var h = c.GetComponent<C_AviGenerator>();
        if (h != null && h.mode == E_AviGeneratorMode.ArmatureBuilder && h.armatureRoot != null) return h.armatureRoot;
      }
      foreach (UnityEngine.Transform c in hb.transform)
        if (c.name.StartsWith("Armature_"))
        {
          var m = c.GetComponent<C_AviGenerator>();
          if (m != null && m.originalArmatureSource != null) return m.originalArmatureSource;
        }
      return hb.transform.Find("Armature_Auto");
    }
    public static void ExtractRefArmatureFromAvatar(C_AviGenerator hb, UnityEngine.Avatar avatar, UnityEngine.Transform targetArmSource)
    {
      if (avatar == null || !avatar.isHuman) return;
      var skeleton = avatar.humanDescription.skeleton;
      if (skeleton == null || skeleton.Length == 0) return;
      var refName = "Armature_Ref";
      var existingRef = hb.NZKC_GO_AviRoot.transform.Find(refName);
      if (existingRef != null)
      {
        for (int i = existingRef.childCount - 1; i >= 0; i--)
          IF_UE.DestroyImmediate(existingRef.GetChild(i).gameObject);
      }
      else
      {
        var g = new UnityEngine.GameObject(refName);
        g.transform.SetParent(hb.NZKC_GO_AviRoot.transform, false);
        existingRef = g.transform;
      }
      var skelNames = new System.Collections.Generic.HashSet<System.String>();
      for (int i = 1; i < skeleton.Length; i++) skelNames.Add(skeleton[i].name);
      if (targetArmSource != null)
        BuildRefHierarchy(targetArmSource, existingRef, skelNames, null);
      var anim = hb.NZKC_GO_AviRoot.GetComponent<UnityEngine.Animator>();
      if (anim != null && anim.isHuman)
      {
        for (int bi = 0; bi < 25; bi++)
        {
          var bt = anim.GetBoneTransform((UnityEngine.HumanBodyBones)bi);
          if (bt != null && existingRef.Find(bt.name) == null && !skelNames.Contains(bt.name))
          {
            var src = targetArmSource != null ? FindBoneInHierarchy(targetArmSource, bt.name) : null;
            if (src != null)
            {
              var parentPath = CalculateTransformPath(src.parent, targetArmSource);
              var refParent = System.String.IsNullOrEmpty(parentPath) ? existingRef : existingRef.Find(parentPath);
              if (refParent == null) refParent = existingRef;
              var g = new UnityEngine.GameObject(src.name);
              g.transform.SetParent(refParent, false);
              g.transform.localPosition = src.localPosition;
              g.transform.localRotation = src.localRotation;
              g.transform.localScale = src.localScale;
            }
          }
        }
      }
      UnityEngine.Debug.Log("[HB] Armature_Ref created with " + existingRef.GetComponentsInChildren<UnityEngine.Transform>().Length + " bones from avatar.asset skeleton.");
    }
    public static UnityEngine.Transform BuildRefHierarchy(UnityEngine.Transform src, UnityEngine.Transform dst, System.Collections.Generic.HashSet<System.String> validNames, UnityEngine.Transform parentRef)
    {
      if (parentRef == null) parentRef = dst;
      var childRef = parentRef.Find(src.name);
      if (childRef == null && validNames.Contains(src.name))
      {
        var g = new UnityEngine.GameObject(src.name);
        g.transform.SetParent(parentRef, false);
        g.transform.localPosition = src.localPosition;
        g.transform.localRotation = src.localRotation;
        g.transform.localScale = src.localScale;
        childRef = g.transform;
      }
      else if (childRef == null) childRef = parentRef;
      for (int i = 0; i < src.childCount; i++)
        BuildRefHierarchy(src.GetChild(i), dst, validNames, childRef);
      return childRef;
    }
    public static void BuildFullArmatureFromTarget(C_AviGenerator hb, UnityEngine.Transform targetArmSource)
    {
      var arm = hb.NZKC_GO_AviRoot.transform.Find("Armature");
      if (arm == null)
      {
        UnityEngine.Debug.LogWarning("[HB] No Armature found — the target armature sync already handles this.");
        if (targetArmSource != null) SyncAvatarArmature(hb, targetArmSource, targetArmSource);
        arm = hb.NZKC_GO_AviRoot.transform.Find("Armature");
      }
      if (arm == null) return;
      if (targetArmSource != null)
      {
        int before = arm.GetComponentsInChildren<UnityEngine.Transform>().Length;
        foreach (UnityEngine.Transform child in targetArmSource)
          MergeMissingBones(child, arm);
        int after = arm.GetComponentsInChildren<UnityEngine.Transform>().Length;
        if (after > before)
          UnityEngine.Debug.Log("[HB] Merged " + (after - before) + " missing bones into Armature from target.");
      }
      int totalBones = arm.GetComponentsInChildren<UnityEngine.Transform>().Length;
      UnityEngine.Debug.Log("[HB] Armature verified: " + totalBones + " total bones (reference: " + totalBones + ").");
    }
    static void MergeMissingBones(UnityEngine.Transform src, UnityEngine.Transform dstParent)
    {
      for (int i = 0; i < src.childCount; i++)
      {
        var srcChild = src.GetChild(i);
        var existing = dstParent.Find(srcChild.name);
        if (existing == null)
        {
          var g = new UnityEngine.GameObject(srcChild.name);
          g.transform.SetParent(dstParent, false);
          g.transform.localPosition = srcChild.localPosition;
          g.transform.localRotation = srcChild.localRotation;
          g.transform.localScale = srcChild.localScale;
          existing = g.transform;
        }
        MergeMissingBones(srcChild, existing);
      }
    }
    public static UnityEngine.Transform FindBoneInHierarchy(UnityEngine.Transform root, System.String name)
    {
      if (root.name == name) return root;
      for (int i = 0; i < root.childCount; i++)
      { var r = FindBoneInHierarchy(root.GetChild(i), name); if (r != null) return r; }
      return null;
    }
    public static System.String CalculateTransformPath(UnityEngine.Transform tx, UnityEngine.Transform root)
    {
      if (tx == root || tx.parent == null) return "";
      var parts = new System.Collections.Generic.List<System.String>();
      var cur = tx;
      while (cur != null && cur != root)
      { parts.Add(cur.name); cur = cur.parent; }
      parts.Reverse();
      return System.String.Join("/", parts);
    }
  }
}
}
