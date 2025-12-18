using Unity.Entities;

namespace Godgame.Scenario
{
    /// <summary>
    /// Tag component baked into scenario subscenes so scenario-only systems can gate themselves.
    /// </summary>
    public struct ScenarioSceneTag : IComponentData
    {
    }
}
