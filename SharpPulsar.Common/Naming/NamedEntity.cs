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
namespace SharpPulsar.Common.Naming
{

	using UtilityClass = lombok.experimental.UtilityClass;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UtilityClass public class NamedEntity
	public class NamedEntity
	{

		// allowed characters for property, namespace, cluster and topic names are
		// alphanumeric (a-zA-Z_0-9) and these special chars -=:.
		// % is allowed as part of valid URL encoding
		public static readonly Pattern NAMED_ENTITY_PATTERN = Pattern.compile("^[-=:.\\w]*$");

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void checkName(String name) throws IllegalArgumentException
		public static void checkName(string name)
		{
			Matcher m = NAMED_ENTITY_PATTERN.matcher(name);
			if (!m.matches())
			{
				throw new System.ArgumentException("Invalid named entity: " + name);
			}
		}
	}
}