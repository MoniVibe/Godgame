using Godgame.Time;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Time
{
    /// <summary>
    /// Records determinism actor snapshots and restores them during rewind playback/catch-up.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(HistorySystemGroup))]
    public partial struct TimeDeterminismHistorySystem : ISystem
    {
        private TimeStreamHistory _history;
        private TimeAwareController _controller;
        private uint _lastRecordedTick;
        private uint _horizonTicks;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _history = new TimeStreamHistory(512, 256, Allocator.Persistent);
            _controller = new TimeAwareController(
                TimeAwareExecutionPhase.Record | TimeAwareExecutionPhase.CatchUp | TimeAwareExecutionPhase.Playback,
                TimeAwareExecutionOptions.SkipWhenPaused);
            _lastRecordedTick = uint.MaxValue;
            _horizonTicks = 0;

            state.RequireForUpdate<TimeDeterminismTag>();
            state.RequireForUpdate<TimeDeterminismState>();
            state.RequireForUpdate<TimeDeterminismConfig>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _history.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            var horizon = GetHorizonTicks(timeState);

            if (!_controller.TryBegin(timeState, rewindState, out var context))
            {
                return;
            }

            if (context.IsRecordPhase)
            {
                if (context.Time.Tick == _lastRecordedTick)
                {
                    return;
                }

                _lastRecordedTick = context.Time.Tick;
                uint minTick = context.Time.Tick > horizon ? context.Time.Tick - horizon : 0;
                _history.PruneOlderThan(minTick);

                var recordIndex = _history.BeginRecord(context.Time.Tick, out var writer);
                Save(ref state, ref writer);
                _history.EndRecord(recordIndex);
            }
            else if (context.IsCatchUpPhase || context.IsPlaybackPhase)
            {
                uint targetTick = context.IsPlaybackPhase ? context.Rewind.PlaybackTick : context.Time.Tick;
                if (!_history.TryGet(targetTick, out var bytes))
                {
                    return;
                }

                var reader = new TimeStreamReader(bytes);
                Load(ref state, ref reader);
            }

            if (context.ModeChangedThisFrame && context.PreviousMode == RewindMode.Playback && context.IsRecordPhase)
            {
                _lastRecordedTick = uint.MaxValue;
            }
        }

        private void Save(ref SystemState state, ref TimeStreamWriter writer)
        {
            var records = new NativeList<TimeDeterminismSnapshot>(Allocator.Temp);
            foreach (var (determinismState, transform, entity) in SystemAPI
                         .Query<RefRO<TimeDeterminismState>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                records.Add(new TimeDeterminismSnapshot
                {
                    Entity = entity,
                    State = determinismState.ValueRO,
                    Transform = transform.ValueRO
                });
            }

            writer.Write(records.Length);
            for (int i = 0; i < records.Length; i++)
            {
                writer.Write(records[i]);
            }

            records.Dispose();
        }

        private void Load(ref SystemState state, ref TimeStreamReader reader)
        {
            var count = reader.Read<int>();
            for (int i = 0; i < count; i++)
            {
                var snapshot = reader.Read<TimeDeterminismSnapshot>();
                if (!SystemAPI.Exists(snapshot.Entity))
                {
                    continue;
                }

                if (SystemAPI.HasComponent<TimeDeterminismState>(snapshot.Entity))
                {
                    SystemAPI.SetComponent(snapshot.Entity, snapshot.State);
                }

                if (SystemAPI.HasComponent<LocalTransform>(snapshot.Entity))
                {
                    SystemAPI.SetComponent(snapshot.Entity, snapshot.Transform);
                }
            }
        }

        private uint GetHorizonTicks(in TimeState timeState)
        {
            if (_horizonTicks != 0)
            {
                return _horizonTicks;
            }

            float ticksPerSecond = 1f / math.max(0.0001f, timeState.FixedDeltaTime);
            uint desired = (uint)math.max(1f, math.round(ticksPerSecond * 3f));

            if (SystemAPI.TryGetSingleton<HistorySettings>(out var settings))
            {
                desired = math.max(desired, (uint)math.round(settings.DefaultTicksPerSecond * 3f));
            }

            _horizonTicks = desired + 4u;
            _history.SetMaxRecords((int)math.max(_horizonTicks + 4u, 8u));
            return _horizonTicks;
        }

        private struct TimeDeterminismSnapshot
        {
            public Entity Entity;
            public TimeDeterminismState State;
            public LocalTransform Transform;
        }
    }
}
