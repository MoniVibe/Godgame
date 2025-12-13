using PureDOTS.Runtime.Resource;
using Unity.Entities;
using Unity.Collections;

namespace Godgame.Resources
{
    /// <summary>
    /// Configuration for a resource source.
    /// </summary>
    public struct ResourceSourceConfig : IComponentData
    {
        public FixedString64Bytes ResourceTypeId;
        public float Amount;
        public float MaxAmount;
        public float RegenRate;
    }
}
