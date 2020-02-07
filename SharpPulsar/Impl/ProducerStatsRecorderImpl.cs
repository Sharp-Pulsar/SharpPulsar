using App.Metrics.Concurrency;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Util;
using System;
using System.Globalization;
using System.IO;
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
namespace SharpPulsar.Impl
{
	[Serializable]
	public class ProducerStatsRecorderImpl<T> : IProducerStatsRecorder
	{

		private const long SerialVersionUID = 1L;
		private Timer stat;
		internal virtual long StatTimeout { get; set; }
		[NonSerialized]
		private ProducerImpl<object> producer;
		[NonSerialized]
		private PulsarClientImpl pulsarClient;
		private long oldTime;
		private long statsIntervalSeconds;
		[NonSerialized]
		private readonly StripedLongAdder numMsgsSent;
		[NonSerialized]
		private readonly StripedLongAdder numBytesSent;
		[NonSerialized]
		private readonly StripedLongAdder numSendFailed;
		[NonSerialized]
		private readonly StripedLongAdder numAcksReceived;
		[NonSerialized]
		private readonly StripedLongAdder totalMsgsSent;
		[NonSerialized]
		private readonly StripedLongAdder totalBytesSent;
		[NonSerialized]
		private readonly StripedLongAdder totalSendFailed;
		[NonSerialized]
		private readonly StripedLongAdder totalAcksReceived;
		private static readonly NumberFormatInfo DEC = new NumberFormatInfo();
		private static readonly NumberFormatInfo THROUGHPUT_FORMAT = new NumberFormatInfo();
		private readonly DoublesSketch ds;

		public long SendMsgsRate { get; set; }
		public long SendBytesRate { get; set; }
		private volatile double[] latencyPctValues;

		private static readonly double[] PERCENTILES = new double[] { 0.5, 0.75, 0.95, 0.99, 0.999, 1.0 };

		public ProducerStatsRecorderImpl()
		{
			DEC.NumberDecimalSeparator = "0.000";
			THROUGHPUT_FORMAT.NumberDecimalSeparator = "0.00";
			numMsgsSent = new StripedLongAdder();
			numBytesSent = new StripedLongAdder();
			numSendFailed = new StripedLongAdder();
			numAcksReceived = new StripedLongAdder();
			totalMsgsSent = new StripedLongAdder();
			totalBytesSent = new StripedLongAdder();
			totalSendFailed = new StripedLongAdder();
			totalAcksReceived = new StripedLongAdder();
			ds = DoublesSketch.builder().build(256);
		}

		public ProducerStatsRecorderImpl(PulsarClientImpl PulsarClient, ProducerConfigurationData Conf, ProducerImpl<T> Producer)
		{
			DEC.NumberDecimalSeparator = "0.000";
			THROUGHPUT_FORMAT.NumberDecimalSeparator = "0.00";
			this.pulsarClient = PulsarClient;
			this.statsIntervalSeconds = PulsarClient.Configuration.StatsIntervalSeconds;
			this.producer = Producer;
			numMsgsSent = new StripedLongAdder();
			numBytesSent = new StripedLongAdder();
			numSendFailed = new StripedLongAdder();
			numAcksReceived = new StripedLongAdder();
			totalMsgsSent = new StripedLongAdder();
			totalBytesSent = new StripedLongAdder();
			totalSendFailed = new StripedLongAdder();
			totalAcksReceived = new StripedLongAdder();
			ds = DoublesSketch.builder().build(256);
			Init(Conf);
		}

		private void Init(ProducerConfigurationData Conf)
		{
			ObjectMapper M = new ObjectMapper();

			try
			{
				log.info("Starting Pulsar producer perf with config: {}", M.WriteValueAsString(Conf));
				//log.info("Pulsar client config: {}", W.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
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

					totalMsgsSent.Add(CurrentNumMsgsSent);
					totalBytesSent.Add(CurrentNumBytesSent);
					totalSendFailed.Add(CurrentNumSendFailedMsgs);
					totalAcksReceived.Add(CurrentNumAcksReceived);

					lock (ds)
					{
						latencyPctValues = ds.getQuantiles(PERCENTILES);
						ds.Reset();
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


		public void UpdateNumMsgsSent(long NumMsgs, long TotalMsgsSize)
		{
			numMsgsSent.Add(NumMsgs);
			numBytesSent.Add(TotalMsgsSize);
		}

		public void IncrementSendFailed()
		{
			numSendFailed.Increment();
		}

		public void IncrementSendFailed(long NumMsgs)
		{
			numSendFailed.Add(NumMsgs);
		}

		public void IncrementNumAcksReceived(long LatencyNs)
		{
			numAcksReceived.Increment();
			lock (ds)
			{
				ds.update(BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.ToMillis(LatencyNs));
			}
		}

		public virtual void Reset()
		{
			numMsgsSent.Reset();
			numBytesSent.Reset();
			numSendFailed.Reset();
			numAcksReceived.Reset();
			totalMsgsSent.Reset();
			totalBytesSent.Reset();
			totalSendFailed.Reset();
			totalAcksReceived.Reset();
		}

		public virtual void UpdateCumulativeStats(IProducerStats Stats)
		{
			if (Stats == null)
			{
				return;
			}
			numMsgsSent.Add(Stats.NumMsgsSent);
			numBytesSent.Add(Stats.NumBytesSent);
			numSendFailed.Add(Stats.NumSendFailed);
			numAcksReceived.Add(Stats.NumAcksReceived);
			totalMsgsSent.Add(Stats.NumMsgsSent);
			totalBytesSent.Add(Stats.NumBytesSent);
			totalSendFailed.Add(Stats.NumSendFailed);
			totalAcksReceived.Add(Stats.NumAcksReceived);
		}

		public virtual long NumMsgsSent
		{
			get
			{
				return numMsgsSent.GetValue();
			}
		}

		public virtual long NumBytesSent
		{
			get
			{
				return numBytesSent.GetValue();
			}
		}

		public virtual long NumSendFailed
		{
			get
			{
				return numSendFailed.GetValue();
			}
		}

		public virtual long NumAcksReceived
		{
			get
			{
				return numAcksReceived.GetValue();
			}
		}

		public virtual long TotalMsgsSent
		{
			get
			{
				return totalMsgsSent.GetValue();
			}
		}

		public virtual long TotalBytesSent
		{
			get
			{
				return totalBytesSent.GetValue();
			}
		}

		public virtual long TotalSendFailed
		{
			get
			{
				return totalSendFailed.GetValue();
			}
		}

		public virtual long TotalAcksReceived
		{
			get
			{
				return totalAcksReceived.GetValue();
			}
		}



		public virtual double SendLatencyMillis50Pct
		{
			get
			{
				return latencyPctValues[0];
			}
		}

		public virtual double SendLatencyMillis75Pct
		{
			get
			{
				return latencyPctValues[1];
			}
		}

		public virtual double SendLatencyMillis95Pct
		{
			get
			{
				return latencyPctValues[2];
			}
		}

		public virtual double SendLatencyMillis99Pct
		{
			get
			{
				return latencyPctValues[3];
			}
		}

		public virtual double SendLatencyMillis999Pct
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

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(ProducerStatsRecorderImpl<T>));
	}

}