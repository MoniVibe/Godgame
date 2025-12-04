using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Godgame.Input;

namespace Godgame.Miracles.Presentation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MiraclePresentationSystem))]
    public partial struct MiracleDesignerTriggerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleDesignerTriggerSource>();
            state.RequireForUpdate<DebugInput>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Check if miracle designer toggle was pressed
            var debugInput = SystemAPI.GetSingleton<DebugInput>();
            if (debugInput.ToggleMiracleDesigner == 0)
            {
                return;
            }

            // Process all trigger sources when toggle is pressed
            foreach (var (source, triggerBuffer, transform) in SystemAPI
                         .Query<RefRO<MiracleDesignerTriggerSource>, DynamicBuffer<MiracleDesignerTrigger>, RefRO<LocalToWorld>>())
            {
                FixedString64Bytes descriptor = default;
                var profile = source.ValueRO.Profile.Value;
                if (profile != null && !string.IsNullOrWhiteSpace(profile.BaseDescriptor))
                {
                    descriptor.CopyFromTruncated(profile.BaseDescriptor.Trim());
                }

                var spawnPosition = transform.ValueRO.Position + source.ValueRO.Offset;
                triggerBuffer.Add(new MiracleDesignerTrigger
                {
                    DescriptorKey = descriptor,
                    Position = spawnPosition,
                    Type = source.ValueRO.Type
                });
            }
        }
    }
}
