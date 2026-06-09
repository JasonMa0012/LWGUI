// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using LWGUI.PerformanceMonitor;
using Debug = UnityEngine.Debug;

namespace LWGUI.PerformanceMonitor.ShaderCompiler
{
    public class ShaderCompilerDefaultFxc : IShaderCompiler
    {
        // Per-compiler stats structure moved here so FXC owns its output data shape.
        public struct ShaderPerfStats
        {
            public float estimatedCost; // Estimated relative performance cost based on experience, not precise results.
            public int   sampleCount;
            public int   samplerCount;
            public int   registerCount;
            public int   interpolatorChannelCount;

            public bool isValid;
        }

        private static ShaderCompilerDefaultFxc _instance;
        public static  ShaderCompilerDefaultFxc instance => _instance ??= new ShaderCompilerDefaultFxc();

        private static int _isSupportCurrentPlatform = -1;

        public static bool isSupportCurrentPlatform
        {
            get
            {
#if UNITY_STANDALONE_WIN
                if (_isSupportCurrentPlatform == -1)
                    _isSupportCurrentPlatform = !string.IsNullOrEmpty(_fxcAbsPath) ? 1 : 0;
                return _isSupportCurrentPlatform == 1;
#else
                return false;
#endif
            }
        }

        public static int priority => 0;
        
        public string compilerName => "Default Fxc";
        
        public ShaderCompilerPlatform api    { get; set; } = ShaderCompilerPlatform.D3D;
        public BuildTarget            target { get; set; } = BuildTarget.StandaloneWindows64;
        public GraphicsTier           tier   { get; set; } = (GraphicsTier)(-1);
        
        public string GetCompiledShaderPath(ShaderPerfData shaderPerfData, string compiledShaderDirectory, string shaderTypeName)
            => Path.Combine(compiledShaderDirectory, $"Fxc_{api}_{target}_{shaderTypeName}.txt");

        public string GetCompiledDxbcPath(ShaderPerfData shaderPerfData)
            => Path.Combine(shaderPerfData.compiledShaderDirectory, $"Fxc_{api}_{target}_{shaderPerfData.shaderTypeName}.dxbc");

        public bool CompilePass(ShaderPerfData shaderPerfData, ShaderData.Pass pass, ShaderType shaderType, string[] keywords,
                                out string     compiledShader)
        {
            compiledShader = string.Empty;

            if (shaderPerfData == null || pass == null || keywords == null)
                return false;

            if (string.IsNullOrEmpty(_fxcAbsPath) || !File.Exists(_fxcAbsPath))
                return false;

            var compileInfo = pass.CompileVariant(shaderType, keywords, api, target, tier, true);
            if (!compileInfo.Success)
                return false;

            // Write DXBC
            var dxbcPath = GetCompiledDxbcPath(shaderPerfData);
            IOHelper.WriteBinaryFile(dxbcPath, compileInfo.ShaderData);

            // Disassemble With fxc.exe
            return IOHelper.RunProcess(_fxcAbsPath, $"/dumpbin \"{dxbcPath}\"", out compiledShader);
        }

        public object AnalyzeShaderPerformance(ShaderPerfData shaderPerfData, string compiledShader)
            => ParseAsmStats(compiledShader);

        public void DrawShaderPerformanceStatsHeader(LWGUIMetaDatas metaDatas)
        {
            EditorGUILayout.LabelField(" ", "       Cost   Samples   Registers");
        }

        public void DrawShaderPerformanceStatsLine(LWGUIMetaDatas metaDatas, ShaderPerfData shaderPerfData)
        {
            EditorGUILayout.BeginHorizontal();

            var statsObj = shaderPerfData.stats;
            if (statsObj is ShaderPerfStats { isValid: true } stats)
            {
                var statsStr = $"{stats.estimatedCost,7:0.0} {stats.sampleCount,7:0} {stats.registerCount,8:0}";
                EditorGUILayout.LabelField($"{shaderPerfData.passName} | {shaderPerfData.shaderTypeName}", statsStr, GUIStyles.label_monospace);

                ToolbarHelper.DrawShaderPerformanceStatsLineButtons(shaderPerfData);
            }
            else
            {
                var status = shaderPerfData.isCompiledSuccessful ? "ANALYSIS FAILED" : "COMPILATION FAILED";
                EditorGUILayout.LabelField($"{shaderPerfData.passName} | {shaderPerfData.shaderTypeName}", status);
            }

            EditorGUILayout.EndHorizontal();
        }


