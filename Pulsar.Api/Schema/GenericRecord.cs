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
namespace org.apache.pulsar.client.api.schema
{

	/// <summary>
	/// An interface represents a message with schema.
	/// </summary>
	public interface GenericRecord
	{

		/// <summary>
		/// Return schema version.
		/// </summary>
		/// <returns> schema version. </returns>
		sbyte[] SchemaVersion {get;}

		/// <summary>
		/// Returns the list of fields associated with the record.
		/// </summary>
		/// <returns> the list of fields associated with the record. </returns>
		IList<Field> Fields {get;}

		/// <summary>
		/// Retrieve the value of the provided <tt>field</tt>.
		/// </summary>
		/// <param name="field"> the field to retrieve the value </param>
		/// <returns> the value object </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default Object getField(Field field)
	//	{
	//		return getField(field.getName());
	//	}

		/// <summary>
		/// Retrieve the value of the provided <tt>fieldName</tt>.
		/// </summary>
		/// <param name="fieldName"> the field name </param>
		/// <returns> the value object </returns>
		object getField(string fieldName);

	}

}