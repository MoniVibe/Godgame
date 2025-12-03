using System;
using PureDOTS.Runtime.Skills;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Modules
{
    /// <summary>
    /// Aggregates maintainer skills into host SkillSet and stamps host refs on installed modules.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ModuleRefitSystem))]
    public partial struct ModuleMaintainerAggregationSystem : ISystem
    {
        private ComponentLookup<SkillSet> _skillLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ModuleSlot>();
            _skillLookup = state.GetComponentLookup<SkillSet>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _skillLookup.Update(ref state);
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (slots, entity) in SystemAPI.Query<DynamicBuffer<ModuleSlot>>().WithEntityAccess())
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    var module = slots[i].InstalledModule;
                    if (module != Entity.Null && !state.EntityManager.HasComponent<ModuleHostReference>(module))
                    {
                        ecb.AddComponent(module, new ModuleHostReference { Host = entity });
                    }
                }

                if (!state.EntityManager.HasBuffer<ModuleMaintainerLink>(entity))
                {
                    continue;
                }

                var maintainerLinks = state.EntityManager.GetBuffer<ModuleMaintainerLink>(entity);
                if (maintainerLinks.Length == 0)
                {
                    continue;
                }

                const int MaxSkillIds = 8;
                Span<int> totals = stackalloc int[MaxSkillIds];
                Span<int> counts = stackalloc int[MaxSkillIds];

                for (int i = 0; i < maintainerLinks.Length; i++)
                {
                    var worker = maintainerLinks[i].Worker;
                    if (worker == Entity.Null || !_skillLookup.HasComponent(worker))
                    {
                        continue;
                    }

                    var skillSet = _skillLookup[worker];
                    for (int entry = 0; entry < skillSet.Entries.Length; entry++)
                    {
                        var id = (int)skillSet.Entries[entry].Id;
                        if (id <= 0 || id >= MaxSkillIds)
                        {
                            continue;
                        }

                        totals[id] += skillSet.Entries[entry].Level;
                        counts[id]++;
                    }
                }

                var aggregated = _skillLookup.HasComponent(entity) ? _skillLookup[entity] : new SkillSet();
                for (int id = 1; id < MaxSkillIds; id++)
                {
                    if (counts[id] == 0)
                    {
                        continue;
                    }

                    var average = (byte)math.clamp(math.round((float)totals[id] / counts[id]), 0f, 255f);
                    aggregated.SetLevel((SkillId)id, average);
                }

                if (_skillLookup.HasComponent(entity))
                {
                    SystemAPI.SetComponent(entity, aggregated);
                }
                else
                {
                    state.EntityManager.AddComponentData(entity, aggregated);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
