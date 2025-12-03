using Godgame.Abilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// MonoBehaviour wrapper to bake SkillSpecCatalog ScriptableObjects into blob assets.
    /// </summary>
    public sealed class SkillSpecCatalogAuthoring : MonoBehaviour
    {
        public SkillSpecCatalog Catalog;
    }

    public sealed class SkillSpecCatalogAuthoringBaker : Baker<SkillSpecCatalogAuthoring>
    {
        public override void Bake(SkillSpecCatalogAuthoring authoring)
        {
            var catalog = authoring.Catalog;
            if (catalog == null || catalog.Skills == null || catalog.Skills.Length == 0)
            {
                return;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SkillSpecCatalogBlob>();

            var skillsArray = builder.Allocate(ref root.Skills, catalog.Skills.Length);

            for (int i = 0; i < catalog.Skills.Length; i++)
            {
                var skillDef = catalog.Skills[i];

                var grantsArray = builder.Allocate(ref skillsArray[i].GrantsSpellIds, skillDef.GrantsSpellIds?.Length ?? 0);
                if (skillDef.GrantsSpellIds != null)
                {
                    for (int j = 0; j < skillDef.GrantsSpellIds.Length; j++)
                    {
                        grantsArray[j] = new FixedString64Bytes(skillDef.GrantsSpellIds[j] ?? "");
                    }
                }

                var requiresArray = builder.Allocate(ref skillsArray[i].Requires, skillDef.Requires?.Length ?? 0);
                if (skillDef.Requires != null)
                {
                    for (int j = 0; j < skillDef.Requires.Length; j++)
                    {
                        requiresArray[j] = new FixedString64Bytes(skillDef.Requires[j] ?? "");
                    }
                }

                skillsArray[i] = new SkillSpecBlob
                {
                    Id = new FixedString64Bytes(skillDef.Id ?? $"Skill_{i}"),
                    Passive = skillDef.Passive ? (byte)1 : (byte)0,
                    StatDeltaPower = skillDef.StatDeltaPower,
                    StatDeltaSpeed = skillDef.StatDeltaSpeed,
                    StatDeltaDefense = skillDef.StatDeltaDefense,
                    Tier = skillDef.Tier,
                    Tags = (uint)skillDef.Tags
                };
            }

            var blobAsset = builder.CreateBlobAssetReference<SkillSpecCatalogBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SkillSpecCatalogBlobRef { Catalog = blobAsset });
        }
    }
}
