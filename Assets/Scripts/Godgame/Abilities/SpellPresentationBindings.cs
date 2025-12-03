using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Abilities
{
    /// <summary>
    /// Presentation binding for a spell (FX references, icon token, style tokens, sockets).
    /// </summary>
    [System.Serializable]
    public struct SpellPresentationBinding
    {
        [Tooltip("Spell ID this binding applies to")]
        public string SpellId;

        [Tooltip("Start FX prefab reference")]
        public GameObject StartFX;

        [Tooltip("Loop FX prefab reference (for channeled spells)")]
        public GameObject LoopFX;

        [Tooltip("Impact FX prefab reference")]
        public GameObject ImpactFX;

        [Tooltip("Sound effect clip")]
        public AudioClip SFX;

        [Tooltip("Icon token prefab (optional)")]
        public GameObject IconToken;

        [Tooltip("Style tokens (for visual theming)")]
        public string[] StyleTokens;

        [Tooltip("Socket names (e.g., Socket_Hand_R)")]
        public string[] Sockets;
    }

    /// <summary>
    /// Binding set (Minimal or Fancy) containing spell presentation bindings.
    /// </summary>
    [CreateAssetMenu(fileName = "SpellBindingSet", menuName = "Godgame/Spell Binding Set")]
    public sealed class SpellBindingSet : ScriptableObject
    {
        [Tooltip("Binding set name")]
        public string SetName;

        [Tooltip("Binding set type (Minimal/Fancy)")]
        public BindingSetType Type;

        [Tooltip("Spell presentation bindings")]
        public SpellPresentationBinding[] Bindings = System.Array.Empty<SpellPresentationBinding>();

        public enum BindingSetType
        {
            Minimal,
            Fancy
        }
    }

    /// <summary>
    /// Runtime component storing active binding set reference.
    /// </summary>
    public struct ActiveSpellBindingSet : IComponentData
    {
        public BlobAssetReference<SpellBindingSetBlob> BindingSet;
    }

    /// <summary>
    /// Blob representation of binding set for runtime lookup.
    /// </summary>
    public struct SpellBindingSetBlob
    {
        public FixedString64Bytes SetName;
        public byte Type; // 0 = Minimal, 1 = Fancy
        public BlobArray<SpellBindingBlob> Bindings;
    }

    /// <summary>
    /// Blob representation of a single spell binding.
    /// </summary>
    public struct SpellBindingBlob
    {
        public FixedString64Bytes SpellId;
        public FixedString64Bytes StartFXPath;
        public FixedString64Bytes LoopFXPath;
        public FixedString64Bytes ImpactFXPath;
        public FixedString64Bytes SFXPath;
        public FixedString64Bytes IconTokenPath;
        public BlobArray<FixedString32Bytes> StyleTokens;
        public BlobArray<FixedString32Bytes> Sockets;
    }
}

