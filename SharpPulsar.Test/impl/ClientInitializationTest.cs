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
/// 

using FakeItEasy;
using SharpPulsar.Api;
using Xunit;

namespace SharpPulsar.Test.Impl
{

    public class ClientInitializationTest
	{
		[Fact]
		public  void TestInitializeAuthWithTls()
        {
			var auth = A.Fake <IAuthentication> ();

			IPulsarClient.Builder().ServiceUrl("pulsar+ssl://localhost:6650").Authentication(auth).Build();

			// Auth should only be started, though we shouldn't have tried to get credentials yet (until we first attempt to
			// connect).
			A.CallTo(() => auth.Start()).MustHaveHappened(1, Times.Exactly);
            
		}
	}

}