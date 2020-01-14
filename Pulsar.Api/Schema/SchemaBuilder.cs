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
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;

	/// <summary>
	/// Builder to build schema.
	/// </summary>
	public interface SchemaBuilder
	{

		/// <summary>
		/// Build the schema for a record.
		/// </summary>
		/// <param name="name"> name of the record. </param>
		/// <returns> builder to build the schema for a record. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static RecordSchemaBuilder record(String name)
	//	{
	//		return DefaultImplementation.newRecordSchemaBuilder(name);
	//	}

	}

}