using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Progression
{
    /// <summary>
    /// Unlocks skills when XP thresholds are reached.
    /// Prioritizes skills on the preordained path.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(XPAllocationSystem))]
    public partial struct SkillUnlockSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused) return;

            var currentTick = timeState.Tick;

            // Check for mastery upgrades and skill unlocks
            foreach (var (progression, skillXPs, unlockedSkills, path) in SystemAPI.Query<
                RefRO<CharacterProgression>,
                DynamicBuffer<SkillXP>,
                DynamicBuffer<UnlockedSkill>,
                RefRW<PreordainedPath>>())
            {
                // Check for mastery upgrades in each domain
                UpdateSkills(skillXPs, currentTick);

                // Update path progress
                if (path.ValueRO.IsActive)
                {
                    var pathValue = path.ValueRW;
                    UpdatePathProgress(ref pathValue, skillXPs);
                    path.ValueRW = pathValue;
                }
            }

            // Handle entities without preordained paths
            foreach (var (progression, skillXPs) in SystemAPI.Query<
                RefRO<CharacterProgression>,
                DynamicBuffer<SkillXP>>()
                .WithNone<PreordainedPath>())
            {
                UpdateSkills(skillXPs, currentTick);
            }
        }

        private void CheckMasteryUpgrade(ref SkillXP skill, uint currentTick)
        {
            // Check if XP threshold reached for next mastery
            while (skill.CurrentXP >= skill.XPToNextMastery && skill.Mastery < SkillMastery.Grandmaster)
            {
                skill.Mastery = (SkillMastery)((byte)skill.Mastery + 1);
                skill.XPToNextMastery = GetXPForMastery(skill.Mastery);
            }
        }

        private void UpdateSkills(DynamicBuffer<SkillXP> skillXPs, uint currentTick)
        {
            for (int i = 0; i < skillXPs.Length; i++)
            {
                var skill = skillXPs[i];
                CheckMasteryUpgrade(ref skill, currentTick);
                skillXPs[i] = skill;
            }
        }

        private uint GetXPForMastery(SkillMastery mastery)
        {
            return mastery switch
            {
                SkillMastery.Novice => 500,
                SkillMastery.Apprentice => 1500,
                SkillMastery.Journeyman => 4000,
                SkillMastery.Expert => 10000,
                SkillMastery.Master => 25000,
                SkillMastery.Grandmaster => 50000,
                _ => 500
            };
        }

        private void UpdatePathProgress(ref PreordainedPath path, DynamicBuffer<SkillXP> skillXPs)
        {
            // Calculate progress based on primary and secondary domain mastery
            float primaryProgress = 0f;
            float secondaryProgress = 0f;

            for (int i = 0; i < skillXPs.Length; i++)
            {
                if (skillXPs[i].Domain == path.PrimaryDomain)
                {
                    primaryProgress = GetMasteryProgressPercent(skillXPs[i].Mastery);
                }
                else if (skillXPs[i].Domain == path.SecondaryDomain)
                {
                    secondaryProgress = GetMasteryProgressPercent(skillXPs[i].Mastery);
                }
            }

            // Primary domain counts 70%, secondary 30%
            float totalProgress = (primaryProgress * 0.7f) + (secondaryProgress * 0.3f);
            path.Progress = (byte)math.clamp(totalProgress * 100f, 0f, 100f);
        }

        private float GetMasteryProgressPercent(SkillMastery mastery)
        {
            return mastery switch
            {
                SkillMastery.Novice => 0f,
                SkillMastery.Apprentice => 0.2f,
                SkillMastery.Journeyman => 0.4f,
                SkillMastery.Expert => 0.6f,
                SkillMastery.Master => 0.8f,
                SkillMastery.Grandmaster => 1f,
                _ => 0f
            };
        }
    }

    /// <summary>
    /// Initializes skill XP buffers when CharacterProgression is added.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SkillXPInitializationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Initialize skill XP buffers for entities with progression but no skill XPs
            foreach (var (progression, entity) in SystemAPI.Query<RefRO<CharacterProgression>>()
                .WithNone<SkillXP>()
                .WithEntityAccess())
            {
                var buffer = ecb.AddBuffer<SkillXP>(entity);

                // Add entries for all skill domains
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Combat));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Arcane));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Stealth));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Divine));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Crafting));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Leadership));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Trade));
                buffer.Add(SkillXP.CreateForDomain(SkillDomain.Survival));
            }

            // Initialize unlocked skills buffer
            foreach (var (progression, entity) in SystemAPI.Query<RefRO<CharacterProgression>>()
                .WithNone<UnlockedSkill>()
                .WithEntityAccess())
            {
                ecb.AddBuffer<UnlockedSkill>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

