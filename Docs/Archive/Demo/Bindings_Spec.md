# Presentation Bindings Specification

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Design specification for Minimal/Fancy presentation binding system

---

## Overview

The presentation binding system allows swapping between visual presentation sets (Minimal and Fancy) at runtime without code changes. This enables demos to showcase the same simulation with different visual fidelity levels.

---

## Binding Concept

### Binding Sets

**Minimal Binding:**
- Primitive meshes (cubes, spheres, capsules)
- No VFX or particle effects
- Debug text overlays
- Wireframe or simple materials
- Purpose: Fast, clear, debug-friendly visuals

**Fancy Binding:**
- Art meshes (final or near-final assets)
- Full VFX and particle effects
- Polished materials and shaders
- Post-processing effects
- Purpose: Production-quality visuals

### Binding Assets

**Location:** `Assets/Resources/Bindings/`

**Files:**
- `Bindings/Minimal.asset` - Minimal binding set (ScriptableObject)
- `Bindings/Fancy.asset` - Fancy binding set (ScriptableObject)

**Structure:**
```csharp
[CreateAssetMenu(menuName = "Godgame/Bindings Set")]
public class PresentationBindingSet : ScriptableObject
{
    public BindingEntry[] Bindings;
}

[Serializable]
public class BindingEntry
{
    public string EffectId;        // e.g., "FX.Miracle.Ping"
    public GameObject Prefab;      // Visual prefab
    public Material Material;       // Optional material override
    public ParticleSystem VFX;      // Optional VFX
}
```

---

## Live Swap Mechanism

### Hotkey Swap

**Hotkey:** B (configurable)

**Flow:**
1. User presses B
2. `DemoBootstrap` updates `DemoOptions.BindingsSet` (0 ↔ 1)
3. Presentation system reads new binding set
4. Loads corresponding `PresentationBindingSet` asset
5. Applies visual mappings
6. Existing visuals update to new bindings

### Implementation

**Presentation System:**
```csharp
public partial struct PresentationBindingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var options = SystemAPI.GetSingleton<DemoOptions>();
        var bindingSet = options.BindingsSet == 0 
            ? LoadBindingSet("Bindings/Minimal") 
            : LoadBindingSet("Bindings/Fancy");
        
        // Apply bindings to active effects
        ApplyBindings(bindingSet);
    }
}
```

**No Code Changes Required:**
- Binding swap is purely data-driven
- No recompilation needed
- Works in builds and editor

---

## What Differs Between Sets

### Visual Elements

| Element | Minimal | Fancy |
|---------|---------|-------|
| **Meshes** | Primitives (cube, sphere) | Art meshes |
| **Materials** | Unlit, flat colors | PBR, textures |
| **VFX** | None | Particle systems |
| **Post-Processing** | None | Bloom, color grading |
| **UI** | Debug text | Polished UI |
| **Animations** | None or simple | Full animations |

### Effect IDs

**Common Effect IDs:**
- `FX.Miracle.Ping` - Miracle activation effect
- `FX.Construction.Ghost` - Construction ghost visual
- `FX.Construction.Complete` - Construction completion effect
- `FX.Villager.Gather` - Resource gathering effect
- `FX.Villager.Deliver` - Delivery effect
- `FX.Module.Refit` - Module refit sparks

**Binding Mapping:**
- Minimal: Simple mesh or debug text for each ID
- Fancy: Full VFX prefab for each ID

---

## Acceptance Criteria

### Binding Swap

1. **No Exceptions:**
   - Swapping bindings doesn't throw exceptions
   - All visuals update smoothly
   - No missing references

2. **Metrics Identical:**
   - Swapping bindings doesn't change simulation metrics
   - Same villager counts, resource totals, etc.
   - Determinism preserved

3. **Performance:**
   - Swap completes within 1 frame
   - No frame drops during swap
   - Memory usage stays within bounds

### Visual Fidelity

1. **Minimal:**
   - All effects have visual representation (even if primitive)
   - Debug information clearly visible
   - Performance optimized

2. **Fancy:**
   - All effects use production-quality visuals
   - VFX and particles work correctly
   - Materials and shaders render properly

---

## Prefab Maker Integration

### Prebuild Step

**Before Demo Build:**
1. Run Prefab Maker in Minimal mode
2. Validate all bindings resolve
3. Check idempotency (run twice, same output)
4. Write idempotency JSON

**Idempotency Check:**
- Run Prefab Maker twice
- Compare output hashes
- Log pass/fail

---

## File Structure

### Binding Assets

```
Assets/
├── Bindings/
│   ├── Minimal.asset
│   ├── Fancy.asset
│   └── README.md (binding documentation)
```

### Implementation

```
Assets/Scripts/Godgame/
├── Presentation/
│   ├── PresentationBindingSet.cs (ScriptableObject)
│   ├── PresentationBindingSystem.cs (ISystem)
│   └── BindingHelpers.cs (utility methods)
```

---

## Related Documentation

- `DemoBootstrap_Spec.md` - DemoBootstrap system (binding swap trigger)
- `Godgame_Slices.md` - Demo slices (binding swap demo)
- `Preflight_Checklist.md` - Preflight validation (binding swap test)

