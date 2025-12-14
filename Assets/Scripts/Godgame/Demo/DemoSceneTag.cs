using Unity.Entities;

namespace Godgame.Demo
{
    /// <summary>
    /// Tag component baked into demo subscenes so demo-only systems can gate themselves.
    /// </summary>
    public struct DemoSceneTag : IComponentData
    {
    }
}
