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

using System.Linq;
using System.Text.RegularExpressions;

namespace SharpPulsar.Sql.Naming
{
	public class NamedEntity
	{

		// allowed characters for property, namespace, cluster and topic names are
		// alphanumeric (a-zA-Z_0-9) and these special chars -=:.
		// % is allowed as part of valid URL encoding
		public static readonly Regex NamedEntityPattern = new Regex("^[-=:.\\w]*$");

		public static void CheckName(string name)
		{
			var m = NamedEntityPattern.Matches(name);
			if (!m.Any())
			{
				throw new System.ArgumentException("Invalid named entity: " + name);
			}
		}
	}
}