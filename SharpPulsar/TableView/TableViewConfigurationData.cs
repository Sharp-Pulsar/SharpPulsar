using System;

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

namespace Org.Apache.Pulsar.Client.Impl
{
	using Data = lombok.Data;
	using NoArgsConstructor = lombok.NoArgsConstructor;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data @NoArgsConstructor public class TableViewConfigurationData implements java.io.Serializable, Cloneable
	[Serializable]
	public class TableViewConfigurationData : ICloneable
	{
		private const long SerialVersionUID = 1L;

		private string topicName = null;
		private long autoUpdatePartitionsSeconds = 60;

		public override TableViewConfigurationData Clone()
		{
			try
			{
				TableViewConfigurationData Clone = (TableViewConfigurationData) base.clone();
				Clone.setTopicName(topicName);
				Clone.setAutoUpdatePartitionsSeconds(autoUpdatePartitionsSeconds);
				return Clone;
			}
			catch (CloneNotSupportedException)
			{
				throw new AssertionError();
			}
		}
	}

}