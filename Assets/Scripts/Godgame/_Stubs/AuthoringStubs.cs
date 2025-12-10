#if UNITY_EDITOR
using System;
using Godgame.Economy;
using Godgame.Runtime;
using Godgame.Runtime.Interaction;

// Editor-only helpers that expose real runtime enums to authoring/inspector code.
// No stub enums or duplicate definitions -- keeps serialization consistent.
namespace Godgame.Authoring
{
    public static class ResourceTypeAuthoring
    {
        public static ResourceType[] All => (ResourceType[])Enum.GetValues(typeof(ResourceType));
    }

    public static class HandStateAuthoring
    {
        public static HandState[] All => (HandState[])Enum.GetValues(typeof(HandState));
    }

    public static class InteractionTagAuthoring
    {
        public static InteractionTag Tag => default;
    }

}
#endif
