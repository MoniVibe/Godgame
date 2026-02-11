using System;
using System.Collections.Generic;
using System.IO;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Scenarios;
using UnityEngine;
using UnityEngine.SceneManagement;
using SystemEnv = System.Environment;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Godgame.Headless
{
	    /// <summary>
	    /// Handles command-line scenario selection when running in batch/headless mode.
	    /// Godgame scenarios are loaded by <see cref="Godgame.Scenario.GodgameScenarioLoaderSystem"/> (not ScenarioRunner).
	    /// </summary>
	    static class GodgameScenarioEntryPoint
	    {
	        private const string ScenarioArg = "--scenario";
	        private const string ReportArg = "--report";
	        private const string PureDotsTelemetryPathEnvVar = "PUREDOTS_TELEMETRY_PATH";
	        private const string PureDotsTelemetryEnableEnvVar = "PUREDOTS_TELEMETRY_ENABLE";
        private const string ScenarioEnvVar = "GODGAME_SCENARIO_PATH";
        private const string HeadlessPresentationEnv = "PUREDOTS_HEADLESS_PRESENTATION";
        private const string PresentationSceneName = "TRI_Godgame_Smoke";
        private static bool s_executed;
        private static bool s_loggedTelemetry;

	        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	        static void LoadPresentationSceneIfRequested()
	        {
	            if (!Application.isBatchMode)
	            {
	                return;
	            }
	            RuntimeMode.RefreshFromEnvironment();
	            if (!RuntimeMode.IsHeadless)
	            {
	                return;
	            }

	            if (!IsTruthy(global::System.Environment.GetEnvironmentVariable(HeadlessPresentationEnv)))
	            {
	                return;
	            }

	            var renderingEnabled = RuntimeMode.IsRenderingEnabled;
	            if (!renderingEnabled)
	            {
	                return;
	            }

	            Debug.Log($"[GodgameScenarioEntryPoint] {HeadlessPresentationEnv}=1 detected; loading presentation scene '{PresentationSceneName}'.");
	            SceneManager.LoadScene(PresentationSceneName, LoadSceneMode.Single);
	        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void RunScenarioIfRequested()
        {
            if (s_executed)
                return;
            if (!Application.isBatchMode)
                return;
            RuntimeMode.RefreshFromEnvironment();
            if (!RuntimeMode.IsHeadless)
                return;
            if (!TryGetArgument(ScenarioArg, out var scenarioArg))
                return;

            s_executed = true;
            if (!TryResolveScenarioPath(scenarioArg, out var scenarioPath))
            {
                Debug.LogError($"[GodgameScenarioEntryPoint] Scenario file not found: {scenarioPath}");
                Quit(1);
                return;
	            }

	            string reportPath = null;
	            string telemetryPath = null;
	            if (TryGetArgument(ReportArg, out var reportArg))
	            {
	                reportPath = ResolvePath(reportArg);
	                var directory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
	                telemetryPath = DeriveTelemetryPath(reportPath);
	                var telemetryDirectory = Path.GetDirectoryName(telemetryPath);
	                if (!string.IsNullOrEmpty(telemetryDirectory))
	                {
	                    Directory.CreateDirectory(telemetryDirectory);
	                }
	            }

	            try
	            {
	                if (LooksLikeScenarioRunnerJson(scenarioPath))
	                {
	                    DisableGodgameHeadlessProofsForScenarioRunner();
	                    var runnerTelemetryPath = SystemEnv.GetEnvironmentVariable(PureDotsTelemetryPathEnvVar);
                    if (string.IsNullOrWhiteSpace(runnerTelemetryPath) && !string.IsNullOrEmpty(telemetryPath))
                    {
                        SystemEnv.SetEnvironmentVariable(PureDotsTelemetryPathEnvVar, telemetryPath);
                        SystemEnv.SetEnvironmentVariable(PureDotsTelemetryEnableEnvVar, "1");
                    }

                    LogTelemetryOutOnce(SystemEnv.GetEnvironmentVariable(PureDotsTelemetryPathEnvVar) ?? "(unset)");
                    var result = ScenarioRunnerExecutor.RunFromFile(scenarioPath, reportPath);
                    Debug.Log($"[GodgameScenarioEntryPoint] ScenarioRunner '{scenarioPath}' completed. ticks={result.RunTicks} snapshots={result.SnapshotLogCount}");
                    if (result.PerformanceBudgetFailed)
                    {
                        var exitPolicy = ScenarioExitUtility.ResolveExitPolicy();
                        var message = $"[GodgameScenarioEntryPoint] Performance budget failure ({result.PerformanceBudgetMetric}) at tick {result.PerformanceBudgetTick}: value={result.PerformanceBudgetValue:F2}, budget={result.PerformanceBudgetLimit:F2}";
                        if (exitPolicy == ExitPolicy.Strict)
                        {
                            Debug.LogError(message);
                            Quit(2);
                        }
                        else
                        {
                            Debug.LogWarning(message);
                            Quit(0);
                        }
                    }
                    else
                    {
                        Quit(0);
                    }
	                    return;
	                }

	                DisableHeadlessProofsForScenario();
	                SystemEnv.SetEnvironmentVariable(ScenarioEnvVar, scenarioPath);

                var existingTelemetryPath = SystemEnv.GetEnvironmentVariable(PureDotsTelemetryPathEnvVar);
                if (!string.IsNullOrWhiteSpace(existingTelemetryPath))
                {
                    LogTelemetryOutOnce(existingTelemetryPath);
                    Debug.Log($"[GodgameScenarioEntryPoint] Scenario='{scenarioPath}', telemetry='{existingTelemetryPath}' (note: Godgame does not emit a ScenarioRunner report; --report is optional and can be used to derive a default telemetry output path).");
                }
                else if (!string.IsNullOrEmpty(telemetryPath))
                {
                    SystemEnv.SetEnvironmentVariable(PureDotsTelemetryPathEnvVar, telemetryPath);
                    SystemEnv.SetEnvironmentVariable(PureDotsTelemetryEnableEnvVar, "1");
                    LogTelemetryOutOnce(telemetryPath);
                    Debug.Log($"[GodgameScenarioEntryPoint] Scenario='{scenarioPath}', telemetry='{telemetryPath}' (note: Godgame does not emit a ScenarioRunner report; --report is used to derive telemetry output).");
                }
                else
                {
                    LogTelemetryOutOnce("(unset)");
                    Debug.Log($"[GodgameScenarioEntryPoint] Scenario='{scenarioPath}' (telemetry path not overridden).");
                }
	            }
	            catch (Exception ex)
	            {
	                Debug.LogError($"[GodgameScenarioEntryPoint] Scenario selection failed: {ex}");
	                Quit(1);
	            }
	        }

        private static bool TryGetArgument(string key, out string value)
        {
            var args = global::System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.Equals(arg, key, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        value = args[i + 1];
                        return true;
                    }
                    break;
                }

                var prefix = key + "=";
                if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    value = arg.Substring(prefix.Length).Trim('"');
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, path));
        }

        private static bool TryResolveScenarioPath(string scenarioArg, out string resolvedPath)
        {
            resolvedPath = ResolvePath(scenarioArg);
            if (File.Exists(resolvedPath))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(scenarioArg))
            {
                return false;
            }

            var trimmed = scenarioArg.Trim();
            var normalized = trimmed.Replace('\\', '/').TrimStart('/');
            var idOnly = Path.GetFileNameWithoutExtension(normalized);
            var candidateRelatives = new List<string>
            {
                Path.Combine("Assets", "Scenarios", "Godgame", idOnly + ".json"),
                Path.Combine("Assets", "Scenarios", idOnly + ".json"),
                Path.Combine("Scenarios", "Godgame", idOnly + ".json"),
                Path.Combine("Scenarios", idOnly + ".json")
            };

            if (!Path.HasExtension(trimmed))
            {
                candidateRelatives.Add(trimmed + ".json");
                candidateRelatives.Add(normalized + ".json");
            }

            for (int i = 0; i < candidateRelatives.Count; i++)
            {
                var candidate = ResolvePath(candidateRelatives[i]);
                if (File.Exists(candidate))
                {
                    resolvedPath = candidate;
                    Debug.Log($"[GodgameScenarioEntryPoint] Resolved scenario '{scenarioArg}' to '{resolvedPath}'.");
                    return true;
                }
            }

            if (TryResolveScenarioPathFromKnownRoots(trimmed, normalized, idOnly, out var rootedPath))
            {
                resolvedPath = rootedPath;
                Debug.Log($"[GodgameScenarioEntryPoint] Resolved scenario '{scenarioArg}' via TRI roots to '{resolvedPath}'.");
                return true;
            }

            return false;
        }

        private static bool TryResolveScenarioPathFromKnownRoots(string rawArg, string normalizedArg, string idOnly, out string resolvedPath)
        {
            resolvedPath = string.Empty;
            var roots = new List<string>();
            AddExistingRoot(roots, SystemEnv.GetEnvironmentVariable("TRI_ROOT"));
            AddExistingRoot(roots, "/home/oni/Tri");
            AddExistingRoot(roots, "/mnt/c/dev/Tri");
            AddExistingRoot(roots, "/mnt/c/dev/tri");
            AddExistingRoot(roots, "/mnt/c/dev");

            var relCandidates = new List<string>
            {
                NormalizeRelativePath(rawArg),
                NormalizeRelativePath(normalizedArg),
                NormalizeRelativePath(Path.Combine("Assets", "Scenarios", "Godgame", idOnly + ".json")),
                NormalizeRelativePath(Path.Combine("Assets", "Scenarios", idOnly + ".json")),
                NormalizeRelativePath(Path.Combine("godgame", "Assets", "Scenarios", "Godgame", idOnly + ".json")),
                NormalizeRelativePath(Path.Combine("godgame", "Assets", "Scenarios", idOnly + ".json"))
            };

            if (!Path.HasExtension(rawArg))
            {
                relCandidates.Add(NormalizeRelativePath(rawArg + ".json"));
                relCandidates.Add(NormalizeRelativePath(normalizedArg + ".json"));
            }

            var refHints = new List<string>
            {
                SystemEnv.GetEnvironmentVariable("GIT_COMMIT"),
                SystemEnv.GetEnvironmentVariable("GIT_BRANCH")
            };

            foreach (var root in roots)
            {
                for (int i = 0; i < relCandidates.Count; i++)
                {
                    var rel = relCandidates[i];
                    if (string.IsNullOrWhiteSpace(rel))
                    {
                        continue;
                    }

                    var candidate = Path.GetFullPath(Path.Combine(root, rel));
                    if (File.Exists(candidate))
                    {
                        resolvedPath = candidate;
                        return true;
                    }
                }

                for (int i = 0; i < refHints.Count; i++)
                {
                    var safeRef = SanitizeRef(refHints[i]);
                    if (string.IsNullOrWhiteSpace(safeRef))
                    {
                        continue;
                    }

                    var wtCandidate = Path.Combine(root, ".tri", "worktrees", "godgame", safeRef, "Assets", "Scenarios", "Godgame", idOnly + ".json");
                    if (File.Exists(wtCandidate))
                    {
                        resolvedPath = wtCandidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static string NormalizeRelativePath(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return input.Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar)
                        .TrimStart(Path.DirectorySeparatorChar);
        }

        private static string SanitizeRef(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = value.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                var keep = char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-';
                if (!keep)
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static void AddExistingRoot(List<string> roots, string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(root);
            }
            catch
            {
                return;
            }

            if (!Directory.Exists(fullPath))
            {
                return;
            }

            for (int i = 0; i < roots.Count; i++)
            {
                if (string.Equals(roots[i], fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            roots.Add(fullPath);
        }

	        private static string DeriveTelemetryPath(string reportPath)
	        {
	            if (string.IsNullOrWhiteSpace(reportPath))
	            {
                return string.Empty;
            }

	            var directory = Path.GetDirectoryName(reportPath);
	            var name = Path.GetFileNameWithoutExtension(reportPath);
	            var telemetryFile = $"{name}_telemetry.ndjson";
	            return string.IsNullOrEmpty(directory) ? telemetryFile : Path.Combine(directory, telemetryFile);
	        }

	        private static bool LooksLikeScenarioRunnerJson(string scenarioPath)
	        {
	            const int charsToRead = 4096;
	            using var stream = File.OpenRead(scenarioPath);
	            using var reader = new StreamReader(stream);
	            var buffer = new char[charsToRead];
	            var read = reader.ReadBlock(buffer, 0, buffer.Length);
	            var head = read > 0 ? new string(buffer, 0, read) : string.Empty;
	            return head.Contains("\"runTicks\"", StringComparison.OrdinalIgnoreCase) ||
	                   head.Contains("\"inputCommands\"", StringComparison.OrdinalIgnoreCase) ||
	                   head.Contains("\"scenarioId\"", StringComparison.OrdinalIgnoreCase);
	        }

	        private static void DisableHeadlessProofsForScenario()
	        {
	            SetEnvIfUnset("PUREDOTS_HEADLESS_TIME_PROOF", "0");
	            SetEnvIfUnset("PUREDOTS_HEADLESS_REWIND_PROOF", "0");
	        }

	        private static void DisableGodgameHeadlessProofsForScenarioRunner()
	        {
	            SetEnvIfUnset("GODGAME_HEADLESS_VILLAGER_PROOF", "0");
	            SetEnvIfUnset("GODGAME_HEADLESS_NEEDS_PROOF", "0");
	            SetEnvIfUnset("GODGAME_HEADLESS_COMBAT_PROOF", "0");
	            SetEnvIfUnset("GODGAME_HEADLESS_VILLAGE_BUILD_PROOF", "0");
	        }

	        private static void SetEnvIfUnset(string key, string value)
	        {
	            if (string.IsNullOrWhiteSpace(SystemEnv.GetEnvironmentVariable(key)))
	            {
	                SystemEnv.SetEnvironmentVariable(key, value);
	            }
	        }

        private static bool IsTruthy(string value)
        {
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static void LogTelemetryOutOnce(string telemetryPath)
        {
            if (s_loggedTelemetry)
            {
                return;
            }

            s_loggedTelemetry = true;
            Debug.Log($"TELEMETRY_OUT:{telemetryPath}");
        }

        private static void Quit(int exitCode)
        {
#if UNITY_EDITOR
            EditorApplication.Exit(exitCode);
#else
            Application.Quit(exitCode);
#endif
        }
    }
}
