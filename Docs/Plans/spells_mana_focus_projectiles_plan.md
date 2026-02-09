# Spells With Mana + Focus + Projectile Behaviors Plan

## Source Docs
- `godgame/Docs/AuthoringSetup.md` (SpellSpecCatalog + binding sets + blob bake workflow)
- `godgame/Docs/Miracle_Effect_Logic_Spec.md` (effect blocks, costs, channel/throw modes, focus costs)
- `godgame/Docs/Individual_Stats_Requirements.md` (mana + spell modifiers)
- `godgame/Docs/AI/Villager_BodyMind_Sync.md` (FocusBudget + deterministic regen + tests)
- `godgame/Docs/Concepts/Magic/Mana_Grid_System.md` (ambient mana field + disruption channel + sampler)
- `puredots/Docs/Concepts/Core/Tech_Tree_Flavors.md` (spell variants + projectile patterns)

## Goal
Introduce spell casting that consumes mana and (optionally) focus, supports projectile behaviors, and applies effect blocks deterministically in headless runs.

## Scope
- In-scope: spell data schema, cast gating (mana + focus), projectile spawning/motion/impact, effect block execution, telemetry, and a headless scenario.
- Out-of-scope: full VFX/presentation, balance tuning, AI behavior authoring, advanced editor tooling beyond existing Prefab Maker catalog flow.

## Schema / Data Model
- `SpellSpecCatalog` (ScriptableObject) -> `SpellSpecCatalogBlobRef`
- `SpellSpec` additions/fields:
  - `Id`, `CastMode` (Channel/Throw), `CooldownTicks`, `Range`, `Radius`, `Shape`
  - `CostMana`, `CostFocus` (optional), `SustainedCostMana`, `SustainedCostFocus`
  - `ProjectileSpecId` (links to projectile behavior)
  - `EffectBlocks[]` (phase: channel/impact/lingering; parameters: damage/heal/status/environment)
- Caster components:
  - `VillagerCombatStats` for `CurrentMana`/`MaxMana` (individual caster pool)
  - `FocusBudget` for focus gating (shared pool + reservations)
  - **Deity mana pool** (global player resource, gained from worship) for god-cast spells
  - **Individual mana pool** (per entity) for entity-cast spells
  - **Mana grid integration** (ambient field + disruption)
    - `ManaGrid` + `ManaGridRuntimeCell` (scalar field, fixed-point)
    - `ManaInfluence` (emitters/siphons/inhibitors)
    - `EnvironmentSampler.SampleMana(pos)` and `SampleManaDisruption(pos)`
  - `SpellCastRequest` buffer (spell id, target, cast mode, charge)
  - `SpellCastState` (cooldowns, channel state, last tick)
- Projectile components (Burst-safe, deterministic):
  - `SpellProjectile`, `ProjectileMotion`, `ProjectileHoming`, `ProjectileLifetime`
  - `ProjectileImpact`, `ProjectileAoE`

## Systems / Flow
1. **SpellCastRequestSystem**
   - Validate `SpellSpec` id exists, check range/target.
   - Sample ambient mana + disruption at caster position.
   - Route cost to **deity mana** or **individual mana** based on spell ownership.
   - Apply ambient mana cost multiplier + disruption failure gate.
   - Gate on mana pool + `FocusBudget.Current`.
2. **SpellCostReservationSystem**
   - Reserve focus for precision/charge; subtract mana on cast start.
   - Handle sustained costs per tick for channel.
3. **SpellProjectileSpawnSystem**
   - Spawn projectile entities using deterministic RNG.
   - Attach motion/homing components from `ProjectileSpecId`.
4. **SpellProjectileMoveSystem**
   - Integrate movement in `FixedStepSimulationSystemGroup` (Burst).
5. **SpellImpactSystem**
   - Resolve impact and apply `EffectBlocks` (damage/heal/status/environment).
   - Apply `SpellDurationModifier` and `SpellIntensityModifier`.
6. **SpellCooldownSystem**
   - Tick cooldowns, clear channel state, release focus reservations.
7. **Telemetry**
   - Emit event + counters: casts attempted/successful, mana spent, focus spent, projectile hits, effect applications.
   - Ambient mana signals: avg ambient mana sampled, casts blocked by disruption/low ambient.

## Scenario Template Parity
- scenarioId: `godgame_spells_mana_focus_projectiles_micro`
- duration_s: 120 (or 600 if multi-cast behavior needed)
- seed handling: fixed seed in scenario JSON
- headlessQuestions:
  - `spell_casts_successful`
  - `mana_spent_total_deity`
  - `mana_spent_total_individual`
  - `focus_spent_total`
  - `projectile_hits`
  - `status_applied_count`
  - `mana_ambient_avg`
  - `mana_disruption_avg`
  - `casts_blocked_by_ambient`
  - `casts_blocked_by_disruption`
- metrics + scoring:
  - Deterministic counts match expected ranges.
  - Zero nondeterministic drift in repeated runs.
- acceptance criteria:
- Casts fail when mana/focus insufficient in the **correct** pool.
  - Projectile behaviors match spec (homing/arc/aoe).
  - Effects apply in correct phase (channel/impact/lingering).

## Determinism + Burst
- All hot paths Burst-safe (no managed allocations, no strings).
- Deterministic RNG source (seeded per spell cast or scenario).
- Avoid time-based randomness outside fixed tick.
- No managed string allocations in telemetry payloads.
- Mana grid uses fixed-point (Q16.16) or deterministic float, gather-based influences (no atomics).

## Verification
- Scenario run: `godgame/Assets/Scenarios/godgame_spells_mana_focus_projectiles_micro.json`
- Commands:
  - Buildbox headless run with scenario
- Expected output:
  - telemetry counts for cast/mana/focus/hits present
  - deterministic digest stable across reruns

## Risks / Open Questions
- Do we reuse miracle pipeline for spells or keep separate?
- FocusBudget semantics: per caster or shared per group?
- How to author ProjectileSpec (new catalog vs inline in SpellSpec)?
- Mana grid not yet implemented in headless scene; may require stubbed sampler with default value.
- Disruption channel semantics: failure probability vs hard block (needs tuning).

## Next Steps
1. Define `ProjectileSpec` data model and authoring surface.
2. Add spell cast request + cost reservation systems.
3. Implement projectile spawn/move/impact pipeline.
4. Add headless scenario + telemetry validation.
