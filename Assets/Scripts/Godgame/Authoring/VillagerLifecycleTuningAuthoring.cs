using Godgame.Relations;
using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring tunables for villager lifecycle and breeding behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerLifecycleTuningAuthoring : MonoBehaviour
    {
        [Header("Day Length")]
        [Min(10f)] public float secondsPerDay = 480f; // 8 minutes per day (docs default)

        [Header("Life Stages (days)")]
        [Min(0f)] public float childStageDays = 10f;
        [Min(0f)] public float youthStageDays = 20f;
        [Min(0f)] public float adultStageDays = 60f;
        [Min(0f)] public float elderStageDays = 90f;

        [Header("Fertility")]
        [Min(0f)] public float fertilityStartDays = 18f;
        [Min(0f)] public float fertilityEndDays = 70f;

        [Header("Pregnancy")]
        [Min(0.1f)] public float pregnancyDays = 3f;
        [Min(0.1f)] public float breedingCooldownDays = 1f;
        [Range(0f, 1f)] public float breedingChancePerDay = 0.15f;
        [Min(0.1f)] public float breedingDistance = 3f;
        public RelationTier minRelationTier = RelationTier.Friendly;

        [Header("Cadence")]
        [Range(1, 600)] public int lifecycleCadenceTicks = 30;
        [Range(1, 600)] public int breedingCadenceTicks = 30;

        private sealed class Baker : Baker<VillagerLifecycleTuningAuthoring>
        {
            public override void Bake(VillagerLifecycleTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerLifecycleTuning
                {
                    SecondsPerDay = Mathf.Max(10f, authoring.secondsPerDay),
                    ChildStageDays = Mathf.Max(0f, authoring.childStageDays),
                    YouthStageDays = Mathf.Max(0f, authoring.youthStageDays),
                    AdultStageDays = Mathf.Max(0f, authoring.adultStageDays),
                    ElderStageDays = Mathf.Max(0f, authoring.elderStageDays),
                    FertilityStartDays = Mathf.Max(0f, authoring.fertilityStartDays),
                    FertilityEndDays = Mathf.Max(authoring.fertilityStartDays, authoring.fertilityEndDays),
                    PregnancyDays = Mathf.Max(0.1f, authoring.pregnancyDays),
                    BreedingCooldownDays = Mathf.Max(0.1f, authoring.breedingCooldownDays),
                    BreedingChancePerDay = Mathf.Clamp01(authoring.breedingChancePerDay),
                    BreedingDistance = Mathf.Max(0.1f, authoring.breedingDistance),
                    LifecycleCadenceTicks = Mathf.Max(1, authoring.lifecycleCadenceTicks),
                    BreedingCadenceTicks = Mathf.Max(1, authoring.breedingCadenceTicks),
                    MinRelationTier = authoring.minRelationTier
                });
            }
        }
    }
}
