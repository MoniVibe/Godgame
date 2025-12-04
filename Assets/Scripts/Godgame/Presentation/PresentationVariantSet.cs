using System;
using System.Collections.Generic;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Group of candidate prefabs/material overrides keyed by a descriptor string.
    /// </summary>
    [CreateAssetMenu(fileName = "PresentationVariantSet", menuName = "Godgame/Presentation/Variant Set")]
    public sealed class PresentationVariantSet : ScriptableObject
    {
        [Serializable]
        public struct Variant
        {
            public string id;
            public string prefabAddress;
            public GameObject prefab;
            public string materialAddress;
            public Material materialOverride;
            public string vfxGraphId;
            public List<string> requiredTags;
            public List<string> forbiddenTags;
            public float weight;
        }

        [SerializeField] PresentationRegistry m_Registry;
        [SerializeField] string m_DescriptorKey;
        [SerializeField] List<Variant> m_Variants = new();

        public PresentationRegistry Registry => m_Registry;
        public string DescriptorKey => m_DescriptorKey;
        public IReadOnlyList<Variant> Variants => m_Variants;
    }
}