        private static string _cachedFxcPath;
        private static bool   _fxcPathChecked;

        private static string _fxcAbsPath
        {
            get
            {
                if (!_fxcPathChecked)
                {
                    _fxcPathChecked = true;
                    _cachedFxcPath = FindFxcInWindowsSdk();
                }
                if (string.IsNullOrEmpty(_cachedFxcPath))
                    Debug.LogError("LWGUI: Can not find fxc.exe! Please install Windows SDK.");
                return _cachedFxcPath;
            }
        }

        private static string FindFxcInWindowsSdk()
        {
            var win10BinDir = @"C:\Program Files (x86)\Windows Kits\10\bin";
            if (Directory.Exists(win10BinDir))
            {
                var versions = Directory.GetDirectories(win10BinDir);
                Array.Sort(versions, (a, b) => string.Compare(b, a, StringComparison.OrdinalIgnoreCase));
                foreach (var versionDir in versions)
                {
                    var fxc = Path.Combine(versionDir, "x64", "fxc.exe");
                    if (File.Exists(fxc)) return fxc;
                    fxc = Path.Combine(versionDir, "x86", "fxc.exe");
                    if (File.Exists(fxc)) return fxc;
                }
            }

            var win81X64 = @"C:\Program Files (x86)\Windows Kits\8.1\bin\x64\fxc.exe";
            if (File.Exists(win81X64)) return win81X64;

            var win81X86 = @"C:\Program Files (x86)\Windows Kits\8.1\bin\x86\fxc.exe";
            if (File.Exists(win81X86)) return win81X86;

            return null;
        }

        // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/shader-model-5-assembly--directx-hlsl-
        private static readonly Dictionary<string, float> _opcodeWeight = new()
        {
            // ALU
            { "add", 1.0f },
            { "and", 1.0f },
            { "bfi", 1.0f },
            { "bfrev", 1.0f },
            { "countbits", 1.0f },
            { "dadd", 4.0f },
            { "ddiv", 16.0f },
            { "deq", 4.0f },
            { "deriv_rtx_coarse", 1.0f },
            { "deriv_rtx_fine", 1.0f },
            { "deriv_rty_coarse", 1.0f },
            { "deriv_rty_fine", 1.0f },
            { "dfma", 4.0f },
            { "dge", 4.0f },
            { "div", 8.0f },
            { "dlt", 4.0f },
            { "dmax", 4.0f },
            { "dmin", 4.0f },
            { "dmov", 1.0f },
            { "dmovc", 1.0f },
            { "dmul", 4.0f },
            { "dne", 4.0f },
            { "dp2", 2.0f },
            { "dp3", 3.0f },
            { "dp4", 4.0f },
            { "drcp", 8.0f },
            { "eq", 1.0f },
            { "exp", 4.0f },
            { "f16tof32", 1.0f },
            { "f32tof16", 1.0f },
            { "firstbit", 1.0f },
            { "frc", 1.0f },
            { "ftod", 2.0f },
            { "ftoi", 1.0f },
            { "ftou", 1.0f },
            { "ge", 1.0f },
            { "iadd", 1.0f },
            { "ibfe", 1.0f },
            { "ieq", 1.0f },
            { "ige", 1.0f },
            { "ilt", 1.0f },
            { "imad", 1.0f },
            { "imin", 1.0f },
            { "imul", 1.0f },
            { "ine", 1.0f },
            { "ineg", 1.0f },
            { "ishl", 1.0f },
            { "ishr", 1.0f },
            { "itof", 1.0f },
            { "log", 4.0f },
            { "lt", 1.0f },
            { "mad", 1.0f },
            { "max", 1.0f },
            { "min", 1.0f },
            { "mov", 1.0f },
            { "movc", 1.0f },
            { "mul", 1.0f },
            { "ne", 1.0f },
            { "not", 1.0f },
            { "or", 1.0f },
            { "rcp", 4.0f },
            { "round_ne", 1.0f },
            { "round_ni", 1.0f },
            { "round_pi", 1.0f },
            { "round_z", 1.0f },
            { "rsq", 4.0f },
            { "sincos", 4.0f },
            { "sqrt", 4.0f },
            { "swapc", 1.0f },
            { "uaddc", 1.0f },
            { "ubfe", 1.0f },
            { "udiv", 8.0f },
            { "uge", 1.0f },
            { "ult", 1.0f },
            { "umad", 1.0f },
            { "umax", 1.0f },
            { "umin", 1.0f },
            { "umul", 1.0f },
            { "ushr", 1.0f },
            { "usubb", 1.0f },
            { "utof", 1.0f },
            { "xor", 1.0f },

            // Non-ALU (ignored)
            { "atomic_and", 0f },
            { "atomic_cmp_store", 0f },
            { "atomic_iadd", 0f },
            { "atomic_imax", 0f },
            { "atomic_imin", 0f },
            { "atomic_or", 0f },
            { "atomic_umax", 0f },
            { "atomic_umin", 0f },
            { "atomic_xor", 0f },
            { "break", 0f },
            { "breakc", 0f },
            { "bufinfo", 0f },
            { "call", 0f },
            { "callc", 0f },
            { "case", 0f },
            { "continue", 0f },
            { "continuec", 0f },
            { "cut", 0f },
            { "cut_stream", 0f },
            { "dcl_constantbuffer", 0f },
            { "dcl_function_body", 0f },
            { "dcl_function_table", 0f },
            { "dcl_globalflags", 0f },
            { "dcl_hs_fork_phase_instance_count", 0f },
            { "dcl_hs_join_phase_instance_count", 0f },
            { "dcl_hs_max_tessfactor", 0f },
            { "dcl_immediateconstantbuffer", 0f },
            { "dcl_indexabletemp", 0f },
            { "dcl_indexrange", 0f },
            { "dcl_input", 0f },
            { "dcl_input vforkinstanceid", 0f },
            { "dcl_input vgsinstanceid", 0f },
            { "dcl_input vjoininstanceid", 0f },
            { "dcl_input voutputcontrolpointid", 0f },
            { "dcl_input vprim", 0f },
            { "dcl_input vthread", 0f },
            { "dcl_input_control_point_count", 0f },
            { "dcl_input_sv", 0f },
            { "dcl_inputprimitive", 0f },
            { "dcl_interface", 0f },
            { "dcl_interface_dynamicindexed", 0f },
            { "dcl_maxoutputvertexcount", 0f },
            { "dcl_output", 0f },
            { "dcl_output odepth", 0f },
            { "dcl_output omask", 0f },
            { "dcl_output_control_point_count", 0f },
            { "dcl_output_sgv", 0f },
            { "dcl_output_siv", 0f },
            { "dcl_outputtopology", 0f },
            { "dcl_resource", 0f },
            { "dcl_resource raw", 0f },
            { "dcl_resource structured", 0f },
            { "dcl_sampler", 0f },
            { "dcl_stream", 0f },
            { "dcl_temps", 0f },
            { "dcl_tessellator_domain", 0f },
            { "dcl_tessellator_output_primitive", 0f },
            { "dcl_tessellator_partitioning", 0f },
            { "dcl_tgsm_raw", 0f },
            { "dcl_tgsm_structured", 0f },
            { "dcl_thread_group", 0f },
            { "dcl_uav_raw", 0f },
            { "dcl_uav_structured", 0f },
            { "dcl_uav_typed", 0f },
            { "default", 0f },
            { "discard", 0f },
            { "else", 0f },
            { "emit", 0f },
            { "emit_stream", 0f },
            { "emitthencut", 0f },
            { "emitthencut_stream", 0f },
            { "endif", 0f },
            { "endloop", 0f },
            { "endswitch", 0f },
            { "fcall", 0f },
            { "gather4", 0f },
            { "gather4_c", 0f },
            { "gather4_po", 0f },
            { "gather4_po_c", 0f },
            { "hs_control_point_phase", 0f },
            { "hs_decls", 0f },
            { "hs_fork_phase", 0f },
            { "hs_join_phase", 0f },
            { "if", 0f },
            { "imm_atomic_alloc", 0f },
            { "imm_atomic_and", 0f },
            { "imm_atomic_cmp_exch", 0f },
            { "imm_atomic_consume", 0f },
            { "imm_atomic_exch", 0f },
            { "imm_atomic_iadd", 0f },
            { "imm_atomic_imax", 0f },
            { "imm_atomic_imin", 0f },
            { "imm_atomic_or", 0f },
            { "imm_atomic_umax", 0f },
            { "imm_atomic_umin", 0f },
            { "imm_atomic_xor", 0f },
            { "label", 0f },
            { "ld", 0f },
            { "ld_raw", 0f },
            { "ld_structured", 0f },
            { "ld_uav_typed", 0f },
            { "ld2dms", 0f },
            { "lod", 0f },
            { "loop", 0f },
            { "nop", 0f },
            { "resinfo", 0f },
            { "ret", 0f },
            { "retc", 0f },
            { "sample", 0f },
            { "sample_b", 0f },
            { "sample_c", 0f },
            { "sample_c_lz", 0f },
            { "sample_d", 0f },
            { "sample_l", 0f },
            { "sampleinfo", 0f },
            { "samplepos", 0f },
            { "store_raw", 0f },
            { "store_structured", 0f },
            { "store_uav_typed", 0f },
            { "switch", 0f },
            { "sync", 0f },
        };

