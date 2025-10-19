
using System;
using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Utilities;

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
namespace SharpPulsar.Common.Util
{
    /// <summary>
    /// ByteBuf holder that contains 2 buffers.
    /// </summary>
    public sealed class ByteBufPair : AbstractReferenceCounted
    {

        private AbstractByteBuffer b1;
        private AbstractByteBuffer b2;
        /// <summary>
        /// Get a new <seealso cref="ByteBufPair"/> from the pool and assign 2 buffers to it.
        /// 
        /// <para>The buffers b1 and b2 lifecycles are now managed by the ByteBufPair:
        /// when the <seealso cref="ByteBufPair"/> is deallocated, b1 and b2 will be released as well.
        /// 
        /// </para>
        /// </summary>
        /// <param name="b1"> </param>
        /// <param name="b2">
        /// @return </param>
        public static ByteBufPair Get(AbstractByteBuffer b1, AbstractByteBuffer b2)
        {
            ByteBufPair buf = new ByteBufPair();
            //buf.SetRefCnt(1);
            buf.b1 = b1;
            buf.b2 = b2;
            return buf;
        }

        public AbstractByteBuffer First
        {
            get
            {
                return b1;
            }
        }

        public AbstractByteBuffer Second
        {
            get
            {
                return b2;
            }
        }

        public int ReadableBytes()
        {
            return b1.ReadableBytes + b2.ReadableBytes;
        }

        /// <summary>
        /// Combines the content of both buffers into a single <seealso cref="ByteBuf"/>.
        /// 
        /// <para>This method creates a new <seealso cref="ByteBuf"/> with the combined readable content
        /// of the two buffers in the given <seealso cref="ByteBufPair"/>. The original buffer is
        /// released after the data is written into the new buffer.
        /// 
        /// </para>
        /// </summary>
        /// <param name="pair"> the <seealso cref="ByteBufPair"/> containing the two buffers to be coalesced </param>
        /// <returns> a new <seealso cref="ByteBuf"/> containing the combined content of both buffers </returns>
        public static AbstractByteBuffer Coalesce(ByteBufPair pair)
        {
            AbstractByteBuffer b = (AbstractByteBuffer)Unpooled.Buffer(pair.ReadableBytes());
            b.WriteBytes(pair.b1, pair.b1.ReaderIndex, pair.b1.ReadableBytes);
            b.WriteBytes(pair.b2, pair.b2.ReaderIndex, pair.b2.ReadableBytes);
            pair.Release();
            return b;
        }

        protected override void Deallocate()
        {
            b1.Release();
            b2.Release();
            b1 = b2 = null;
        }

        public override IReferenceCounted Touch(object hint)
        {
            b1.Touch(hint);
            b2.Touch(hint);
            return this;
        }

        
    }
}
