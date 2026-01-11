#if GODGAME_HAS_DIGGER
using System;
using System.Reflection;
using PureDOTS.Environment;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation.Digger
{
    public sealed class DiggerDigOpApplier : MonoBehaviour
    {
        private static readonly Action DebugCallback = () => { };
        private const string DiggerMasterRuntimeTypeName = "Digger.Modules.Runtime.Sources.DiggerMasterRuntime, Digger.Runtime";
        private const string ModificationParametersTypeName = "Digger.Modules.Core.Sources.ModificationParameters, Digger.Core";
        private const string ActionTypeName = "Digger.Modules.Core.Sources.ActionType, Digger.Core";
        private const string BrushTypeName = "Digger.Modules.Core.Sources.BrushType, Digger.Core";

        [SerializeField] private int maxOpsPerFrame = 32;
        [SerializeField] private bool useAsync = true;
        [SerializeField] private bool enableDebugMetrics;

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _queueQuery;
        private bool _queueQueryReady;

        private object _diggerMaster;
        private MethodInfo _modifyMethod;
        private Type _modificationParametersType;
        private Type _actionType;
        private Type _brushType;

        private FieldInfo _positionField;
        private FieldInfo _brushField;
        private FieldInfo _actionField;
        private FieldInfo _textureIndexField;
        private FieldInfo _opacityField;
        private FieldInfo _sizeField;
        private FieldInfo _stalagmiteUpsideDownField;
        private FieldInfo _opacityIsTargetField;
        private FieldInfo _customBrushField;
        private FieldInfo _callbackField;
        private PropertyInfo _modificationResultProperty;

        private float _nextDiggerLookupTime;
        private int _appliedOps;
        private int _skippedOps;
        private uint _lastTick;
        private Vector3 _lastPosition;

        public int AppliedOps => _appliedOps;
        public int SkippedOps => _skippedOps;
        public uint LastTick => _lastTick;
        public Vector3 LastPosition => _lastPosition;

        private void Awake()
        {
            if (!DiggerViewGate.IsEnabled)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (!DiggerViewGate.IsEnabled)
            {
                return;
            }

            if (!TryResolveWorld())
            {
                return;
            }

            if (!TryResolveDiggerMaster())
            {
                DrainQueue();
                return;
            }

            if (_queueQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var queueEntity = _queueQuery.GetSingletonEntity();
            var buffer = _entityManager.GetBuffer<DiggerDigOp>(queueEntity);
            if (buffer.Length == 0)
            {
                return;
            }

            var processed = Mathf.Min(Mathf.Max(1, maxOpsPerFrame), buffer.Length);
            for (var i = 0; i < processed; i++)
            {
                var op = buffer[i];
                if (!ApplyOp(op))
                {
                    _skippedOps++;
                }
            }

            buffer.RemoveRange(0, processed);
        }

        private bool TryResolveWorld()
        {
            if (_world != null && _world.IsCreated)
            {
                if (IsPresentationWorld(_world))
                {
                    return true;
                }
            }

            _world = World.DefaultGameObjectInjectionWorld;
            if (_world == null || !_world.IsCreated || !IsPresentationWorld(_world))
            {
                foreach (var candidate in World.All)
                {
                    if (candidate != null && candidate.IsCreated && IsPresentationWorld(candidate))
                    {
                        _world = candidate;
                        break;
                    }
                }
            }

            if (_world == null || !_world.IsCreated || !IsPresentationWorld(_world))
            {
                return false;
            }

            _entityManager = _world.EntityManager;
            _queueQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<DiggerDigOpQueue>(), ComponentType.ReadWrite<DiggerDigOp>());
            _queueQueryReady = true;
            return true;
        }

        private static bool IsPresentationWorld(World world)
        {
            var flags = world.Flags;
            if ((flags & WorldFlags.Game) == 0 && (flags & WorldFlags.GameClient) == 0)
            {
                return false;
            }

            if ((flags & WorldFlags.GameServer) != 0)
            {
                return false;
            }

            return true;
        }

        private bool TryResolveDiggerMaster()
        {
            if (_diggerMaster != null && _modifyMethod != null)
            {
                return true;
            }

            if (UnityEngine.Time.unscaledTime < _nextDiggerLookupTime)
            {
                return false;
            }

            _nextDiggerLookupTime = UnityEngine.Time.unscaledTime + 1f;

            var diggerType = ResolveDiggerType(DiggerMasterRuntimeTypeName);
            if (diggerType == null)
            {
                return false;
            }

            _diggerMaster = FindObjectOfType(diggerType);
            if (_diggerMaster == null)
            {
                return false;
            }

            _modificationParametersType = ResolveDiggerType(ModificationParametersTypeName);
            _actionType = ResolveDiggerType(ActionTypeName);
            _brushType = ResolveDiggerType(BrushTypeName);

            if (_modificationParametersType == null || _actionType == null || _brushType == null)
            {
                _diggerMaster = null;
                return false;
            }

            _modifyMethod = useAsync
                ? diggerType.GetMethod("ModifyAsyncBuffured", new[] { _modificationParametersType })
                : diggerType.GetMethod("Modify", new[] { _modificationParametersType });

            if (_modifyMethod == null && useAsync)
            {
                _modifyMethod = diggerType.GetMethod("Modify", new[] { _modificationParametersType });
            }

            if (_modifyMethod == null)
            {
                _diggerMaster = null;
                return false;
            }

            _positionField = _modificationParametersType.GetField("Position");
            _brushField = _modificationParametersType.GetField("Brush");
            _actionField = _modificationParametersType.GetField("Action");
            _textureIndexField = _modificationParametersType.GetField("TextureIndex");
            _opacityField = _modificationParametersType.GetField("Opacity");
            _sizeField = _modificationParametersType.GetField("Size");
            _stalagmiteUpsideDownField = _modificationParametersType.GetField("StalagmiteUpsideDown");
            _opacityIsTargetField = _modificationParametersType.GetField("OpacityIsTarget");
            _customBrushField = _modificationParametersType.GetField("CustomBrush");
            _callbackField = _modificationParametersType.GetField("Callback");
            _modificationResultProperty = diggerType.GetProperty("ModificationResult")
                ?? diggerType.GetProperty("LastModificationResult");

            return _positionField != null
                && _brushField != null
                && _actionField != null
                && _textureIndexField != null
                && _opacityField != null
                && _sizeField != null
                && _stalagmiteUpsideDownField != null
                && _opacityIsTargetField != null
                && _customBrushField != null
                && _callbackField != null;
        }

        private void DrainQueue()
        {
            if (!_queueQueryReady || _queueQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var queueEntity = _queueQuery.GetSingletonEntity();
            var buffer = _entityManager.GetBuffer<DiggerDigOp>(queueEntity);
            buffer.Clear();
        }

        private static Type ResolveDiggerType(string typeName)
        {
            return Type.GetType(typeName, false);
        }

        private bool ApplyOp(in DiggerDigOp op)
        {
            if (_diggerMaster == null || _modifyMethod == null)
            {
                return false;
            }

            var position = op.Shape == TerrainModificationShape.Brush
                ? op.Start
                : (op.Start + op.End) * 0.5f;

            var size = math.max(op.Radius, op.Depth);
            if (size <= 0f)
            {
                size = 0.5f;
            }

            var parameters = Activator.CreateInstance(_modificationParametersType);
            if (parameters == null)
            {
                return false;
            }

            var actionValue = ResolveActionValue(op.Kind);
            var brushValue = Enum.ToObject(_brushType, 0);

            _positionField.SetValue(parameters, position);
            _brushField.SetValue(parameters, brushValue);
            _actionField.SetValue(parameters, Enum.ToObject(_actionType, actionValue));
            _textureIndexField.SetValue(parameters, Mathf.Clamp((int)op.MaterialId, 0, 7));
            _opacityField.SetValue(parameters, 1f);
            _sizeField.SetValue(parameters, new float3(size));
            _stalagmiteUpsideDownField.SetValue(parameters, false);
            _opacityIsTargetField.SetValue(parameters, false);
            _customBrushField.SetValue(parameters, null);
            _callbackField.SetValue(parameters, enableDebugMetrics ? DebugCallback : null);

            _modifyMethod.Invoke(_diggerMaster, new[] { parameters });

            _appliedOps++;
            _lastTick = op.Tick;
            _lastPosition = new Vector3(position.x, position.y, position.z);

            if (enableDebugMetrics && _modificationResultProperty != null)
            {
                _ = _modificationResultProperty.GetValue(_diggerMaster);
            }

            return true;
        }

        private static int ResolveActionValue(TerrainModificationKind kind)
        {
            switch (kind)
            {
                case TerrainModificationKind.Fill:
                    return 1;
                case TerrainModificationKind.PaintMaterial:
                    return 2;
                case TerrainModificationKind.Dig:
                case TerrainModificationKind.Carve:
                default:
                    return 0;
            }
        }
    }
}
#else
using UnityEngine;

namespace Godgame.Presentation.Digger
{
    public sealed class DiggerDigOpApplier : MonoBehaviour
    {
        public int AppliedOps => 0;
        public int SkippedOps => 0;
        public uint LastTick => 0;
        public Vector3 LastPosition => default;

        private void Awake()
        {
            enabled = false;
        }
    }
}
#endif
