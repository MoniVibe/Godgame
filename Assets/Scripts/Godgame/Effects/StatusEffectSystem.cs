using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Effects
{
    /// <summary>
    /// System that processes status effects: ticks durations, applies periodic damage/healing,
    /// removes expired effects, and computes modifier totals.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct StatusEffectSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            float deltaTime = timeState.DeltaTime;

            if (deltaTime <= 0)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Process all entities with status effects
            foreach (var (effects, modifiers, entity) in SystemAPI
                .Query<DynamicBuffer<StatusEffect>, RefRW<StatusEffectModifiers>>()
                .WithEntityAccess())
            {
                ProcessEffects(effects, ref modifiers.ValueRW, deltaTime, entity, ref ecb);
            }

            // Process entities without modifiers component (add it)
            foreach (var (effects, entity) in SystemAPI
                .Query<DynamicBuffer<StatusEffect>>()
                .WithEntityAccess()
                .WithNone<StatusEffectModifiers>())
            {
                if (effects.Length > 0)
                {
                    ecb.AddComponent(entity, StatusEffectModifiers.Default);
                    ecb.AddComponent(entity, new HasStatusEffectsTag());
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void ProcessEffects(
            DynamicBuffer<StatusEffect> effects,
            ref StatusEffectModifiers modifiers,
            float deltaTime,
            Entity entity,
            ref EntityCommandBuffer ecb)
        {
            // Reset modifiers
            modifiers = StatusEffectModifiers.Default;

            // Track which effects to remove
            var toRemove = new NativeList<int>(effects.Length, Allocator.Temp);

            // Process each effect
            for (int i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];

                // Tick duration (unless permanent)
                if (effect.Duration > 0)
                {
                    effect.Duration -= deltaTime;
                    if (effect.Duration <= 0)
                    {
                        effect.Duration = 0;
                        toRemove.Add(i);
                        effects[i] = effect;
                        continue;
                    }
                }

                // Process periodic effects
                if (effect.TickInterval > 0)
                {
                    effect.TimeSinceLastTick += deltaTime;
                    // Tick logic handled elsewhere (damage systems)
                    effects[i] = effect;
                }

                // Apply effect to modifiers
                ApplyEffectToModifiers(in effect, ref modifiers);

                effects[i] = effect;
            }

            // Remove expired effects (iterate backwards to preserve indices)
            for (int i = toRemove.Length - 1; i >= 0; i--)
            {
                effects.RemoveAt(toRemove[i]);
            }

            toRemove.Dispose();

            // Remove tag if no effects remain
            if (effects.Length == 0)
            {
                ecb.RemoveComponent<HasStatusEffectsTag>(entity);
            }
        }

        private void ApplyEffectToModifiers(in StatusEffect effect, ref StatusEffectModifiers modifiers)
        {
            float effectiveMagnitude = effect.Magnitude * effect.StackCount;

            switch (effect.Type)
            {
                // Speed modifiers
                case StatusEffectType.SpeedBoost:
                case StatusEffectType.Haste:
                    modifiers.SpeedMultiplier *= (1f + effectiveMagnitude);
                    break;

                case StatusEffectType.SpeedSlow:
                case StatusEffectType.Chilled:
                    modifiers.SpeedMultiplier *= math.max(0.1f, 1f - effectiveMagnitude);
                    break;

                // Damage modifiers
                case StatusEffectType.DamageBoost:
                case StatusEffectType.Inspired:
                    modifiers.DamageMultiplier *= (1f + effectiveMagnitude);
                    break;

                case StatusEffectType.Weakened:
                    modifiers.DamageMultiplier *= math.max(0.1f, 1f - effectiveMagnitude);
                    break;

                case StatusEffectType.Vulnerable:
                case StatusEffectType.Marked:
                    modifiers.DamageTakenMultiplier *= (1f + effectiveMagnitude);
                    break;

                case StatusEffectType.DamageReduction:
                case StatusEffectType.Shield:
                    modifiers.DamageTakenMultiplier *= math.max(0.1f, 1f - effectiveMagnitude);
                    break;

                // Armor
                case StatusEffectType.ArmorBoost:
                    modifiers.ArmorBonus += effectiveMagnitude;
                    break;

                case StatusEffectType.ArmorBreak:
                    modifiers.ArmorBonus -= effectiveMagnitude;
                    break;

                // Crowd control
                case StatusEffectType.Stunned:
                case StatusEffectType.Feared:
                    modifiers.CanMove = false;
                    modifiers.CanAttack = false;
                    modifiers.CanCast = false;
                    break;

                case StatusEffectType.Rooted:
                    modifiers.CanMove = false;
                    break;

                case StatusEffectType.Silenced:
                    modifiers.CanCast = false;
                    break;

                case StatusEffectType.Charmed:
                case StatusEffectType.Confused:
                    // AI system handles these
                    break;

                // Visibility
                case StatusEffectType.Invisible:
                    modifiers.IsVisible = false;
                    break;

                // DoT effects
                case StatusEffectType.Bleed:
                case StatusEffectType.Poison:
                case StatusEffectType.Burn:
                case StatusEffectType.OnFire:
                case StatusEffectType.Shock:
                    modifiers.TotalDoTDamage += effectiveMagnitude;
                    break;

                // HoT effects
                case StatusEffectType.Regeneration:
                case StatusEffectType.HealOverTime:
                    modifiers.TotalHoTHealing += effectiveMagnitude;
                    break;

                // Blessed/Cursed
                case StatusEffectType.Blessed:
                case StatusEffectType.Divine:
                    modifiers.HealingMultiplier *= (1f + effectiveMagnitude * 0.5f);
                    modifiers.DamageTakenMultiplier *= (1f - effectiveMagnitude * 0.25f);
                    break;

                case StatusEffectType.Cursed:
                case StatusEffectType.Demonic:
                    modifiers.HealingMultiplier *= math.max(0.1f, 1f - effectiveMagnitude * 0.5f);
                    modifiers.DamageTakenMultiplier *= (1f + effectiveMagnitude * 0.25f);
                    break;
            }
        }
    }

    /// <summary>
    /// System that processes apply/remove status effect requests.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(StatusEffectSystem))]
    public partial struct StatusEffectRequestSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Process apply requests
            foreach (var (request, entity) in SystemAPI.Query<RefRO<ApplyStatusEffectRequest>>().WithEntityAccess())
            {
                var target = request.ValueRO.Target;
                var effect = request.ValueRO.Effect;

                if (target != Entity.Null && state.EntityManager.Exists(target))
                {
                    // Ensure target has effect buffer
                    if (!SystemAPI.HasBuffer<StatusEffect>(target))
                    {
                        ecb.AddBuffer<StatusEffect>(target);
                    }

                    // Add effect (stacking logic would go here)
                    var buffer = SystemAPI.GetBuffer<StatusEffect>(target);
                    bool found = false;

                    // Check for existing effect of same type (stacking)
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i].Type == effect.Type && buffer[i].Source == effect.Source)
                        {
                            var existing = buffer[i];
                            if (existing.StackCount < existing.MaxStacks)
                            {
                                existing.StackCount++;
                                existing.Duration = math.max(existing.Duration, effect.Duration);
                            }
                            else
                            {
                                // Refresh duration only
                                existing.Duration = math.max(existing.Duration, effect.Duration);
                            }
                            buffer[i] = existing;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        buffer.Add(effect);
                    }
                }

                // Remove request
                ecb.DestroyEntity(entity);
            }

            // Process remove requests
            foreach (var (request, entity) in SystemAPI.Query<RefRO<RemoveStatusEffectRequest>>().WithEntityAccess())
            {
                var target = request.ValueRO.Target;
                var effectType = request.ValueRO.Type;

                if (target != Entity.Null && SystemAPI.HasBuffer<StatusEffect>(target))
                {
                    var buffer = SystemAPI.GetBuffer<StatusEffect>(target);

                    for (int i = buffer.Length - 1; i >= 0; i--)
                    {
                        if (buffer[i].Type == effectType)
                        {
                            if (request.ValueRO.RemoveAllStacks)
                            {
                                buffer.RemoveAt(i);
                            }
                            else
                            {
                                var effect = buffer[i];
                                if (effect.StackCount > 1)
                                {
                                    effect.StackCount--;
                                    buffer[i] = effect;
                                }
                                else
                                {
                                    buffer.RemoveAt(i);
                                }
                                break;
                            }
                        }
                    }
                }

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Helper methods for applying status effects.
    /// </summary>
    public static class StatusEffectHelper
    {
        /// <summary>
        /// Create an entity command to apply a status effect.
        /// </summary>
        public static Entity CreateApplyRequest(
            ref EntityCommandBuffer ecb,
            Entity target,
            StatusEffect effect)
        {
            var requestEntity = ecb.CreateEntity();
            ecb.AddComponent(requestEntity, new ApplyStatusEffectRequest
            {
                Target = target,
                Effect = effect
            });
            return requestEntity;
        }

        /// <summary>
        /// Create an entity command to remove a status effect.
        /// </summary>
        public static Entity CreateRemoveRequest(
            ref EntityCommandBuffer ecb,
            Entity target,
            StatusEffectType type,
            bool removeAllStacks = true)
        {
            var requestEntity = ecb.CreateEntity();
            ecb.AddComponent(requestEntity, new RemoveStatusEffectRequest
            {
                Target = target,
                Type = type,
                RemoveAllStacks = removeAllStacks
            });
            return requestEntity;
        }
    }
}

