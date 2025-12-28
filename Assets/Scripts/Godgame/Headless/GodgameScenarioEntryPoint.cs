using System;
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
            var scenarioPath = ResolvePath(scenarioArg);
            if (!File.Exists(scenarioPath))
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
	                    var runnerTelemetryPath = SystemEnv.GetEnvironmentVariable(PureDotsTelemetryPathEnvVar);
	                    if (string.IsNullOrWhiteSpace(runnerTelemetryPath) && !string.IsNullOrEmpty(telemetryPath))
	                    {
	                        SystemEnv.SetEnvironmentVariable(PureDotsTelemetryPathEnvVar, telemetryPath);
	                        SystemEnv.SetEnvironmentVariable(PureDotsTelemetryEnableEnvVar, "1");
	                    }

	                    var result = ScenarioRunnerExecutor.RunFromFile(scenarioPath, reportPath);
	                    Debug.Log($"[GodgameScenarioEntryPoint] ScenarioRunner '{scenarioPath}' completed. ticks={result.RunTicks} snapshots={result.SnapshotLogCount}");
	                    if (result.PerformanceBudgetFailed)
	                    {
	                        Debug.LogError($"[GodgameScenarioEntryPoint] Performance budget failure ({result.PerformanceBudgetMetric}) at tick {result.PerformanceBudgetTick}: value={result.PerformanceBudgetValue:F2}, budget={result.PerformanceBudgetLimit:F2}");
	                        Quit(2);
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
	                    Debug.Log($"[GodgameScenarioEntryPoint] Scenario='{scenarioPath}', telemetry='{existingTelemetryPath}' (note: Godgame does not emit a ScenarioRunner report; --report is optional and can be used to derive a default telemetry output path).");
	                }
	                else if (!string.IsNullOrEmpty(telemetryPath))
	                {
	                    SystemEnv.SetEnvironmentVariable(PureDotsTelemetryPathEnvVar, telemetryPath);
	                    SystemEnv.SetEnvironmentVariable(PureDotsTelemetryEnableEnvVar, "1");
	                    Debug.Log($"[GodgameScenarioEntryPoint] Scenario='{scenarioPath}', telemetry='{telemetryPath}' (note: Godgame does not emit a ScenarioRunner report; --report is used to derive telemetry output).");
	                }
	                else
	                {
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
