using UnityEngine;
using System;

namespace Godgame.Presentation
{
    [CreateAssetMenu(menuName = "Godgame/Bindings Set")]
    public class PresentationBindingSet : ScriptableObject
    {
        public BindingEntry[] Bindings;
    }

    [Serializable]
    public class BindingEntry
    {
        public string EffectId;        // e.g., "FX.Miracle.Ping"
        public GameObject Prefab;      // Visual prefab
        public Material Material;       // Optional material override
        public ParticleSystem VFX;      // Optional VFX
    }
}
