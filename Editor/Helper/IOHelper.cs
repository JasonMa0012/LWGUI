// Copyright (c) Jason Ma

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LWGUI.PerformanceMonitor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LWGUI
{
    public static class IOHelper
    {
        #region Paths

        private static string _cachedProjectPath;

        // D:/Unity/ProjectName/
        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedProjectPath))
                    _cachedProjectPath = Application.dataPath[..^6];
                return _cachedProjectPath;
            }
        }

        public static string GetAbsPath(string unityProjectRelativePath)
        {
            if (string.IsNullOrEmpty(unityProjectRelativePath))
                return null;
            
            if (unityProjectRelativePath.StartsWith("Packages/", StringComparison.Ordinal))
            {
                var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(unityProjectRelativePath);
                if (pkgInfo != null)
                {
                    var prefix = $"Packages/{pkgInfo.name}/";
                    var relativePart = unityProjectRelativePath.Substring(prefix.Length);
                    return Path.Combine(pkgInfo.resolvedPath, relativePart);
                }
            }
            return Path.Combine(ProjectPath, unityProjectRelativePath);
        }

        public static string GetAssetAbsPathFromGUID(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
                return null;
            return GetAbsPath(assetPath);
        }
        
        public static string GetRelativePath(string absPath) => Path.GetFullPath(absPath).Replace(Path.GetFullPath(ProjectPath), string.Empty);

        private static string _cachedCompiledShaderCachePath;

        public static string CompiledShaderCacheRootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedCompiledShaderCachePath))
                    _cachedCompiledShaderCachePath = Path.Combine(ProjectPath, "Library", "LWGUI", "ShaderPerfCache");
                return _cachedCompiledShaderCachePath;
            }
        }

        public static string GetValidFileName(string text)
        {
            StringBuilder str = new StringBuilder();
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (var c in text)
            {
                if (!invalidFileNameChars.Contains(c))
                {
                    str.Append(c);
                }
            }

            return str.ToString();
        }

        #endregion

        #region File

        public static bool ExistAndNotEmpty(string filePath) => File.Exists(filePath) && new FileInfo(filePath).Length > 1;

        public static string GenerateUniqueFileName(string directory, string baseName, string extension)
        {
            var absDirectory = GetAbsPath(directory);

            string rootName = baseName;
            int startIndex = 1;

            int lastUnderscore = baseName.LastIndexOf('_');
            if (lastUnderscore >= 0 && lastUnderscore < baseName.Length - 1
                && int.TryParse(baseName.Substring(lastUnderscore + 1), out int existingIndex))
            {
                rootName = baseName.Substring(0, lastUnderscore);
                startIndex = existingIndex + 1;
            }

            if (!File.Exists(Path.Combine(absDirectory, baseName + "." + extension)))
                return baseName;

            int index = startIndex;
            string candidate;
            do
            {
                candidate = rootName + "_" + index;
                index++;
            } while (File.Exists(Path.Combine(absDirectory, candidate + "." + extension)));

            return candidate;
        }

        public static void WriteBinaryFile(string filePath, byte[] bytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
                File.WriteAllBytes(filePath, bytes ?? Array.Empty<byte>());
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static void WriteTextFile(string filePath, string text)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
                File.WriteAllText(filePath, text ?? string.Empty, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static string ReadTextFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        /// <summary>
        ///   <para>Displays the "save file" dialog and returns the selected path name.</para>
        /// </summary>
        /// <param name="title">The title of the window to display.</param>
        /// <param name="relativeDirectory">The working directory that this dialog opens on.</param>
        /// <param name="defaultName">The placeholder text to display in the "Save As" text field. This is the name of file to be saved. </param>
        /// <param name="extension">The file extension to use in the saved file path. For example, enter "png" to save an image in the PNG format.</param>
        /// <returns>
        ///   <para>A string absolute path to the saved file if the dialog was canceled or the save failed, it returns an empty string.</para>
        /// </returns>
        public static string SaveFilePanel(string title, string relativeDirectory, string defaultName, string extension)
        {
            // When a new folder is created in the file selection window, the Current Work Directory is modified, which causes Unity to crash
            var savedCwd = Directory.GetCurrentDirectory();
            var absPath = EditorUtility.SaveFilePanel(title, relativeDirectory, defaultName, extension);
            Directory.SetCurrentDirectory(savedCwd);
            
            return absPath;
        }

        #endregion

        #region Process

        public static bool RunProcess(string     file, string args,
                                      out string output)
        {
            output = string.Empty;

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            p.OutputDataReceived += (_, e) => stdout.AppendLine(e.Data);
            p.ErrorDataReceived += (_,  e) => stderr.AppendLine(e.Data);

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();

            output = stdout.ToString();

            if (p.ExitCode != 0)
            {
				output = stderr.ToString() + output;
                Debug.LogError($"LWGUI: Process Exit Code {p.ExitCode}: {output}" +
                               $"File: {file}\n" +
                               $"Args: {args}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                output = stderr.ToString();
                return false;
            }

            return true;
        }

        public static bool RunCMD(string     args,
                                  out string output)
        {
            return RunProcess("cmd.exe", $"/C {args}",
                out output);
        }

        public static void OpenFile(string filePath)
        {
            Process.Start(filePath);
        }

        #endregion

        #region Performance Monitor

        public static string GetCompiledShaderCacheRootDirectory(Shader shader)
        {
            var shaderPath = AssetDatabase.Contains(shader) ? AssetDatabase.GetAssetPath(shader) : null;
            var shaderCachePath = shaderPath != null
                // Assets/Shaders/Lit.shader => Shaders/Lit
                ? Path.Combine(Path.GetDirectoryName(shaderPath[7..]) ?? string.Empty, Path.GetFileNameWithoutExtension(shaderPath))
                : GetValidFileName(shader.name.Replace('/', '_').Replace('\\', '_'));

            return Path.Combine(CompiledShaderCacheRootPath, shaderCachePath);
        }

        public static string GetCompiledShaderVariantCacheDirectory(Shader shader, ShaderPerfData shaderPerfData)
        {
            return Path.Combine(
                GetCompiledShaderCacheRootDirectory(shader),
                shaderPerfData.subshaderIndex.ToString(),
                shaderPerfData.passName,
                shaderPerfData.hash);
        }

        public static void ClearShaderPerfCache(Shader shader)
        {
            if (shader == null)
                return;
            
            try
            {
                var shaderDir = GetCompiledShaderCacheRootDirectory(shader);
                if (Directory.Exists(shaderDir))
                    Directory.Delete(shaderDir, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LWGUI: Cleaning the Shader({shader.name}) cache failed: {e.Message}");
            }
        }

        #endregion
    }
}