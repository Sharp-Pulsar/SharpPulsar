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

namespace Org.Apache.Pulsar.Common.Policies.Data
{
	/// <summary>
	/// Strategy to use when checking an auto-updated schema for compatibility to the current schema.
	/// </summary>
	public enum SchemaAutoUpdateCompatibilityStrategy
	{
		/// <summary>
		/// Don't allow any auto updates.
		/// </summary>
		AutoUpdateDisabled,

		/// <summary>
		/// Messages written in the previous schema can be read by the new schema.
		/// To be backward compatible, the new schema must not add any new fields that
		/// don't have default values. However, it may remove fields.
		/// </summary>
		Backward,

		/// <summary>
		/// Messages written in the new schema can be read by the previous schema.
		/// To be forward compatible, the new schema must not remove any fields which
		/// don't have default values in the previous schema. However, it may add new fields.
		/// </summary>
		Forward,

		/// <summary>
		/// Backward and Forward.
		/// </summary>
		Full,

		/// <summary>
		/// Always Compatible - The new schema will not be checked for compatibility against
		/// old schemas. In other words, new schemas will always be marked assumed compatible.
		/// </summary>
		AlwaysCompatible,

		/// <summary>
		/// Be similar to Backward. BackwardTransitive ensure all previous version schema can
		/// be read by the new schema.
		/// </summary>
		BackwardTransitive,

		/// <summary>
		/// Be similar to Forward, ForwardTransitive ensure new schema can be ready by all previous
		/// version schema.
		/// </summary>
		ForwardTransitive,

		/// <summary>
		/// BackwardTransitive and ForwardTransitive.
		/// </summary>
		FullTransitive
	}

}