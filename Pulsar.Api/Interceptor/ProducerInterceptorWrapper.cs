using System;

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
namespace org.apache.pulsar.client.api.interceptor
{
	using org.apache.pulsar.client.api;
	using org.apache.pulsar.client.api;

	/// <summary>
	/// A wrapper for old style producer interceptor.
	/// </summary>
	public class ProducerInterceptorWrapper : ProducerInterceptor
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.client.api.ProducerInterceptor<?> innerInterceptor;
		private readonly org.apache.pulsar.client.api.ProducerInterceptor<object> innerInterceptor;

		public ProducerInterceptorWrapper<T1>(org.apache.pulsar.client.api.ProducerInterceptor<T1> innerInterceptor)
		{
			this.innerInterceptor = innerInterceptor;
		}

		public virtual void close()
		{
			innerInterceptor.close();
		}

		public override bool eligible(Message message)
		{
			return true;
		}

		public virtual Message beforeSend(Producer producer, Message message)
		{
			return innerInterceptor.beforeSend(producer, message);
		}

		public virtual void onSendAcknowledgement(Producer producer, Message message, MessageId msgId, Exception exception)
		{
			innerInterceptor.onSendAcknowledgement(producer, message, msgId, exception);
		}
	}

}