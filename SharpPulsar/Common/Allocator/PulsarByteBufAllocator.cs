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
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;


	using UtilityClass = lombok.experimental.UtilityClass;
	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using ByteBufAllocatorBuilder = org.apache.bookkeeper.common.allocator.ByteBufAllocatorBuilder;
	using LeakDetectionPolicy = org.apache.bookkeeper.common.allocator.LeakDetectionPolicy;
	using PoolingPolicy = org.apache.bookkeeper.common.allocator.PoolingPolicy;

	/// <summary>
	/// Holder of a ByteBuf allocator.
	/// </summary>
	public class PulsarByteBufAllocator
	{

		public const string PULSAR_ALLOCATOR_POOLED = "pulsar.allocator.pooled";
		public const string PULSAR_ALLOCATOR_EXIT_ON_OOM = "pulsar.allocator.exit_on_oom";
		public const string PULSAR_ALLOCATOR_LEAK_DETECTION = "pulsar.allocator.leak_detection";

		public static readonly ByteBufAllocator DEFAULT;

		private static readonly IList<System.Action<System.OutOfMemoryException>> LISTENERS = new CopyOnWriteArrayList<System.Action<System.OutOfMemoryException>>();

		public static void RegisterOOMListener(System.Action<System.OutOfMemoryException> listener)
		{
			LISTENERS.Add(listener);
		}

		private static readonly bool EXIT_ON_OOM;

		static PulsarByteBufAllocator()
		{
			bool isPooled = "true".Equals(System.getProperty(PULSAR_ALLOCATOR_POOLED, "true"), StringComparison.OrdinalIgnoreCase);
			EXIT_ON_OOM = "true".Equals(System.getProperty(PULSAR_ALLOCATOR_EXIT_ON_OOM, "false"), StringComparison.OrdinalIgnoreCase);

			LeakDetectionPolicy leakDetectionPolicy = LeakDetectionPolicy.valueOf(System.getProperty(PULSAR_ALLOCATOR_LEAK_DETECTION, "Disabled"));

			if (log.DebugEnabled)
			{
				log.debug("Is Pooled: {} -- Exit on OOM: {}", isPooled, EXIT_ON_OOM);
			}

			ByteBufAllocatorBuilder builder = ByteBufAllocatorBuilder.create().leakDetectionPolicy(leakDetectionPolicy).pooledAllocator(PooledByteBufAllocator.DEFAULT).outOfMemoryListener(oomException =>
			{
			LISTENERS.ForEach(c =>
			{
				try
				{
					c.accept(oomException);
				}
				catch (Exception t)
				{
					log.warn("Exception during OOM listener: {}", t.Message, t);
				}
			});
			if (EXIT_ON_OOM)
			{
				log.info("Exiting JVM process for OOM error: {}", oomException.Message, oomException);
				Runtime.Runtime.halt(1);
			}
			});

			if (isPooled)
			{
				builder.poolingPolicy(PoolingPolicy.PooledDirect);
			}
			else
			{
				builder.poolingPolicy(PoolingPolicy.UnpooledHeap);
			}

			DEFAULT = builder.build();
		}
	}

}