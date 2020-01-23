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
namespace SharpPulsar.Api.Schema
{
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// Building the schema for a <seealso cref="GenericRecord"/>.
	/// </summary>
	public interface RecordSchemaBuilder
	{

		/// <summary>
		/// Attach val-name property pair to the record schema.
		/// </summary>
		/// <param name="name"> property name </param>
		/// <param name="val"> property value </param>
		/// <returns> record schema builder </returns>
		RecordSchemaBuilder Property(string Name, string Val);

		/// <summary>
		/// Add a field with the given name to the record.
		/// </summary>
		/// <param name="fieldName"> name of the field </param>
		/// <returns> field schema builder to build the field. </returns>
		IFieldSchemaBuilder Field(string FieldName);

		/// <summary>
		/// Add a field with the given name and genericSchema to the record.
		/// </summary>
		/// <param name="fieldName"> name of the field </param>
		/// <param name="genericSchema"> schema of the field </param>
		/// <returns> field schema builder to build the field. </returns>
		IFieldSchemaBuilder Field(string FieldName, GenericSchema GenericSchema);

		/// <summary>
		/// Add doc to the record schema.
		/// </summary>
		/// <param name="doc"> documentation </param>
		/// <returns> field schema builder </returns>
		RecordSchemaBuilder Doc(string Doc);

		/// <summary>
		/// Build the schema info.
		/// </summary>
		/// <returns> the schema info. </returns>
		SchemaInfo Build(SchemaType SchemaType);

	}

}