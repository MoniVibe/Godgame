// [TRI-STUB] This is an ahead-of-time stub. Safe to compile, does nothing at runtime.
using PureDOTS.Runtime.Interception;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Stubs
{
    public static class GodgameInterceptBridgeStub
    {
        public static void RequestIntercept(EntityManager manager, Entity interceptor, Entity target, float3 lastKnownPos, float3 velocityEstimate)
        {
            InterceptServiceStub.RequestIntercept(manager, interceptor, target, lastKnownPos, velocityEstimate);
        }
    }
}
