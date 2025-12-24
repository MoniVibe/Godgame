using Unity.Entities;

namespace Godgame.Construction
{
    /// <summary>
    /// Tracks outstanding resource requirements for construction targets.
    /// </summary>
    public struct ConstructionIntake : IComponentData
    {
        public ushort ResourceTypeIndex;
        public int Cost;
        public int Paid;
    }
}



