namespace NZK.NaNimate
{
public static partial class NZKNaNimateGroupScanner {
public sealed class ScanResult
        {
            public bool success;
            public string error;
            public string blendPath;
            public System.Collections.Generic.List<SourceMesh> meshes = new System.Collections.Generic.List<SourceMesh>();
            public System.Collections.Generic.HashSet<string> importedBones = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            /* All group names present anywhere in the source blend. */
            public System.Collections.Generic.IEnumerable<string> AllGroups()
            {
                System.Collections.Generic.List<string> all =
                    new System.Collections.Generic.List<string>();
                System.Collections.Generic.HashSet<string> seen =
                    new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                for (int m = 0; m < meshes.Count; m++)
                {
                    SourceMesh mesh = meshes[m];
                    if (mesh.vertexGroups == null)
                        continue;
                    for (int g = 0; g < mesh.vertexGroups.Count; g++)
                    {
                        string name = mesh.vertexGroups[g];
                        if (seen.Add(name))
                            all.Add(name);
                    }
                }
                all.Sort((a, b) => string.Compare(
                    a, b, System.StringComparison.OrdinalIgnoreCase));
                return all;
            }
            /* Groups in the source that are NOT present as bones in the import. */
            public System.Collections.Generic.IEnumerable<string> MissingGroups()
            {
                return FilterGroups(requireImported: false);
            }
            /* Groups in the source that ARE present as bones in the import. */
            public System.Collections.Generic.IEnumerable<string> ImportedGroups()
            {
                return FilterGroups(requireImported: true);
            }
            System.Collections.Generic.List<string> FilterGroups(bool requireImported)
            {
                System.Collections.Generic.List<string> result =
                    new System.Collections.Generic.List<string>();
                foreach (string group in AllGroups())
                {
                    if (importedBones.Contains(group) == requireImported)
                        result.Add(group);
                }
                return result;
            }
        }
}
}
