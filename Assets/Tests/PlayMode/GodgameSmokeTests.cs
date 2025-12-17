using System.Collections;
using NUnit.Framework;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GodgameSmokeTests
{
    [UnityTest]
    public IEnumerator SmokeScene_HasCoreSingletonsAndRenderables()
    {
        yield return SceneManager.LoadSceneAsync("TRI_Godgame_Smoke", LoadSceneMode.Single);
        yield return null;

        var world = World.DefaultGameObjectInjectionWorld;
        Assert.IsNotNull(world, "Default world missing");
        var em = world.EntityManager;

        bool hasTime = em.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).CalculateEntityCount() > 0;
        bool hasTick = em.CreateEntityQuery(ComponentType.ReadOnly<TickTimeState>()).CalculateEntityCount() > 0;
        bool hasRewind = em.CreateEntityQuery(ComponentType.ReadOnly<RewindState>()).CalculateEntityCount() > 0;

        Assert.IsTrue(hasTime && hasTick && hasRewind, "Missing time/rewind singletons");

        var renderQuery = em.CreateEntityQuery(
            ComponentType.ReadOnly<RenderSemanticKey>(),
            ComponentType.ReadOnly<LocalTransform>());
        Assert.Greater(renderQuery.CalculateEntityCount(), 0, "No renderable entities with RenderSemanticKey + Transform");
    }

    [UnityTest]
    public IEnumerator VillagerLoop_AdvancesTick()
    {
        yield return SceneManager.LoadSceneAsync("TRI_Godgame_Smoke", LoadSceneMode.Single);
        yield return null;

        var world = World.DefaultGameObjectInjectionWorld;
        Assert.IsNotNull(world, "Default world missing");
        var em = world.EntityManager;

        var tickQuery = em.CreateEntityQuery(ComponentType.ReadOnly<TickTimeState>());
        using var beforeTicks = tickQuery.ToComponentDataArray<TickTimeState>(Allocator.Temp);
        var startTick = beforeTicks.Length > 0 ? beforeTicks[0].Tick : 0u;

        yield return null;
        yield return null;

        using var afterTicks = tickQuery.ToComponentDataArray<TickTimeState>(Allocator.Temp);
        var endTick = afterTicks.Length > 0 ? afterTicks[0].Tick : startTick;

        Assert.Greater(endTick, startTick, "TickTimeState did not advance across frames");
    }
}
