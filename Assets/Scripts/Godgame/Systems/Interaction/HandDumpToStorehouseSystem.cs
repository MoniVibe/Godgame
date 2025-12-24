using Godgame.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems.Hand;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using DivineHandState = Godgame.Runtime.DivineHandState;
using DivineHandConfig = Godgame.Runtime.DivineHandConfig;

namespace Godgame.Systems.Interaction
{
    /// <summary>
    /// Handles dump commands targeting storehouses by depositing the hand's payload.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(HandCommandEmitterSystem))]
    public partial struct HandDumpToStorehouseSystem : ISystem
    {
        private ComponentLookup<StorehouseInventory> _inventoryLookup;
        private BufferLookup<StorehouseInventoryItem> _itemsLookup;
        private BufferLookup<StorehouseCapacityElement> _capacityLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DivineHandState>();
            state.RequireForUpdate<DivineHandConfig>();
            state.RequireForUpdate<HandCommand>();
            state.RequireForUpdate<HandPayload>();
            state.RequireForUpdate<ResourceTypeIndex>();

            _inventoryLookup = state.GetComponentLookup<StorehouseInventory>(false);
            _itemsLookup = state.GetBufferLookup<StorehouseInventoryItem>(false);
            _capacityLookup = state.GetBufferLookup<StorehouseCapacityElement>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            uint currentTick = timeState.Tick;
            float deltaTime = timeState.FixedDeltaTime;
            var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;

            _inventoryLookup.Update(ref state);
            _itemsLookup.Update(ref state);
            _capacityLookup.Update(ref state);

            foreach (var (handStateRef, configRef, payloadBuffer, commandBuffer) in SystemAPI
                         .Query<RefRW<DivineHandState>, RefRO<DivineHandConfig>, DynamicBuffer<HandPayload>, DynamicBuffer<HandCommand>>())
            {
                var handState = handStateRef.ValueRW;
                var config = configRef.ValueRO;
                var payload = payloadBuffer;
                var commands = commandBuffer;

                for (int i = commands.Length - 1; i >= 0; i--)
                {
                    var command = commands[i];

                    if (command.Tick != currentTick || command.Type != HandCommandType.Dump)
                    {
                        continue;
                    }

                    if (TryDump(ref handState, payload, in config, command, deltaTime, catalog, currentTick))
                    {
                        commands.RemoveAt(i);
                    }
                }

                handState.HeldAmount = (int)math.round(HandPayloadUtility.GetTotalAmount(payload));
                handState.HeldResourceTypeIndex = HandPayloadUtility.ResolveDominantType(payload, DivineHandConstants.NoResourceType);

                handStateRef.ValueRW = handState;
            }
        }

        private bool TryDump(ref DivineHandState handState,
            DynamicBuffer<HandPayload> payload,
            in DivineHandConfig config,
            in HandCommand command,
            float deltaTime,
            BlobAssetReference<ResourceTypeIndexBlob> catalog,
            uint currentTick)
        {
            if (payload.Length == 0)
            {
                return true;
            }

            var target = command.TargetEntity;
            if (target == Entity.Null || !_inventoryLookup.HasComponent(target))
            {
                return false;
            }

            float dumpRate = math.max(0f, config.DumpRate);
            int desiredUnits = math.max(1, (int)math.floor(dumpRate * deltaTime));
            desiredUnits = math.min(desiredUnits, handState.HeldAmount);
            if (desiredUnits <= 0)
            {
                return false;
            }

            ushort resourceType = command.ResourceTypeIndex != DivineHandConstants.NoResourceType
                ? command.ResourceTypeIndex
                : handState.HeldResourceTypeIndex;
            if (resourceType == DivineHandConstants.NoResourceType)
            {
                return true;
            }

            float removed = HandPayloadUtility.RemoveAmount(ref payload, resourceType, desiredUnits);
            int removableUnits = (int)math.floor(removed);
            if (removableUnits <= 0)
            {
                return false;
            }

            int deposited = DepositToStorehouse(target, resourceType, removableUnits, catalog, currentTick);
            if (deposited <= 0)
            {
                HandPayloadUtility.AddAmount(ref payload, resourceType, removableUnits);
                return false;
            }

            if (deposited < removableUnits)
            {
                HandPayloadUtility.AddAmount(ref payload, resourceType, removableUnits - deposited);
            }

            return true;
        }

        private int DepositToStorehouse(Entity storehouse,
            ushort resourceTypeIndex,
            int units,
            BlobAssetReference<ResourceTypeIndexBlob> catalog,
            uint currentTick)
        {
            if (!catalog.IsCreated || !_inventoryLookup.HasComponent(storehouse) ||
                !_itemsLookup.HasBuffer(storehouse) || !_capacityLookup.HasBuffer(storehouse))
            {
                return 0;
            }

            ref var resourceIds = ref catalog.Value.Ids;
            if (resourceTypeIndex >= resourceIds.Length)
            {
                return 0;
            }

            var resourceId = resourceIds[resourceTypeIndex];
            var capacities = _capacityLookup[storehouse];
            int capacityIndex = FindCapacityIndex(capacities, resourceId);
            if (capacityIndex < 0)
            {
                return 0;
            }

            var items = _itemsLookup[storehouse];
            int itemIndex = FindInventoryIndex(items, resourceId);
            float maxCapacity = capacities[capacityIndex].MaxCapacity;
            float currentAmount = itemIndex >= 0 ? items[itemIndex].Amount : 0f;
            float available = math.max(0f, maxCapacity - currentAmount);
            if (available <= 0f)
            {
                return 0;
            }

            int depositUnits = math.min(units, (int)math.floor(available));
            if (depositUnits <= 0)
            {
                return 0;
            }

            if (itemIndex >= 0)
            {
                var item = items[itemIndex];
                item.Amount += depositUnits;
                if (item.TierId == 0)
                {
                    item.TierId = (byte)ResourceQualityTier.Unknown;
                }
                if (item.AverageQuality == 0)
                {
                    item.AverageQuality = 200;
                }
                items[itemIndex] = item;
            }
            else
            {
                items.Add(new StorehouseInventoryItem
                {
                    ResourceTypeId = resourceId,
                    Amount = depositUnits,
                    Reserved = 0f,
                    TierId = (byte)ResourceQualityTier.Unknown,
                    AverageQuality = 200
                });
            }

            var inventory = _inventoryLookup[storehouse];
            inventory.TotalStored += depositUnits;
            inventory.LastUpdateTick = currentTick;
            _inventoryLookup[storehouse] = inventory;

            return depositUnits;
        }

        private static int FindCapacityIndex(DynamicBuffer<StorehouseCapacityElement> capacities, FixedString64Bytes resourceId)
        {
            for (int i = 0; i < capacities.Length; i++)
            {
                if (capacities[i].ResourceTypeId.Equals(resourceId))
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindInventoryIndex(DynamicBuffer<StorehouseInventoryItem> items, FixedString64Bytes resourceId)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].ResourceTypeId.Equals(resourceId))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

