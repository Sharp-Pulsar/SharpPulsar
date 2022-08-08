using SharpPulsar.Shared;
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
namespace SharpPulsar.Interfaces.Schema
{

    /// <summary>
    /// This is an abstraction over the logical value that is store into a Message.
    /// Pulsar decodes the payload of the Message using the Schema that is configured for the topic.
    /// </summary>
    public interface IGenericObject
    {

        /// <summary>
        /// Return the schema tyoe.
        /// </summary>
        /// <returns> the schema type </returns>
        /// <exception cref="NotSupportedException"> if this feature is not implemented </exception>
        /// <seealso cref= SchemaType#AVRO </seealso>
        /// <seealso cref= SchemaType#PROTOBUF_NATIVE </seealso>
        /// <seealso cref= SchemaType#JSON </seealso>
        SchemaType SchemaType
        {
            get
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// Return the internal native representation of the Record,
        /// like a AVRO GenericRecord.
        /// </summary>
        /// <returns> the internal representation of the record </returns>
        /// <exception cref="NotSupportedException"> if the operation is not supported </exception>
        object NativeObject
        {
            get
            {
                throw new System.NotSupportedException();
            }
        }

    }
}
