using System;
using System.IO;
using PureDOTS.Runtime.Scenarios;
using UnityEngine;
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
            if (TryGetArgument(ReportArg, out var reportArg))
            {
                reportPath = ResolvePath(reportArg);
                var directory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            try
            {
                Debug.Log($"[GodgameScenarioEntryPoint] Running scenario '{scenarioPath}' (report: {(string.IsNullOrEmpty(reportPath) ? "<none>" : reportPath)})");
                var result = ScenarioRunnerExecutor.RunFromFile(scenarioPath, reportPath);
                Debug.Log($"[GodgameScenarioEntryPoint] Scenario '{result.ScenarioId}' completed successfully (ticks={result.RunTicks} snapshots={result.SnapshotLogCount}).");
                Quit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GodgameScenarioEntryPoint] Scenario execution failed: {ex}");
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
