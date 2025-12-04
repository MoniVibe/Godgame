using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Tag component used to mark canonical PureDOTS components that were created by bridge systems.
    /// This allows bridge systems to safely clean up canonical components when the source Godgame component is removed.
    /// </summary>
    public struct SyncedFromGodgame : IComponentData
    {
    }

    /// <summary>
    /// Tag applied to entities when running playback/rewind so sync systems can skip them safely.
    /// </summary>
    public struct PlaybackGuardTag : IComponentData
    {
    }
}
