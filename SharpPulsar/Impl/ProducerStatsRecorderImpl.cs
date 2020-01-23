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

	using ProducerStats = SharpPulsar.Api.ProducerStats;
	using ProducerConfigurationData = SharpPulsar.Impl.Conf.ProducerConfigurationData;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ObjectWriter = com.fasterxml.jackson.databind.ObjectWriter;
	using SerializationFeature = com.fasterxml.jackson.databind.SerializationFeature;
	using DoublesSketch = com.yahoo.sketches.quantiles.DoublesSketch;

	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;

	[Serializable]
	public class ProducerStatsRecorderImpl : ProducerStatsRecorder
	{

		private const long SerialVersionUID = 1L;
		private TimerTask stat;
		internal virtual StatTimeout {get;}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private ProducerImpl<?> producer;
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

		public virtual SendMsgsRate {get;}
		public virtual SendBytesRate {get;}
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

		public ProducerStatsRecorderImpl<T1>(PulsarClientImpl PulsarClient, ProducerConfigurationData Conf, ProducerImpl<T1> Producer)
		{
			this.pulsarClient = PulsarClient;
			this.statsIntervalSeconds = PulsarClient.Configuration.StatsIntervalSeconds;
			this.producer = Producer;
			numMsgsSent = new LongAdder();
			numBytesSent = new LongAdder();
			numSendFailed = new LongAdder();
			numAcksReceived = new LongAdder();
			totalMsgsSent = new LongAdder();
			totalBytesSent = new LongAdder();
			totalSendFailed = new LongAdder();
			totalAcksReceived = new LongAdder();
			ds = DoublesSketch.builder().build(256);
			Init(Conf);
		}

		private void Init(ProducerConfigurationData Conf)
		{
			ObjectMapper M = new ObjectMapper();
			M.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			ObjectWriter W = M.writerWithDefaultPrettyPrinter();

			try
			{
				log.info("Starting Pulsar producer perf with config: {}", W.writeValueAsString(Conf));
				log.info("Pulsar client config: {}", W.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
			}
			catch (IOException E)
			{
				log.error("Failed to dump config info", E);
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

				long CurrentNumMsgsSent = numMsgsSent.sumThenReset();
				long CurrentNumBytesSent = numBytesSent.sumThenReset();
				long CurrentNumSendFailedMsgs = numSendFailed.sumThenReset();
				long CurrentNumAcksReceived = numAcksReceived.sumThenReset();

				totalMsgsSent.add(CurrentNumMsgsSent);
				totalBytesSent.add(CurrentNumBytesSent);
				totalSendFailed.add(CurrentNumSendFailedMsgs);
				totalAcksReceived.add(CurrentNumAcksReceived);

				lock (ds)
				{
					latencyPctValues = ds.getQuantiles(PERCENTILES);
					ds.reset();
				}

				SendMsgsRate = CurrentNumMsgsSent / Elapsed;
				SendBytesRate = CurrentNumBytesSent / Elapsed;

				if ((CurrentNumMsgsSent | CurrentNumSendFailedMsgs | CurrentNumAcksReceived | CurrentNumMsgsSent) != 0)
				{

					for (int I = 0; I < latencyPctValues.Length; I++)
					{
						if (double.IsNaN(latencyPctValues[I]))
						{
							latencyPctValues[I] = 0;
						}
					}

					log.info("[{}] [{}] Pending messages: {} --- Publish throughput: {} msg/s --- {} Mbit/s --- " + "Latency: med: {} ms - 95pct: {} ms - 99pct: {} ms - 99.9pct: {} ms - max: {} ms --- " + "Ack received rate: {} ack/s --- Failed messages: {}", producer.Topic, producer.ProducerName, producer.PendingQueueSize, THROUGHPUT_FORMAT.format(SendMsgsRate), THROUGHPUT_FORMAT.format(SendBytesRate / 1024 / 1024 * 8), DEC.format(latencyPctValues[0] / 1000.0), DEC.format(latencyPctValues[2] / 1000.0), DEC.format(latencyPctValues[3] / 1000.0), DEC.format(latencyPctValues[4] / 1000.0), DEC.format(latencyPctValues[5] / 1000.0), THROUGHPUT_FORMAT.format(CurrentNumAcksReceived / Elapsed), CurrentNumSendFailedMsgs);
				}

			}
			catch (Exception E)
			{
				log.error("[{}] [{}]: {}", producer.Topic, producer.ProducerName, E.Message);
			}
			finally
			{
				// schedule the next stat info
				StatTimeout = pulsarClient.Timer().newTimeout(stat, statsIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
			}

			};

			oldTime = System.nanoTime();
			StatTimeout = pulsarClient.Timer().newTimeout(stat, statsIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
		}


		public override void UpdateNumMsgsSent(long NumMsgs, long TotalMsgsSize)
		{
			numMsgsSent.add(NumMsgs);
			numBytesSent.add(TotalMsgsSize);
		}

		public override void IncrementSendFailed()
		{
			numSendFailed.increment();
		}

		public override void IncrementSendFailed(long NumMsgs)
		{
			numSendFailed.add(NumMsgs);
		}

		public override void IncrementNumAcksReceived(long LatencyNs)
		{
			numAcksReceived.increment();
			lock (ds)
			{
				ds.update(BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.toMillis(LatencyNs));
			}
		}

		public virtual void Reset()
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

		public virtual void UpdateCumulativeStats(ProducerStats Stats)
		{
			if (Stats == null)
			{
				return;
			}
			numMsgsSent.add(Stats.NumMsgsSent);
			numBytesSent.add(Stats.NumBytesSent);
			numSendFailed.add(Stats.NumSendFailed);
			numAcksReceived.add(Stats.NumAcksReceived);
			totalMsgsSent.add(Stats.NumMsgsSent);
			totalBytesSent.add(Stats.NumBytesSent);
			totalSendFailed.add(Stats.NumSendFailed);
			totalAcksReceived.add(Stats.NumAcksReceived);
		}

		public virtual long NumMsgsSent
		{
			get
			{
				return numMsgsSent.longValue();
			}
		}

		public virtual long NumBytesSent
		{
			get
			{
				return numBytesSent.longValue();
			}
		}

		public virtual long NumSendFailed
		{
			get
			{
				return numSendFailed.longValue();
			}
		}

		public virtual long NumAcksReceived
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



		public virtual double SendLatencyMillis50pct
		{
			get
			{
				return latencyPctValues[0];
			}
		}

		public virtual double SendLatencyMillis75pct
		{
			get
			{
				return latencyPctValues[1];
			}
		}

		public virtual double SendLatencyMillis95pct
		{
			get
			{
				return latencyPctValues[2];
			}
		}

		public virtual double SendLatencyMillis99pct
		{
			get
			{
				return latencyPctValues[3];
			}
		}

		public virtual double SendLatencyMillis999pct
		{
			get
			{
				return latencyPctValues[4];
			}
		}

		public virtual double SendLatencyMillisMax
		{
			get
			{
				return latencyPctValues[5];
			}
		}

		public virtual void CancelStatsTimeout()
		{
			if (StatTimeout != null)
			{
				StatTimeout.cancel();
				StatTimeout = null;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProducerStatsRecorderImpl));
	}

}