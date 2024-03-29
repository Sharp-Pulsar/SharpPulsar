﻿/// <summary>
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


using SharpPulsar.Interfaces;

namespace SharpPulsar.Utils
{
	public class MessageIdUtils
	{
		public static long GetOffset(IMessageId messageId)
		{
			var msgId = (MessageId) messageId;
			var ledgerId = msgId.LedgerId;
			var entryId = msgId.EntryId;

			// Combine ledger id and entry id to form offset
			// Use less than 32 bits to represent entry id since it will get
			// rolled over way before overflowing the max int range
			var offset = (ledgerId << 28) | entryId;
			return offset;
		}

		public static IMessageId GetMessageId(long offset)
		{
			// Demultiplex ledgerId and entryId from offset
			var ledgerId = (long)((ulong)offset >> 28);
            //var it = 0x0F_FF_FF_FFL;

            var entryId = offset & 0x0F_FF_FF_FFL;

			return new MessageId(ledgerId, entryId, -1);
		}
	}

}