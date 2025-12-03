using Godgame.Modules;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Pure data authoring for a single module item (gear/upgrade) with durability and slot tag.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ModuleDefinitionAuthoring : MonoBehaviour
    {
        [SerializeField]
        private string moduleId = "module.default";

        [SerializeField]
        [Tooltip("Slot id (e.g., slot.hands.main, slot.torso.armor). See ModuleSlotIds.")]
        private string slotType = "slot.hands.main";

        [SerializeField]
        [Tooltip("Maximum condition/durability for this module.")]
        private float maxCondition = 100f;

        [SerializeField]
        [Tooltip("Initial condition at spawn.")]
        private float startingCondition = 100f;

        [SerializeField]
        [Tooltip("Passive degradation rate per second.")]
        private float degradationPerSecond = 0.1f;

        private sealed class Baker : Unity.Entities.Baker<ModuleDefinitionAuthoring>
        {
            public override void Bake(ModuleDefinitionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var maxCond = math.max(1f, authoring.maxCondition);
                var startCond = math.clamp(authoring.startingCondition, 0f, maxCond);

                AddComponent(entity, new ModuleData
                {
                    ModuleId = authoring.moduleId,
                    SlotType = authoring.slotType,
                    Status = ModuleStatus.Operational,
                    MaxCondition = maxCond,
                    Condition = startCond,
                    DegradationPerSecond = math.max(0f, authoring.degradationPerSecond),
                    LastServiceTick = 0
                });
            }
        }
    }
}
