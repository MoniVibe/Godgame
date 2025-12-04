using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Updates profession focus modifiers based on active focus abilities.
    /// Job systems should read ProfessionFocusModifiers to adjust their outcomes.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FocusAbilitySystem))]
    public partial struct ProfessionFocusModifierSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create default config if not present
            if (!SystemAPI.HasSingleton<ProfessionFocusConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, ProfessionFocusConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<TimeState>().Tick;

            new UpdateModifiersJob
            {
                CurrentTick = currentTick
            }.ScheduleParallel();
        }

        /// <summary>
        /// Calculates profession modifiers from active focus abilities.
        /// </summary>
        [BurstCompile]
        public partial struct UpdateModifiersJob : IJobEntity
        {
            public uint CurrentTick;

            public void Execute(
                ref ProfessionFocusModifiers modifiers,
                in DynamicBuffer<ActiveFocusAbility> activeAbilities,
                in ProfessionSkills skills)
            {
                // Reset to neutral values
                modifiers.Reset();
                modifiers.LastUpdateTick = CurrentTick;

                // Process each active ability and accumulate modifiers
                for (int i = 0; i < activeAbilities.Length; i++)
                {
                    var ability = activeAbilities[i];

                    // Skip non-profession abilities
                    if (!FocusAbilityDefinitions.IsProfessionAbility(ability.AbilityType))
                        continue;

                    // Get tradeoffs for this ability
                    var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(ability.AbilityType);

                    // Get skill-based effectiveness
                    byte skillLevel = ProfessionFocusHelpers.GetSkillLevelForAbility(ability.AbilityType, skills);
                    float effectiveness = FocusAbilityDefinitions.GetProfessionEffectivenessMultiplier(ability.AbilityType, skillLevel);

                    // Apply ability-specific modifiers
                    ApplyAbilityModifiers(ref modifiers, ability.AbilityType, ability.EffectMagnitude * effectiveness, speed, quality, waste);
                }
            }

            private void ApplyAbilityModifiers(
                ref ProfessionFocusModifiers mods,
                FocusAbilityType ability,
                float magnitude,
                float speedMod,
                float qualityMod,
                float wasteMod)
            {
                // Track active archetype
                var archetype = FocusAbilityDefinitions.GetArchetype(ability);
                if (archetype >= FocusArchetype.Crafting)
                {
                    mods.ActiveArchetype = archetype;
                }

                // Apply base tradeoffs (multiplicative)
                mods.SpeedMultiplier *= speedMod;
                mods.QualityMultiplier *= qualityMod;
                mods.WasteMultiplier *= wasteMod;

                // Apply ability-specific effects
                switch (ability)
                {
                    // === CRAFTING ===
                    case FocusAbilityType.BatchCrafting:
                        mods.TargetCountMultiplier = math.max(mods.TargetCountMultiplier, magnitude); // 3 items
                        break;

                    case FocusAbilityType.PrecisionWork:
                        mods.QualityMultiplier *= (1f + magnitude); // +25% quality
                        break;

                    case FocusAbilityType.Reinforce:
                        mods.QualityMultiplier *= (1f + magnitude * 0.5f); // Durability as quality proxy
                        break;

                    case FocusAbilityType.EfficientCrafting:
                        mods.WasteMultiplier *= (1f - magnitude); // -40% waste
                        break;

                    case FocusAbilityType.Inspiration:
                        mods.BonusChance = math.max(mods.BonusChance, magnitude); // 20% bonus chance
                        break;

                    case FocusAbilityType.StudiedCrafting:
                        mods.XPMultiplier *= (1f + magnitude); // +50% XP
                        break;

                    // === GATHERING ===
                    case FocusAbilityType.GatherOverdrive:
                        mods.SpeedMultiplier *= magnitude; // 3x speed
                        break;

                    case FocusAbilityType.ResourceSense:
                        // Reveal radius - doesn't affect modifiers directly
                        break;

                    case FocusAbilityType.DeepExtraction:
                        mods.QualityMultiplier *= (1f + magnitude); // +100% yield = quality proxy
                        break;

                    case FocusAbilityType.LuckyFind:
                        mods.BonusChance = math.max(mods.BonusChance, magnitude); // 15% rare find
                        break;

                    case FocusAbilityType.SustainableHarvest:
                        mods.WasteMultiplier *= (1f - magnitude); // 50% chance no depletion
                        break;

                    case FocusAbilityType.Multitasking:
                        mods.TargetCountMultiplier = math.max(mods.TargetCountMultiplier, magnitude); // 2 nodes
                        break;

                    // === HEALING ===
                    case FocusAbilityType.MassHeal:
                        mods.TargetCountMultiplier = math.max(mods.TargetCountMultiplier, 3f); // Multiple targets
                        mods.PerTargetEffectiveness = math.min(mods.PerTargetEffectiveness, magnitude); // 50% each
                        break;

                    case FocusAbilityType.LifeClutch:
                        mods.PerTargetEffectiveness = 1f; // Full effectiveness on single target
                        mods.TargetCountMultiplier = 1f;
                        break;

                    case FocusAbilityType.RegenerationAura:
                        mods.TargetCountMultiplier = math.max(mods.TargetCountMultiplier, 5f); // Nearby allies
                        mods.PerTargetEffectiveness = magnitude / 100f; // 5 HP/sec as fraction
                        break;

                    case FocusAbilityType.IntensiveCare:
                        mods.QualityMultiplier *= (1f + magnitude); // +100% healing
                        mods.TargetCountMultiplier = 1f; // Single target only
                        break;

                    case FocusAbilityType.LifeTransfer:
                        mods.QualityMultiplier *= magnitude; // 2:1 transfer ratio
                        break;

                    case FocusAbilityType.Triage:
                        mods.DurationMultiplier *= magnitude / 10f; // 30 second window
                        break;

                    // === TEACHING ===
                    case FocusAbilityType.GroupInstruction:
                        mods.TargetCountMultiplier = math.max(mods.TargetCountMultiplier, 3f); // Multiple students
                        mods.PerTargetEffectiveness = math.min(mods.PerTargetEffectiveness, magnitude); // 60% each
                        break;

                    case FocusAbilityType.InspiringPresence:
                        mods.XPMultiplier *= (1f + magnitude); // +20% learning speed
                        break;

                    case FocusAbilityType.HandsOnTraining:
                        mods.XPMultiplier *= (1f + magnitude); // +30% practical XP
                        break;

                    case FocusAbilityType.MindLink:
                        mods.SpeedMultiplier *= magnitude; // 5x transfer rate
                        break;

                    case FocusAbilityType.Eureka:
                        mods.SpeedMultiplier *= magnitude; // 2x research speed
                        break;

                    // === REFINING ===
                    case FocusAbilityType.BatchRefine:
                        mods.TargetCountMultiplier = math.max(mods.TargetCountMultiplier, magnitude); // 3x materials
                        break;

                    case FocusAbilityType.QualityControl:
                        mods.QualityMultiplier *= (1f + magnitude); // +15% purity
                        break;

                    case FocusAbilityType.Reclamation:
                        mods.WasteMultiplier *= (1f - magnitude); // 50% waste recovery
                        break;

                    case FocusAbilityType.Transmutation:
                        mods.BonusChance = math.max(mods.BonusChance, magnitude); // 10% rare byproduct
                        break;

                    case FocusAbilityType.GentleProcessing:
                        mods.QualityMultiplier *= (1f + magnitude * 0.5f); // Preserve properties
                        mods.SpeedMultiplier *= 0.7f; // Slower processing
                        break;
                }

                // Clamp modifiers to reasonable ranges
                mods.SpeedMultiplier = math.clamp(mods.SpeedMultiplier, 0.1f, 5f);
                mods.QualityMultiplier = math.clamp(mods.QualityMultiplier, 0.3f, 2f);
                mods.WasteMultiplier = math.clamp(mods.WasteMultiplier, 0.1f, 2f);
                mods.TargetCountMultiplier = math.clamp(mods.TargetCountMultiplier, 1f, 5f);
                mods.PerTargetEffectiveness = math.clamp(mods.PerTargetEffectiveness, 0.2f, 1f);
                mods.BonusChance = math.clamp(mods.BonusChance, 0f, 0.5f);
                mods.XPMultiplier = math.clamp(mods.XPMultiplier, 0.5f, 3f);
                mods.DurationMultiplier = math.clamp(mods.DurationMultiplier, 0.25f, 4f);
            }
        }
    }

    /// <summary>
    /// Validates profession focus ability requests against skill requirements.
    /// Filters out ability requests that the entity doesn't have the skill for.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FocusAbilitySystem))]
    public partial struct ProfessionFocusValidationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new ValidateProfessionAbilitiesJob
            {
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel();

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public partial struct ValidateProfessionAbilitiesJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(
                Entity entity,
                [ChunkIndexInQuery] int sortKey,
                ref FocusAbilityRequest request,
                in ProfessionSkills skills)
            {
                // Only validate profession abilities
                if (!FocusAbilityDefinitions.IsProfessionAbility(request.RequestedAbility))
                    return;

                // Get skill level for this ability's archetype
                byte skillLevel = ProfessionFocusHelpers.GetSkillLevelForAbility(request.RequestedAbility, skills);

                // Check if entity meets skill requirement
                if (!FocusAbilityDefinitions.CanUseProfessionAbility(request.RequestedAbility, skillLevel))
                {
                    // Clear the invalid request
                    ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                }
            }
        }
    }

    /// <summary>
    /// Static helper methods for integrating profession focus with job systems.
    /// </summary>
    public static class ProfessionFocusIntegration
    {
        /// <summary>
        /// Applies crafting modifiers to an item being crafted.
        /// </summary>
        public static (int quantity, byte quality, int waste) ApplyCraftingModifiers(
            int baseQuantity,
            byte baseQuality,
            int baseMaterialCost,
            float baseWasteRate,
            in ProfessionFocusModifiers mods,
            in ProfessionFocusConfig config,
            uint randomSeed)
        {
            // Calculate final quantity
            int quantity = (int)(baseQuantity * mods.TargetCountMultiplier);

            // Roll for bonus items
            if (ProfessionFocusHelpers.RollBonusChance(mods.BonusChance, randomSeed))
            {
                quantity++;
            }

            // Calculate final quality
            byte quality = ProfessionFocusHelpers.CalculateFinalQuality(
                baseQuality,
                mods.QualityMultiplier,
                config.MinQuality,
                config.MaxQuality);

            // Calculate waste
            int waste = ProfessionFocusHelpers.CalculateWaste(
                baseMaterialCost,
                mods.WasteMultiplier,
                baseWasteRate);

            return (quantity, quality, waste);
        }

        /// <summary>
        /// Applies gathering modifiers to a resource gather action.
        /// </summary>
        public static (int yield, int waste, bool preservedNode) ApplyGatheringModifiers(
            int baseYield,
            float baseWasteRate,
            in ProfessionFocusModifiers mods,
            uint randomSeed)
        {
            // Calculate final yield
            int yield = (int)(baseYield * mods.QualityMultiplier); // Quality = yield for gathering

            // Calculate waste
            int waste = (int)(baseYield * baseWasteRate * mods.WasteMultiplier);

            // Roll for node preservation (SustainableHarvest)
            bool preserved = ProfessionFocusHelpers.RollBonusChance(mods.BonusChance, randomSeed);

            return (yield, waste, preserved);
        }

        /// <summary>
        /// Applies healing modifiers to a heal action.
        /// </summary>
        public static (float healAmount, int targetCount) ApplyHealingModifiers(
            float baseHealAmount,
            in ProfessionFocusModifiers mods)
        {
            float healAmount = baseHealAmount * mods.QualityMultiplier * mods.PerTargetEffectiveness;
            int targetCount = (int)mods.TargetCountMultiplier;

            return (healAmount, targetCount);
        }

        /// <summary>
        /// Applies teaching modifiers to an XP transfer.
        /// </summary>
        public static (uint xpTransferred, float duration) ApplyTeachingModifiers(
            uint baseXP,
            float baseDuration,
            in ProfessionFocusModifiers mods)
        {
            uint xp = (uint)(baseXP * mods.XPMultiplier * mods.PerTargetEffectiveness);
            float duration = baseDuration * mods.DurationMultiplier / mods.SpeedMultiplier;

            return (xp, duration);
        }

        /// <summary>
        /// Applies refining modifiers to a refine action.
        /// </summary>
        public static (int output, byte purity, int waste) ApplyRefiningModifiers(
            int baseOutput,
            byte basePurity,
            int baseMaterials,
            float baseWasteRate,
            in ProfessionFocusModifiers mods,
            in ProfessionFocusConfig config,
            uint randomSeed)
        {
            // Calculate output (batch processing)
            int output = (int)(baseOutput * mods.TargetCountMultiplier);

            // Roll for rare byproduct
            if (ProfessionFocusHelpers.RollBonusChance(mods.BonusChance, randomSeed))
            {
                output++;
            }

            // Calculate purity
            byte purity = ProfessionFocusHelpers.CalculateFinalQuality(
                basePurity,
                mods.QualityMultiplier,
                config.MinQuality,
                config.MaxQuality);

            // Calculate waste
            int waste = ProfessionFocusHelpers.CalculateWaste(
                baseMaterials,
                mods.WasteMultiplier,
                baseWasteRate);

            return (output, purity, waste);
        }

        /// <summary>
        /// Calculates time for an action with speed modifiers.
        /// </summary>
        public static float CalculateActionTime(float baseTime, in ProfessionFocusModifiers mods)
        {
            return baseTime / math.max(0.1f, mods.SpeedMultiplier);
        }
    }
}

