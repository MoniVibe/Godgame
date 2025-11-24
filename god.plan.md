<!-- 0baa87e0-55d9-4da1-8a4b-47a8f2004cc0 341e8c17-43e7-42d8-b49a-d3ccbf85bfc6 -->
# Material Rule Engine Plan

## 1. Schema & Traits Expansion

- Add `MaterialCategory`, `MaterialUsage`, `MaterialTraits`, `HazardClass`, `PackageClass` enums/flags.
- Extend `MaterialTemplate` with stat blocks (MaterialStats, LogisticsBlock, EconomyBlock, MiracleAffinity, StyleTokens), quality/purity/variant fields.
- Implement `MaterialRule`, `MaterialRuleSetBlob`, and recipe structs (input→output with time/energy/tool/skill requirements).

## 2. Authoring & Data Flow

- Create ScriptableObject or DOTS blob authoring assets for materials, rules, and recipes.
- Support VariantOf definitions with delta overrides (quality/purity/traits adjustments).
- Add process recipe authoring (graph view optional, ensures deterministic baking).

## 3. Editor UX Enhancements

- Update Prefab Editor UI to display trait chips, stats, logistics, economy blocks.
- Add validation panel with "why invalid" explanations and substitution suggestions.
- Implement filters by trait/usage/category and bulk edit operations.
- Add Dry-Run button to preview prefab generation using rules/substitutions without asset creation.

## 4. Rule Engine & Validation

- Implement trait-driven rule evaluation (property-based constraints instead of hard lists).
- Validate unit consistency (mass/volume), forbidden combos (Armor ∧ Flammable), recipe cycle detection, unreachable materials, missing localization.
- Provide auto-suggest substitutes based on SubstitutionRank and trait compatibility.

## 5. Prefab Generation Integration

- Modify prefab generator to pick materials/tools via rule engine (usage + min stats) instead of hardcoding names.
- Support substitution rules when preferred materials unavailable; emit telemetry events.
- Ensure ECS outputs only material IDs + style tokens (no asset refs).

## 6. Tests & Documentation

- Add EditMode/PlayMode tests: rule validation matrix, recipe cycle detection, unit sanity, substitution paths, blob serialization round-trips.
- Update docs (Prefab Creation Guide, README) with new workflows, schemas, and validation steps.

### To-dos

- [x] Survey repo state and capture current progress
- [x] Draft next-phase plan and placeholder presentation
- [x] Add bands/logistics authoring + registry mirror tests
- [x] Implement storehouse components/API + conservation test
- [x] Build villager job loop systems + telemetry tests
- [x] Implement construction ghost/build flow + tests
- [x] Wire time control inputs/HUD + rewind test
- [x] Parallelize ModuleRefitSystem job
- [x] Expand placeholder IDs + optionality test
- [x] Run EditMode/PlayMode suites & document results

---

# Godgame Prefab Maker — Cross‑Type Expansion Plan (Phase 3)

**Goal:** Keep the tool data‑driven and asset‑agnostic. Materials are the spine; add thin, rule‑based layers for Buildings, Equipment, Individuals, Miracles/FX, Resource Nodes/Spawners, and Logistics/Containers. Everything bakes to DOTS blobs; ECS carries IDs + style tokens only.

---

## Core principles (don't regress)

* **Traits over hard lists.** Material rules remain property‑based; other types consume those traits (e.g., Armor requires `Ductile ∧ Hardness≥t` and forbids `Flammable`).

* **Idempotent generation.** Re‑running the maker with unchanged catalogs produces no diffs (same GUIDs, same blobs).

* **Prefab == data shell.** No gameplay logic or scene refs. Only IDs, sockets, tags, and style tokens.

* **Validation first.** Every generator step has lints + "why invalid" explanations + auto‑substitutions.

---

## Type expansions (minimal schemas + rules)

### 1) Buildings & Facilities

**Data:**

* `BuildingId`, `Footprint { size, shape }`, `Placement { biome, slope, altitude, water, roadAdjacency }`,

* `Cost { MaterialUsage reqs + min stats }`, `Upgrades { VariantOf + deltas }`,

* `FacilityTags { RefitFacility?, Storage?, RitualSite? }`, `Residency/Continuity`.

**Rules:**

* Placement validates against map properties (biome/slope); Cost validates via Material Rule Engine (substitutions allowed).

* Adjacency bonuses/forbids (e.g., "not within 10m of water" or "+eff near road").

**Prefab content:** ID, footprint gizmo, socket anchors (entrances, power, logistics), facility tags.

**Tests:** `Building_PlacementRules_EditMode`, `Building_CostValidation_PlayMode`, `Building_BindingOptionality_PlayMode`.

