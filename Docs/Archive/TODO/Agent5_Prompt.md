# Agent 5: Fix CreateAssetMenu Warnings - Part 1

## Your Mission
Fix console warnings about `[CreateAssetMenu]` attribute being applied to non-ScriptableObject classes in PureDOTS.

## Project Location
`C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS`

## Required Reading First
Read `TRI_PROJECT_BRIEFING.md` in the project root, specifically section **P12**.

---

## Classes to Fix

You are responsible for these 5 authoring classes:

1. **CultureStoryCatalogAuthoring**
2. **LessonCatalogAuthoring**
3. **SpellCatalogAuthoring**
4. **ItemPartCatalogAuthoring**
5. **EnlightenmentProfileAuthoring**

---

## The Problem

```
CreateAssetMenu attribute on PureDOTS.Authoring.Culture.CultureStoryCatalogAuthoring will be ignored 
as PureDOTS.Authoring.Culture.CultureStoryCatalogAuthoring is not derived from ScriptableObject.
```

The `[CreateAssetMenu]` attribute only works on classes that inherit from `ScriptableObject`. These authoring classes inherit from `MonoBehaviour` (for ECS baking), so the attribute is ignored and clutters the console.

---

## Investigation Step

First, find the files:

```bash
cd "C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS"

# Find each file
grep -r "class CultureStoryCatalogAuthoring" --include="*.cs" -l
grep -r "class LessonCatalogAuthoring" --include="*.cs" -l
grep -r "class SpellCatalogAuthoring" --include="*.cs" -l
grep -r "class ItemPartCatalogAuthoring" --include="*.cs" -l
grep -r "class EnlightenmentProfileAuthoring" --include="*.cs" -l
```

Likely locations:
- `Packages/com.moni.puredots/Runtime/Authoring/Culture/`
- `Packages/com.moni.puredots/Runtime/Authoring/Knowledge/`
- `Packages/com.moni.puredots/Runtime/Authoring/Spells/`
- `Packages/com.moni.puredots/Runtime/Authoring/Items/`

---

## Fix Pattern

```csharp
// ❌ WRONG - CreateAssetMenu on MonoBehaviour (causes warning)
[CreateAssetMenu(menuName = "PureDOTS/Culture/Story Catalog")]
public class CultureStoryCatalogAuthoring : MonoBehaviour
{
    // Baker code...
}

// ✅ CORRECT - Remove the CreateAssetMenu attribute
public class CultureStoryCatalogAuthoring : MonoBehaviour
{
    // Baker code stays the same
}
```

**Note**: Do NOT change the class to inherit from ScriptableObject. These are authoring components that must stay as MonoBehaviour for ECS baking to work. Just remove the `[CreateAssetMenu]` attribute.

---

## Step-by-Step Instructions

For each of the 5 classes:

1. Open the file containing the class
2. Find the `[CreateAssetMenu(...)]` attribute above the class declaration
3. Delete the entire `[CreateAssetMenu(...)]` line
4. Keep everything else unchanged
5. Save the file

---

## Example Fix

**Before:**
```csharp
using UnityEngine;

namespace PureDOTS.Authoring.Culture
{
    [CreateAssetMenu(fileName = "NewStoryCatalog", menuName = "PureDOTS/Culture/Story Catalog")]
    public class CultureStoryCatalogAuthoring : MonoBehaviour
    {
        public List<StoryEntry> stories;
        
        public class Baker : Baker<CultureStoryCatalogAuthoring>
        {
            // ...
        }
    }
}
```

**After:**
```csharp
using UnityEngine;

namespace PureDOTS.Authoring.Culture
{
    public class CultureStoryCatalogAuthoring : MonoBehaviour
    {
        public List<StoryEntry> stories;
        
        public class Baker : Baker<CultureStoryCatalogAuthoring>
        {
            // ...
        }
    }
}
```

---

## Verification

After fixing all 5 classes, verify by:
1. Opening Unity Editor for PureDOTS project
2. Triggering domain reload
3. Check Console - should have NO warnings mentioning:
   - `CultureStoryCatalogAuthoring`
   - `LessonCatalogAuthoring`
   - `SpellCatalogAuthoring`
   - `ItemPartCatalogAuthoring`
   - `EnlightenmentProfileAuthoring`

---

## Constraints

- Only remove the `[CreateAssetMenu]` attribute
- Do NOT change the base class (keep `: MonoBehaviour`)
- Do NOT modify any other code in the file
- Do NOT remove other attributes like `[DisallowMultipleComponent]` if present

---

## Report When Complete

```
Files Fixed:
✅ CultureStoryCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ LessonCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ SpellCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ ItemPartCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ EnlightenmentProfileAuthoring - [PATH] - Removed CreateAssetMenu

Build status: Domain reload succeeded with no CreateAssetMenu warnings for these classes
```


