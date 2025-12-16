using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Shared steering helpers used by flee and hazard avoidance logic so both paths stay in sync.
    /// </summary>
    public static class VillagerSteeringMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 BlendDirection(float3 desiredDirection, float3 avoidanceVector, float avoidanceUrgency)
        {
            var desired = math.normalizesafe(desiredDirection, new float3(0f, 0f, 1f));
            if (math.lengthsq(avoidanceVector) < 1e-5f || avoidanceUrgency <= 0f)
            {
                return desired;
            }

            var avoidance = math.normalizesafe(avoidanceVector, desired);
            var weight = math.saturate(avoidanceUrgency);
            var blended = math.normalize(math.lerp(desired, avoidance, weight));
            return blended;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ComputeFleeDestination(float3 origin, float3 fleeDirection, float fleeDistance)
        {
            var dir = math.normalizesafe(fleeDirection, new float3(0f, 0f, 1f));
            return origin + dir * math.max(1f, fleeDistance);
        }
    }
}
