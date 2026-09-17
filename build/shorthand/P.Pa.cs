namespace NZK{  public static partial class P{
  /// <summary>
  /// Absolute filesystem path for a project-relative asset path.
  ///
  /// Extracted verbatim from NZKNaNimateMeshGenerator.AbsoluteFromProject and
  /// NZKNaNimateMeshFolder.AbsoluteFromProject: the AssetDatabase APIs used
  /// throughout the toolkit take project-relative paths while every existence
  /// and existence-proof check runs on the FILESYSTEM, so this conversion is
  /// needed by more than one consumer.
  ///
  /// The parent of Application.dataPath is the project root, i.e. the folder
  /// that CONTAINS "Assets". Separators are normalised to the host separator
  /// so a forward-slash asset path still resolves on Windows.
  /// </summary>
  public static string Pa(string projectRelative){
    string root=System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName;
    return System.IO.Path.Combine(root,(projectRelative??string.Empty).Replace('/',System.IO.Path.DirectorySeparatorChar));
  }
  }
  }
