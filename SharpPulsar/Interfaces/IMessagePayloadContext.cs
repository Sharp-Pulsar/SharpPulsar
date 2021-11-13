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

namespace SharpPulsar.Interfaces
{
    // / <summary>
    // / The context of the message payload, which usually represents a batched message (batch) or a single message.
    // / </summary>
    public interface IMessagePayloadContext<T>
    {

        // / <summary>
        // / Get a value associated with the given key.
        // / 
        // / When the message payload is not produced by Pulsar producer, a specific property is usually added to indicate the
        // / format. So this method is useful to determine whether the payload is produced by Pulsar producer.
        // / </summary>
        // / <param name="key"> </param>
        // / <returns> the value associated with the key or null if the key or value doesn't exist </returns>
        string GetProperty(string key);

        // / <summary>
        // / Get the number of messages when the payload is produced by Pulsar producer.
        // / </summary>
        // / <returns> the number of messages </returns>
        int NumMessages { get; }

        // / <summary>
        // / Check whether the payload is a batch when the payload is produced by Pulsar producer.
        // / </summary>
        // / <returns> true if the payload is a batch </returns>
        bool Batch { get; }

        // / <summary>
        // / Get the internal single message with a specific index from a payload if the payload is a batch.
        // / </summary>
        // / <param name="index"> the batch index </param>
        // / <param name="numMessages"> the number of messages in the batch </param>
        // / <param name="payload"> the message payload </param>
        // / <param name="containMetadata"> whether the payload contains the single message metadata </param>
        // / <param name="schema"> the schema of the batch </param>
        // / @param <T> </param>
        // / <returns> the created message
        // / @implNote The `index` and `numMessages` parameters are used to create the message id with batch index.
        // /   If `containMetadata` is true, parse the single message metadata from the payload first. The fields of single
        // /   message metadata will overwrite the same fields of the entry's metadata. </returns>
        IMessage<T> GetMessageAt(int index, int numMessages, IMessagePayload payload, bool containMetadata, ISchema<T> schema);

        // / <summary>
        // / Convert the given payload to a single message if the entry is not a batch.
        // / </summary>
        // / <param name="payload"> the message payload </param>
        // / <param name="schema"> the schema of the message </param>
        // / @param <T> </param>
        // / <returns> the created message </returns>
        IMessage<T> AsSingleMessage(IMessagePayload payload, ISchema<T> schema);
    }

}