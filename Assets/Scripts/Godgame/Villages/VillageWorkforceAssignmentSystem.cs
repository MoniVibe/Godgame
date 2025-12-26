using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Assigns villager work roles using a village workforce profile.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageNeedAwarenessSystem))]
    public partial struct VillageWorkforceAssignmentSystem : ISystem
    {
        private ComponentLookup<VillagerWorkRole> _roleLookup;
        private ComponentLookup<VillagerWorkRoleOverride> _overrideLookup;
        private ComponentLookup<VillagerLifecycleState> _lifecycleLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillageWorkforceProfile>();
            _roleLookup = state.GetComponentLookup<VillagerWorkRole>();
            _overrideLookup = state.GetComponentLookup<VillagerWorkRoleOverride>(true);
            _lifecycleLookup = state.GetComponentLookup<VillagerLifecycleState>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var tuning = SystemAPI.HasSingleton<VillagerLifecycleTuning>()
                ? SystemAPI.GetSingleton<VillagerLifecycleTuning>()
                : new VillagerLifecycleTuning { SecondsPerDay = 480f };

            var ticksPerDay = math.max(1u, (uint)math.ceil(tuning.SecondsPerDay / math.max(1e-4f, timeState.FixedDeltaTime)));

            _roleLookup.Update(ref state);
            _overrideLookup.Update(ref state);
            _lifecycleLookup.Update(ref state);

            foreach (var (profile, members, villageEntity) in SystemAPI
                         .Query<RefRW<VillageWorkforceProfile>, DynamicBuffer<VillageMember>>()
                         .WithEntityAccess())
            {
                var profileValue = profile.ValueRO;
                var cooldownTicks = (uint)math.max(0f, profileValue.ReassignmentCooldownDays) * ticksPerDay;
                if (profileValue.LastAssignmentTick != 0u && timeState.Tick - profileValue.LastAssignmentTick < cooldownTicks)
                {
                    continue;
                }

                var assignable = new NativeList<Entity>(members.Length, Allocator.Temp);
                for (int i = 0; i < members.Length; i++)
                {
                    var villager = members[i].VillagerEntity;
                    if (villager == Entity.Null || !_roleLookup.HasComponent(villager))
                    {
                        continue;
                    }

                    if (_overrideLookup.HasComponent(villager))
                    {
                        continue;
                    }

                    if (_lifecycleLookup.HasComponent(villager))
                    {
                        var lifecycle = _lifecycleLookup[villager];
                        if (lifecycle.Stage < VillagerLifeStage.Adult)
                        {
                            continue;
                        }
                    }

                    assignable.Add(villager);
                }

                var totalAssignable = assignable.Length;
                if (totalAssignable <= 0)
                {
                    assignable.Dispose();
                    continue;
                }

                var foresterRatio = math.max(0f, profileValue.ForesterRatio);
                var minerRatio = math.max(0f, profileValue.MinerRatio);
                var farmerRatio = math.max(0f, profileValue.FarmerRatio);
                var builderRatio = math.max(0f, profileValue.BuilderRatio);
                var breederRatio = math.max(0f, profileValue.BreederRatio);
                var haulerRatio = math.max(0f, profileValue.HaulerRatio);
                var sum = foresterRatio + minerRatio + farmerRatio + builderRatio + breederRatio + haulerRatio;
                if (sum <= 0f)
                {
                    assignable.Dispose();
                    continue;
                }

                var foresterDesired = totalAssignable * (foresterRatio / sum);
                var minerDesired = totalAssignable * (minerRatio / sum);
                var farmerDesired = totalAssignable * (farmerRatio / sum);
                var builderDesired = totalAssignable * (builderRatio / sum);
                var breederDesired = totalAssignable * (breederRatio / sum);
                var haulerDesired = totalAssignable * (haulerRatio / sum);

                var foresterCount = (int)math.floor(foresterDesired);
                var minerCount = (int)math.floor(minerDesired);
                var farmerCount = (int)math.floor(farmerDesired);
                var builderCount = (int)math.floor(builderDesired);
                var breederCount = (int)math.floor(breederDesired);
                var haulerCount = (int)math.floor(haulerDesired);

                var remaining = totalAssignable - (foresterCount + minerCount + farmerCount + builderCount + breederCount + haulerCount);
                var foresterRemainder = foresterDesired - foresterCount;
                var minerRemainder = minerDesired - minerCount;
                var farmerRemainder = farmerDesired - farmerCount;
                var builderRemainder = builderDesired - builderCount;
                var breederRemainder = breederDesired - breederCount;
                var haulerRemainder = haulerDesired - haulerCount;

                for (int i = 0; i < remaining; i++)
                {
                    var max = foresterRemainder;
                    var pick = VillagerWorkRoleKind.Forester;
                    if (minerRemainder > max)
                    {
                        max = minerRemainder;
                        pick = VillagerWorkRoleKind.Miner;
                    }
                    if (farmerRemainder > max)
                    {
                        max = farmerRemainder;
                        pick = VillagerWorkRoleKind.Farmer;
                    }
                    if (builderRemainder > max)
                    {
                        max = builderRemainder;
                        pick = VillagerWorkRoleKind.Builder;
                    }
                    if (breederRemainder > max)
                    {
                        max = breederRemainder;
                        pick = VillagerWorkRoleKind.Breeder;
                    }
                    if (haulerRemainder > max)
                    {
                        pick = VillagerWorkRoleKind.Hauler;
                    }

                    switch (pick)
                    {
                        case VillagerWorkRoleKind.Miner:
                            minerCount++;
                            minerRemainder = 0f;
                            break;
                        case VillagerWorkRoleKind.Farmer:
                            farmerCount++;
                            farmerRemainder = 0f;
                            break;
                        case VillagerWorkRoleKind.Builder:
                            builderCount++;
                            builderRemainder = 0f;
                            break;
                        case VillagerWorkRoleKind.Breeder:
                            breederCount++;
                            breederRemainder = 0f;
                            break;
                        case VillagerWorkRoleKind.Hauler:
                            haulerCount++;
                            haulerRemainder = 0f;
                            break;
                        default:
                            foresterCount++;
                            foresterRemainder = 0f;
                            break;
                    }
                }

                var seed = math.hash(new uint2((uint)(villageEntity.Index + 1), timeState.Tick));
                var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);

                for (int i = assignable.Length - 1; i > 0; i--)
                {
                    var j = random.NextInt(0, i + 1);
                    var tmp = assignable[i];
                    assignable[i] = assignable[j];
                    assignable[j] = tmp;
                }

                var index = 0;
                AssignRange(assignable, ref index, foresterCount, VillagerWorkRoleKind.Forester, ref _roleLookup);
                AssignRange(assignable, ref index, minerCount, VillagerWorkRoleKind.Miner, ref _roleLookup);
                AssignRange(assignable, ref index, farmerCount, VillagerWorkRoleKind.Farmer, ref _roleLookup);
                AssignRange(assignable, ref index, builderCount, VillagerWorkRoleKind.Builder, ref _roleLookup);
                AssignRange(assignable, ref index, breederCount, VillagerWorkRoleKind.Breeder, ref _roleLookup);
                AssignRange(assignable, ref index, haulerCount, VillagerWorkRoleKind.Hauler, ref _roleLookup);

                profile.ValueRW.LastAssignmentTick = timeState.Tick;
                assignable.Dispose();
            }
        }

        private static void AssignRange(NativeList<Entity> list, ref int index, int count, VillagerWorkRoleKind role, ref ComponentLookup<VillagerWorkRole> lookup)
        {
            for (int i = 0; i < count && index < list.Length; i++, index++)
            {
                var villager = list[index];
                if (!lookup.HasComponent(villager))
                {
                    continue;
                }

                lookup[villager] = new VillagerWorkRole { Value = role };
            }
        }
    }
}
