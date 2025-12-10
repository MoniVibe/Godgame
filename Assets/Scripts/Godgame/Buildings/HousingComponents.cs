using Unity.Entities;

namespace Godgame.Buildings
{
    /// <summary>
    /// Defines a building as housing for villagers.
    /// </summary>
    public struct HousingDefinition : IComponentData
    {
        public int MaxResidents;
        public float ComfortLevel;
        public float RestorationRate;
    }
}
