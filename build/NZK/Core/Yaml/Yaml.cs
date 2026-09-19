namespace NZK
{
public static partial class Core {
public static partial class Yaml {
    /* ── document / block markers ────────────────────────────────────────── */
    public const System.Char Sep = '\u00a7';              // block line separator
    /* "--- !u!" — anchored marker. The "%TAG !u! tag:..." header contains a bare
       "!u!" too, so always search for THIS full string, never just "!u!". */
    public const System.String Doc = "--- !u!";
    public const System.String TagHdr = "%TAG !u! tag:unity3d.com,2011:";
    public const System.String YmlHdr = "%YAML 1.1";
    /* ── block model ─────────────────────────────────────────────────────── */
        // 0-based position in the file
    /** <summary>Compose a YBlock from raw text, deriving tag/fileID/name.</summary> */
    public static YBlock Make(System.String rawText,System.Int32 order)
    { var yb = new YBlock();
      yb.rawText = rawText;
      yb.typeTag = TypeTag(rawText);
      yb.fileId  = FileId(rawText);
      yb.name    = BlockName(rawText);
      yb.order   = order;
      return yb; }
    /* ── whole-file parsing ──────────────────────────────────────────────── */
    /** <summary>Split a whole YAML asset into a header plus one YBlock per document.</summary> */
    public static System.Collections.Generic.List<YBlock> Parse(System.String text)
    { var blocks = new System.Collections.Generic.List<YBlock>();
      if (System.String.IsNullOrEmpty(text)) return blocks;
      int first = text.IndexOf("\n" + Doc);
      if (first > 0) blocks.Add(Make(text.Substring(0,first + 1),0));
      int pos = 0,order = blocks.Count;
      while (pos < text.Length)
      { int start = text.IndexOf(Doc,pos);
        if (start < 0) break;
        int next  = text.IndexOf(Doc,start + Doc.Length);
        int end   = next >= 0 ? next : text.Length;
        blocks.Add(Make(text.Substring(start,end - start).TrimEnd() + Sep,order++));
        pos = end; }
      return blocks; }
    /** <summary>Reassemble blocks into a single YAML asset, in sort order.</summary> */
    public static System.String Compose(System.Collections.Generic.List<YBlock> blocks)
    { var sb = new System.Text.StringBuilder();
      foreach (var b in blocks)
      { if (b.rawText != null) sb.Append(b.rawText); }
      return sb.ToString(); }
    /* ── header helpers ──────────────────────────────────────────────────── */
    /** <summary>True when the text is a Unity YAML asset (has %YAML or a document marker).</summary> */
    public static System.Boolean IsYaml(System.String text) => !System.String.IsNullOrEmpty(text) && (text.StartsWith(YmlHdr) || text.Contains(Doc));
    /** <summary>Read "--- !u!TYPE &FILEID" → TYPE.
     *  Anchors on the full "--- !u!" marker so the "%TAG !u! tag:..." header can never match.</summary> */
    public static System.String TypeTag(System.String rawText)
    { if (System.String.IsNullOrEmpty(rawText)) return "";
      int s = rawText.IndexOf(Doc);
      if (s < 0) return "";
      s += Doc.Length;
      int e = rawText.IndexOf(' ',s);
      return e > s ? rawText.Substring(s,e - s) : ""; }
    /** <summary>Read "--- !u!TYPE &FILEID" → FILEID (or 0).</summary> */
    public static long FileId(System.String rawText)
    { if (System.String.IsNullOrEmpty(rawText)) return 0;
      int s = rawText.IndexOf('&');
      if (s < 0) return 0;
      s++;
      int e = rawText.IndexOf('\n',s);
      long v = 0;
      System.Int64.TryParse(e > s ? rawText.Substring(s,e - s) : rawText.Substring(s),
        System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out v);
      return v; }
    /** <summary>Read the m_Name value of a block (or "").</summary> */
    public static System.String BlockName(System.String rawText)
    { int p = rawText == null ? -1 : rawText.IndexOf("m_Name: ");
      if (p < 0) return "";
      System.String rest = rawText.Substring(p + 8);
      int e = rest.IndexOf('\n');
      return (e < 0 ? rest : rest.Substring(0,e)).Trim(); }
    /* ── line helpers ────────────────────────────────────────────────────── */
    /** <summary>Split block text into its '\u00a7'-separated lines.</summary> */
    public static System.String[] Lines(System.String rawText) =>
      System.String.IsNullOrEmpty(rawText) ? System.Array.Empty<System.String>() : rawText.Split(Sep);
    /** <summary>Join block lines back into block text.</summary> */
    public static System.String Join(System.String[] lines) => System.String.Join(Sep.ToString(),lines);
    /** <summary>Parse the value part of a YAML line (after ": ").</summary> */
    public static System.String Value(System.String line)
    { int c = line == null ? -1 : line.IndexOf(':');
      return c >= 0 ? line.Substring(c + 1).Trim() : ""; }
    /** <summary>Parse the field name of a "m_Foo: ..." line (or "").</summary> */
    public static System.String Field(System.String line)
    { int c = line == null ? -1 : line.IndexOf(':');
      return c > 0 ? line.Substring(0,c).Trim() : ""; }
    /** <summary>True when the trimmed line is a "m_Something:" field line.</summary> */
    public static System.Boolean IsField(System.String line)
    { var t = line?.Trim();
      return !System.String.IsNullOrEmpty(t) && t.StartsWith("m_") && t.IndexOf(':') > 0; }
    /** <summary>Read a field's value by name from block text (or null).</summary> */
    public static System.String Get(System.String rawText,System.String field)
    { foreach (var l in Lines(rawText))
      { if (!IsField(l)) continue;
        if (Field(l) == field) return Value(l.Trim()); }
      return null; }
    /** <summary>Write a field's value in block text; returns the new text (unchanged when not found).</summary> */
    public static System.String Set(System.String rawText,System.String field,System.String value)
    { var lines = Lines(rawText);
      for (int i = 0; i < lines.Length; i++)
      { if (!IsField(lines[i])) continue;
        if (Field(lines[i]) == field) { lines[i] = "  " + field + ": " + value; return Join(lines); } }
      return rawText; }
    /* ── Unity type tags ─────────────────────────────────────────────────── */
    /* 74 = AnimationClip, 91 = AnimatorController,
       114 = AnimatorStateTransition, 1101 = AnimatorTransition,
       1102 = AnimatorState, 1107 = AnimatorStateMachine,
       1113 = AnimatorOverrideController, 1114 = AnimatorTransition(exit marker) */
    public const System.String T_AnimClip       = "74";
    public const System.String T_AController    = "91";
    public const System.String T_OverrideCtrl   = "1113";
    public const System.String T_State          = "1102";
    public const System.String T_Transition     = "1101";
    public const System.String T_StateMachine   = "1107";
    public const System.String T_ExitTransition = "1114";
    /** <summary>Parse a "m_Motion: {fileID: ...,guid: ...,type: 2}" value for its guid (or null).</summary> */
    public static System.String MotionGuid(System.String value)
    { if (System.String.IsNullOrEmpty(value) || !value.Contains("guid:")) return null;
      int s = value.IndexOf("guid: ") + 6;
      int e = value.IndexOf(',',s);
      return (e > s ? value.Substring(s,e - s) : value.Substring(s)).Trim(); }
    /** <summary>Format a clip reference as a m_Motion line value.</summary> */
    public static System.String MotionRef(System.String guid,System.Int64 fileId = 7400000) =>
      "{fileID: " + fileId + ",guid: " + guid + ",type: 2}";
    /** <summary>Convert a "1"/"0"/"true"/"false" YAML scalar to a bool.</summary> */
    public static System.Boolean AsBool(System.String value) => value == "1" || value == "true";
    /** <summary>Format a bool as a YAML scalar.</summary> */
    public static System.String Bool(System.Boolean value) => value ? "1" : "0";
    /** <summary>Parse a YAML float scalar (0 on failure).</summary> */
    public static System.Single AsFloat(System.String value)
    { System.Single f;
      System.Single.TryParse(value,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out f);
      return f; }
    /** <summary>Format a float as a YAML scalar. </summary> */
    public static System.String Float(System.Single value) => value.ToString("G",System.Globalization.CultureInfo.InvariantCulture);
    /* ── value literals Unity understands ───────────────────────────────── */
    /* These are YAML/Unity definitions, not NaN policy: any system that writes a
       curve needs them. NZK.Core.Nan implements the check/set logic on top. */
    public const System.String True   = "1";
    public const System.String False  = "0";
    public const System.String Zero   = "0";
    public const System.String NaN    = "NaN";
    public const System.String VecNaN = "{x: NaN, y: NaN, z: NaN}";
    /* Curve flag line: "flags: n". Unity marks a curve through this field. */
    public const System.String FlagsKey = "flags: ";
    /** <summary>Curve flag value Unity writes for a scale curve (seen on normal AND NaN curves). </summary> */
    public const System.String FlagsScale = "16";
    /** <summary>Curve flag value for a plain/normal curve. </summary> */
    public const System.String FlagsNone  = "0";
    /* Scale curve identifiers. */
    public const System.String AttrScale = "m_LocalScale.";
    public const System.String KeyScaleCurves  = "m_ScaleCurves:";
    public const System.String KeyEditorCurves = "m_EditorCurves:";
    /** <summary>True when the line names a scale curve (m_LocalScale.x/y/z or m_ScaleCurves:). </summary>
    public static System.Boolean IsScaleAttr(System.String line)
    { var t = line?.Trim();
      return !System.String.IsNullOrEmpty(t) &&
             (t.StartsWith(AttrScale) || t.StartsWith(KeyScaleCurves)); }
    /** <summary>True when the trimmed line is a curve flag line. </summary> */
    public static System.Boolean IsFlagLine(System.String line)
    { var t = line?.Trim();
      return !System.String.IsNullOrEmpty(t) && t.StartsWith(FlagsKey); }
    /** <summary>Read the "flags: n" value of a line ("" when not a flag line). </summary> */
    public static System.String Flags(System.String line) =>
      IsFlagLine(line) ? Value(line.Trim()) : "";
    /** <summary>Set the "flags: n" line to the given value ONLY for scale curves; returns the new text. </summary>
     *  <para>A Unity .anim curve block is the group of lines that starts at "- curve:" and runs to
     *  the next "- " list item or top-level key. "attribute:" precedes "flags:" inside a block,
     *  but other keys can sit between them, so blocks are walked line by line and the LAST
     *  "attribute:" seen is remembered until the block boundary.</para>
     *  <para>Only blocks whose attribute names a scale property ("m_LocalScale.x/y/z", see AttrScale)
     *  count, plus every block inside an "m_ScaleCurves:" section (those rows carry no attribute:).
     *  Every other "flags:" line is emitted byte-for-byte unchanged.</para>
     *  <para>Why: "flags" is the binding class marker of the curve, not a data value. "16" is the
     *  SCALE flag (FlagsScale), "0" the plain flag (FlagsNone). Nan.ScaleToNaN (nan.cs.nzk:87) and
     *  Nan.ScaleForceNaN (nan.cs.nzk:118) both end with SetFlags(o,FlagsScale), so the old
     *  document-global regex relabelled rotation, position, path, m_IsActive and material curves as
     *  scale too on EVERY avatar upload and every right-click "Fix Zero Scale Animations".</para>
     *  <para>PROVEN DAMAGE in the live project (not a git repo: no history and no undo):
     *  Assets/NZK/anims/HeadSwap On.anim line 689 is "flags: 16" ending an m_LocalPosition.x curve
     *  (its "attribute: m_LocalPosition.x" is line 685), with the same at lines 719 (.y) and 749 (.z),
     *  plus m_IsActive rows and material-curve rows; project-wide "flags: 0" x5780, "flags: 16" x10821,
     *  "flags: 2" x42, and 754 .anim files carry "flags: 16". The toolkit's own authoring template
     *  writes "flags: 0" for its m_LocalScale.x/y/z rows, so "16" is post-process-only corruption.
     *  The corruption is idempotent: rewriting 16 -> 16 returns the input unchanged, so ScaleToNaN
     *  returns null and FixAnimationClipScale bails out early and the 754 damaged files can never
     *  self-heal through the old path.</para>
     *  <para>IsScaleAttr stays the single-line predicate for "is this line a scale curve marker";
     *  SetFlags is now the block-aware equivalent that also honours the preceding "attribute:".</para>
     *  <para>Returns the ORIGINAL text reference when nothing changed, so the o == text identity
     *  check in Nan.ScaleToNaN still works.</para>
     *  <remarks>No regex and no 'using': every type is fully qualified and the line walk is
     *  StringBuilder-free so that line endings and all other characters are preserved exactly.</remarks> */
    public static System.String SetFlags(System.String text,System.String flags)
    { if (System.String.IsNullOrEmpty(text)) return text;
      /* Split WITHOUT dropping anything: split and rejoin with '\n' reproduces the input
         byte-for-byte (this is used on a '"' + '\n' + '"-separated line list, never on the '\u00a7' form). */
      var lines = text.Split('\n');
      /* Scale curve sections whose rows have no "attribute:" line. */
      var scaleSectionKeys = new System.String[] { KeyScaleCurves,KeyEditorCurves,"m_PositionCurves:","m_RotationCurves:","m_EulerCurves:","m_CompressedRotationCurves:","m_GenericBindings:" };
      System.Boolean inScaleSection = false, blockIsScale = false, anyChange = false;
      for (int i = 0; i < lines.Length; i++)
      { System.String raw = lines[i];
        System.String t = raw.Trim();
        /* Block boundary: a new list item, a new document key, or a "--- !u!" marker. */
        if (t.StartsWith("- ") || (t.Length > 0 && raw.Length > 0 && raw[0] != ' ' && raw[0] != '\t') || t.StartsWith(Doc))
        { blockIsScale = inScaleSection; }
        /* Track "m_ScaleCurves:" vs any other top-level m_ key. */
        if (t.Length > 0 && raw.Length > 0 && raw[0] != ' ' && raw[0] != '\t')
        { for (int k = 0; k < scaleSectionKeys.Length; k++)
            if (t.StartsWith(scaleSectionKeys[k])) { inScaleSection = k == 0; k = scaleSectionKeys.Length; } }
        /* Remember the last attribute: seen in this block. */
        if (t.StartsWith("attribute:")) blockIsScale = t.Substring(10).Trim().StartsWith(AttrScale);
        /* Only rewrite the flag when the current block is a scale curve. */
        if (t.StartsWith(FlagsKey))
        { System.String cur = t.Substring(FlagsKey.Length).Trim();
          if (blockIsScale && cur != flags)
          { var nl = raw.IndexOf('\n');
            System.String body = nl < 0 ? raw : raw.Substring(0,nl);
            System.String end  = nl < 0 ? "" : raw.Substring(nl);
            lines[i] = body.Substring(0,body.Length - cur.Length) + flags + end;
            anyChange = true; } } }
      return anyChange ? System.String.Join("\n",lines) : text; }
    /** <summary>True when v parses as a real number (NaN/Inf rejected as text "NaN"). </summary> */
    public static System.Boolean IsNumber(System.String v)
    { System.Single f;
      return !System.String.IsNullOrEmpty(v) &&
             System.Single.TryParse(v,System.Globalization.NumberStyles.Float,
               System.Globalization.CultureInfo.InvariantCulture,out f); }
    /** <summary>True when v parses as a float equal to zero (rejects NaN/Inf/non-numeric). </summary> */
    public static System.Boolean IsZero(System.String v)
    { System.Single f;
      return !System.String.IsNullOrEmpty(v) &&
             System.Single.TryParse(v,System.Globalization.NumberStyles.Float,
               System.Globalization.CultureInfo.InvariantCulture,out f) &&
             f == 0f; }
    /** <summary>Group text of a regex match ("" when the group is missing). </summary> */
    public static System.String Group(System.Text.RegularExpressions.Match m,System.Int32 i) =>
      m.Groups[i].Success ? m.Groups[i].Value : "";
    /** <summary>True when at least one axis is a real number (a vector worth forcing). </summary> */
    public static System.Boolean IsScaleAxes(System.String x,System.String y,System.String z) =>
      IsNumber(x) || IsNumber(y) || IsNumber(z);
    /* ── structure only ─────────────────────────────────────────────────── */
    /* This class knows how a Unity YAML document is BUILT (markers, spans, type
       tags, line/field IO). It knows nothing about scale or NaN semantics —
       that lives in NZK.Core.Nan (nan.cs.nzk), which calls back in here. */
    /** <summary>One "--- !u!" document span in raw file text: [start,end). </summary> */
    
    /** <summary>Every "--- !u!" document span in raw file text, in order.
     *  Raw offsets (no '\u00a7' separator), so callers can rebuild the file byte-clean. </summary> */
    public static System.Collections.Generic.List<Span> Spans(System.String text)
    { var spans = new System.Collections.Generic.List<Span>();
      if (System.String.IsNullOrEmpty(text)) return spans;
      int pos = 0;
      while (pos < text.Length)
      { int s = text.IndexOf(Doc,pos);
        if (s < 0) break;
        int n = text.IndexOf(Doc,s + Doc.Length);
        int e = n >= 0 ? n : text.Length;
        spans.Add(new Span { start = s,end = e });
        pos = e; }
      return spans; }
    /** <summary>Spans of only the AnimationClip (74) documents. </summary> */
    public static System.Collections.Generic.List<Span> ClipSpans(System.String text)
    { var clips = new System.Collections.Generic.List<Span>();
      foreach (var sp in Spans(text))
      { if (TypeTag(text.Substring(sp.start,sp.end - sp.start)) == T_AnimClip) clips.Add(sp); }
      return clips; }
    /** <summary>Raw text of every AnimationClip (74) document. </summary> */
    public static System.Collections.Generic.List<System.String> ClipDocs(System.String text)
    { var docs = new System.Collections.Generic.List<System.String>();
      if (System.String.IsNullOrEmpty(text)) return docs;
      foreach (var sp in ClipSpans(text))
      { docs.Add(text.Substring(sp.start,sp.end - sp.start)); }
      return docs; }
  
}
}
}
