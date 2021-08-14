using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using System;
using System.IO;
using System.Text;

namespace SharpPulsar.Schemas
{
    public class KeyValue<TK, TV>
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
		public delegate KeyValue<TK, TV> KeyValueDecoder<TK, TV>(byte[] keyData, byte[] valueData);

		/// <summary>
		/// Encode a <tt>key</tt> and <tt>value</tt> pair into a bytes array.
		/// </summary>
		/// <param name="key"> key object to encode </param>
		/// <param name="keyWriter"> a writer to encode key object </param>
		/// <param name="value"> value object to encode </param>
		/// <param name="valueWriter"> a writer to encode value object </param>
		/// <returns> the encoded bytes array </returns>
		public static byte[] Encode(TK key, ISchema<TK> keyWriter, TV value, ISchema<TV> valueWriter)
		{
			var keyBytes = keyWriter.Encode(key);
			var valueBytes = valueWriter.Encode(value);

			var result = new byte[4 + keyBytes.Length + 4 + valueBytes.Length];
			using var stream = new MemoryStream(result);
			using var binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(keyBytes.Length.IntToBigEndian());
			binaryWriter.Write(keyBytes);
			binaryWriter.Write(valueBytes.Length.IntToBigEndian());
			binaryWriter.Write(valueBytes);
			
			return result;
		}

		/// <summary>
		/// Decode the value into a key/value pair.
		/// </summary>
		/// <param name="data"> the encoded bytes </param>
		/// <param name="decoder"> the decoder to decode encoded key/value bytes </param>
		/// <returns> the decoded key/value pair </returns>
		public static KeyValue<TK, TV> Decode(byte[] data, KeyValueDecoder<TK, TV> decoder)
		{
			using var stream = new MemoryStream(data);
			using var binaryReader = new BinaryReader(stream);

			var keyLength = binaryReader.ReadInt32().IntFromBigEndian();
			byte[] keyBytes = binaryReader.ReadBytes(keyLength);

			var valueLength = binaryReader.ReadInt32().IntFromBigEndian();
			byte[] valueBytes = binaryReader.ReadBytes(valueLength); 
			return decoder(keyBytes, valueBytes);
		}
	}
}
