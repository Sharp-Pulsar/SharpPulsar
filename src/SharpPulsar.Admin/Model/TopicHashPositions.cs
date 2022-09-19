﻿using System.Collections.Generic;

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
namespace SharpPulsar.Admin.Model
{
	using AllArgsConstructor = lombok.AllArgsConstructor;
	using Getter = lombok.Getter;
	using NoArgsConstructor = lombok.NoArgsConstructor;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Getter @NoArgsConstructor @AllArgsConstructor public class TopicHashPositions
	public class TopicHashPositions
	{
		private string @namespace;
		private string bundle;
		private IDictionary<string, long> topicHashPositions;
	}

}