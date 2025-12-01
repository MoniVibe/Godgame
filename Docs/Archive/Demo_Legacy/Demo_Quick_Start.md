# Demo Quick Start Checklist

**Quick reference for setting up a Godgame demo scene**

## 5-Minute Setup

### 1. Open/Create Scene
- [ ] Open `Assets/Scenes/Godgame_DemoScene.unity` OR create new scene
- [ ] Save scene

### 2. Add Bootstrap
- [ ] Create empty GameObject → `DemoBootstrap`
- [ ] Add `GodgameDemoBootstrapAuthoring` component
- [ ] Configure:
  - [ ] Villager Prefab (create if needed)
  - [ ] Storehouse Prefab (create if needed)
  - [ ] Initial Villager Count: 10
  - [ ] Resource Node Count: 6
  - [ ] Spawn Radius: 15
  - [ ] Node Radius: 20

### 3. Create Prefabs (if missing)

**Villager Prefab:**
- [ ] Create GameObject with Capsule child
- [ ] Add `VillagerAuthoring` (if exists)
- [ ] Save as `Assets/Prefabs/Villager.prefab`

**Storehouse Prefab:**
- [ ] Create GameObject with Cube child
- [ ] Add `StorehouseAuthoring` (if exists)
- [ ] Save as `Assets/Prefabs/Storehouse.prefab`

### 4. Configure Scene
- [ ] Position camera to view spawn area (center at origin)
- [ ] Add Directional Light
- [ ] Verify render pipeline settings

### 5. Test
- [ ] Press Play
- [ ] Verify entities spawn
- [ ] Verify villagers move and gather
- [ ] Check DOTS Hierarchy for entities
- [ ] Check console for errors

## What to Showcase

### Core Features
1. **Villager AI**
   - Autonomous navigation
   - Resource gathering
   - Storehouse delivery
   - Personality traits

2. **Resource Flow**
   - Nodes → Villagers → Storehouses
   - Registry synchronization
   - Telemetry/metrics

3. **Time Controls** (if enabled)
   - Pause/Play
   - Speed control
   - Rewind determinism

### Inspectors to Keep Open
- DOTS Hierarchy (View → DOTS Hierarchy)
- Game View
- Console (for debug logs)

## Troubleshooting

| Issue | Solution |
|-------|----------|
| No entities spawn | Check bootstrap config, verify prefabs assigned |
| Villagers don't move | Check VillagerJobSystem enabled, verify navigation |
| Registry not syncing | Check GodgameRegistryBridgeSystem enabled |
| Performance issues | Reduce villager count, check Burst compilation |

## Next Steps

1. Record video showcasing features
2. Update `Docs/Progress.md` with demo status
3. Share with team/stakeholders
4. Gather feedback and iterate

---

**See `Docs/Guides/Demo_Showcase_Guide.md` for detailed instructions.**

