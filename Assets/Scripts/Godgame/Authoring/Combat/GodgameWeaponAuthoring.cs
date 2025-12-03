using PureDOTS.Runtime.Combat;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring.Combat
{
    /// <summary>
    /// Authoring component for Godgame weapons (melee, ranged).
    /// Bakes Weapon buffer entries for entities.
    /// </summary>
    public class GodgameWeaponAuthoring : MonoBehaviour
    {
        [System.Serializable]
        public class WeaponData
        {
            [Tooltip("Weapon name")]
            public string Name = "Weapon";

            [Tooltip("Attack range")]
            public float Range = 5f;

            [Tooltip("Attacks per second")]
            public float FireRate = 1f;

            [Tooltip("Base damage")]
            public float BaseDamage = 10f;

            [Tooltip("Damage type")]
            public DamageType DamageType = DamageType.Physical;

            [Tooltip("Fire arc (degrees, 0 = no constraint)")]
            public float FireArcDegrees = 0f;

            [Tooltip("Projectile type (empty = melee/hitscan)")]
            public string ProjectileType = "";
        }

        [Tooltip("Weapons on this entity")]
        public WeaponData[] Weapons = new WeaponData[1] { new WeaponData() };
    }

    /// <summary>
    /// Baker for GodgameWeaponAuthoring.
    /// </summary>
    public class GodgameWeaponBaker : Baker<GodgameWeaponAuthoring>
    {
        public override void Bake(GodgameWeaponAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            if (authoring.Weapons == null || authoring.Weapons.Length == 0)
            {
                return;
            }

            // Add Weapon buffer
            var weaponBuffer = AddBuffer<WeaponComponent>(entity);

            for (byte i = 0; i < authoring.Weapons.Length && i < 4; i++) // Max 4 weapons
            {
                var weaponData = authoring.Weapons[i];
                weaponBuffer.Add(new WeaponComponent
                {
                    Range = weaponData.Range,
                    FireRate = weaponData.FireRate,
                    BaseDamage = weaponData.BaseDamage,
                    DamageType = weaponData.DamageType,
                    ProjectileType = new Unity.Collections.FixedString32Bytes(weaponData.ProjectileType),
                    FireArcDegrees = weaponData.FireArcDegrees,
                    LastFireTime = 0f,
                    CooldownRemaining = 0f,
                    WeaponIndex = i
                });
            }

            // Add FireEvent buffer
            AddBuffer<FireEvent>(entity);
        }
    }
}

