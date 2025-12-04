using Unity.Entities;

namespace Godgame.Fauna
{
    /// <summary>
    /// Keeps runtime counts in sync and prunes buffer entries for despawned agents.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FaunaAmbientSoundSystem))]
    public partial struct FaunaAmbientMaintenanceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FaunaAmbientVolume>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            foreach (var (runtimeRW, agentBuffer) in SystemAPI.Query<RefRW<FaunaAmbientVolumeRuntime>, DynamicBuffer<FaunaAmbientActiveAgent>>())
            {
                int aliveCount = 0;
                for (int i = agentBuffer.Length - 1; i >= 0; i--)
                {
                    var agentEntity = agentBuffer[i].Agent;
                    if (!entityManager.Exists(agentEntity))
                    {
                        agentBuffer.RemoveAt(i);
                        continue;
                    }

                    aliveCount++;
                }

                runtimeRW.ValueRW.ActiveAgents = aliveCount;
            }
        }
    }
}
