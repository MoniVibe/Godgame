using Unity.Entities;

namespace Godgame
{
    /// <summary>
    /// Temporary stub so demo UI and telemetry compile.
    /// Values will all be zero until a real logistics pipeline is wired.
    /// </summary>
    public struct LogisticsRequestRegistry : IComponentData
    {
        public int TotalRequests;
        public int PendingRequests;
        public int InProgressRequests;
        public int CriticalRequests;

        public int TotalRequestedUnits;
        public int TotalRemainingUnits;
    }
}

