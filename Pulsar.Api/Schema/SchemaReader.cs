using System.IO;

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
namespace org.apache.pulsar.client.api.schema
{

	/// <summary>
	/// Deserialize messages from bytes.
	/// </summary>

	public interface SchemaReader<T>
	{

		/// <summary>
		/// Serialize bytes convert pojo.
		/// </summary>
		/// <param name="bytes"> the data </param>
		/// <returns> the serialized object </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default T read(byte[] bytes)
	//	{
	//		return read(bytes, 0, bytes.length);
	//	}

		/// <summary>
		/// serialize bytes convert pojo.
		/// </summary>
		/// <param name="bytes"> the data </param>
		/// <param name="offset"> the byte[] initial position </param>
		/// <param name="length"> the byte[] read length </param>
		/// <returns> the serialized object </returns>
		T read(sbyte[] bytes, int offset, int length);

		/// <summary>
		/// serialize bytes convert pojo.
		/// </summary>
		/// <param name="inputStream"> the stream of message </param>
		/// <returns> the serialized object </returns>
		T read(Stream inputStream);
	}

}