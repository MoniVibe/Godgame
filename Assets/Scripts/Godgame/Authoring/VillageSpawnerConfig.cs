using PureDOTS.Runtime.Components;
using Unity.Entities;

namespace Godgame.Authoring
{
    /// <summary>
    /// Runtime component describing villager spawner settings; mirrors the PureDOTS sample definition to unblock registry sync.
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
