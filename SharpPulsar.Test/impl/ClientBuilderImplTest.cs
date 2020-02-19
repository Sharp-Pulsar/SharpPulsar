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

using System;
using SharpPulsar.Api;
using Xunit;

namespace SharpPulsar.Test.Impl
{

    public class ClientBuilderImplTest
	{
		[Fact]
		public  void TestClientBuilderWithServiceUrlAndServiceUrlProviderNotSet()
		{
            var exception = Assert.Throws<ArgumentException>(()=> IPulsarClient.Builder().Build());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal("service URL or service URL provider needs to be specified on the ClientBuilder object.", exception.Message);
			
		}
		[Fact]
		public void TestClientBuilderWithNullServiceUrl()
		{
            var exception = Assert.Throws<ArgumentException>(() => IPulsarClient.Builder().ServiceUrl(null).Build());
			//IPulsarClient.Builder().ServiceUrl(null).Build();
			Assert.Equal("Param serviceUrl must not be blank.", exception.Message);
		}
		[Fact]
		public void TestClientBuilderWithNullServiceUrlProvider()
		{
            var exception = Assert.Throws<ArgumentException>(() => IPulsarClient.Builder().ServiceUrlProvider(null).Build());
			Assert.Equal("Param serviceUrlProvider must not be null.", exception.Message);
        }

	}

}