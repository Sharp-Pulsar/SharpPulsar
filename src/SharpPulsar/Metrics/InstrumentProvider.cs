
/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace SharpPulsar.Metrics
{
    using GlobalOpenTelemetry = io.opentelemetry.api.GlobalOpenTelemetry;
    using OpenTelemetry = io.opentelemetry.api.OpenTelemetry;
    using Attributes = io.opentelemetry.api.common.Attributes;
    using Meter = io.opentelemetry.api.metrics.Meter;
    using ObservableLongMeasurement = io.opentelemetry.api.metrics.ObservableLongMeasurement;
    using PulsarVersion = org.apache.pulsar.PulsarVersion;

    public class InstrumentProvider
    {

        public static readonly InstrumentProvider NOOP = new InstrumentProvider(OpenTelemetry.noop());

        private readonly Meter meter;

        public InstrumentProvider(OpenTelemetry otel)
        {
            if (otel == null)
            {
                // By default, metrics are disabled, unless the OTel java agent is configured.
                // This allows to enable metrics without any code change.
                otel = GlobalOpenTelemetry.get();
            }
            this.meter = otel.getMeterProvider().meterBuilder("org.apache.pulsar.client").setInstrumentationVersion(PulsarVersion.getVersion()).build();
        }

        public virtual Counter NewCounter(string name, Unit unit, string description, string topic, Attributes attributes)
        {
            return new Counter(meter, name, unit, description, topic, attributes);
        }

        public virtual UpDownCounter NewUpDownCounter(string name, TimeUnit.TimeUnit unit, string description, string topic, Attributes attributes)
        {
            return new UpDownCounter(meter, name, unit, description, topic, attributes);
        }

        public virtual LatencyHistogram NewLatencyHistogram(string name, string description, string topic, Attributes attributes)
        {
            return new LatencyHistogram(meter, name, description, topic, attributes);
        }

        public virtual ObservableUpDownCounter NewObservableUpDownCounter(string name, TimeUnit.TimeUnit unit, string description, string topic, Attributes attributes, System.Action<ObservableLongMeasurement> callback)
        {
            return new ObservableUpDownCounter(meter, name, unit, description, topic, attributes, callback);
        }
    }
}
