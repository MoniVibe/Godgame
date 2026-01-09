#if GODGAME_HAS_DIGGER
using PureDOTS.Environment;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation.Digger
{
    [UpdateInGroup(typeof(PureDOTS.Systems.EnvironmentSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.Environment.TerrainModificationApplySystem))]
    public partial struct Godgame_DiggerDigOpBridgeSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _localTransformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<TerrainModificationQueue>();
            _localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!DiggerViewGate.IsEnabled || !IsPresentationWorld(state.World))
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            if (!SystemAPI.TryGetSingletonEntity<TerrainModificationQueue>(out var queueEntity))
            {
                return;
            }

            var requests = SystemAPI.GetBuffer<TerrainModificationRequest>(queueEntity);
            if (requests.Length == 0)
            {
                return;
            }

            var diggerQueueEntity = EnsureQueue(ref state);
            var diggerOps = state.EntityManager.GetBuffer<DiggerDigOp>(diggerQueueEntity);
            _localTransformLookup.Update(ref state);

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                ResolveWorldPositions(request, ref _localTransformLookup, out var start, out var end);

                diggerOps.Add(new DiggerDigOp
                {
                    Kind = request.Kind,
                    Shape = request.Shape,
                    Start = start,
                    End = end,
                    Radius = math.max(0f, request.Radius),
                    Depth = math.max(0f, request.Depth),
                    MaterialId = request.MaterialId,
                    Tick = request.RequestedTick == 0 ? timeState.Tick : request.RequestedTick
                });
            }
        }

        private static Entity EnsureQueue(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonEntity<DiggerDigOpQueue>(out var entity))
            {
                return entity;
            }

            entity = state.EntityManager.CreateEntity(typeof(DiggerDigOpQueue));
            state.EntityManager.AddBuffer<DiggerDigOp>(entity);
            state.EntityManager.SetName(entity, "DiggerDigOpQueue");
            return entity;
        }

        private static bool IsPresentationWorld(World world)
        {
            var flags = world.Flags;
            if ((flags & WorldFlags.Game) == 0 && (flags & WorldFlags.GameClient) == 0)
            {
                return false;
            }

            if ((flags & WorldFlags.GameServer) != 0)
            {
                return false;
            }

            return true;
        }

        private static void ResolveWorldPositions(
            in TerrainModificationRequest request,
            ref ComponentLookup<LocalTransform> localTransformLookup,
            out float3 start,
            out float3 end)
        {
            start = request.Start;
            end = request.End;

            if (request.Space != TerrainModificationSpace.VolumeLocal || request.VolumeEntity == Entity.Null)
            {
                return;
            }

            if (!localTransformLookup.HasComponent(request.VolumeEntity))
            {
                return;
            }

            var localTransform = localTransformLookup[request.VolumeEntity];
            var matrix = float4x4.TRS(localTransform.Position, localTransform.Rotation, new float3(localTransform.Scale));
            start = math.transform(matrix, start);
            end = math.transform(matrix, end);
        }
    }
}
#else
using Unity.Entities;

namespace Godgame.Presentation.Digger
{
    public partial struct Godgame_DiggerDigOpBridgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }
        public void OnUpdate(ref SystemState state) { }
    }
}
#endif
