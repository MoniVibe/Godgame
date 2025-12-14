using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
            ref var moistureGrid = ref SystemAPI.GetSingletonRW<MoistureGrid>().ValueRW;

            // MVP: For 1×1 grid, mirror the climate moisture.
            if (moistureGrid.Width != 1 || moistureGrid.Height != 1)
            {
                return;
            }

            var newValue = math.clamp(climate.Moisture01, 0f, 1f);
            if (moistureGrid.Values.IsCreated &&
                moistureGrid.Values.Value.Length == 1 &&
                math.abs(moistureGrid.Values.Value[0] - newValue) <= 1e-4f)
            {
                return; // nothing changed
            }

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobArray<float>>();
            var moistureArray = builder.Allocate(ref root, 1);
            moistureArray[0] = newValue;
            var newBlob = builder.CreateBlobAssetReference<BlobArray<float>>(Allocator.Persistent);

            if (moistureGrid.Values.IsCreated)
            {
                moistureGrid.Values.Dispose();
            }

            moistureGrid.Values = newBlob;
        }
    }
}

