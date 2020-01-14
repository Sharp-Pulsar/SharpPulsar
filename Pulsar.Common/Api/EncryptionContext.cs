using System.Collections.Generic;

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
namespace org.apache.pulsar.common.api
{

	using AllArgsConstructor = lombok.AllArgsConstructor;
	using Getter = lombok.Getter;
	using NoArgsConstructor = lombok.NoArgsConstructor;
	using Setter = lombok.Setter;

	using CompressionType = org.apache.pulsar.client.api.CompressionType;

	/// <summary>
	/// Class representing an encryption context.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter @Setter public class EncryptionContext
	public class EncryptionContext
	{

		private IDictionary<string, EncryptionKey> keys;
		private sbyte[] param;
		private string algorithm;
		private CompressionType compressionType;
		private int uncompressedMessageSize;
		private int? batchSize;

		/// <summary>
		/// Encryption key with metadata.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter @Setter @NoArgsConstructor @AllArgsConstructor public static class EncryptionKey
		public class EncryptionKey
		{
			internal sbyte[] keyValue;
			internal IDictionary<string, string> metadata;
		}

	}
}