namespace NZK
{
public static partial class Core {
public static class ParamUtil
  {/* Sanitize a parameter name: replace spaces/hyphens with _,strip others */
    public static System.String Sanitize(System.String n)
    {
      var sb = new System.Text.StringBuilder();
      foreach (var c in n)
      {
        if (System.Char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
        else if (c == ' ' || c == '-') sb.Append('_');
      }
      return sb.ToString();
    }
    /* Generate a toggle parameter name with prefix */
    public static System.String ToggleName(System.String displayName) =>
      GestureParams.TogglePrefix + Sanitize(displayName);
    /* Sanitize filename chars (for asset names,not params) */
    public static System.String SanitizeFileName(System.String n)
    {
      var invalids = System.IO.Path.GetInvalidFileNameChars();
      var result = new System.Text.StringBuilder();
      foreach (var c in n)
        if (System.Array.IndexOf(invalids, c) < 0 && c != ' ') result.Append(c);
      return result.ToString();
    }
  }
}
}
