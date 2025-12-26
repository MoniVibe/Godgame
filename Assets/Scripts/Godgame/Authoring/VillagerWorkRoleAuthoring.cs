using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring override for per-villager work role and hauling preference.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerWorkRoleAuthoring : MonoBehaviour
    {
        public VillagerWorkRoleKind role = VillagerWorkRoleKind.Forester;

        [Header("Haul Override")]
        public bool overrideHaulPreference = false;
        [Range(0f, 1f)] public float haulChance = 0.25f;
        [Range(0.1f, 60f)] public float haulCooldownSeconds = 6f;
        public bool forceHaul = false;

        private sealed class Baker : Baker<VillagerWorkRoleAuthoring>
        {
            public override void Bake(VillagerWorkRoleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new VillagerWorkRoleOverride
                {
                    Value = authoring.role
                });

                AddComponent(entity, new VillagerWorkRole
                {
                    Value = authoring.role
                });

                if (authoring.overrideHaulPreference)
                {
                    AddComponent(entity, new VillagerHaulPreference
                    {
                        HaulChance = Mathf.Clamp01(authoring.haulChance),
                        HaulCooldownSeconds = Mathf.Max(0.1f, authoring.haulCooldownSeconds),
                        NextHaulAllowedTick = 0u,
                        ForceHaul = (byte)(authoring.forceHaul ? 1 : 0)
                    });
                    AddComponent<VillagerHaulPreferenceOverride>(entity);
                }
            }
        }
    }
}
