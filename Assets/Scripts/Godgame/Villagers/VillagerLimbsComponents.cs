using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Limb health and injury tracking for a villager.
    /// </summary>
    public struct VillagerLimb : IBufferElementData
    {
        /// <summary>
        /// Limb identifier (e.g., "Head", "LeftArm", "RightLeg").
        /// </summary>
        public FixedString64Bytes LimbId;

        /// <summary>
        /// Limb health percentage (0-100).
        /// </summary>
        public byte Health;

        /// <summary>
        /// Flags indicating permanent injuries (e.g., LostEye, CrippledArm).
        /// Bit flags: 0x01 = Lost, 0x02 = Crippled, 0x04 = Maimed, etc.
        /// </summary>
        public byte InjuryFlags;
    }

    /// <summary>
    /// Implant/prosthetic tracking for a villager.
    /// </summary>
    public struct VillagerImplant : IBufferElementData
    {
        /// <summary>
        /// Implant identifier (e.g., "WoodenLeg", "IronArm").
        /// </summary>
        public FixedString64Bytes ImplantId;

        /// <summary>
        /// Which limb this implant is attached to (empty = body).
        /// </summary>
        public FixedString64Bytes AttachedToLimb;

        /// <summary>
        /// Implant quality/effectiveness (0-100).
        /// </summary>
        public byte Quality;
    }
}


