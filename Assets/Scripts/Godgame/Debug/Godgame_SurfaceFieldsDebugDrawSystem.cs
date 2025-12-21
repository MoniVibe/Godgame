using Godgame.Input;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Streaming;
using PureDOTS.Runtime.WorldGen;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Debugging
{
    /// <summary>
    /// Lightweight visualization for SurfaceFields streaming: draws chunk bounds using Debug.DrawLine.
    /// Toggle via DebugInput.ToggleOverlays (O key).
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_SurfaceFieldsDebugDrawSystem : ISystem
    {
        private bool _enabled;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton(out DebugInput debugInput) && debugInput.ToggleOverlays == 1)
            {
                _enabled = !_enabled;
            }

            if (!_enabled)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton(out SurfaceFieldsDomainConfig domainConfig))
            {
                return;
            }

            var cellSize = math.max(0.01f, domainConfig.CellSize);
            var chunkSize = new float2(domainConfig.CellsPerChunk.x * cellSize, domainConfig.CellsPerChunk.y * cellSize);

            var drawRadius = 2;
            if (SystemAPI.TryGetSingleton(out SurfaceFieldsStreamingConfig streamingConfig))
            {
                drawRadius = math.max(0, streamingConfig.KeepRadiusChunks);
            }

            var centerCoord = int3.zero;
            float3 focusPosition = default;
            var hasFocus = false;
            foreach (var focus in SystemAPI.Query<RefRO<StreamingFocus>>())
            {
                focusPosition = focus.ValueRO.Position;
                hasFocus = true;
                break;
            }

            if (hasFocus)
            {
                centerCoord = ChunkCoordFromWorld(domainConfig, focusPosition);
            }

            foreach (var chunk in SystemAPI.Query<RefRO<SurfaceFieldsChunkComponent>>())
            {
                var coord = chunk.ValueRO.ChunkCoord;
                if (drawRadius > 0)
                {
                    var d = math.max(math.abs(coord.x - centerCoord.x), math.abs(coord.z - centerCoord.z));
                    if (d > drawRadius)
                    {
                        continue;
                    }
                }

                var originXZ = domainConfig.WorldOriginXZ + new float2(coord.x * chunkSize.x, coord.z * chunkSize.y);
                var y = 0.25f;

                var p0 = new Vector3(originXZ.x, y, originXZ.y);
                var p1 = new Vector3(originXZ.x + chunkSize.x, y, originXZ.y);
                var p2 = new Vector3(originXZ.x + chunkSize.x, y, originXZ.y + chunkSize.y);
                var p3 = new Vector3(originXZ.x, y, originXZ.y + chunkSize.y);

                var color = ResolveChunkColor(chunk.ValueRO);

                UnityEngine.Debug.DrawLine(p0, p1, color, 0f, false);
                UnityEngine.Debug.DrawLine(p1, p2, color, 0f, false);
                UnityEngine.Debug.DrawLine(p2, p3, color, 0f, false);
                UnityEngine.Debug.DrawLine(p3, p0, color, 0f, false);
            }
        }

        private static int3 ChunkCoordFromWorld(in SurfaceFieldsDomainConfig domainConfig, float3 worldPosition)
        {
            var cellSize = math.max(0.01f, domainConfig.CellSize);
            var chunkSize = new float2(domainConfig.CellsPerChunk.x * cellSize, domainConfig.CellsPerChunk.y * cellSize);
            var local = worldPosition.xz - domainConfig.WorldOriginXZ;
            var chunk = (int2)math.floor(local / chunkSize);
            return new int3(chunk.x, 0, chunk.y);
        }

        private static Color ResolveChunkColor(in SurfaceFieldsChunkComponent chunk)
        {
            if (!chunk.Chunk.IsCreated)
            {
                return Color.magenta;
            }

            var summary = chunk.Chunk.Value.Summary;
            var total = (double)summary.LandCellCount + summary.WaterCellCount;
            var waterFrac = total > 0 ? (float)(summary.WaterCellCount / total) : 0f;

            var land = new Color(0.15f, 0.65f, 0.2f, 1f);
            var water = new Color(0.15f, 0.35f, 0.95f, 1f);
            return Color.Lerp(land, water, waterFrac);
        }
    }
}
