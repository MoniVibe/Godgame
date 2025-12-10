using Unity.Entities;

namespace Godgame.Buildings
{
    /// <summary>
    /// Defines a building as a worship site.
    /// </summary>
    public struct WorshipDefinition : IComponentData
    {
        public float ManaGenerationRate;
        public float WorshipperCapacity;
    }
}
