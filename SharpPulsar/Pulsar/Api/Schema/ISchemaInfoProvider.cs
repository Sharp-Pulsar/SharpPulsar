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
namespace SharpPulsar.Pulsar.Api.Schema
{
    /// <summary>
	/// Schema Provider.
	/// </summary>
	public interface ISchemaInfoProvider
	{

		/// <summary>
		/// Retrieve the schema info of a given <tt>schemaVersion</tt>.
		/// </summary>
		/// <param name="schemaVersion"> schema version </param>
		/// <returns> schema info of the provided <tt>schemaVersion</tt> </returns>
		ISchemaInfo GetSchemaByVersion(sbyte[] schemaVersion);

		/// <summary>
		/// Retrieve the latest schema info.
		/// </summary>
		/// <returns> the latest schema </returns>
		ISchemaInfo LatestSchema {get;}

		/// <summary>
		/// Retrieve the topic name.
		/// </summary>
		/// <returns> the topic name </returns>
		string TopicName {get;}

	}

}