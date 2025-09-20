using System.Threading.Tasks;
using Akka.Actor;
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
namespace SharpPulsar.Common
{
    /// <summary>
    /// The provider to provide the service url.
    /// 
    /// <para>This allows applications to retrieve the service URL from an external configuration provider and,
    /// more importantly, to force the Pulsar client to reconnect if the service URL has been changed.
    /// 
    /// </para>
    /// <para>It can be passed with <seealso cref="IServiceUrlProvider"/>
    /// </para>
    /// </summary>
    public interface IServiceUrlProvider
	{
        /// <summary>
        /// Initialize the service url provider with Pulsar client instance.
        /// 
        /// <para>This can be used by the provider to force the Pulsar client to reconnect whenever the service url might have
        /// changed. See <seealso cref="PulsarClient.UpdateServiceUrl(string)"/>.
        /// 
        /// </para>
        /// </summary>
        /// <param name="client">
        ///            created pulsar client. </param>
        void Initialize(PulsarClient pulsarClient);


        void CreateActor(ActorSystem actorSystem);


        /// <summary>
        /// Get the current service URL the Pulsar client should connect to.
        /// </summary>
        /// <returns> the pulsar service url. </returns>

        string ServiceUrl { get; }

        /// <summary>
        /// Get the current service URL the Pulsar client should connect to.
        /// </summary>
        /// <returns> the pulsar service url. </returns>
        ValueTask<string> ServiceUrlAsync();

    }

}