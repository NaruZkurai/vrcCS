using System;
using System.IO;
using System.Text.RegularExpressions;

// Mirror of NZK.Core.Yaml scale->NaN members (verbatim logic) for offline testing.
static class Yaml
{
    public const string Doc = "--- !u!";
    public const string NaN = "NaN";
    public const string VecNaN = "{x: NaN, y: NaN, z: NaN}";
    public const string T_AnimClip = "74";

    public struct YBlock { public string rawText; public string typeTag; }

    public static string TypeTag(string rawText)
    { if (string.IsNullOrEmpty(rawText)) return "";
      int s = rawText.IndexOf(Doc);          // Doc == "--- !u!" (anchored)
      if (s < 0) return "";
      s += Doc.Length;
      int e = rawText.IndexOf(' ', s);
      return e > s ? rawText.Substring(s, e - s) : ""; }

    public static System.Collections.Generic.List<YBlock> Parse(string text)
    { var blocks = new System.Collections.Generic.List<YBlock>();
      if (string.IsNullOrEmpty(text)) return blocks;
      blocks.Add(new YBlock { rawText = text.Substring(0, 0), typeTag = "" });
      blocks.Clear();
      int pos = 0;
      while (pos < text.Length)
      { int start = text.IndexOf(Doc, pos);
        if (start < 0) break;
        int next = text.IndexOf(Doc, start + Doc.Length);
        int end = next >= 0 ? next : text.Length;
        var raw = text.Substring(start, end - start).TrimEnd() + "\u00a7";
        blocks.Add(new YBlock { rawText = raw, typeTag = TypeTag(raw) });
        pos = end; }
      return blocks; }

    public static string Compose(System.Collections.Generic.List<YBlock> b)
    { var sb = new System.Text.StringBuilder();
      foreach (var x in b) if (x.rawText != null) sb.Append(x.rawText);
      return sb.ToString(); }

    public static bool IsZero(string v)
    { float f;
      return !string.IsNullOrEmpty(v) &&
             float.TryParse(v, System.Globalization.NumberStyles.Float,
               System.Globalization.CultureInfo.InvariantCulture, out f) &&
             f == 0f; }

    static string G(Match m, int i) => m.Groups[i].Success ? m.Groups[i].Value : "";

    public static bool IsNumber(string v)
    { float f;
      return !string.IsNullOrEmpty(v) &&
             float.TryParse(v, System.Globalization.NumberStyles.Float,
               System.Globalization.CultureInfo.InvariantCulture, out f); }

    // ---- DETECT ----
    public static bool HasZeroVector(string text)
    { if (string.IsNullOrEmpty(text)) return false;
      var rx = new Regex("value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}");
      foreach (Match m in rx.Matches(text))
        if (IsZero(G(m, 1)) && IsZero(G(m, 2)) && IsZero(G(m, 3))) return true;
      return false; }

    public static bool HasZeroScalar(string text)
    { if (string.IsNullOrEmpty(text)) return false;
      var rx = new Regex("(?m)^(\\s*value:\\s*)(\\S+)\\s*$");
      foreach (Match m in rx.Matches(text))
        if (IsZero(G(m, 2))) return true;
      return false; }

    public static bool HasZeroScale(string text) => HasZeroVector(text) || HasZeroScalar(text);

    // ---- SET (zero only) ----
    public static string ScaleVectorsZeroToNaN(string text)
    { if (string.IsNullOrEmpty(text)) return text;
      var rx = new Regex("value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}");
      return rx.Replace(text, m => IsZero(G(m, 1)) && IsZero(G(m, 2)) && IsZero(G(m, 3))
        ? "value: " + VecNaN : m.Value); }

    public static string ScaleScalarsZeroToNaN(string text)
    { if (string.IsNullOrEmpty(text)) return text;
      var rx = new Regex("(?m)^(\\s*value:\\s*)(\\S+)\\s*$");
      return rx.Replace(text, m => IsZero(G(m, 2)) ? m.Groups[1].Value + NaN : m.Value); }

    // ---- SET (unconditional) ----
    public static string ScaleVectorsToNaN(string text)
    { if (string.IsNullOrEmpty(text)) return text;
      var rx = new Regex("value:\\s*\\{x:\\s*([^,}]+),\\s*y:\\s*([^,}]+),\\s*z:\\s*([^,}]+)\\}");
      return rx.Replace(text, m => (IsNumber(G(m, 1)) || IsNumber(G(m, 2)) || IsNumber(G(m, 3)))
        ? "value: " + VecNaN : m.Value); }

    public static string ScaleScalarsToNaN(string text)
    { if (string.IsNullOrEmpty(text)) return text;
      var rx = new Regex("(?m)^(\\s*value:\\s*)(\\S+)\\s*$");
      return rx.Replace(text, m => IsNumber(G(m, 2)) ? m.Groups[1].Value + NaN : m.Value); }

    public static string ScaleToNaN(string text)
    { if (string.IsNullOrEmpty(text)) return null;
      string o = ScaleVectorsZeroToNaN(text);
      o = ScaleScalarsZeroToNaN(o);
      return o == text ? null : o; }

