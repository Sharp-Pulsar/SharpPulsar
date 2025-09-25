
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
    /// Functional interface for customizing a producer builder for a specific topic.
    /// 
    /// <para>This interface allows for customizing the producer builder configuration for either the retry letter topic
    /// or the dead letter topic. The customization might include setting producer properties such as batching, timeouts,
    /// or any other producer-specific configuration.
    /// 
    /// </para>
    /// </summary>
    /// <seealso cref="DeadLetterProducerBuilderContext"/>
    public interface IDeadLetterProducerBuilderCustomizer
    {
        /// <summary>
        /// Customize the given producer builder with settings specific to the topic context provided.
        /// </summary>
        /// <param name="context">         the context containing information about the input topic and the subscription </param>
        /// <param name="producerBuilder"> the producer builder instance to be customized </param>
        void Customize(IDeadLetterProducerBuilderContext context, IProducerBuilder<byte[]> producerBuilder);
    }
}
