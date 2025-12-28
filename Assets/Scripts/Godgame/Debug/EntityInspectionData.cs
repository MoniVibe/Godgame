using Unity.Entities;
using Unity.Collections;

namespace Godgame.Debugging
{
    /// <summary>
    /// Component storing inspection data for selected entities.
    /// Written by Godgame_EntityInspectionSystem, read by EntityInspectionUI.
    /// </summary>
    public struct EntityInspectionData : IComponentData
    {
        /// <summary>Currently inspected entity</summary>
        public Entity InspectedEntity;
        /// <summary>Summary text with key sim data</summary>
        public FixedString512Bytes Summary;
        /// <summary>Whether inspection data is valid (0/1)</summary>
        public byte IsValid;
    }

    /// <summary>
    /// Tag component identifying the inspection singleton entity.
    /// </summary>
    public struct EntityInspectionSingleton : IComponentData { }
}
