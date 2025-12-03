using Unity.Entities;

namespace Godgame.Time
{
    /// <summary>
    /// Time control input snapshot component for HUD and time system consumption.
    /// </summary>
    public struct TimeControlInput : IComponentData
    {
        public byte TogglePause;
        public byte Step;
        public float SpeedDelta;
        public byte RewindHold;
    }
}

