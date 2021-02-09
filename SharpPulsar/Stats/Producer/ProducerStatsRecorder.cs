using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Akka.Actor;
using Akka.Event;
using App.Metrics.Concurrency;

using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Utility;

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
namespace SharpPulsar.Stats.Producer
{
    
    public sealed class ProducerStatsRecorder : IProducerStatsRecorder
    {

        private const long SerialVersionUid = 1L;
        internal ICancelable StatTimeout { get; set; }
        private long _oldTime;
        private readonly string _producerName;
        private readonly string _topic;
        private readonly long _pendingQueueSize;
        private readonly ActorSystem _system;
        private readonly long _statsIntervalSeconds;
        private readonly StripedLongAdder _numMsgsSent;
        private readonly StripedLongAdder _numBytesSent;
        private readonly StripedLongAdder _numSendFailed;
        private readonly StripedLongAdder _numAcksReceived;
        private readonly StripedLongAdder _totalMsgsSent;
        private readonly StripedLongAdder _totalBytesSent;
        private readonly StripedLongAdder _totalSendFailed;
        private readonly StripedLongAdder _totalAcksReceived;
        private static readonly NumberFormatInfo Dec = new NumberFormatInfo();
        private static readonly NumberFormatInfo ThroughputFormat = new NumberFormatInfo();
        internal static double[] Latency = new double[256];

        public double SendMsgsRate { get; set; }
        public double SendBytesRate { get; set; }
        private double[] _latencyPctValues;

        private readonly ILoggingAdapter _log;

        private static readonly double[] Percentiles = { 0.5, 0.75, 0.95, 0.99, 0.999, 1.0 };

        public ProducerStatsRecorder(ActorSystem system, string producerName, string topic, long pendingQueueSize)
        {
            _system = system;
            _producerName = producerName;
            _topic = topic;
            _pendingQueueSize = pendingQueueSize;
            _log = system.Log;
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
        }

        public ProducerStatsRecorder(long statsIntervalSeconds, ProducerConfigurationData conf, ActorSystem system, string producerName, string topic, long pendingQueueSize)
        {
            _system = system;
            _producerName = producerName;
            _topic = topic;
            _pendingQueueSize = pendingQueueSize;
            _log = system.Log;
            Dec.NumberDecimalSeparator = "0.000";
            ThroughputFormat.NumberDecimalSeparator = "0.00";
            _statsIntervalSeconds = statsIntervalSeconds;
            _numMsgsSent = new StripedLongAdder();
            _numBytesSent = new StripedLongAdder();
            _numSendFailed = new StripedLongAdder();
            _numAcksReceived = new StripedLongAdder();
            _totalMsgsSent = new StripedLongAdder();
            _totalBytesSent = new StripedLongAdder();
            _totalSendFailed = new StripedLongAdder();
            _totalAcksReceived = new StripedLongAdder();
            Init(conf);
        }

        private void Init(ProducerConfigurationData conf)
        {
            try
            {
                _log.Info($"Starting Pulsar producer perf with config: {JsonSerializer.Serialize(conf, new JsonSerializerOptions{WriteIndented = true})}");
                //log.info("Pulsar client config: {}", W.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
            }
            catch (IOException e)
            {
                _log.Error("Failed to dump config info", e);
            }

            _oldTime = DateTime.Now.Millisecond;
            StatTimeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(_statsIntervalSeconds), StatsAction);
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
            lock (Latency)
            {
                var current = Latency.Length + 1;
                Latency[current] = (TimeSpan.FromTicks(latencyNs).TotalMilliseconds);
            }
        }

        public void Reset()
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

        public void UpdateCumulativeStats(IProducerStats stats)
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

        public long NumMsgsSent => _numMsgsSent.GetValue();

        public long NumBytesSent => _numBytesSent.GetValue();

        public long NumSendFailed => _numSendFailed.GetValue();

        public long NumAcksReceived => _numAcksReceived.GetValue();

        public long TotalMsgsSent => _totalMsgsSent.GetValue();

        public long TotalBytesSent => _totalBytesSent.GetValue();

        public long TotalSendFailed => _totalSendFailed.GetValue();

        public long TotalAcksReceived => _totalAcksReceived.GetValue();


