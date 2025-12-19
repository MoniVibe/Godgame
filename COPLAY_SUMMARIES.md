# Coplay Summaries Log

## 2025-12-19: Godgame Validation

### Result
**FAIL**

### Evidence
Key signals observed:
- `[GodgameScenarioLoaderSystem] SettlementConfig count=0`
- `[GodgameScenarioLoaderSystem] Waiting for SettlementConfig SubScene to load...`
- `[GodgameScenarioLoaderSystem] Loading scenario: Villager Loop Small`
- `[GodgameScenarioLoaderSystem] Configured scenario with 5 villagers, 1 storehouses.`
- `[RenderSanitySystem] World 'Game World' has 32 RenderSemanticKey entities.`

### Errors
The session failed due to repeated AssertionExceptions in the debug systems.

```
AssertionException: MaterialMeshIndexRange is only valid when HasMaterialMeshIndexRange is true
Assertion failure. Value was False
Expected: True
  at UnityEngine.Assertions.Assert.Fail (System.String message, System.String userMessage)
  at UnityEngine.Assertions.Assert.IsTrue (System.Boolean condition, System.String message)
  at Unity.Assertions.Assert.IsTrue (System.Boolean condition, System.String message)
  at Unity.Rendering.SubMeshIndexInfo32.get_MaterialMeshIndexRangeAsInt ()
  at Unity.Rendering.MaterialMeshInfo.get_MaterialMeshIndexRange ()
  at Godgame.DebugSystems.GodgameBuildingVisualPostProbeSystem.LogSample (Unity.Entities.SystemState& state, System.String label, System.UInt16 key, Unity.Entities.Entity entity, System.Int32 count)
```
