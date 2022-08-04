using System;
using System.Collections.Generic;
using SharpPulsar.Interfaces;

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
namespace SharpPulsar.Builder
{
    public class ControlledClusterFailoverBuilder : IControlledClusterFailoverBuilder
    {
        internal string defaultServiceUrl;
        internal string urlProvider;
        internal IDictionary<string, string> Header = null;
        internal TimeSpan Interval = TimeSpan.FromMilliseconds(30_000);

        public virtual IControlledClusterFailoverBuilder DefaultServiceUrl(string ServiceUrl)
        {
            defaultServiceUrl = ServiceUrl;
            return this;
        }
        public virtual IControlledClusterFailoverBuilder UrlProvider(string UrlProvider)
        {
            urlProvider = UrlProvider;
            return this;
        }

        public virtual IControlledClusterFailoverBuilder UrlProviderHeader(IDictionary<string, string> header)
        {
            Header = header;
            return this;
        }

        public virtual IControlledClusterFailoverBuilder CheckInterval(TimeSpan interval)
        {
            Interval = interval;
            return this;
        }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(defaultServiceUrl))
                throw new ArgumentNullException("default service url shouldn't be null");

            if (string.IsNullOrWhiteSpace(urlProvider))
                throw new ArgumentNullException("urlProvider shouldn't be null");

            CheckArgument(Interval > TimeSpan.Zero, "checkInterval should > 0");
        }

        public static void CheckArgument(bool expression, object errorMessage)
        {
            if (!expression)
            {
                throw new ArgumentException(errorMessage.ToString());
            }
        }
    }
}