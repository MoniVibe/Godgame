using Unity.Collections;
using Unity.Entities;

namespace Godgame.Modules
{
    /// <summary>
    /// Canonical slot ids for Godgame equipment/upgrade points plus helpers to seed buffers.
    /// </summary>
    public static class ModuleSlotIds
    {
        // Villager equipment
        public static readonly FixedString64Bytes HeadHelm = "slot.head.helm";
        public static readonly FixedString64Bytes NeckAmulet = "slot.neck.amulet";
        public static readonly FixedString64Bytes TorsoUnderlayer = "slot.torso.underlayer"; // shirt
        public static readonly FixedString64Bytes TorsoArmor = "slot.torso.armor";
        public static readonly FixedString64Bytes ArmsBracers = "slot.arms.bracers";
        public static readonly FixedString64Bytes HandsGloves = "slot.hands.gloves";
        public static readonly FixedString64Bytes MainHand = "slot.hands.main";
        public static readonly FixedString64Bytes OffHand = "slot.hands.off";
        public static readonly FixedString64Bytes Sidearm = "slot.hands.sidearm"; // wand/crossbow/slingshot
        public static readonly FixedString64Bytes LegsPants = "slot.legs.pants";
        public static readonly FixedString64Bytes LegsLeggings = "slot.legs.leggings";
        public static readonly FixedString64Bytes FeetBoots = "slot.feet.boots";
        public static readonly FixedString64Bytes FeetSocks = "slot.feet.socks";
        public static readonly FixedString64Bytes RingLeft = "slot.ring.left";
        public static readonly FixedString64Bytes RingRight = "slot.ring.right";
        public static readonly FixedString64Bytes TrinketA = "slot.trinket.a";
        public static readonly FixedString64Bytes TrinketB = "slot.trinket.b";
        public static readonly FixedString64Bytes UsableA = "slot.usable.a";
        public static readonly FixedString64Bytes UsableB = "slot.usable.b";
        public static readonly FixedString64Bytes UsableC = "slot.usable.c";
        public static readonly FixedString64Bytes Backpack = "slot.backpack";
        public static readonly FixedString64Bytes Cloak = "slot.cloak";
        public static readonly FixedString64Bytes Mount = "slot.mount";

        // Wagon/vehicle slots
        public static readonly FixedString64Bytes WagonMountA = "slot.wagon.mount.a";
        public static readonly FixedString64Bytes WagonMountB = "slot.wagon.mount.b";
        public static readonly FixedString64Bytes WagonCargo = "slot.wagon.cargo";
        public static readonly FixedString64Bytes WagonPersonnel = "slot.wagon.personnel";
        public static readonly FixedString64Bytes WagonMaterial = "slot.wagon.material";
        public static readonly FixedString64Bytes WagonDecor = "slot.wagon.decor";

        // Building slots
        public static readonly FixedString64Bytes BuildingMaterial = "slot.building.material";
        public static readonly FixedString64Bytes BuildingDecorA = "slot.building.decor.a";
        public static readonly FixedString64Bytes BuildingDecorB = "slot.building.decor.b";

        public static void AddVillagerSlots(DynamicBuffer<ModuleSlot> buffer)
        {
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = HeadHelm });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = NeckAmulet });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = TorsoUnderlayer });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = TorsoArmor });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = ArmsBracers });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = HandsGloves });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = MainHand });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = OffHand });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = Sidearm });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = LegsPants });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = LegsLeggings });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = FeetBoots });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = FeetSocks });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = RingLeft });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = RingRight });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = TrinketA });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = TrinketB });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = UsableA });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = UsableB });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = UsableC });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = Backpack });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = Cloak });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = Mount });
        }

        public static void AddWagonSlots(DynamicBuffer<ModuleSlot> buffer)
        {
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = WagonMountA });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = WagonMountB });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = WagonCargo });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = WagonPersonnel });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = WagonMaterial });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = WagonDecor });
        }

        public static void AddBuildingSlots(DynamicBuffer<ModuleSlot> buffer)
        {
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = BuildingMaterial });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = BuildingDecorA });
            buffer.Add(new ModuleSlot { SlotIndex = (byte)buffer.Length, SlotType = BuildingDecorB });
        }
    }
}
