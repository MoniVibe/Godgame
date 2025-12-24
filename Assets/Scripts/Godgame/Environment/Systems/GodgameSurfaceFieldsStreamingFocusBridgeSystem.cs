using PureDOTS.Runtime.Camera;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Streaming;
using PureDOTS.Runtime.WorldGen;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Systems
{
    public struct GodgameSurfaceFieldsStreamingFocusTag : IComponentData
    {
    }

    /// <summary>
    /// Feeds the Godgame camera rig focus point into a <see cref="StreamingFocus"/> so SurfaceFields chunk streaming can follow the camera.
    /// Reads from CameraRigService (standard pipeline) instead of ECS CameraRigState.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.RecordSimulationSystemGroup))]
    public partial struct GodgameSurfaceFieldsStreamingFocusBridgeSystem : ISystem
    {
        private EntityQuery _focusQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<SurfaceFieldsChunkRequestQueue>();

            _focusQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<GodgameSurfaceFieldsStreamingFocusTag>(),
                ComponentType.ReadWrite<StreamingFocus>());
        }

        public void OnUpdate(ref SystemState state)
        {
            // Read from CameraRigService instead of ECS CameraRigState
            if (!CameraRigService.TryGetState(out var rigState))
            {
                return; // Camera not initialized yet
            }

            var desiredPosition = (float3)rigState.Focus;
            var deltaTime = SystemAPI.Time.DeltaTime;

            var entityManager = state.EntityManager;
            var focusEntity = _focusQuery.IsEmptyIgnoreFilter
                ? entityManager.CreateEntity(typeof(GodgameSurfaceFieldsStreamingFocusTag), typeof(StreamingFocus))
                : _focusQuery.GetSingletonEntity();

            var focus = entityManager.GetComponentData<StreamingFocus>(focusEntity);
            var previousPosition = focus.Position;

            focus.Position = desiredPosition;
            focus.Velocity = deltaTime > 0f ? (desiredPosition - previousPosition) / deltaTime : float3.zero;
            focus.RadiusScale = math.max(0.01f, focus.RadiusScale == 0f ? 1f : focus.RadiusScale);

            entityManager.SetComponentData(focusEntity, focus);
        }
    }
}

