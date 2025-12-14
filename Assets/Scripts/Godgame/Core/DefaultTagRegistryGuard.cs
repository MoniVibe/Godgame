using UnityEngine;

namespace Godgame.Core
{
    public static class DefaultTagRegistryGuard
    {
        private static bool _done;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _done = false;
        }

        public static bool TryEnter()
        {
            if (_done) return false;
            _done = true;
            return true;
        }
    }
}
