using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Schema;
using System;
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
		internal abstract ValueTask InternalSend(IMessage<T> message, TaskCompletionSource<Message<T>> future);
		protected internal abstract long LastDisconnectedTimestamp();
		protected internal abstract bool Connected();
		protected internal abstract ValueTask<IProducerStats> Stats();
		protected internal abstract ValueTask<long> LastSequenceId();
		protected internal abstract ValueTask<string> ProducerName();

		protected internal readonly ProducerConfigurationData Conf;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ProducerInterceptors<T> Interceptors;
		protected internal readonly Dictionary<SchemaHash, byte[]> SchemaCache;
		protected internal MultiSchemaMode _multiSchemaMode = MultiSchemaMode.Auto;
		protected internal IActorRef Client;
		protected internal readonly ClientConfigurationData ClientConfiguration;
		protected internal HandlerState State;
		private readonly string _topic;
        protected internal TaskCompletionSource<IActorRef> ProducerCreatedFuture;

        protected ProducerActorBase(IActorRef client, IActorRef lookup, IActorRef cnxPool, string topic, ProducerConfigurationData conf, TaskCompletionSource<IActorRef> producerCreatedFuture, ISchema<T> schema, ProducerInterceptors<T> interceptors, ClientConfigurationData configurationData)
		{
            ProducerCreatedFuture = producerCreatedFuture;
			ClientConfiguration = configurationData;
			Client = client;
			_topic = topic;

            if (conf.BatchingEnabled && conf.AckReceivedListerner == null)
            {
                conf.AckReceivedListerner = (acked) =>
                {
                    Context.System.Log.Info($"AckReceived(ledger-id:{acked.LedgerId}, entery-id:{acked.EntryId}, sequence-id:{acked.SequenceId}, highest-sequence-id:{acked.HighestSequenceId})");
                };
            }
            Conf = conf;
			Schema = schema;
			Interceptors = interceptors;
			SchemaCache = new Dictionary<SchemaHash, byte[]>();
			if(!conf.MultiSchema)
			{
				_multiSchemaMode = MultiSchemaMode.Disabled;
			}
			var pName = ProducerName().GetAwaiter().GetResult();
			State = new HandlerState(lookup, cnxPool, topic, Context.System, pName);

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
        protected internal virtual void OnPartitionsChange(string topicName, int partitions)
        {
            if (Interceptors != null)
            {
                Interceptors.OnPartitionsChange(topicName, partitions);
            }
        }
        public override string ToString()
        {
            return "ProducerBase{" + "topic='" + Topic + '\'' + '}';
        }
        protected internal enum MultiSchemaMode
		{
			Auto,
			Enabled,
			Disabled
		}
	}

}