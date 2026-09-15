namespace NZK
{
public static partial class Core {
public static partial class MenuItems {
  public static System.Collections.Generic.List<UnityEngine.AnimationClip>
  AnimationClips = new System.Collections.Generic.List<UnityEngine.AnimationClip>();
  public const string ApplicationDataPath = "Assets";
  public static string nan = "NaN";
  public static string cnan = ": " + nan;
  public static string xn = "x" + cnan;
  public static string yn = "y" + cnan;
  public static string zn = "z" + cnan;
  public static bool FixAnimationClipScale(UnityEngine.AnimationClip clip)
  {
    if (clip == null) return false;
    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    string fullPath = NZK.E.PC(ApplicationDataPath, assetPath.Substring("Assets/".Length));
    if (!System.IO.File.Exists(fullPath))
    {
      UnityEngine.Debug.LogError("File not found: " + fullPath, clip);
      return false;
    }
    string content = System.IO.File.ReadAllText(fullPath);
    bool wasModified = false;
    if (content.Contains("m_ScaleCurves:") || content.Contains("m_EditorCurves:"))
    {
      bool fixedVector = false, fixedScalar = false;
      string vectorResult = ReplaceVectorScaleValue(content);
      if (!string.IsNullOrEmpty(vectorResult) && !string.Equals(vectorResult, content))
      { fixedVector = true; content = vectorResult;}
      string scalarResult = FixEditorCurveScale(content);
      if (!string.IsNullOrEmpty(scalarResult) && !string.Equals(scalarResult, content))
      {
        fixedScalar = true;
        content = scalarResult;
      }
      wasModified = fixedVector || fixedScalar;
      if (wasModified)
        UnityEngine.Debug.Log("Fixed zero scale keyframe in: "
          + (fixedVector && fixedScalar ? "scale+editor curves " : fixedVector ? "scale curves " : "editor curves ")
          + clip.name);
    }
    if (!wasModified)
    {
      UnityEngine.Debug.Log("No zero scale values found in: " + clip.name);
      return false;
    }
    System.IO.File.WriteAllText(fullPath, content);
    UnityEngine.Debug.Log("Fixed zero scales in: " + clip.name);
    return true;
  }
  public static bool HasSelectedAnimations()
  {
    string[] guids = UnityEditor.Selection.assetGUIDs;
    if (guids == null || guids.Length == 0) return false;
    foreach (string guid in guids)
    {
      string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
      UnityEngine.AnimationClip clip =
        UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
      if (clip != null) return true;
    }
    return false;
  }
  
  public static string ReplaceVectorScaleValue(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    var regex = new System.Text.RegularExpressions.Regex(
      "value:\\s*\\{x:\\s*[-0-9.eE]*0(?:\\.0+)?[^,}]*,\\s*y:\\s*[-0-9.eE]*0(?:\\.0+)?[^,}]*,\\s*z:\\s*[-0-9.eE]*0(?:\\.0+)?[^,}]*\\}",
      System.Text.RegularExpressions.RegexOptions.Multiline);
    return regex.Replace(input, m => "value: {x: NaN, y: NaN, z: NaN}");
  }
  // m_EditorCurves rows are per-axis (attribute: m_LocalScale.x/.y/.z), one curve
  // per block but consecutive x/y/z of the SAME path sit adjacent. A scale is
  // zero only when all three channels are 0 for the same key index. Scan blocks,
  // pair xyz triples sharing one path, NaN every key column where all three == 0.
  public static string FixEditorCurveScale(string content)
  {
    if (string.IsNullOrEmpty(content)) return content;
    int e0 = content.IndexOf("  m_EditorCurves:", System.StringComparison.Ordinal);
    if (e0 < 0) return content;
    e0 += "  m_EditorCurves:".Length;
    int e1 = content.IndexOf("  m_FloatCurves:", e0, System.StringComparison.Ordinal);
    if (e1 < 0) e1 = content.Length;
    var tok = new System.Text.RegularExpressions.Regex("(?m)^  - serializedVersion: 2\n");
    var msStart = new System.Collections.Generic.List<int>();
    foreach (System.Text.RegularExpressions.Match mm in tok.Matches(content, e0))
    {
      int g = e0 + mm.Index;
      if (g >= e1) break;
      msStart.Add(g);
    }
    if (msStart.Count == 0) return content;
    System.Collections.Generic.List<string> axes =
      new System.Collections.Generic.List<string>();
    System.Collections.Generic.List<string> paths =
      new System.Collections.Generic.List<string>();
    System.Collections.Generic.List<System.Collections.Generic.List<int>>
      valStarts = new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
    var valRegex = new System.Text.RegularExpressions.Regex("(?m)^\\s*value:\\s*(\\S+)");
    for (int i = 0; i < msStart.Count; i++)
    {
      int s = msStart[i];
      int t = (i + 1 < msStart.Count) ? msStart[i + 1] : e1;
      string seg = content.Substring(s, t - s);
      var am = System.Text.RegularExpressions.Regex.Match(seg, "m_LocalScale\\.([xyz])");
      var pm = System.Text.RegularExpressions.Regex.Match(seg, "(?m)^  path:\\s*(\\S+)");
      axes.Add(am.Success ? am.Groups[1].Value : "");
      paths.Add(pm.Success ? pm.Groups[1].Value : "");
      var lineStarts = new System.Collections.Generic.List<int>();
      foreach (System.Text.RegularExpressions.Match vm in valRegex.Matches(seg))
      {
        int atV = (s - e0) + vm.Index + vm.Value.IndexOf("value:", System.StringComparison.Ordinal);
        lineStarts.Add(atV);
      }
      valStarts.Add(lineStarts);
    }
    var mark = new System.Collections.Generic.HashSet<int>();
    int i2 = 0;
    while (i2 + 2 < axes.Count)
    {
      bool trip = axes[i2] == "x" && axes[i2 + 1] == "y" && axes[i2 + 2] == "z"
            && paths[i2] == paths[i2 + 1] && paths[i2] == paths[i2 + 2];
      if (!trip) { i2++; continue; }
      for (int k = 0; k < valStarts[i2].Count && k < valStarts[i2 + 1].Count && k < valStarts[i2 + 2].Count; k++)
      {
        bool allZero = lineZero(content, valStarts[i2][k] + e0)
              && lineZero(content, valStarts[i2 + 1][k] + e0)
              && lineZero(content, valStarts[i2 + 2][k] + e0);
        if (allZero)
        {
          mark.Add(valStarts[i2][k] + e0);
          mark.Add(valStarts[i2 + 1][k] + e0);
          mark.Add(valStarts[i2 + 2][k] + e0);
        }
      }
      i2 += 3;
    }
    if (mark.Count == 0) return content;
    var sb = new System.Text.StringBuilder(content);
    int[] pos = new int[mark.Count];
    mark.CopyTo(pos);
    System.Array.Sort(pos);
    for (int q = pos.Length - 1; q >= 0; q--)
    {
      int p = pos[q];
      int e = p;
      while (e < content.Length && content[e] != '\n') e++;
      while (e > p && (content[e - 1] == ' ' || content[e - 1] == '\t' || content[e - 1] == '\r')) e--;
      sb.Remove(p, e - p);
      sb.Insert(p, "value: NaN");
    }
    return sb.ToString();
  }
  private static bool lineZero(string content, int p)
  {
    int s = p + "value:".Length;
    while (s < content.Length && (content[s] == ' ' || content[s] == '\t')) s++;
    int t = s;
    while (t < content.Length && content[t] != '\n' && content[t] != ',') t++;
    string txt = content.Substring(s, t - s).Trim();
    float v;
    if (float.TryParse(txt, System.Globalization.NumberStyles.Float,
               System.Globalization.CultureInfo.InvariantCulture, out v))
      return v == 0f;
    return txt == "0";
  }
  public static bool DetectAnimationHasScaleValueOf(float x, float y, float z)
  {
    UnityEngine.Vector3 targetScale = new UnityEngine.Vector3(x, y, z);
    foreach (UnityEngine.AnimationClip animationClip in AnimationClips)
    {
      if (animationClip != null && targetScale == UnityEngine.Vector3.zero)
        return true;
    }
    return false;
  }
  public static FileContents GetFileContents(string path)
  {
    return new FileContents(System.IO.File.ReadAllText(path));
  }
  
  [UnityEditor.MenuItem("Assets/NZK Toolkit/Check Zero Scale Animations", false, 31)]
  public static void CheckZeroScaleAnimationsAsset()
  {
    string[] guids = UnityEditor.Selection.assetGUIDs;
    if (guids == null || guids.Length == 0)
    {
      NZK.E.Dd("No Selection", "Select one or more animation clip files.", "OK");
      return;
    }
    System.Collections.Generic.List<string> zeroScaleClips =
      new System.Collections.Generic.List<string>();
    foreach (string guid in guids)
    {
      string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
      UnityEngine.AnimationClip clip =
        UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
      if (clip == null) continue;
      string assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
      string fullPath = NZK.E.PC(ApplicationDataPath,
                     assetPath.Substring("Assets/".Length));
      if (!System.IO.File.Exists(fullPath)) continue;
      string content = System.IO.File.ReadAllText(fullPath);
      bool hasZero = content.Contains("value: {x: 0");
      if (hasZero) zeroScaleClips.Add(clip.name);
    }
    string message = zeroScaleClips.Count > 0
      ? string.Join("\n", zeroScaleClips)
      : "No zero scale curves found.";
    NZK.E.Dd("Zero Scale Animation Check",
        "Found zero scale curves in:\n" + message, "OK");
  }
}
}
}
