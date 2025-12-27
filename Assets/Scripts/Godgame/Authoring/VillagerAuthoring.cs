using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Communication;
using PureDOTS.Runtime.Interrupts;
using PureDOTS.Runtime.Modularity;
using PureDOTS.Runtime.Perception;
using PureDOTS.Runtime.Spatial;
using PureDOTS.Systems;
using Godgame.AI;
using Godgame.Modules;
using Godgame.Villagers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for individual villager entities.
    /// Bakes PureDOTS components required for villager gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int villagerId = 1;

        [SerializeField]
        private int factionId = 0;

        [SerializeField]
        private Godgame.Villagers.VillagerJob.JobType jobType = Godgame.Villagers.VillagerJob.JobType.Gatherer;

        [SerializeField]
        private VillagerAIState.Goal aiGoal = VillagerAIState.Goal.Work;

        [SerializeField]
        private float3 spawnPosition = float3.zero;

        [SerializeField, Range(-100, 100)]
        private sbyte vengefulScore = 0; // -100 (Vengeful) to +100 (Forgiving)

        [SerializeField, Range(-100, 100)]
        private sbyte boldScore = 0; // -100 (Craven) to +100 (Bold)

        [SerializeField, Range(-100, 100)]
        private sbyte patienceScore = 0; // -100 (Impatient) to +100 (Patient)

        [SerializeField]
        private bool isUndead = false;

        [SerializeField]
        private bool isSummoned = false;

        // Core Attributes (Experience Modifiers) - 0-100
        [SerializeField]
        private float physique = 50f;

        [SerializeField]
        private float finesse = 50f;

        [SerializeField]
        private float will = 50f;

        [SerializeField]
        private float wisdom = 50f;

        // Derived Attributes - 0-100
        [SerializeField]
        private float strength = 50f;

        [SerializeField]
        private float agility = 50f;

        [SerializeField]
        private float intelligence = 50f;

        // Social Stats
        [SerializeField]
        private float fame = 0f; // 0-1000

        [SerializeField]
        private float wealth = 0f;

        [SerializeField]
        private float reputation = 0f; // -100 to +100

        [SerializeField]
        private float glory = 0f; // 0-1000

        [SerializeField]
        private float renown = 0f; // 0-1000

        // Combat Stats (Base values, 0 = auto-calculate)
        [SerializeField]
        private float baseAttack = 0f; // 0-100

        [SerializeField]
        private float baseDefense = 0f; // 0-100

        [SerializeField]
        private float baseHealthOverride = 0f; // 0 = use baseHealth or calculate

        [SerializeField]
        private float baseHealth = 100f; // HP

        [SerializeField]
        private float baseStamina = 10f; // Rounds (0 = auto-calculate)

        [SerializeField]
        private float baseMana = 0f; // 0-100 (0 = auto-calculate or non-magic)

        // Need Stats - 0-100
        [SerializeField]
        private float food = 100f;

        [SerializeField]
        private float rest = 100f;

        [SerializeField]
        private float sleep = 100f;

        [SerializeField]
        private float generalHealth = 100f;

        // Resistances - 0-100% (stored as 0.0-1.0)
        [SerializeField]
        private float physicalResistance = 0f;

        [SerializeField]
        private float fireResistance = 0f;

        [SerializeField]
        private float coldResistance = 0f;

        [SerializeField]
        private float poisonResistance = 0f;

        [SerializeField]
        private float magicResistance = 0f;

        [SerializeField]
        private float lightningResistance = 0f;

        [SerializeField]
        private float holyResistance = 0f;

        [SerializeField]
        private float darkResistance = 0f;

        // Healing & Spell Modifiers - multipliers (default 1.0)
        [SerializeField]
        private float healBonus = 1.0f;

        [SerializeField]
        private float spellDurationModifier = 1.0f;

        [SerializeField]
        private float spellIntensityModifier = 1.0f;

        [Header("PureDOTS Mind Defaults")]
        [SerializeField, Range(0.001f, 0.05f)]
        private float hungerDecayPerTick = 0.01f;

        [SerializeField, Range(0.001f, 0.05f)]
        private float restDecayPerTick = 0.01f;

        [SerializeField, Range(0.001f, 0.05f)]
        private float faithDecayPerTick = 0.005f;

        [SerializeField, Range(0.001f, 0.05f)]
        private float safetyDecayPerTick = 0.008f;

        [SerializeField, Range(0.001f, 0.05f)]
        private float socialDecayPerTick = 0.006f;

        [SerializeField, Range(0.001f, 0.05f)]
        private float workPressurePerTick = 0.004f;

        [SerializeField]
        private float focusMax = 1f;

        [SerializeField]
        private float focusRegenPerTick = 0.03f;

        [SerializeField]
        private bool lockFocusAtSpawn = false;

        [SerializeField, Range(1, 20)]
        private int mindCadenceTicks = 5;

        [Header("Divine Hand Interaction")]
        [SerializeField]
        private bool allowDivineHandPickup = true;

        [SerializeField]
        private float handPickableMass = 75f;

        [SerializeField]
        private float handMaxHoldDistance = 12f;

        [SerializeField]
        private float handThrowImpulseMultiplier = 1f;

        [SerializeField, Range(0.01f, 1f)]
        private float handFollowLerp = 0.25f;

        private sealed class Baker : Unity.Entities.Baker<VillagerAuthoring>
        {
            public override void Bake(VillagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);
                
                // Add villager components
                AddComponent(entity, new Godgame.Villagers.VillagerId
                {
                    Value = authoring.villagerId,
                    FactionId = authoring.factionId
                });

                AddComponent(entity, new Godgame.Villagers.VillagerJob
                {
                    Type = authoring.jobType,
                    Phase = Godgame.Villagers.VillagerJob.JobPhase.Idle,
                    ActiveTicketId = 0,
                    Productivity = 1f
                });

                AddComponent(entity, new VillagerAIState
                {
                    CurrentState = VillagerAIState.State.Idle,
                    CurrentGoal = authoring.aiGoal,
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    StateTimer = 0f,
                    StateStartTick = 0
                });

                // Add EntityIntent component (defaults to Idle)
                AddComponent(entity, new EntityIntent
                {
                    Mode = IntentMode.Idle,
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    TriggeringInterrupt = InterruptType.None,
                    IntentSetTick = 0,
                    Priority = InterruptPriority.Low,
                    IsValid = 0
                });

                // Add Interrupt buffer for interrupt-driven intent system
                AddBuffer<Interrupt>(entity);

                var jobStateType = authoring.jobType == Godgame.Villagers.VillagerJob.JobType.Gatherer
                    ? Godgame.Villagers.JobType.Gather
                    : Godgame.Villagers.JobType.None;
                AddComponent(entity, new VillagerJobState
                {
                    Type = jobStateType,
                    Phase = Godgame.Villagers.JobPhase.Idle,
                    Target = Entity.Null,
                    ResourceTypeIndex = 0,
                    OutputResourceTypeIndex = ushort.MaxValue,
                    CarryCount = 0f,
                    CarryMax = 0f,
                    DropoffCooldown = 0f
                });

                AddComponent(entity, new Godgame.Villagers.Navigation
                {
                    Destination = float3.zero,
                    Speed = 0f
                });

                AddComponent<CommunicationModuleTag>(entity);
                AddComponent(entity, MediumContext.DefaultGas);

                AddComponent(entity, new AIRole { RoleId = GodgameAIRoleDefinitions.RoleCivilian });
                AddComponent(entity, new AIDoctrine { DoctrineId = GodgameAIRoleDefinitions.DoctrineCivilian });
                AddComponent(entity, new AIBehaviorProfile
                {
                    ProfileId = GodgameAIRoleDefinitions.ProfileCivilian,
                    ProfileHash = GodgameAIRoleDefinitions.ProfileCivilianHash,
                    ProfileEntity = Entity.Null,
                    SourceId = GodgameAIRoleDefinitions.SourceScenario
                });

                // Add personality component
                AddComponent(entity, new VillagerPersonality
                {
                    VengefulScore = authoring.vengefulScore,
                    BoldScore = authoring.boldScore,
                    PatienceScore = authoring.patienceScore
                });

                // Add undead tag if flagged
                if (authoring.isUndead)
                {
                    AddComponent<UndeadTag>(entity);
                }

                // Add summoned tag if flagged
                if (authoring.isSummoned)
                {
                    AddComponent<SummonedTag>(entity);
                }

                // Add core attributes
                AddComponent(entity, new Godgame.Villagers.VillagerAttributes
                {
                    Physique = (byte)math.clamp((int)authoring.physique, 0, 100),
                    Finesse = (byte)math.clamp((int)authoring.finesse, 0, 100),
                    Will = (byte)math.clamp((int)authoring.will, 0, 100),
                    Wisdom = (byte)math.clamp((int)authoring.wisdom, 0, 100)
                });

                // Add derived attributes
                AddComponent(entity, new VillagerDerivedAttributes
                {
                    Strength = (byte)math.clamp((int)authoring.strength, 0, 100),
                    Agility = (byte)math.clamp((int)authoring.agility, 0, 100),
                    Intelligence = (byte)math.clamp((int)authoring.intelligence, 0, 100)
                });

                // Add social stats
                AddComponent(entity, new VillagerSocialStats
                {
                    Fame = (ushort)math.clamp((int)authoring.fame, 0, 1000),
                    Wealth = math.max(0f, authoring.wealth),
                    Reputation = (sbyte)math.clamp((int)authoring.reputation, -100, 100),
                    Glory = (ushort)math.clamp((int)authoring.glory, 0, 1000),
                    Renown = (ushort)math.clamp((int)authoring.renown, 0, 1000)
                });

                // Add resistances (convert float 0.0-1.0 to byte 0-100)
                AddComponent(entity, new VillagerResistances
                {
                    Physical = (byte)math.clamp((int)(authoring.physicalResistance * 100f), 0, 100),
                    Fire = (byte)math.clamp((int)(authoring.fireResistance * 100f), 0, 100),
                    Cold = (byte)math.clamp((int)(authoring.coldResistance * 100f), 0, 100),
                    Poison = (byte)math.clamp((int)(authoring.poisonResistance * 100f), 0, 100),
                    Magic = (byte)math.clamp((int)(authoring.magicResistance * 100f), 0, 100),
                    Lightning = (byte)math.clamp((int)(authoring.lightningResistance * 100f), 0, 100),
                    Holy = (byte)math.clamp((int)(authoring.holyResistance * 100f), 0, 100),
                    Dark = (byte)math.clamp((int)(authoring.darkResistance * 100f), 0, 100)
                });

                // Add modifiers (convert float multiplier to ushort 0-200 representing 0.0-2.0)
                AddComponent(entity, new VillagerModifiers
                {
                    HealBonus = (ushort)math.clamp((int)(authoring.healBonus * 100f), 0, 200),
                    SpellDurationModifier = (ushort)math.clamp((int)(authoring.spellDurationModifier * 100f), 0, 200),
                    SpellIntensityModifier = (ushort)math.clamp((int)(authoring.spellIntensityModifier * 100f), 0, 200)
                });

                // Add needs (convert float 0-100 to byte)
                var foodValue = math.clamp(authoring.food, 0f, 100f);
                var restValue = math.clamp(authoring.rest, 0f, 100f);
                var sleepValue = math.clamp(authoring.sleep, 0f, 100f);
                var healthValue = math.clamp(authoring.generalHealth, 0f, 100f);
                var moraleValue = math.clamp(authoring.generalHealth, 0f, 100f);

                var needs = new Godgame.Villagers.VillagerNeeds
                {
                    Food = (byte)math.clamp((int)foodValue, 0, 100),
                    Rest = (byte)math.clamp((int)restValue, 0, 100),
                    Sleep = (byte)math.clamp((int)sleepValue, 0, 100),
                    GeneralHealth = (byte)math.clamp((int)healthValue, 0, 100),
                    Health = healthValue,
                    MaxHealth = 100f,
                    Energy = restValue,
                    Morale = moraleValue
                };
                AddComponent(entity, needs);

                // Add mood (initialize from general health or default)
                AddComponent(entity, new Godgame.Villagers.VillagerMood
                {
                    Mood = moraleValue
                });

                AddComponent(entity, new PureDOTS.Runtime.Components.VillagerNeeds
                {
                    Food = (byte)math.clamp((int)foodValue, 0, 100),
                    Rest = (byte)math.clamp((int)restValue, 0, 100),
                    Sleep = (byte)math.clamp((int)sleepValue, 0, 100),
                    GeneralHealth = (byte)math.clamp((int)healthValue, 0, 100),
                    Health = healthValue,
                    MaxHealth = 100f,
                    Hunger = foodValue,
                    Energy = restValue,
                    Morale = moraleValue,
                    Temperature = 0f
                });

                AddComponent(entity, new PureDOTS.Runtime.Components.VillagerMood
                {
                    Mood = moraleValue,
                    TargetMood = moraleValue,
                    MoodChangeRate = 1f,
                    Wellbeing = moraleValue,
                    Alignment = 50f,
                    LastAlignmentInfluenceTick = 0
                });

                // Add combat stats (will be calculated by VillagerStatCalculationSystem if baseAttack/baseDefense are 0)
                AddComponent(entity, new Godgame.Villagers.VillagerCombatStats
                {
                    Attack = (byte)math.clamp((int)authoring.baseAttack, 0, 100),
                    Defense = (byte)math.clamp((int)authoring.baseDefense, 0, 100),
                    MaxHealth = authoring.baseHealthOverride > 0f ? authoring.baseHealthOverride : authoring.baseHealth,
                    CurrentHealth = authoring.baseHealthOverride > 0f ? authoring.baseHealthOverride : authoring.baseHealth,
                    Stamina = (byte)math.clamp((int)authoring.baseStamina, 0, 100),
                    CurrentStamina = (byte)math.clamp((int)authoring.baseStamina, 0, 100),
                    MaxMana = (byte)math.clamp((int)authoring.baseMana, 0, 100),
                    CurrentMana = (byte)math.clamp((int)authoring.baseMana, 0, 100),
                    AttackDamage = 0f,
                    AttackSpeed = 0f,
                    CurrentTarget = Entity.Null
                });

                // Seed empty equipment slots so future gear/repair systems have deterministic buffers.
                var moduleSlots = AddBuffer<ModuleSlot>(entity);
                ModuleSlotIds.AddVillagerSlots(moduleSlots);
                AddComponent(entity, new ModuleMaintainerAssignment { WorkerEntity = entity });

                // Set position if specified
                if (math.any(authoring.spawnPosition != float3.zero))
                {
                    var transform = GetComponent<Transform>();
                    if (transform != null)
                    {
                        transform.position = authoring.spawnPosition;
                    }
                }

                // Shared PureDOTS villager needs/intent components
                AddComponent(entity, new VillagerNeedState
                {
                    HungerUrgency = ToUrgency(authoring.food),
                    RestUrgency = ToUrgency(authoring.rest),
                    FaithUrgency = ToUrgency(authoring.wisdom),
                    SafetyUrgency = ToUrgency(authoring.generalHealth),
                    SocialUrgency = ToUrgency(authoring.glory),
                    WorkUrgency = ToUrgency(authoring.wealth)
                });

                AddComponent(entity, new VillagerNeedTuning
                {
                    HungerDecayPerTick = authoring.hungerDecayPerTick,
                    RestDecayPerTick = authoring.restDecayPerTick,
                    FaithDecayPerTick = authoring.faithDecayPerTick,
                    SafetyDecayPerTick = authoring.safetyDecayPerTick,
                    SocialDecayPerTick = authoring.socialDecayPerTick,
                    WorkPressurePerTick = authoring.workPressurePerTick,
                    MaxUrgency = 1f
                });

                AddComponent(entity, new FocusBudget
                {
                    Current = math.max(0.01f, authoring.focusMax),
                    Max = math.max(0.01f, authoring.focusMax),
                    RegenPerTick = math.max(0f, authoring.focusRegenPerTick),
                    Reserved = 0f,
                    IsLocked = (byte)(authoring.lockFocusAtSpawn ? 1 : 0)
                });
                AddBuffer<FocusBudgetReservation>(entity);

                AddComponent(entity, new VillagerGoalState
                {
                    CurrentGoal = VillagerGoal.Idle,
                    PreviousGoal = VillagerGoal.Idle,
                    LastGoalChangeTick = 0,
                    CurrentGoalUrgency = 0f
                });
                AddComponent(entity, new VillagerFleeIntent());
                AddComponent(entity, new VillagerMindCadence
                {
                    CadenceTicks = math.max(1, authoring.mindCadenceTicks),
                    LastRunTick = 0
                });
                AddComponent(entity, new VillagerThreatState
                {
                    ThreatEntity = Entity.Null,
                    ThreatDirection = new float3(0f, 0f, 1f),
                    Urgency = 0f,
                    HasLineOfSight = 0
                });

                AddComponent(entity, CommDecisionConfig.Default);
                AddComponent(entity, CommDecodeFactors.Default);

                if (authoring.allowDivineHandPickup)
                {
                    AddComponent<Pickable>(entity);
                    AddComponent(entity, new HandPickable
                    {
                        Mass = math.max(0.1f, authoring.handPickableMass),
                        MaxHoldDistance = math.max(0.1f, authoring.handMaxHoldDistance),
                        ThrowImpulseMultiplier = math.max(0.1f, authoring.handThrowImpulseMultiplier),
                        FollowLerp = math.clamp(authoring.handFollowLerp, 0.01f, 1f)
                    });
                }

                AddAISystemComponents(entity);
            }

            private void AddAISystemComponents(Entity entity)
            {
                var blobBuilder = new BlobBuilder(Allocator.Temp);
                ref var root = ref blobBuilder.ConstructRoot<AIUtilityArchetypeBlob>();
                var actions = blobBuilder.Allocate(ref root.Actions, 4);

                // Action 0: Satisfy hunger (virtual sensor 0).
                ref var action0 = ref actions[0];
                var factors0 = blobBuilder.Allocate(ref action0.Factors, 1);
                factors0[0] = new AIUtilityCurveBlob
                {
                    SensorIndex = 0,
                    Threshold = 0.3f,
                    Weight = 2f,
                    ResponsePower = 2f,
                    MaxValue = 1f
                };

                // Action 1: Rest (virtual sensor 1).
                ref var action1 = ref actions[1];
                var factors1 = blobBuilder.Allocate(ref action1.Factors, 1);
                factors1[0] = new AIUtilityCurveBlob
                {
                    SensorIndex = 1,
                    Threshold = 0.2f,
                    Weight = 1.5f,
                    ResponsePower = 1.5f,
                    MaxValue = 1f
                };

                // Action 2: Improve morale (virtual sensor 2).
                ref var action2 = ref actions[2];
                var factors2 = blobBuilder.Allocate(ref action2.Factors, 1);
                factors2[0] = new AIUtilityCurveBlob
                {
                    SensorIndex = 2,
                    Threshold = 0.4f,
                    Weight = 1f,
                    ResponsePower = 1f,
                    MaxValue = 1f
                };

                // Action 3: Work (first spatial reading at index 3).
                ref var action3 = ref actions[3];
                var factors3 = blobBuilder.Allocate(ref action3.Factors, 1);
                factors3[0] = new AIUtilityCurveBlob
                {
                    SensorIndex = 3,
                    Threshold = 0f,
                    Weight = 0.8f,
                    ResponsePower = 1f,
                    MaxValue = 1f
                };

                var utilityBlob = blobBuilder.CreateBlobAssetReference<AIUtilityArchetypeBlob>(Allocator.Persistent);
                blobBuilder.Dispose();
                AddBlobAsset(ref utilityBlob, out _);

                AddComponent(entity, new AISensorConfig
                {
                    UpdateInterval = 0.5f,
                    Range = 30f,
                    MaxResults = 8,
                    QueryOptions = SpatialQueryOptions.RequireDeterministicSorting,
                    PrimaryCategory = AISensorCategory.ResourceNode,
                    SecondaryCategory = AISensorCategory.Storehouse
                });

                AddComponent(entity, new AISensorState
                {
                    Elapsed = 0f,
                    LastSampleTick = 0
                });

                AddBuffer<AISensorReading>(entity);

                AddComponent(entity, new AIBehaviourArchetype
                {
                    UtilityBlob = utilityBlob
                });

                AddComponent(entity, new AIUtilityState
                {
                    BestActionIndex = 0,
                    BestScore = 0f,
                    LastEvaluationTick = 0
                });

                AddBuffer<AIActionState>(entity);

                AddComponent(entity, new AISteeringConfig
                {
                    MaxSpeed = 3f,
                    Acceleration = 8f,
                    Responsiveness = 0.5f,
                    DegreesOfFreedom = 2,
                    ObstacleLookAhead = 2f
                });

                AddComponent(entity, new AISteeringState
                {
                    DesiredDirection = float3.zero,
                    LinearVelocity = float3.zero,
                    LastSampledTarget = float3.zero,
                    LastUpdateTick = 0
                });

                AddComponent(entity, new AITargetState
                {
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    ActionIndex = 0,
                    Flags = 0
                });

                var binding = new VillagerAIUtilityBinding();
                binding.Goals.Add(VillagerAIState.Goal.SurviveHunger);
                binding.Goals.Add(VillagerAIState.Goal.Rest);
                binding.Goals.Add(VillagerAIState.Goal.Rest);
                binding.Goals.Add(VillagerAIState.Goal.Work);
                AddComponent(entity, binding);

                AddComponent(entity, new VillagerAIPipelineBridgeState
                {
                    LastBridgedTick = 0,
                    LastActionIndex = 0,
                    LastScore = 0f,
                    IsAIPipelineActive = 1
                });
            }

            private static float ToUrgency(float statValue)
            {
                if (math.abs(statValue) < 1e-3f)
                {
                    return 1f;
                }

                return math.saturate(1f - (statValue / 100f));
            }
        }
    }
}
