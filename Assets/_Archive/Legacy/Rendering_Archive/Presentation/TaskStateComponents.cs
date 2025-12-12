#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
namespace Godgame.Presentation
{
    /// <summary>
    /// Task state enum for villagers, used in VillagerVisualState.TaskState.
    /// </summary>
    public enum VillagerTaskState : byte
    {
        None,
        Idle,
        Walking,
        Carrying,
        Working,
        Fighting,
        Fleeing,
        Gathering,
        MiracleAffected,
    }

    /// <summary>
    /// Aggregate state enum for villages, used in VillageCenterVisualState.
    /// </summary>
    public enum VillageAggregateState : byte
    {
        Normal,
        UnderMiracle,
        Crisis,
        Starving,
        Prosperous,
    }
}
#endif
