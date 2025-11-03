
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
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using SharpPulsar.Protocol.Schema;

namespace SharpPulsar.Common.Protocol.Proto
{
    /// <summary>
    /// Utility class for managing Netty LenghtFieldBasedFrameDecoder instances in a Netty ChannelPipeline
    /// for the Pulsar binary protocol.
    /// </summary>
    public class FrameDecoderUtil
    {
        public const string FRAME_DECODER_HANDLER = "frameDecoder";

        /// <summary>
        /// Adds a LengthFieldBasedFrameDecoder to the given ChannelPipeline.
        /// </summary>
        /// <param name="pipeline"> the ChannelPipeline to which the decoder will be added </param>
        /// <param name="maxMessageSize"> the maximum size of messages that can be decoded </param>
        public static void AddFrameDecoder(IChannelPipeline pipeline, int maxMessageSize)
        {
            pipeline.AddLast(FRAME_DECODER_HANDLER, CreateFrameDecoder(maxMessageSize));
        }

        /// <summary>
        /// Replaces the existing LengthFieldBasedFrameDecoder in the given ChannelPipeline with a new one.
        /// </summary>
        /// <param name="pipeline"> the ChannelPipeline in which the decoder will be replaced </param>
        /// <param name="maxMessageSize"> the maximum size of messages that can be decoded </param>
        public static void ReplaceFrameDecoder(IChannelPipeline pipeline, int maxMessageSize)
        {
            pipeline.Replace(FRAME_DECODER_HANDLER, FRAME_DECODER_HANDLER, CreateFrameDecoder(maxMessageSize));
        }

        /// <summary>
        /// Removes the LengthFieldBasedFrameDecoder from the given ChannelPipeline.
        /// This is useful in the Pulsar Proxy to remove the decoder before direct proxying of messages without decoding.
        /// </summary>
        /// <param name="pipeline"> the ChannelPipeline from which the decoder will be removed </param>
        public static void RemoveFrameDecoder(IChannelPipeline pipeline)
        {
            pipeline.Remove(FRAME_DECODER_HANDLER);
        }

        private static LengthFieldBasedFrameDecoder CreateFrameDecoder(int maxMessageSize)
        {
            return new LengthFieldBasedFrameDecoder(maxMessageSize + Commands.MessageSizeFramePadding, 0, 4, 0, 4);
        }
    }
}
