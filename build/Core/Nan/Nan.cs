namespace NZK
{
public static partial class Core {
public static class Nan
  {
    /* ── DETECT: does this text hold a zero scale? (read-only) ───────────── */
    /** <summary>True when any "{x: a, y: b, z: c}" scale vector is all-zero.</summary> */
    public static System.Boolean HasZeroVector(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return false;
      var rx = new System.Text.RegularExpressions.Regex(
        "value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}");
      foreach (System.Text.RegularExpressions.Match m in rx.Matches(text))
      { if (Yaml.IsZero(Yaml.Group(m,1)) && Yaml.IsZero(Yaml.Group(m,2)) && Yaml.IsZero(Yaml.Group(m,3))) return true; }
      return false; }
    /** <summary>True when any scalar scale row is "value: 0".</summary> */
    public static System.Boolean HasZeroScalar(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return false;
      var rx = new System.Text.RegularExpressions.Regex("(?m)^(\\s*value:\\s*)(\\S+)\\s*$");
      foreach (System.Text.RegularExpressions.Match m in rx.Matches(text))
      { if (Yaml.IsZero(Yaml.Group(m,2))) return true; }
      return false; }
    /** <summary>True when the text holds ANY zero scale value (vector or scalar).</summary> */
    public static System.Boolean HasZeroScale(System.String text) =>
      HasZeroVector(text) || HasZeroScalar(text);
    /** <summary>True when the text already carries a NaN scale value.</summary> */
    public static System.Boolean HasNaNScale(System.String text) =>
      !System.String.IsNullOrEmpty(text) &&
      (text.Contains(Yaml.VecNaN) || text.Contains("value: " + Yaml.NaN));
    /** <summary>Of a whole .anim asset, true when its AnimationClip docs hold a zero scale.</summary> */
    public static System.Boolean ClipHasZeroScale(System.String text)
    { foreach (var doc in Yaml.ClipDocs(text))
      { if (HasZeroScale(doc)) return true; }
      return false; }
    /* ── SET zero-only: only EXACT 0 becomes NaN ─────────────────────────── */
    /** <summary>Rewrite only "{x: 0, y: 0, z: 0}" scale vectors to NaN (m_ScaleCurves).
     *  A vector converts only when ALL THREE axes are zero.</summary> */
    public static System.String ScaleVectorsZeroToNaN(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return text;
      var rx = new System.Text.RegularExpressions.Regex(
        "value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}");
      return rx.Replace(text,m => Yaml.IsZero(Yaml.Group(m,1)) && Yaml.IsZero(Yaml.Group(m,2)) && Yaml.IsZero(Yaml.Group(m,3))
        ? "value: " + Yaml.VecNaN
        : m.Value); }
    /** <summary>Rewrite only scalar "value: 0" rows to "value: NaN" (m_EditorCurves scale rows).</summary> */
    public static System.String ScaleScalarsZeroToNaN(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return text;
      var rx = new System.Text.RegularExpressions.Regex("(?m)^(\\s*value:\\s*)(\\S+)\\s*$");
      return rx.Replace(text,m => Yaml.IsZero(Yaml.Group(m,2))
        ? m.Groups[1].Value + Yaml.NaN
        : m.Value); }
    /** <summary>Zero scale values (vector + per-axis + flags) → NaN. null when unchanged. </summary> */
    public static System.String ScaleToNaN(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return null;
      System.String o = ScaleVectorsZeroToNaN(text);
      o = ScaleScalarsZeroToNaN(o);
      o = Yaml.SetFlags(o,Yaml.FlagsScale);
      return o == text ? null : o; }
    /** <summary>Of a whole .anim asset, NaN only the scale values of AnimationClip
     *  documents. Raw file text in/out, so the file stays byte/line-clean. null when unchanged. </summary> */
    public static System.String ClipScaleToNaN(System.String text) =>
      PerClip(text,ScaleToNaN);
    /* ── SET unconditional: force ANY numeric scale to NaN ───────────────── */
    /** <summary>Force EVERY scale vector to NaN, whatever its value. </summary> */
    public static System.String ScaleVectorsToNaN(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return text;
      var rx = new System.Text.RegularExpressions.Regex(
        "value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}");
      return rx.Replace(text,m => Yaml.IsScaleAxes(Yaml.Group(m,1),Yaml.Group(m,2),Yaml.Group(m,3))
        ? "value: " + Yaml.VecNaN
        : m.Value); }
    /** <summary>Force EVERY scalar scale row to NaN, whatever its value. </summary> */
    public static System.String ScaleScalarsToNaN(System.String text)
    { if (System.String.IsNullOrEmpty(text)) return text;
      var rx = new System.Text.RegularExpressions.Regex("(?m)^(\\s*value:\\s*)(\\S+)\\s*$");
      return rx.Replace(text,m => Yaml.IsNumber(Yaml.Group(m,2))
        ? m.Groups[1].Value + Yaml.NaN
        : m.Value); }
    /** <summary>Force scale (vector + scalars + flags) to NaN regardless of value. null when unchanged. </summary> */
    public static System.String ScaleForceNaN(System.String doc)
    { if (System.String.IsNullOrEmpty(doc)) return null;
      System.String o = ScaleVectorsToNaN(doc);
      o = ScaleScalarsToNaN(o);
      o = Yaml.SetFlags(o,Yaml.FlagsScale);
      return o == doc ? null : o; }
    /** <summary>Of a whole .anim asset, force every AnimationClip scale to NaN. null when unchanged. </summary> */
    public static System.String ClipScaleForceNaN(System.String text) =>
      PerClip(text,ScaleForceNaN);
    /** <summary>Run a per-document NaN transform over only the AnimationClip blocks of a
     *  whole .anim asset, preserving the %YAML/%TAG header and every non-clip block.
     *  null when nothing changed. </summary> */
    static System.String PerClip(System.String text,System.Func<System.String,System.String> transform)
    { var spans = Yaml.ClipSpans(text);
      if (spans.Count == 0) return null;
      var sb = new System.Text.StringBuilder(text.Length);
      sb.Append(text.Substring(0,spans[0].start));         // preserve %YAML/%TAG header
      System.Boolean changed = false;
      foreach (var span in spans)
      { var doc = text.Substring(span.start,span.end - span.start);
        var conv = transform(doc);
        if (conv != null) { doc = conv; changed = true; }
        sb.Append(doc); }
      return changed ? sb.ToString() : null; }
  }
}
}
