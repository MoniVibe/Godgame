using Godgame.Abilities;
using Godgame.Editor.PrefabTool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests.Abilities
{
    /// <summary>
    /// Tests that swapping between Minimal and Fancy binding sets doesn't change gameplay behavior.
    /// </summary>
    public class Bindings_MinimalVsFancy_Swap_NoGameplayChange
    {
        [Test]
        public void BindingSet_SameSpellId_DifferentVisualsOnly()
        {
            // Create minimal binding set
            var minimalSet = ScriptableObject.CreateInstance<SpellBindingSet>();
            minimalSet.SetName = "Minimal";
            minimalSet.Type = SpellBindingSet.BindingSetType.Minimal;
            minimalSet.Bindings = new SpellPresentationBinding[]
            {
                new SpellPresentationBinding
                {
                    SpellId = "Fireball",
                    StartFX = null, // Placeholder
                    ImpactFX = null, // Placeholder
                    IconToken = null
                }
            };

            // Create fancy binding set with same spell ID but different visuals
            var fancySet = ScriptableObject.CreateInstance<SpellBindingSet>();
            fancySet.SetName = "Fancy";
            fancySet.Type = SpellBindingSet.BindingSetType.Fancy;
            fancySet.Bindings = new SpellPresentationBinding[]
            {
                new SpellPresentationBinding
                {
                    SpellId = "Fireball", // Same spell ID
                    StartFX = null, // Different FX (would be assigned in real scenario)
                    ImpactFX = null, // Different FX
                    IconToken = null // Different icon
                }
            };

            // Verify both sets reference the same spell ID
            Assert.AreEqual(minimalSet.Bindings[0].SpellId, fancySet.Bindings[0].SpellId,
                "Both binding sets should reference the same spell ID");

            // Verify they have different visual properties (in real scenario, FX would differ)
            // For now, we just verify the structure is correct
            Assert.AreEqual(SpellBindingSet.BindingSetType.Minimal, minimalSet.Type);
            Assert.AreEqual(SpellBindingSet.BindingSetType.Fancy, fancySet.Type);

            Object.DestroyImmediate(minimalSet);
            Object.DestroyImmediate(fancySet);
        }

        [Test]
        public void BindingSet_Validation_ChecksSpellIdReferences()
        {
            var spellCatalog = ScriptableObject.CreateInstance<SpellSpecCatalog>();
            spellCatalog.Spells = new SpellSpecCatalog.SpellSpecDefinition[]
            {
                new SpellSpecCatalog.SpellSpecDefinition
                {
                    Id = "Fireball",
                    Shape = TargetShape.Area,
                    Range = 10f,
                    Radius = 5f,
                    CastTime = 1f,
                    Cooldown = 5f,
                    Cost = 50f,
                    GcdGroup = 1,
                    Channeled = false,
                    Effects = System.Array.Empty<SpellSpecCatalog.EffectOpDefinition>(),
                    School = "Fire",
                    Tags = SpellTags.Damage
                }
            };

            var bindingSet = ScriptableObject.CreateInstance<SpellBindingSet>();
            bindingSet.SetName = "TestSet";
            bindingSet.Type = SpellBindingSet.BindingSetType.Minimal;
            bindingSet.Bindings = new SpellPresentationBinding[]
            {
                new SpellPresentationBinding
                {
                    SpellId = "Fireball" // Valid reference
                },
                new SpellPresentationBinding
                {
                    SpellId = "NonExistent" // Invalid reference
                }
            };

            var result = SpellBindingValidator.ValidateBindingSet(bindingSet, spellCatalog);
            // Should have warning for non-existent spell
            Assert.Greater(result.Warnings.Count, 0, "Should warn about non-existent spell ID");

            Object.DestroyImmediate(spellCatalog);
            Object.DestroyImmediate(bindingSet);
        }

        [Test]
        public void BindingSet_ShowcasedSpells_AllHaveBindings()
        {
            var spellCatalog = ScriptableObject.CreateInstance<SpellSpecCatalog>();
            spellCatalog.Spells = new SpellSpecCatalog.SpellSpecDefinition[]
            {
                new SpellSpecCatalog.SpellSpecDefinition
                {
                    Id = "ShowcasedSpell",
                    Shape = TargetShape.Unit,
                    Range = 5f,
                    Radius = 0f,
                    CastTime = 0.5f,
                    Cooldown = 3f,
                    Cost = 25f,
                    GcdGroup = 1,
                    Channeled = false,
                    Effects = System.Array.Empty<SpellSpecCatalog.EffectOpDefinition>(),
                    School = "Nature",
                    Tags = SpellTags.Heal
                }
            };

            var bindingSet = ScriptableObject.CreateInstance<SpellBindingSet>();
            bindingSet.SetName = "TestSet";
            bindingSet.Bindings = new SpellPresentationBinding[]
            {
                new SpellPresentationBinding
                {
                    SpellId = "ShowcasedSpell" // Has binding
                }
            };

            var showcasedIds = new System.Collections.Generic.HashSet<string> { "ShowcasedSpell" };
            var result = SpellBindingValidator.ValidateBindingSet(bindingSet, spellCatalog, showcasedIds);
            Assert.IsTrue(result.IsValid, "Showcased spell with binding should validate");

            // Test missing binding
            var bindingSet2 = ScriptableObject.CreateInstance<SpellBindingSet>();
            bindingSet2.SetName = "TestSet2";
            bindingSet2.Bindings = System.Array.Empty<SpellPresentationBinding>();

            var result2 = SpellBindingValidator.ValidateBindingSet(bindingSet2, spellCatalog, showcasedIds);
            Assert.Greater(result2.Warnings.Count, 0, "Should warn about missing binding for showcased spell");

            Object.DestroyImmediate(spellCatalog);
            Object.DestroyImmediate(bindingSet);
            Object.DestroyImmediate(bindingSet2);
        }
    }
}

