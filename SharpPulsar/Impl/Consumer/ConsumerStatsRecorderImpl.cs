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

	using ConsumerStats = org.apache.pulsar.client.api.ConsumerStats;
	using Message = org.apache.pulsar.client.api.Message;
	using SharpPulsar.Impl.conf;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ObjectWriter = com.fasterxml.jackson.databind.ObjectWriter;
	using SerializationFeature = com.fasterxml.jackson.databind.SerializationFeature;

	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;

	public class ConsumerStatsRecorderImpl : ConsumerStatsRecorder
	{

		private const long serialVersionUID = 1L;
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

		private volatile double receivedMsgsRate;
		private volatile double receivedBytesRate;

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

		public ConsumerStatsRecorderImpl<T1, T2>(PulsarClientImpl pulsarClient, ConsumerConfigurationData<T1> conf, ConsumerImpl<T2> consumer)
		{
			this.pulsarClient = pulsarClient;
			this.consumer = consumer;
			this.statsIntervalSeconds = pulsarClient.Configuration.StatsIntervalSeconds;
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
			init(conf);
		}

		private void init<T1>(ConsumerConfigurationData<T1> conf)
		{
			ObjectMapper m = new ObjectMapper();
			m.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			ObjectWriter w = m.writerWithDefaultPrettyPrinter();

			try
			{
				log.info("Starting Pulsar consumer status recorder with config: {}", w.writeValueAsString(conf));
				log.info("Pulsar client config: {}", w.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
			}
			catch (IOException e)
			{
				log.error("Failed to dump config info: {}", e);
			}

			stat = (timeout) =>
			{
			if (timeout.Cancelled)
			{
				return;
			}
			try
			{
				long now = System.nanoTime();
				double elapsed = (now - oldTime) / 1e9;
				oldTime = now;
				long currentNumMsgsReceived = numMsgsReceived.sumThenReset();
				long currentNumBytesReceived = numBytesReceived.sumThenReset();
				long currentNumReceiveFailed = numReceiveFailed.sumThenReset();
				long currentNumBatchReceiveFailed = numBatchReceiveFailed.sumThenReset();
				long currentNumAcksSent = numAcksSent.sumThenReset();
				long currentNumAcksFailed = numAcksFailed.sumThenReset();

				totalMsgsReceived.add(currentNumMsgsReceived);
				totalBytesReceived.add(currentNumBytesReceived);
				totalReceiveFailed.add(currentNumReceiveFailed);
				totalBatchReceiveFailed.add(currentNumBatchReceiveFailed);
				totalAcksSent.add(currentNumAcksSent);
				totalAcksFailed.add(currentNumAcksFailed);

				receivedMsgsRate = currentNumMsgsReceived / elapsed;
				receivedBytesRate = currentNumBytesReceived / elapsed;

				if ((currentNumMsgsReceived | currentNumBytesReceived | currentNumReceiveFailed | currentNumAcksSent | currentNumAcksFailed) != 0)
				{
					log.info("[{}] [{}] [{}] Prefetched messages: {} --- " + "Consume throughput received: {} msgs/s --- {} Mbit/s --- " + "Ack sent rate: {} ack/s --- " + "Failed messages: {} --- batch messages: {} ---" + "Failed acks: {}", consumer.Topic, consumer.Subscription, consumer.consumerName, consumer.incomingMessages.size(), THROUGHPUT_FORMAT.format(receivedMsgsRate), THROUGHPUT_FORMAT.format(receivedBytesRate * 8 / 1024 / 1024), THROUGHPUT_FORMAT.format(currentNumAcksSent / elapsed), currentNumReceiveFailed, currentNumBatchReceiveFailed, currentNumAcksFailed);
				}
			}
			catch (Exception e)
			{
				log.error("[{}] [{}] [{}]: {}", consumer.Topic, consumer.subscription, consumer.consumerName, e.Message);
			}
			finally
			{
				// schedule the next stat info
				statTimeout = pulsarClient.timer().newTimeout(stat, statsIntervalSeconds, TimeUnit.SECONDS);
			}
			};

			oldTime = System.nanoTime();
			statTimeout = pulsarClient.timer().newTimeout(stat, statsIntervalSeconds, TimeUnit.SECONDS);
		}

		public virtual void updateNumMsgsReceived<T1>(Message<T1> message)
		{
			if (message != null)
			{
				numMsgsReceived.increment();
				numBytesReceived.add(message.Data.length);
			}
		}

		public virtual void incrementNumAcksSent(long numAcks)
		{
			numAcksSent.add(numAcks);
		}

		public virtual void incrementNumAcksFailed()
		{
			numAcksFailed.increment();
		}

		public virtual void incrementNumReceiveFailed()
		{
			numReceiveFailed.increment();
		}

		public virtual void incrementNumBatchReceiveFailed()
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

		public virtual void reset()
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

		public virtual void updateCumulativeStats(ConsumerStats stats)
		{
			if (stats == null)
			{
				return;
			}
			numMsgsReceived.add(stats.NumMsgsReceived);
			numBytesReceived.add(stats.NumBytesReceived);
			numReceiveFailed.add(stats.NumReceiveFailed);
			numBatchReceiveFailed.add(stats.NumBatchReceiveFailed);
			numAcksSent.add(stats.NumAcksSent);
			numAcksFailed.add(stats.NumAcksFailed);
			totalMsgsReceived.add(stats.TotalMsgsReceived);
			totalBytesReceived.add(stats.TotalBytesReceived);
			totalReceiveFailed.add(stats.TotalReceivedFailed);
			totalBatchReceiveFailed.add(stats.TotaBatchReceivedFailed);
			totalAcksSent.add(stats.TotalAcksSent);
			totalAcksFailed.add(stats.TotalAcksFailed);
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

		public override long NumBatchReceiveFailed
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

		public override long TotaBatchReceivedFailed
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

		public override double RateMsgsReceived
		{
			get
			{
				return receivedMsgsRate;
			}
		}

		public override double RateBytesReceived
		{
			get
			{
				return receivedBytesRate;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConsumerStatsRecorderImpl));
	}

}