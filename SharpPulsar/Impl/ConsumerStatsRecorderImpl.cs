using System;
using System.Globalization;
using System.IO;
using App.Metrics.Concurrency;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

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
	public class ConsumerStatsRecorderImpl<T> : ConsumerStatsRecorder
	{

		private const long SerialVersionUid = 1L;
		private ITimerTask _stat;
		private ITimeout _statTimeout;
		private ConsumerImpl<T> _consumer;
		private PulsarClientImpl _pulsarClient;
		private long _oldTime;
		private long _statsIntervalSeconds;
		private readonly StripedLongAdder _numMsgsReceived;
		private readonly StripedLongAdder _numBytesReceived;
		private readonly StripedLongAdder _numReceiveFailed;
		private readonly StripedLongAdder _numBatchReceiveFailed;
		private readonly StripedLongAdder _numAcksSent;
		private readonly StripedLongAdder _numAcksFailed;
		private readonly StripedLongAdder _totalMsgsReceived;
		private readonly StripedLongAdder _totalBytesReceived;
		private readonly StripedLongAdder _totalReceiveFailed;
		private readonly StripedLongAdder _totalBatchReceiveFailed;
		private readonly StripedLongAdder _totalAcksSent;
		private readonly StripedLongAdder _totalAcksFailed;

		public virtual double RateMsgsReceived {get; set; }
		public virtual double RateBytesReceived {get; set; }

		private static readonly NumberFormatInfo ThroughputFormat = new NumberFormatInfo();

        public double OldTime => _oldTime;
		public ConsumerStatsRecorderImpl()
		{
            ThroughputFormat.NumberDecimalSeparator = "0.00";
			_numMsgsReceived = new StripedLongAdder();
			_numBytesReceived = new StripedLongAdder();
			_numReceiveFailed = new StripedLongAdder();
			_numBatchReceiveFailed = new StripedLongAdder();
			_numAcksSent = new StripedLongAdder();
			_numAcksFailed = new StripedLongAdder();
			_totalMsgsReceived = new StripedLongAdder();
			_totalBytesReceived = new StripedLongAdder();
			_totalReceiveFailed = new StripedLongAdder();
			_totalBatchReceiveFailed = new StripedLongAdder();
			_totalAcksSent = new StripedLongAdder();
			_totalAcksFailed = new StripedLongAdder();
		}

		public ConsumerStatsRecorderImpl(PulsarClientImpl pulsarClient, ConsumerConfigurationData<T> conf, ConsumerImpl<T> consumer)
		{
            ThroughputFormat.NumberDecimalSeparator = "0.00";
			this._pulsarClient = pulsarClient;
			this._consumer = consumer;
			this._statsIntervalSeconds = pulsarClient.Configuration.StatsIntervalSeconds;
			_numMsgsReceived = new StripedLongAdder();
			_numBytesReceived = new StripedLongAdder();
			_numReceiveFailed = new StripedLongAdder();
			_numBatchReceiveFailed = new StripedLongAdder();
			_numAcksSent = new StripedLongAdder();
			_numAcksFailed = new StripedLongAdder();
			_totalMsgsReceived = new StripedLongAdder();
			_totalBytesReceived = new StripedLongAdder();
			_totalReceiveFailed = new StripedLongAdder();
			_totalBatchReceiveFailed = new StripedLongAdder();
			_totalAcksSent = new StripedLongAdder();
			_totalAcksFailed = new StripedLongAdder();
			Init(conf);
		}

		private void Init(ConsumerConfigurationData<T> conf)
		{
			/*var m = new ObjectMapper();
			m.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			ObjectWriter w = m.writerWithDefaultPrettyPrinter();*/

			try
			{
				//Log.LogInformation("Starting Pulsar consumer status recorder with config: {}", w.writeValueAsString(conf));
				//Log.LogInformation("Pulsar client config: {}", w.withoutAttribute("authentication").writeValueAsString(_pulsarClient.Configuration));
			}
			catch (IOException e)
			{
				Log.LogError("Failed to dump config info: {}", e);
			}

			_oldTime = DateTime.Now.Millisecond;
			_statTimeout = _pulsarClient.Timer.NewTimeout(new StatsTimerTask(this), TimeSpan.FromSeconds(_statsIntervalSeconds));
		}

		public class StatsTimerTask : ITimerTask
		{
            private readonly ConsumerStatsRecorderImpl<T> _outerInstance;

            public StatsTimerTask(ConsumerStatsRecorderImpl<T> outerInstance)
            {
                _outerInstance = outerInstance;
            }
			public void Run(ITimeout timeout)
			{
				if (timeout.Canceled)
				{
					return;
				}
				try
				{
					long now = DateTime.Now.Millisecond;
					var elapsed = (now - _outerInstance.OldTime) / 1e9;
                    _outerInstance._oldTime = now;
					var currentNumMsgsReceived = _outerInstance._numMsgsReceived.GetAndReset();
					var currentNumBytesReceived = _outerInstance._numBytesReceived.GetAndReset();
					var currentNumReceiveFailed = _outerInstance._numReceiveFailed.GetAndReset();
					var currentNumBatchReceiveFailed = _outerInstance._numBatchReceiveFailed.GetAndReset();
					var currentNumAcksSent = _outerInstance._numAcksSent.GetAndReset();
					var currentNumAcksFailed = _outerInstance._numAcksFailed.GetAndReset();

                    _outerInstance._totalMsgsReceived.Add(currentNumMsgsReceived);
                    _outerInstance._totalBytesReceived.Add(currentNumBytesReceived);
                    _outerInstance._totalReceiveFailed.Add(currentNumReceiveFailed);
                    _outerInstance._totalBatchReceiveFailed.Add(currentNumBatchReceiveFailed);
                    _outerInstance._totalAcksSent.Add(currentNumAcksSent);
                    _outerInstance._totalAcksFailed.Add(currentNumAcksFailed);

                    _outerInstance.RateMsgsReceived = currentNumMsgsReceived / elapsed;
                    _outerInstance.RateBytesReceived = currentNumBytesReceived / elapsed;

					if ((currentNumMsgsReceived | currentNumBytesReceived | currentNumReceiveFailed | currentNumAcksSent | currentNumAcksFailed) != 0)
					{
						Log.LogInformation("[{}] [{}] [{}] Prefetched messages: {} --- " + "Consume throughput received: {} msgs/s --- {} Mbit/s --- " + "Ack sent rate: {} ack/s --- " + "Failed messages: {} --- batch messages: {} ---" + "Failed acks: {}", _outerInstance._consumer.Topic, _outerInstance._consumer.Subscription, _outerInstance._consumer.ConsumerName, _outerInstance._consumer.IncomingMessages.size(), (_outerInstance.RateMsgsReceived).ToString(ThroughputFormat), (_outerInstance.RateBytesReceived * 8 / 1024 / 1024).ToString(ThroughputFormat), (currentNumAcksSent / elapsed).ToString(ThroughputFormat), currentNumReceiveFailed, currentNumBatchReceiveFailed, currentNumAcksFailed);
					}
				}
				catch (System.Exception e)
				{
					Log.LogError("[{}] [{}] [{}]: {}", _outerInstance._consumer.Topic, _outerInstance._consumer.Subscription, _outerInstance._consumer.ConsumerName, e.Message);
				}
				finally
				{
					// schedule the next stat info
                    _outerInstance._statTimeout = _outerInstance._pulsarClient.Timer.NewTimeout(this, TimeSpan.FromSeconds(_outerInstance._statsIntervalSeconds));
				}
			}
		}
		public void UpdateNumMsgsReceived<T1>(IMessage<T1> message)
		{
			if (message != null)
			{
				_numMsgsReceived.Increment();
				_numBytesReceived.Add(message.Data.Length);
			}
		}

		public void IncrementNumAcksSent(long numAcks)
		{
			_numAcksSent.Add(numAcks);
		}

		public void IncrementNumAcksFailed()
		{
			_numAcksFailed.Increment();
		}

		public void IncrementNumReceiveFailed()
		{
			_numReceiveFailed.Increment();
		}

		public void IncrementNumBatchReceiveFailed()
		{
			_numBatchReceiveFailed.Increment();
		}

		public virtual ITimeout StatTimeout => _statTimeout;

        public void Reset()
		{
			_numMsgsReceived.Reset();
			_numBytesReceived.Reset();
			_numReceiveFailed.Reset();
			_numBatchReceiveFailed.Reset();
			_numAcksSent.Reset();
			_numAcksFailed.Reset();
			_totalMsgsReceived.Reset();
			_totalBytesReceived.Reset();
			_totalReceiveFailed.Reset();
			_totalBatchReceiveFailed.Reset();
			_totalAcksSent.Reset();
			_totalAcksFailed.Reset();
		}

		public void UpdateCumulativeStats(IConsumerStats stats)
		{
			if (stats == null)
			{
				return;
			}
			_numMsgsReceived.Add(stats.NumMsgsReceived);
			_numBytesReceived.Add(stats.NumBytesReceived);
			_numReceiveFailed.Add(stats.NumReceiveFailed);
			_numBatchReceiveFailed.Add(stats.NumBatchReceiveFailed);
			_numAcksSent.Add(stats.NumAcksSent);
			_numAcksFailed.Add(stats.NumAcksFailed);
			_totalMsgsReceived.Add(stats.TotalMsgsReceived);
			_totalBytesReceived.Add(stats.TotalBytesReceived);
			_totalReceiveFailed.Add(stats.TotalReceivedFailed);
			_totalBatchReceiveFailed.Add(stats.TotaBatchReceivedFailed);
			_totalAcksSent.Add(stats.TotalAcksSent);
			_totalAcksFailed.Add(stats.TotalAcksFailed);
		}

		public virtual long NumMsgsReceived => _numMsgsReceived.GetValue();

        public virtual long NumBytesReceived => _numBytesReceived.GetValue();

        public virtual long NumAcksSent => _numAcksSent.GetValue();

        public virtual long NumAcksFailed => _numAcksFailed.GetValue();

        public virtual long NumReceiveFailed => _numReceiveFailed.GetValue();

        public virtual long NumBatchReceiveFailed => _numBatchReceiveFailed.GetValue();

        public virtual long TotalMsgsReceived => _totalMsgsReceived.GetValue();

        public virtual long TotalBytesReceived => _totalBytesReceived.GetValue();

        public virtual long TotalReceivedFailed => _totalReceiveFailed.GetValue();

        public virtual long TotaBatchReceivedFailed => _totalBatchReceiveFailed.GetValue();

        public virtual long TotalAcksSent => _totalAcksSent.GetValue();

        public virtual long TotalAcksFailed => _totalAcksFailed.GetValue();


        private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(ConsumerStatsRecorderImpl<T>));
	}

}