#if UNITY_EDITOR
namespace NZK
{
public static partial class Core {
public static partial class NZKNaNimateGroupScanner {
        /* Blender source meshes and the vertex groups they declare. */
        
        
        const string BlenderBinary = "/usr/local/bin/blender";
        const string ProbeBegin = "NZKPROBE_BEGIN";
        const string ProbeEnd = "NZKPROBE_END";
        const int ProbeTimeoutMs = 900000;
        /* Locate the source .blend for a given imported model asset.
         * Prefers a sibling .blend with the same base name, then any nearby .blend.
         */
        public static string ResolveSourceBlend(string modelAssetPath)
        {
            if (NZK.B.NoE(modelAssetPath))
                return null;
            string absoluteModel = NZK.S.P.r2a(modelAssetPath);
            /* Already a .blend - use it directly. */
            if (NZK.S.EndsWithOIC(absoluteModel, ".blend") && System.IO.File.Exists(absoluteModel))
                return absoluteModel;
            /* Sibling .blend sharing the model's base name (fbx -> blend). */
            string baseName = NZK.S.P.Next(absoluteModel);
            string directory = System.IO.Path.GetDirectoryName(absoluteModel);
            if (!NZK.B.NoE(directory) && System.IO.Directory.Exists(directory))
            {
                string sibling = NZK.S.P.C(directory, baseName + ".blend");
                if (System.IO.File.Exists(sibling))
                    return sibling;
                string[] candidates = System.IO.Directory.GetFiles(
                    directory, "*.blend", System.IO.SearchOption.TopDirectoryOnly);
                if (candidates.Length == 1)
                    return candidates[0];
            }
            return null;
        }
        /* Read all mesh objects and their vertex groups from a .blend using Blender headless. */
        public static ScanResult ScanSourceBlend(string blendAbsolutePath)
        {
            var result = new ScanResult { blendPath = blendAbsolutePath };
            if (NZK.B.NoE(blendAbsolutePath) || !System.IO.File.Exists(blendAbsolutePath))
            {
                result.error = "Source .blend not found: " + (blendAbsolutePath ?? "<null>");
                return result;
            }
            if (!System.IO.File.Exists(BlenderBinary))
            {
                result.error = "Blender not found at " + BlenderBinary + ". Cannot read source vertex groups.";
                return result;
            }
            string scriptPath = WriteProbeScript();
            if (scriptPath == null)
            {
                result.error = "Failed to write temporary Blender probe script.";
                return result;
            }
            try
            {
                string stdout = RunBlender(scriptPath, blendAbsolutePath, out string stderr, out int exitCode);
                if (exitCode != 0 && NZK.B.NoE(stdout))
                {
                    result.error = "Blender exited with code " + exitCode + ". " + Truncate(stderr);
                    return result;
                }
                string payload = ExtractBetween(stdout, ProbeBegin, ProbeEnd);
                if (payload == null)
                {
                    result.error = "Blender output did not contain a probe payload. " + Truncate(stderr);
                    return result;
                }
                result.meshes = ParseProbePayload(payload);
                result.success = true;
                return result;
            }
            catch (System.Exception e)
            {
                result.error = e.Message;
                return result;
            }
            finally
            {
                TryDelete(scriptPath);
            }
        }
        /* Resolve which source group names exist as bones on the imported model. */
        public static System.Collections.Generic.HashSet<string> ResolveImportedBones(UnityEngine.GameObject modelRoot)
        {
            var bones = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (modelRoot == null)
                return bones;
            foreach (UnityEngine.Transform t in modelRoot.GetComponentsInChildren<UnityEngine.Transform>(true))
                bones.Add(t.name);
            /* UnityEngine.SkinnedMeshRenderer bone slots can reference bones outside the model subtree. */
            foreach (UnityEngine.SkinnedMeshRenderer smr in modelRoot.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true))
            {
                foreach (UnityEngine.Transform bone in smr.bones)
                {
                    if (bone != null)
                        bones.Add(bone.name);
                }
            }
            return bones;
        }
        /* Full scan for a model asset: source groups + imported bone resolution. */
        public static ScanResult Scan(string modelAssetPath, UnityEngine.GameObject modelRoot)
        {
            string blend = ResolveSourceBlend(modelAssetPath);
            ScanResult result = ScanSourceBlend(blend);
            if (!result.success)
                return result;
            result.importedBones = ResolveImportedBones(modelRoot);
            return result;
        }
        /* ---- Blender plumbing ------------------------------------------------- */
        static string WriteProbeScript()
        {
            try
            {
                string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "nzk_probe_vertex_groups.py");
                System.IO.File.WriteAllText(path, ProbeScriptContents);
                return path;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(NZK.S.NZKNaNimatePrefix("Could not write probe script: " + e.Message));
                return null;
            }
        }
        const string ProbeScriptContents =
@"import bpy, sys, json
argv = sys.argv
blend = argv[argv.index('--') + 1] if '--' in argv else None
if blend:
    bpy.ops.wm.open_mainfile(filepath=blend)
out = []
for obj in bpy.data.objects:
    if obj.type != 'MESH':
        continue
    out.append({
        'name': obj.name,
        'vertexCount': len(obj.data.vertices),
        'polyCount': len(obj.data.polygons),
        'vertexGroups': [g.name for g in obj.vertex_groups],
    })
print('NZKPROBE_BEGIN')
print(json.dumps(out))
print('NZKPROBE_END')
";
        static string RunBlender(string scriptPath, string blendPath, out string stderr, out int exitCode)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = BlenderBinary,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("--background");
            psi.ArgumentList.Add("--factory-startup");
            psi.ArgumentList.Add("--python");
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add(blendPath);
            var stdout = new System.Text.StringBuilder();
            var stderrBuilder = new System.Text.StringBuilder();
            using (var process = new System.Diagnostics.Process { StartInfo = psi })
            {
                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) stdout.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) stderrBuilder.AppendLine(e.Data);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (!process.WaitForExit(ProbeTimeoutMs))
                {
                    try { process.Kill(); } catch { /* best effort */ }
                    stderr = "Blender probe timed out after " + (ProbeTimeoutMs / 1000) + "s.";
                    exitCode = -1;
                    return stdout.ToString();
                }
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            stderr = stderrBuilder.ToString();
            return stdout.ToString();
        }
        static System.Collections.Generic.List<SourceMesh> ParseProbePayload(string json)
        {
            var meshes = new System.Collections.Generic.List<SourceMesh>();
            if (NZK.B.N.ll.ws(json))
                return meshes;
            /* Lightweight parse: the probe emits a flat array of objects with a
             * string array field. Avoids a hard dependency on a JSON library. */
            foreach (string objectBlock in SplitTopLevelObjects(json))
            {
                string name = ExtractStringField(objectBlock, "name");
                if (NZK.B.NoE(name))
                    continue;
                meshes.Add(new SourceMesh
                {
                    name = name,
                    vertexCount = ExtractIntField(objectBlock, "vertexCount"),
                    polyCount = ExtractIntField(objectBlock, "polyCount"),
                    vertexGroups = ExtractStringArrayField(objectBlock, "vertexGroups"),
                });
            }
            return SortMeshes(meshes);
        }
        /* Order meshes by name without LINQ. */
        static System.Collections.Generic.List<SourceMesh> SortMeshes(
            System.Collections.Generic.List<SourceMesh> meshes)
        {
            meshes.Sort((a, b) => string.Compare(
                a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
            return meshes;
        }
        static System.Collections.Generic.IEnumerable<string> SplitTopLevelObjects(string json)
        {
            int depth = 0;
            int start = -1;
            bool inString = false;
            bool escaped = false;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (inString)
                {
                    if (escaped) { escaped = false; }
                    else if (c == '\\') { escaped = true; }
                    else if (c == '"') { inString = false; }
                    continue;
                }
                if (c == '"') { inString = true; continue; }
                if (c == '{')
                {
                    if (depth == 0) start = i;
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        yield return json.Substring(start, i - start + 1);
                        start = -1;
                    }
                }
            }
        }
        static string ExtractStringField(string block, string field)
        {
            string marker = "\"" + field + "\"";
            int idx = block.IndexOf(marker, System.StringComparison.Ordinal);
            if (idx < 0) return null;
            int colon = block.IndexOf(':', idx + marker.Length);
            if (colon < 0) return null;
            int openQuote = block.IndexOf('"', colon + 1);
            if (openQuote < 0) return null;
            var sb = new System.Text.StringBuilder();
            for (int i = openQuote + 1; i < block.Length; i++)
            {
                char c = block[i];
                if (c == '\\' && i + 1 < block.Length)
                {
                    sb.Append(block[i + 1]);
                    i++;
                    continue;
                }
                if (c == '"') break;
                sb.Append(c);
            }
            return sb.ToString();
        }
        static int ExtractIntField(string block, string field)
        {
            string marker = "\"" + field + "\"";
            int idx = block.IndexOf(marker, System.StringComparison.Ordinal);
            if (idx < 0) return 0;
            int colon = block.IndexOf(':', idx + marker.Length);
            if (colon < 0) return 0;
            /* Numeric fields are unbracketed; only bare digits/minus are valid.
             * Guard against a missing value (e.g. "vertexCount": "polyCount")
             * by requiring the first non-space char to be a digit or minus. */
            int start = colon + 1;
            while (start < block.Length && char.IsWhiteSpace(block[start]))
                start++;
            if (start >= block.Length)
                return 0;
            if (!char.IsDigit(block[start]) && block[start] != '-')
                return 0;
            int end = start;
            while (end < block.Length && (char.IsDigit(block[end]) || block[end] == '-'))
                end++;
            string raw = block.Substring(start, end - start);
            return int.TryParse(raw, out int result) ? result : 0;
        }
        static System.Collections.Generic.List<string> ExtractStringArrayField(string block, string field)
        {
            var values = new System.Collections.Generic.List<string>();
            string marker = "\"" + field + "\"";
            int idx = block.IndexOf(marker, System.StringComparison.Ordinal);
            if (idx < 0) return values;
            int open = block.IndexOf('[', idx);
            if (open < 0) return values;
            int close = block.IndexOf(']', open);
            if (close < 0) return values;
            string arrayBody = block.Substring(open + 1, close - open - 1);
            int cursor = 0;
            while (cursor < arrayBody.Length)
            {
                int quote = arrayBody.IndexOf('"', cursor);
                if (quote < 0) break;
                var sb = new System.Text.StringBuilder();
                int i = quote + 1;
                for (; i < arrayBody.Length; i++)
                {
                    char c = arrayBody[i];
                    if (c == '\\' && i + 1 < arrayBody.Length)
                    {
                        sb.Append(arrayBody[i + 1]);
                        i++;
                        continue;
                    }
                    if (c == '"') break;
                    sb.Append(c);
                }
                values.Add(sb.ToString());
                cursor = i + 1;
            }
            return values;
        }
        static string ExtractBetween(string text, string begin, string end)
        {
            if (NZK.B.NoE(text)) return null;
            int start = text.IndexOf(begin, System.StringComparison.Ordinal);
            if (start < 0) return null;
            start += begin.Length;
            int stop = text.IndexOf(end, start, System.StringComparison.Ordinal);
            if (stop < 0) return null;
            return text.Substring(start, stop - start).Trim();
        }
        static string Truncate(string value)
        {
            if (NZK.B.NoE(value)) return string.Empty;
            value = value.Replace('\n', ' ').Replace('\r', ' ').Trim();
            return value.Length <= 240 ? value : value.Substring(0, 240) + "…";
        }
        static void TryDelete(string path)
        {
            try
            {
                if (NZK.S.Has(path) && System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch { /* best effort */ }
        }
    
}
}
}
#endif
