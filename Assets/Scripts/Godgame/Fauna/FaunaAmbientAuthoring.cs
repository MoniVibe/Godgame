using Godgame.Environment;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Fauna
{
    [DisallowMultipleComponent]
    public sealed class FaunaAmbientAuthoring : MonoBehaviour
    {
        [SerializeField] private FaunaAmbientProfile profile;

        [Min(1f)]
        [SerializeField] private float radius = 25f;

        [SerializeField] [Range(0.25f, 60f)] private float spawnIntervalSeconds = 6f;

        [SerializeField] [Min(0)] private int maxAgents = 8;

        [SerializeField] private bool alignToGround = true;

        [SerializeField] private float spawnHeightOffset = 0f;

        [SerializeField] private uint randomSeed = 1337;

        private sealed class Baker : Baker<FaunaAmbientAuthoring>
        {
            public override void Bake(FaunaAmbientAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new FaunaAmbientVolume
                {
                    Profile = new UnityObjectRef<FaunaAmbientProfile> { Value = authoring.profile },
                    Radius = math.max(1f, authoring.radius),
                    SpawnIntervalSeconds = math.max(0.25f, authoring.spawnIntervalSeconds),
                    MaxAgents = math.max(0, authoring.maxAgents),
                    AlignToGround = (byte)(authoring.alignToGround ? 1 : 0),
                    SpawnHeightOffset = authoring.spawnHeightOffset
                });

                var seed = authoring.randomSeed == 0 ? (uint)UnityEngine.Random.Range(1, int.MaxValue) : authoring.randomSeed;
                AddComponent(entity, new FaunaAmbientVolumeRuntime
                {
                    NextSpawnTime = 0f,
                    ActiveAgents = 0,
                    RandomState = seed
                });

                AddBuffer<FaunaAmbientActiveAgent>(entity);
            }
        }
    }
}
