using Godgame.Villages;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Maps villager work roles to gather job resource targets.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageWorkforceAssignmentSystem))]
    [UpdateAfter(typeof(VillagerWorkRoleFallbackSystem))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct VillagerWorkRoleResourceSyncSystem : ISystem
    {
        private ComponentLookup<VillagerResourceNeed> _resourceNeedLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<VillagerWorkRole>();
            _resourceNeedLookup = state.GetComponentLookup<VillagerResourceNeed>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<ResourceTypeIndex>(out var resourceIndex) || !resourceIndex.Catalog.IsCreated)
            {
                return;
            }

            var currentTick = SystemAPI.HasSingleton<TimeState>() ? SystemAPI.GetSingleton<TimeState>().Tick : 0u;
            var catalog = resourceIndex.Catalog;

            VillagerWorkTuning tuningSnapshot = default;
            var hasTuning = SystemAPI.TryGetSingletonRW<VillagerWorkTuning>(out var tuningRW);
            if (hasTuning)
            {
                ref var tuning = ref tuningRW.ValueRW;
                if (tuning.LastResolvedTick == 0u || tuning.LastResolvedTick != currentTick)
                {
                    tuning.ForesterInputIndex = ResolveIndex(catalog, tuning.ForesterInputId);
                    tuning.ForesterOutputIndex = ResolveIndex(catalog, tuning.ForesterOutputId);
                    tuning.MinerOutputIndex = ResolveIndex(catalog, tuning.MinerOutputId);
                    tuning.FarmerOutputIndex = ResolveIndex(catalog, tuning.FarmerOutputId);

                    if (tuning.ForesterOutputIndex == ushort.MaxValue)
                    {
                        tuning.ForesterOutputIndex = tuning.ForesterInputIndex;
                    }

                    tuning.LastResolvedTick = currentTick;
                }

                tuningSnapshot = tuning;
            }
            else
            {
                tuningSnapshot = new VillagerWorkTuning
                {
                    ForesterInputIndex = ResolveIndex(catalog, BuildWoodId()),
                    ForesterOutputIndex = ResolveIndex(catalog, BuildLumberId()),
                    MinerOutputIndex = ResolveIndex(catalog, BuildOreId()),
                    FarmerOutputIndex = ResolveIndex(catalog, BuildGrainId())
                };

                if (tuningSnapshot.ForesterOutputIndex == ushort.MaxValue)
                {
                    tuningSnapshot.ForesterOutputIndex = tuningSnapshot.ForesterInputIndex;
                }
            }

            _resourceNeedLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (job, role, entity) in SystemAPI.Query<RefRW<VillagerJobState>, RefRO<VillagerWorkRole>>()
                         .WithEntityAccess())
            {
                var roleKind = role.ValueRO.Value;
                var desiredJobType = JobType.None;
                ushort inputIndex = ushort.MaxValue;
                ushort outputIndex = ushort.MaxValue;

                switch (roleKind)
                {
                    case VillagerWorkRoleKind.Forester:
                        desiredJobType = JobType.Gather;
                        inputIndex = tuningSnapshot.ForesterInputIndex;
                        outputIndex = tuningSnapshot.ForesterOutputIndex;
                        break;
                    case VillagerWorkRoleKind.Miner:
                        desiredJobType = JobType.Gather;
                        inputIndex = tuningSnapshot.MinerOutputIndex;
                        outputIndex = tuningSnapshot.MinerOutputIndex;
                        break;
                    case VillagerWorkRoleKind.Farmer:
                        desiredJobType = JobType.Gather;
                        inputIndex = tuningSnapshot.FarmerOutputIndex;
                        outputIndex = tuningSnapshot.FarmerOutputIndex;
                        break;
                    case VillagerWorkRoleKind.Hauler:
                        desiredJobType = JobType.Gather;
                        inputIndex = ushort.MaxValue;
                        outputIndex = ushort.MaxValue;
                        break;
                    default:
                        desiredJobType = JobType.None;
                        inputIndex = ushort.MaxValue;
                        outputIndex = ushort.MaxValue;
                        break;
                }

                if (desiredJobType == JobType.Gather && roleKind != VillagerWorkRoleKind.Hauler)
                {
                    if (inputIndex == ushort.MaxValue)
                    {
                        desiredJobType = JobType.None;
                        outputIndex = ushort.MaxValue;
                    }
                    else if (outputIndex == ushort.MaxValue)
                    {
                        outputIndex = inputIndex;
                    }
                }

                job.ValueRW.Type = desiredJobType;
                job.ValueRW.ResourceTypeIndex = inputIndex;
                job.ValueRW.OutputResourceTypeIndex = outputIndex;

                if (desiredJobType == JobType.None)
                {
                    job.ValueRW.Phase = JobPhase.Idle;
                    job.ValueRW.Target = Entity.Null;
                }

                if (_resourceNeedLookup.HasComponent(entity))
                {
                    var need = _resourceNeedLookup[entity];
                    need.ResourceTypeIndex = inputIndex;
                    _resourceNeedLookup[entity] = need;
                }
                else
                {
                    ecb.AddComponent(entity, new VillagerResourceNeed
                    {
                        ResourceTypeIndex = inputIndex
                    });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static ushort ResolveIndex(BlobAssetReference<ResourceTypeIndexBlob> catalog, Unity.Collections.FixedString64Bytes resourceId)
        {
            if (!catalog.IsCreated || resourceId.Length == 0)
            {
                return ushort.MaxValue;
            }

            var lookup = catalog.Value.LookupIndex(resourceId);
            return lookup >= 0 ? (ushort)lookup : ushort.MaxValue;
        }

        private static FixedString64Bytes BuildWoodId()
        {
            FixedString64Bytes id = default;
            id.Append('w');
            id.Append('o');
            id.Append('o');
            id.Append('d');
            return id;
        }

        private static FixedString64Bytes BuildLumberId()
        {
            FixedString64Bytes id = default;
            id.Append('l');
            id.Append('u');
            id.Append('m');
            id.Append('b');
            id.Append('e');
            id.Append('r');
            return id;
        }

        private static FixedString64Bytes BuildOreId()
        {
            FixedString64Bytes id = default;
            id.Append('o');
            id.Append('r');
            id.Append('e');
            return id;
        }

        private static FixedString64Bytes BuildGrainId()
        {
            FixedString64Bytes id = default;
            id.Append('g');
            id.Append('r');
            id.Append('a');
            id.Append('i');
            id.Append('n');
            return id;
        }
    }
}
