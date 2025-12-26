using Godgame.Relations;
using Godgame.Villagers;
using PureDOTS.Runtime.Perception;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for villager chatter tuning.
    /// </summary>
    public sealed class VillagerChatterTuningAuthoring : MonoBehaviour
    {
        [Header("Cadence")]
        [SerializeField, Range(1, 60)]
        private int cadenceTicks = 6;

        [SerializeField, Range(0.05f, 2f)]
        private float baseCooldownSeconds = 0.2f;

        [Header("Chance")]
        [SerializeField, Range(0f, 1f)]
        private float chatterChanceIdle = 0.03f;

        [SerializeField, Range(0f, 1f)]
        private float chatterChanceWorking = 0.015f;

        [Header("Perception")]
        [SerializeField, Range(0.1f, 50f)]
        private float maxDistance = 6f;

        [SerializeField, Range(0f, 1f)]
        private float minPerceptionConfidence = 0.15f;

        [SerializeField]
        private PerceptionChannel transportMask = PerceptionChannel.Hearing;

        [Header("Social Gate")]
        [SerializeField]
        private RelationTier minRelationTier = RelationTier.Neutral;

        [SerializeField, Range(1, 4)]
        private byte maxRecipients = 1;

        private sealed class Baker : Baker<VillagerChatterTuningAuthoring>
        {
            public override void Bake(VillagerChatterTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerChatterConfig
                {
                    BaseCooldownSeconds = Mathf.Max(0.01f, authoring.baseCooldownSeconds),
                    ChatterChanceIdle = Mathf.Clamp01(authoring.chatterChanceIdle),
                    ChatterChanceWorking = Mathf.Clamp01(authoring.chatterChanceWorking),
                    MaxDistance = Mathf.Max(0f, authoring.maxDistance),
                    MinPerceptionConfidence = Mathf.Clamp01(authoring.minPerceptionConfidence),
                    CadenceTicks = Mathf.Max(1, authoring.cadenceTicks),
                    MaxRecipients = (byte)Mathf.Clamp(authoring.maxRecipients, 1, 8),
                    MinRelationTier = authoring.minRelationTier,
                    TransportMask = authoring.transportMask
                });
            }
        }
    }
}
