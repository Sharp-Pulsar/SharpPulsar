using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.API;
using SharpPulsar.Common.Precondition;
using SharpPulsar.Configuration;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Shared;
using SharpPulsar.Shared.Exceptions;
using SharpPulsar.Table;
using SharpPulsar.Table.Messages;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Builder
{

    public class TableViewBuilder<T> : ITableViewBuilder<T>
    {

        private readonly PulsarClient _client;
        private readonly ISchema<T> _schema;
        private TableViewConfigurationData _conf;

        internal TableViewBuilder(PulsarClient client, ISchema<T> schema)
        {
            _client = client;
            _schema = schema;
            _conf = new TableViewConfigurationData();
        }

        public virtual ITableViewBuilder<T> LoadConf(IDictionary<string, object> config)
        {
            _conf = (TableViewConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;
        }

        public virtual ITableView<T> Create()
        {
            try
            {
                return CreateAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw PulsarClientException.Unwrap(e);
            }
        }

        public virtual async ValueTask<ITableView<T>> CreateAsync()
        {
            var actor = _client.ActorSystem.ActorOf(TableViewActor<T>.Prop(_client, _schema, _conf));
            // await Task.Delay(TimeSpan.FromSeconds(5));
            var response = await actor.Ask<AskResponse>(StartMessage.Instance);
            if (response.Failed)
            {
                await actor.GracefulStop(TimeSpan.FromSeconds(1));
                throw response.Exception;
            }
            return new TableView<T>(actor);
        }

        public virtual ITableViewBuilder<T> Topic(string topic)
        {
            Condition.CheckArgument(string.IsNullOrWhiteSpace(topic), "topic cannot be blank");
            _conf.TopicName = topic.Trim();
            return this;
        }

        public virtual ITableViewBuilder<T> AutoUpdatePartitionsInterval(int interval, TimeUnit.TimeUnit unit)
        {
            Condition.CheckArgument(unit.ToSeconds(interval) >= 1, "minimum is 1 second");
            _conf.AutoUpdatePartitionsSeconds = unit.ToSeconds(interval);
            return this;
        }

        public virtual ITableViewBuilder<T> SubscriptionName(string subscriptionName)
        {
            Condition.CheckArgument(string.IsNullOrWhiteSpace(subscriptionName), "subscription name cannot be blank");
            _conf.SubscriptionName = subscriptionName.Trim();
            return this;
        }

        public virtual ITableViewBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
        {
            _conf.CryptoKeyReader = cryptoKeyReader;
            return this;
        }

        public virtual ITableViewBuilder<T> DefaultCryptoKeyReader(string privateKey)
        {
            Condition.CheckArgument(string.IsNullOrWhiteSpace(privateKey), "privateKey cannot be blank");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().DefaultPrivateKey(privateKey).Build());
        }

        public virtual ITableViewBuilder<T> DefaultCryptoKeyReader(IDictionary<string, string> privateKeys)
        {
            Condition.CheckArgument(privateKeys.Count > 0, "privateKeys cannot be empty");
            return CryptoKeyReader(Crypto.DefaultCryptoKeyReader.Builder().PrivateKeys(privateKeys).Build());
        }

        public virtual ITableViewBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
        {
            _conf.CryptoFailureAction = action;
            return this;
        }

    }
}
