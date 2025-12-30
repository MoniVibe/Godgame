# Agent Quickstart (Godgame)

- Start here: `../TRI_PROJECT_BRIEFING.md` (canonical orientation).
- Active scene for smoke: `Assets/Scenes/TRI_Godgame_Smoke.unity` (SubScene for world slice).
- Avoid legacy: anything under `Assets/_Archive/**` is reference-only.
- Rendering: prefer modern render catalog path (`Assets/Rendering/GodgameRenderCatalog.asset` + debug systems); ignore `Assets/_Archive/Legacy/**`.
- Where to add game code: `Assets/Scripts/Godgame/...` (presentation), PureDOTS stays in package.
- Smoke bring-up: ensure TimeState/TickTimeState/RewindState + registry singletons baked in the smoke SubScene; use `Godgame_DemoValidationSystem` as reference pattern.
- Cameras: live in `Assets/Scripts/Godgame/Camera/`, drive motion with `Time.deltaTime`; no camera Monos in PureDOTS.
- Cross-OS split: WSL headless/logic uses `/home/oni/Tri` (ext4); Windows presentation uses `C:\dev\Tri`. Avoid `/mnt/c` for active WSL work.
- Ownership boundary: keep `Assets/` + `.meta` changes on Windows; keep PureDOTS/logic changes on WSL to avoid GUID/PPtr churn.
- WSL is case-sensitive; fix casing mismatches that Windows may tolerate.
