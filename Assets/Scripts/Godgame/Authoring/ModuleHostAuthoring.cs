using System;
using Godgame.Modules;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for entities that own module slots and maintenance config.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ModuleHostAuthoring : MonoBehaviour
    {
        [Serializable]
        public struct Slot
        {
            public string SlotType;
            public ModuleDefinitionAuthoring InstalledModule;
        }

        [SerializeField] private Slot[] slots;
        [SerializeField] private GameObject[] maintainerCrew;

        [Header("Maintenance Config (leave blank for defaults)")]
        [SerializeField] private bool overrideMaintenanceConfig;
        [SerializeField] private float baseWorkRate = 6f;
        [SerializeField] private float skillRateBonus = 0.2f;
        [SerializeField] private float workRequiredPerCondition = 0.05f;
        [SerializeField] private float autoRepairThreshold = 0.55f;
        [SerializeField] private float criticalThreshold = 0.2f;

        private sealed class Baker : Baker<ModuleHostAuthoring>
        {
            public override void Bake(ModuleHostAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);

                var config = authoring.overrideMaintenanceConfig
                    ? new ModuleMaintenanceConfig
                    {
                        BaseWorkRate = Mathf.Max(0f, authoring.baseWorkRate),
                        SkillRateBonus = Mathf.Max(0f, authoring.skillRateBonus),
                        WorkRequiredPerCondition = Mathf.Max(0.0001f, authoring.workRequiredPerCondition),
                        AutoRepairThreshold = Mathf.Clamp01(authoring.autoRepairThreshold),
                        CriticalThreshold = Mathf.Clamp01(authoring.criticalThreshold)
                    }
                    : ModuleMaintenanceDefaults.Create();
                AddComponent(entity, config);

                var slotBuffer = AddBuffer<ModuleSlot>(entity);
                if (authoring.slots != null)
                {
                    for (byte i = 0; i < authoring.slots.Length; i++)
                    {
                        var slotDef = authoring.slots[i];
                        var moduleEntity = slotDef.InstalledModule != null
                            ? GetEntity(slotDef.InstalledModule, TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable)
                            : Entity.Null;

                        slotBuffer.Add(new ModuleSlot
                        {
                            SlotIndex = i,
                            SlotType = new Unity.Collections.FixedString64Bytes(slotDef.SlotType ?? string.Empty),
                            InstalledModule = moduleEntity
                        });

                        if (moduleEntity != Entity.Null)
                        {
                            AddComponent(moduleEntity, new ModuleHostReference { Host = entity });
                        }
                    }
                }

                if (authoring.maintainerCrew != null && authoring.maintainerCrew.Length > 0)
                {
                    var crewBuffer = AddBuffer<ModuleMaintainerLink>(entity);
                    foreach (var crew in authoring.maintainerCrew)
                    {
                        if (crew == null)
                        {
                            continue;
                        }

                        var crewEntity = GetEntity(crew, TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);
                        crewBuffer.Add(new ModuleMaintainerLink { Worker = crewEntity });
                    }
                }
            }
        }
    }
}
