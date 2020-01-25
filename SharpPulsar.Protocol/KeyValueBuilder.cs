using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Protocol
{
    public class KeyValueBuilder
    {
        private KeyValue _value;
        public KeyValueBuilder()
        {
            _value = new KeyValue();
        }
        public KeyValueBuilder SetKey(string key)
        {
            _value.Key = key;
            return this;
        }
        public KeyValueBuilder SetValue(string value)
        {
            _value.Value = value;
            return this;
        }
        public KeyValue Build()
        {
            return _value;
        }
    }
}
