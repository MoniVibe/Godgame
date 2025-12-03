#if UNITY_EDITOR
using Unity.Entities;

// Temporary editor-only placeholders to unblock authoring assembly compilation.
namespace Godgame.Camera
{
    public enum CameraMode
    {
        Default = 0
    }
}

namespace Godgame.Interaction
{
    public struct InteractionTag : IComponentData {}

    public enum HandState : byte
    {
        Empty = 0,
        Carrying = 1,
        Charging = 2,
        Cooldown = 3
    }
}

namespace Godgame.Resources
{
    public enum ResourceType : ushort
    {
        None = 0,
        Wood = 1,
        Ore = 2
    }
}
#endif
