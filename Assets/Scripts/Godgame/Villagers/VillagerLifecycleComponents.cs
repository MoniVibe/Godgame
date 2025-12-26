using Godgame.Relations;
using Unity.Entities;

namespace Godgame.Villagers
{
    public enum VillagerLifeStage : byte
    {
        Child = 0,
        Youth = 1,
        Adult = 2,
        Elder = 3
    }

    public enum VillagerSex : byte
    {
        None = 0,
        Female = 1,
        Male = 2
    }

    /// <summary>
    /// Tracks villager age and life stage.
    /// </summary>
    public struct VillagerLifecycleState : IComponentData
    {
        public float AgeDays;
        public VillagerLifeStage Stage;
        public uint LastUpdateTick;
    }

    /// <summary>
    /// Tracks fertility and pregnancy state.
    /// </summary>
    public struct VillagerReproductionState : IComponentData
    {
        public VillagerSex Sex;
        public float Fertility; // 0..1
        public byte IsPregnant;
        public float PregnancyDays;
        public uint ConceptionTick;
        public uint NextConceptionTick;
        public Entity Partner;
        public ushort BirthCount;
        public byte PendingBirths;
    }

    /// <summary>
    /// Tuning for lifecycle and breeding cadence.
    /// </summary>
    public struct VillagerLifecycleTuning : IComponentData
    {
        public float SecondsPerDay;
        public float ChildStageDays;
        public float YouthStageDays;
        public float AdultStageDays;
        public float ElderStageDays;

        public float FertilityStartDays;
        public float FertilityEndDays;
        public float PregnancyDays;
        public float BreedingCooldownDays;
        public float BreedingChancePerDay;
        public float BreedingDistance;
        public int LifecycleCadenceTicks;
        public int BreedingCadenceTicks;
        public RelationTier MinRelationTier;
    }
}
