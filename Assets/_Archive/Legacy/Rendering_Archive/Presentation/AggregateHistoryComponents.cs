#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// Rolling buffer entry for aggregate history sampling.
    /// Stores a snapshot of key aggregate stats at a specific tick.
    /// </summary>
    public struct AggregateHistory : IBufferElementData
    {
        /// <summary>Tick when this sample was taken</summary>
        public uint Tick;
        /// <summary>Population (for villages)</summary>
        public int Population;
        /// <summary>Wealth (for villages, total resource value)</summary>
        public int Wealth;
        /// <summary>Food amount (for villages)</summary>
        public int Food;
        /// <summary>Vegetation health (for regions, 0-1)</summary>
        public float VegetationHealth;
        /// <summary>Fertility (for regions, 0-100)</summary>
        public float Fertility;
    }

    /// <summary>
    /// Component attached to village entities for tracking history.
    /// Contains a rolling buffer of aggregate samples.
    /// </summary>
    public struct VillageHistory : IComponentData
    {
        /// <summary>Fixed-size buffer (60 samples = ~1 minute at 30 TPS)</summary>
        public DynamicBuffer<AggregateHistory> History;
    }

    /// <summary>
    /// Component attached to region/biome entities for tracking history.
    /// Contains a rolling buffer of aggregate samples.
    /// </summary>
    public struct RegionHistory : IComponentData
    {
        /// <summary>Fixed-size buffer (60 samples = ~1 minute at 30 TPS)</summary>
        public DynamicBuffer<AggregateHistory> History;
    }

    /// <summary>
    /// Configuration for aggregate history sampling.
    /// </summary>
    public struct AggregateHistoryConfig : IComponentData
    {
        /// <summary>Sample interval in ticks (default: 30 ticks = 1 second at 30 TPS)</summary>
        public uint SampleIntervalTicks;
        /// <summary>Maximum samples to keep per entity (default: 60)</summary>
        public int MaxSamples;
    }
}
#endif
