# Addendum: Master vs Apprentices - Nuance Notes
Date: 2026-02-09
Owner: shonh
Status: active

## Purpose
Capture master/apprentice combat nuances (focus, deflection, disable cadence) without bloating the goal card.

## Nuance Map
- Master focus stance discipline and deflect timing.
- Apprentice volley cadence, spacing, and lock churn.
- Disable spell timing and recovery loops.
- Projectile behavior and deflection edge cases.
 - Relations and learning loops (mentor + cohort).
 - Sidelined injured students and spectators.

## Cross-Domain Equivalence
- Master mage maps to apex operator/ship; this drill maps to skill-expression testbeds (archery, gunnery, piloting).
- PureDOTS owns the timing/accuracy/deflection primitives; game-specific skins should not fork the model.

## Experience and Doctrine Expression
- Experience deltas: master elite has near-instant reaction and stable lock; apprentices show slower reacquire and higher aim spread.
- Profile x experience: defer profile interplay until firing range + mage baseline are stable.
- Doctrine/tooling: focus and deflection are the primary “tools” here; no drones.

## Relations and Learning
- Master is a mentor, apprentices are a cohort; cohesion improves cadence.
- Learning loop: apprentices gain small skill deltas from exposure; master stabilizes under pressure.
- If learning is stubbed, log intended deltas as telemetry only.

## Profile Interplay Notes
- Interplay focus: environment-driven (training drill) rather than profile-vs-profile.
- Expected expressions: master stays calm and efficient; apprentices follow discipline rules and do not panic.

## Schedule and Regimen Nuances
- Lawful/stoic apprentices follow drill blocks; chaotic/corrupt may drift or fire outside cadence.
- Master (warlike + pure) keeps the session on schedule; allows brief breaks only for injury.
- Needs overrides (hunger/fatigue/injury) can preempt training; interruptions should be recorded.

## Positioning and Formation
- Apprentices form a ring at fixed radius; master remains centered.
- Sidelined students move to a safe edge and stop casting.
- Spectators stand outside the ring; no interaction unless variant enabled.

## Spell/Combat Nuances
- Focus cadence: master cycles focus between apprentices with minimal idle.
- Deflection timing: deflect should trigger during incoming projectile windows; mis-timed deflect causes self-hit.
- Disable timing: disable locks out apprentice casting for a short window; avoid overlap with deflect window.
 - Barrier pulse (optional): brief group protection, high mana cost.
 - Control/sway (variant): master can redirect a subset of projectiles.

## Deflection Model (Coarse, Low-Overhead)
- Ownership: PureDOTS shared deflection/timing/accuracy model; Godgame only tunes values.
- Use windowed deflection, not per-projectile physics.
- Threat slots: evaluate top 3-5 incoming projectiles per tick (10-20Hz).
- Precompute impact windows per volley; deflect resolves once per window.
- Outcomes: success (redirect/nullify), partial (redirect + reduced damage), fail (no change).
- Arc rules: only within forward arc; out-of-arc attempts fail or are skipped.

## Deflection Timing Parameters (Targets)
- windup_ms: 120 (elite), 200 (rookie)
- deflect_window_ms: 160 (elite), 120 (rookie)
- recovery_ms: 250 base
- prequeue_ms: 80 (elite), 0 (rookie)
- arc_deg: 180 base (elite may widen +20)

## Overhead Controls
- Cap deflection checks per tick; no per-projectile collision tests.
- Cache arc checks unless facing changes.
- If budget exceeded, collapse to volley-level deflect only.

## Needs and Interrupts
- Hunger/fatigue reduce focus uptime; master may pause only if critical.
- Injury triggers immediate sideline; apprentices stop casting.
- Record interrupts inside the training window for later analysis.

## Formation and Timing
- Ring formation is fixed; no movement in base run.
- Volley staggering: apprentices stagger shots to avoid continuous saturation; aim for readable cadence.

## Edge Cases
- If deflection is stubbed, record “intended deflect” vs actual collision outcome.
- If disable doesn’t interrupt casting, log it as a failure case.
 - If spectators impact morale, ensure it does not affect timing in base run.

## Nuance Budget / Perf Notes (Optional)
- Budget target: TBD after core combat systems land.
- Expensive features: per-projectile deflect checks, per-apprentice disable state.
- Fallbacks: coarse deflect window checks; reduce per-projectile detail if needed.

## Telemetry Wish-list
- master.deflect_timing_error_ms
- master.disable_overlap_rate
- apprentice.volley_cadence_variance

## Open Questions
- Should apprentices attempt evasive micro-movement in a variant?
- Should focus apply a visible slow/aim-assist effect for readability?
