using System.IO;
using UnityEditor;
using UnityEngine;
using Godgame.Authoring;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Core prefab generation logic.
    /// </summary>
    public static class PrefabGenerator
    {
        private const string PrefabRootPath = "Assets/Prefabs";

        /// <summary>
        /// Creates prefab folder structure if it doesn't exist.
        /// </summary>
        public static void EnsureFolderStructure()
        {
            if (!AssetDatabase.IsValidFolder(PrefabRootPath))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            string[] folders = {
                $"{PrefabRootPath}/Buildings",
                $"{PrefabRootPath}/Individuals",
                $"{PrefabRootPath}/Equipment",
                $"{PrefabRootPath}/Materials",
                $"{PrefabRootPath}/Tools",
                $"{PrefabRootPath}/Reagents",
                $"{PrefabRootPath}/Miracles"
            };

            foreach (var folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                    if (!AssetDatabase.IsValidFolder(parent))
                    {
                        var parentName = Path.GetFileName(parent);
                        var rootParent = Path.GetDirectoryName(parent).Replace('\\', '/');
                        if (!AssetDatabase.IsValidFolder(rootParent))
                        {
                            rootParent = "Assets";
                        }
                        AssetDatabase.CreateFolder(rootParent, parentName);
                    }
                    string folderName = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        /// <summary>
        /// Generates a building prefab from template.
        /// </summary>
        public static GameObject GenerateBuildingPrefab(BuildingTemplate template)
        {
            // Calculate derived stats
            template.calculatedHealth = StatCalculation.CalculateBuildingHealth(template);
            template.calculatedDesirability = StatCalculation.CalculateBuildingDesirability(template);

            // Create GameObject
            GameObject go = new GameObject(template.displayName ?? template.name);
            
            // Add appropriate authoring component based on building type
            switch (template.buildingType)
            {
                case BuildingTemplate.BuildingType.Storage:
                    var storehouse = go.AddComponent<StorehouseAuthoring>();
                    storehouse.SetPrivateField("label", template.displayName ?? template.name);
                    storehouse.SetPrivateField("storehouseId", template.id);
                    storehouse.SetPrivateField("totalCapacity", template.storageCapacity);
                    break;

                // Add other building types as authoring components are created
                // case BuildingTemplate.BuildingType.Residence:
                //     var housing = go.AddComponent<HousingAuthoring>();
                //     housing.SetPrivateField("maxResidents", template.maxResidents);
                //     break;
            }

            // Add visual presentation
            AddVisualPresentation(go, template, template.buildingType);
            
            // Add VFX presentation
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Generates an individual entity prefab from template.
        /// </summary>
        public static GameObject GenerateIndividualPrefab(IndividualTemplate template)
        {
            GameObject go = new GameObject(template.displayName ?? template.name);

            switch (template.individualType)
            {
                case IndividualTemplate.IndividualType.Villager:
                    var villager = go.AddComponent<VillagerAuthoring>();
                    villager.SetPrivateField("villagerId", template.id);
                    villager.SetPrivateField("factionId", template.factionId);
                    villager.SetPrivateField("vengefulScore", template.vengefulScore);
                    villager.SetPrivateField("boldScore", template.boldScore);
                    villager.SetPrivateField("isUndead", template.isUndead);
                    villager.SetPrivateField("isSummoned", template.isSummoned);
                    
                    // Core Attributes
                    villager.SetPrivateField("physique", template.physique);
                    villager.SetPrivateField("finesse", template.finesse);
                    villager.SetPrivateField("will", template.will);
                    villager.SetPrivateField("wisdom", template.wisdom);
                    
                    // Derived Attributes
                    villager.SetPrivateField("strength", template.strength);
                    villager.SetPrivateField("agility", template.agility);
                    villager.SetPrivateField("intelligence", template.intelligence);
                    
                    // Social Stats
                    villager.SetPrivateField("fame", template.fame);
                    villager.SetPrivateField("wealth", template.wealth);
                    villager.SetPrivateField("reputation", template.reputation);
                    villager.SetPrivateField("glory", template.glory);
                    villager.SetPrivateField("renown", template.renown);
                    
                    // Combat Stats
                    villager.SetPrivateField("baseAttack", template.baseAttack);
                    villager.SetPrivateField("baseDefense", template.baseDefense);
                    villager.SetPrivateField("baseHealthOverride", template.baseHealthOverride);
                    villager.SetPrivateField("baseHealth", template.baseHealth);
                    villager.SetPrivateField("baseStamina", template.baseStamina);
                    villager.SetPrivateField("baseMana", template.baseMana);
                    
                    // Need Stats
                    villager.SetPrivateField("food", template.food);
                    villager.SetPrivateField("rest", template.rest);
                    villager.SetPrivateField("sleep", template.sleep);
                    villager.SetPrivateField("generalHealth", template.generalHealth);
                    
                    // Resistances (convert dictionary to individual fields)
                    if (template.resistances != null)
                    {
                        villager.SetPrivateField("physicalResistance", template.resistances.ContainsKey("Physical") ? template.resistances["Physical"] : 0f);
                        villager.SetPrivateField("fireResistance", template.resistances.ContainsKey("Fire") ? template.resistances["Fire"] : 0f);
                        villager.SetPrivateField("coldResistance", template.resistances.ContainsKey("Cold") ? template.resistances["Cold"] : 0f);
                        villager.SetPrivateField("poisonResistance", template.resistances.ContainsKey("Poison") ? template.resistances["Poison"] : 0f);
                        villager.SetPrivateField("magicResistance", template.resistances.ContainsKey("Magic") ? template.resistances["Magic"] : 0f);
                        villager.SetPrivateField("lightningResistance", template.resistances.ContainsKey("Lightning") ? template.resistances["Lightning"] : 0f);
                        villager.SetPrivateField("holyResistance", template.resistances.ContainsKey("Holy") ? template.resistances["Holy"] : 0f);
                        villager.SetPrivateField("darkResistance", template.resistances.ContainsKey("Dark") ? template.resistances["Dark"] : 0f);
                    }
                    
                    // Modifiers
                    villager.SetPrivateField("healBonus", template.healBonus);
                    villager.SetPrivateField("spellDurationModifier", template.spellDurationModifier);
                    villager.SetPrivateField("spellIntensityModifier", template.spellIntensityModifier);
                    break;
            }

            AddVisualPresentation(go, template, template.individualType);
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Generates a material prefab from template.
        /// </summary>
        public static GameObject GenerateMaterialPrefab(MaterialTemplate template)
        {
            GameObject go = new GameObject(template.displayName ?? template.name);
            
            // Materials typically don't need authoring components (they're resources)
            // But we can add a simple component for identification if needed
            
            AddVisualPresentation(go, template, template.category);
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Generates an equipment prefab from template.
        /// </summary>
        public static GameObject GenerateEquipmentPrefab(EquipmentTemplate template)
        {
            GameObject go = new GameObject(template.displayName ?? template.name);
            
            // Calculate derived stats (would use actual material if available)
            template.calculatedDurability = StatCalculation.CalculateEquipmentDurability(template);
            template.stats.calculatedDamage = StatCalculation.CalculateEquipmentDamage(template);
            template.stats.calculatedArmor = StatCalculation.CalculateEquipmentArmor(template);
            template.stats.weight = StatCalculation.CalculateEquipmentWeight(template);
            
            // Equipment would use ModuleDefinitionAuthoring or similar
            // For now, just create the GameObject
            
            AddVisualPresentation(go, template, template.equipmentType);
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Generates a tool prefab from template.
        /// </summary>
        public static GameObject GenerateToolPrefab(ToolTemplate template)
        {
            GameObject go = new GameObject(template.displayName ?? template.name);
            
            // Calculate quality (would use actual material purity/quality and craftsman skill if available)
            // For now, use base quality
            if (template.qualityDerivation != null)
            {
                // Example calculation with placeholder values
                template.calculatedQuality = ProductionChainEditor.CalculateQuality(
                    template.qualityDerivation,
                    75f, // Material purity (would come from actual material)
                    60f, // Material quality (would come from actual material)
                    50f, // Craftsman skill (would come from runtime)
                    40f  // Forge quality (would come from runtime)
                );
            }
            else
            {
                template.calculatedQuality = StatCalculation.CalculateToolQuality(template);
            }
            
            AddVisualPresentation(go, template, "Tool");
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Generates a reagent prefab from template.
        /// </summary>
        public static GameObject GenerateReagentPrefab(ReagentTemplate template)
        {
            GameObject go = new GameObject(template.displayName ?? template.name);
            
            AddVisualPresentation(go, template, "Reagent");
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Generates a miracle prefab from template.
        /// </summary>
        public static GameObject GenerateMiraclePrefab(MiracleTemplate template)
        {
            GameObject go = new GameObject(template.displayName ?? template.name);
            
            // Miracles would use a MiracleAuthoring component when created
            // For now, just create the GameObject
            
            AddVisualPresentation(go, template, "Miracle");
            AddVFXPresentation(go, template);

            return go;
        }

        /// <summary>
        /// Saves a GameObject as a prefab asset.
        /// </summary>
        public static string SavePrefab(GameObject go, string category, string name)
        {
            EnsureFolderStructure();
            string path = $"{PrefabRootPath}/{category}/{name}.prefab";
            
            // Delete existing prefab if it exists
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            return path;
        }

        /// <summary>
        /// Adds visual presentation to the GameObject from template.
        /// </summary>
        private static void AddVisualPresentation(GameObject parent, PrefabTemplate template, object category)
        {
            if (template == null || parent == null) return;
            
            GameObject visual = null;
            var presentation = template.visualPresentation;
            
            // Ensure presentation exists (migration safety)
            if (presentation == null)
            {
                presentation = new VisualPresentation();
                template.visualPresentation = presentation;
            }
            
            // Ensure primitive fallback is enabled for demo visibility
            if (!presentation.usePrimitiveFallback && 
                presentation.prefabAsset == null && 
                presentation.meshAsset == null && 
                presentation.spriteAsset == null)
            {
                presentation.usePrimitiveFallback = true;
            }

            // Try to use assigned visual asset
            if (presentation.prefabAsset != null)
            {
                // Instantiate prefab asset
                visual = Object.Instantiate(presentation.prefabAsset);
                visual.name = "Visual";
                visual.transform.SetParent(parent.transform);
                visual.transform.localPosition = presentation.positionOffset;
                visual.transform.localRotation = Quaternion.Euler(presentation.rotationOffset);
                visual.transform.localScale = presentation.scale;
                
                // Apply material override if specified
                if (presentation.materialOverride != null)
                {
                    var renderers = visual.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.material = presentation.materialOverride;
                    }
                }
            }
            else if (presentation.meshAsset != null)
            {
                // Use mesh asset directly
                visual = new GameObject("Visual");
                visual.transform.SetParent(parent.transform);
                visual.transform.localPosition = presentation.positionOffset;
                visual.transform.localRotation = Quaternion.Euler(presentation.rotationOffset);
                visual.transform.localScale = presentation.scale;
                
                var meshFilter = visual.AddComponent<MeshFilter>();
                meshFilter.mesh = presentation.meshAsset;
                
                var meshRenderer = visual.AddComponent<MeshRenderer>();
                if (presentation.materialOverride != null)
                {
                    meshRenderer.material = presentation.materialOverride;
                }
                else
                {
                    meshRenderer.material = new Material(Shader.Find("Standard"));
                }
            }
            else if (presentation.spriteAsset != null)
            {
                // Use sprite asset (2D)
                visual = new GameObject("Visual");
                visual.transform.SetParent(parent.transform);
                visual.transform.localPosition = presentation.positionOffset;
                visual.transform.localRotation = Quaternion.Euler(presentation.rotationOffset);
                visual.transform.localScale = presentation.scale;
                
                var spriteRenderer = visual.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = presentation.spriteAsset;
            }
            else if (presentation.usePrimitiveFallback)
            {
                // Fallback to primitive
                visual = CreatePrimitiveVisual(parent, category, presentation);
            }

            if (visual != null)
            {
                // Remove colliders (not needed for ECS)
                var colliders = visual.GetComponentsInChildren<Collider>();
                foreach (var collider in colliders)
                {
                    Object.DestroyImmediate(collider);
                }
            }
        }

        /// <summary>
        /// Creates a primitive visual based on category and presentation settings.
        /// </summary>
        private static GameObject CreatePrimitiveVisual(GameObject parent, object category, VisualPresentation presentation)
        {
            GameObject visual = null;
            PrimitiveType primitiveType = presentation.primitiveType;

            // Choose primitive based on category if not explicitly set
            if (presentation.primitiveType == PrimitiveType.Cube && presentation.usePrimitiveFallback)
            {
                if (category is BuildingTemplate.BuildingType buildingType)
                {
                    switch (buildingType)
                    {
                        case BuildingTemplate.BuildingType.Storage:
                            primitiveType = PrimitiveType.Cube;
                            break;
                        default:
                            primitiveType = PrimitiveType.Cube;
                            break;
                    }
                }
                else if (category is IndividualTemplate.IndividualType)
                {
                    primitiveType = PrimitiveType.Capsule;
                }
                else if (category is MaterialCategory)
                {
                    primitiveType = PrimitiveType.Sphere;
                }
                else if (category is EquipmentTemplate.EquipmentType)
                {
                    primitiveType = PrimitiveType.Cylinder;
                }
                else
                {
                    primitiveType = PrimitiveType.Sphere;
                }
            }

            visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = "Visual";
            visual.transform.SetParent(parent.transform);
            visual.transform.localPosition = presentation.positionOffset;
            visual.transform.localRotation = Quaternion.Euler(presentation.rotationOffset);
            visual.transform.localScale = presentation.scale;

            return visual;
        }

        /// <summary>
        /// Adds VFX presentation to the GameObject from template.
        /// </summary>
        private static void AddVFXPresentation(GameObject parent, PrefabTemplate template)
        {
            if (template == null || parent == null) return;
            
            var vfxPresentation = template.vfxPresentation;
            
            // Ensure presentation exists (migration safety)
            if (vfxPresentation == null)
            {
                vfxPresentation = new VFXPresentation();
                template.vfxPresentation = vfxPresentation;
            }
            
            if (vfxPresentation.vfxAsset != null)
            {
                var vfxGameObject = new GameObject("VFX");
                vfxGameObject.transform.SetParent(parent.transform);
                vfxGameObject.transform.localPosition = vfxPresentation.positionOffset;
                vfxGameObject.transform.localRotation = Quaternion.Euler(vfxPresentation.rotationOffset);
                
                var visualEffect = vfxGameObject.AddComponent<UnityEngine.VFX.VisualEffect>();
                visualEffect.visualEffectAsset = vfxPresentation.vfxAsset;
                
                if (vfxPresentation.playOnAwake)
                {
                    visualEffect.Play();
                }
                
                if (vfxPresentation.loop)
                {
                    // VFX Graph loop is typically controlled in the graph itself
                    // but we can set it here if needed
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for setting private fields via reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        public static void SetPrivateField(this object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
    }
}

