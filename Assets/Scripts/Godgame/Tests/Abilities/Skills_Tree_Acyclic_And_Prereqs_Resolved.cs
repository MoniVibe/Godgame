using Godgame.Abilities;
using Godgame.Editor.PrefabTool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests.Abilities
{
    /// <summary>
    /// Tests that skill trees are acyclic and prerequisites are resolved.
    /// </summary>
    public class Skills_Tree_Acyclic_And_Prereqs_Resolved
    {
        [Test]
        public void SkillTree_ValidTree_ValidatesSuccessfully()
        {
            var catalog = ScriptableObject.CreateInstance<SkillSpecCatalog>();
            catalog.Skills = new SkillSpecCatalog.SkillSpecDefinition[]
            {
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "Skill1",
                    Passive = false,
                    Requires = new string[0],
                    Tier = 1
                },
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "Skill2",
                    Passive = false,
                    Requires = new[] { "Skill1" },
                    Tier = 2
                },
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "Skill3",
                    Passive = false,
                    Requires = new[] { "Skill2" },
                    Tier = 3
                }
            };

            var result = SkillTreeValidator.ValidateCatalog(catalog);
            Assert.IsTrue(result.IsValid, "Valid linear tree should validate");
            Assert.AreEqual(0, result.Errors.Count);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SkillTree_CyclicDependency_DetectsCycle()
        {
            var catalog = ScriptableObject.CreateInstance<SkillSpecCatalog>();
            catalog.Skills = new SkillSpecCatalog.SkillSpecDefinition[]
            {
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "SkillA",
                    Passive = false,
                    Requires = new[] { "SkillB" },
                    Tier = 1
                },
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "SkillB",
                    Passive = false,
                    Requires = new[] { "SkillA" },
                    Tier = 1
                }
            };

            var result = SkillTreeValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Cyclic dependency should be invalid");
            Assert.Greater(result.Errors.Count, 0, "Should have cycle detection error");

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SkillTree_MissingPrerequisite_DetectsMissing()
        {
            var catalog = ScriptableObject.CreateInstance<SkillSpecCatalog>();
            catalog.Skills = new SkillSpecCatalog.SkillSpecDefinition[]
            {
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "Skill1",
                    Passive = false,
                    Requires = new[] { "NonExistentSkill" },
                    Tier = 1
                }
            };

            var result = SkillTreeValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Missing prerequisite should be invalid");
            Assert.Greater(result.Errors.Count, 0, "Should have missing prerequisite error");

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SkillTree_DuplicateIds_DetectsDuplicates()
        {
            var catalog = ScriptableObject.CreateInstance<SkillSpecCatalog>();
            catalog.Skills = new SkillSpecCatalog.SkillSpecDefinition[]
            {
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "Duplicate",
                    Passive = false,
                    Requires = new string[0],
                    Tier = 1
                },
                new SkillSpecCatalog.SkillSpecDefinition
                {
                    Id = "Duplicate",
                    Passive = false,
                    Requires = new string[0],
                    Tier = 1
                }
            };

            var result = SkillTreeValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Duplicate IDs should be invalid");
            Assert.Greater(result.Errors.Count, 0, "Should have duplicate ID error");

            Object.DestroyImmediate(catalog);
        }
    }
}

