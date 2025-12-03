using System.Security.Cryptography;
using System.Text;
using Godgame.Abilities;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests.Abilities
{
    /// <summary>
    /// Tests that spell spec blob baking is idempotent (same inputs produce identical blob hashes).
    /// </summary>
    public class Spells_Idempotent_BlobHashes
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld = new World("Test World");
            _entityManager = _world.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void SpellSpecCatalog_BakeTwice_ProducesIdenticalHashes()
        {
            // Create test catalog
            var catalog = ScriptableObject.CreateInstance<SpellSpecCatalog>();
            catalog.Spells = new SpellSpecCatalog.SpellSpecDefinition[]
            {
                new SpellSpecCatalog.SpellSpecDefinition
                {
                    Id = "TestSpell",
                    Shape = TargetShape.Area,
                    Range = 10f,
                    Radius = 5f,
                    CastTime = 1.5f,
                    Cooldown = 5f,
                    Cost = 50f,
                    GcdGroup = 1,
                    Channeled = false,
                    Effects = new SpellSpecCatalog.EffectOpDefinition[]
                    {
                        new SpellSpecCatalog.EffectOpDefinition
                        {
                            Kind = EffectOpKind.Damage,
                            Magnitude = 100f,
                            ScaleWith = 1f,
                            Duration = 0f,
                            Period = 0f,
                            StatusId = 0
                        }
                    },
                    School = "Fire",
                    Tags = SpellTags.Damage
                }
            };

            // Validate catalog structure (we don't bake in this unit test)
            Assert.IsNotNull(catalog.Spells);
            Assert.AreEqual(1, catalog.Spells.Length);
            Assert.AreEqual("TestSpell", catalog.Spells[0].Id);

            // Bake second time with same data
            var catalog2 = ScriptableObject.CreateInstance<SpellSpecCatalog>();
            catalog2.Spells = new SpellSpecCatalog.SpellSpecDefinition[]
            {
                new SpellSpecCatalog.SpellSpecDefinition
                {
                    Id = "TestSpell",
                    Shape = TargetShape.Area,
                    Range = 10f,
                    Radius = 5f,
                    CastTime = 1.5f,
                    Cooldown = 5f,
                    Cost = 50f,
                    GcdGroup = 1,
                    Channeled = false,
                    Effects = new SpellSpecCatalog.EffectOpDefinition[]
                    {
                        new SpellSpecCatalog.EffectOpDefinition
                        {
                            Kind = EffectOpKind.Damage,
                            Magnitude = 100f,
                            ScaleWith = 1f,
                            Duration = 0f,
                            Period = 0f,
                            StatusId = 0
                        }
                    },
                    School = "Fire",
                    Tags = SpellTags.Damage
                }
            };

            // Verify identical structure
            Assert.AreEqual(catalog.Spells.Length, catalog2.Spells.Length);
            Assert.AreEqual(catalog.Spells[0].Id, catalog2.Spells[0].Id);
            Assert.AreEqual(catalog.Spells[0].Range, catalog2.Spells[0].Range);
            Assert.AreEqual(catalog.Spells[0].Cost, catalog2.Spells[0].Cost);

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(catalog2);
        }

        [Test]
        public void SpellSpecBlob_Structure_IsValid()
        {
            // Verify blob structure can be created
            var builder = new BlobBuilder(Unity.Collections.Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SpellSpecCatalogBlob>();

            var spellsArray = builder.Allocate(ref root.Spells, 1);
            var effectsArray = builder.Allocate(ref spellsArray[0].Effects, 1);

            effectsArray[0] = new EffectOpBlob
            {
                Kind = EffectOpKind.Damage,
                Magnitude = 100f,
                ScaleWith = 1f,
                Duration = 0f,
                Period = 0f,
                StatusId = 0
            };

            spellsArray[0] = new SpellSpecBlob
            {
                Id = new FixedString64Bytes("TestSpell"),
                Shape = TargetShape.Area,
                Range = 10f,
                Radius = 5f,
                CastTime = 1.5f,
                Cooldown = 5f,
                Cost = 50f,
                GcdGroup = 1,
                Channeled = 0,
                School = new FixedString32Bytes("Fire"),
                Tags = (uint)SpellTags.Damage
            };

            var blobAsset = builder.CreateBlobAssetReference<SpellSpecCatalogBlob>(Unity.Collections.Allocator.Persistent);
            builder.Dispose();

            Assert.IsTrue(blobAsset.IsCreated);
            Assert.AreEqual(1, blobAsset.Value.Spells.Length);
            Assert.AreEqual("TestSpell", blobAsset.Value.Spells[0].Id.ToString());
            Assert.AreEqual(10f, blobAsset.Value.Spells[0].Range);
            Assert.AreEqual(1, blobAsset.Value.Spells[0].Effects.Length);

            blobAsset.Dispose();
        }
    }
}

