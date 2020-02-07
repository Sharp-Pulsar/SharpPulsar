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
namespace SharpPulsar.Api.Interceptor
{
	using SharpPulsar.Api;

	/// <summary>
	/// A wrapper for old style producer interceptor.
	/// </summary>
	public class ProducerInterceptorWrapper<T> : IProducerInterceptor
	{
		private readonly IProducerInterceptor _innerInterceptor;

		public ProducerInterceptorWrapper(IProducerInterceptor innerInterceptor)
		{
			this._innerInterceptor = innerInterceptor;
		}

		public void Dispose()
		{
			_innerInterceptor.Close();
		}
		public void Close()
		{
			_innerInterceptor.Close();
		}

		public bool Eligible<T>(Message<T> message)
		{
			return true;
		}

		public Message<T> BeforeSend<T>(IProducer<T> producer, Message<T> message)
		{
			return _innerInterceptor.BeforeSend(producer, message);
		}

		public void OnSendAcknowledgement<T>(IProducer<T> producer, Message<T> message, IMessageId msgId, System.Exception exception)
		{
			_innerInterceptor.OnSendAcknowledgement(producer, message, msgId, exception);
		}
	}

}