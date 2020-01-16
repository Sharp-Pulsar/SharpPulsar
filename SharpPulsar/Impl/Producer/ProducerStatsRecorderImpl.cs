using SharpPulsar.Interface.Producer;
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
namespace org.apache.pulsar.client.impl
{

	public class ProducerStatsRecorderImpl : IProducerStatsRecorder
	{

		private const long serialVersionUID = 1L;
		private TimerTask stat;
		private Timeout statTimeout;
		private ProducerImpl<object> producer;
		private PulsarClientImpl pulsarClient;
		private long oldTime;
		private long statsIntervalSeconds;
		private readonly LongAdder numMsgsSent;
		private readonly LongAdder numBytesSent;
		private readonly LongAdder numSendFailed;
		private readonly LongAdder numAcksReceived;
		private readonly LongAdder totalMsgsSent;
		private readonly LongAdder totalBytesSent;
		private readonly LongAdder totalSendFailed;
		private readonly LongAdder totalAcksReceived;
		private static readonly DecimalFormat DEC = new DecimalFormat("0.000");
		private static readonly DecimalFormat THROUGHPUT_FORMAT = new DecimalFormat("0.00");
		private readonly DoublesSketch ds;

		private volatile double sendMsgsRate;
		private volatile double sendBytesRate;
		private volatile double[] latencyPctValues;

		private static readonly double[] PERCENTILES = new double[] {0.5, 0.75, 0.95, 0.99, 0.999, 1.0};

		public ProducerStatsRecorderImpl()
		{
			numMsgsSent = new LongAdder();
			numBytesSent = new LongAdder();
			numSendFailed = new LongAdder();
			numAcksReceived = new LongAdder();
			totalMsgsSent = new LongAdder();
			totalBytesSent = new LongAdder();
			totalSendFailed = new LongAdder();
			totalAcksReceived = new LongAdder();
			ds = DoublesSketch.builder().build(256);
		}

		public ProducerStatsRecorderImpl<T1>(PulsarClientImpl pulsarClient, ProducerConfigurationData conf, ProducerImpl<T1> producer)
		{
			this.pulsarClient = pulsarClient;
			this.statsIntervalSeconds = pulsarClient.Configuration.StatsIntervalSeconds;
			this.producer = producer;
			numMsgsSent = new LongAdder();
			numBytesSent = new LongAdder();
			numSendFailed = new LongAdder();
			numAcksReceived = new LongAdder();
			totalMsgsSent = new LongAdder();
			totalBytesSent = new LongAdder();
			totalSendFailed = new LongAdder();
			totalAcksReceived = new LongAdder();
			ds = DoublesSketch.builder().build(256);
			init(conf);
		}

