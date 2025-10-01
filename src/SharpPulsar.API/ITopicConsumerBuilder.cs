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
    /// <seealso cref="TopicConsumerBuilder"/> is used to configure topic specific options to override those set at the
    /// <seealso cref="ConsumerBuilder"/> level.
    /// </summary>
    /// <seealso cref="ConsumerBuilder.topicConfiguration(String)"
    ////>
    /// @param <T> the type of the value in the <seealso cref="ConsumerBuilder"/> </param>
    public interface ITopicConsumerBuilder<T>
    {
        /// <summary>
        /// Configure the priority level of this topic.
        /// </summary>
        /// <seealso cref="ConsumerBuilder.priorityLevel(int)"
        ////>
        /// <param name="priorityLevel"> the priority of this topic </param>
        /// <returns> the <seealso cref="TopicConsumerBuilder"/> instance </returns>
        ITopicConsumerBuilder<T> PriorityLevel(int priorityLevel);

        /// <summary>
        /// Complete the configuration of the topic specific options and return control back to the
        /// <seealso cref="ConsumerBuilder"/> instance.
        /// </summary>
        /// <returns> the <seealso cref="ConsumerBuilder"/> instance </returns>
        IConsumerBuilder<T> Build();
    }
}
