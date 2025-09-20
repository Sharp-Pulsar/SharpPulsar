using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Protocol
{
    public class KeyValueBuilder
    {
        private readonly KeyValue _value;
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
