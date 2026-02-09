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

## Experience and Doctrine Expression
- Experience deltas: master elite has near-instant reaction and stable lock; apprentices show slower reacquire and higher aim spread.
- Profile x experience: defer profile interplay until firing range + mage baseline are stable.
- Doctrine/tooling: focus and deflection are the primary “tools” here; no drones.

## Profile Interplay Notes
- Interplay focus: environment-driven (training drill) rather than profile-vs-profile.
- Expected expressions: master stays calm and efficient; apprentices follow discipline rules and do not panic.

## Schedule and Regimen Nuances
- Lawful/stoic apprentices follow drill blocks; chaotic/corrupt may drift or fire outside cadence.
- Master (warlike + pure) keeps the session on schedule; allows brief breaks only for injury.
- Needs overrides (hunger/fatigue/injury) can preempt training; interruptions should be recorded.

## Spell/Combat Nuances
- Focus cadence: master cycles focus between apprentices with minimal idle.
- Deflection timing: deflect should trigger during incoming projectile windows; mis-timed deflect causes self-hit.
- Disable timing: disable locks out apprentice casting for a short window; avoid overlap with deflect window.

## Formation and Timing
- Ring formation is fixed; no movement in base run.
- Volley staggering: apprentices stagger shots to avoid continuous saturation; aim for readable cadence.

## Edge Cases
- If deflection is stubbed, record “intended deflect” vs actual collision outcome.
- If disable doesn’t interrupt casting, log it as a failure case.

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
