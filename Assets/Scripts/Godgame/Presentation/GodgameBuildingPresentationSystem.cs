using Godgame.Buildings;
using Godgame.Presentation;
using Godgame.Rendering;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Ensures storehouse/housing/worship entities have presentation tags and baseline render state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(GodgamePresentationRegistryIdentitySystem))]
    public partial struct Godgame_BuildingPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StorehouseConfig>();
            state.RequireForUpdate<HousingDefinition>();
            state.RequireForUpdate<WorshipDefinition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<StorehouseConfig>>()
                         .WithAll<LocalTransform>()
                         .WithNone<StorehousePresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<StorehousePresentationTag>(entity);
                EnsurePresentationDefaults(ref state, ref ecb, entity, GodgameSemanticKeys.Storehouse);
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<HousingDefinition>>()
                         .WithAll<LocalTransform>()
                         .WithNone<HousingPresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<HousingPresentationTag>(entity);
                EnsurePresentationDefaults(ref state, ref ecb, entity, GodgameSemanticKeys.Housing);
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<WorshipDefinition>>()
                         .WithAll<LocalTransform>()
                         .WithNone<WorshipPresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<WorshipPresentationTag>(entity);
                EnsurePresentationDefaults(ref state, ref ecb, entity, GodgameSemanticKeys.Worship);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void EnsurePresentationDefaults(ref SystemState state, ref EntityCommandBuffer ecb, Entity entity, ushort semanticKey)
        {
            if (!SystemAPI.HasComponent<PresentationLODState>(entity))
            {
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
            }

            if (!SystemAPI.HasComponent<PresentationLayer>(entity))
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Colony });
            }

            if (!SystemAPI.HasComponent<RenderSemanticKey>(entity))
            {
                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, semanticKey, default);
            }

            if (!SystemAPI.HasComponent<RenderTint>(entity))
            {
                var tint = GodgamePresentationColors.ForBuilding(semanticKey);
                ecb.AddComponent(entity, new RenderTint { Value = tint });
            }
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