        private static int IndexOfWhitespace(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (char.IsWhiteSpace(c)) return i;
            }
            return -1;
        }

        private static string NormalizeOpcode(string opcode)
        {
            // normalize to lower, strip optional _sat suffix used in some ops like mul_sat/mov_sat
            // opcode = opcode.ToLowerInvariant();
            if (opcode.EndsWith("_sat", StringComparison.Ordinal))
                opcode = opcode.Substring(0, opcode.Length - 3);
            return opcode;
        }

        private static ShaderPerfStats ParseAsmStats(string asmText)
        {
            if (string.IsNullOrEmpty(asmText))
            {
                return new ShaderPerfStats();
            }

            var totalCost = 0f;
            var sampleCount = 0;
            var samplers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var registerCount = 0;
            var interpChannels = 0;
            using (var reader = new StringReader(asmText))
            {
                int lineIndex = -1;
                while (reader.ReadLine() is { } line)
                {
                    lineIndex++;
                    line = line.Trim();

                    if (lineIndex < 2) continue;
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("//")) continue;

                    // TODO: Statistical flow control and other special instructions

                    var firstSpace = IndexOfWhitespace(line);
                    // Skip headers like ps_#_# labels not considered
                    if (firstSpace <= 0) continue;

                    var opcode = line.Substring(0, firstSpace).Trim();
                    opcode = NormalizeOpcode(opcode);

                    if (string.IsNullOrWhiteSpace(opcode)) continue;
                    if (!char.IsLetter(opcode[0])) continue;

                    if (_opcodeWeight.TryGetValue(opcode, out var w))
                    {
                        totalCost += w;
                    }
                    else
                    {
                        Debug.LogWarning($"LWGUI: {typeof(ShaderCompilerDefaultFxc)}: Unknown opcode: {opcode}");
                    }

                    // Texture sampling stats
                    if (opcode.StartsWith("sample", StringComparison.Ordinal) || opcode.StartsWith("gather4", StringComparison.Ordinal))
                    {
                        sampleCount++;
                    }
                    else if (string.Equals(opcode, "ld", StringComparison.Ordinal))
                    {
                        // count ld reading from textures t#
                        if (line.Contains(", t")) sampleCount++;
                    }

                    // Sampler declarations
                    if (opcode == "dcl_sampler")
                    {
                        // e.g. dcl_sampler s1, mode_default
                        var idx = line.IndexOf('s');
                        if (idx >= 0)
                        {
                            var end = idx + 1;
                            while (end < line.Length && char.IsDigit(line[end])) end++;
                            if (end > idx)
                            {
                                var sid = line.Substring(idx, end - idx);
                                samplers.Add(sid);
                            }
                        }
                    }

                    // Temp registers
                    if (opcode == "dcl_temps")
                    {
                        // e.g. dcl_temps 27
                        var numStr = line.Substring(firstSpace).Trim();
                        int.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out registerCount);
                    }

                    // Interpolator channels: dcl_input_ps lines only
                    if (opcode == "dcl_input_ps")
                    {
                        var dot = line.IndexOf('.');
                        if (dot >= 0)
                        {
                            int count = 0;
                            for (int i = dot + 1; i < line.Length; i++)
                            {
                                var c = line[i];
                                if (c == 'x' || c == 'y' || c == 'z' || c == 'w') count++;
                                else break;
                            }
                            interpChannels += count;
                        }
                    }
                }
            }

            return new ShaderPerfStats
            {
                estimatedCost = totalCost,
                sampleCount = sampleCount,
                samplerCount = samplers.Count,
                registerCount = registerCount,
                interpolatorChannelCount = interpChannels,
                isValid = true
            };
        }
    }
}