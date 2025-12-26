using Godgame.Relations;
using PureDOTS.Runtime.Perception;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    public enum VillagerChatterKind : byte
    {
        Greeting = 0,
        Idle = 1,
        Work = 2
    }

    /// <summary>
    /// Tracks mouth/speech availability for a villager.
    /// </summary>
    public struct VillagerSpeechChannel : IComponentData
    {
        public uint NextAvailableTick;
        public uint LastSpeechTick;
        public uint LastMessageId;
        public Entity LastListener;
    }

    /// <summary>
    /// Chatter tuning and thresholds.
    /// </summary>
    public struct VillagerChatterConfig : IComponentData
    {
        public float BaseCooldownSeconds;
        public float ChatterChanceIdle;
        public float ChatterChanceWorking;
        public float MaxDistance;
        public float MinPerceptionConfidence;
        public int CadenceTicks;
        public byte MaxRecipients;
        public RelationTier MinRelationTier;
        public PerceptionChannel TransportMask;

        public static VillagerChatterConfig Default => new VillagerChatterConfig
        {
            BaseCooldownSeconds = 0.2f,
            ChatterChanceIdle = 0.03f,
            ChatterChanceWorking = 0.015f,
            MaxDistance = 6f,
            MinPerceptionConfidence = 0.15f,
            CadenceTicks = 6,
            MaxRecipients = 1,
            MinRelationTier = RelationTier.Neutral,
            TransportMask = PerceptionChannel.Hearing
        };
    }

    /// <summary>
    /// Singleton tag for chatter event buffer.
    /// </summary>
    public struct VillagerChatterEventBuffer : IComponentData { }

    [InternalBufferCapacity(16)]
    public struct VillagerChatterEvent : IBufferElementData
    {
        public Entity Speaker;
        public Entity Listener;
        public VillagerChatterKind Kind;
        public uint Tick;
        public uint MessageId;
    }
}
