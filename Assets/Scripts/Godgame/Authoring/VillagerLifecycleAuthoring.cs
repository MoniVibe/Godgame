using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring overrides for villager lifecycle and reproduction state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerLifecycleAuthoring : MonoBehaviour
    {
        [Header("Lifecycle")]
        [Min(0f)] public float ageDays = 20f;
        public VillagerLifeStage stage = VillagerLifeStage.Adult;

        [Header("Reproduction")]
        public VillagerSex sex = VillagerSex.None;
        [Range(0f, 1f)] public float fertility = 0.6f;
        public bool isPregnant = false;
        [Min(0f)] public float pregnancyDays = 0f;

        private sealed class Baker : Baker<VillagerLifecycleAuthoring>
        {
            public override void Bake(VillagerLifecycleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new VillagerLifecycleState
                {
                    AgeDays = Mathf.Max(0f, authoring.ageDays),
                    Stage = authoring.stage,
                    LastUpdateTick = 0u
                });

                AddComponent(entity, new VillagerReproductionState
                {
                    Sex = authoring.sex,
                    Fertility = Mathf.Clamp01(authoring.fertility),
                    IsPregnant = (byte)(authoring.isPregnant ? 1 : 0),
                    PregnancyDays = Mathf.Max(0f, authoring.pregnancyDays),
                    ConceptionTick = 0u,
                    NextConceptionTick = 0u,
                    Partner = Entity.Null,
                    BirthCount = 0,
                    PendingBirths = 0
                });
            }
        }
    }
}
