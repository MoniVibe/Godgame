using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Agency;
using PureDOTS.Runtime.Authority;
using PureDOTS.Runtime.Knowledge;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Ensures village governance/runtime components exist and seeds initial derived membership for smoke/presentation.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Scenario.SettlementSpawnSystem))]
    [UpdateAfter(typeof(Godgame.Scenario.GodgameScenarioBootstrapSystem))]
    [UpdateBefore(typeof(Godgame.Scenario.Godgame_SmokeValidationSystem))]
    public partial struct VillageGovernanceBootstrapSystem : ISystem
    {
        private EntityQuery _villagerQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Village>();

            _villagerQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VillagerNeedState>(),
                    ComponentType.ReadOnly<LocalTransform>()
                }
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var hasVillagers = !_villagerQuery.IsEmptyIgnoreFilter;
            NativeArray<Entity> villagerEntities = default;
            NativeArray<LocalTransform> villagerTransforms = default;

            if (hasVillagers)
            {
                villagerEntities = _villagerQuery.ToEntityArray(Allocator.Temp);
                villagerTransforms = _villagerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            }

            foreach (var (villageRef, entity) in SystemAPI.Query<RefRO<Village>>().WithEntityAccess())
            {
                EnsureVillageRuntime(entityManager, entity, in villageRef.ValueRO, hasVillagers, villagerEntities, villagerTransforms, ref ecb);
            }

            ecb.Playback(entityManager);
            ecb.Dispose();

            if (hasVillagers)
            {
                villagerEntities.Dispose();
                villagerTransforms.Dispose();
            }
        }

        private static void EnsureVillageRuntime(
            EntityManager entityManager,
            Entity villageEntity,
            in Village village,
            bool hasVillagers,
            NativeArray<Entity> villagerEntities,
            NativeArray<LocalTransform> villagerTransforms,
            ref EntityCommandBuffer ecb)
        {
            if (!entityManager.HasComponent<VillageNeedAwareness>(villageEntity))
            {
                ecb.AddComponent(villageEntity, new VillageNeedAwareness
                {
                    Hunger = 0f,
                    Rest = 0f,
                    Faith = 0f,
                    Safety = 0f,
                    Social = 0f,
                    Work = 0f,
                    MaxNeed = 0f,
                    DominantNeed = VillageNeedChannel.Hunger,
                    SampleCount = 0,
                    InfluenceEntity = Entity.Null,
                    DerivedMembers = 1,
                    LastSampleTick = 0u
                });
            }

            if (!entityManager.HasComponent<VillageConstructionRuntime>(villageEntity))
            {
                ecb.AddComponent(villageEntity, new VillageConstructionRuntime
                {
                    ActiveSite = Entity.Null,
                    ActiveWorker = Entity.Null,
                    ActiveBuildingType = VillageBuildingType.None,
                    LastIssuedTick = 0u
                });
            }

            if (!entityManager.HasComponent<VillageAlignmentState>(villageEntity))
            {
                ecb.AddComponent(villageEntity, new VillageAlignmentState
                {
                    MoralAxis = 0,
                    OrderAxis = 0,
                    PurityAxis = 0,
                    DominantOutlookId = 0
                });
            }

            if (!entityManager.HasBuffer<VillageMember>(villageEntity))
            {
                ecb.AddBuffer<VillageMember>(villageEntity);
            }

            if (!entityManager.HasBuffer<VillageExpansionRequest>(villageEntity))
            {
                ecb.AddBuffer<VillageExpansionRequest>(villageEntity);
            }

            if (!entityManager.HasComponent<GroupKnowledgeCacheTag>(villageEntity))
            {
                ecb.AddComponent<GroupKnowledgeCacheTag>(villageEntity);
            }

            if (!entityManager.HasBuffer<GroupKnowledgeEntry>(villageEntity))
            {
                ecb.AddBuffer<GroupKnowledgeEntry>(villageEntity);
            }

            EnsureVillageAuthority(entityManager, villageEntity, ref ecb);

            if (!entityManager.HasBuffer<VillageMember>(villageEntity))
            {
                return;
            }

            var members = entityManager.GetBuffer<VillageMember>(villageEntity);
            if (members.Length != 0)
            {
                return;
            }

            if (!hasVillagers)
            {
                return;
            }

            var center = entityManager.HasComponent<LocalTransform>(villageEntity)
                ? entityManager.GetComponentData<LocalTransform>(villageEntity).Position
                : village.CenterPosition;
            var radiusSq = math.max(0.1f, village.InfluenceRadius) * math.max(0.1f, village.InfluenceRadius);

            for (int i = 0; i < villagerEntities.Length; i++)
            {
                if (math.distancesq(villagerTransforms[i].Position.xz, center.xz) > radiusSq)
                {
                    continue;
                }

                members.Add(new VillageMember { VillagerEntity = villagerEntities[i] });
            }
        }

        private static void EnsureVillageAuthority(EntityManager entityManager, Entity villageEntity, ref EntityCommandBuffer ecb)
        {
            var seatCount = 0;
            if (entityManager.HasBuffer<AuthoritySeatRef>(villageEntity))
            {
                seatCount = entityManager.GetBuffer<AuthoritySeatRef>(villageEntity).Length;
            }
            else
            {
                ecb.AddBuffer<AuthoritySeatRef>(villageEntity);
            }

            var hasBody = entityManager.HasComponent<AuthorityBody>(villageEntity);

            if (seatCount > 0)
            {
                if (!hasBody)
                {
                    var seats = entityManager.GetBuffer<AuthoritySeatRef>(villageEntity);
                    var executiveSeat = seats[0].SeatEntity;
                    for (int i = 0; i < seats.Length; i++)
                    {
                        var candidate = seats[i].SeatEntity;
                        if (candidate != Entity.Null &&
                            entityManager.HasComponent<AuthoritySeat>(candidate) &&
                            entityManager.GetComponentData<AuthoritySeat>(candidate).IsExecutive != 0)
                        {
                            executiveSeat = candidate;
                            break;
                        }
                    }

                    ecb.AddComponent(villageEntity, new AuthorityBody
                    {
                        Mode = AuthorityBodyMode.SingleExecutive,
                        ExecutiveSeat = executiveSeat,
                        CreatedTick = 0u
                    });
                }

                return;
            }

            var mayorSeat = ecb.CreateEntity();
            ecb.AddComponent(mayorSeat, AuthoritySeatDefaults.CreateExecutive(
                villageEntity,
                new FixedString64Bytes("village.mayor"),
                AgencyDomain.Governance | AgencyDomain.Construction | AgencyDomain.Security | AgencyDomain.Logistics));
            ecb.AddComponent(mayorSeat, AuthoritySeatDefaults.Vacant(0u));
            var mayorDelegations = ecb.AddBuffer<AuthorityDelegation>(mayorSeat);

            var stewardSeat = ecb.CreateEntity();
            ecb.AddComponent(stewardSeat, AuthoritySeatDefaults.CreateDelegate(
                villageEntity,
                new FixedString64Bytes("village.steward"),
                AgencyDomain.Construction,
                AuthoritySeatRights.Recommend | AuthoritySeatRights.Issue));
            ecb.AddComponent(stewardSeat, AuthoritySeatDefaults.Vacant(0u));

            var marshalSeat = ecb.CreateEntity();
            ecb.AddComponent(marshalSeat, AuthoritySeatDefaults.CreateDelegate(
                villageEntity,
                new FixedString64Bytes("village.marshal"),
                AgencyDomain.Security,
                AuthoritySeatRights.Recommend | AuthoritySeatRights.Issue));
            ecb.AddComponent(marshalSeat, AuthoritySeatDefaults.Vacant(0u));

            var quartermasterSeat = ecb.CreateEntity();
            ecb.AddComponent(quartermasterSeat, AuthoritySeatDefaults.CreateDelegate(
                villageEntity,
                new FixedString64Bytes("village.quartermaster"),
                AgencyDomain.Logistics,
                AuthoritySeatRights.Recommend | AuthoritySeatRights.Issue));
            ecb.AddComponent(quartermasterSeat, AuthoritySeatDefaults.Vacant(0u));

            mayorDelegations.Add(new AuthorityDelegation
            {
                DelegateSeat = stewardSeat,
                Domains = AgencyDomain.Construction,
                GrantedRights = AuthoritySeatRights.Recommend | AuthoritySeatRights.Issue,
                Condition = AuthorityDelegationCondition.Always,
                Attribution = AuthorityAttributionMode.AsDelegateSeat
            });
            mayorDelegations.Add(new AuthorityDelegation
            {
                DelegateSeat = marshalSeat,
                Domains = AgencyDomain.Security,
                GrantedRights = AuthoritySeatRights.Recommend | AuthoritySeatRights.Issue,
                Condition = AuthorityDelegationCondition.Always,
                Attribution = AuthorityAttributionMode.AsDelegateSeat
            });
            mayorDelegations.Add(new AuthorityDelegation
            {
                DelegateSeat = quartermasterSeat,
                Domains = AgencyDomain.Logistics,
                GrantedRights = AuthoritySeatRights.Recommend | AuthoritySeatRights.Issue,
                Condition = AuthorityDelegationCondition.Always,
                Attribution = AuthorityAttributionMode.AsDelegateSeat
            });

            ecb.AppendToBuffer(villageEntity, new AuthoritySeatRef { SeatEntity = mayorSeat });
            ecb.AppendToBuffer(villageEntity, new AuthoritySeatRef { SeatEntity = stewardSeat });
            ecb.AppendToBuffer(villageEntity, new AuthoritySeatRef { SeatEntity = marshalSeat });
            ecb.AppendToBuffer(villageEntity, new AuthoritySeatRef { SeatEntity = quartermasterSeat });

            var body = new AuthorityBody
            {
                Mode = AuthorityBodyMode.SingleExecutive,
                ExecutiveSeat = mayorSeat,
                CreatedTick = 0u
            };

            if (hasBody)
            {
                ecb.SetComponent(villageEntity, body);
            }
            else
            {
                ecb.AddComponent(villageEntity, body);
            }
        }
    }
}
