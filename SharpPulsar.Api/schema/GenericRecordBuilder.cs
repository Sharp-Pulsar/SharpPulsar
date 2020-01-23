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
	/// <summary>
	/// Generic Record Builder to build a <seealso cref="GenericRecord"/>.
	/// </summary>
	public interface GenericRecordBuilder
	{

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		GenericRecordBuilder Set(string FieldName, object Value);

		/// <summary>
		/// Sets the value of a field.
		/// </summary>
		/// <param name="field"> the field to set. </param>
		/// <param name="value"> the value to set. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		GenericRecordBuilder Set(Field Field, object Value);

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="fieldName"> the name of the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		GenericRecordBuilder Clear(string FieldName);

		/// <summary>
		/// Clears the value of the given field.
		/// </summary>
		/// <param name="field"> the field to clear. </param>
		/// <returns> a reference to the RecordBuilder. </returns>
		GenericRecordBuilder Clear(Field Field);

		/// <summary>
		/// Build a generic record.
		/// </summary>
		/// <returns> a generic record. </returns>
		GenericRecord Build();

	}

}