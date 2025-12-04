using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Logistics
{
    /// <summary>
    /// Priority levels for transport orders.
    /// </summary>
    public enum TransportPriority : byte
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// Flags for transport order state.
    /// </summary>
    [Flags]
    public enum TransportFlags : byte
    {
        None = 0,
        Urgent = 1 << 0,
        Blocking = 1 << 1,
        PlayerPinned = 1 << 2
    }

    /// <summary>
    /// Preferred transport vehicle type for the order.
    /// </summary>
    public enum TransportVehicleType : byte
    {
        Any = 0,
        Wagon = 1,
        Hauler = 2,
        Freighter = 3,
        MinerVessel = 4
    }

    /// <summary>
    /// Godgame-specific transport order component.
    /// Bridges to PureDOTS LogisticsRequest component.
    /// </summary>
    public struct TransportOrder : IComponentData
    {
        public Entity SourceEntity;
        public Entity DestinationEntity;
        public float3 SourcePosition;
        public float3 DestinationPosition;
        public ushort ResourceTypeIndex; // Maps to PureDOTS ResourceTypeIndex
        public float RequestedUnits;
        public float FulfilledUnits;
        public TransportPriority Priority;
        public TransportFlags Flags;
        public uint CreatedTick;
        public uint LastUpdateTick;
        
        // Godgame-specific fields
        public TransportVehicleType PreferredVehicle; // Preferred transport type
        public float MaxDistance;                    // Maximum allowed transport distance
        public FixedString64Bytes OrderLabel;         // Human-readable label for UI
    }

    /// <summary>
    /// Waypoint buffer for complex transport routes.
    /// </summary>
    public struct TransportWaypoint : IBufferElementData
    {
        public float3 Position;
        public Entity WaypointEntity;  // Optional entity reference
        public float WaitTime;         // Time to wait at this waypoint
    }
}

