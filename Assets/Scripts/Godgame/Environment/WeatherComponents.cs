using PureDOTS.Environment;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment
{
    public enum WeatherType : byte
    {
        Clear = 0,
        Rain = 1,
        Snow = 2,
        Fog = 3,
        Storm = 4
    }

    public enum TimeOfDayPhase : byte
    {
        Dawn = 0,
        Morning = 1,
        Afternoon = 2,
        Dusk = 3,
        Night = 4
    }

    public enum WeatherEventType : byte
    {
        PhaseChanged = 0,
        WeatherChanged = 1,
        WeatherIntensityChanged = 2,
        SpecialFxTriggered = 3
    }

    public enum WeatherRequestSource : byte
    {
        System = 0,
        TimeOfDay = 1,
        Biome = 2,
        Miracle = 3,
        Script = 4
    }

    [InternalBufferCapacity(8)]
    public struct WeatherRequest : IBufferElementData
    {
        public WeatherRequestSource Source;
        public WeatherType Weather;
        public float Intensity;
        public float DurationSeconds;
        public float3 Position;
        public FixedString64Bytes Payload;
    }

    [InternalBufferCapacity(8)]
    public struct WeatherEvent : IBufferElementData
    {
        public WeatherEventType EventType;
        public WeatherType Weather;
        public TimeOfDayPhase Phase;
        public WeatherRequestSource Source;
        public float Intensity;
        public uint Tick;
        public float3 Position;
        public FixedString64Bytes Payload;
    }

    public struct WeatherState : IComponentData
    {
        public WeatherType Current;
        public WeatherType Target;
        public float Intensity;
        public float MoisturePercent;
        public float TemperatureCelsius;
        public byte ActivePhase;
        public BiomeType DominantBiome;
        public float DominantBiomeMoisture;
        public uint LastChangeTick;

        public byte WeatherToken => (byte)Current;
    }

    public struct WeatherForecast : IComponentData
    {
        public WeatherType Next;
        public float TransitionSeconds;
        public float TimeRemainingSeconds;
    }

    public struct WeatherOverrideState : IComponentData
    {
        public byte IsActive;
        public WeatherRequestSource Source;
        public WeatherType Weather;
        public float Intensity;
        public float SecondsRemaining;
        public float3 LastRequestPosition;
        public FixedString64Bytes Payload;
    }

    public struct WeatherRequestQueueTag : IComponentData { }
}
