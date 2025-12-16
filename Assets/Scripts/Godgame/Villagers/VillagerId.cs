using Unity.Entities;

namespace Godgame.Villagers
{
    public struct VillagerId : IComponentData
    {
        public int Value;
        public int FactionId;
    }
}
