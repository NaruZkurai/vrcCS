namespace NZK
{
public static partial class Core {
public class YamlBlock : UnityEngine.MonoBehaviour
  {
    public System.String rawText;   // full block text including "--- !u!..." header to next "---" or EOF
    public int sortOrder;    // original position in the reference file (0-based)
    public System.String typeTag;   // e.g. "1101","1102","1107","91","114"
    public long fileId;    // the &fileID from the YAML header
    public System.String blockName;   // the m_Name value if present (for debugging)
  }
}
}
