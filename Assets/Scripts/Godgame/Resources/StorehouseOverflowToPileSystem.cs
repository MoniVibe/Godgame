using Godgame.Resources;
using Godgame.Systems;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Systems.Resources
{
    /// <summary>
    /// When dumping to a storehouse with no capacity for the held resource, redirect the payload into an aggregate pile.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct StorehouseOverflowToPileSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AggregatePileConfig>();
            state.RequireForUpdate<AggregatePileRuntimeState>();
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<DivineHandState>();
            state.RequireForUpdate<DivineHandCommand>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var configEntity = SystemAPI.GetSingletonEntity<AggregatePileConfig>();
            var pileCommands = EnsureSpawnBuffer(ref state, configEntity);
            var config = SystemAPI.GetSingleton<AggregatePileConfig>();
            var catalogRef = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalogRef.IsCreated || catalogRef.Value.Ids.Length == 0)
            {
                return;
            }

            foreach (var (handStateRef, commandRef) in SystemAPI
                         .Query<RefRW<DivineHandState>, RefRW<DivineHandCommand>>()
                         .WithAll<DivineHandTag>())
            {
                ref var handState = ref handStateRef.ValueRW;
                ref var command = ref commandRef.ValueRW;

                if (command.Type != DivineHandCommandType.Dump ||
                    handState.HeldAmount <= config.ConservationEpsilon ||
                    handState.HeldResourceTypeIndex == DivineHandConstants.NoResourceType ||
                    command.TargetEntity == Entity.Null ||
                    !SystemAPI.HasComponent<StorehouseInventory>(command.TargetEntity) ||
                    !SystemAPI.HasComponent<LocalTransform>(command.TargetEntity))
                {
                    continue;
                }

                var capacities = SystemAPI.GetBuffer<StorehouseCapacityElement>(command.TargetEntity);
                var items = SystemAPI.GetBuffer<StorehouseInventoryItem>(command.TargetEntity);
                var capacity = FindCapacity(capacities, ResolveResourceId(catalogRef, handState.HeldResourceTypeIndex));
                if (capacity <= 0f)
                {
                    var storeTransform = SystemAPI.GetComponent<LocalTransform>(command.TargetEntity);
                    RedirectToPile(ref pileCommands, ref handState, ref command, config, storeTransform);
                    continue;
                }

                var current = FindStored(items, ResolveResourceId(catalogRef, handState.HeldResourceTypeIndex));
                var available = capacity - current;
                if (available <= config.ConservationEpsilon)
                {
                    var storeTransform = SystemAPI.GetComponent<LocalTransform>(command.TargetEntity);
                    RedirectToPile(ref pileCommands, ref handState, ref command, config, storeTransform);
                }
            }
        }

        private static void RedirectToPile(
            ref DynamicBuffer<AggregatePileSpawnCommand> commands,
            ref DivineHandState handState,
            ref DivineHandCommand command,
            in AggregatePileConfig config,
            in LocalTransform storeTransform)
        {
            commands.Add(new AggregatePileSpawnCommand
            {
                ResourceType = handState.HeldResourceTypeIndex,
                Amount = handState.HeldAmount,
                Position = storeTransform.Position
            });

            handState.HeldAmount = 0;
            handState.HeldResourceTypeIndex = DivineHandConstants.NoResourceType;
            command.Type = DivineHandCommandType.None;
            command.TargetEntity = Entity.Null;
            command.TimeSinceIssued = 0f;
        }

        private static float FindCapacity(DynamicBuffer<StorehouseCapacityElement> capacities, FixedString64Bytes resourceId)
        {
            for (int i = 0; i < capacities.Length; i++)
            {
                if (capacities[i].ResourceTypeId.Equals(resourceId))
                {
                    return capacities[i].MaxCapacity;
                }
            }

            return 0f;
        }

        private static float FindStored(DynamicBuffer<StorehouseInventoryItem> items, FixedString64Bytes resourceId)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].ResourceTypeId.Equals(resourceId))
                {
                    return items[i].Amount + items[i].Reserved;
                }
            }

            return 0f;
        }

        private static FixedString64Bytes ResolveResourceId(BlobAssetReference<ResourceTypeIndexBlob> catalog, ushort resourceTypeIndex)
        {
            if (!catalog.IsCreated || resourceTypeIndex >= catalog.Value.Ids.Length)
            {
                return default;
            }

            return catalog.Value.Ids[resourceTypeIndex];
        }

        private DynamicBuffer<AggregatePileSpawnCommand> EnsureSpawnBuffer(ref SystemState state, Entity configEntity)
        {
            if (!state.EntityManager.HasBuffer<AggregatePileSpawnCommand>(configEntity))
            {
                state.EntityManager.AddBuffer<AggregatePileSpawnCommand>(configEntity);
            }

            return state.EntityManager.GetBuffer<AggregatePileSpawnCommand>(configEntity);
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
