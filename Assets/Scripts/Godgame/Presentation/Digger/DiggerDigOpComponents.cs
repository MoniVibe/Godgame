#if GODGAME_HAS_DIGGER
using System;
using PureDOTS.Environment;
using PureDOTS.Runtime.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation.Digger
{
    [InternalBufferCapacity(32)]
    public struct DiggerDigOp : IBufferElementData
    {
        public TerrainModificationKind Kind;
        public TerrainModificationShape Shape;
        public float3 Start;
        public float3 End;
        public float Radius;
        public float Depth;
        public byte MaterialId;
        public uint Tick;
    }

    public struct DiggerDigOpQueue : IComponentData { }

    internal static class DiggerViewGate
    {
        private const string EnableEnvVar = "GODGAME_ENABLE_DIGGER_VIEW";

        public static bool IsEnabled
        {
            get
            {
                if (!RuntimeMode.IsRenderingEnabled)
                {
                    return false;
                }

                var value = global::System.Environment.GetEnvironmentVariable(EnableEnvVar);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }

                value = value.Trim();
                if (IsFalse(value))
                {
                    return false;
                }

                return IsTrue(value);
            }
        }

        private static bool IsTrue(string value)
        {
            return value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFalse(string value)
        {
            return value.Equals("0", StringComparison.OrdinalIgnoreCase)
                || value.Equals("false", StringComparison.OrdinalIgnoreCase)
                || value.Equals("no", StringComparison.OrdinalIgnoreCase)
                || value.Equals("off", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#else
using PureDOTS.Environment;
using PureDOTS.Runtime.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation.Digger
{
    [InternalBufferCapacity(32)]
    public struct DiggerDigOp : IBufferElementData
    {
        public TerrainModificationKind Kind;
        public TerrainModificationShape Shape;
        public float3 Start;
        public float3 End;
        public float Radius;
        public float Depth;
        public byte MaterialId;
        public uint Tick;
    }

    public struct DiggerDigOpQueue : IComponentData { }

    internal static class DiggerViewGate
    {
        public static bool IsEnabled => false;
    }
}
#endif
