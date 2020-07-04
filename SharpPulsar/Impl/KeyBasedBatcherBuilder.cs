﻿using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;
using SharpPulsar.Protocol;

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
namespace SharpPulsar.Impl
{

	public class KeyBasedBatcherBuilder : IBatcherBuilder
    {
        private readonly ActorSystem _system;

        public KeyBasedBatcherBuilder(ActorSystem system)
        {
            _system = system;
        }

        private const long SerialVersionUID = 1L;

		public IBatchMessageContainer Build()
		{
			return new BatchMessageKeyBasedContainer(_system);
		}
	}

}