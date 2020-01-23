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
namespace SharpPulsar.Api
{

	/// <summary>
	/// EncryptionKeyInfo contains the encryption key and corresponding metadata which contains additional information about
	/// the key such as version, timestamp.
	/// </summary>
	public class EncryptionKeyInfo
	{

		private IDictionary<string, string> metadata = null;
		private sbyte[] key = null;

		public EncryptionKeyInfo()
		{
			this.key = null;
			this.metadata = null;
		}

		public EncryptionKeyInfo(sbyte[] Key, IDictionary<string, string> Metadata)
		{
			this.key = Key;
			this.metadata = Metadata;
		}

		public virtual sbyte[] Key
		{
			get
			{
				return key;
			}
			set
			{
				this.key = value;
			}
		}


		public virtual IDictionary<string, string> Metadata
		{
			get
			{
				return metadata;
			}
			set
			{
				this.metadata = value;
			}
		}

	}

}