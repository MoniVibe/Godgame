using Godgame.Abilities;
using Godgame.Editor.PrefabTool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests.Abilities
{
    /// <summary>
    /// Tests that status specs have no cycles and periodic effects are valid.
    /// </summary>
    public class Statuses_NoCycles_PeriodicValid
    {
        [Test]
        public void StatusSpec_ValidPeriodic_ValidatesSuccessfully()
        {
            var catalog = ScriptableObject.CreateInstance<StatusSpecCatalog>();
            catalog.Statuses = new StatusSpecCatalog.StatusSpecDefinition[]
            {
                new StatusSpecCatalog.StatusSpecDefinition
                {
                    Id = "Burning",
                    MaxStacks = 5,
                    Dispellable = true,
                    DispelTags = DispelTags.Magic,
                    Duration = 10f,
                    Period = 1f // Tick every 1 second
                }
            };

            var result = StatusSpecValidator.ValidateCatalog(catalog);
            Assert.IsTrue(result.IsValid, "Valid periodic status should validate");
            Assert.AreEqual(0, result.Errors.Count);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void StatusSpec_PeriodGreaterThanDuration_DetectsInvalid()
        {
            var catalog = ScriptableObject.CreateInstance<StatusSpecCatalog>();
            catalog.Statuses = new StatusSpecCatalog.StatusSpecDefinition[]
            {
                new StatusSpecCatalog.StatusSpecDefinition
                {
                    Id = "InvalidStatus",
                    MaxStacks = 1,
                    Dispellable = true,
                    Duration = 5f,
                    Period = 10f // Period > Duration is invalid
                }
            };

            var result = StatusSpecValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Period > Duration should be invalid");
            Assert.Greater(result.Errors.Count, 0);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void StatusSpec_PeriodicWithoutDuration_DetectsInvalid()
        {
            var catalog = ScriptableObject.CreateInstance<StatusSpecCatalog>();
            catalog.Statuses = new StatusSpecCatalog.StatusSpecDefinition[]
            {
                new StatusSpecCatalog.StatusSpecDefinition
                {
                    Id = "InvalidStatus",
                    MaxStacks = 1,
                    Dispellable = true,
                    Duration = 0f, // No duration
                    Period = 1f // But has periodic tick
                }
            };

            var result = StatusSpecValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Periodic without duration should be invalid");
            Assert.Greater(result.Errors.Count, 0);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void StatusSpec_NegativeDuration_DetectsInvalid()
        {
            var catalog = ScriptableObject.CreateInstance<StatusSpecCatalog>();
            catalog.Statuses = new StatusSpecCatalog.StatusSpecDefinition[]
            {
                new StatusSpecCatalog.StatusSpecDefinition
                {
                    Id = "InvalidStatus",
                    MaxStacks = 1,
                    Dispellable = true,
                    Duration = -5f,
                    Period = 0f
                }
            };

            var result = StatusSpecValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Negative duration should be invalid");
            Assert.Greater(result.Errors.Count, 0);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void StatusSpec_DuplicateIds_DetectsDuplicates()
        {
            var catalog = ScriptableObject.CreateInstance<StatusSpecCatalog>();
            catalog.Statuses = new StatusSpecCatalog.StatusSpecDefinition[]
            {
                new StatusSpecCatalog.StatusSpecDefinition
                {
                    Id = "Duplicate",
                    MaxStacks = 1,
                    Dispellable = true,
                    Duration = 10f,
                    Period = 0f
                },
                new StatusSpecCatalog.StatusSpecDefinition
                {
                    Id = "Duplicate",
                    MaxStacks = 1,
                    Dispellable = true,
                    Duration = 10f,
                    Period = 0f
                }
            };

            var result = StatusSpecValidator.ValidateCatalog(catalog);
            Assert.IsFalse(result.IsValid, "Duplicate IDs should be invalid");
            Assert.Greater(result.Errors.Count, 0);

            Object.DestroyImmediate(catalog);
        }
    }
}

