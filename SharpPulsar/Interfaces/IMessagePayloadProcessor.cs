// / <summary>
// / Licensed to the Apache Software Foundation (ASF) under one
// / or more contributor license agreements.  See the NOTICE file
// / distributed with this work for additional information
// / regarding copyright ownership.  The ASF licenses this file
// / to you under the Apache License, Version 2.0 (the
// / "License"); you may not use this file except in compliance
// / with the License.  You may obtain a copy of the License at
// / 
// /   http://www.apache.org/licenses/LICENSE-2.0
// / 
// / Unless required by applicable law or agreed to in writing,
// / software distributed under the License is distributed on an
// / "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// / KIND, either express or implied.  See the License for the
// / specific language governing permissions and limitations
// / under the License.
// / </summary>

using System;

namespace SharpPulsar.Interfaces
{

    // / <summary>
    // / The processor to process a message payload.
    // / 
    // / It's responsible to convert the raw buffer to some messages, then trigger some callbacks so that consumer can consume
    // / these messages and handle the exception if it existed.
    // / 
    // / The most important part is to decode the raw buffer. After that, we can call
    // / <seealso cref="MessagePayloadContext.getMessageAt"/> or <seealso cref="MessagePayloadContext.asSingleMessage"/> to construct
    // / <seealso cref="Message"/> for consumer to consume. Since we need to pass the <seealso cref="MessagePayload"/> object to these methods, we
    // / can use <seealso cref="MessagePayloadFactory.DEFAULT"/> to create it or just reuse the payload argument.
    // / </summary>
    public interface IMessagePayloadProcessor
    {

        // / <summary>
        // / Process the message payload.

        /// The default processor for Pulsar format payload. It should be noted getNumMessages() and isBatch() methods of
        /// EntryContext only work for Pulsar format. For other formats, the message metadata might be stored in the payload.
        // / </summary>
        // / <param name="payload"> the payload whose underlying buffer is a Netty ByteBuf </param>
        // / <param name="context"> the message context that contains the message format information and methods to create a message </param>
        // / <param name="schema"> the message's schema </param>
        // / <param name="messageConsumer"> the callback to consume each message </param>
        // / @param <T> </param>
        // / <exception cref="Exception"> </exception>
        public static void Process<T>(MessagePayload payload, MessagePayloadContext context, ISchema<T> schema, Action<IMessage<T>> messageConsumer)
        {
            if (context.Batch)
            {
                var numMessages = context.NumMessages;
                for (var i = 0; i < numMessages; i++)
                {
                    messageConsumer.Invoke(context.GetMessageAt(i, numMessages, payload, true, schema));
                }
            }
            else
            {
                messageConsumer.Invoke(context.AsSingleMessage(payload, schema));
            }
        }

    }
}