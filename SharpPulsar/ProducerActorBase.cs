using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Queues;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
namespace SharpPulsar
{
	internal abstract class ProducerActorBase<T> : ReceiveActor
	{
		internal abstract ValueTask InternalSendWithTxn(IMessage<T> message, IActorRef txn);
		internal abstract ValueTask InternalSend(IMessage<T> message);
		protected internal abstract ValueTask<long> LastDisconnectedTimestamp();
		protected internal abstract ValueTask<bool> Connected();
		protected internal abstract ValueTask<IProducerStats> Stats();
		protected internal abstract ValueTask<long> LastSequenceId();
		protected internal abstract ValueTask<string> ProducerName();

		protected internal readonly ProducerConfigurationData Conf;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ProducerInterceptors<T> Interceptors;
		protected internal readonly Dictionary<SchemaHash, sbyte[]> SchemaCache;
		protected internal MultiSchemaMode _multiSchemaMode = MultiSchemaMode.Auto;
		protected internal IActorRef Client;
		protected internal readonly ClientConfigurationData ClientConfiguration;
		protected internal readonly ProducerQueueCollection<T> ProducerQueue;
		protected internal HandlerState State;
		private string _topic;

		public ProducerActorBase(IActorRef client, string topic, ProducerConfigurationData conf, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData configurationData, ProducerQueueCollection<T> queue)
		{			
			ClientConfiguration = configurationData;
			ProducerQueue = queue;
			Client = client;
			_topic = topic;
			Conf = conf;
			Schema = schema;
			Interceptors = interceptors;
			SchemaCache = new Dictionary<SchemaHash, sbyte[]>();
			if(!conf.MultiSchema)
			{
				_multiSchemaMode = MultiSchemaMode.Disabled;
			}
			var pName = ProducerName().GetAwaiter().GetResult();
			State = new HandlerState(client, topic, Context.System, pName);

		}

		protected internal virtual string Topic
		{
			get
			{
				return _topic;
			}
		}

		protected internal virtual ProducerConfigurationData Configuration
		{
			get
			{
				return Conf;
			}
		}


		protected internal virtual IMessage<T> BeforeSend(IMessage<T> message)
		{
			if(Interceptors != null)
			{
				return Interceptors.BeforeSend(Self, message);
			}
			else
			{
				return message;
			}
		}

		protected internal virtual void OnSendAcknowledgement(IMessage<T> message, IMessageId msgId, Exception exception)
		{
			if(Interceptors != null && message != null)
			{
				Interceptors.OnSendAcknowledgement(Self, message, msgId, exception);
			}
		}


		protected internal enum MultiSchemaMode
		{
			Auto,
			Enabled,
			Disabled
		}
	}

}