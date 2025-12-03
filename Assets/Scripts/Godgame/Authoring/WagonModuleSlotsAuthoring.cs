using Godgame.Modules;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Seeds wagon/vehicle entities with module slot buffers for mounts, cargo, and decor.
    /// Pure data only; does not assign modules.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WagonModuleSlotsAuthoring : MonoBehaviour
    {
        private sealed class Baker : Unity.Entities.Baker<WagonModuleSlotsAuthoring>
        {
            public override void Bake(WagonModuleSlotsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var slots = AddBuffer<ModuleSlot>(entity);
                ModuleSlotIds.AddWagonSlots(slots);
            }
        }
    }
}
