using System.Collections.Generic;
using System.Linq;
using Godgame.Abilities;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// UI for the "Spells & Skills" tab in Prefab Editor Window.
    /// Handles import/update of spec assets, dry-run diff, bake execution, and validation.
    /// </summary>
    public static class SpellsSkillsTabUI
    {
        private static SpellSpecCatalog selectedSpellCatalog;
        private static SkillSpecCatalog selectedSkillCatalog;
        private static StatusSpecCatalog selectedStatusCatalog;
        private static SpellBindingSet selectedBindingSet;
        private static Vector2 spellScrollPos;
        private static Vector2 skillScrollPos;
        private static Vector2 statusScrollPos;
        private static Vector2 bindingScrollPos;
        private static bool showValidationResults = false;
        private static string validationOutput = "";

        public static void DrawTab()
        {
            EditorGUILayout.LabelField("Spells & Skills", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Catalog selection
            DrawCatalogSelection();

            EditorGUILayout.Space();

            // Sub-tabs for different spec types
            var subTab = GUILayout.Toolbar(0, new[] { "Spells", "Skills", "Statuses", "Bindings" });

            EditorGUILayout.Space();

            switch (subTab)
            {
                case 0:
                    DrawSpellsPanel();
                    break;
                case 1:
                    DrawSkillsPanel();
                    break;
                case 2:
                    DrawStatusesPanel();
                    break;
                case 3:
                    DrawBindingsPanel();
                    break;
            }

            EditorGUILayout.Space();

            // Action buttons
            DrawActionButtons();
        }

        private static void DrawCatalogSelection()
        {
            EditorGUILayout.LabelField("Catalogs", EditorStyles.boldLabel);

            selectedSpellCatalog = (SpellSpecCatalog)EditorGUILayout.ObjectField(
                "Spell Catalog", selectedSpellCatalog, typeof(SpellSpecCatalog), false);

            selectedSkillCatalog = (SkillSpecCatalog)EditorGUILayout.ObjectField(
                "Skill Catalog", selectedSkillCatalog, typeof(SkillSpecCatalog), false);

            selectedStatusCatalog = (StatusSpecCatalog)EditorGUILayout.ObjectField(
                "Status Catalog", selectedStatusCatalog, typeof(StatusSpecCatalog), false);

            selectedBindingSet = (SpellBindingSet)EditorGUILayout.ObjectField(
                "Binding Set", selectedBindingSet, typeof(SpellBindingSet), false);
        }

        private static void DrawSpellsPanel()
        {
            EditorGUILayout.LabelField("Spell Specifications", EditorStyles.boldLabel);

            if (selectedSpellCatalog == null)
            {
                EditorGUILayout.HelpBox("No spell catalog selected. Assign one above.", MessageType.Info);
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(spellScrollPos))
            {
                spellScrollPos = scroll.scrollPosition;

                if (selectedSpellCatalog.Spells != null && selectedSpellCatalog.Spells.Length > 0)
                {
                    EditorGUILayout.LabelField($"Total Spells: {selectedSpellCatalog.Spells.Length}");

                    foreach (var spell in selectedSpellCatalog.Spells)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"ID: {spell.Id}", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Shape: {spell.Shape}, Range: {spell.Range}, Cost: {spell.Cost}");
                        EditorGUILayout.LabelField($"Cast Time: {spell.CastTime}s, Cooldown: {spell.Cooldown}s");
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No spells defined in catalog.", MessageType.Warning);
                }
            }
        }

        private static void DrawSkillsPanel()
        {
            EditorGUILayout.LabelField("Skill Specifications", EditorStyles.boldLabel);

            if (selectedSkillCatalog == null)
            {
                EditorGUILayout.HelpBox("No skill catalog selected. Assign one above.", MessageType.Info);
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(skillScrollPos))
            {
                skillScrollPos = scroll.scrollPosition;

                if (selectedSkillCatalog.Skills != null && selectedSkillCatalog.Skills.Length > 0)
                {
                    EditorGUILayout.LabelField($"Total Skills: {selectedSkillCatalog.Skills.Length}");

                    foreach (var skill in selectedSkillCatalog.Skills)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"ID: {skill.Id}", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Type: {(skill.Passive ? "Passive" : "Active")}, Tier: {skill.Tier}");
                        if (skill.Requires != null && skill.Requires.Length > 0)
                        {
                            EditorGUILayout.LabelField($"Requires: {string.Join(", ", skill.Requires)}");
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No skills defined in catalog.", MessageType.Warning);
                }
            }
        }

        private static void DrawStatusesPanel()
        {
            EditorGUILayout.LabelField("Status Specifications", EditorStyles.boldLabel);

            if (selectedStatusCatalog == null)
            {
                EditorGUILayout.HelpBox("No status catalog selected. Assign one above.", MessageType.Info);
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(statusScrollPos))
            {
                statusScrollPos = scroll.scrollPosition;

                if (selectedStatusCatalog.Statuses != null && selectedStatusCatalog.Statuses.Length > 0)
                {
                    EditorGUILayout.LabelField($"Total Statuses: {selectedStatusCatalog.Statuses.Length}");

                    foreach (var status in selectedStatusCatalog.Statuses)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"ID: {status.Id}", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Duration: {status.Duration}s, Period: {status.Period}s, Stacks: {status.MaxStacks}");
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No statuses defined in catalog.", MessageType.Warning);
                }
            }
        }

        private static void DrawBindingsPanel()
        {
            EditorGUILayout.LabelField("Spell Presentation Bindings", EditorStyles.boldLabel);

            if (selectedBindingSet == null)
            {
                EditorGUILayout.HelpBox("No binding set selected. Assign one above.", MessageType.Info);
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(bindingScrollPos))
            {
                bindingScrollPos = scroll.scrollPosition;

                EditorGUILayout.LabelField($"Set: {selectedBindingSet.SetName} ({selectedBindingSet.Type})");

                if (selectedBindingSet.Bindings != null && selectedBindingSet.Bindings.Length > 0)
                {
                    EditorGUILayout.LabelField($"Total Bindings: {selectedBindingSet.Bindings.Length}");

                    foreach (var binding in selectedBindingSet.Bindings)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Spell ID: {binding.SpellId}", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"StartFX: {(binding.StartFX != null ? binding.StartFX.name : "None")}");
                        EditorGUILayout.LabelField($"ImpactFX: {(binding.ImpactFX != null ? binding.ImpactFX.name : "None")}");
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No bindings defined in set.", MessageType.Warning);
                }
            }
        }

        private static void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Dry-Run (Preview)"))
                {
                    PerformDryRun();
                }

                if (GUILayout.Button("Validate All"))
                {
                    ValidateAll();
                }

                if (GUILayout.Button("Bake Blobs"))
                {
                    BakeBlobs();
                }
            }

            if (showValidationResults)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(validationOutput, GUILayout.Height(200));
            }
        }

        private static void PerformDryRun()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Dry-Run Report ===");
            report.AppendLine();

            if (selectedSpellCatalog != null)
            {
                report.AppendLine($"Spell Catalog: {selectedSpellCatalog.name}");
                report.AppendLine($"  Spells: {selectedSpellCatalog.Spells?.Length ?? 0}");
                report.AppendLine();
            }

            if (selectedSkillCatalog != null)
            {
                report.AppendLine($"Skill Catalog: {selectedSkillCatalog.name}");
                report.AppendLine($"  Skills: {selectedSkillCatalog.Skills?.Length ?? 0}");
                report.AppendLine();
            }

            if (selectedStatusCatalog != null)
            {
                report.AppendLine($"Status Catalog: {selectedStatusCatalog.name}");
                report.AppendLine($"  Statuses: {selectedStatusCatalog.Statuses?.Length ?? 0}");
                report.AppendLine();
            }

            if (selectedBindingSet != null)
            {
                report.AppendLine($"Binding Set: {selectedBindingSet.SetName}");
                report.AppendLine($"  Bindings: {selectedBindingSet.Bindings?.Length ?? 0}");
                report.AppendLine();
            }

            report.AppendLine("Dry-run complete. No assets were modified.");

            validationOutput = report.ToString();
            showValidationResults = true;

            Debug.Log(validationOutput);
        }

        private static void ValidateAll()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Validation Report ===");
            report.AppendLine();

            bool allValid = true;

            if (selectedSpellCatalog != null)
            {
                var result = SpellSpecValidator.ValidateCatalog(selectedSpellCatalog);
                report.AppendLine($"Spell Catalog: {selectedSpellCatalog.name}");
                report.AppendLine($"  Valid: {result.IsValid}");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  ERROR: {error}");
                }
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"  WARNING: {warning}");
                }
                report.AppendLine();
                if (!result.IsValid) allValid = false;
            }

            if (selectedSkillCatalog != null)
            {
                var result = SkillTreeValidator.ValidateCatalog(selectedSkillCatalog);
                report.AppendLine($"Skill Catalog: {selectedSkillCatalog.name}");
                report.AppendLine($"  Valid: {result.IsValid}");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  ERROR: {error}");
                }
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"  WARNING: {warning}");
                }
                report.AppendLine();
                if (!result.IsValid) allValid = false;
            }

            if (selectedStatusCatalog != null)
            {
                var result = StatusSpecValidator.ValidateCatalog(selectedStatusCatalog);
                report.AppendLine($"Status Catalog: {selectedStatusCatalog.name}");
                report.AppendLine($"  Valid: {result.IsValid}");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  ERROR: {error}");
                }
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"  WARNING: {warning}");
                }
                report.AppendLine();
                if (!result.IsValid) allValid = false;
            }

            if (selectedBindingSet != null && selectedSpellCatalog != null)
            {
                var result = SpellBindingValidator.ValidateBindingSet(selectedBindingSet, selectedSpellCatalog);
                report.AppendLine($"Binding Set: {selectedBindingSet.SetName}");
                report.AppendLine($"  Valid: {result.IsValid}");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  ERROR: {error}");
                }
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"  WARNING: {warning}");
                }
                report.AppendLine();
                if (!result.IsValid) allValid = false;
            }

            report.AppendLine($"Overall: {(allValid ? "VALID" : "INVALID")}");

            validationOutput = report.ToString();
            showValidationResults = true;

            if (allValid)
            {
                Debug.Log(validationOutput);
            }
            else
            {
                Debug.LogError(validationOutput);
            }
        }

        private static void BakeBlobs()
        {
            // Trigger bakers by marking catalogs as dirty and forcing reimport
            if (selectedSpellCatalog != null)
            {
                EditorUtility.SetDirty(selectedSpellCatalog);
            }

            if (selectedSkillCatalog != null)
            {
                EditorUtility.SetDirty(selectedSkillCatalog);
            }

            if (selectedStatusCatalog != null)
            {
                EditorUtility.SetDirty(selectedStatusCatalog);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Blobs baked. Check console for any errors.");
        }
    }
}

