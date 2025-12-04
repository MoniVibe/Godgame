using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Helper class for loading resources, forwarding to UnityEngine.Resources.
    /// Note: This is separate from the Godgame.Resources namespace which contains resource-related game code.
    /// </summary>
    public static class ResourcesHelper
    {
        public static T Load<T>(string path) where T : Object
            => UnityEngine.Resources.Load<T>(path);
    }
}
