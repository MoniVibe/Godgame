using Godgame.Abilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// MonoBehaviour wrapper to bake SpellSpecCatalog ScriptableObjects into blob assets.
    /// </summary>
    public sealed class SpellSpecCatalogAuthoring : MonoBehaviour
    {
        public SpellSpecCatalog Catalog;
    }

    public sealed class SpellSpecCatalogAuthoringBaker : Baker<SpellSpecCatalogAuthoring>
    {
        public override void Bake(SpellSpecCatalogAuthoring authoring)
        {
            var catalog = authoring.Catalog;
            if (catalog == null || catalog.Spells == null || catalog.Spells.Length == 0)
            {
                return;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SpellSpecCatalogBlob>();

            var spellsArray = builder.Allocate(ref root.Spells, catalog.Spells.Length);

            for (int i = 0; i < catalog.Spells.Length; i++)
            {
                var spellDef = catalog.Spells[i];
                var effectsArray = builder.Allocate(ref spellsArray[i].Effects, spellDef.Effects?.Length ?? 0);

                if (spellDef.Effects != null)
                {
                    for (int j = 0; j < spellDef.Effects.Length; j++)
                    {
                        var effectDef = spellDef.Effects[j];
                        effectsArray[j] = new EffectOpBlob
                        {
                            Kind = effectDef.Kind,
                            Magnitude = effectDef.Magnitude,
                            ScaleWith = effectDef.ScaleWith,
                            Duration = effectDef.Duration,
                            Period = effectDef.Period,
                            StatusId = effectDef.StatusId
                        };
                    }
                }

                spellsArray[i] = new SpellSpecBlob
                {
                    Id = new FixedString64Bytes(spellDef.Id ?? $"Spell_{i}"),
                    Shape = spellDef.Shape,
                    Range = spellDef.Range,
                    Radius = spellDef.Radius,
                    CastTime = spellDef.CastTime,
                    Cooldown = spellDef.Cooldown,
                    Cost = spellDef.Cost,
                    GcdGroup = spellDef.GcdGroup,
                    Channeled = spellDef.Channeled ? (byte)1 : (byte)0,
                    School = new FixedString32Bytes(spellDef.School ?? ""),
                    Tags = (uint)spellDef.Tags
                };
            }

            var blobAsset = builder.CreateBlobAssetReference<SpellSpecCatalogBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SpellSpecCatalogBlobRef { Catalog = blobAsset });
        }
    }
}
