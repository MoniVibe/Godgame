#if UNITY_BURST
using Unity.Burst;

// Disable Burst compilation when running the test assembly to avoid managed call restrictions.
[UnityEngine.Scripting.Preserve]
internal static class DisableBurstForTests
{
    static DisableBurstForTests()
    {
        BurstCompiler.Options.EnableBurstCompilation = false;
    }
}
#endif
