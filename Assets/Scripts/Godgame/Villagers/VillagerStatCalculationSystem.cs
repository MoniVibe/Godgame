using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Calculates derived combat stats from base attributes.
    /// Runs after attribute changes to update combat stats.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerStatInitializationSystem))]
    public partial struct VillagerStatCalculationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(SystemAPI.QueryBuilder()
                .WithAll<VillagerAttributes, VillagerDerivedAttributes, VillagerCombatStats>()
                .Build());
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (attributes, derivedAttributes, combatStatsRef) in SystemAPI
                         .Query<RefRO<VillagerAttributes>, RefRO<VillagerDerivedAttributes>, RefRW<VillagerCombatStats>>())
            {
                var combatStats = combatStatsRef.ValueRO;
                var strength = derivedAttributes.ValueRO.Strength;
                var finesse = attributes.ValueRO.Finesse;
                var will = attributes.ValueRO.Will;
                var intelligence = derivedAttributes.ValueRO.Intelligence;

                // Track if we need to recalculate (if current value matches calculated value, it was auto-calculated)
                // For simplicity, we recalculate if the value seems auto-calculated (matches formula) or is 0
                // This allows explicit overrides to persist while auto-calculated values update with attributes

                // Calculate Attack if it's 0 or seems auto-calculated
                // Note: We recalculate every frame if Attack is 0, allowing dynamic updates
                if (combatStats.Attack == 0 && strength > 0 && finesse > 0)
                {
                    var calculatedAttack = (byte)math.clamp(
                        (finesse * 0.7f) + (strength * 0.3f),
                        0f, 100f);
                    combatStats.Attack = calculatedAttack;
                }

                // Calculate Defense if it's 0
                if (combatStats.Defense == 0 && finesse > 0)
                {
                    var calculatedDefense = (byte)math.clamp(finesse * 0.6f, 0f, 100f);
                    combatStats.Defense = calculatedDefense;
                }

                // Calculate MaxHealth if it's 0 or very low (likely auto-calculated)
                if (combatStats.MaxHealth <= 0f && strength > 0 && will > 0)
                {
                    var calculatedHealth = (strength * 0.6f) + (will * 0.4f) + 50f;
                    combatStats.MaxHealth = calculatedHealth;
                    // Only update CurrentHealth if it was also 0 or matches old MaxHealth
                    if (combatStats.CurrentHealth <= 0f || math.abs(combatStats.CurrentHealth - combatStats.MaxHealth) < 0.1f)
                    {
                        combatStats.CurrentHealth = combatStats.MaxHealth;
                    }
                }

                // Calculate Stamina if it's 0
                if (combatStats.Stamina == 0 && strength > 0)
                {
                    var calculatedStamina = (byte)math.clamp(strength / 10f, 1f, 100f);
                    combatStats.Stamina = calculatedStamina;
                    if (combatStats.CurrentStamina == 0)
                    {
                        combatStats.CurrentStamina = calculatedStamina;
                    }
                }

                // Calculate MaxMana if it's 0 (0 can mean auto-calculate or non-magic)
                // Only calculate if will and intelligence are both > 0 (indicating magic user)
                if (combatStats.MaxMana == 0 && will > 0 && intelligence > 0)
                {
                    var calculatedMana = (byte)math.clamp(
                        (will * 0.5f) + (intelligence * 0.5f),
                        0f, 100f);
                    combatStats.MaxMana = calculatedMana;
                    if (combatStats.CurrentMana == 0)
                    {
                        combatStats.CurrentMana = calculatedMana;
                    }
                }

                combatStatsRef.ValueRW = combatStats;
            }
        }
    }
}

