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
namespace SharpPulsar.Impl.Auth
{
	/// <summary>
	/// Class representing an encryption context.
	/// </summary>
	public class EncryptionContext
	{

		public IDictionary<string, EncryptionKey> Keys { get; set; }
		public sbyte[] Param { get; set; }
		public string Algorithm;
		public int CompressionType { get; set; }//hack
		public int UncompressedMessageSize { get; set; }
		public int? BatchSize { get; set; }

		/// <summary>
		/// Encryption key with metadata.
		/// </summary>
		public class EncryptionKey
		{
			public sbyte[] KeyValue { get; set; }
			public IDictionary<string, string> Metadata { get; set; }
		}

	}
}