using Unity.Entities;

namespace Godgame.Villagers
{
    public struct VillagerAIState : IComponentData
    {
        public enum State : byte
        {
            Idle = 0,
            Working = 1
        }

        public enum Goal : byte
        {
            Work = 0,
            Rest = 1
        }

        public State CurrentState;
        public Goal CurrentGoal;
        public Entity TargetEntity;
    }
}
