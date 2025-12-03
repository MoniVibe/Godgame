using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Godgame.Editor.PrefabTool;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Main editor window for prefab generation tool.
    /// </summary>
    public class PrefabEditorWindow : EditorWindow
    {
        private int selectedCategoryTab = 0;
        private readonly string[] categoryTabs = { "Buildings", "Individuals", "Equipment", "Materials", "Tools", "Reagents", "Miracles", "Spells & Skills", "Biomes", "Vegetation" };

        // Template storage
        private List<BuildingTemplate> buildingTemplates = new List<BuildingTemplate>();
        private List<IndividualTemplate> individualTemplates = new List<IndividualTemplate>();
        private List<EquipmentTemplate> equipmentTemplates = new List<EquipmentTemplate>();
        private List<MaterialTemplate> materialTemplates = new List<MaterialTemplate>();
        private List<ToolTemplate> toolTemplates = new List<ToolTemplate>();
        private List<ReagentTemplate> reagentTemplates = new List<ReagentTemplate>();
        private List<MiracleTemplate> miracleTemplates = new List<MiracleTemplate>();

        // UI state
        private Vector2 scrollPosition;
        private Vector2 editorScrollPosition;
        private int selectedTemplateIndex = -1;
        private PrefabTemplate selectedTemplate = null; // Currently editing template
        private string searchFilter = "";
        
        // Bulk edit state
        private bool bulkEditMode = false;
        private HashSet<int> selectedTemplateIds = new HashSet<int>();
        
        // Filter state
        private Rarity? filterRarity = null; // null = no filter
        private byte filterTechTier = 255; // 255 = no filter
        private string filterCategory = "";

        [MenuItem("Godgame/Prefab Editor")]
        public static void ShowWindow()
        {
            GetWindow<PrefabEditorWindow>("Prefab Editor");
        }
        
        [MenuItem("Godgame/Generate All Building Prefabs with Placeholders")]
        public static void GenerateAllBuildingPrefabsWithPlaceholders()
        {
            var window = GetWindow<PrefabEditorWindow>("Prefab Editor");
            window.LoadDefaultTemplates();
            
            int count = 0;
            foreach (var template in window.GetBuildingTemplates())
            {
                // Ensure visual presentation placeholder is enabled
                if (template.visualPresentation == null)
                    template.visualPresentation = new VisualPresentation();
                template.visualPresentation.usePrimitiveFallback = true;
                
                // Set appropriate primitive type based on building type
                switch (template.buildingType)
                {
                    case BuildingTemplate.BuildingType.Storage:
                        template.visualPresentation.primitiveType = PrimitiveType.Cube;
                        template.visualPresentation.scale = new Vector3(2f, 2f, 2f);
                        break;
                    case BuildingTemplate.BuildingType.Utility:
                        template.visualPresentation.primitiveType = PrimitiveType.Cylinder;
                        template.visualPresentation.scale = new Vector3(1f, 2f, 1f);
                        break;
                    case BuildingTemplate.BuildingType.Worship:
                        template.visualPresentation.primitiveType = PrimitiveType.Cube;
                        template.visualPresentation.scale = new Vector3(2f, 3f, 2f);
                        break;
                    default:
                        template.visualPresentation.primitiveType = PrimitiveType.Cube;
                        template.visualPresentation.scale = new Vector3(2f, 2f, 2f);
                        break;
                }
                
                window.GenerateBuildingPrefab(template);
                count++;
            }
            
            Debug.Log($"Generated {count} building prefabs with visual placeholders");
            UnityEditor.EditorUtility.DisplayDialog("Success", $"Generated {count} building prefabs with visual placeholders!", "OK");
        }

        private void OnEnable()
        {
            LoadDefaultTemplates();
        }
        
        // Public accessors for CLI access
        public List<BuildingTemplate> GetBuildingTemplates() => buildingTemplates;
        public List<IndividualTemplate> GetIndividualTemplates() => individualTemplates;
        public List<EquipmentTemplate> GetEquipmentTemplates() => equipmentTemplates;
        public List<MaterialTemplate> GetMaterialTemplates() => materialTemplates;
        public List<ToolTemplate> GetToolTemplates() => toolTemplates;
        public List<ReagentTemplate> GetReagentTemplates() => reagentTemplates;
        public List<MiracleTemplate> GetMiracleTemplates() => miracleTemplates;

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                // Category tabs
                selectedCategoryTab = GUILayout.Toolbar(selectedCategoryTab, categoryTabs);

                EditorGUILayout.Space();

                // Search filter
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Search:", GUILayout.Width(60));
                    searchFilter = EditorGUILayout.TextField(searchFilter);
                }

                EditorGUILayout.Space();

                // Split view: List on left, Editor on right
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Left panel: Template list
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(400)))
                    {
                        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
                        {
                            scrollPosition = scroll.scrollPosition;

                            switch (selectedCategoryTab)
                            {
                                case 0: DrawBuildingsTab(); break;
                                case 1: DrawIndividualsTab(); break;
                                case 2: DrawEquipmentTab(); break;
                                case 3: DrawMaterialsTab(); break;
                                case 4: DrawToolsTab(); break;
                                case 5: DrawReagentsTab(); break;
                                case 6: DrawMiraclesTab(); break;
                                case 7: SpellsSkillsTabUI.DrawTab(); break;
                                case 8: 
                                    // TODO: Uncomment when BiomeDefinitionAuthoring is implemented
                                    // BiomesTabUI.DrawBiomesTab(new Rect(0, 0, position.width, position.height)); 
                                    EditorGUILayout.HelpBox("Biomes tab is not yet available. BiomeDefinitionAuthoring needs to be implemented first.", MessageType.Info);
                                    break;
                                case 9: 
                                    // TODO: Uncomment when PlantSpecCatalogAuthoring and StandSpecCatalogAuthoring are implemented
                                    // VegetationTabUI.DrawVegetationTab(new Rect(0, 0, position.width, position.height)); 
                                    EditorGUILayout.HelpBox("Vegetation tab is not yet available. PlantSpecCatalogAuthoring and StandSpecCatalogAuthoring need to be implemented first.", MessageType.Info);
                                    break;
                            }
                        }
                    }

                    // Right panel: Template editor
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (var scroll = new EditorGUILayout.ScrollViewScope(editorScrollPosition))
                        {
                            editorScrollPosition = scroll.scrollPosition;

                            if (selectedTemplate != null)
                            {
                                DrawTemplateEditor(selectedTemplate);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Select a template from the list to edit it.", MessageType.Info);
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                // Action buttons
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Dry-Run (Preview)"))
                    {
                        PerformDryRun();
                    }
                    if (GUILayout.Button("Generate All Prefabs"))
                    {
                        GenerateAllPrefabs();
                    }
                    if (GUILayout.Button("Validate All"))
                    {
                        ValidateAllTemplates();
                    }
                }

                EditorGUILayout.Space();

                // Bulk generator panel
                BulkGeneratorUI.DrawBulkGeneratorPanel(
                    materialTemplates,
                    equipmentTemplates,
                    toolTemplates,
                    (equipment) => 
                    { 
                        equipmentTemplates.Add(equipment);
                        EditorUtility.SetDirty(this);
                    },
                    (tool) => 
                    { 
                        toolTemplates.Add(tool);
                        EditorUtility.SetDirty(this);
                    },
                    () => 
                    {
                        EditorUtility.SetDirty(this);
                        AssetDatabase.SaveAssets();
                        Repaint(); // Refresh UI
                    }
                );
            }
        }

        private void DrawTemplateEditor(PrefabTemplate template)
        {
            switch (template)
            {
                case BuildingTemplate building:
                    EditBuildingTemplate(building);
                    break;
                case IndividualTemplate individual:
                    EditIndividualTemplate(individual);
                    break;
                case EquipmentTemplate equipment:
                    EditEquipmentTemplate(equipment);
                    break;
                case MaterialTemplate material:
                    EditMaterialTemplate(material);
                    break;
                case ToolTemplate tool:
                    EditToolTemplate(tool);
                    break;
                case ReagentTemplate reagent:
                    EditReagentTemplate(reagent);
                    break;
                case MiracleTemplate miracle:
                    EditMiracleTemplate(miracle);
                    break;
                default:
                    EditorGUILayout.HelpBox("Unknown template type.", MessageType.Warning);
                    break;
            }
        }

        private void DrawBuildingsTab()
        {
            EditorGUILayout.LabelField("Building Templates", EditorStyles.boldLabel);

            // Filter templates
            var filtered = buildingTemplates.Where(t => 
                string.IsNullOrEmpty(searchFilter) || 
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) ||
                (t.displayName != null && t.displayName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // List templates
            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                bool isSelected = selectedTemplateIndex == i;
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)))
                {
                    selectedTemplateIndex = i;
                }

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Type: {template.buildingType}", GUILayout.Width(150));
                EditorGUILayout.LabelField($"Health: {template.calculatedHealth:F1}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"Desirability: {template.calculatedDesirability:F1}", GUILayout.Width(120));

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateBuildingPrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Building Template"))
            {
                buildingTemplates.Add(new BuildingTemplate
                {
                    name = "NewBuilding",
                    displayName = "New Building",
                    id = GetNextId(buildingTemplates),
                    buildingType = BuildingTemplate.BuildingType.Residence
                });
            }
        }

        private void DrawIndividualsTab()
        {
            EditorGUILayout.LabelField("Individual Templates", EditorStyles.boldLabel);

            var filtered = individualTemplates.Where(t =>
                string.IsNullOrEmpty(searchFilter) ||
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase)
            ).ToList();

            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Type: {template.individualType}", GUILayout.Width(150));

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateIndividualPrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Individual Template"))
            {
                individualTemplates.Add(new IndividualTemplate
                {
                    name = "NewIndividual",
                    displayName = "New Individual",
                    id = GetNextId(individualTemplates),
                    individualType = IndividualTemplate.IndividualType.Villager
                });
            }
        }

        private void DrawEquipmentTab()
        {
            EditorGUILayout.LabelField("Equipment Templates", EditorStyles.boldLabel);

            var filtered = equipmentTemplates.Where(t =>
                string.IsNullOrEmpty(searchFilter) ||
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) ||
                (t.displayName != null && t.displayName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Q:{template.quality:F0} R:{template.rarity} T:{template.techTier}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"ID:{template.itemDefId.Value}", GUILayout.Width(60));

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateEquipmentPrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Equipment Template"))
            {
                equipmentTemplates.Add(new EquipmentTemplate
                {
                    name = "NewEquipment",
                    displayName = "New Equipment",
                    id = GetNextId(equipmentTemplates),
                    equipmentType = EquipmentTemplate.EquipmentType.Weapon,
                    slotKind = SlotKind.Hand
                });
            }
        }

        private void DrawMaterialsTab()
        {
            EditorGUILayout.LabelField("Material Templates", EditorStyles.boldLabel);

            var filtered = materialTemplates.Where(t =>
                string.IsNullOrEmpty(searchFilter) ||
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) ||
                (t.displayName != null && t.displayName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Category: {template.category}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"Q:{template.quality:F0} R:{template.rarity} T:{template.techTier}", GUILayout.Width(150));
                EditorGUILayout.LabelField($"ID:{template.materialId.Value}", GUILayout.Width(60));

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateMaterialPrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Material Template"))
            {
                materialTemplates.Add(new MaterialTemplate
                {
                    name = "NewMaterial",
                    displayName = "New Material",
                    id = GetNextId(materialTemplates),
                    category = MaterialCategory.Raw
                });
            }
        }

        private void DrawToolsTab()
        {
            EditorGUILayout.LabelField("Tool Templates", EditorStyles.boldLabel);

            var filtered = toolTemplates.Where(t =>
                string.IsNullOrEmpty(searchFilter) ||
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) ||
                (t.displayName != null && t.displayName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Q:{template.quality:F0} R:{template.rarity} T:{template.techTier}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"ID:{template.itemDefId.Value} RID:{template.recipeId.Value}", GUILayout.Width(100));
                
                if (template.productionInputs != null && template.productionInputs.Count > 0)
                {
                    EditorGUILayout.LabelField($"Inputs: {template.productionInputs.Count}", GUILayout.Width(100));
                }
                
                if (!string.IsNullOrEmpty(template.producedFrom))
                {
                    EditorGUILayout.LabelField($"From: {template.producedFrom}", GUILayout.Width(150));
                }

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateToolPrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Tool Template"))
            {
                toolTemplates.Add(new ToolTemplate
                {
                    name = "NewTool",
                    displayName = "New Tool",
                    id = GetNextId(toolTemplates)
                });
            }
        }

        private void DrawReagentsTab()
        {
            EditorGUILayout.LabelField("Reagent Templates", EditorStyles.boldLabel);

            var filtered = reagentTemplates.Where(t =>
                string.IsNullOrEmpty(searchFilter) ||
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) ||
                (t.displayName != null && t.displayName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Potency: {template.potency:F1}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"Rarity: {template.rarity:F1}", GUILayout.Width(100));

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateReagentPrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Reagent Template"))
            {
                reagentTemplates.Add(new ReagentTemplate
                {
                    name = "NewReagent",
                    displayName = "New Reagent",
                    id = GetNextId(reagentTemplates)
                });
            }
        }

        private void DrawMiraclesTab()
        {
            EditorGUILayout.LabelField("Miracle Templates", EditorStyles.boldLabel);

            var filtered = miracleTemplates.Where(t =>
                string.IsNullOrEmpty(searchFilter) ||
                t.name.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) ||
                (t.displayName != null && t.displayName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            for (int i = 0; i < filtered.Count; i++)
            {
                var template = filtered[i];
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(template.displayName ?? template.name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"Cost: {template.manaCost:F1}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"Range: {template.range:F1}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"Cooldown: {template.cooldown:F1}s", GUILayout.Width(100));

                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    selectedTemplate = template;
                    selectedTemplateIndex = i;
                }
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateMiraclePrefab(template);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add New Miracle Template"))
            {
                miracleTemplates.Add(new MiracleTemplate
                {
                    name = "NewMiracle",
                    displayName = "New Miracle",
                    id = GetNextId(miracleTemplates)
                });
            }
        }

        private void EditBuildingTemplate(BuildingTemplate template)
        {
            if (template == null) return;
            
            // Use inline editor for consistency with other types
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            
            EditorGUILayout.Space();
            
            // Building-specific fields
            template.buildingType = (BuildingTemplate.BuildingType)EditorGUILayout.EnumPopup("Building Type", template.buildingType);
            template.baseHealth = EditorGUILayout.FloatField("Base Health", template.baseHealth);
            template.baseDesirability = EditorGUILayout.FloatField("Base Desirability", template.baseDesirability);
            
            EditorGUILayout.Space();
            if (template.bonuses == null)
                template.bonuses = new List<PrefabBonus>();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            if (template.visualPresentation == null)
                template.visualPresentation = new VisualPresentation();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            if (template.vfxPresentation == null)
                template.vfxPresentation = new VFXPresentation();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
        }

        private void EditIndividualTemplate(IndividualTemplate template)
        {
            if (template == null) return;
            
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            template.individualType = (IndividualTemplate.IndividualType)EditorGUILayout.EnumPopup("Individual Type", template.individualType);
            template.factionId = EditorGUILayout.IntField("Faction ID", template.factionId);
            
            EditorGUILayout.Space();
            
            // Core Attributes (Experience Modifiers)
            EditorGUILayout.LabelField("Core Attributes (Experience Modifiers)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.physique = EditorGUILayout.Slider("Physique", template.physique, 0f, 100f);
            template.finesse = EditorGUILayout.Slider("Finesse", template.finesse, 0f, 100f);
            template.will = EditorGUILayout.Slider("Will", template.will, 0f, 100f);
            template.wisdom = EditorGUILayout.Slider("Wisdom", template.wisdom, 0f, 100f);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Derived Attributes
            EditorGUILayout.LabelField("Derived Attributes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.strength = EditorGUILayout.Slider("Strength", template.strength, 0f, 100f);
            template.agility = EditorGUILayout.Slider("Agility", template.agility, 0f, 100f);
            template.intelligence = EditorGUILayout.Slider("Intelligence", template.intelligence, 0f, 100f);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Social Stats
            EditorGUILayout.LabelField("Social Stats", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.fame = EditorGUILayout.Slider("Fame", template.fame, 0f, 1000f);
            template.wealth = EditorGUILayout.FloatField("Wealth (Currency)", template.wealth);
            template.reputation = EditorGUILayout.Slider("Reputation", template.reputation, -100f, 100f);
            template.glory = EditorGUILayout.Slider("Glory", template.glory, 0f, 1000f);
            template.renown = EditorGUILayout.Slider("Renown", template.renown, 0f, 1000f);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Combat Stats
            EditorGUILayout.LabelField("Combat Stats (Base Values)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Set to 0 to auto-calculate from attributes at runtime.", MessageType.Info);
            EditorGUI.indentLevel++;
            template.baseAttack = EditorGUILayout.Slider("Base Attack (0=auto)", template.baseAttack, 0f, 100f);
            template.baseDefense = EditorGUILayout.Slider("Base Defense (0=auto)", template.baseDefense, 0f, 100f);
            template.baseHealthOverride = EditorGUILayout.FloatField("Base Health Override (0=use baseHealth)", template.baseHealthOverride);
            if (template.baseHealthOverride <= 0f)
            {
                template.baseHealth = EditorGUILayout.FloatField("Base Health", template.baseHealth);
            }
            template.baseStamina = EditorGUILayout.FloatField("Base Stamina (Rounds, 0=auto)", template.baseStamina);
            template.baseMana = EditorGUILayout.Slider("Base Mana (0=auto, 0=non-magic)", template.baseMana, 0f, 100f);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Need Stats
            EditorGUILayout.LabelField("Need Stats (Starting Values)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.food = EditorGUILayout.Slider("Food", template.food, 0f, 100f);
            template.rest = EditorGUILayout.Slider("Rest", template.rest, 0f, 100f);
            template.sleep = EditorGUILayout.Slider("Sleep", template.sleep, 0f, 100f);
            template.generalHealth = EditorGUILayout.Slider("General Health", template.generalHealth, 0f, 100f);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Resistances
            EditorGUILayout.LabelField("Resistances (0-100% Reduction)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Damage type resistances. Values are multipliers (0.0 = no resistance, 1.0 = 100% immunity).", MessageType.Info);
            if (template.resistances == null)
                template.resistances = new Dictionary<string, float>();
            
            EditorGUI.indentLevel++;
            string[] damageTypes = { "Physical", "Fire", "Cold", "Poison", "Magic", "Lightning", "Holy", "Dark" };
            foreach (string damageType in damageTypes)
            {
                if (!template.resistances.ContainsKey(damageType))
                    template.resistances[damageType] = 0f;
                template.resistances[damageType] = EditorGUILayout.Slider(damageType, template.resistances[damageType], 0f, 1f);
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Healing & Spell Modifiers
            EditorGUILayout.LabelField("Healing & Spell Modifiers", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.healBonus = EditorGUILayout.FloatField("Heal Bonus (Multiplier)", template.healBonus);
            template.spellDurationModifier = EditorGUILayout.FloatField("Spell Duration Modifier", template.spellDurationModifier);
            template.spellIntensityModifier = EditorGUILayout.FloatField("Spell Intensity Modifier", template.spellIntensityModifier);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Limb System
            EditorGUILayout.LabelField("Limb System", EditorStyles.boldLabel);
            if (template.limbs == null)
                template.limbs = new List<LimbReference>();
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.limbs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.limbs[i].limbId = EditorGUILayout.TextField("Limb ID", template.limbs[i].limbId);
                template.limbs[i].health = EditorGUILayout.Slider("Health", template.limbs[i].health, 0f, 100f);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.limbs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Limb"))
            {
                template.limbs.Add(new LimbReference { limbId = "", health = 100f });
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Implants
            EditorGUILayout.LabelField("Implants", EditorStyles.boldLabel);
            if (template.implants == null)
                template.implants = new List<ImplantReference>();
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.implants.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.implants[i].implantId = EditorGUILayout.TextField("Implant ID", template.implants[i].implantId);
                template.implants[i].attachedToLimb = EditorGUILayout.TextField("Attached To", template.implants[i].attachedToLimb);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.implants.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Implant"))
            {
                template.implants.Add(new ImplantReference { implantId = "", attachedToLimb = "" });
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Outlooks & Alignments
            EditorGUILayout.LabelField("Outlooks & Alignments", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.alignmentId = EditorGUILayout.TextField("Alignment ID", template.alignmentId);
            
            if (template.outlookIds == null)
                template.outlookIds = new List<string>();
            EditorGUILayout.LabelField("Outlook IDs:");
            for (int i = 0; i < template.outlookIds.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.outlookIds[i] = EditorGUILayout.TextField(template.outlookIds[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.outlookIds.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Outlook ID"))
            {
                template.outlookIds.Add("");
            }
            
            if (template.disposition == null)
                template.disposition = new Dictionary<string, float>();
            EditorGUILayout.LabelField("Disposition:");
            string[] dispositionTypes = { "Loyalty", "Fear", "Love", "Trust", "Respect" };
            foreach (string dispType in dispositionTypes)
            {
                if (!template.disposition.ContainsKey(dispType))
                    template.disposition[dispType] = 0f;
                template.disposition[dispType] = EditorGUILayout.Slider(dispType, template.disposition[dispType], -100f, 100f);
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Personality Traits
            EditorGUILayout.LabelField("Personality Traits", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Vengeful/Forgiving: -100 (Vengeful) to +100 (Forgiving)\nBold/Craven: -100 (Craven) to +100 (Bold)", MessageType.Info);
            EditorGUI.indentLevel++;
            template.vengefulScore = (sbyte)EditorGUILayout.IntSlider("Vengeful ↔ Forgiving", template.vengefulScore, -100, 100);
            template.boldScore = (sbyte)EditorGUILayout.IntSlider("Craven ↔ Bold", template.boldScore, -100, 100);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Creature Type Flags
            EditorGUILayout.LabelField("Creature Type Flags", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.isUndead = EditorGUILayout.Toggle("Is Undead", template.isUndead);
            template.isSummoned = EditorGUILayout.Toggle("Is Summoned", template.isSummoned);
            EditorGUILayout.HelpBox("Undead flag affects various effects (healing, resurrection, etc.)\nSummoned flag affects various effects (duration, dismissal, etc.)", MessageType.Info);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Titles (Deeds of Rulership)
            EditorGUILayout.LabelField("Titles (Deeds of Rulership)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Titles are deeds of rulership passed on with inheritance or otherwise depending on outlooks. " +
                "Examples: Hero of a village, Elite living in a mansion, Ruler over a village and its lands. " +
                "Titles are acquired when founding a village, successfully defending it, having enough renown, etc.\n\n" +
                "Titles have levels from 1 (leader of an upstart band) to 10+ (ruler of multiple empires). " +
                "Only the HIGHEST level title is presented/displayed, but the entity is known by ALL its titles.", MessageType.Info);
            
            if (template.titles == null)
                template.titles = new List<Title>();
            
            // Show primary title summary
            if (template.titles.Count > 0)
            {
                var activeTitles = TitleHelper.GetActiveTitles(template.titles);
                var formerTitles = TitleHelper.GetFormerTitles(template.titles);
                var primaryTitle = TitleHelper.GetHighestLevelTitle(template.titles, activeOnly: true);
                
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Title Summary:", EditorStyles.boldLabel);
                    
                    if (primaryTitle != null)
                    {
                        EditorGUILayout.LabelField($"Primary Title: {primaryTitle.GetEffectiveDisplayName()}");
                        EditorGUILayout.LabelField($"  Level {primaryTitle.titleLevel} - {primaryTitle.GetLevelDescription()}", EditorStyles.miniLabel);
                    }
                    else if (formerTitles.Count > 0)
                    {
                        var highestFormer = TitleHelper.GetHighestLevelTitle(template.titles, activeOnly: false);
                        EditorGUILayout.LabelField($"Primary Title: {highestFormer.GetEffectiveDisplayName()} (FORMER)", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"  Level {highestFormer.titleLevel} - {highestFormer.GetLevelDescription()}", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.LabelField(TitleHelper.GetTitleSummary(template.titles), EditorStyles.miniLabel);
                }
                EditorGUILayout.Space();
            }
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.titles.Count; i++)
            {
                var title = template.titles[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    bool removed = false;
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool isPrimary = TitleHelper.GetHighestLevelTitle(template.titles, activeOnly: true) == title;
                        bool isActive = title.IsActive();
                        string statusText = isActive ? "ACTIVE" : title.status.ToString().ToUpper();
                        string titleLabel = isPrimary ? $"Title {i + 1} (PRIMARY - Level {title.titleLevel} - {statusText})" : $"Title {i + 1} (Level {title.titleLevel} - {statusText})";
                        
                        // Color code: active = normal, former = gray
                        var originalColor = GUI.color;
                        if (!isActive)
                            GUI.color = Color.gray;
                        
                        EditorGUILayout.LabelField(titleLabel, EditorStyles.boldLabel, GUILayout.Width(350));
                        GUI.color = originalColor;
                        
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            template.titles.RemoveAt(i);
                            removed = true;
                        }
                    }

                    if (removed)
                    {
                        break;
                    }
                    
                    bool isActiveTitle = title.IsActive();
                    bool isPrimaryTitle = TitleHelper.GetHighestLevelTitle(template.titles, activeOnly: true) == title;
                    if (isPrimaryTitle && isActiveTitle)
                    {
                        EditorGUILayout.HelpBox("This is the PRIMARY title (highest level active) - will be displayed.", MessageType.Info);
                    }
                    else if (isPrimaryTitle && !isActiveTitle)
                    {
                        EditorGUILayout.HelpBox("This is the highest level title, but it is FORMER. Only former titles exist - will be displayed as 'Former [Title]'.", MessageType.Warning);
                    }
                    else if (!isActiveTitle)
                    {
                        EditorGUILayout.HelpBox("This title is FORMER - carries reduced prestige (shadow of proper title).", MessageType.Warning);
                    }
                    
                    title.titleName = EditorGUILayout.TextField("Title Name", title.titleName);
                    title.displayName = EditorGUILayout.TextField("Display Name", title.displayName);
                    title.titleType = (TitleType)EditorGUILayout.EnumPopup("Title Type", title.titleType);
                    title.status = (TitleStatus)EditorGUILayout.EnumPopup("Status", title.status);
                    
                    if (!title.IsActive())
                    {
                        EditorGUI.indentLevel++;
                        title.lossReason = EditorGUILayout.TextField("Loss Reason", title.lossReason);
                        EditorGUILayout.HelpBox($"Effective Display: {title.GetEffectiveDisplayName()}", MessageType.None);
                        EditorGUI.indentLevel--;
                    }
                    
                    title.associatedSettlement = EditorGUILayout.TextField("Associated Settlement", title.associatedSettlement);
                    title.associatedBuilding = EditorGUILayout.TextField("Associated Building", title.associatedBuilding);
                    title.acquisitionMethod = (TitleAcquisitionMethod)EditorGUILayout.EnumPopup("Acquisition Method", title.acquisitionMethod);
                    title.isInheritable = EditorGUILayout.Toggle("Is Inheritable", title.isInheritable);
                    title.minRenown = EditorGUILayout.Slider("Min Renown Required", title.minRenown, 0f, 1000f);
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        title.titleLevel = EditorGUILayout.IntSlider("Title Level", title.titleLevel, 1, 15);
                        EditorGUILayout.LabelField($"({title.GetLevelDescription()})", EditorStyles.miniLabel);
                    }
                    
                    // Show effective bonuses (reduced for former titles)
                    EditorGUILayout.LabelField("Bonuses:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    var effectiveBonuses = title.GetEffectiveBonuses();
                    if (!title.IsActive() && effectiveBonuses != title.bonuses)
                    {
                        EditorGUILayout.HelpBox("Former title bonuses are reduced (shadow of proper title).", MessageType.Info);
                    }
                    EditorGUILayout.LabelField($"Reputation: {effectiveBonuses.reputationBonus:F1} | Fame: {effectiveBonuses.fameBonus:F1} | Renown: {effectiveBonuses.renownBonus:F1}");
                    EditorGUILayout.LabelField($"Social Standing: {effectiveBonuses.socialStandingModifier:F2}x | Authority: {effectiveBonuses.authorityLevel}", EditorStyles.miniLabel);
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.LabelField("Title Bonuses", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    if (title.bonuses == null)
                        title.bonuses = new TitleBonuses();
                    title.bonuses.reputationBonus = EditorGUILayout.Slider("Reputation Bonus", title.bonuses.reputationBonus, -100f, 100f);
                    title.bonuses.fameBonus = EditorGUILayout.Slider("Fame Bonus", title.bonuses.fameBonus, 0f, 1000f);
                    title.bonuses.wealthBonus = EditorGUILayout.FloatField("Wealth Bonus", title.bonuses.wealthBonus);
                    title.bonuses.renownBonus = EditorGUILayout.Slider("Renown Bonus", title.bonuses.renownBonus, 0f, 1000f);
                    title.bonuses.gloryBonus = EditorGUILayout.Slider("Glory Bonus", title.bonuses.gloryBonus, 0f, 1000f);
                    title.bonuses.socialStandingModifier = EditorGUILayout.FloatField("Social Standing Modifier", title.bonuses.socialStandingModifier);
                    title.bonuses.authorityLevel = EditorGUILayout.IntField("Authority Level", title.bonuses.authorityLevel);
                    EditorGUI.indentLevel--;
                    
                    title.description = EditorGUILayout.TextArea(title.description, GUILayout.Height(40));
                }
                
                EditorGUILayout.Space();
            }
            
            if (GUILayout.Button("Add Title"))
            {
                template.titles.Add(new Title
                {
                    titleName = "",
                    displayName = "",
                    titleType = TitleType.Hero,
                    acquisitionMethod = TitleAcquisitionMethod.Founding,
                    isInheritable = true,
                    bonuses = new TitleBonuses()
                });
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Legacy fields
            EditorGUILayout.LabelField("Legacy Fields", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            template.baseSpeed = EditorGUILayout.FloatField("Base Speed", template.baseSpeed);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            if (template.bonuses == null)
                template.bonuses = new List<PrefabBonus>();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            if (template.visualPresentation == null)
                template.visualPresentation = new VisualPresentation();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            if (template.vfxPresentation == null)
                template.vfxPresentation = new VFXPresentation();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
            
            EditorUtility.SetDirty(this);
        }

        private void EditEquipmentTemplate(EquipmentTemplate template)
        {
            if (template == null) return;
            
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            
            EditorGUILayout.Space();
            
            // Unified Quality / Rarity / Tech Tier (per items.md spec)
            EditorGUILayout.LabelField("Quality & Rarity", EditorStyles.boldLabel);
            template.quality = EditorGUILayout.Slider("Base Quality", template.quality, 0f, 100f);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Calculated Quality", template.calculatedQuality);
            EditorGUI.EndDisabledGroup();
            template.rarity = (Rarity)EditorGUILayout.EnumPopup("Base Rarity", template.rarity);
            template.techTier = (byte)EditorGUILayout.IntSlider("Tech Tier", template.techTier, 0, 10);
            template.requiredTechTier = (byte)EditorGUILayout.IntSlider("Required Tech Tier", template.requiredTechTier, 0, 10);
            
            EditorGUILayout.Space();
            
            // Catalog IDs (per items.md spec)
            EditorGUILayout.LabelField("Catalog IDs", EditorStyles.boldLabel);
            template.itemDefId = new ItemDefId((ushort)EditorGUILayout.IntField("ItemDefId", template.itemDefId.Value));
            EditorGUILayout.HelpBox(
                "Runtime ItemDefId (ushort). Set during blob baking. " +
                "Editor uses this for validation and export.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            EditorGUILayout.Space();
            
            // Equipment type and slot
            template.equipmentType = (EquipmentTemplate.EquipmentType)EditorGUILayout.EnumPopup("Equipment Type", template.equipmentType);
            template.slotKind = (SlotKind)EditorGUILayout.EnumPopup("Slot", template.slotKind);
            
            EditorGUILayout.Space();
            
            // Equipment stats
            EditorGUILayout.LabelField("Equipment Stats", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            if (template.equipmentType == EquipmentTemplate.EquipmentType.Weapon)
            {
                template.stats.damage = EditorGUILayout.FloatField("Base Damage", template.stats.damage);
                template.stats.critChance = EditorGUILayout.FloatField("Crit Chance (%)", template.stats.critChance);
                template.stats.critDamage = EditorGUILayout.FloatField("Crit Damage Multiplier", template.stats.critDamage);
            }
            else if (template.equipmentType == EquipmentTemplate.EquipmentType.Armor)
            {
                template.stats.armor = EditorGUILayout.FloatField("Base Armor", template.stats.armor);
                template.stats.blockChance = EditorGUILayout.FloatField("Block Chance (%)", template.stats.blockChance);
            }
            
            template.stats.weight = EditorGUILayout.FloatField("Weight (kg, 0 = use mass)", template.stats.weight);
            if (template.stats.weight <= 0f)
            {
                template.mass = EditorGUILayout.FloatField("Mass (kg)", template.mass);
            }
            template.stats.encumbrance = EditorGUILayout.FloatField("Encumbrance (%)", template.stats.encumbrance);
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Durability
            template.baseDurability = EditorGUILayout.FloatField("Base Durability", template.baseDurability);
            
            EditorGUILayout.Space();
            
            // Production Facilities
            EditorGUILayout.LabelField("Production Facilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "List of facility IDs/types that can produce this equipment. " +
                "Examples: 'Forge', 'Workshop', 'Apothecary', 'Smithy'. " +
                "Empty list = can be produced anywhere or no production facility required.",
                MessageType.Info
            );
            if (template.productionFacilities == null)
                template.productionFacilities = new List<string>();
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.productionFacilities.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.productionFacilities[i] = EditorGUILayout.TextField($"Facility {i + 1}", template.productionFacilities[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.productionFacilities.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Facility"))
            {
                template.productionFacilities.Add("");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            if (template.bonuses == null)
                template.bonuses = new List<PrefabBonus>();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            if (template.visualPresentation == null)
                template.visualPresentation = new VisualPresentation();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            if (template.vfxPresentation == null)
                template.vfxPresentation = new VFXPresentation();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
        }

        private void EditMaterialTemplate(MaterialTemplate template)
        {
            if (template == null) return;
            
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            
            EditorGUILayout.Space();
            
            // Unified Quality / Rarity / Tech Tier (per items.md spec)
            EditorGUILayout.LabelField("Quality & Rarity", EditorStyles.boldLabel);
            template.quality = EditorGUILayout.Slider("Base Quality", template.quality, 0f, 100f);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Calculated Quality", template.calculatedQuality);
            EditorGUI.EndDisabledGroup();
            template.rarity = (Rarity)EditorGUILayout.EnumPopup("Base Rarity", template.rarity);
            template.techTier = (byte)EditorGUILayout.IntSlider("Tech Tier", template.techTier, 0, 10);
            template.requiredTechTier = (byte)EditorGUILayout.IntSlider("Required Tech Tier", template.requiredTechTier, 0, 10);
            
            EditorGUILayout.Space();
            
            // Catalog IDs (per items.md spec)
            EditorGUILayout.LabelField("Catalog IDs", EditorStyles.boldLabel);
            template.materialId = new MaterialId((ushort)EditorGUILayout.IntField("MaterialId", template.materialId.Value));
            EditorGUILayout.HelpBox(
                "Runtime MaterialId (ushort). Set during blob baking. " +
                "Editor uses this for validation and export.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            // Material properties
            EditorGUILayout.LabelField("Material Properties", EditorStyles.boldLabel);
            template.category = (MaterialCategory)EditorGUILayout.EnumPopup("Category", template.category);
            template.purity = EditorGUILayout.Slider("Purity", template.purity, 0f, 100f);
            
            EditorGUILayout.Space();
            
            // Used in production
            EditorGUILayout.LabelField("Used In Production", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "List of tools/products that use this material (for reference). " +
                "This is automatically updated when tools reference this material.",
                MessageType.Info
            );
            
            if (template.usedInProduction != null && template.usedInProduction.Count > 0)
            {
                foreach (var product in template.usedInProduction)
                {
                    EditorGUILayout.LabelField($"  • {product}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (Not used in any production chains yet)");
            }
            
            EditorGUILayout.Space();
            
            // Production Facilities
            EditorGUILayout.LabelField("Production Facilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "List of facility IDs/types that can produce/extract this material. " +
                "Examples: 'Forge', 'Workshop', 'Apothecary', 'Mine', 'Quarry'. " +
                "Empty list = can be produced anywhere, gathered from environment, or no production facility required.",
                MessageType.Info
            );
            if (template.productionFacilities == null)
                template.productionFacilities = new List<string>();
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.productionFacilities.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.productionFacilities[i] = EditorGUILayout.TextField($"Facility {i + 1}", template.productionFacilities[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.productionFacilities.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Facility"))
            {
                template.productionFacilities.Add("");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Material attributes
            if (template.possibleAttributes == null)
                template.possibleAttributes = new List<MaterialAttribute>();
            ProductionChainEditor.DrawMaterialAttributesEditor(template.possibleAttributes);
            
            EditorGUILayout.Space();
            if (template.bonuses == null)
                template.bonuses = new List<PrefabBonus>();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            if (template.visualPresentation == null)
                template.visualPresentation = new VisualPresentation();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            if (template.vfxPresentation == null)
                template.vfxPresentation = new VFXPresentation();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
        }

        private void EditToolTemplate(ToolTemplate template)
        {
            if (template == null) return;
            
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            
            EditorGUILayout.Space();
            
            // Unified Quality / Rarity / Tech Tier (per items.md spec)
            EditorGUILayout.LabelField("Quality & Rarity", EditorStyles.boldLabel);
            template.quality = EditorGUILayout.Slider("Base Quality", template.quality, 0f, 100f);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Calculated Quality", template.calculatedQuality);
            EditorGUI.EndDisabledGroup();
            template.rarity = (Rarity)EditorGUILayout.EnumPopup("Base Rarity", template.rarity);
            template.techTier = (byte)EditorGUILayout.IntSlider("Tech Tier", template.techTier, 0, 10);
            template.requiredTechTier = (byte)EditorGUILayout.IntSlider("Required Tech Tier", template.requiredTechTier, 0, 10);
            
            EditorGUILayout.Space();
            
            // Catalog IDs (per items.md spec)
            EditorGUILayout.LabelField("Catalog IDs", EditorStyles.boldLabel);
            template.itemDefId = new ItemDefId((ushort)EditorGUILayout.IntField("ItemDefId", template.itemDefId.Value));
            template.recipeId = new RecipeId((ushort)EditorGUILayout.IntField("RecipeId", template.recipeId.Value));
            EditorGUILayout.HelpBox(
                "Runtime IDs (ushort). Set during blob baking. " +
                "Editor uses these for validation and export.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            // Production chain
            EditorGUILayout.LabelField("Production Chain", EditorStyles.boldLabel);
            template.producedFrom = EditorGUILayout.TextField("Produced From", template.producedFrom);
            EditorGUILayout.HelpBox(
                "Name of the material/tool this is produced from (e.g., 'Iron_Ingot'). " +
                "Leave empty if this is a base material.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            ProductionChainEditor.DrawProductionInputsEditor(template.productionInputs, materialTemplates);
            
            EditorGUILayout.Space();
            
            // Recipe metadata (per items.md spec)
            template.baseFacilityQuality = EditorGUILayout.Slider(
                "Base Facility Quality", 
                template.baseFacilityQuality, 
                0f, 
                100f
            );
            EditorGUILayout.HelpBox(
                "Base facility quality for this recipe (0-100). " +
                "Used in quality calculation per items.md spec.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            // Quality derivation
            ProductionChainEditor.DrawQualityDerivationEditor(template.qualityDerivation);
            
            // Preview calculated quality (using example values)
            EditorGUI.BeginDisabledGroup(true);
            float previewQuality = ProductionChainEditor.CalculateQuality(
                template.qualityDerivation,
                75f, // Example material purity
                60f, // Example material quality
                50f, // Example craftsman skill
                40f  // Example forge quality
            );
            EditorGUILayout.FloatField("Preview Quality (example)", previewQuality);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // Material attributes
            if (template.possibleAttributes == null)
                template.possibleAttributes = new List<MaterialAttribute>();
            ProductionChainEditor.DrawMaterialAttributesEditor(template.possibleAttributes);
            
            EditorGUILayout.Space();
            
            // Production Facilities
            EditorGUILayout.LabelField("Production Facilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "List of facility IDs/types that can produce this tool. " +
                "Examples: 'Forge', 'Workshop', 'CarpenterShop', 'Smithy'. " +
                "Empty list = can be produced anywhere or no production facility required.",
                MessageType.Info
            );
            if (template.productionFacilities == null)
                template.productionFacilities = new List<string>();
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.productionFacilities.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.productionFacilities[i] = EditorGUILayout.TextField($"Facility {i + 1}", template.productionFacilities[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.productionFacilities.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Facility"))
            {
                template.productionFacilities.Add("");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Tool properties
            EditorGUILayout.LabelField("Tool Properties", EditorStyles.boldLabel);
            template.constructionSpeedBonus = EditorGUILayout.FloatField("Construction Speed Bonus", template.constructionSpeedBonus);
            template.durabilityBonus = EditorGUILayout.FloatField("Durability Bonus", template.durabilityBonus);
            template.workEfficiency = EditorGUILayout.FloatField("Work Efficiency Multiplier", template.workEfficiency);
            
            EditorGUILayout.Space();
            if (template.bonuses == null)
                template.bonuses = new List<PrefabBonus>();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            if (template.visualPresentation == null)
                template.visualPresentation = new VisualPresentation();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            if (template.vfxPresentation == null)
                template.vfxPresentation = new VFXPresentation();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
        }

        private void EditReagentTemplate(ReagentTemplate template)
        {
            if (template == null) return;
            
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            
            EditorGUILayout.Space();
            
            // Reagent properties
            EditorGUILayout.LabelField("Reagent Properties", EditorStyles.boldLabel);
            template.potency = EditorGUILayout.Slider("Potency", template.potency, 0f, 100f);
            template.rarity = EditorGUILayout.FloatField("Rarity", template.rarity);
            
            EditorGUILayout.Space();
            
            // Production Facilities
            EditorGUILayout.LabelField("Production Facilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "List of facility IDs/types that can produce this reagent. " +
                "Examples: 'Apothecary', 'AlchemyLab', 'Workshop', 'Temple'. " +
                "Empty list = can be produced anywhere or no production facility required.",
                MessageType.Info
            );
            if (template.productionFacilities == null)
                template.productionFacilities = new List<string>();
            EditorGUI.indentLevel++;
            for (int i = 0; i < template.productionFacilities.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.productionFacilities[i] = EditorGUILayout.TextField($"Facility {i + 1}", template.productionFacilities[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.productionFacilities.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Facility"))
            {
                template.productionFacilities.Add("");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            if (template.bonuses == null)
                template.bonuses = new List<PrefabBonus>();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            if (template.visualPresentation == null)
                template.visualPresentation = new VisualPresentation();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            if (template.vfxPresentation == null)
                template.vfxPresentation = new VFXPresentation();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
        }

        private void EditMiracleTemplate(MiracleTemplate template)
        {
            if (template == null) return;
            
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.description = EditorGUILayout.TextField("Description", template.description);
            
            EditorGUILayout.Space();
            
            // Miracle Effect Logic Editor
            MiracleEffectEditor.DrawMiracleEffectsEditor(template);
            
            EditorGUILayout.Space();
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
        }

        public void GenerateBuildingPrefab(BuildingTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateBuildingPrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Buildings", template.name);
            Debug.Log($"Generated building prefab: {path}");
            AssetDatabase.Refresh();
        }

        public void GenerateIndividualPrefab(IndividualTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateIndividualPrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Individuals", template.name);
            Debug.Log($"Generated individual prefab: {path}");
            AssetDatabase.Refresh();
        }

        public void GenerateEquipmentPrefab(EquipmentTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateEquipmentPrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Equipment", template.name);
            Debug.Log($"Generated equipment prefab: {path}");
            AssetDatabase.Refresh();
        }

        public void GenerateMaterialPrefab(MaterialTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateMaterialPrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Materials", template.name);
            Debug.Log($"Generated material prefab: {path}");
            AssetDatabase.Refresh();
        }

        public void GenerateToolPrefab(ToolTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateToolPrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Tools", template.name);
            Debug.Log($"Generated tool prefab: {path}");
            AssetDatabase.Refresh();
        }

        public void GenerateReagentPrefab(ReagentTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateReagentPrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Reagents", template.name);
            Debug.Log($"Generated reagent prefab: {path}");
            AssetDatabase.Refresh();
        }

        public void GenerateMiraclePrefab(MiracleTemplate template)
        {
            GameObject go = PrefabGenerator.GenerateMiraclePrefab(template);
            string path = PrefabGenerator.SavePrefab(go, "Miracles", template.name);
            Debug.Log($"Generated miracle prefab: {path}");
            AssetDatabase.Refresh();
        }

        private void GenerateAllPrefabs()
        {
            int count = 0;
            foreach (var template in buildingTemplates)
            {
                GenerateBuildingPrefab(template);
                count++;
            }
            foreach (var template in individualTemplates)
            {
                GenerateIndividualPrefab(template);
                count++;
            }
            foreach (var template in equipmentTemplates)
            {
                GenerateEquipmentPrefab(template);
                count++;
            }
            foreach (var template in materialTemplates)
            {
                GenerateMaterialPrefab(template);
                count++;
            }
            foreach (var template in toolTemplates)
            {
                GenerateToolPrefab(template);
                count++;
            }
            foreach (var template in reagentTemplates)
            {
                GenerateReagentPrefab(template);
                count++;
            }
            foreach (var template in miracleTemplates)
            {
                GenerateMiraclePrefab(template);
                count++;
            }
            Debug.Log($"Generated {count} prefabs");
        }

        private void ValidateAllTemplates()
        {
            List<string> allErrors = new List<string>();
            List<string> allWarnings = new List<string>();

            // Validate buildings
            foreach (var template in buildingTemplates)
            {
                var result = PrefabValidator.ValidateBuilding(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
            }

            // Validate individuals
            foreach (var template in individualTemplates)
            {
                var result = PrefabValidator.ValidateIndividual(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
            }

            // Validate equipment
            foreach (var template in equipmentTemplates)
            {
                var result = PrefabValidator.ValidateEquipment(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
            }

            // Validate materials
            foreach (var template in materialTemplates)
            {
                var result = PrefabValidator.ValidateMaterial(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
            }

            // Validate tools (including production chain validation)
            foreach (var template in toolTemplates)
            {
                var result = PrefabValidator.ValidateTool(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
                
                // Also validate production chain
                var chainResult = ProductionChainValidator.ValidateToolProductionChain(
                    template,
                    materialTemplates,
                    toolTemplates
                );
                allErrors.AddRange(chainResult.Errors);
                allWarnings.AddRange(chainResult.Warnings);
            }

            // Validate reagents
            foreach (var template in reagentTemplates)
            {
                var result = PrefabValidator.ValidateReagent(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
            }

            // Validate miracles
            foreach (var template in miracleTemplates)
            {
                var result = PrefabValidator.ValidateMiracle(template);
                allErrors.AddRange(result.Errors);
                allWarnings.AddRange(result.Warnings);
            }

            // Validate ID uniqueness across all types
            var allTemplates = new List<PrefabTemplate>();
            allTemplates.AddRange(buildingTemplates.Cast<PrefabTemplate>());
            allTemplates.AddRange(individualTemplates.Cast<PrefabTemplate>());
            allTemplates.AddRange(equipmentTemplates.Cast<PrefabTemplate>());
            allTemplates.AddRange(materialTemplates.Cast<PrefabTemplate>());
            allTemplates.AddRange(toolTemplates.Cast<PrefabTemplate>());
            allTemplates.AddRange(reagentTemplates.Cast<PrefabTemplate>());
            allTemplates.AddRange(miracleTemplates.Cast<PrefabTemplate>());

            var idResult = PrefabValidator.ValidateIdUniqueness(allTemplates);
            allErrors.AddRange(idResult.Errors);
            allWarnings.AddRange(idResult.Warnings);

            // Report results
            if (allErrors.Count > 0)
            {
                Debug.LogError($"Validation found {allErrors.Count} errors:\n" + string.Join("\n", allErrors));
            }
            if (allWarnings.Count > 0)
            {
                Debug.LogWarning($"Validation found {allWarnings.Count} warnings:\n" + string.Join("\n", allWarnings));
            }
            if (allErrors.Count == 0 && allWarnings.Count == 0)
            {
                Debug.Log("All templates validated successfully!");
            }
        }

        public void LoadDefaultTemplates()
        {
            // Load default templates (could be from ScriptableObjects or JSON)
            // For now, create some defaults

            // Buildings
            var storehouse = new BuildingTemplate
            {
                name = "Storehouse_Basic",
                displayName = "Basic Storehouse",
                id = 1001,
                buildingType = BuildingTemplate.BuildingType.Storage,
                baseHealth = 100f,
                baseDesirability = 30f,
                storageCapacity = 500f,
                materials = new List<MaterialQuality>
                {
                    new MaterialQuality { materialName = "Wood", qualityMultiplier = 1f, healthBonus = 0f }
                }
            };
            // Ensure visual presentation placeholder is enabled
            if (storehouse.visualPresentation == null)
                storehouse.visualPresentation = new VisualPresentation();
            storehouse.visualPresentation.usePrimitiveFallback = true;
            storehouse.visualPresentation.primitiveType = PrimitiveType.Cube;
            buildingTemplates.Add(storehouse);

            // Fertility Statue example
            var fertilityStatue = new BuildingTemplate
            {
                name = "Fertility_Statue",
                displayName = "Fertility Statue",
                id = 1002,
                buildingType = BuildingTemplate.BuildingType.Utility,
                baseHealth = 50f,
                baseDesirability = 20f,
                areaBonusRange = 15f,
                bonusValue = 15f,
                bonusType = "fertility"
            };
            // Ensure visual presentation placeholder is enabled
            if (fertilityStatue.visualPresentation == null)
                fertilityStatue.visualPresentation = new VisualPresentation();
            fertilityStatue.visualPresentation.usePrimitiveFallback = true;
            fertilityStatue.visualPresentation.primitiveType = PrimitiveType.Cylinder; // Statue-like shape
            fertilityStatue.bonuses.Add(new PrefabBonus
            {
                bonusType = BonusType.Fertility,
                bonusValue = 15f,
                isPercentage = true,
                radius = 15f,
                useFalloff = true,
                falloffRate = 1f,
                affectsAllies = true,
                affectsSelf = false,
                targetTags = new List<string> { "Villager" },
                isPermanent = true,
                stacks = true,
                displayName = "Fertility Aura",
                description = "Increases pregnancy chance for nearby villagers"
            });
            buildingTemplates.Add(fertilityStatue);

            // Temple example
            var temple = new BuildingTemplate
            {
                name = "Temple_Basic",
                displayName = "Basic Temple",
                id = 1003,
                buildingType = BuildingTemplate.BuildingType.Worship,
                baseHealth = 150f,
                baseDesirability = 50f,
                manaGenerationRate = 10f,
                worshipperCapacity = 20f
            };
            // Ensure visual presentation placeholder is enabled
            if (temple.visualPresentation == null)
                temple.visualPresentation = new VisualPresentation();
            temple.visualPresentation.usePrimitiveFallback = true;
            temple.visualPresentation.primitiveType = PrimitiveType.Cube;
            temple.visualPresentation.scale = new Vector3(2f, 3f, 2f); // Taller building
            temple.bonuses.Add(new PrefabBonus
            {
                bonusType = BonusType.Happiness,
                bonusValue = 10f,
                isPercentage = false,
                radius = 20f,
                useFalloff = true,
                falloffRate = 0.8f,
                affectsAllies = true,
                affectsSelf = false,
                targetTags = new List<string> { "Villager" },
                isPermanent = true,
                stacks = true,
                displayName = "Divine Presence",
                description = "Increases happiness for nearby villagers"
            });
            temple.bonuses.Add(new PrefabBonus
            {
                bonusType = BonusType.ManaGeneration,
                bonusValue = 5f,
                isPercentage = false,
                radius = 0f, // Self only
                affectsSelf = true,
                affectsAllies = false,
                isPermanent = true,
                stacks = false,
                displayName = "Mana Generation",
                description = "Generates mana over time"
            });
            buildingTemplates.Add(temple);

            // Individuals
            individualTemplates.Add(new IndividualTemplate
            {
                name = "Villager_Basic",
                displayName = "Basic Villager",
                id = 1,
                individualType = IndividualTemplate.IndividualType.Villager,
                factionId = 0,
                baseHealth = 100f,
                baseSpeed = 5f
            });

            // Equipment
            var ironSword = new EquipmentTemplate
            {
                name = "Sword_Iron",
                displayName = "Iron Sword",
                id = 2001,
                equipmentType = EquipmentTemplate.EquipmentType.Weapon,
                slotKind = SlotKind.Hand,
                baseDurability = 100f,
                requiredUsage = MaterialUsage.Weapon,
                minStats = new MaterialStats { hardness = 50f, toughness = 40f },
                stats = new EquipmentStats
                {
                    damage = 25f,
                    critChance = 5f,
                    critDamage = 1.5f,
                    weight = 2.5f
                }
            };
            equipmentTemplates.Add(ironSword);

            var ironArmor = new EquipmentTemplate
            {
                name = "Armor_Iron",
                displayName = "Iron Armor",
                id = 2002,
                equipmentType = EquipmentTemplate.EquipmentType.Armor,
                slotKind = SlotKind.Body,
                baseDurability = 150f,
                requiredUsage = MaterialUsage.Armor,
                minStats = new MaterialStats { hardness = 60f, toughness = 50f },
                stats = new EquipmentStats
                {
                    armor = 30f,
                    blockChance = 10f,
                    weight = 15f,
                    encumbrance = 15f
                }
            };
            equipmentTemplates.Add(ironArmor);

            // Materials
            materialTemplates.Add(new MaterialTemplate
            {
                name = "Wood_Oak",
                displayName = "Oak Wood",
                id = 3001,
                category = MaterialCategory.Raw,
                usage = MaterialUsage.Building | MaterialUsage.Fuel,
                traits = MaterialTraits.Flammable | MaterialTraits.Flexible,
                baseQuality = 60f,
                stats = new MaterialStats { hardness = 30f, toughness = 40f, density = 0.7f }
            });

            materialTemplates.Add(new MaterialTemplate
            {
                name = "Stone_Granite",
                displayName = "Granite Stone",
                id = 3002,
                category = MaterialCategory.Raw,
                usage = MaterialUsage.Building,
                traits = MaterialTraits.Hard | MaterialTraits.Rigid | MaterialTraits.Fireproof,
                baseQuality = 70f,
                stats = new MaterialStats { hardness = 80f, toughness = 60f, density = 2.7f }
            });

            var ironIngot = new MaterialTemplate
            {
                name = "Iron_Ingot",
                displayName = "Iron Ingot",
                id = 3003,
                category = MaterialCategory.Extracted,
                usage = MaterialUsage.Building | MaterialUsage.Weapon | MaterialUsage.Tool,
                traits = MaterialTraits.Ductile | MaterialTraits.Hard | MaterialTraits.Conductive,
                baseQuality = 65f,
                purity = 85f,
                stats = new MaterialStats { hardness = 60f, toughness = 50f, density = 7.8f, meltingPoint = 1538f },
                usedInProduction = new List<string> { "Iron_Screws", "Iron_Mace", "Hammer_Iron", "Sword_Iron" },
                possibleAttributes = new List<MaterialAttribute>
                {
                    new MaterialAttribute
                    {
                        name = "IncreasedDurability",
                        value = 15f,
                        isPercentage = true,
                        minCraftsmanSkill = 70f,
                        chanceToAdd = 0.3f
                    },
                    new MaterialAttribute
                    {
                        name = "SharpEdge",
                        value = 5f,
                        isPercentage = false,
                        minCraftsmanSkill = 80f,
                        chanceToAdd = 0.2f
                    }
                }
            };
            materialTemplates.Add(ironIngot);

            // Tools - Production chain example: Iron → Iron Screws → Iron Mace
            var ironScrews = new ToolTemplate
            {
                name = "Iron_Screws",
                displayName = "Iron Screws",
                id = 4001,
                baseQuality = 50f,
                producedFrom = "Iron_Ingot",
                productionInputs = new List<ProductionInput>
                {
                    new ProductionInput
                    {
                        materialName = "Iron_Ingot",
                        quantity = 1f,
                        minPurity = 50f,
                        minQuality = 0f
                    }
                },
                qualityDerivation = new QualityDerivation
                {
                    materialPurityWeight = 0.4f,
                    materialQualityWeight = 0.3f,
                    craftsmanSkillWeight = 0.2f,
                    forgeQualityWeight = 0.1f,
                    baseQualityMultiplier = 1f,
                    minQuality = 0f,
                    maxQuality = 100f
                },
                possibleAttributes = new List<MaterialAttribute>
                {
                    new MaterialAttribute
                    {
                        name = "IncreasedDurability",
                        value = 10f,
                        isPercentage = true,
                        minCraftsmanSkill = 60f,
                        chanceToAdd = 0.5f
                    }
                },
                workEfficiency = 1f
            };
            toolTemplates.Add(ironScrews);

            var ironMace = new ToolTemplate
            {
                name = "Mace_Iron",
                displayName = "Iron Mace",
                id = 4002,
                baseQuality = 60f,
                producedFrom = "Iron_Screws",
                productionInputs = new List<ProductionInput>
                {
                    new ProductionInput
                    {
                        materialName = "Iron_Screws",
                        quantity = 5f,
                        minPurity = 60f,
                        minQuality = 40f
                    },
                    new ProductionInput
                    {
                        materialName = "Iron_Ingot",
                        quantity = 2f,
                        minPurity = 70f,
                        minQuality = 50f
                    }
                },
                qualityDerivation = new QualityDerivation
                {
                    materialPurityWeight = 0.3f,
                    materialQualityWeight = 0.4f,
                    craftsmanSkillWeight = 0.25f,
                    forgeQualityWeight = 0.05f,
                    baseQualityMultiplier = 1f,
                    minQuality = 20f,
                    maxQuality = 100f
                },
                workEfficiency = 1.2f
            };
            toolTemplates.Add(ironMace);

            var ironHammer = new ToolTemplate
            {
                name = "Hammer_Iron",
                displayName = "Iron Hammer",
                id = 4003,
                baseQuality = 60f,
                producedFrom = "Iron_Ingot",
                productionInputs = new List<ProductionInput>
                {
                    new ProductionInput
                    {
                        materialName = "Iron_Ingot",
                        quantity = 1f,
                        minPurity = 50f
                    }
                },
                qualityDerivation = new QualityDerivation
                {
                    materialPurityWeight = 0.4f,
                    materialQualityWeight = 0.3f,
                    craftsmanSkillWeight = 0.2f,
                    forgeQualityWeight = 0.1f
                },
                constructionSpeedBonus = 10f,
                durabilityBonus = 5f,
                workEfficiency = 1.1f
            };
            toolTemplates.Add(ironHammer);

            // Reagents
            reagentTemplates.Add(new ReagentTemplate
            {
                name = "Herb_Common",
                displayName = "Common Herb",
                id = 5001,
                potency = 30f,
                rarity = 10f
            });

            // Miracles
            miracleTemplates.Add(new MiracleTemplate
            {
                name = "Heal_Minor",
                displayName = "Minor Heal",
                id = 6001,
                manaCost = 10f,
                cooldown = 5f,
                range = 10f,
                duration = 0f,
                areaShape = AreaShape.Point
            });
            
            // Migrate all templates after loading defaults
            MigrateAllTemplatesInternal();
        }

        /// <summary>
        /// Internal migration method called on window enable and after loading defaults.
        /// </summary>
        private void MigrateAllTemplatesInternal()
        {
            int migrated = 0;
            migrated += PrefabTemplateMigrator.MigrateTemplates(buildingTemplates);
            migrated += PrefabTemplateMigrator.MigrateTemplates(individualTemplates);
            migrated += PrefabTemplateMigrator.MigrateTemplates(equipmentTemplates);
            migrated += PrefabTemplateMigrator.MigrateTemplates(materialTemplates);
            migrated += PrefabTemplateMigrator.MigrateTemplates(toolTemplates);
            migrated += PrefabTemplateMigrator.MigrateTemplates(reagentTemplates);
            migrated += PrefabTemplateMigrator.MigrateTemplates(miracleTemplates);
            
            if (migrated > 0)
            {
                Debug.Log($"Prefab Editor: Migrated {migrated} template(s) to current schema version.");
            }
        }

        private int GetNextId<T>(List<T> templates) where T : PrefabTemplate
        {
            if (templates.Count == 0) return 1;
            return templates.Max(t => t.id) + 1;
        }

        private void PerformDryRun()
        {
            // Initialize material catalog and rules
            var materialCatalog = materialTemplates;
            var rules = new List<MaterialRule>(); // Could load from ScriptableObject

            // Generate dry-run reports for all types
            var buildingReport = DryRunGenerator.GenerateDryRunBuildings(buildingTemplates, materialCatalog, rules);
            var equipmentReport = DryRunGenerator.GenerateDryRunEquipment(equipmentTemplates, materialCatalog, rules);
            var materialReport = DryRunGenerator.GenerateDryRunMaterials(materialTemplates, rules);
            var individualReport = DryRunGenerator.GenerateDryRunIndividuals(individualTemplates);
            var toolReport = DryRunGenerator.GenerateDryRunTools(toolTemplates, materialCatalog);

            // Display summary
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("=== BUILDINGS ===");
            summary.AppendLine(DryRunGenerator.GenerateDiffSummary(buildingReport));
            summary.AppendLine();
            summary.AppendLine("=== EQUIPMENT ===");
            summary.AppendLine(DryRunGenerator.GenerateDiffSummary(equipmentReport));
            summary.AppendLine();
            summary.AppendLine("=== MATERIALS ===");
            summary.AppendLine(DryRunGenerator.GenerateDiffSummary(materialReport));
            summary.AppendLine();
            summary.AppendLine("=== INDIVIDUALS ===");
            summary.AppendLine(DryRunGenerator.GenerateDiffSummary(individualReport));
            summary.AppendLine();
            summary.AppendLine("=== TOOLS ===");
            summary.AppendLine(DryRunGenerator.GenerateDiffSummary(toolReport));

            // Show in console
            Debug.Log(summary.ToString());

            // Export JSON
            var json = DryRunGenerator.ExportToJson(buildingReport);
            var jsonPath = System.IO.Path.Combine(Application.dataPath, "..", "Logs", "dry-run-report.json");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(jsonPath));
            System.IO.File.WriteAllText(jsonPath, json);
            Debug.Log($"Dry-run JSON exported to: {jsonPath}");

            // Calculate totals
            int totalPrefabs = buildingReport.totalPrefabs + equipmentReport.totalPrefabs + 
                              materialReport.totalPrefabs + individualReport.totalPrefabs + toolReport.totalPrefabs;
            int totalCreate = buildingReport.wouldCreate + equipmentReport.wouldCreate + 
                             materialReport.wouldCreate + individualReport.wouldCreate + toolReport.wouldCreate;
            int totalErrors = buildingReport.errors + equipmentReport.errors + 
                             materialReport.errors + individualReport.errors + toolReport.errors;

            EditorUtility.DisplayDialog("Dry-Run Complete", 
                $"Total Prefabs: {totalPrefabs}\n" +
                $"Would Create: {totalCreate}\n" +
                $"Errors: {totalErrors}\n\n" +
                $"Buildings: {buildingReport.totalPrefabs}\n" +
                $"Equipment: {equipmentReport.totalPrefabs}\n" +
                $"Materials: {materialReport.totalPrefabs}\n" +
                $"Individuals: {individualReport.totalPrefabs}\n\n" +
                "Check Console and Logs/dry-run-report.json for details.", 
                "OK");
        }
    }
}
