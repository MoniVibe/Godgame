using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Migrates legacy prefab templates to the latest schema version.
    /// Ensures forward compatibility when schema fields are added or changed.
    /// </summary>
    public static class PrefabTemplateMigrator
    {
        /// <summary>
        /// Current schema version. Increment when making breaking changes.
        /// </summary>
        public const int CurrentSchemaVersion = 2;

        /// <summary>
        /// Migrates a template to the current schema version.
        /// </summary>
        public static bool MigrateTemplate(PrefabTemplate template, int fromVersion = 0)
        {
            if (template == null) return false;

            bool migrated = false;

            // Ensure base fields exist (schema v1)
            if (fromVersion < 1)
            {
                migrated |= MigrateToV1(template);
            }

            // Ensure quality/rarity/tech tier fields exist (schema v2)
            if (fromVersion < 2)
            {
                migrated |= MigrateToV2(template);
            }

            return migrated;
        }

        /// <summary>
        /// Migrates to schema version 1: Ensures visual presentation and bonuses exist.
        /// </summary>
        private static bool MigrateToV1(PrefabTemplate template)
        {
            bool migrated = false;

            if (template.visualPresentation == null)
            {
                template.visualPresentation = new VisualPresentation();
                migrated = true;
            }

            if (template.vfxPresentation == null)
            {
                template.vfxPresentation = new VFXPresentation();
                migrated = true;
            }

            if (template.bonuses == null)
            {
                template.bonuses = new List<PrefabBonus>();
                migrated = true;
            }

            return migrated;
        }

        /// <summary>
        /// Migrates to schema version 2: Ensures quality/rarity/tech tier fields exist.
        /// </summary>
        private static bool MigrateToV2(PrefabTemplate template)
        {
            bool migrated = false;

            // Quality defaults to 50 if not set
            if (template.quality == 0 && template.calculatedQuality == 0)
            {
                // Try to infer from legacy fields
                switch (template)
                {
                    case MaterialTemplate material:
                        template.quality = material.baseQuality > 0 ? material.baseQuality : 50f;
                        break;
                    case ToolTemplate tool:
                        template.quality = tool.baseQuality > 0 ? tool.baseQuality : 50f;
                        break;
                    default:
                        template.quality = 50f;
                        break;
                }
                migrated = true;
            }

            // Rarity defaults to Common if not set
            if (template.rarity == 0) // Rarity.Common = 0, but we check if it was never initialized
            {
                template.rarity = Rarity.Common;
                migrated = true;
            }

            // Tech tier defaults to 0
            if (template.techTier == 0 && template.requiredTechTier == 0)
            {
                // Already defaults, but mark as migrated if we set quality/rarity
                if (migrated) { }
            }

            // Migrate type-specific fields
            switch (template)
            {
                case MaterialTemplate material:
                    migrated |= MigrateMaterialTemplate(material);
                    break;
                case EquipmentTemplate equipment:
                    migrated |= MigrateEquipmentTemplate(equipment);
                    break;
                case ToolTemplate tool:
                    migrated |= MigrateToolTemplate(tool);
                    break;
            }

            return migrated;
        }

        /// <summary>
        /// Migrates MaterialTemplate-specific fields.
        /// </summary>
        private static bool MigrateMaterialTemplate(MaterialTemplate template)
        {
            bool migrated = false;

            // Ensure materialId is initialized
            if (template.materialId.Value == 0 && template.id > 0)
            {
                template.materialId = new MaterialId((ushort)Mathf.Min(template.id, ushort.MaxValue));
                migrated = true;
            }

            // Ensure stats exist
            if (template.stats == null)
            {
                template.stats = new MaterialStats();
                migrated = true;
            }

            // Ensure possibleAttributes exists
            if (template.possibleAttributes == null)
            {
                template.possibleAttributes = new List<MaterialAttribute>();
                migrated = true;
            }

            return migrated;
        }

        /// <summary>
        /// Migrates EquipmentTemplate-specific fields.
        /// </summary>
        private static bool MigrateEquipmentTemplate(EquipmentTemplate template)
        {
            bool migrated = false;

            // Ensure itemDefId is initialized
            if (template.itemDefId.Value == 0 && template.id > 0)
            {
                template.itemDefId = new ItemDefId((ushort)Mathf.Min(template.id, ushort.MaxValue));
                migrated = true;
            }

            // Ensure stats exist
            if (template.stats == null)
            {
                template.stats = new EquipmentStats();
                migrated = true;
            }

            // Ensure minStats exist
            if (template.minStats == null)
            {
                template.minStats = new MaterialStats();
                migrated = true;
            }

            return migrated;
        }

        /// <summary>
        /// Migrates ToolTemplate-specific fields.
        /// </summary>
        private static bool MigrateToolTemplate(ToolTemplate template)
        {
            bool migrated = false;

            // Ensure itemDefId and recipeId are initialized
            if (template.itemDefId.Value == 0 && template.id > 0)
            {
                template.itemDefId = new ItemDefId((ushort)Mathf.Min(template.id, ushort.MaxValue));
                migrated = true;
            }

            if (template.recipeId.Value == 0 && template.id > 0)
            {
                template.recipeId = new RecipeId((ushort)Mathf.Min(template.id, ushort.MaxValue));
                migrated = true;
            }

            // Ensure productionInputs exists
            if (template.productionInputs == null)
            {
                template.productionInputs = new List<ProductionInput>();
                migrated = true;
            }

            // Ensure qualityDerivation exists
            if (template.qualityDerivation == null)
            {
                template.qualityDerivation = new QualityDerivation();
                migrated = true;
            }

            // Ensure possibleAttributes exists
            if (template.possibleAttributes == null)
            {
                template.possibleAttributes = new List<MaterialAttribute>();
                migrated = true;
            }

            return migrated;
        }

        /// <summary>
        /// Migrates all templates in a collection.
        /// </summary>
        public static int MigrateTemplates<T>(List<T> templates) where T : PrefabTemplate
        {
            int migratedCount = 0;
            foreach (var template in templates)
            {
                if (MigrateTemplate(template))
                {
                    migratedCount++;
                }
            }
            return migratedCount;
        }

        /// <summary>
        /// Migrates all templates in the PrefabEditorWindow.
        /// </summary>
        [MenuItem("Godgame/Prefab Tool/Migrate All Templates")]
        public static void MigrateAllTemplates()
        {
            var window = EditorWindow.GetWindow<PrefabEditorWindow>();
            if (window == null)
            {
                EditorUtility.DisplayDialog("Migration", "Please open the Prefab Editor window first.", "OK");
                return;
            }

            int totalMigrated = 0;

            // Use reflection to access private template lists
            var windowType = typeof(PrefabEditorWindow);
            var templateLists = new[]
            {
                windowType.GetField("buildingTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                windowType.GetField("individualTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                windowType.GetField("equipmentTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                windowType.GetField("materialTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                windowType.GetField("toolTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                windowType.GetField("reagentTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                windowType.GetField("miracleTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            };

            foreach (var field in templateLists)
            {
                if (field != null)
                {
                    var list = field.GetValue(window) as System.Collections.IList;
                    if (list != null)
                    {
                        foreach (PrefabTemplate template in list)
                        {
                            if (MigrateTemplate(template))
                            {
                                totalMigrated++;
                            }
                        }
                    }
                }
            }

            EditorUtility.DisplayDialog("Migration Complete",
                $"Migrated {totalMigrated} template(s) to schema version {CurrentSchemaVersion}.",
                "OK");

            Debug.Log($"Prefab Template Migration: {totalMigrated} template(s) migrated to schema v{CurrentSchemaVersion}");
        }
    }
}

