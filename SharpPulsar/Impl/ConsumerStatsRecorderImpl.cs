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
namespace SharpPulsar.Impl
{

	using ConsumerStats = SharpPulsar.Api.ConsumerStats;
	using SharpPulsar.Api;
	using SharpPulsar.Impl.Conf;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ObjectWriter = com.fasterxml.jackson.databind.ObjectWriter;
	using SerializationFeature = com.fasterxml.jackson.databind.SerializationFeature;

	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;

	[Serializable]
	public class ConsumerStatsRecorderImpl : ConsumerStatsRecorder
	{

		private const long SerialVersionUID = 1L;
		private TimerTask stat;
		private Timeout statTimeout;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private ConsumerImpl<?> consumer;
		private ConsumerImpl<object> consumer;
		private PulsarClientImpl pulsarClient;
		private long oldTime;
		private long statsIntervalSeconds;
		private readonly LongAdder numMsgsReceived;
		private readonly LongAdder numBytesReceived;
		private readonly LongAdder numReceiveFailed;
		private readonly LongAdder numBatchReceiveFailed;
		private readonly LongAdder numAcksSent;
		private readonly LongAdder numAcksFailed;
		private readonly LongAdder totalMsgsReceived;
		private readonly LongAdder totalBytesReceived;
		private readonly LongAdder totalReceiveFailed;
		private readonly LongAdder totalBatchReceiveFailed;
		private readonly LongAdder totalAcksSent;
		private readonly LongAdder totalAcksFailed;

		public virtual RateMsgsReceived {get;}
		public virtual RateBytesReceived {get;}

		private static readonly DecimalFormat THROUGHPUT_FORMAT = new DecimalFormat("0.00");

		public ConsumerStatsRecorderImpl()
		{
			numMsgsReceived = new LongAdder();
			numBytesReceived = new LongAdder();
			numReceiveFailed = new LongAdder();
			numBatchReceiveFailed = new LongAdder();
			numAcksSent = new LongAdder();
			numAcksFailed = new LongAdder();
			totalMsgsReceived = new LongAdder();
			totalBytesReceived = new LongAdder();
			totalReceiveFailed = new LongAdder();
			totalBatchReceiveFailed = new LongAdder();
			totalAcksSent = new LongAdder();
			totalAcksFailed = new LongAdder();
		}

		public ConsumerStatsRecorderImpl<T1, T2>(PulsarClientImpl PulsarClient, ConsumerConfigurationData<T1> Conf, ConsumerImpl<T2> Consumer)
		{
			this.pulsarClient = PulsarClient;
			this.consumer = Consumer;
			this.statsIntervalSeconds = PulsarClient.Configuration.StatsIntervalSeconds;
			numMsgsReceived = new LongAdder();
			numBytesReceived = new LongAdder();
			numReceiveFailed = new LongAdder();
			numBatchReceiveFailed = new LongAdder();
			numAcksSent = new LongAdder();
			numAcksFailed = new LongAdder();
			totalMsgsReceived = new LongAdder();
			totalBytesReceived = new LongAdder();
			totalReceiveFailed = new LongAdder();
			totalBatchReceiveFailed = new LongAdder();
			totalAcksSent = new LongAdder();
			totalAcksFailed = new LongAdder();
			Init(Conf);
		}

