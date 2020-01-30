﻿using System.Collections.Generic;

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
namespace SharpPulsar.Common.Naming
{

	/// <summary>
	/// Validator for metadata configuration.
	/// </summary>
	public class Metadata
	{

		private const int MAX_METADATA_SIZE = 1024; // 1 Kb

		private Metadata()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void validateMetadata(java.util.Map<String, String> metadata) throws IllegalArgumentException
		public static void validateMetadata(IDictionary<string, string> metadata)
		{
			if (metadata == null)
			{
				return;
			}

			int size = 0;
			foreach (KeyValuePair<string, string> e in metadata.SetOfKeyValuePairs())
			{
				size += (e.Key.length() + e.Value.length());
				if (size > MAX_METADATA_SIZE)
				{
					throw new System.ArgumentException(ErrorMessage);
				}
			}
		}

		private static string ErrorMessage
		{
			get
			{
				return "metadata has a max size of 1 Kb";
			}
		}
	}

}