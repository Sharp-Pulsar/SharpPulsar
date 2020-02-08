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
	public class ProducerStatsRecorderImpl<T> : Api.IProducerStatsRecorder
	{

		private const long SerialVersionUid = 1L;
		private Timer _stat;
		internal virtual long StatTimeout { get; set; }
		[NonSerialized]
		private ProducerImpl<object> _producer;
		[NonSerialized]
		private PulsarClientImpl _pulsarClient;
		private long _oldTime;
		private long _statsIntervalSeconds;
		[NonSerialized]
		private readonly StripedLongAdder _numMsgsSent;
		[NonSerialized]
		private readonly StripedLongAdder _numBytesSent;
		[NonSerialized]
		private readonly StripedLongAdder _numSendFailed;
		[NonSerialized]
		private readonly StripedLongAdder _numAcksReceived;
		[NonSerialized]
		private readonly StripedLongAdder _totalMsgsSent;
		[NonSerialized]
		private readonly StripedLongAdder _totalBytesSent;
		[NonSerialized]
		private readonly StripedLongAdder _totalSendFailed;
		[NonSerialized]
		private readonly StripedLongAdder _totalAcksReceived;
		private static readonly NumberFormatInfo Dec = new NumberFormatInfo();
		private static readonly NumberFormatInfo ThroughputFormat = new NumberFormatInfo();
		private readonly DoublesSketch _ds;

		public double SendMsgsRate { get; set; }
		public double SendBytesRate { get; set; }
		private volatile double[] _latencyPctValues;

		private static readonly double[] Percentiles = new double[] { 0.5, 0.75, 0.95, 0.99, 0.999, 1.0 };

		public ProducerStatsRecorderImpl()
		{
			Dec.NumberDecimalSeparator = "0.000";
			ThroughputFormat.NumberDecimalSeparator = "0.00";
			_numMsgsSent = new StripedLongAdder();
			_numBytesSent = new StripedLongAdder();
			_numSendFailed = new StripedLongAdder();
			_numAcksReceived = new StripedLongAdder();
			_totalMsgsSent = new StripedLongAdder();
			_totalBytesSent = new StripedLongAdder();
			_totalSendFailed = new StripedLongAdder();
			_totalAcksReceived = new StripedLongAdder();
			_ds = DoublesSketch.builder().build(256);
		}

		public ProducerStatsRecorderImpl(PulsarClientImpl pulsarClient, ProducerConfigurationData conf, ProducerImpl<T> producer)
		{
			Dec.NumberDecimalSeparator = "0.000";
			ThroughputFormat.NumberDecimalSeparator = "0.00";
			this._pulsarClient = pulsarClient;
			this._statsIntervalSeconds = pulsarClient.Configuration.StatsIntervalSeconds;
			this._producer = producer;
			_numMsgsSent = new StripedLongAdder();
			_numBytesSent = new StripedLongAdder();
			_numSendFailed = new StripedLongAdder();
			_numAcksReceived = new StripedLongAdder();
			_totalMsgsSent = new StripedLongAdder();
			_totalBytesSent = new StripedLongAdder();
			_totalSendFailed = new StripedLongAdder();
			_totalAcksReceived = new StripedLongAdder();
			_ds = DoublesSketch.builder().build(256);
			Init(conf);
		}

		private void Init(ProducerConfigurationData conf)
		{
			ObjectMapper m = new ObjectMapper();

			try
			{
				Log.LogInformation("Starting Pulsar producer perf with config: {}", m.WriteValueAsString(conf));
				//log.info("Pulsar client config: {}", W.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
			}
			catch (IOException e)
			{
				Log.LogError("Failed to dump config info", e);
			}

			_stat = (timeout) =>
			{

				if (timeout.Cancelled)
				{
					return;
				}

				try
				{
					long now = System.nanoTime();
					double elapsed = (now - _oldTime) / 1e9;
					_oldTime = now;

					long currentNumMsgsSent = _numMsgsSent.sumThenReset();
					long currentNumBytesSent = _numBytesSent.sumThenReset();
					long currentNumSendFailedMsgs = _numSendFailed.sumThenReset();
					long currentNumAcksReceived = _numAcksReceived.sumThenReset();

					_totalMsgsSent.Add(currentNumMsgsSent);
					_totalBytesSent.Add(currentNumBytesSent);
					_totalSendFailed.Add(currentNumSendFailedMsgs);
					_totalAcksReceived.Add(currentNumAcksReceived);

					lock (_ds)
					{
						_latencyPctValues = _ds.getQuantiles(Percentiles);
						_ds.Reset();
					}

					SendMsgsRate = currentNumMsgsSent / elapsed;
					SendBytesRate = currentNumBytesSent / elapsed;

					if ((currentNumMsgsSent | currentNumSendFailedMsgs | currentNumAcksReceived | currentNumMsgsSent) != 0)
					{

						for (int i = 0; i < _latencyPctValues.Length; i++)
						{
							if (double.IsNaN(_latencyPctValues[i]))
							{
								_latencyPctValues[i] = 0;
							}
						}

						Log.info("[{}] [{}] Pending messages: {} --- Publish throughput: {} msg/s --- {} Mbit/s --- " + "Latency: med: {} ms - 95pct: {} ms - 99pct: {} ms - 99.9pct: {} ms - max: {} ms --- " + "Ack received rate: {} ack/s --- Failed messages: {}", _producer.Topic, _producer.ProducerName, _producer.PendingQueueSize, ThroughputFormat.format(SendMsgsRate), ThroughputFormat.format(SendBytesRate / 1024 / 1024 * 8), Dec.format(_latencyPctValues[0] / 1000.0), Dec.format(_latencyPctValues[2] / 1000.0), Dec.format(_latencyPctValues[3] / 1000.0), Dec.format(_latencyPctValues[4] / 1000.0), Dec.format(_latencyPctValues[5] / 1000.0), ThroughputFormat.format(currentNumAcksReceived / elapsed), currentNumSendFailedMsgs);
					}

				}
				catch (Exception e)
				{
					Log.error("[{}] [{}]: {}", _producer.Topic, _producer.ProducerName, e.Message);
				}
				finally
				{
					// schedule the next stat info
					StatTimeout = _pulsarClient.Timer().newTimeout(_stat, _statsIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
				}

			};

			_oldTime = System.nanoTime();
			StatTimeout = _pulsarClient.Timer().newTimeout(_stat, _statsIntervalSeconds, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
		}


		public void UpdateNumMsgsSent(long numMsgs, long totalMsgsSize)
		{
			_numMsgsSent.Add(numMsgs);
			_numBytesSent.Add(totalMsgsSize);
		}

		public void IncrementSendFailed()
		{
			_numSendFailed.Increment();
		}

		public void IncrementSendFailed(long numMsgs)
		{
			_numSendFailed.Add(numMsgs);
		}

		public void IncrementNumAcksReceived(long latencyNs)
		{
			_numAcksReceived.Increment();
			lock (_ds)
			{
				_ds.update(BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.ToMillis(latencyNs));
			}
		}

		public virtual void Reset()
		{
			_numMsgsSent.Reset();
			_numBytesSent.Reset();
			_numSendFailed.Reset();
			_numAcksReceived.Reset();
			_totalMsgsSent.Reset();
			_totalBytesSent.Reset();
			_totalSendFailed.Reset();
			_totalAcksReceived.Reset();
		}

		public virtual void UpdateCumulativeStats(IProducerStats stats)
		{
			if (stats == null)
			{
				return;
			}
			_numMsgsSent.Add(stats.NumMsgsSent);
			_numBytesSent.Add(stats.NumBytesSent);
			_numSendFailed.Add(stats.NumSendFailed);
			_numAcksReceived.Add(stats.NumAcksReceived);
			_totalMsgsSent.Add(stats.NumMsgsSent);
			_totalBytesSent.Add(stats.NumBytesSent);
			_totalSendFailed.Add(stats.NumSendFailed);
			_totalAcksReceived.Add(stats.NumAcksReceived);
		}

		public virtual long NumMsgsSent
		{
			get
			{
				return _numMsgsSent.GetValue();
			}
		}

		public virtual long NumBytesSent
		{
			get
			{
				return _numBytesSent.GetValue();
			}
		}

		public virtual long NumSendFailed
		{
			get
			{
				return _numSendFailed.GetValue();
			}
		}

		public virtual long NumAcksReceived
		{
			get
			{
				return _numAcksReceived.GetValue();
			}
		}

		public virtual long TotalMsgsSent
		{
			get
			{
				return _totalMsgsSent.GetValue();
			}
		}

		public virtual long TotalBytesSent
		{
			get
			{
				return _totalBytesSent.GetValue();
			}
		}

		public virtual long TotalSendFailed
		{
			get
			{
				return _totalSendFailed.GetValue();
			}
		}

		public virtual long TotalAcksReceived
		{
			get
			{
				return _totalAcksReceived.GetValue();
			}
		}



		public virtual double SendLatencyMillis50Pct
		{
			get
			{
				return _latencyPctValues[0];
			}
		}

		public virtual double SendLatencyMillis75Pct
		{
			get
			{
				return _latencyPctValues[1];
			}
		}

		public virtual double SendLatencyMillis95Pct
		{
			get
			{
				return _latencyPctValues[2];
			}
		}

		public virtual double SendLatencyMillis99Pct
		{
			get
			{
				return _latencyPctValues[3];
			}
		}

		public virtual double SendLatencyMillis999Pct
		{
			get
			{
				return _latencyPctValues[4];
			}
		}

		public virtual double SendLatencyMillisMax
		{
			get
			{
				return _latencyPctValues[5];
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

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(ProducerStatsRecorderImpl<T>));
	}

}