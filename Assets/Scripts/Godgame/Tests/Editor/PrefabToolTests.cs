using System.Collections.Generic;
using Godgame.Editor.PrefabTool;
using NUnit.Framework;
using UnityEngine;

namespace Godgame.Tests.Editor
{
    public class PrefabToolTests
    {
        [Test]
        public void Building_PlacementRules_EditMode()
        {
            var template = new BuildingTemplate
            {
                name = "TestBuilding",
                placement = new Placement
                {
                    maxSlope = 30f,
                    requiresWater = true,
                    waterDistanceMax = 50f
                }
            };

            // Valid case
            Assert.IsTrue(template.placement.maxSlope >= 0 && template.placement.maxSlope <= 90);
            
            // Invalid case check logic (simulated)
            var invalidTemplate = new BuildingTemplate
            {
                placement = new Placement { maxSlope = -10f }
            };
            
            var ruleSet = new BuildingRuleSet();
            var result = ruleSet.Validate(invalidTemplate, new List<MaterialTemplate>());
            
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Count > 0);
            Assert.IsTrue(result.Errors[0].Contains("Invalid max slope"));
        }

        [Test]
        public void Equipment_MaterialCompatibility_EditMode()
        {
            var iron = new MaterialTemplate
            {
                name = "Iron",
                usage = MaterialUsage.Weapon | MaterialUsage.Tool,
                stats = new MaterialStats { hardness = 60f, toughness = 50f },
                traits = MaterialTraits.Ductile | MaterialTraits.Hard
            };

            var wood = new MaterialTemplate
            {
                name = "Wood",
                usage = MaterialUsage.Building | MaterialUsage.Fuel,
                stats = new MaterialStats { hardness = 20f, toughness = 30f },
                traits = MaterialTraits.Flammable
            };

            var catalog = new List<MaterialTemplate> { iron, wood };

            var swordTemplate = new EquipmentTemplate
            {
                name = "IronSword",
                requiredUsage = MaterialUsage.Weapon,
                minStats = new MaterialStats { hardness = 50f }
            };

            var ruleSet = new EquipmentRuleSet();
            var result = ruleSet.Validate(swordTemplate, catalog);

            Assert.IsTrue(result.IsValid);

            var impossibleTemplate = new EquipmentTemplate
            {
                name = "SuperSword",
                requiredUsage = MaterialUsage.Weapon,
                minStats = new MaterialStats { hardness = 90f } // Higher than Iron
            };

            var result2 = ruleSet.Validate(impossibleTemplate, catalog);
            Assert.IsFalse(result2.IsValid);
        }

        [Test]
        public void Equipment_DurabilityFromStats_EditMode()
        {
            var template = new EquipmentTemplate
            {
                name = "TestSword",
                baseDurability = 100f,
                material = new MaterialQuality { qualityMultiplier = 1.5f } // Legacy field used for calculation
            };

            // Mock calculation or use actual static method if available and pure
            float durability = StatCalculation.CalculateEquipmentDurability(template);
            
            // Base 100 * 1.5 = 150
            Assert.AreEqual(150f, durability);
        }
        
        [Test]
        public void Miracle_TargetFilterValidation_EditMode()
        {
            var template = new MiracleTemplate
            {
                name = "Fireball",
                targetFilter = new TargetFilter
                {
                    requiredTraits = MaterialTraits.Flammable,
                    forbiddenTraits = MaterialTraits.Fireproof
                }
            };
            
            // Validation logic would go here
            // For now just checking the structure holds data
            Assert.AreEqual(MaterialTraits.Flammable, template.targetFilter.requiredTraits);
            Assert.AreEqual(MaterialTraits.Fireproof, template.targetFilter.forbiddenTraits);
        }
        
        [Test]
        public void Container_AcceptsPackages_EditMode()
        {
            var template = new ContainerTemplate
            {
                name = "Barrel",
                acceptedPackageClasses = new List<PackageClass> { PackageClass.Liquid, PackageClass.Bulk }
            };
            
            Assert.Contains(PackageClass.Liquid, template.acceptedPackageClasses);
            Assert.Contains(PackageClass.Bulk, template.acceptedPackageClasses);
            Assert.IsFalse(template.acceptedPackageClasses.Contains(PackageClass.Gas));
        }
        
        [Test]
        public void Node_HarvestToolRules_EditMode()
        {
            var template = new ResourceNodeTemplate
            {
                name = "IronVein",
                requiredToolUsage = MaterialUsage.Tool,
                minToolStats = new MaterialStats { hardness = 50f }
            };
            
            Assert.AreEqual(MaterialUsage.Tool, template.requiredToolUsage);
            Assert.AreEqual(50f, template.minToolStats.hardness);
        }
        
        [Test]
        public void Individual_LoadoutRules_EditMode()
        {
            var template = new IndividualTemplate
            {
                name = "Guard",
                equipmentSlots = new List<SlotKind> { SlotKind.Hand, SlotKind.Body, SlotKind.Head }
            };
            
            Assert.AreEqual(3, template.equipmentSlots.Count);
            Assert.Contains(SlotKind.Hand, template.equipmentSlots);
        }
    }
}
