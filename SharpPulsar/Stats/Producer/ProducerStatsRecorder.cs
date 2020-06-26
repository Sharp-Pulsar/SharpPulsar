using System;
using System.Globalization;
using System.IO;
using System.Linq;
using App.Metrics.Concurrency;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
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
    [Serializable]
    public sealed class ProducerStatsRecorder : IProducerStatsRecorder
    {

        private const long SerialVersionUid = 1L;
        internal ITimeout StatTimeout { get; set; }
        [NonSerialized] private IProducer _producer;
        [NonSerialized] private PulsarClientImpl _pulsarClient;
        private long _oldTime;
        private long _statsIntervalSeconds;
        [NonSerialized] private readonly StripedLongAdder _numMsgsSent;
        [NonSerialized] private readonly StripedLongAdder _numBytesSent;
        [NonSerialized] private readonly StripedLongAdder _numSendFailed;
        [NonSerialized] private readonly StripedLongAdder _numAcksReceived;
        [NonSerialized] private readonly StripedLongAdder _totalMsgsSent;
        [NonSerialized] private readonly StripedLongAdder _totalBytesSent;
        [NonSerialized] private readonly StripedLongAdder _totalSendFailed;
        [NonSerialized] private readonly StripedLongAdder _totalAcksReceived;
        private static readonly NumberFormatInfo Dec = new NumberFormatInfo();
        private static readonly NumberFormatInfo ThroughputFormat = new NumberFormatInfo();
        internal static double[] Latency = new double[256];

        public double SendMsgsRate { get; set; }
        public double SendBytesRate { get; set; }
        private volatile double[] _latencyPctValues;

        private static readonly double[] Percentiles = new double[] { 0.5, 0.75, 0.95, 0.99, 0.999, 1.0 };

        public ProducerStatsRecorder()
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
        }

        public ProducerStatsRecorder(PulsarClientImpl pulsarClient, ProducerConfigurationData conf,
            IProducer producer)
        {
            Dec.NumberDecimalSeparator = "0.000";
            ThroughputFormat.NumberDecimalSeparator = "0.00";
            _pulsarClient = pulsarClient;
            _statsIntervalSeconds = pulsarClient.Configuration.StatsIntervalSeconds;
            _producer = producer;
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
            var m = new ObjectMapper();

            try
            {
                Log.LogInformation("Starting Pulsar producer perf with config: {}", m.WriteValueAsString(conf));
                //log.info("Pulsar client config: {}", W.withoutAttribute("authentication").writeValueAsString(pulsarClient.Configuration));
            }
            catch (IOException e)
            {
                Log.LogError("Failed to dump config info", e);
            }

            _oldTime = DateTime.Now.Millisecond;
            StatTimeout = _pulsarClient.Timer.NewTimeout(new StatsTimerTask(this), TimeSpan.FromSeconds(_statsIntervalSeconds));
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
                Latency[current] = (BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.ToMillis(latencyNs));
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

        public class StatsTimerTask : ITimerTask
        {
            private readonly ProducerStatsRecorder _outerInstance;

            public StatsTimerTask(ProducerStatsRecorder outerInstance)
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

                    var currentNumMsgsSent = _outerInstance._numMsgsSent.GetAndReset();
                    var currentNumBytesSent = _outerInstance._numBytesSent.GetAndReset();
                    var currentNumSendFailedMsgs = _outerInstance._numSendFailed.GetAndReset();
                    var currentNumAcksReceived = _outerInstance._numAcksReceived.GetAndReset();

                    _outerInstance._totalMsgsSent.Add(currentNumMsgsSent);
                    _outerInstance._totalBytesSent.Add(currentNumBytesSent);
                    _outerInstance._totalSendFailed.Add(currentNumSendFailedMsgs);
                    _outerInstance._totalAcksReceived.Add(currentNumAcksReceived);

                    lock (Latency)
                    {
                        _outerInstance._latencyPctValues = _outerInstance.GetQuantiles(Percentiles);
                        Latency = new double[256];
                    }

                    _outerInstance.SendMsgsRate = currentNumMsgsSent / elapsed;
                    _outerInstance.SendBytesRate = currentNumBytesSent / elapsed;

                    if ((currentNumMsgsSent | currentNumSendFailedMsgs | currentNumAcksReceived | currentNumMsgsSent) !=
                        0)
                    {

                        for (var i = 0; i < _outerInstance._latencyPctValues.Length; i++)
                        {
                            if (double.IsNaN(_outerInstance._latencyPctValues[i]))
                            {
                                _outerInstance._latencyPctValues[i] = 0;
                            }
                        }

                        Log.LogInformation(
                            "[{}] [{}] Pending messages: {} --- Publish throughput: {} msg/s --- {} Mbit/s --- " +
                            "Latency: med: {} ms - 95pct: {} ms - 99pct: {} ms - 99.9pct: {} ms - max: {} ms --- " +
                            "Ack received rate: {} ack/s --- Failed messages: {}", _outerInstance._producer.Topic,
                            //_outerInstance._producer.ProducerName, _outerInstance._producer.PendingQueueSize,
                            _outerInstance.SendMsgsRate.ToString(ThroughputFormat),
                            (_outerInstance.SendBytesRate / 1024 / 1024 * 8).ToString(ThroughputFormat),
                            (_outerInstance._latencyPctValues[0] / 1000.0).ToString(Dec),
                            (_outerInstance._latencyPctValues[2] / 1000.0).ToString(Dec),
                            (_outerInstance._latencyPctValues[3] / 1000.0).ToString(Dec),
                            (_outerInstance._latencyPctValues[4] / 1000.0).ToString(Dec),
                            (_outerInstance._latencyPctValues[5] / 1000.0).ToString(Dec),
                            (currentNumAcksReceived / elapsed).ToString(ThroughputFormat), currentNumSendFailedMsgs);
                    }

                }
                catch (System.Exception e)
                {
                    Log.LogError("[{}] [{}]: {}", _outerInstance._producer.Topic, _outerInstance._producer.ProducerName,
                        e.Message);
                }
                finally
                {
                    // schedule the next stat info
                    _outerInstance.StatTimeout = _outerInstance._pulsarClient.Timer.NewTimeout(this,
                        TimeSpan.FromSeconds(_outerInstance._statsIntervalSeconds));
                }
            }
        }

        private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(ProducerStatsRecorder));

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