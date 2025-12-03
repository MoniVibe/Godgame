using Godgame.Modules;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Skills;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Modules
{
    /// <summary>
    /// Applies explicit damage events to modules and queues refit/repair work as needed.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ModuleDegradationSystem))]
    public partial struct ModuleDamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ModuleDamageRequest>();
            state.RequireForUpdate<ModuleSlot>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var config = SystemAPI.TryGetSingleton<ModuleMaintenanceConfig>(out var cfg)
                ? cfg
                : ModuleMaintenanceDefaults.Create();

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (slots, damageBuffer, entity) in SystemAPI
                         .Query<DynamicBuffer<ModuleSlot>, DynamicBuffer<ModuleDamageRequest>>()
                         .WithEntityAccess())
            {
                if (damageBuffer.Length == 0)
                {
                    continue;
                }

                for (int i = damageBuffer.Length - 1; i >= 0; i--)
                {
                    var request = damageBuffer[i];
                    if (request.SlotIndex >= slots.Length)
                    {
                        damageBuffer.RemoveAt(i);
                        continue;
                    }

                    var slot = slots[request.SlotIndex];
                    if (slot.InstalledModule == Entity.Null || !state.EntityManager.HasComponent<ModuleData>(slot.InstalledModule))
                    {
                        damageBuffer.RemoveAt(i);
                        continue;
                    }

                    var module = state.EntityManager.GetComponentData<ModuleData>(slot.InstalledModule);
                    module.MaxCondition = math.max(module.MaxCondition, 1f);

                    var newCondition = math.max(0f, module.Condition - math.max(0f, request.Damage));
                    module.Condition = newCondition;

                    if (module.Condition <= module.MaxCondition * config.CriticalThreshold)
                    {
                        module.Status = ModuleStatus.Offline;
                    }
                    else
                    {
                        module.Status = ModuleStatus.Damaged;
                    }

                    if (module.Condition < module.MaxCondition * config.AutoRepairThreshold &&
                        !state.EntityManager.HasComponent<ModuleRefitRequest>(slot.InstalledModule))
                    {
                        var workRequired = math.max((module.MaxCondition - module.Condition) * config.WorkRequiredPerCondition, 0.01f);
                        ecb.AddComponent(slot.InstalledModule, new ModuleRefitRequest
                        {
                            WorkRemaining = workRequired,
                            TargetCondition = module.MaxCondition,
                            SkillId = SkillId.Processing,
                            AutoRequested = 1
                        });
                    }

                    state.EntityManager.SetComponentData(slot.InstalledModule, module);
                    damageBuffer.RemoveAt(i);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
