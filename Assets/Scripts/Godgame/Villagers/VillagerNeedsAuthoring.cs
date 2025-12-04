using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Villagers
{
    /// <summary>
    /// Authoring component for villager needs and interrupt systems.
    /// Add to villager prefabs to enable needs-based utility scheduling and interrupt handling.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerNeedsAuthoring : MonoBehaviour
    {
        [Header("Health")]
        [Range(1f, 200f)] public float maxHealth = 100f;
        [Range(0f, 200f)] public float startingHealth = 100f;

        [Header("Energy")]
        [Range(0f, 1000f)] public float startingEnergy = 800f;
        [Tooltip("Energy decay rate when idle (per second)")]
        [Range(0f, 50f)] public float idleEnergyDecay = 5f;
        [Tooltip("Energy decay rate when working (per second)")]
        [Range(0f, 100f)] public float workEnergyDecay = 25f;

        [Header("Morale")]
        [Range(0f, 1000f)] public float startingMorale = 700f;
        [Tooltip("Morale decay rate (per second)")]
        [Range(0f, 20f)] public float moraleDecay = 2f;

        private sealed class VillagerNeedsBaker : Baker<VillagerNeedsAuthoring>
        {
            public override void Bake(VillagerNeedsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add VillagerNeeds component
                AddComponent(entity, new VillagerNeeds
                {
                    Health = math.min(authoring.startingHealth, authoring.maxHealth),
                    MaxHealth = authoring.maxHealth,
                    Energy = authoring.startingEnergy,
                    Morale = authoring.startingMorale
                });

                // Add interrupt components (default to no interrupt)
                AddComponent(entity, new VillagerInterrupt
                {
                    Type = VillagerInterruptType.None,
                    SourceEntity = Entity.Null,
                    StartTick = 0,
                    Duration = 0f
                });

                // Note: VillagerInterruptState is only added when needed (by system)
            }
        }
    }
}

