using PureDOTS.Runtime.Components;
using Unity.Entities;
using MiracleType = PureDOTS.Runtime.Components.MiracleType;

namespace Godgame.Miracles
{
    /// <summary>
    /// Ensures each hand has miracle slot definitions populated with default configs.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameMiracleSlotBootstrapSystem : ISystem
    {
        private bool _initialized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleCasterState>();
            state.RequireForUpdate<MiracleSlotDefinition>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_initialized)
            {
                state.Enabled = false;
                return;
            }

            foreach (var (slotBuffer, casterState) in SystemAPI.Query<DynamicBuffer<MiracleSlotDefinition>, RefRO<MiracleCasterState>>())
            {
                if (slotBuffer.Length > 0)
                {
                    continue;
                }

                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, new RainMiracleConfig
                {
                    RainCloudPrefab = Entity.Null,
                    CloudCount = 4,
                    SpawnRadius = 12f,
                    SpawnHeightOffset = 18f,
                    SpawnSpreadAngle = 120f,
                    Seed = 1
                });

                slotBuffer.Add(new MiracleSlotDefinition
                {
                    SlotIndex = 0,
                    Type = MiracleType.None, // TODO: Replace Rain with valid PureDOTS miracle type
                    MiraclePrefab = Entity.Null,
                    ConfigEntity = configEntity
                });
            }

            _initialized = true;
            state.Enabled = false;
        }
    }
}