		private void Init<T1>(ConsumerConfigurationData<T1> Conf)
		{
			ObjectMapper M = new ObjectMapper();
			M.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			ObjectWriter W = M.writerWithDefaultPrettyPrinter();

			try
			{
				log.info("Starting Pulsar consumer status recorder with config: {}", W.writeValueAsString(Conf));
				log.info("Pulsar client config: {}", W.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
			}
			catch (IOException E)
			{
				log.error("Failed to dump config info: {}", E);
			}

			stat = (timeout) =>
			{
			if (timeout.Cancelled)
			{
				return;
			}
			try
			{
				long Now = System.nanoTime();
				double Elapsed = (Now - oldTime) / 1e9;
				oldTime = Now;
				long CurrentNumMsgsReceived = numMsgsReceived.sumThenReset();
				long CurrentNumBytesReceived = numBytesReceived.sumThenReset();
				long CurrentNumReceiveFailed = numReceiveFailed.sumThenReset();
				long CurrentNumBatchReceiveFailed = numBatchReceiveFailed.sumThenReset();
				long CurrentNumAcksSent = numAcksSent.sumThenReset();
				long CurrentNumAcksFailed = numAcksFailed.sumThenReset();

				totalMsgsReceived.add(CurrentNumMsgsReceived);
				totalBytesReceived.add(CurrentNumBytesReceived);
				totalReceiveFailed.add(CurrentNumReceiveFailed);
				totalBatchReceiveFailed.add(CurrentNumBatchReceiveFailed);
				totalAcksSent.add(CurrentNumAcksSent);
				totalAcksFailed.add(CurrentNumAcksFailed);

				RateMsgsReceived = CurrentNumMsgsReceived / Elapsed;
				RateBytesReceived = CurrentNumBytesReceived / Elapsed;

				if ((CurrentNumMsgsReceived | CurrentNumBytesReceived | CurrentNumReceiveFailed | CurrentNumAcksSent | CurrentNumAcksFailed) != 0)
				{
					log.info("[{}] [{}] [{}] Prefetched messages: {} --- " + "Consume throughput received: {} msgs/s --- {} Mbit/s --- " + "Ack sent rate: {} ack/s --- " + "Failed messages: {} --- batch messages: {} ---" + "Failed acks: {}", consumer.Topic, consumer.Subscription, consumer.ConsumerNameConflict, consumer.IncomingMessages.size(), THROUGHPUT_FORMAT.format(RateMsgsReceived), THROUGHPUT_FORMAT.format(RateBytesReceived * 8 / 1024 / 1024), THROUGHPUT_FORMAT.format(CurrentNumAcksSent / Elapsed), CurrentNumReceiveFailed, CurrentNumBatchReceiveFailed, CurrentNumAcksFailed);
				}
			}
			catch (Exception E)
			{
				log.error("[{}] [{}] [{}]: {}", consumer.Topic, consumer.SubscriptionConflict, consumer.ConsumerNameConflict, E.Message);
			}
			finally
			{
				// schedule the next stat info
				statTimeout = pulsarClient.Timer().newTimeout(stat, statsIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
			}
			};

			oldTime = System.nanoTime();
			statTimeout = pulsarClient.Timer().newTimeout(stat, statsIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
		}

		public override void UpdateNumMsgsReceived<T1>(Message<T1> Message)
		{
			if (Message != null)
			{
				numMsgsReceived.increment();
				numBytesReceived.add(Message.Data.Length);
			}
		}

		public override void IncrementNumAcksSent(long NumAcks)
		{
			numAcksSent.add(NumAcks);
		}

		public override void IncrementNumAcksFailed()
		{
			numAcksFailed.increment();
		}

		public override void IncrementNumReceiveFailed()
		{
			numReceiveFailed.increment();
		}

		public override void IncrementNumBatchReceiveFailed()
		{
			numBatchReceiveFailed.increment();
		}

		public virtual Optional<Timeout> StatTimeout
		{
			get
			{
				return Optional.ofNullable(statTimeout);
			}
		}

		public override void Reset()
		{
			numMsgsReceived.reset();
			numBytesReceived.reset();
			numReceiveFailed.reset();
			numBatchReceiveFailed.reset();
			numAcksSent.reset();
			numAcksFailed.reset();
			totalMsgsReceived.reset();
			totalBytesReceived.reset();
			totalReceiveFailed.reset();
			totalBatchReceiveFailed.reset();
			totalAcksSent.reset();
			totalAcksFailed.reset();
		}

		public override void UpdateCumulativeStats(ConsumerStats Stats)
		{
			if (Stats == null)
			{
				return;
			}
			numMsgsReceived.add(Stats.NumMsgsReceived);
			numBytesReceived.add(Stats.NumBytesReceived);
			numReceiveFailed.add(Stats.NumReceiveFailed);
			numBatchReceiveFailed.add(Stats.NumBatchReceiveFailed);
			numAcksSent.add(Stats.NumAcksSent);
			numAcksFailed.add(Stats.NumAcksFailed);
			totalMsgsReceived.add(Stats.TotalMsgsReceived);
			totalBytesReceived.add(Stats.TotalBytesReceived);
			totalReceiveFailed.add(Stats.TotalReceivedFailed);
			totalBatchReceiveFailed.add(Stats.TotaBatchReceivedFailed);
			totalAcksSent.add(Stats.TotalAcksSent);
			totalAcksFailed.add(Stats.TotalAcksFailed);
		}

		public virtual long NumMsgsReceived
		{
			get
			{
				return numMsgsReceived.longValue();
			}
		}

		public virtual long NumBytesReceived
		{
			get
			{
				return numBytesReceived.longValue();
			}
		}

		public virtual long NumAcksSent
		{
			get
			{
				return numAcksSent.longValue();
			}
		}

		public virtual long NumAcksFailed
		{
			get
			{
				return numAcksFailed.longValue();
			}
		}

		public virtual long NumReceiveFailed
		{
			get
			{
				return numReceiveFailed.longValue();
			}
		}

		public virtual long NumBatchReceiveFailed
		{
			get
			{
				return numBatchReceiveFailed.longValue();
			}
		}

		public virtual long TotalMsgsReceived
		{
			get
			{
				return totalMsgsReceived.longValue();
			}
		}

		public virtual long TotalBytesReceived
		{
			get
			{
				return totalBytesReceived.longValue();
			}
		}

		public virtual long TotalReceivedFailed
		{
			get
			{
				return totalReceiveFailed.longValue();
			}
		}

		public virtual long TotaBatchReceivedFailed
		{
			get
			{
				return totalBatchReceiveFailed.longValue();
			}
		}

		public virtual long TotalAcksSent
		{
			get
			{
				return totalAcksSent.longValue();
			}
		}

		public virtual long TotalAcksFailed
		{
			get
			{
				return totalAcksFailed.longValue();
			}
		}



		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConsumerStatsRecorderImpl));
	}

}