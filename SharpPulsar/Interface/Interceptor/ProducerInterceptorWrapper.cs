using System;
using System.Threading.Tasks;

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
namespace Pulsar.Api.Interceptor
{
	/// <summary>
	/// A wrapper for old style producer interceptor.
	/// </summary>
	public class ProducerInterceptorWrapper : IProducerInterceptor
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.client.api.ProducerInterceptor<?> innerInterceptor;
		private readonly IProducerInterceptor<T> _innerInterceptor;

		public ProducerInterceptorWrapper(IProducerInterceptor innerInterceptor)
		{
			_innerInterceptor = innerInterceptor;
		}


		public bool Eligible(IMessage message)
		{
			return true;
		}

		public IMessage BeforeSend(IProducer producer, IMessage message)
		{
			return _innerInterceptor.BeforeSend(producer, message);
		}

		public void OnSendAcknowledgement(IProducer producer, IMessage message, IMessageId msgId, Exception exception)
		{
			_innerInterceptor.OnSendAcknowledgement(producer, message, msgId, exception);
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}
	}

}