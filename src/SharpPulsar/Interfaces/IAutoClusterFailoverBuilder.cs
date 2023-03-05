using System;
using System.Collections.Generic;
using SharpPulsar.Common;

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
namespace SharpPulsar.Interfaces
{

    /// <summary>
    /// <seealso cref="IAutoClusterFailoverBuilder"/> is used to configure and create instance of <seealso cref="IServiceUrlProvider"/>.
    /// 
    /// @since 2.10.0
    /// </summary>
    public interface IAutoClusterFailoverBuilder
    {
        /// <summary>
        /// Set the primary service url.
        /// </summary>
        /// <param name="primary">
        /// @return </param>
        IAutoClusterFailoverBuilder Primary(string primary);

        /// <summary>
        /// Set the secondary service url.
        /// </summary>
        /// <param name="secondary">
        /// @return </param>
        IAutoClusterFailoverBuilder Secondary(IList<string> secondary);

        /// <summary>
        /// Set secondary choose policy. The default secondary choose policy is `ORDER`. </summary>
        /// <param name="policy">
        /// @return </param>
        IAutoClusterFailoverBuilder FailoverPolicy(FailoverPolicy policy);

        /// <summary>
        /// Set secondary authentication.
        /// </summary>
        /// <param name="authentication">
        /// @return </param>
        IAutoClusterFailoverBuilder SecondaryAuthentication(IDictionary<string, IAuthentication> authentication);

        /// <summary>
        /// Set secondary tlsTrustCertsFilePath.
        /// </summary>
        /// <param name="tlsTrustCertsFilePath">
        /// @return </param>
        IAutoClusterFailoverBuilder SecondaryTlsTrustCertsFilePath(IDictionary<string, string> tlsTrustCertsFilePath);

        /// <summary>
        /// Set secondary tlsTrustStorePath.
        /// </summary>
        /// <param name="tlsTrustStorePath">
        /// @return </param>
        IAutoClusterFailoverBuilder SecondaryTlsTrustStorePath(IDictionary<string, string> tlsTrustStorePath);

        /// <summary>
        /// Set secondary tlsTrustStorePassword.
        /// </summary>
        /// <param name="tlsTrustStorePassword">
        /// @return </param>
        IAutoClusterFailoverBuilder SecondaryTlsTrustStorePassword(IDictionary<string, string> tlsTrustStorePassword);
        /// <summary>
        /// Set the switch failoverDelay. When one cluster failed longer than failoverDelay, it will trigger cluster switch.
        /// </summary>
        /// <param name="failoverDelay"> </param>
        /// @return </param>
        IAutoClusterFailoverBuilder FailoverDelay(TimeSpan failoverDelay);

        /// <summary>
        /// Set the switchBackDelay. When switched to the secondary cluster, and after the primary cluster comes back,
        /// it will wait for switchBackDelay to switch back to the primary cluster.
        /// </summary>
        /// <param name="switchBackDelay"> </param>
        /// @return </param>
        IAutoClusterFailoverBuilder SwitchBackDelay(TimeSpan switchBackDelay);

        /// <summary>
        /// Set the checkInterval for probe.
        /// </summary>
        /// <param name="interval"> </param>
        /// @return </param>
        IAutoClusterFailoverBuilder CheckInterval(TimeSpan interval);

        void Validate();
    }

    public enum FailoverPolicy
    {
        ORDER
    }

}