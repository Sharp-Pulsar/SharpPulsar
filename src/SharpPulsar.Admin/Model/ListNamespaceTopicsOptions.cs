﻿using System.Text.Json.Serialization;
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
    public class ListNamespaceTopicsOptions
    {
        /// <summary>
        /// Set to true to get topics including system topic, otherwise not.
        /// </summary>
        /// 
        [JsonPropertyName("includeSystemTopic")]
        public bool IncludeSystemTopic { get; set; }

        /// <summary>
        /// Allowed topic domain mode (persistent, non_persistent, all).
        /// </summary>
        /// 
        [JsonPropertyName("mode")]
        public Mode Mode { get; set; }
    }

}