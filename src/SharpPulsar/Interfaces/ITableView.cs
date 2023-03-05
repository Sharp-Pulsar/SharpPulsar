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

    public interface ITableView<T> : System.IDisposable
    {

        /// <summary>
        /// Returns the number of key-value mappings in the <seealso cref="ITableView{T}"/>.
        /// </summary>
        /// <returns> the number of key-value mappings in this TableView </returns>
        int Size();

        /// <summary>
        /// Returns {@code true} if this <seealso cref="ITableView{T}"/> contains no key-value mappings.
        /// </summary>
        /// <returns> true if this TableView contains no key-value mappings </returns>
        bool Empty { get; }

        /// <summary>
        /// Returns {@code true} if this <seealso cref="ITableView{T}"/> contains a mapping for the specified
        /// key.
        /// </summary>
        /// <param name="key"> key whose presence in this map is to be tested </param>
        /// <returns> true if this map contains a mapping for the specified key </returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Returns the value to which the specified key is mapped, or null if this map contains
        /// no mapping for the key.
        /// </summary>
        /// <param name="key"> the key whose associated value is to be returned </param>
        /// <returns> the value associated with the key or null if the keys was not found </returns>
        T Get(string key);

        /// <summary>
        /// Returns a Set view of the mappings contained in this map.
        /// </summary>
        /// <returns> a set view of the mappings contained in this map </returns>
        ISet<KeyValuePair<string, T>> EntrySet();

        /// <summary>
        /// Returns a <seealso cref="System.Collections.Generic.ISet<object>"/> view of the keys contained in this <seealso cref="TableView"/>.
        /// </summary>
        /// <returns> a set view of the keys contained in this map </returns>
        ISet<string> KeySet();

        /// <summary>
        /// Returns a Collection view of the values contained in this <seealso cref="TableView"/>.
        /// </summary>
        /// <returns> a collection view of the values contained in this TableView </returns>
        ICollection<T> Values();

        /// <summary>
        /// Performs the give action for each entry in this map until all entries
        /// have been processed or the action throws an exception.
        /// </summary>
        /// <param name="action"> The action to be performed for each entry </param>
        void ForEachAndListen(System.Action<string, T> action);

        /// <summary>
        /// Close the table view and releases resources allocated.
        /// </summary>
        /// <returns> a future that can used to track when the table view has been closed. </returns>
        Task CloseAsync();
    }

}