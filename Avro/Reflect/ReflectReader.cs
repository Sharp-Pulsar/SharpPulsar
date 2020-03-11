/* 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Avro.Reflect
{
    using Avro.Generic;
    using Avro.IO;

    /// <summary>
    /// Reader wrapper class for reading data and storing into specific classes.
    /// </summary>
    /// <typeparam name="T">Specific class type.</typeparam>
    public class ReflectReader<T> : DatumReader<T>
    {
        /// <summary>
        /// Reader class for reading data and storing into specific classes.
        /// </summary>
        private readonly ReflectDefaultReader _reader;

        /// <summary>
        /// Gets default reader.
        /// </summary>
        public ReflectDefaultReader Reader { get => this._reader; }

        /// <summary>
        /// Gets schema for the writer class.
        /// </summary>
        public Schema WriterSchema { get => this._reader.WriterSchema; }

        /// <summary>
        /// Gets schema for the reader class.
        /// </summary>
        public Schema ReaderSchema { get => this._reader.ReaderSchema; }

        /// <summary>
        /// Constructs a generic reader for the given schemas using the DefaultReader. If the
        /// reader's and writer's schemas are different this class performs the resolution.
        /// </summary>
        /// <param name="writerSchema">The schema used while generating the data.</param>
        /// <param name="readerSchema">The schema desired by the reader.</param>
        /// <param name="cache">Class cache.</param>
        public ReflectReader(Schema writerSchema, Schema readerSchema, ClassCache cache = null)
        {
            this._reader = new ReflectDefaultReader(typeof(T), writerSchema, readerSchema, cache);
        }

        /// <summary>
        /// Constructs a generic reader from an instance of a ReflectDefaultReader (non-generic).
        /// </summary>
        /// <param name="reader"></param>
        public ReflectReader(ReflectDefaultReader reader)
        {
            this._reader = reader;
        }

        /// <summary>
        /// Generic read function.
        /// </summary>
        /// <param name="reuse">object to store data read.</param>
        /// <param name="dec">decorder to use for reading data.</param>
        /// <returns></returns>
        public T Read(T reuse, Decoder dec)
        {
            return this._reader.Read(reuse, dec);
        }

        /// <summary>
        /// Generic read function.
        /// </summary>
        /// <param name="dec">decorder to use for reading data.</param>
        /// <returns></returns>
        public T Read(Decoder dec)
        {
            return this._reader.Read(default(T), dec);
        }
    }
}
