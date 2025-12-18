using System.Globalization;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Telemetry
{
    internal sealed class GodgameTelemetryJsonWriter
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        private readonly StringBuilder _builder;
        private bool _needsComma;

        public GodgameTelemetryJsonWriter(int capacity = 256)
        {
            _builder = new StringBuilder(capacity);
            _builder.Append('{');
            _needsComma = false;
        }

        private void AppendComma()
        {
            if (_needsComma)
            {
                _builder.Append(',');
            }
            else
            {
                _needsComma = true;
            }
        }

        public void AddString(string key, string value)
        {
            AppendComma();
            _builder.Append('\"').Append(key).Append("\":\"").Append(value).Append('\"');
        }

        public void AddInt(string key, int value)
        {
            AppendComma();
            _builder.Append('\"').Append(key).Append("\":").Append(value);
        }

        public void AddUInt(string key, uint value)
        {
            AppendComma();
            _builder.Append('\"').Append(key).Append("\":").Append(value);
        }

        public void AddBool(string key, bool value)
        {
            AppendComma();
            _builder.Append('\"').Append(key).Append("\":").Append(value ? "true" : "false");
        }

        public void AddEntity(string prefix, Entity entity)
        {
            AddInt($"{prefix}Id", entity.Index);
            AddInt($"{prefix}Ver", entity.Version);
        }

        public FixedString128Bytes Build()
        {
            _builder.Append('}');
            return ToFixedString128(_builder);
        }

        private static FixedString128Bytes ToFixedString128(StringBuilder builder)
        {
            var str = builder.ToString();
            var result = new FixedString128Bytes();
            if (str.Length >= result.Capacity)
            {
                var truncated = str.Substring(0, result.Capacity - 1);
                result.Append(truncated);
#if UNITY_EDITOR
                Debug.LogWarning($"[GodgameTelemetryJsonWriter] Payload truncated to {result.Capacity - 1} characters.");
#endif
            }
            else
            {
                result.Append(str);
            }

            return result;
        }
    }
}
