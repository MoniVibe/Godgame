using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Desired workforce ratios for a village.
    /// </summary>
    public struct VillageWorkforceProfile : IComponentData
    {
        public float ForesterRatio;
        public float MinerRatio;
        public float FarmerRatio;
        public float BuilderRatio;
        public float BreederRatio;
        public float HaulerRatio;
        public float ReassignmentCooldownDays;
        public uint LastAssignmentTick;
    }
}
