using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Godgame.Modules;
using Godgame.Villagers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Linq;

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
        private Godgame.Villagers.VillagerAIState.Goal aiGoal = Godgame.Villagers.VillagerAIState.Goal.Work;

        [SerializeField]
        private float3 spawnPosition = float3.zero;

        [SerializeField]
        private sbyte vengefulScore = 0; // -100 (Vengeful) to +100 (Forgiving)

        [SerializeField]
        private sbyte boldScore = 0; // -100 (Craven) to +100 (Bold)

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

                AddComponent(entity, new Godgame.Villagers.VillagerAIState
                {
                    CurrentState = Godgame.Villagers.VillagerAIState.State.Idle,
                    CurrentGoal = authoring.aiGoal,
                    TargetEntity = Entity.Null
                });

                // Add personality component
                AddComponent(entity, new VillagerPersonality
                {
                    VengefulScore = authoring.vengefulScore,
                    BoldScore = authoring.boldScore
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
                var needs = new Godgame.Villagers.VillagerNeeds
                {
                    Food = (byte)math.clamp((int)authoring.food, 0, 100),
                    Rest = (byte)math.clamp((int)authoring.rest, 0, 100),
                    Sleep = (byte)math.clamp((int)authoring.sleep, 0, 100),
                    GeneralHealth = (byte)math.clamp((int)authoring.generalHealth, 0, 100),
                    Health = math.clamp(authoring.generalHealth, 0f, 100f),
                    MaxHealth = 100f,
                    Energy = math.clamp(authoring.rest, 0f, 100f)
                };
                AddComponent(entity, needs);

                // Add mood (initialize from general health or default)
                AddComponent(entity, new Godgame.Villagers.VillagerMood
                {
                    Mood = math.clamp(authoring.generalHealth, 0f, 100f)
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



