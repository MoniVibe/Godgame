using Unity.Collections;
using Unity.Entities;

namespace Godgame.Buildings
{
    /// <summary>
    /// Defines a building as a utility structure providing bonuses.
    /// </summary>
    public struct UtilityDefinition : IComponentData
    {
        public float AreaBonusRange;
        public float BonusValue;
        public FixedString32Bytes BonusType;
    }
}
