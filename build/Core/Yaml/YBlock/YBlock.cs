namespace NZK
{
public static partial class Core {
public static partial class Yaml {
public struct YBlock
    { public System.String rawText;   // full text incl. "--- !u!..." header
      public System.String typeTag;   // e.g. "1101","1102","1107","91","114"
      public System.String name;      // m_Name value when present
      public System.Int64 fileId;     // the &fileID from the marker
      public System.Int32 order; }
}
}
}
