using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Akka.Actor;
using Akka.Event;
using App.Metrics.Concurrency;
using SharpPulsar.Configuration;
using SharpPulsar.Stats.Consumer.Api;
using SharpPulsar.Interfaces;
using System.Collections.Generic;

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
namespace SharpPulsar.Stats.Consumer
{
    public class ConsumerStatsRecorder<T> : IConsumerStatsRecorder
	{

		private const long SerialVersionUid = 1L;
        private ICancelable _statTimeout;
		private long _oldTime;
        private readonly string _topic;
        private readonly string _name;
        private readonly string _subscription;
		private readonly TimeSpan _statsIntervalSeconds;
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

        private readonly ActorSystem _system;
        private readonly ILoggingAdapter _log;

		public virtual double RateMsgsReceived { get; set; }
		public virtual double RateBytesReceived { get; set; }

		private static readonly NumberFormatInfo ThroughputFormat = new NumberFormatInfo();

		public double OldTime => _oldTime;
		public ConsumerStatsRecorder()
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

		public ConsumerStatsRecorder(ActorSystem system, ConsumerConfigurationData<T> conf, string topic, string consumerName, string subscription, TimeSpan statsIntervalSeconds)
        {
            _system = system;
            _log = system.Log;
            _topic = topic;
            _name = consumerName;
            _subscription = subscription;
			ThroughputFormat.NumberDecimalSeparator = "0.00";
			_statsIntervalSeconds = statsIntervalSeconds;
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
            try
			{
				_log.Info($"Starting Pulsar consumer status recorder with config: {JsonSerializer.Serialize(conf, new JsonSerializerOptions{WriteIndented = true})}");
			}
			catch (IOException e)
			{
				_log.Error($"Failed to dump config info: {e}");
			}

			_oldTime = DateTime.Now.Millisecond;
			_statTimeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(_statsIntervalSeconds, StatsAction);
		}

        private void StatsAction()
        {

			try
			{
				long now = DateTime.Now.Millisecond;
				var elapsed = (now - OldTime) / 1e9;
				_oldTime = now;
				var currentNumMsgsReceived = _numMsgsReceived.GetAndReset();
				var currentNumBytesReceived = _numBytesReceived.GetAndReset();
				var currentNumReceiveFailed = _numReceiveFailed.GetAndReset();
				var currentNumBatchReceiveFailed = _numBatchReceiveFailed.GetAndReset();
				var currentNumAcksSent = _numAcksSent.GetAndReset();
				var currentNumAcksFailed = _numAcksFailed.GetAndReset();

				_totalMsgsReceived.Add(currentNumMsgsReceived);
				_totalBytesReceived.Add(currentNumBytesReceived);
				_totalReceiveFailed.Add(currentNumReceiveFailed);
				_totalBatchReceiveFailed.Add(currentNumBatchReceiveFailed);
				_totalAcksSent.Add(currentNumAcksSent);
				_totalAcksFailed.Add(currentNumAcksFailed);

				RateMsgsReceived = currentNumMsgsReceived / elapsed;
				RateBytesReceived = currentNumBytesReceived / elapsed;

				if ((currentNumMsgsReceived | currentNumBytesReceived | currentNumReceiveFailed | currentNumAcksSent | currentNumAcksFailed) != 0)
				{
					_log.Info($"[{_topic}] [{_subscription}] [{_name}] Consume throughput received: {(RateMsgsReceived).ToString(ThroughputFormat)} msgs/s --- {(RateBytesReceived * 8 / 1024 / 1024).ToString(ThroughputFormat)} Mbit/s --- Ack sent rate: {(currentNumAcksSent / elapsed).ToString(ThroughputFormat)} ack/s --- Failed messages: {currentNumReceiveFailed} --- batch messages: {currentNumBatchReceiveFailed} ---Failed acks: {currentNumAcksFailed}");
				}
			}
			catch (Exception e)
			{
				_log.Error($"[{_topic}] [{_name}] [{_subscription}]: {e.Message}");
			}
			finally
			{
				// schedule the next stat info
                _statTimeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(_statsIntervalSeconds, StatsAction);
			}
		}
		public void UpdateNumMsgsReceived(IMessage<T> message)
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

		public virtual ICancelable StatTimeout => _statTimeout;

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
		public virtual void UpdateNumMsgsReceived<T1>(IMessage<T1> message)
		{
			if (message != null)
			{
				_numMsgsReceived.Increment();
				_numBytesReceived.Add(message.Data.Length);
			}
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


		public virtual int? MsgNumInReceiverQueue
		{
			get
			{
				/*if (_consumer is ConsumerBase)
				{
					return ((ConsumerBase<object>)_consumer).incomingMessages.size();
				}*/
				return null;
			}
		}

		public virtual IDictionary<long, int> MsgNumInSubReceiverQueue
		{
			get
			{
				/*if (_consumer is MultiTopicsConsumerImpl)
				{
					IList<ConsumerImpl<object>> consumerList = ((MultiTopicsConsumerImpl)_consumer).Consumers;
					return consumerList.ToDictionary((consumerImpl) => consumerImpl.consumerId, (consumerImpl) => consumerImpl.incomingMessages.size());
				}*/
				return null;
			}
		}

    }

}