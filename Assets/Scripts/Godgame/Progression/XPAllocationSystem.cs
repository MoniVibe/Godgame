using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Progression
{
    /// <summary>
    /// Allocates XP to skill domains based on actions performed.
    /// Preordained paths receive XP multipliers for their domains.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct XPAllocationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create config singleton if it doesn't exist
            if (!SystemAPI.HasSingleton<ProgressionConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, ProgressionConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused) return;

            var config = SystemAPI.GetSingleton<ProgressionConfig>();

            // Process XP award events
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (xpEvent, progression, skillXPs, path, entity) in SystemAPI.Query<
                RefRO<XPAwardEvent>,
                RefRW<CharacterProgression>,
                DynamicBuffer<SkillXP>,
                RefRO<PreordainedPath>>()
                .WithEntityAccess())
            {
                ProcessXPAward(
                    xpEvent.ValueRO,
                    ref progression.ValueRW,
                    skillXPs,
                    path.ValueRO,
                    config);

                // Remove the event after processing
                ecb.RemoveComponent<XPAwardEvent>(entity);
            }

            // Also handle entities without preordained paths
            foreach (var (xpEvent, progression, skillXPs, entity) in SystemAPI.Query<
                RefRO<XPAwardEvent>,
                RefRW<CharacterProgression>,
                DynamicBuffer<SkillXP>>()
                .WithNone<PreordainedPath>()
                .WithEntityAccess())
            {
                ProcessXPAwardWithoutPath(
                    xpEvent.ValueRO,
                    ref progression.ValueRW,
                    skillXPs,
                    config);

                ecb.RemoveComponent<XPAwardEvent>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void ProcessXPAward(
            in XPAwardEvent xpEvent,
            ref CharacterProgression progression,
            DynamicBuffer<SkillXP> skillXPs,
            in PreordainedPath path,
            in ProgressionConfig config)
        {
            // Calculate XP with path multipliers
            float multiplier = 1f;
            if (path.IsActive)
            {
                if (xpEvent.Domain == path.PrimaryDomain)
                    multiplier = config.PreordainedPrimaryMultiplier;
                else if (xpEvent.Domain == path.SecondaryDomain)
                    multiplier = config.PreordainedSecondaryMultiplier;
            }

            uint finalXP = (uint)(xpEvent.Amount * multiplier);

            // Add to total XP
            progression.TotalXP += finalXP;

            // Add to domain-specific XP
            for (int i = 0; i < skillXPs.Length; i++)
            {
                if (skillXPs[i].Domain == xpEvent.Domain)
                {
                    var skill = skillXPs[i];
                    skill.CurrentXP += finalXP;
                    skillXPs[i] = skill;
                    break;
                }
            }

            // Check for level up
            CheckLevelUp(ref progression, config);
        }

        private void ProcessXPAwardWithoutPath(
            in XPAwardEvent xpEvent,
            ref CharacterProgression progression,
            DynamicBuffer<SkillXP> skillXPs,
            in ProgressionConfig config)
        {
            // No path multiplier
            progression.TotalXP += xpEvent.Amount;

            // Add to domain-specific XP
            for (int i = 0; i < skillXPs.Length; i++)
            {
                if (skillXPs[i].Domain == xpEvent.Domain)
                {
                    var skill = skillXPs[i];
                    skill.CurrentXP += xpEvent.Amount;
                    skillXPs[i] = skill;
                    break;
                }
            }

            CheckLevelUp(ref progression, config);
        }

        private void CheckLevelUp(ref CharacterProgression progression, in ProgressionConfig config)
        {
            while (progression.TotalXP >= progression.XPToNextLevel && progression.Level < 100)
            {
                progression.Level++;
                progression.AvailableSkillPoints += config.SkillPointsPerLevel;
                progression.XPToNextLevel = CharacterProgression.CalculateXPForLevel(progression.Level);
            }
        }
    }

    /// <summary>
    /// Helper system to award XP from various game actions.
    /// Other systems call these helpers to award XP without directly manipulating components.
    /// </summary>
    public partial struct XPAwardHelper : ISystem
    {
        /// <summary>
        /// Awards combat XP for killing an enemy.
        /// </summary>
        public static void AwardCombatKillXP(
            EntityCommandBuffer ecb,
            Entity entity,
            uint enemyLevel,
            uint currentTick)
        {
            uint baseXP = 10 + (enemyLevel * 5);
            ecb.AddComponent(entity, new XPAwardEvent
            {
                Amount = baseXP,
                Domain = SkillDomain.Combat,
                Source = "CombatKill",
                AwardedTick = currentTick
            });
        }

        /// <summary>
        /// Awards crafting XP for completing an item.
        /// </summary>
        public static void AwardCraftingXP(
            EntityCommandBuffer ecb,
            Entity entity,
            byte itemTier,
            uint currentTick)
        {
            uint baseXP = 5 + (uint)(itemTier * 10);
            ecb.AddComponent(entity, new XPAwardEvent
            {
                Amount = baseXP,
                Domain = SkillDomain.Crafting,
                Source = "Crafting",
                AwardedTick = currentTick
            });
        }

        /// <summary>
        /// Awards divine XP for performing worship or miracles.
        /// </summary>
        public static void AwardDivineXP(
            EntityCommandBuffer ecb,
            Entity entity,
            uint amount,
            uint currentTick)
        {
            ecb.AddComponent(entity, new XPAwardEvent
            {
                Amount = amount,
                Domain = SkillDomain.Divine,
                Source = "Worship",
                AwardedTick = currentTick
            });
        }

        /// <summary>
        /// Awards leadership XP for leading successful actions.
        /// </summary>
        public static void AwardLeadershipXP(
            EntityCommandBuffer ecb,
            Entity entity,
            byte followersInvolved,
            uint currentTick)
        {
            uint baseXP = 5 + (uint)(followersInvolved * 2);
            ecb.AddComponent(entity, new XPAwardEvent
            {
                Amount = baseXP,
                Domain = SkillDomain.Leadership,
                Source = "Leadership",
                AwardedTick = currentTick
            });
        }

        /// <summary>
        /// Awards trade XP for completing transactions.
        /// </summary>
        public static void AwardTradeXP(
            EntityCommandBuffer ecb,
            Entity entity,
            uint transactionValue,
            uint currentTick)
        {
            uint baseXP = math.max(1, transactionValue / 100);
            ecb.AddComponent(entity, new XPAwardEvent
            {
                Amount = baseXP,
                Domain = SkillDomain.Trade,
                Source = "Trade",
                AwardedTick = currentTick
            });
        }
    }
}

