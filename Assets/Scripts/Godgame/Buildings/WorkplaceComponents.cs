using Unity.Entities;

namespace Godgame.Buildings
{
    /// <summary>
    /// Defines a building as a workplace.
    /// </summary>
    public struct WorkplaceDefinition : IComponentData
    {
        public float WorkCapacity;
        public float EfficiencyMultiplier;
    }
}
