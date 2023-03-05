using System;
using System.Collections.Generic;
using Akka.Actor;
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

namespace SharpPulsar
{
    /// <summary>
    /// ReaderInterceptorUtil is used to wrap ReaderInterceptor by ConsumerInterceptor.
    /// </summary>
    public class ReaderInterceptorUtil
    {
        public static List<IConsumerInterceptor<T>> ConvertToConsumerInterceptors<T>(IActorRef reader, IList<IReaderInterceptor<T>> interceptorList)
        {
            if (interceptorList == null || interceptorList.Count == 0)
            {
                return null;
            }
            var consumerInterceptorList = new List<IConsumerInterceptor<T>>(interceptorList.Count);
            foreach (var readerInterceptor in interceptorList)
            {
                consumerInterceptorList.Add(GetInterceptor(reader, readerInterceptor));
            }
            return consumerInterceptorList;
        }

        private static IConsumerInterceptor<T> GetInterceptor<T>(IActorRef reader, IReaderInterceptor<T> readerInterceptor)
        {
            return new ConsumerInterceptorAnonymousInnerClass<T>(reader, readerInterceptor);
        }

        private class ConsumerInterceptorAnonymousInnerClass<T> : IConsumerInterceptor<T>
        {
            private IActorRef _reader;
            private IReaderInterceptor<T> _readerInterceptor;

            public ConsumerInterceptorAnonymousInnerClass(IActorRef reader, IReaderInterceptor<T> readerInterceptor)
            {
                _reader = reader;
                _readerInterceptor = readerInterceptor;
            }

            public void Close()
            {
                _readerInterceptor.Close();
            }

            public  IMessage<T> BeforeConsume(IActorRef consumer, IMessage<T> message)
            {
                return _readerInterceptor.BeforeRead(_reader, message);
            }

            public void OnAcknowledge(IActorRef consumer, IMessageId messageId, Exception exception)
            {
                // nothing to do
            }

            public void OnAcknowledgeCumulative(IActorRef consumer, IMessageId messageId, Exception exception)
            {
                // nothing to do
            }

            public void OnNegativeAcksSend(IActorRef consumer, ISet<IMessageId> set)
            {
                // nothing to do
            }

            public void OnAckTimeoutSend(IActorRef consumer, ISet<IMessageId> set)
            {
                // nothing to do
            }

            public void OnPartitionsChange(string topicName, int partitions)
            {
                _readerInterceptor.OnPartitionsChange(topicName, partitions);
            }

            public void Dispose()
            {
                
            }
        }

    }

}
