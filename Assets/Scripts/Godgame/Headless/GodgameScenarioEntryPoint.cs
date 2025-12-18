using System;
using System.IO;
using PureDOTS.Runtime.Scenarios;
using UnityEngine;
using SystemEnv = System.Environment;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Godgame.Headless
{
    /// <summary>
    /// Handles command-line scenario execution when running in batch/headless mode.
    /// </summary>
    static class GodgameScenarioEntryPoint
    {
        private const string ScenarioArg = "--scenario";
        private const string ReportArg = "--report";
        private const string TelemetryEnvVar = "GODGAME_BEHAVIOR_TELEMETRY_PATH";
        private const string ScenarioEnvVar = "GODGAME_SCENARIO_PATH";
        private static bool s_executed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RunScenarioIfRequested()
        {
            if (s_executed)
                return;
            if (!Application.isBatchMode)
                return;
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
            var scenarioOverridePath = scenarioPath;
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
                if (!string.IsNullOrEmpty(telemetryPath))
                {
                    SystemEnv.SetEnvironmentVariable(TelemetryEnvVar, telemetryPath);
                }
                else
                {
                    SystemEnv.SetEnvironmentVariable(TelemetryEnvVar, null);
                }

                Debug.Log($"[GodgameScenarioEntryPoint] Running scenario '{scenarioPath}' (report: {(string.IsNullOrEmpty(reportPath) ? "<none>" : reportPath)})");
                SystemEnv.SetEnvironmentVariable(ScenarioEnvVar, scenarioOverridePath);
                var result = ScenarioRunnerExecutor.RunFromFile(scenarioPath, reportPath);
                Debug.Log($"[GodgameScenarioEntryPoint] Scenario '{result.ScenarioId}' completed successfully (ticks={result.RunTicks} snapshots={result.SnapshotLogCount}).");
                LogScenarioIssues(result);
                if (ScenarioExitUtility.ShouldExitNonZero(result, out var severity))
                {
                    var exitCode = severity == ScenarioSeverity.Fatal ? 2 : 3;
                    Debug.LogError($"[GodgameScenarioEntryPoint] Scenario '{result.ScenarioId}' completed with severity {severity}.");
                    Quit(exitCode);
                }
                else
                {
                    Quit(0);
                }
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GodgameScenarioEntryPoint] Scenario execution failed: {ex}");
                Quit(1);
            }
            finally
            {
                SystemEnv.SetEnvironmentVariable(TelemetryEnvVar, null);
                SystemEnv.SetEnvironmentVariable(ScenarioEnvVar, null);
            }
        }

        private static void LogScenarioIssues(in ScenarioRunResult result)
        {
            if (result.Issues == null || result.Issues.Count == 0)
            {
                return;
            }

            foreach (var issue in result.Issues)
            {
                Debug.Log($"[GodgameScenarioEntryPoint] Issue {issue.Kind}/{issue.Severity} ({issue.Code.ToString()}): {issue.Message.ToString()}");
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
            var telemetryFile = $"{name}_behavior.ndjson";
            return string.IsNullOrEmpty(directory) ? telemetryFile : Path.Combine(directory, telemetryFile);
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
