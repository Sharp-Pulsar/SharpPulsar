
using System.Threading;

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
namespace SharpPulsar.API
{
    /// <summary>
    /// Interface for providing service to execute message listeners.
    /// </summary>
    public interface IMessageListenerExecutor
    {

        /// <summary>
        /// select a thread by message to execute the runnable!
        /// <para>
        /// Suggestions:
        /// </para>
        /// <para>
        /// 1. The message listener task will be submitted to this executor for execution,
        /// so the implementations of this interface should carefully consider execution
        /// order if sequential consumption is required.
        /// </para>
        /// <para>
        /// 2. The users should release resources(e.g. threads) of the executor after closing
        /// the consumer to avoid leaks.
        /// </para> 
        /// 
        /// </summary>
        /// <example>
        /// Action action = () => 
        /// {
        /// };
        /// Thread thread = new Thread(()
        /// {
        /// });
        /// thread.Start();
        /// </example>
        /// <param name="message">  the message </param>
        /// <param name="runnable"> the runnable to execute, that is, the message listener task </param>
        void Execute<T1>(IMessage<T1> message, ThreadStart runnable);
    }
}
