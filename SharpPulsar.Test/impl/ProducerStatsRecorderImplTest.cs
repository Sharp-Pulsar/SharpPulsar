using System.Threading;

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
namespace SharpPulsar.Test.Impl
{
	using HashedWheelTimer = io.netty.util.HashedWheelTimer;
	using Timer = io.netty.util.Timer;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using ProducerConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ProducerConfigurationData;

    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	/// <summary>
	/// Unit tests of <seealso cref="ProducerStatsRecorderImpl"/>.
	/// </summary>
	public class ProducerStatsRecorderImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIncrementNumAcksReceived() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestIncrementNumAcksReceived()
		{
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.StatsIntervalSeconds = 1;
			PulsarClientImpl Client = mock(typeof(PulsarClientImpl));
			when(Client.Configuration).thenReturn(Conf);
			Timer Timer = new HashedWheelTimer();
			when(Client.timer()).thenReturn(Timer);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ProducerImpl<?> producer = mock(ProducerImpl.class);
			ProducerImpl<object> Producer = mock(typeof(ProducerImpl));
			when(Producer.Topic).thenReturn("topic-test");
			when(Producer.ProducerName).thenReturn("producer-test");
			when(Producer.PendingQueueSize).thenReturn(1);
			ProducerConfigurationData ProducerConfigurationData = new ProducerConfigurationData();
			ProducerStatsRecorderImpl Recorder = new ProducerStatsRecorderImpl(Client, ProducerConfigurationData, Producer);
			long LatencyNs = TimeUnit.SECONDS.toNanos(1);
			Recorder.incrementNumAcksReceived(LatencyNs);
			Thread.Sleep(1200);
			assertEquals(1000.0, Recorder.SendLatencyMillisMax, 0.5);
		}
	}

}