		private void init(ProducerConfigurationData conf)
		{
			ObjectMapper m = new ObjectMapper();
			m.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			ObjectWriter w = m.writerWithDefaultPrettyPrinter();

			try
			{
				log.info("Starting Pulsar producer perf with config: {}", w.writeValueAsString(conf));
				log.info("Pulsar client config: {}", w.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
			}
			catch (IOException e)
			{
				log.error("Failed to dump config info", e);
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

				long currentNumMsgsSent = numMsgsSent.sumThenReset();
				long currentNumBytesSent = numBytesSent.sumThenReset();
				long currentNumSendFailedMsgs = numSendFailed.sumThenReset();
				long currentNumAcksReceived = numAcksReceived.sumThenReset();

				totalMsgsSent.add(currentNumMsgsSent);
				totalBytesSent.add(currentNumBytesSent);
				totalSendFailed.add(currentNumSendFailedMsgs);
				totalAcksReceived.add(currentNumAcksReceived);

				lock (ds)
				{
					latencyPctValues = ds.getQuantiles(PERCENTILES);
					ds.reset();
				}

				sendMsgsRate = currentNumMsgsSent / elapsed;
				sendBytesRate = currentNumBytesSent / elapsed;

				if ((currentNumMsgsSent | currentNumSendFailedMsgs | currentNumAcksReceived | currentNumMsgsSent) != 0)
				{

					for (int i = 0; i < latencyPctValues.Length; i++)
					{
						if (double.IsNaN(latencyPctValues[i]))
						{
							latencyPctValues[i] = 0;
						}
					}

					log.info("[{}] [{}] Pending messages: {} --- Publish throughput: {} msg/s --- {} Mbit/s --- " + "Latency: med: {} ms - 95pct: {} ms - 99pct: {} ms - 99.9pct: {} ms - max: {} ms --- " + "Ack received rate: {} ack/s --- Failed messages: {}", producer.Topic, producer.ProducerName, producer.PendingQueueSize, THROUGHPUT_FORMAT.format(sendMsgsRate), THROUGHPUT_FORMAT.format(sendBytesRate / 1024 / 1024 * 8), DEC.format(latencyPctValues[0] / 1000.0), DEC.format(latencyPctValues[2] / 1000.0), DEC.format(latencyPctValues[3] / 1000.0), DEC.format(latencyPctValues[4] / 1000.0), DEC.format(latencyPctValues[5] / 1000.0), THROUGHPUT_FORMAT.format(currentNumAcksReceived / elapsed), currentNumSendFailedMsgs);
				}

			}
			catch (Exception e)
			{
				log.error("[{}] [{}]: {}", producer.Topic, producer.ProducerName, e.Message);
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

		internal virtual Timeout StatTimeout
		{
			get
			{
				return statTimeout;
			}
		}

		public virtual void updateNumMsgsSent(long numMsgs, long totalMsgsSize)
		{
			numMsgsSent.add(numMsgs);
			numBytesSent.add(totalMsgsSize);
		}

		public virtual void incrementSendFailed()
		{
			numSendFailed.increment();
		}

		public virtual void incrementSendFailed(long numMsgs)
		{
			numSendFailed.add(numMsgs);
		}

		public virtual void incrementNumAcksReceived(long latencyNs)
		{
			numAcksReceived.increment();
			lock (ds)
			{
				ds.update(TimeUnit.NANOSECONDS.toMillis(latencyNs));
			}
		}

		internal virtual void reset()
		{
			numMsgsSent.reset();
			numBytesSent.reset();
			numSendFailed.reset();
			numAcksReceived.reset();
			totalMsgsSent.reset();
			totalBytesSent.reset();
			totalSendFailed.reset();
			totalAcksReceived.reset();
		}

		internal virtual void updateCumulativeStats(ProducerStats stats)
		{
			if (stats == null)
			{
				return;
			}
			numMsgsSent.add(stats.NumMsgsSent);
			numBytesSent.add(stats.NumBytesSent);
			numSendFailed.add(stats.NumSendFailed);
			numAcksReceived.add(stats.NumAcksReceived);
			totalMsgsSent.add(stats.NumMsgsSent);
			totalBytesSent.add(stats.NumBytesSent);
			totalSendFailed.add(stats.NumSendFailed);
			totalAcksReceived.add(stats.NumAcksReceived);
		}

		public override long NumMsgsSent
		{
			get
			{
				return numMsgsSent.longValue();
			}
		}

		public override long NumBytesSent
		{
			get
			{
				return numBytesSent.longValue();
			}
		}

		public override long NumSendFailed
		{
			get
			{
				return numSendFailed.longValue();
			}
		}

		public override long NumAcksReceived
		{
			get
			{
				return numAcksReceived.longValue();
			}
		}

		public virtual long TotalMsgsSent
		{
			get
			{
				return totalMsgsSent.longValue();
			}
		}

		public virtual long TotalBytesSent
		{
			get
			{
				return totalBytesSent.longValue();
			}
		}

		public virtual long TotalSendFailed
		{
			get
			{
				return totalSendFailed.longValue();
			}
		}

		public virtual long TotalAcksReceived
		{
			get
			{
				return totalAcksReceived.longValue();
			}
		}

		public override double SendMsgsRate
		{
			get
			{
				return sendMsgsRate;
			}
		}

		public override double SendBytesRate
		{
			get
			{
				return sendBytesRate;
			}
		}

		public override double SendLatencyMillis50pct
		{
			get
			{
				return latencyPctValues[0];
			}
		}

		public override double SendLatencyMillis75pct
		{
			get
			{
				return latencyPctValues[1];
			}
		}

		public override double SendLatencyMillis95pct
		{
			get
			{
				return latencyPctValues[2];
			}
		}

		public override double SendLatencyMillis99pct
		{
			get
			{
				return latencyPctValues[3];
			}
		}

		public override double SendLatencyMillis999pct
		{
			get
			{
				return latencyPctValues[4];
			}
		}

		public override double SendLatencyMillisMax
		{
			get
			{
				return latencyPctValues[5];
			}
		}

		public virtual void cancelStatsTimeout()
		{
			if (statTimeout != null)
			{
				statTimeout.cancel();
				statTimeout = null;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProducerStatsRecorderImpl));
	}

}