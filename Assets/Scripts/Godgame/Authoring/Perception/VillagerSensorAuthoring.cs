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

        [Header("Emissions")]
        [Tooltip("Smell emission strength (0-1)")]
        [Range(0f, 1f)]
        public float SmellEmission = 0.4f;

        [Tooltip("Sound emission strength (0-1)")]
        [Range(0f, 1f)]
        public float SoundEmission = 0.3f;

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

            var baseRange = math.max(authoring.VisionRange, math.max(authoring.HearingRange,
                math.max(authoring.SmellRange, authoring.ParanormalRange)));
            var rangeDenom = math.max(baseRange, 0.01f);

            // Add SenseCapability with Godgame channel mappings
            AddComponent(entity, new SenseCapability
            {
                EnabledChannels = PerceptionChannel.Vision | 
                                 PerceptionChannel.Hearing | 
                                 PerceptionChannel.Smell | 
                                 PerceptionChannel.Paranormal,
                Range = baseRange,
                FieldOfView = authoring.VisionFOV,
                Acuity = 1f,
                UpdateInterval = authoring.UpdateInterval,
                MaxTrackedTargets = authoring.MaxTrackedTargets,
                Flags = 0
            });

            var organs = AddBuffer<SenseOrganState>(entity);
            organs.Add(new SenseOrganState
            {
                OrganType = SenseOrganType.Eye,
                Channels = PerceptionChannel.Vision,
                Gain = 1f,
                Condition = authoring.VisionAcuity,
                NoiseFloor = 1f - authoring.VisionAcuity,
                RangeMultiplier = authoring.VisionRange / rangeDenom
            });
            organs.Add(new SenseOrganState
            {
                OrganType = SenseOrganType.Ear,
                Channels = PerceptionChannel.Hearing,
                Gain = 1f,
                Condition = authoring.HearingAcuity,
                NoiseFloor = 1f - authoring.HearingAcuity,
                RangeMultiplier = authoring.HearingRange / rangeDenom
            });
            organs.Add(new SenseOrganState
            {
                OrganType = SenseOrganType.Nose,
                Channels = PerceptionChannel.Smell,
                Gain = 1f,
                Condition = authoring.SmellAcuity,
                NoiseFloor = 1f - authoring.SmellAcuity,
                RangeMultiplier = authoring.SmellRange / rangeDenom
            });
            organs.Add(new SenseOrganState
            {
                OrganType = SenseOrganType.ParanormalSense,
                Channels = PerceptionChannel.Paranormal,
                Gain = 1f,
                Condition = authoring.ParanormalAcuity,
                NoiseFloor = 1f - authoring.ParanormalAcuity,
                RangeMultiplier = authoring.ParanormalRange / rangeDenom
            });

            var hasEmissions = authoring.SmellEmission > 0f || authoring.SoundEmission > 0f;
            AddComponent(entity, new SensorySignalEmitter
            {
                Channels = PerceptionChannel.Smell | PerceptionChannel.Hearing,
                SmellStrength = authoring.SmellEmission,
                SoundStrength = authoring.SoundEmission,
                EMStrength = 0f,
                IsActive = (byte)(hasEmissions ? 1 : 0)
            });

            // Add PerceptionState buffer
            AddBuffer<PerceivedEntity>(entity);

            // Add PerceptionState component
            AddComponent<PerceptionState>(entity);

            AddComponent<SignalPerceptionState>(entity);
        }
    }
}
