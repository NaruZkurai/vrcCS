#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Fix
  { public static System.Boolean FixAnimationClipScale(UnityEngine.AnimationClip clip)
  { if (clip == null) return false;
    System.String assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
    if (System.String.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/",System.StringComparison.Ordinal)) return false;
    System.String fullPath = NaNimate.Paths.Abs(assetPath);
    if (!System.IO.File.Exists(fullPath)) return false;
    System.String content = System.IO.File.ReadAllText(fullPath);
    System.Boolean wasModified = false;
    if (content.Contains("m_ScaleCurves:") || content.Contains("m_EditorCurves:"))
    { System.Boolean fixedVector = false, fixedScalar = false;
      System.String vectorResult = NaNimate.Fix.ReplaceVectorScaleValue(content);
      if (!System.String.IsNullOrEmpty(vectorResult) && !System.String.Equals(vectorResult,content)) { fixedVector = true; content = vectorResult; }
      System.String scalarResult = NaNimate.Fix.FixEditorCurveScale(content);
      if (!System.String.IsNullOrEmpty(scalarResult) && !System.String.Equals(scalarResult,content)) { fixedScalar = true; content = scalarResult; }
      wasModified = fixedVector || fixedScalar; }
    if (!wasModified) return false;
    System.IO.File.WriteAllText(fullPath,content);
    UnityEditor.AssetDatabase.ImportAsset(assetPath,UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate);
    return true; }
  public static System.String ReplaceVectorScaleValue(System.String input)
  { if (System.String.IsNullOrEmpty(input)) return input;
    var regex = new System.Text.RegularExpressions.Regex("value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}",System.Text.RegularExpressions.RegexOptions.Multiline);
    return regex.Replace(input, m =>
      NZK.Core.Yaml.IsZero(m.Groups[1].Value) &&
      NZK.Core.Yaml.IsZero(m.Groups[2].Value) &&
      NZK.Core.Yaml.IsZero(m.Groups[3].Value)
        ? "value: {x: NaN, y: NaN, z: NaN}" : m.Value); }
  public static System.String FixEditorCurveScale(System.String content)
  { if (System.String.IsNullOrEmpty(content)) return content;
    int e0 = content.IndexOf("  m_EditorCurves:",System.StringComparison.Ordinal);
    if (e0 < 0) return content;
    e0 += "  m_EditorCurves:".Length;
    int e1 = content.IndexOf("  m_FloatCurves:",e0,System.StringComparison.Ordinal);
    if (e1 < 0) e1 = content.Length;
    var tok = new System.Text.RegularExpressions.Regex("(?m)^  - serializedVersion: 2\n");
    var msStart = new System.Collections.Generic.List<int>();
    foreach (System.Text.RegularExpressions.Match mm in tok.Matches(content,e0))
    { int g = e0 + mm.Index; if (g >= e1) break; msStart.Add(g); }
    if (msStart.Count == 0) return content;
    System.Collections.Generic.List<System.String> axes = new System.Collections.Generic.List<System.String>();
    System.Collections.Generic.List<System.String> paths = new System.Collections.Generic.List<System.String>();
    System.Collections.Generic.List<System.Collections.Generic.List<int>> valStarts = new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
    var valRegex = new System.Text.RegularExpressions.Regex("(?m)^\\s*value:\\s*(\\S+)");
    for (int i = 0; i < msStart.Count; i++)
    { int s = msStart[i]; int t = (i + 1 < msStart.Count) ? msStart[i + 1] : e1;
      System.String seg = content.Substring(s,t - s);
      var am = System.Text.RegularExpressions.Regex.Match(seg,"m_LocalScale\\.([xyz])");
      var pm = System.Text.RegularExpressions.Regex.Match(seg,"(?m)^  path:\\s*(\\S+)");
      axes.Add(am.Success ? am.Groups[1].Value : ""); paths.Add(pm.Success ? pm.Groups[1].Value : "");
      var lineStarts = new System.Collections.Generic.List<int>();
      foreach (System.Text.RegularExpressions.Match vm in valRegex.Matches(seg))
      { int atV = (s - e0) + vm.Index + vm.Value.IndexOf("value:",System.StringComparison.Ordinal); lineStarts.Add(atV); }
      valStarts.Add(lineStarts); }
    var mark = new System.Collections.Generic.HashSet<int>();
    int i2 = 0;
    while (i2 + 2 < axes.Count)
    { System.Boolean trip = axes[i2] == "x" && axes[i2 + 1] == "y" && axes[i2 + 2] == "z" && paths[i2] == paths[i2 + 1] && paths[i2] == paths[i2 + 2];
      if (!trip) { i2++; continue; }
      for (int k = 0; k < valStarts[i2].Count && k < valStarts[i2 + 1].Count && k < valStarts[i2 + 2].Count; k++)
      { System.Boolean allZero = NaNimate.Fix.lineZero(content,valStarts[i2][k] + e0) && NaNimate.Fix.lineZero(content,valStarts[i2 + 1][k] + e0) && NaNimate.Fix.lineZero(content,valStarts[i2 + 2][k] + e0);
        if (allZero) { mark.Add(valStarts[i2][k] + e0); mark.Add(valStarts[i2 + 1][k] + e0); mark.Add(valStarts[i2 + 2][k] + e0); } }
      i2 += 3; }
    if (mark.Count == 0) return content;
    var sb = new System.Text.StringBuilder(content);
    int[] pos = new int[mark.Count]; mark.CopyTo(pos); System.Array.Sort(pos);
    for (int q = pos.Length - 1; q >= 0; q--)
    { int p = pos[q]; int e = p;
      while (e < content.Length && content[e] != '\n') e++;
      while (e > p && (content[e - 1] == ' ' || content[e - 1] == '\t' || content[e - 1] == '\r')) e--;
      sb.Remove(p,e - p); sb.Insert(p,"value: NaN"); }
    return sb.ToString(); }
  private static System.Boolean lineZero(System.String content,int p)
  { int s = p + "value:".Length;
    while (s < content.Length && (content[s] == ' ' || content[s] == '\t')) s++;
    int t = s;
    while (t < content.Length && content[t] != '\n' && content[t] != ',') t++;
    System.String txt = content.Substring(s,t - s).Trim();
    float v;
    if (float.TryParse(txt,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out v)) return v == 0f;
    return txt == "0"; }
  }
}
}
}
#endif
