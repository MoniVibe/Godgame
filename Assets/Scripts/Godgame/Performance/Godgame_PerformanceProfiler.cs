using System.IO;
using UnityEngine;
using Unity.Profiling;

namespace Godgame.Performance
{
    /// <summary>
    /// MonoBehaviour for capturing Unity Profiler data during profiling runs.
    /// Logs frame time, GC allocations, draw calls, and batches to CSV or log file.
    /// Editor-only component.
    /// </summary>
    public class Godgame_PerformanceProfiler : MonoBehaviour
    {
        [Header("Profiling Settings")]
        [Tooltip("Enable profiling")]
        public bool EnableProfiling = false;

        [Tooltip("Output file path (relative to project root)")]
        public string OutputPath = "Logs/performance_profile.csv";

        [Tooltip("Log interval (frames)")]
        public int LogInterval = 60; // Log every 60 frames (~1 second at 60fps)

        private int _frameCounter;
        private StreamWriter _writer;
        private bool _headerWritten;

        private void Start()
        {
            if (!EnableProfiling)
            {
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            // Create output directory if needed
            string directory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Open file for writing
            _writer = new StreamWriter(OutputPath, append: true);
            _headerWritten = false;
            _frameCounter = 0;
#endif
        }

        private void Update()
        {
            if (!EnableProfiling)
            {
                return;
            }

            _frameCounter++;

            if (_frameCounter >= LogInterval)
            {
                _frameCounter = 0;
                LogProfilerData();
            }
        }

        private void LogProfilerData()
        {
#if GODGAME_LEGACY_DEBUG && UNITY_EDITOR
            if (_writer == null)
            {
                return;
            }

            // Write header if first time
            if (!_headerWritten)
            {
                _writer.WriteLine("Frame,FrameTimeMs,GCAllocKB,DrawCalls,Batches");
                _headerWritten = true;
            }

            // Get profiler data
            float frameTimeMs = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(0) / 1000f; // Placeholder - actual frame time from Profiler
            long gcAllocKB = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(0) / 1024; // Placeholder
            int drawCalls = UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset != null ? 0 : 0; // Placeholder
            int batches = 0; // Placeholder

            // Log to file
            _writer.WriteLine($"{Time.frameCount},{frameTimeMs:F2},{gcAllocKB},{drawCalls},{batches}");
            _writer.Flush();

            // Performance logging disabled for now - will revisit once demos are visually stable
            // If Debug.LogWarning calls exist here, they are disabled via GODGAME_LEGACY_DEBUG guard
            // UnityEngine.Debug.LogWarning($"[Perf] Frame budget (ms): {frameBudget}");
            // UnityEngine.Debug.LogWarning($"[Perf] Average frame time (ms): {avgFrameMs}");
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
#endif
        }

        private void OnApplicationQuit()
        {
            OnDestroy();
        }
    }
}

