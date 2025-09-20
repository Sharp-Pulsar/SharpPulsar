using SharpPulsar.API;
using SharpPulsar.API.Internal;
using SharpPulsar.Common.Schema;
using SharpPulsar.Extension;
using SharpPulsar.Protocol.Extension;
using SharpPulsar.Shared;
using System;
using System.IO;
using System.Text;

namespace SharpPulsar.Common.Schema
{
    public class KeyValue<TK, TV>: IKeyValue<TK, TV>
    {
		private readonly TK _key;
		private readonly TV _value;

		public KeyValue(TK key, TV value)
		{
			_key = key;
			_value = value;
		}

		public virtual TK Key => _key;

		public virtual TV Value => _value;

		public override int GetHashCode()
		{
			return HashCode.Combine(_key, _value);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is KeyValue<TK, TV>))
			{
				return false;
			}
			var another = (KeyValue<TK, TV>)obj;
			return Equals(_key, another._key) && Equals(_value, another._value);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(key = \"").Append(_key).Append("\", value = \"").Append(_value).Append("\")");
			return sb.ToString();
		}

		/// <summary>
		/// Decoder to decode key/value bytes.
		/// </summary>
		public delegate KeyValue<TK, TV> KeyValueDecoder(byte[] keyData, byte[] valueData);

		/// <summary>
		/// Encode a <tt>key</tt> and <tt>value</tt> pair into a bytes array.
		/// </summary>
		/// <param name="key"> key object to encode </param>
		/// <param name="keyWriter"> a writer to encode key object </param>
		/// <param name="value"> value object to encode </param>
		/// <param name="valueWriter"> a writer to encode value object </param>
		/// <returns> the encoded bytes array </returns>
		public static EncodeData Encode(string topic, TK key, ISchema<TK> keyWriter, TV value, ISchema<TV> valueWriter)
		{
            EncodeData keyEncodeData;
            if (key == null)
            {
                keyEncodeData = new EncodeData(new byte[0]);
            }
            else
            {
                keyEncodeData = keyWriter.Encode(topic, key);
            }

            EncodeData valueEncodeData;
            if (value == null)
            {
                valueEncodeData = new EncodeData(new byte[0]);
            }
            else
            {
                valueEncodeData = valueWriter.Encode(topic, value);
            }
            
			var result = new byte[4 + keyEncodeData.Data.Length + 4 + valueEncodeData.Data.Length];
            using (var stream = new MemoryStream(result))
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(key == null ? -1 : keyEncodeData.Data.Length.IntToBigEndian());
                    binaryWriter.Write(keyEncodeData.Data);
                    binaryWriter.Write(value == null ? -1 : valueEncodeData.Data.Length.IntToBigEndian());
                    binaryWriter.Write(valueEncodeData.Data);

                    return new EncodeData(stream.ToArray(), GenerateKVSchemaId(keyEncodeData.SchemaId, valueEncodeData.SchemaId));
                }

            }
		}

		/// <summary>
		/// Decode the value into a key/value pair.
		/// </summary>
		/// <param name="data"> the encoded bytes </param>
		/// <param name="decoder"> the decoder to decode encoded key/value bytes </param>
		/// <returns> the decoded key/value pair </returns>
		public static KeyValue<TK, TV> Decode(byte[] data, KeyValueDecoder decoder)
		{
			using var stream = new MemoryStream(data);
			using var binaryReader = new BinaryReader(stream);

			var keyLength = binaryReader.ReadInt32().IntFromBigEndian();
            byte[] keyBytes = keyLength == -1 ? null : new byte[keyLength];
            if (keyBytes != null)
            {
                binaryReader.Read(keyBytes);
            }           

			var valueLength = binaryReader.ReadInt32().IntFromBigEndian();
			byte[] valueBytes = valueLength == -1 ? null : new byte[valueLength];

            if (valueBytes != null)
            {
                binaryReader.Read(valueBytes);
            }
            return decoder(keyBytes, valueBytes);
		}
        /// <summary>
        /// Generate a combined schema id for key/value schema.
        /// The format is:
        /// schemaId = schemaKeyLength + keySchemaIdBytes + schemaValueLength + valueSchemaIdBytes
        /// where schemaKeyLength and schemaValueLength are 4 bytes integer.
        /// If keySchemaIdBytes or valueSchemaIdBytes is null, the length will be 0.
        /// So the total length of schemaId is:
        /// 4 + keySchemaIdBytes.length + 4 + valueSchemaIdBytes.length
        /// </summary>
        /// <param name="keySchemaId"> the schema id of key schema </param>
        /// <param name="valueSchemaId"> the schema id of value schema </param>
        public static byte[] GenerateKVSchemaId(byte[] keySchemaId, byte[] valueSchemaId)
        {
            if (!EncodeData.IsValidSchemaId(keySchemaId) && !EncodeData.IsValidSchemaId(valueSchemaId))
            {
                return null;
            }
            keySchemaId = keySchemaId == null ? new byte[0] : keySchemaId;
            valueSchemaId = valueSchemaId == null ? new byte[0] : valueSchemaId;
            var result = new byte[4 + keySchemaId.Length + 4 + valueSchemaId.Length];
            using var stream = new MemoryStream(result);
            using var binaryWriter = new BinaryWriter(stream);

            binaryWriter.Write(keySchemaId.Length.IntToBigEndian());
            binaryWriter.Write(keySchemaId);
            binaryWriter.Write(valueSchemaId.Length.IntToBigEndian());
            binaryWriter.Write(valueSchemaId);
            return result;
        }

        public static KeyValue<byte[], byte[]> GetSchemaId(byte[] schemaId)
        {
            using var stream = new MemoryStream(schemaId);
            using var binaryReader = new BinaryReader(stream);

            var keySchemaLength = binaryReader.ReadInt32();
            byte[] keySchemaId = [];
            if (keySchemaLength > 0)
            {
                keySchemaId = new byte[keySchemaLength];
                binaryReader.Read(keySchemaId);
            }

            int valueSchemaLength = binaryReader.ReadInt32();
            byte[] valueSchemaId = [];
            if (valueSchemaLength > 0)
            {
                valueSchemaId = new byte[valueSchemaLength];
                binaryReader.Read(valueSchemaId);
            }
            return new KeyValue<byte[], byte[]>(keySchemaId, valueSchemaId);
        }

    }
}
