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

namespace SharpPulsar.Interfaces
{
    /// <summary>
    /// <seealso cref="ITableViewBuilder{T}"/> is used to configure and create instances of <seealso cref="ITableView{T}"/>.
    /// </summary>
    /// 
    /// @since 2.10.0/>
    public interface ITableViewBuilder<T>
    {

        /// <summary>
        /// Load the configuration from provided <tt>config</tt> map.
        /// 
        ///  <para>Example:
        /// 
        ///  <pre>{@code
        ///  Map<String, Object> config = new HashMap<>();
        ///  config.put("topicName", "test-topic");
        ///  config.put("autoUpdatePartitionsSeconds", "300");
        /// 
        ///  TableViewBuilder<byte[]> builder = ...;
        ///  builder = builder.loadConf(config);
        /// 
        ///  TableView<byte[]> tableView = builder.create();
        ///  }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="config"> configuration to load </param>
        /// <returns> the <seealso cref="ITableViewBuilder{T}"/> instance </returns>
        ITableViewBuilder<T> LoadConf(IDictionary<string, object> config);

        /// <summary>
        /// Finalize the creation of the <seealso cref="TableView"/> instance.
        /// 
        /// <para>This method will block until the tableView is created successfully or an exception is thrown.
        /// 
        /// </para>
        /// </summary>
        /// <returns> the <seealso cref="ITableView{T}"/> instance </returns>
        /// <exception cref="PulsarClientException">
        ///              if the tableView creation fails </exception>

        ITableView<T> Create();

        /// <summary>
        /// Finalize the creation of the <seealso cref="ITableView{T}"/> instance in asynchronous mode.
        /// 
        ///  <para>This method will return a <seealso cref="Task"/> that can be used to access the instance when it's ready.
        /// 
        /// </para>
        /// </summary>
        /// <returns> the <seealso cref="ITableView{T}"/> instance </returns>
        ValueTask<ITableView<T>> CreateAsync();

        /// <summary>
        /// Set the topic name of the <seealso cref="ITableView{T}"/>.
        /// </summary>
        /// <param name="topic"> the name of the topic to create the <seealso cref="ITableView{T}"/> </param>
        /// <returns> the <seealso cref="ITableViewBuilder{T}"/> builder instance </returns>
        ITableViewBuilder<T> Topic(string topic);

        /// <summary>
        /// Set the interval of updating partitions <i>(default: 1 minute)</i>. </summary>
        /// <param name="interval"> the interval of updating partitions </param>
        /// <param name="unit"> the time unit of the interval </param>
        /// <returns> the <seealso cref="ITableViewBuilder{T}"/> builder instance </returns>
        ITableViewBuilder<T> AutoUpdatePartitionsInterval(TimeSpan interval);
    }

}