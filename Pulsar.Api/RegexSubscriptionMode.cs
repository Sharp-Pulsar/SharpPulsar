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
namespace Pulsar.Api
{
	/// <summary>
	/// When subscribing to topics using a regular expression, one can specify
	/// to only pick a certain type of topics.
	/// </summary>
	public enum RegexSubscriptionMode
	{
		/// <summary>
		/// Only subscribe to persistent topics.
		/// </summary>
		PersistentOnly,

		/// <summary>
		/// Only subscribe to non-persistent topics.
		/// </summary>
		NonPersistentOnly,

		/// <summary>
		/// Subscribe to both persistent and non-persistent topics.
		/// </summary>
		AllTopics
	}

}