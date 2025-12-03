using PureDOTS.Runtime.Components;
using Unity.Entities;

namespace Godgame
{
    /// <summary>
    /// Runtime component describing villager spawner settings. Used by authoring bakers and registry sync systems.
    /// </summary>
    public struct VillageSpawnerConfig : IComponentData
    {
        public Entity VillagerPrefab;
        public int VillagerCount;
        public float SpawnRadius;
        public VillagerJob.JobType DefaultJobType;
        public VillagerAIState.Goal DefaultAIGoal;
        public int SpawnedCount;
    }
}