    public static string ScaleToNaNRaw(string doc)
    { if (string.IsNullOrEmpty(doc)) return null;
      string o = ScaleVectorsZeroToNaN(doc);
      o = ScaleScalarsZeroToNaN(o);
      return o == doc ? null : o; }

    public static string ScaleForceNaNRaw(string doc)
    { if (string.IsNullOrEmpty(doc)) return null;
      string o = ScaleVectorsToNaN(doc);
      o = ScaleScalarsToNaN(o);
      return o == doc ? null : o; }

    static void Spans(string text, System.Collections.Generic.List<int> s, System.Collections.Generic.List<int> e)
    { int pos = 0;
      while (pos < text.Length)
      { int a = text.IndexOf(Doc, pos); if (a < 0) break;
        int n = text.IndexOf(Doc, a + Doc.Length);
        int b = n >= 0 ? n : text.Length;
        s.Add(a); e.Add(b); pos = b; } }

    public static bool ClipHasZeroScale(string text)
    { if (string.IsNullOrEmpty(text)) return false;
      var s = new System.Collections.Generic.List<int>(); var e = new System.Collections.Generic.List<int>();
      Spans(text, s, e);
      for (int i = 0; i < s.Count; i++)
      { var d = text.Substring(s[i], e[i] - s[i]);
        if (TypeTag(d) == T_AnimClip && HasZeroScale(d)) return true; }
      return false; }

    static string Map(string text, System.Func<string, string> f)
    { if (string.IsNullOrEmpty(text)) return null;
      var s = new System.Collections.Generic.List<int>(); var e = new System.Collections.Generic.List<int>();
      Spans(text, s, e);
      if (s.Count == 0) return null;
      var sb = new System.Text.StringBuilder(text.Length);
      sb.Append(text.Substring(0, s[0]));
      bool changed = false;
      for (int i = 0; i < s.Count; i++)
      { var d = text.Substring(s[i], e[i] - s[i]);
        if (TypeTag(d) == T_AnimClip) { var c = f(d); if (c != null) { d = c; changed = true; } }
        sb.Append(d); }
      return changed ? sb.ToString() : null; }

    public static string ClipScaleToNaN(string t) => Map(t, ScaleToNaNRaw);
    public static string ClipScaleForceNaN(string t) => Map(t, ScaleForceNaNRaw);
}

static class P
{
    static int Main(string[] a)
    {
        // Always read the REAL asset folder, never a stale local copy.
        string dir = a.Length > 0 ? a[0] : "/nzk/unity/blank project/Assets";
        // name -> (detect says zero?, zero-only fix changes?, force-set changes?)
        //   given CURRENT on-disk state:
        //   0tonan.anim      NaN vec / NaN scalars -> detect no,  zero-fix no,  force no
        //   0tonan 2.anim    NaN vec / NaN scalars -> detect no,  zero-fix no,  force no
        //   0tonan 3.anim      3 vec /   3 scalars -> detect no,  zero-fix NO,  force YES
        //   0tonan 4.anim    NaN vec /   0 scalars -> detect YES, zero-fix YES, force YES
        //   0tonan 5.anim      0 vec /   0 scalars -> detect YES, zero-fix YES, force YES
        //   0tonantest.anim  NaN vec /   0 scalars -> detect YES, zero-fix YES, force YES
        //   0tonan_1.anim      1 vec /   1 scalars -> detect no,  zero-fix NO,  force YES
        var exp = new (string name, bool det, bool zero, bool force)[]
        {
            ("0tonan.anim",     false, false, false),
            ("0tonan 2.anim",   false, false, false),
            ("0tonan 3.anim",   false, false, true ),
            ("0tonan 4.anim",   true,  true,  true ),
            ("0tonan 5.anim",   true,  true,  true ),
            ("0tonantest.anim", true,  true,  true ),
            ("0tonan_1.anim",   false, false, true ),
        };
        int pass = 0, fail = 0;
        foreach (var x in exp)
        {
            string src = File.ReadAllText(Path.Combine(dir, x.name));
            bool det = Yaml.ClipHasZeroScale(src);
            string zo = Yaml.ClipScaleToNaN(src);
            string fo = Yaml.ClipScaleForceNaN(src);
            bool zch = zo != null, fch = fo != null;
            string zb = zo ?? src, fb = fo ?? src;
            bool ok = det == x.det && zch == x.zero && fch == x.force;
            bool keep = zb.StartsWith("%YAML 1.1") && fb.StartsWith("%YAML 1.1")
                        && zb.IndexOf('\u00a7') < 0 && fb.IndexOf('\u00a7') < 0;
            if (ok && keep) pass++; else fail++;
            Console.WriteLine("{0,-16} detect={1,-5} zeroFix={2,-5} force={3,-5}  want d={4,-5} z={5,-5} f={6,-5}  {7}",
                x.name, det, zch, fch, x.det, x.zero, x.force, (ok && keep) ? "PASS" : "FAIL");
        }
        Console.WriteLine($"\n{pass} passed, {fail} failed");
        return fail == 0 ? 0 : 1;
    }
}
