#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NaNimate {
public static class Write
  { public static System.String MasterText(System.String a,System.String fxPath,System.String exprPath,System.String menuPath,System.String masterPath,System.String mainDbtPath,System.Collections.Generic.List<System.String> tp)
  { var sb = new System.Text.StringBuilder();
    sb.AppendLine("NZK DBT Generated Sync\nAvatar: " + a + "\n\n|---------Start NZK Generated Params---------|");
    sb.AppendLine("Files:\n1) " + fxPath + "\n2) " + exprPath + "\n3) " + menuPath + "\n4) " + masterPath + "\n\nMain DBT:\n- " + mainDbtPath + "\n\nExpected Parameters:");
    tp.ForEach(p => sb.AppendLine("- " + p + " | FX=float | VRC=System.Boolean | Menu=toggle")); sb.AppendLine("|---------End NZK Generated Params---------|"); return sb.ToString(); }
  public static void MasterParams(System.String a,System.String masterPath,System.String fxPath,System.String exprPath,System.String menuPath,System.String mainDbtPath,System.Collections.Generic.List<System.String> tp)
  { System.String abs = NaNimate.Paths.Abs(masterPath); System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(abs));
    System.IO.File.WriteAllText(abs,NaNimate.Write.MasterText(a,fxPath,exprPath,menuPath,masterPath,mainDbtPath,tp)); }
  public static void NaNAnim(System.String clipPath,System.String objectPath)
  { System.String abs = NaNimate.Paths.Abs(clipPath); System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(abs));
    System.IO.File.WriteAllText(abs,Files.Templates.NaNAnimationYamlTemplate(System.IO.Path.GetFileNameWithoutExtension(clipPath),objectPath));
    UnityEditor.AssetDatabase.ImportAsset(clipPath,UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate); UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport); } }
}
}
}
#endif
