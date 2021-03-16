using System.Collections.Generic;

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
	internal class ChunkedMessageCtx
	{

		protected internal int TotalChunks = -1;
		protected internal List<byte> ChunkedMsgBuffer;
		protected internal int LastChunkedMessageId = -1;
		protected internal MessageId[] ChunkedMessageIds;
		protected internal long ReceivedTime = 0;

		internal static ChunkedMessageCtx Get(int numChunksFromMsg, List<byte> chunkedMsgBuffer)
		{
			ChunkedMessageCtx ctx = new ChunkedMessageCtx
			{
				TotalChunks = numChunksFromMsg,
				ChunkedMsgBuffer = chunkedMsgBuffer,
				ChunkedMessageIds = new MessageId[numChunksFromMsg],
				ReceivedTime = DateTimeHelper.CurrentUnixTimeMillis()
			};
			return ctx;
		}

		internal virtual void Recycle()
		{
			TotalChunks = -1;
			ChunkedMsgBuffer = null;
			LastChunkedMessageId = -1;
		}
	}
}