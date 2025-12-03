using System;
using System.Collections.Generic;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Binding set: Reusable material→prefab mappings for bulk operations.
    /// Example: "Iron" → "Iron Sword", "Iron Axe", "Iron Mace"
    /// </summary>
    [Serializable]
    public class BindingSet
    {
        public string name;
        public string description;
        
        /// <summary>
        /// Source material name (or pattern).
        /// </summary>
        public string sourceMaterialName;
        
        /// <summary>
        /// Target prefab templates to generate.
        /// </summary>
        public List<BindingTarget> targets = new List<BindingTarget>();
        
        /// <summary>
        /// Apply this binding set to generate prefabs from a material.
        /// </summary>
        public List<PrefabTemplate> Apply(MaterialTemplate sourceMaterial)
        {
            var generated = new List<PrefabTemplate>();
            
            foreach (var target in targets)
            {
                var template = target.CreateTemplate(sourceMaterial);
                generated.Add(template);
            }
            
            return generated;
        }
    }
    
    /// <summary>
    /// A single binding target (one prefab to generate from a material).
    /// </summary>
    [Serializable]
    public class BindingTarget
    {
        /// <summary>
        /// Template type to generate.
        /// </summary>
        public BindingTargetType targetType;
        
        /// <summary>
        /// Name pattern (e.g., "{Material} Sword").
        /// </summary>
        public string namePattern;
        
        /// <summary>
        /// Display name pattern.
        /// </summary>
        public string displayNamePattern;
        
        /// <summary>
        /// Stat overrides (if any).
        /// </summary>
        public StatOverrides statOverrides = new StatOverrides();
        
        /// <summary>
        /// Create a template from a source material.
        /// </summary>
        public PrefabTemplate CreateTemplate(MaterialTemplate sourceMaterial)
        {
            string name = namePattern.Replace("{Material}", sourceMaterial.name);
            string displayName = displayNamePattern.Replace("{Material}", sourceMaterial.displayName);
            
            switch (targetType)
            {
                case BindingTargetType.Equipment:
                    return CreateEquipmentTemplate(sourceMaterial, name, displayName);
                case BindingTargetType.Tool:
                    return CreateToolTemplate(sourceMaterial, name, displayName);
                default:
                    throw new ArgumentException($"Unsupported target type: {targetType}");
            }
        }
        
        private EquipmentTemplate CreateEquipmentTemplate(MaterialTemplate source, string name, string displayName)
        {
            var template = new EquipmentTemplate
            {
                name = name,
                displayName = displayName,
                id = 0, // Will be reassigned
                requiredUsage = source.usage,
                quality = source.quality,
                rarity = source.rarity,
                techTier = source.techTier,
                requiredTechTier = source.requiredTechTier
            };
            
            // Apply stat overrides
            if (statOverrides.baseAttack > 0)
                template.stats.damage = statOverrides.baseAttack;
            if (statOverrides.baseDefense > 0)
                template.stats.armor = statOverrides.baseDefense;
            if (statOverrides.durability > 0)
                template.baseDurability = statOverrides.durability;
            if (statOverrides.mass > 0)
                template.mass = statOverrides.mass;
            
            return template;
        }
        
        private ToolTemplate CreateToolTemplate(MaterialTemplate source, string name, string displayName)
        {
            var template = new ToolTemplate
            {
                name = name,
                displayName = displayName,
                id = 0,
                quality = source.quality,
                rarity = source.rarity,
                techTier = source.techTier,
                requiredTechTier = source.requiredTechTier
            };
            
            if (statOverrides.durability > 0)
                template.durabilityBonus = statOverrides.durability;
            if (statOverrides.workEfficiency > 0)
                template.workEfficiency = statOverrides.workEfficiency;
            
            return template;
        }
    }
    
    /// <summary>
    /// Type of binding target.
    /// </summary>
    public enum BindingTargetType
    {
        Equipment,
        Tool,
        Material,
        Building
    }
    
    /// <summary>
    /// Stat overrides for binding targets.
    /// </summary>
    [Serializable]
    public class StatOverrides
    {
        public float baseAttack = 0f;
        public float baseDefense = 0f;
        public float durability = 0f;
        public float workEfficiency = 0f;
        public float mass = 0f;
    }
}

