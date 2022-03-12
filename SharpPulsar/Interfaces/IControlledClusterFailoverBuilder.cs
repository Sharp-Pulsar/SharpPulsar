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
    /// <seealso cref="IControlledClusterFailoverBuilder"/> is used to configure and create instance of <seealso cref="ServiceUrlProvider"/>.
    /// 
    /// @since 2.10.0
    /// </summary>
    public interface IControlledClusterFailoverBuilder
    {
        /// <summary>
        /// Set default service url.
        /// </summary>
        /// <param name="serviceUrl">
        /// @return </param>
        IControlledClusterFailoverBuilder DefaultServiceUrl(string serviceUrl);

        /// <summary>
        /// Set the service url provider. ServiceUrlProvider will fetch serviceUrl from urlProvider periodically.
        /// </summary>
        /// <param name="urlProvider">
        /// @return </param>
        IControlledClusterFailoverBuilder UrlProvider(string urlProvider);

        /// <summary>
        /// Set the service url provider header to authenticate provider service. </summary>
        /// <param name="header">
        /// @return </param>
        IControlledClusterFailoverBuilder UrlProviderHeader(IDictionary<string, string> header);

        /// <summary>
        /// Set the probe check interval. </summary>
        /// <param name="interval"> </param>
        /// <param name="timeSpan">
        /// @return </param>
        IControlledClusterFailoverBuilder CheckInterval(long interval, TimeSpan timeSpan);

        /// <summary>
        /// Build the ServiceUrlProvider instance.
        /// 
        /// @return </summary>
        /// <exception cref="Exception"> </exception>
        IServiceUrlProvider Build();
    }

}