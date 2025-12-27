using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class VillagerDirectiveConfigAuthoring : MonoBehaviour
    {
        [Header("Directive Cadence")]
        public float minDurationSeconds = 20f;
        public float maxDurationSeconds = 60f;
        [Range(0f, 1f)]
        public float chaosWeight = 0.6f;
        [Range(0f, 1f)]
        public float orderWeight = 0.6f;

        [Header("Base Weights")]
        public float baseWork = 1.15f;
        public float baseRest = 1f;
        public float baseSocial = 1f;
        public float baseFaith = 0.75f;
        public float baseRoam = 0.6f;

        [Header("Work Bias")]
        public VillagerDirectiveBias workBias = VillagerDirectiveConfig.Default.WorkBias;

        [Header("Rest Bias")]
        public VillagerDirectiveBias restBias = VillagerDirectiveConfig.Default.RestBias;

        [Header("Social Bias")]
        public VillagerDirectiveBias socialBias = VillagerDirectiveConfig.Default.SocialBias;

        [Header("Faith Bias")]
        public VillagerDirectiveBias faithBias = VillagerDirectiveConfig.Default.FaithBias;

        [Header("Roam Bias")]
        public VillagerDirectiveBias roamBias = VillagerDirectiveConfig.Default.RoamBias;

        [Header("Idle Bias")]
        public VillagerDirectiveBias idleBias = VillagerDirectiveBias.Default;

        private sealed class Baker : Baker<VillagerDirectiveConfigAuthoring>
        {
            public override void Bake(VillagerDirectiveConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerDirectiveConfig
                {
                    BaseWeights = new VillagerDirectiveWeights
                    {
                        Work = Mathf.Max(0f, authoring.baseWork),
                        Rest = Mathf.Max(0f, authoring.baseRest),
                        Social = Mathf.Max(0f, authoring.baseSocial),
                        Faith = Mathf.Max(0f, authoring.baseFaith),
                        Roam = Mathf.Max(0f, authoring.baseRoam)
                    },
                    MinDurationSeconds = Mathf.Max(1f, authoring.minDurationSeconds),
                    MaxDurationSeconds = Mathf.Max(authoring.minDurationSeconds, authoring.maxDurationSeconds),
                    ChaosWeight = Mathf.Clamp01(authoring.chaosWeight),
                    OrderWeight = Mathf.Clamp01(authoring.orderWeight),
                    WorkBias = authoring.workBias,
                    RestBias = authoring.restBias,
                    SocialBias = authoring.socialBias,
                    FaithBias = authoring.faithBias,
                    RoamBias = authoring.roamBias,
                    IdleBias = authoring.idleBias
                });
            }
        }
    }
}
