using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Updates moisture grid from climate state.
    /// MVP: Copies ClimateState.Moisture01 into a 1×1 grid.
    /// Future: Expand to spatial grid with propagation.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct MoistureGridSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClimateState>();
            state.RequireForUpdate<MoistureGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var climate = SystemAPI.GetSingleton<ClimateState>();
            var moistureGrid = SystemAPI.GetSingletonRW<MoistureGrid>();

            // MVP: For 1×1 grid, just mirror the climate moisture
            // Future: When grid expands, update all cells based on climate + local factors
            if (moistureGrid.ValueRO.Width == 1 && moistureGrid.ValueRO.Height == 1)
            {
                // Update the single cell value
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobArray<float>>();
                var moistureArray = builder.Allocate(ref root, 1);
                moistureArray[0] = math.clamp(climate.Moisture01, 0f, 1f);
                var newBlob = builder.CreateBlobAssetReference<BlobArray<float>>(Allocator.Persistent);
                builder.Dispose();

                // Replace the old blob (old one will be garbage collected)
                moistureGrid.ValueRW.Values = newBlob;
            }
        }
    }
}

