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
    public class AutoClusterFailoverBuilder : IAutoClusterFailoverBuilder
    {
        internal string primary;
        internal IList<string> secondary;
        internal IDictionary<string, IAuthentication> SecondaryAuthentications = null;
        internal IDictionary<string, string> SecondaryTlsTrustCertsFilePaths = null;
        internal IDictionary<string, string> SecondaryTlsTrustStorePaths = null;
        internal IDictionary<string, string> SecondaryTlsTrustStorePasswords = null;
        internal FailoverPolicy failoverPolicy = Interfaces.FailoverPolicy.ORDER;
        internal TimeSpan FailoverDelayNs;
        internal TimeSpan SwitchBackDelayNs;
        internal TimeSpan CheckIntervalMs = TimeSpan.FromMilliseconds(30_000);

        public virtual IAutoClusterFailoverBuilder Primary(string Primary)
        {
            primary = Primary;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder Secondary(IList<string> Secondary)
        {
            secondary = Secondary;
            return this;
        }
        public virtual IAutoClusterFailoverBuilder FailoverPolicy(FailoverPolicy Policy)
        {
            failoverPolicy = Policy;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder SecondaryAuthentication(IDictionary<string, IAuthentication> authentication)
        {
            SecondaryAuthentications = authentication;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder SecondaryTlsTrustCertsFilePath(IDictionary<string, string> tlsTrustCertsFilePath)
        {
            SecondaryTlsTrustCertsFilePaths = tlsTrustCertsFilePath;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder SecondaryTlsTrustStorePath(IDictionary<string, string> tlsTrustStorePath)
        {
            SecondaryTlsTrustStorePaths = tlsTrustStorePath;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder SecondaryTlsTrustStorePassword(IDictionary<string, string> tlsTrustStorePassword)
        {
            SecondaryTlsTrustStorePasswords = tlsTrustStorePassword;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder FailoverDelay(TimeSpan failoverDelay)
        {
            FailoverDelayNs = failoverDelay;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder SwitchBackDelay(TimeSpan switchBackDelay)
        {
            SwitchBackDelayNs = switchBackDelay;
            return this;
        }

        public virtual IAutoClusterFailoverBuilder CheckInterval(TimeSpan interval)
        {
            CheckIntervalMs = interval;
            return this;
        }

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(primary))
                throw new ArgumentNullException(nameof(primary), "primary service url shouldn't be null");
            CheckArgument(secondary != null && secondary.Count > 0, "secondary cluster service url shouldn't be null and should set at least one");
            CheckArgument(FailoverDelayNs.TotalSeconds > 0, "failoverDelay should > 0");
            CheckArgument(SwitchBackDelayNs.TotalSeconds > 0, "switchBackDelay should > 0");
            CheckArgument(CheckIntervalMs.TotalSeconds > 0, "checkInterval should > 0");
            var SecondarySize = secondary.Count;

            CheckArgument(SecondaryAuthentications == null || SecondaryAuthentications.Count == SecondarySize, "secondaryAuthentication should be null or size equal with secondary url size");
            CheckArgument(SecondaryTlsTrustCertsFilePaths == null || SecondaryTlsTrustCertsFilePaths.Count == SecondarySize, "secondaryTlsTrustCertsFilePath should be null or size equal with secondary url size");
            CheckArgument(SecondaryTlsTrustStorePaths == null || SecondaryTlsTrustStorePaths.Count == SecondarySize, "secondaryTlsTrustStorePath should be null or size equal with secondary url size");
            CheckArgument(SecondaryTlsTrustStorePasswords == null || SecondaryTlsTrustStorePasswords.Count == SecondarySize, "secondaryTlsTrustStorePassword should be null or size equal with secondary url size");

        }

        public static void CheckArgument(bool expression, string errorMessage)
        {
            if (!expression)
            {
                throw new ArgumentException(errorMessage.ToString());
            }
        }
    }

}