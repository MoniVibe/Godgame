using PureDOTS.Runtime.Components;
using Unity.Entities;

namespace Godgame.Runtime
{
    /// <summary>
    /// Minimal miracle token used by the DivineHand pipeline to detect "held miracle" and emit MiracleReleaseEvent.
    /// </summary>
    public struct MiracleToken : IComponentData
    {
        public MiracleType Type;
        public Entity ConfigEntity;
    }
}

