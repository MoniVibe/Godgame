using PureDOTS.Runtime.Perception;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring.Perception
{
    /// <summary>
    /// Authoring component for villager sensors.
    /// Maps Godgame concepts (Vision/Hearing/Smell/Paranormal) to PureDOTS Perception channels.
    /// </summary>
    public class VillagerSensorAuthoring : MonoBehaviour
    {
        [Header("Vision")]
        [Tooltip("Vision detection range")]
        public float VisionRange = 50f;

        [Tooltip("Vision field of view (degrees)")]
        public float VisionFOV = 120f;

        [Tooltip("Vision acuity (0-1)")]
        [Range(0f, 1f)]
        public float VisionAcuity = 1f;

        [Header("Hearing")]
        [Tooltip("Hearing detection range")]
        public float HearingRange = 30f;

        [Tooltip("Hearing acuity (0-1)")]
        [Range(0f, 1f)]
        public float HearingAcuity = 0.8f;

        [Header("Smell")]
        [Tooltip("Smell detection range")]
        public float SmellRange = 10f;

        [Tooltip("Smell acuity (0-1)")]
        [Range(0f, 1f)]
        public float SmellAcuity = 0.6f;

        [Header("Paranormal")]
        [Tooltip("Paranormal detection range (for miracles, divine presence)")]
        public float ParanormalRange = 100f;

        [Tooltip("Paranormal acuity (0-1)")]
        [Range(0f, 1f)]
        public float ParanormalAcuity = 0.5f;

        [Header("Settings")]
        [Tooltip("Sensor update interval (seconds)")]
        public float UpdateInterval = 0.5f;

        [Tooltip("Maximum entities to track")]
        public byte MaxTrackedTargets = 8;
    }

    /// <summary>
    /// Baker for VillagerSensorAuthoring.
    /// </summary>
    public class VillagerSensorBaker : Baker<VillagerSensorAuthoring>
    {
        public override void Bake(VillagerSensorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add SenseCapability with Godgame channel mappings
            AddComponent(entity, new SenseCapability
            {
                EnabledChannels = PerceptionChannel.Vision | 
                                 PerceptionChannel.Hearing | 
                                 PerceptionChannel.Smell | 
                                 PerceptionChannel.Paranormal,
                Range = math.max(authoring.VisionRange, math.max(authoring.HearingRange, 
                         math.max(authoring.SmellRange, authoring.ParanormalRange))),
                FieldOfView = authoring.VisionFOV,
                Acuity = 1f, // Use average or max - Phase 1: simple
                UpdateInterval = authoring.UpdateInterval,
                MaxTrackedTargets = authoring.MaxTrackedTargets,
                Flags = 0
            });

            // Add PerceptionState buffer
            AddBuffer<PerceivedEntity>(entity);

            // Add PerceptionState component
            AddComponent<PerceptionState>(entity);
        }
    }
}

