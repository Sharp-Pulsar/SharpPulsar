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
namespace SharpPulsar.Api.Schema
{
	using SharpPulsar.Api;

	/// <summary>
	/// A schema that serializes and deserializes between <seealso cref="IGenericRecord"/> and bytes.
	/// </summary>
	public interface IGenericSchema<T> : ISchema<T> where T : IGenericRecord
	{

		/// <summary>
		/// Returns the list of fields.
		/// </summary>
		/// <returns> the list of fields of generic record. </returns>
		IList<Field> Fields {get;}

		/// <summary>
		/// Create a builder to build <seealso cref="IGenericRecord"/>.
		/// </summary>
		/// <returns> generic record builder </returns>
		IGenericRecordBuilder NewRecordBuilder();

	}

}