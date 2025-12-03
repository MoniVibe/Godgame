using Godgame.Modules;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Seeds building entities with material/decor module slot buffers.
    /// Pure data only; modules are assigned at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BuildingModuleSlotsAuthoring : MonoBehaviour
    {
        private sealed class Baker : Unity.Entities.Baker<BuildingModuleSlotsAuthoring>
        {
            public override void Bake(BuildingModuleSlotsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var slots = AddBuffer<ModuleSlot>(entity);
                ModuleSlotIds.AddBuildingSlots(slots);
            }
        }
    }
}
