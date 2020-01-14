﻿/// <summary>
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

	using Data = lombok.Data;

	/// <summary>
	/// Authentication data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data(staticConstructor = "of") public final class AuthData
	public sealed class AuthData
	{
		// CHECKSTYLE.OFF: StaticVariableName
		public static sbyte[] INIT_AUTH_DATA = "PulsarAuthInit".getBytes(UTF_8);
		// CHECKSTYLE.ON: StaticVariableName

		private readonly sbyte[] bytes;

		public bool Complete
		{
			get
			{
				return bytes == null;
			}
		}
	}

}