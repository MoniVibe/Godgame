using Godgame.Villages;
using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class VillageDirectiveProfileAuthoring : MonoBehaviour
    {
        [Header("Directive Weights")]
        public float work = 1f;
        public float rest = 1f;
        public float social = 1f;
        public float faith = 1f;
        public float roam = 1f;

        [Header("Blend")]
        [Range(0f, 1f)]
        public float weight = 1f;

        [Header("Cadence Override")]
        public float minDurationSeconds = 0f;
        public float maxDurationSeconds = 0f;

        private sealed class Baker : Baker<VillageDirectiveProfileAuthoring>
        {
            public override void Bake(VillageDirectiveProfileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VillageDirectiveProfile
                {
                    Weights = new VillagerDirectiveWeights
                    {
                        Work = Mathf.Max(0f, authoring.work),
                        Rest = Mathf.Max(0f, authoring.rest),
                        Social = Mathf.Max(0f, authoring.social),
                        Faith = Mathf.Max(0f, authoring.faith),
                        Roam = Mathf.Max(0f, authoring.roam)
                    },
                    Weight = Mathf.Clamp01(authoring.weight),
                    MinDurationSeconds = Mathf.Max(0f, authoring.minDurationSeconds),
                    MaxDurationSeconds = Mathf.Max(0f, authoring.maxDurationSeconds)
                });
            }
        }
    }
}
