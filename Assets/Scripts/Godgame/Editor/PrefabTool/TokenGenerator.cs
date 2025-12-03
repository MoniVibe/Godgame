using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Token generators for name/ID generation.
    /// Provides consistent naming and ID assignment patterns.
    /// </summary>
    public static class TokenGenerator
    {
        /// <summary>
        /// Generate a unique name from a base name and existing names.
        /// </summary>
        public static string GenerateUniqueName(string baseName, IEnumerable<string> existingNames)
        {
            if (!existingNames.Contains(baseName))
            {
                return baseName;
            }
            
            int counter = 1;
            string candidate = $"{baseName}_{counter}";
            
            while (existingNames.Contains(candidate))
            {
                counter++;
                candidate = $"{baseName}_{counter}";
            }
            
            return candidate;
        }
        
        /// <summary>
        /// Generate a catalog ID (MaterialId, ItemDefId, RecipeId) from a name.
        /// Uses a simple hash-based approach for consistency.
        /// </summary>
        public static ushort GenerateCatalogId(string name, ushort maxValue = ushort.MaxValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                return 0;
            }
            
            // Simple hash-based ID generation
            int hash = name.GetHashCode();
            ushort id = (ushort)(Math.Abs(hash) % maxValue);
            
            // Ensure non-zero
            if (id == 0)
            {
                id = 1;
            }
            
            return id;
        }
        
        /// <summary>
        /// Generate a display name from a technical name.
        /// Converts "Iron_Sword" → "Iron Sword"
        /// </summary>
        public static string GenerateDisplayName(string technicalName)
        {
            if (string.IsNullOrEmpty(technicalName))
            {
                return "";
            }
            
            // Replace underscores with spaces
            string displayName = technicalName.Replace("_", " ");
            
            // Capitalize first letter of each word
            displayName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(displayName.ToLower());
            
            return displayName;
        }
        
        /// <summary>
        /// Generate a technical name from a display name.
        /// Converts "Iron Sword" → "Iron_Sword"
        /// </summary>
        public static string GenerateTechnicalName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return "";
            }
            
            // Replace spaces with underscores
            string technicalName = displayName.Replace(" ", "_");
            
            // Remove special characters
            technicalName = Regex.Replace(technicalName, @"[^a-zA-Z0-9_]", "");
            
            return technicalName;
        }
        
        /// <summary>
        /// Generate a variant name (e.g., "Iron_Sword_Plus1", "Iron_Sword_Plus2").
        /// </summary>
        public static string GenerateVariantName(string baseName, int variantLevel, string suffix = "Plus")
        {
            return $"{baseName}_{suffix}{variantLevel}";
        }
        
        /// <summary>
        /// Generate a variant display name (e.g., "Iron Sword +1", "Iron Sword +2").
        /// </summary>
        public static string GenerateVariantDisplayName(string baseDisplayName, int variantLevel, string prefix = "+")
        {
            return $"{baseDisplayName} {prefix}{variantLevel}";
        }
        
        /// <summary>
        /// Generate next available ID from a list of templates.
        /// </summary>
        public static int GenerateNextId<T>(IEnumerable<T> templates) where T : PrefabTemplate
        {
            if (templates == null || !templates.Any())
            {
                return 1;
            }
            
            int maxId = templates.Max(t => t.id);
            return maxId + 1;
        }
        
        /// <summary>
        /// Validate name format (alphanumeric + underscores, no spaces).
        /// </summary>
        public static bool IsValidTechnicalName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            
            return Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$");
        }
        
        /// <summary>
        /// Sanitize a name to be a valid technical name.
        /// </summary>
        public static string SanitizeTechnicalName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Unnamed";
            }
            
            // Replace spaces with underscores
            string sanitized = name.Replace(" ", "_");
            
            // Remove special characters
            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", "");
            
            // Ensure it starts with a letter
            if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
            {
                sanitized = "Item_" + sanitized;
            }
            
            // Ensure non-empty
            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = "Unnamed";
            }
            
            return sanitized;
        }
    }
}

