using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.API.Common.Schema;

namespace SharpPulsar.API.Schema
{
    internal class IKeyValueSchema
    {
    }
}
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
namespace SharpPulsar.API.Schema
{

    /// <summary>
    /// This interface models a Schema that is composed of two parts.
    /// A Key and a Value. </summary>
    /// @param <K> the type of the Key </param>
    /// @param <V> the type of the Value. </param>
    public interface IKeyValueSchema<K, V> : ISchema<KeyValue<K, V>>
    {

        /// <summary>
        /// Get the Schema of the Key. </summary>
        /// <returns> the Schema of the Key </returns>
        ISchema<K> KeySchema { get; }

        /// <summary>
        /// Get the Schema of the Value.
        /// </summary>
        /// <returns> the Schema of the Value </returns>
        ISchema<V> ValueSchema { get; }

        /// <summary>
        /// Get the KeyValueEncodingType.
        /// </summary>
        /// <returns> the KeyValueEncodingType </returns>
        /// <seealso cref="KeyValueEncodingType.INLINE"/>
        /// <seealso cref="KeyValueEncodingType.SEPARATED"/>
        KeyValueEncodingType KeyValueEncodingType { get; }
    }
}
