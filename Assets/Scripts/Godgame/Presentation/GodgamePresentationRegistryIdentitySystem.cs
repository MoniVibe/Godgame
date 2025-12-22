using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Presentation
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct GodgamePresentationRegistryIdentitySystem : ISystem
    {
        private RegistryId _villagerId;
        private RegistryId _villageCenterId;
        private RegistryId _resourceChunkId;
        private RegistryId _resourceNodeId;
        private RegistryId _vegetationId;
        private RegistryId _storehouseId;
        private RegistryId _housingId;
        private RegistryId _worshipId;
        private RegistryId _constructionGhostId;
        private RegistryId _bandId;
        private RegistryId _ghostTetherId;

        public void OnCreate(ref SystemState state)
        {
            _villagerId = RegistryId.FromString("godgame.villager");
            _villageCenterId = RegistryId.FromString("godgame.village_center");
            _resourceChunkId = RegistryId.FromString("godgame.resource_chunk");
            _resourceNodeId = RegistryId.FromString("godgame.resource_node");
            _vegetationId = RegistryId.FromString("godgame.vegetation");
            _storehouseId = RegistryId.FromString("godgame.storehouse");
            _housingId = RegistryId.FromString("godgame.housing");
            _worshipId = RegistryId.FromString("godgame.worship");
            _constructionGhostId = RegistryId.FromString("godgame.construction_ghost");
            _bandId = RegistryId.FromString("godgame.band");
            _ghostTetherId = RegistryId.FromString("godgame.ghost_tether");

            state.RequireForUpdate<PresentationContentRegistryReference>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            AddIdentity(ref ecb, _villagerId, ref state, ComponentType.ReadOnly<VillagerPresentationTag>());
            AddIdentity(ref ecb, _villageCenterId, ref state, ComponentType.ReadOnly<VillageCenterPresentationTag>());
            AddIdentity(ref ecb, _resourceChunkId, ref state, ComponentType.ReadOnly<ResourceChunkPresentationTag>());
            AddIdentity(ref ecb, _resourceNodeId, ref state, ComponentType.ReadOnly<ResourceNodePresentationTag>());
            AddIdentity(ref ecb, _vegetationId, ref state, ComponentType.ReadOnly<VegetationPresentationTag>());
            AddIdentity(ref ecb, _storehouseId, ref state, ComponentType.ReadOnly<StorehousePresentationTag>());
            AddIdentity(ref ecb, _housingId, ref state, ComponentType.ReadOnly<HousingPresentationTag>());
            AddIdentity(ref ecb, _worshipId, ref state, ComponentType.ReadOnly<WorshipPresentationTag>());
            AddIdentity(ref ecb, _constructionGhostId, ref state, ComponentType.ReadOnly<ConstructionGhostPresentationTag>());
            AddIdentity(ref ecb, _bandId, ref state, ComponentType.ReadOnly<BandPresentationTag>());
            AddIdentity(ref ecb, _ghostTetherId, ref state, ComponentType.ReadOnly<GhostTetherTag>());

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static void AddIdentity(ref EntityCommandBuffer ecb, RegistryId id, ref SystemState state, ComponentType marker)
        {
            if (!id.IsValid)
            {
                return;
            }

            using var query = state.EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[] { marker },
                None = new[] { ComponentType.ReadOnly<RegistryIdentity>() }
            });

            using var entities = query.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                ecb.AddComponent(entities[i], new RegistryIdentity { Id = id });
            }
        }
    }
}
