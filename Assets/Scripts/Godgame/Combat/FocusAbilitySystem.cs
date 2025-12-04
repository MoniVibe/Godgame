using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Processes focus ability requests and manages active abilities.
    /// Handles:
    /// - Activating new abilities (checking requirements/cost)
    /// - Draining focus for active abilities
    /// - Deactivating abilities when focus depletes or duration expires
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FocusRegenSystem))]
    public partial struct FocusAbilitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = timeState.DeltaTime;
            var currentTick = timeState.Tick;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Process ability requests
            new ProcessAbilityRequestsJob
            {
                CurrentTick = currentTick,
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel();

            // Update active abilities (drain, duration)
            new UpdateActiveAbilitiesJob
            {
                DeltaTime = deltaTime,
                CurrentTick = currentTick
            }.ScheduleParallel();

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public partial struct ProcessAbilityRequestsJob : IJobEntity
        {
            public uint CurrentTick;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(
                Entity entity,
                [ChunkIndexInQuery] int sortKey,
                ref EntityFocus focus,
                ref FocusAbilityRequest request,
                DynamicBuffer<ActiveFocusAbility> activeAbilities,
                in CombatStats stats)
            {
                // Ignore empty requests
                if (request.RequestedAbility == FocusAbilityType.None)
                {
                    ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                    return;
                }

                // Check if toggling off an existing ability
                if (request.ToggleOff)
                {
                    for (int i = activeAbilities.Length - 1; i >= 0; i--)
                    {
                        if (activeAbilities[i].AbilityType == request.RequestedAbility)
                        {
                            activeAbilities.RemoveAt(i);
                            break;
                        }
                    }
                    ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                    return;
                }

                // Check if entity can use this ability
                if (!FocusAbilityDefinitions.CanUseAbility(request.RequestedAbility, stats))
                {
                    ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                    return;
                }

                // Check if entity has enough focus
                float cost = FocusAbilityDefinitions.GetBaseCost(request.RequestedAbility);
                var costType = FocusAbilityDefinitions.GetCostType(request.RequestedAbility);

                // Burst abilities need full cost upfront
                if (costType == FocusCostType.Burst && focus.CurrentFocus < cost)
                {
                    ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                    return;
                }

                // Per-second/per-use just need some focus to start
                if (focus.CurrentFocus < cost * 0.5f)
                {
                    ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                    return;
                }

                // Check if already active (for toggle abilities)
                bool isToggle = FocusAbilityDefinitions.IsToggleAbility(request.RequestedAbility);
                if (isToggle)
                {
                    for (int i = 0; i < activeAbilities.Length; i++)
                    {
                        if (activeAbilities[i].AbilityType == request.RequestedAbility)
                        {
                            // Already active, just remove request
                            ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
                            return;
                        }
                    }
                }

                // Activate the ability
                float effectMagnitude = FocusAbilityDefinitions.GetEffectMagnitude(request.RequestedAbility);
                float effectiveness = FocusAbilityDefinitions.GetEffectivenessMultiplier(request.RequestedAbility, stats);
                float duration = FocusAbilityDefinitions.GetDefaultDuration(request.RequestedAbility);

                var newAbility = new ActiveFocusAbility
                {
                    AbilityType = request.RequestedAbility,
                    CostType = costType,
                    DrainRate = cost, // For per-second, this is per second; for burst, this was the one-time cost
                    RemainingDuration = isToggle ? 0f : (duration > 0f ? duration : 1f), // 0 = toggle/indefinite
                    EffectMagnitude = effectMagnitude * effectiveness,
                    ActivatedTick = CurrentTick,
                    IsToggle = isToggle
                };

                // Apply burst cost immediately
                if (costType == FocusCostType.Burst)
                {
                    focus.CurrentFocus -= cost;
                    newAbility.DrainRate = 0f; // Already paid
                }

                activeAbilities.Add(newAbility);

                // Remove the processed request
                ECB.RemoveComponent<FocusAbilityRequest>(sortKey, entity);
            }
        }

        [BurstCompile]
        public partial struct UpdateActiveAbilitiesJob : IJobEntity
        {
            public float DeltaTime;
            public uint CurrentTick;

            public void Execute(ref EntityFocus focus, DynamicBuffer<ActiveFocusAbility> activeAbilities)
            {
                // Skip if in coma
                if (focus.IsInComa) return;

                // Process each active ability
                for (int i = activeAbilities.Length - 1; i >= 0; i--)
                {
                    var ability = activeAbilities[i];
                    bool shouldRemove = false;

                    // Handle per-second drain
                    if (ability.CostType == FocusCostType.PerSecond)
                    {
                        float drainThisFrame = ability.DrainRate * DeltaTime;

                        // Check if we can afford to keep it active
                        if (focus.CurrentFocus < drainThisFrame)
                        {
                            shouldRemove = true;
                        }
                        else
                        {
                            focus.CurrentFocus -= drainThisFrame;
                        }
                    }

                    // Handle duration-based abilities
                    if (!ability.IsToggle && ability.RemainingDuration > 0f)
                    {
                        ability.RemainingDuration -= DeltaTime;
                        if (ability.RemainingDuration <= 0f)
                        {
                            shouldRemove = true;
                        }
                        else
                        {
                            activeAbilities[i] = ability;
                        }
                    }

                    if (shouldRemove)
                    {
                        activeAbilities.RemoveAt(i);
                    }
                }

                // Clamp focus after draining
                focus.CurrentFocus = math.max(0f, focus.CurrentFocus);
            }
        }
    }

    /// <summary>
    /// Helper system to apply focus ability effects to combat/stats.
    /// This provides example integration points for other systems.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FocusAbilitySystem))]
    public partial struct FocusEffectApplicationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // This system provides static helper methods for other systems to query
            // active focus abilities and their effects.
            // Actual effect application happens in the relevant combat/spell systems.
        }
    }

    /// <summary>
    /// Static helpers to query focus ability effects from other systems.
    /// </summary>
    public static class FocusEffectHelpers
    {
        /// <summary>
        /// Checks if an entity has an active focus ability of the given type.
        /// </summary>
        public static bool HasActiveAbility(in DynamicBuffer<ActiveFocusAbility> abilities, FocusAbilityType type)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == type)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the effect magnitude for an active ability.
        /// Returns 0 if not active.
        /// </summary>
        public static float GetAbilityMagnitude(in DynamicBuffer<ActiveFocusAbility> abilities, FocusAbilityType type)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == type)
                    return abilities[i].EffectMagnitude;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the attack speed multiplier from focus abilities.
        /// </summary>
        public static float GetAttackSpeedMultiplier(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            float multiplier = 1f;

            for (int i = 0; i < abilities.Length; i++)
            {
                switch (abilities[i].AbilityType)
                {
                    case FocusAbilityType.DualWieldStrike:
                        multiplier *= abilities[i].EffectMagnitude; // 2x
                        break;
                    case FocusAbilityType.AttackSpeedBoost:
                        multiplier *= (1f + abilities[i].EffectMagnitude); // +30%
                        break;
                    case FocusAbilityType.RapidFire:
                        multiplier *= abilities[i].EffectMagnitude; // 1.5x
                        break;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// Gets the damage reduction from focus abilities.
        /// </summary>
        public static float GetDamageReduction(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            float reduction = 0f;

            for (int i = 0; i < abilities.Length; i++)
            {
                switch (abilities[i].AbilityType)
                {
                    case FocusAbilityType.IgnorePain:
                        reduction += abilities[i].EffectMagnitude; // 25%
                        break;
                    case FocusAbilityType.Fortify:
                        reduction = 1f; // 100% (invulnerable)
                        break;
                }
            }

            return math.min(reduction, 1f);
        }

        /// <summary>
        /// Gets the dodge bonus from focus abilities.
        /// </summary>
        public static float GetDodgeBonus(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == FocusAbilityType.DodgeBoost)
                    return abilities[i].EffectMagnitude;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the crit chance bonus from focus abilities.
        /// </summary>
        public static float GetCritBonus(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == FocusAbilityType.CriticalFocus)
                    return abilities[i].EffectMagnitude;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the spell power multiplier from focus abilities.
        /// </summary>
        public static float GetSpellPowerMultiplier(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            float multiplier = 1f;

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == FocusAbilityType.EmpowerCast)
                {
                    multiplier *= (1f + abilities[i].EffectMagnitude);
                }
            }

            return multiplier;
        }

        /// <summary>
        /// Gets the cooldown reduction from focus abilities (0-1 range).
        /// </summary>
        public static float GetCooldownReduction(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == FocusAbilityType.CooldownReduction)
                    return abilities[i].EffectMagnitude;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the mana regen multiplier from focus abilities.
        /// </summary>
        public static float GetManaRegenMultiplier(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            float multiplier = 1f;

            for (int i = 0; i < abilities.Length; i++)
            {
                switch (abilities[i].AbilityType)
                {
                    case FocusAbilityType.ManaRegen:
                        multiplier *= (1f + abilities[i].EffectMagnitude);
                        break;
                    case FocusAbilityType.ManaChannel:
                        multiplier *= abilities[i].EffectMagnitude;
                        break;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// Gets the summon cap bonus from focus abilities.
        /// </summary>
        public static int GetSummonCapBonus(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == FocusAbilityType.SummonBoost)
                    return (int)abilities[i].EffectMagnitude;
            }
            return 0;
        }

        /// <summary>
        /// Gets the buff duration multiplier from focus abilities.
        /// </summary>
        public static float GetBuffDurationMultiplier(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == FocusAbilityType.BuffExtend)
                    return 1f + abilities[i].EffectMagnitude;
            }
            return 1f;
        }

        /// <summary>
        /// Checks if entity can parry (and should attempt parry check).
        /// </summary>
        public static bool CanParry(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            return HasActiveAbility(abilities, FocusAbilityType.Parry);
        }

        /// <summary>
        /// Checks if next cast should be multicast.
        /// </summary>
        public static bool ShouldMulticast(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            return HasActiveAbility(abilities, FocusAbilityType.Multicast);
        }

        /// <summary>
        /// Checks if next cast should be instant.
        /// </summary>
        public static bool ShouldQuickCast(in DynamicBuffer<ActiveFocusAbility> abilities)
        {
            return HasActiveAbility(abilities, FocusAbilityType.QuickCast);
        }

        /// <summary>
        /// Gets the number of targets for multi-target abilities.
        /// </summary>
        public static int GetMultiTargetCount(in DynamicBuffer<ActiveFocusAbility> abilities, FocusAbilityType type)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityType == type)
                    return (int)abilities[i].EffectMagnitude;
            }
            return 1;
        }
    }
}

