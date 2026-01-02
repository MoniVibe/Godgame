using Unity.Collections;
using Unity.Entities;

namespace Godgame.Mana
{
    public enum ManaAllocationState : byte
    {
        Disabled = 0,
        Standby = 1,
        Normal = 2,
        Overcharged = 3,
        Max = 4
    }

    public enum ManaFocusMode : byte
    {
        Balanced = 0,
        Weapons = 1,
        Defense = 2,
        Mobility = 3,
        Stealth = 4,
        Emergency = 5
    }

    public enum ManaModuleCategory : byte
    {
        Weapons = 0,
        Shields = 1,
        Mobility = 2,
        Sensors = 3,
        Stealth = 4,
        Support = 5,
        Utility = 6
    }

    public struct ManaFocus : IComponentData
    {
        public ManaFocusMode Mode;
    }

    public struct ManaFocusCommand : IComponentData
    {
        public ManaFocusMode Mode;
    }

    public struct ManaModuleConfig : IComponentData
    {
        public ManaModuleCategory Category;
        public FixedString64Bytes LimbId;
        public float BaseBurnoutRiskMultiplier;
        public float RampUpPerSecond;
        public float RampDownPerSecond;
    }

    public struct ManaModuleState : IComponentData
    {
        public ManaAllocationState State;
    }

    public static class ManaAllocationUtility
    {
        public const float DisabledPercent = 0f;
        public const float StandbyPercent = 25f;
        public const float NormalPercent = 100f;
        public const float OverchargedPercent = 150f;
        public const float MaxPercent = 200f;

        public static float ToPercent(ManaAllocationState state)
        {
            return state switch
            {
                ManaAllocationState.Disabled => DisabledPercent,
                ManaAllocationState.Standby => StandbyPercent,
                ManaAllocationState.Normal => NormalPercent,
                ManaAllocationState.Overcharged => OverchargedPercent,
                ManaAllocationState.Max => MaxPercent,
                _ => NormalPercent
            };
        }
    }
}
