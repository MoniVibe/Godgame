using PureDOTS.Runtime.Identity;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures a default cooldown profile exists when no authoring is present.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerCooldownProfileBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<VillagerCooldownProfile>())
            {
                return;
            }

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, VillagerCooldownProfile.Default);

            var outlookRules = state.EntityManager.AddBuffer<VillagerCooldownOutlookRule>(entity);
            AddDefaultOutlookRules(ref outlookRules);
            state.EntityManager.AddBuffer<VillagerCooldownArchetypeModifier>(entity);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

        private static void AddDefaultOutlookRules(ref DynamicBuffer<VillagerCooldownOutlookRule> buffer)
        {
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Warlike,
                CooldownScale = 0.85f,
                SocializeWeight = -0.2f,
                WanderWeight = 0.25f,
                PressureEnterScale = 0.9f,
                PressureExitScale = 0.85f,
                CrowdingNeighborCapScale = 1.2f,
                CadenceScale = 0.85f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Peaceful,
                CooldownScale = 1.2f,
                SocializeWeight = 0.3f,
                WanderWeight = -0.1f,
                PressureEnterScale = 1.1f,
                PressureExitScale = 1.15f,
                CrowdingNeighborCapScale = 0.85f,
                CadenceScale = 1.15f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Spiritual,
                CooldownScale = 1.15f,
                SocializeWeight = 0.2f,
                WanderWeight = 0f,
                PressureEnterScale = 1.05f,
                PressureExitScale = 1.05f,
                CrowdingNeighborCapScale = 0.95f,
                CadenceScale = 1.1f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Materialistic,
                CooldownScale = 0.9f,
                SocializeWeight = -0.1f,
                WanderWeight = 0.15f,
                PressureEnterScale = 0.95f,
                PressureExitScale = 0.95f,
                CrowdingNeighborCapScale = 1.1f,
                CadenceScale = 0.9f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Scholarly,
                CooldownScale = 1.05f,
                SocializeWeight = 0.1f,
                WanderWeight = 0.1f,
                PressureEnterScale = 1f,
                PressureExitScale = 1f,
                CrowdingNeighborCapScale = 0.95f,
                CadenceScale = 1.05f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Pragmatic,
                CooldownScale = 0.95f,
                SocializeWeight = 0f,
                WanderWeight = 0.1f,
                PressureEnterScale = 0.95f,
                PressureExitScale = 0.95f,
                CrowdingNeighborCapScale = 1f,
                CadenceScale = 0.95f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Xenophobic,
                CooldownScale = 1f,
                SocializeWeight = -0.25f,
                WanderWeight = 0.2f,
                PressureEnterScale = 1.05f,
                PressureExitScale = 1.05f,
                CrowdingNeighborCapScale = 0.7f,
                CadenceScale = 0.9f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Egalitarian,
                CooldownScale = 1.15f,
                SocializeWeight = 0.25f,
                WanderWeight = 0f,
                PressureEnterScale = 1.05f,
                PressureExitScale = 1.1f,
                CrowdingNeighborCapScale = 0.9f,
                CadenceScale = 1.05f
            });
            buffer.Add(new VillagerCooldownOutlookRule
            {
                OutlookType = (byte)OutlookType.Authoritarian,
                CooldownScale = 1.05f,
                SocializeWeight = 0.05f,
                WanderWeight = 0.1f,
                PressureEnterScale = 0.95f,
                PressureExitScale = 0.95f,
                CrowdingNeighborCapScale = 1.1f,
                CadenceScale = 0.95f
            });
        }
    }
}
