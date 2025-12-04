using Godgame.Environment;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Fauna
{
    /// <summary>
    /// Defines a spawn volume for ambient fauna. Placed via authoring MonoBehaviour.
    /// </summary>
    public struct FaunaAmbientVolume : IComponentData
    {
        public UnityObjectRef<FaunaAmbientProfile> Profile;
        public float Radius;
        public float SpawnIntervalSeconds;
        public int MaxAgents;
        public float SpawnHeightOffset;
        public byte AlignToGround;
    }

    public struct FaunaAmbientVolumeRuntime : IComponentData
    {
        public float NextSpawnTime;
        public int ActiveAgents;
        public uint RandomState;
    }

    [InternalBufferCapacity(8)]
    public struct FaunaAmbientActiveAgent : IBufferElementData
    {
        public Entity Agent;
    }

    /// <summary>
    /// Runtime state for each ambient creature entity.
    /// </summary>
    public struct FaunaAmbientAgent : IComponentData
    {
        public Entity Volume;
        public int RuleIndex;
        public float3 HomePosition;
        public float3 TargetPosition;
        public float IdleTimer;
        public float MoveSpeed;
        public float WanderRadius;
        public TimeOfDayWindowData ActivityWindow;
        public uint RandomState;
        public float AmbientCooldown;
        public byte Flags;
        public byte BehaviourState;
        public FixedString64Bytes BaseDescriptor;
        public FixedString64Bytes MiracleDescriptor;
    }

    public struct FaunaAmbientSound : IComponentData
    {
        public UnityObjectRef<AudioClip> Clip;
        public float IntervalSeconds;
        public float Cooldown;
    }

    public static class FaunaAmbientFlags
    {
        public const byte None = 0;
        public const byte Nocturnal = 1 << 0;
        public const byte ExternalController = 1 << 1;
    }

    public static class FaunaAmbientBehaviour
    {
        public const byte Dormant = 0;
        public const byte Idle = 1;
        public const byte Moving = 2;
    }
}
