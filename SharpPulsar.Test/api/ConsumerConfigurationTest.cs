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

using Microsoft.Extensions.Logging;
using Moq;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using Xunit;

namespace SharpPulsar.Test.Api
{

/// <summary>
	/// Unit test of <seealso cref="ConsumerConfiguration"/>.
	/// </summary>
	public class ConsumerConfigurationTest
	{

		private static readonly ILogger Log = new LoggerFactory().CreateLogger((typeof(ConsumerConfigurationTest));

		[Fact]
		public virtual void TestJsonIgnore()
		{

            var conf = new ConsumerConfigurationData<object>
            {
                ConsumerEventListener = new ConsumerEventListenerAnonymousInnerClass(this),
                MessageListener = (MessageListener) (consumer, msg)
            };

            {
			};

			conf.CryptoKeyReader = new Mock<ICryptoKeyReader>().Object;

			var m = new ObjectMapper();
			//m.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			//ObjectWriter w = m.writerWithDefaultPrettyPrinter();

			var confAsString = m.WriteValueAsString(conf);
			Log.LogInformation("conf : {}", confAsString);

			Assert.DoesNotContain("messageListener", confAsString);
            Assert.DoesNotContain("consumerEventListener", confAsString);
            Assert.DoesNotContain("cryptoKeyReader", confAsString);
		}

		public class ConsumerEventListenerAnonymousInnerClass : IConsumerEventListener
		{
			private readonly ConsumerConfigurationTest _outerInstance;

			public ConsumerEventListenerAnonymousInnerClass(ConsumerConfigurationTest outerInstance)
			{
				this._outerInstance = outerInstance;
			}


			public void BecameActive<T1>(IConsumer<T1> consumer, int partitionId)
			{
			}

			public void BecameInactive<T1>(IConsumer<T1> consumer, int partitionId)
			{
			}
		}

	}

}