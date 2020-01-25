using System;
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
namespace SharpPulsar.Common.Allocator
{
	using ByteBufAllocator = DotNetty.Buffers.AbstractByteBufferAllocator;
	using PooledByteBufAllocator = DotNetty.Buffers.PooledByteBufferAllocator;


	/// <summary>
	/// Holder of a ByteBuf allocator.
	/// </summary>
	public class PulsarByteBufAllocator
	{

		public const string PulsarAllocatorPooled = "pulsar.allocator.pooled";
		public const string PulsarAllocatorExitOnOom = "pulsar.allocator.exit_on_oom";
		public const string PulsarAllocatorLeakDetection = "pulsar.allocator.leak_detection";

		public static readonly ByteBufAllocator DEFAULT;


		public static void RegisterOOMListener(System.Action<System.OutOfMemoryException> Listener)
		{
			LISTENERS.Add(Listener);
		}

		private static readonly bool EXIT_ON_OOM;

		static PulsarByteBufAllocator()
		{
			bool IsPooled = "true".Equals(System.getProperty(PulsarAllocatorPooled, "true"), StringComparison.OrdinalIgnoreCase);
			EXIT_ON_OOM = "true".Equals(System.getProperty(PulsarAllocatorExitOnOom, "false"), StringComparison.OrdinalIgnoreCase);

			LeakDetectionPolicy LeakDetectionPolicy = LeakDetectionPolicy.valueOf(System.getProperty(PulsarAllocatorLeakDetection, "Disabled"));

			if (log.DebugEnabled)
			{
				log.debug("Is Pooled: {} -- Exit on OOM: {}", IsPooled, EXIT_ON_OOM);
			}

			ByteBufAllocatorBuilder Builder = ByteBufAllocatorBuilder.create().leakDetectionPolicy(LeakDetectionPolicy).pooledAllocator(PooledByteBufAllocator.DEFAULT).outOfMemoryListener(oomException =>
			{
			LISTENERS.ForEach(c =>
			{
				try
				{
					c.accept(oomException);
				}
				catch (Exception T)
				{
					log.warn("Exception during OOM listener: {}", T.Message, T);
				}
			});
			if (EXIT_ON_OOM)
			{
				log.info("Exiting JVM process for OOM error: {}", oomException.Message, oomException);
				Runtime.Runtime.halt(1);
			}
			});

			if (IsPooled)
			{
				Builder.poolingPolicy(PoolingPolicy.PooledDirect);
			}
			else
			{
				Builder.poolingPolicy(PoolingPolicy.UnpooledHeap);
			}

			DEFAULT = Builder.build();
		}
	}

}