        public double SendLatencyMillis50Pct => _latencyPctValues[0];

        public double SendLatencyMillis75Pct => _latencyPctValues[1];

        public double SendLatencyMillis95Pct => _latencyPctValues[2];

        public double SendLatencyMillis99Pct => _latencyPctValues[3];

        public double SendLatencyMillis999Pct => _latencyPctValues[4];

        public double SendLatencyMillisMax => _latencyPctValues[5];
        public double OldTime => _oldTime;

        public void CancelStatsTimeout()
        {
            if (StatTimeout != null)
            {
                StatTimeout.Cancel();
                StatTimeout = null;
            }
        }

        private void StatsAction()
        {
            try
            {
                long now = DateTime.Now.Millisecond;
                var elapsed = (now - OldTime) / 1e9;
                _oldTime = now;

                var currentNumMsgsSent = _numMsgsSent.GetAndReset();
                var currentNumBytesSent = _numBytesSent.GetAndReset();
                var currentNumSendFailedMsgs = _numSendFailed.GetAndReset();
                var currentNumAcksReceived = _numAcksReceived.GetAndReset();

                _totalMsgsSent.Add(currentNumMsgsSent);
                _totalBytesSent.Add(currentNumBytesSent);
                _totalSendFailed.Add(currentNumSendFailedMsgs);
                _totalAcksReceived.Add(currentNumAcksReceived);

                lock (Latency)
                {
                    _latencyPctValues = GetQuantiles(Percentiles);
                    Latency = new double[256];
                }

                SendMsgsRate = currentNumMsgsSent / elapsed;
                SendBytesRate = currentNumBytesSent / elapsed;

                if ((currentNumMsgsSent | currentNumSendFailedMsgs | currentNumAcksReceived | currentNumMsgsSent) !=
                    0)
                {

                    for (var i = 0; i < _latencyPctValues.Length; i++)
                    {
                        if (double.IsNaN(_latencyPctValues[i]))
                        {
                            _latencyPctValues[i] = 0;
                        }
                    }

                    _log.Info(
                        $"[{_topic}] [{_producerName}] Pending messages: {_pendingQueueSize} --- Publish throughput: {SendMsgsRate.ToString(ThroughputFormat)} msg/s --- {(SendBytesRate / 1024 / 1024 * 8).ToString(ThroughputFormat)} Mbit/s --- " +
                        $"Latency: med: {(_latencyPctValues[0] / 1000.0).ToString(Dec)} ms - 95pct: {(_latencyPctValues[2] / 1000.0).ToString(Dec)} ms - 99pct: {(_latencyPctValues[3] / 1000.0).ToString(Dec)} ms - 99.9pct: {(_latencyPctValues[4] / 1000.0).ToString(Dec)} ms - max: {(_latencyPctValues[5] / 1000.0).ToString(Dec)} ms --- " +
                        $"Ack received rate: {(currentNumAcksReceived / elapsed).ToString(ThroughputFormat)} ack/s --- Failed messages: {currentNumSendFailedMsgs}");
                }

            }
            catch (Exception e)
            {
                _log.Error("[{}] [{}]: {}", _topic, _producerName, e.Message);
            }
            finally
            {
                // schedule the next stat info
                StatTimeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(_statsIntervalSeconds), StatsAction);
            }
        }

        //https://stackoverflow.com/questions/8137391/percentile-calculation
        public double[] GetQuantiles(double[] ranks)
        {
            Array.Sort(Latency);
            var quantiles = new double[ranks.Length];
            var l = Latency.Length;
            for (var i = 0; i < ranks.Length; i++)
            {
                var rank = ranks[i];
                var n = (l - 1) * rank + 1;
                if (n == 0.0)
                {
                    quantiles[i] = Latency.Min();
                }
                else if (n == 1.0)
                {
                    quantiles[i] = Latency.Max();
                }
                else if (n == l && (n != 0 && n != 1))
                {
                    quantiles[i] = Latency[l - 1];
                }
                else
                {
                    var k = (int)n;
                    var d = n - k;
                    quantiles[i] = Latency[k - 1] + d * (Latency[k] - Latency[k - 1]);
                }
            }

            return quantiles;
        }
    }

}