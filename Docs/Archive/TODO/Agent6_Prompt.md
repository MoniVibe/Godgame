# Agent 6: Fix CreateAssetMenu Warnings - Part 2

## Your Mission
Fix console warnings about `[CreateAssetMenu]` attribute being applied to non-ScriptableObject classes in PureDOTS.

## Project Location
`C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS`

## Required Reading First
Read `TRI_PROJECT_BRIEFING.md` in the project root, specifically section **P12**.

---

## Classes to Fix

You are responsible for these 5 authoring classes:

1. **BuffCatalogAuthoring**
2. **SchoolComplexityCatalogAuthoring**
3. **QualityFormulaAuthoring**
4. **SpellSignatureCatalogAuthoring**
5. **QualityCurveAuthoring**

---

## The Problem

```
CreateAssetMenu attribute on PureDOTS.Authoring.Buffs.BuffCatalogAuthoring will be ignored 
as PureDOTS.Authoring.Buffs.BuffCatalogAuthoring is not derived from ScriptableObject.
```

The `[CreateAssetMenu]` attribute only works on classes that inherit from `ScriptableObject`. These authoring classes inherit from `MonoBehaviour` (for ECS baking), so the attribute is ignored and clutters the console.

---

## Investigation Step

First, find the files:

```bash
cd "C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS"

# Find each file
grep -r "class BuffCatalogAuthoring" --include="*.cs" -l
grep -r "class SchoolComplexityCatalogAuthoring" --include="*.cs" -l
grep -r "class QualityFormulaAuthoring" --include="*.cs" -l
grep -r "class SpellSignatureCatalogAuthoring" --include="*.cs" -l
grep -r "class QualityCurveAuthoring" --include="*.cs" -l
```

Likely locations:
- `Packages/com.moni.puredots/Runtime/Authoring/Buffs/`
- `Packages/com.moni.puredots/Runtime/Authoring/Spells/`
- `Packages/com.moni.puredots/Runtime/Authoring/Shared/`

---

## Fix Pattern

```csharp
// ❌ WRONG - CreateAssetMenu on MonoBehaviour (causes warning)
[CreateAssetMenu(menuName = "PureDOTS/Buffs/Buff Catalog")]
public class BuffCatalogAuthoring : MonoBehaviour
{
    // Baker code...
}

// ✅ CORRECT - Remove the CreateAssetMenu attribute
public class BuffCatalogAuthoring : MonoBehaviour
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

namespace PureDOTS.Authoring.Buffs
{
    [CreateAssetMenu(fileName = "NewBuffCatalog", menuName = "PureDOTS/Buffs/Buff Catalog")]
    public class BuffCatalogAuthoring : MonoBehaviour
    {
        public List<BuffDefinition> buffs;
        
        public class Baker : Baker<BuffCatalogAuthoring>
        {
            // ...
        }
    }
}
```

**After:**
```csharp
using UnityEngine;

namespace PureDOTS.Authoring.Buffs
{
    public class BuffCatalogAuthoring : MonoBehaviour
    {
        public List<BuffDefinition> buffs;
        
        public class Baker : Baker<BuffCatalogAuthoring>
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
   - `BuffCatalogAuthoring`
   - `SchoolComplexityCatalogAuthoring`
   - `QualityFormulaAuthoring`
   - `SpellSignatureCatalogAuthoring`
   - `QualityCurveAuthoring`

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
✅ BuffCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ SchoolComplexityCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ QualityFormulaAuthoring - [PATH] - Removed CreateAssetMenu
✅ SpellSignatureCatalogAuthoring - [PATH] - Removed CreateAssetMenu
✅ QualityCurveAuthoring - [PATH] - Removed CreateAssetMenu

Build status: Domain reload succeeded with no CreateAssetMenu warnings for these classes
```


