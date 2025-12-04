using PureDOTS.Runtime.Components;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Miracles
{
    [DisallowMultipleComponent]
    public sealed class GodgameMiracleSlotAuthoring : MonoBehaviour
    {
        [System.Serializable]
        public struct SlotEntry
        {
            public byte slotIndex;
            public GameObject miraclePrefab;
            public MiracleType type;
            public GameObject configSource;
        }

        public SlotEntry[] slots;

        private class Baker : Baker<GodgameMiracleSlotAuthoring>
        {
            public override void Bake(GodgameMiracleSlotAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<MiracleSlotDefinition>(entity);
                if (authoring.slots == null)
                {
                    return;
                }

                foreach (var slot in authoring.slots)
                {
                    if (slot.miraclePrefab == null)
                    {
                        continue;
                    }

                    var prefabEntity = GetEntity(slot.miraclePrefab, TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);
                    Entity configEntity = Entity.Null;
                    if (slot.configSource != null)
                    {
                        configEntity = GetEntity(slot.configSource, TransformUsageFlags.None);
                    }

                    buffer.Add(new MiracleSlotDefinition
                    {
                        SlotIndex = slot.slotIndex,
                        MiraclePrefab = prefabEntity,
                        Type = slot.type,
                        ConfigEntity = configEntity
                    });
                }
            }
        }
    }
}
