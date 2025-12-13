using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// ScriptableObject registry mapping presentation descriptor keys to prefabs/tints.
    /// </summary>
    [CreateAssetMenu(fileName = "PresentationRegistry", menuName = "Godgame/Presentation Registry")]
    public sealed class PresentationRegistry : ScriptableObject
    {
        [Serializable]
        public struct Descriptor
        {
            public string descriptorKey;
            public GameObject prefab;
            public Vector3 defaultOffset;
            public float defaultScale;
            public Color defaultTint;
            public int defaultFlags;
        }

        [Serializable]
        public struct SwappableEntry
        {
            public string key;
            public string description;
        }

        [SerializeField] private List<Descriptor> descriptors = new();
        [SerializeField] private List<SwappableEntry> m_Entries = new();

        public IEnumerable<string> Keys => descriptors.Select(d => d.descriptorKey)
            .Concat(m_Entries.Select(e => e.key))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim())
            .Distinct();

        public bool TryGetDescriptor(string key, out Descriptor descriptor)
        {
            for (int i = 0; i < descriptors.Count; i++)
            {
                if (string.Equals(descriptors[i].descriptorKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    descriptor = descriptors[i];
                    return true;
                }
            }

            descriptor = default;
            return false;
        }
    }
}














