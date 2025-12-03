using Godgame.Adapters.Launch;
using PureDOTS.Runtime.Launch;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Godgame-specific slingshot/launcher authoring.
    /// Extends the base launcher with Godgame-specific settings.
    /// </summary>
    public class GodgameSlingshotAuthoring : MonoBehaviour
    {
        [Header("Queue Settings")]
        [Tooltip("Maximum number of pending launches in queue")]
        [Range(1, 32)]
        public int MaxQueueSize = 4;

        [Tooltip("Minimum ticks between launches (cooldown)")]
        [Range(0, 120)]
        public int CooldownTicks = 30; // ~0.5s at 60 ticks/sec

        [Header("Launch Settings")]
        [Tooltip("Default launch speed for thrown objects")]
        public float DefaultSpeed = 15f;

        [Header("Godgame Settings")]
        [Tooltip("Maximum range for targeting")]
        public float MaxRange = 50f;

        [Tooltip("Arc height multiplier for parabolic throws")]
        public float ArcHeightMultiplier = 0.3f;

        public class Baker : Baker<GodgameSlingshotAuthoring>
        {
            public override void Bake(GodgameSlingshotAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add launcher tag
                AddComponent<LauncherTag>(entity);

                // Add base launcher config
                AddComponent(entity, new LauncherConfig
                {
                    MaxQueueSize = (byte)Mathf.Clamp(authoring.MaxQueueSize, 1, 32),
                    CooldownTicks = (uint)authoring.CooldownTicks,
                    DefaultSpeed = authoring.DefaultSpeed
                });

                // Add runtime state
                AddComponent(entity, new LauncherState
                {
                    LastLaunchTick = 0,
                    QueueCount = 0,
                    Version = 0
                });

                // Add buffers
                AddBuffer<LaunchRequest>(entity);
                AddBuffer<LaunchQueueEntry>(entity);

                // Add Godgame-specific tag and config
                AddComponent<GodgameSlingshotTag>(entity);
                AddComponent(entity, new GodgameSlingshotConfig
                {
                    MaxRange = authoring.MaxRange,
                    ArcHeightMultiplier = authoring.ArcHeightMultiplier
                });
            }
        }
    }
}

