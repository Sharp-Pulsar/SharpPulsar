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


namespace SharpPulsar.Batch.Api
{
    /// <summary>
	/// Batcher builder.
	/// </summary>
	public interface IBatcherBuilder
	{

		/// <summary>
		/// Default batch message container.
		/// 
		/// <para>incoming single messages:
		/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
		/// 
		/// </para>
		/// <para>batched into single batch message:
		/// [(k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)]
		/// </para>
		/// </summary>

		/// <summary>
		/// Key based batch message container.
		/// 
		/// <para>incoming single messages:
		/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
		/// 
		/// </para>
		/// <para>batched into multiple batch messages:
		/// [(k1, v1), (k1, v2), (k1, v3)], [(k2, v1), (k2, v2), (k2, v3)], [(k3, v1), (k3, v2), (k3, v3)]
		/// </para>
		/// </summary>

		/// <summary>
		/// Build a new batch message container. </summary>
		/// <returns> new batch message container </returns>
		IBatchMessageContainer Build<T>();

		public static IBatcherBuilder Default(ILoggingAdapter log) => DefaultImplementation.NewDefaultBatcherBuilder(log);
		public static IBatcherBuilder KeyBased(ILoggingAdapter log) => DefaultImplementation.NewKeyBasedBatcherBuilder(log);

	}

}