---

### 2) Equipment & Tools

**Data:** `EquipmentId`, `SlotKind` (Hand, Body, Head), `Mass`, `Durability`, `AllowedMaterials` (expressed as rule: usage + min stats), `Effects` (stat modifiers).

**Rules:**

* Material compatibility via traits + min hardness/toughness.

* Durability scales with material stats/purity.

**Prefab content:** ID, `Socket_Attach` transform, style tokens.

**Tests:** `Equipment_MaterialCompatibility_EditMode`, `Equipment_Substitution_PlayMode`, `Equipment_DurabilityFromStats_EditMode`.

---

### 3) Individuals (Villagers)

**Data:** `IndividualArchetypeId`, `Disciplines` (skills/levels), `EquipmentSlots`, `InventoryCaps` (via Logistics PackageClass), `MiracleAffinity` (reused from materials where applicable), `SpawnWeights`.

**Rules:**

* Equipment gating by slot + discipline requirements.

* Carry limits derived from archetype + logistic package.

**Prefab content:** ID, sockets for held items, style tokens (palette only).

**Tests:** `Individual_LoadoutRules_EditMode`, `Individual_SpawnWeights_Deterministic_PlayMode`.

---

### 4) Miracles & FX

**Data:** `MiracleId`, `TargetFilters` (tags/traits: MaterialTraits, Individual tags), `AreaShape`, `Cost`, `Cooldown`, `AffinityMods`.

**Rules:**

* Valid targets decided by traits/tags; presentation IDs are separate and swappable.

**Prefab content:** ID only + optional FX placeholder token.

**Tests:** `Miracle_TargetFilterValidation_EditMode`, `Miracle_EffectIdBinding_PlayMode`.

---

### 5) Resource Nodes & Spawners

**Data:** `NodeId`, `ResourceTypeIndex` (ties to MaterialTemplate), `Capacity`, `RegrowthRate`, `HarvestToolRules`, `Hazard`.

**Rules:**

* Harvest allowed iff tool/material rules satisfied; regrowth deterministic per seed.

**Prefab content:** ID, footprint gizmo, socket for interaction.

**Tests:** `Node_Regrowth_Deterministic_PlayMode`, `Node_HarvestToolRules_EditMode`.

---

### 6) Logistics & Containers

**Data:** `ContainerId`, `CapacityUnits`, `AcceptedPackageClasses`, `ThroughputRate`, `Decay/SpoilagePolicy` (mirrors material `LogisticsBlock`).

**Rules:**

* Accept only materials with compatible `PackageClass` & hazard; spoilage/rot rules for consumables.

**Prefab content:** ID, socket for input/output.

**Tests:** `Container_AcceptsPackages_EditMode`, `Container_SpoilageRules_PlayMode`.

---

## Editor UX extensions

* **Tabs per type** (Materials, Buildings, Equipment, Individuals, Miracles, Nodes, Containers).

* **Filters** by trait/usage/category; bulk edits for shared fields.

* **Why invalid** panel shows rule clauses that failed and suggested substitutes.

* **Dry‑Run per type**: Prints a deterministic diff + JSON report; no asset writes.

* **Adopt/Repair** tab: scans existing prefabs, adds missing sockets/components, fixes naming/paths.

---

## Rule‑engine abstraction (shared)

* Introduce an internal interface:

  * `IRuleSet<TSpec>` — validates a prefab spec using material traits/stats.

  * `IPrefabRecipe<TSpec>` — converts catalog/spec data → prefab definition (no assets).

  * `IPrefabValidator<TSpec>` — produces rich diagnostics + substitutions.

* Each type above supplies small rule sets; all reuse the Material Rule Engine.

---

## Pipeline (per type)

1. Load catalogs/blobs → build in‑memory **Spec** list.

2. **Dry‑run**: evaluate rules, compute substitutions, produce diff.

3. **Generate**: write/repair prefabs (IDs, sockets, tags, style tokens).

4. **Binding**: update presentation binding blob (IDs → prefab refs) for placeholders only.

5. **Bake**: authoring to DOTS blobs; ECS holds **IDs + style tokens** exclusively.

---

## Acceptance (for expanding beyond Materials)

* Generators exist for Buildings & Equipment first (highest leverage), with rule validation and idempotent writes.

* Tests green for each type (at least the ones listed).

* Removing the PresentationBridge still leaves all generation/baking tests green.

---

## Start‑today cut (1 week)

* Implement Buildings + Equipment specs, rules, and generators.

* Add `Dry‑Run` + JSON report and the 6 core tests.

* Wire into CI `-executeMethod` so idempotency/regression checks run nightly.

