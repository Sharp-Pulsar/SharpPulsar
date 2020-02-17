using SharpPulsar.Api;
using System;
using System.Text;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Common.Schema
{
	/// <summary>
	/// A simple KeyValue class.
	/// </summary>
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
			var another = (KeyValue<TK, TV>) obj;
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
		public delegate KeyValue<TK, TV> KeyValueDecoder<TK, TV>(sbyte[] keyData, sbyte[] valueData);

		/// <summary>
		/// Encode a <tt>key</tt> and <tt>value</tt> pair into a bytes array.
		/// </summary>
		/// <param name="key"> key object to encode </param>
		/// <param name="keyWriter"> a writer to encode key object </param>
		/// <param name="value"> value object to encode </param>
		/// <param name="valueWriter"> a writer to encode value object </param>
		/// <returns> the encoded bytes array </returns>
		public static sbyte[] Encode(TK key, ISchema<TK> keyWriter, TV value, ISchema<TV> valueWriter)
		{
			var keyBytes = keyWriter.Encode(key);
			var valueBytes = valueWriter.Encode(value);
			var byteBuffer = ByteBuffer.Allocate(4 + keyBytes.Length + 4 + valueBytes.Length);
			byteBuffer.PutInt(keyBytes.Length).Put((byte[])(object)keyBytes, 0, keyBytes.Length).PutInt(valueBytes.Length).Put((byte[])(object)valueBytes, 0, valueBytes.Length);
           
            return (sbyte[])(object)byteBuffer.ToArray();
		}

		/// <summary>
		/// Decode the value into a key/value pair.
		/// </summary>
		/// <param name="data"> the encoded bytes </param>
		/// <param name="decoder"> the decoder to decode encoded key/value bytes </param>
		/// <returns> the decoded key/value pair </returns>
		public static KeyValue<TK, TV> Decode(sbyte[] data, KeyValueDecoder<TK, TV> decoder)
		{
			var byteBuffer = ByteBuffer.Allocate(data.Length).Wrap((byte[])(object)data);
			var keyLength = byteBuffer.GetInt();
			var keyBytes = new sbyte[keyLength];
			byteBuffer.Get((byte[])(object)keyBytes, 0, keyLength);

			var valueLength = byteBuffer.GetInt();
			var valueBytes = new sbyte[valueLength];
			byteBuffer.Get((byte[])(object)valueBytes, 0, valueLength);

			return decoder(keyBytes, valueBytes);
		}
	}

}