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
	public class KeyValue<K, V>
	{
		private readonly K key;
		private readonly V value;

		public KeyValue(K key, V value)
		{
			this.key = key;
			this.value = value;
		}

		public virtual K Key
		{
			get
			{
				return key;
			}
		}

		public virtual V Value
		{
			get
			{
				return value;
			}
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(key, value);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is KeyValue<K, V>))
			{
				return false;
			}
			KeyValue<K, V> another = (KeyValue<K, V>) obj;
			return Equals(key, another.key) && Equals(value, another.value);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(key = \"").Append(key).Append("\", value = \"").Append(value).Append("\")");
			return sb.ToString();
		}

		/// <summary>
		/// Decoder to decode key/value bytes.
		/// </summary>
		public delegate KeyValue<K, V> KeyValueDecoder<K, V>(sbyte[] keyData, sbyte[] valueData);

		/// <summary>
		/// Encode a <tt>key</tt> and <tt>value</tt> pair into a bytes array.
		/// </summary>
		/// <param name="key"> key object to encode </param>
		/// <param name="keyWriter"> a writer to encode key object </param>
		/// <param name="value"> value object to encode </param>
		/// <param name="valueWriter"> a writer to encode value object </param>
		/// <returns> the encoded bytes array </returns>
		public static sbyte[] Encode(K key, ISchema<K> keyWriter, V value, ISchema<V> valueWriter)
		{
			sbyte[] keyBytes = keyWriter.Encode(key);
			sbyte[] valueBytes = valueWriter.Encode(value);
			ByteBuffer byteBuffer = ByteBuffer.Allocate(4 + keyBytes.Length + 4 + valueBytes.Length);
			byteBuffer.PutInt(keyBytes.Length).Put((byte)(object)keyBytes).PutInt(valueBytes.Length).Put((byte)(object)valueBytes);
			return byteBuffer.ToArray();
		}

		/// <summary>
		/// Decode the value into a key/value pair.
		/// </summary>
		/// <param name="data"> the encoded bytes </param>
		/// <param name="decoder"> the decoder to decode encoded key/value bytes </param>
		/// <returns> the decoded key/value pair </returns>
		public static KeyValue<K, V> Decode(sbyte[] data, KeyValueDecoder<K, V> decoder)
		{
			ByteBuffer byteBuffer = ByteBuffer.Wrap(data);
			int keyLength = byteBuffer.Int;
			sbyte[] keyBytes = new sbyte[keyLength];
			byteBuffer.Get(keyBytes);

			int valueLength = byteBuffer.Int;
			sbyte[] valueBytes = new sbyte[valueLength];
			byteBuffer.Get(valueBytes);

			return decoder(keyBytes, valueBytes);
		}
	}

}