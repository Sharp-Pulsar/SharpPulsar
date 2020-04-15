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

using Avro.Schemas;

namespace Avro.File
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Avro.CodeGen;
    using Avro.Generic;
    using Avro.IO;

    /// <summary>
    /// Stores in a file a sequence of data conforming to a schema. The schema is stored in the file
    /// with the data. Each datum in a file is of the same schema. Data is written with a
    /// <see cref="DatumWriter{T}"/>. Data is grouped into blocks. A synchronization marker is
    /// written between blocks, so that files may be split. Blocks may be compressed. Extensible
    /// metadata is stored at the end of the file. Files may be appended to.
    /// </summary>
    /// <typeparam name="T">Type of datum to write to the file.</typeparam>
    public class DataFileWriter<T> : IFileWriter<T>
    {
        private Schema _schema;
        private Codec _codec;
        private Stream _stream;
        private MemoryStream _blockStream;
        private Encoder _encoder, _blockEncoder;
        private DatumWriter<T> _writer;

        private byte[] _syncData;
        private bool _isOpen;
        private bool _headerWritten;
        private int _blockCount;
        private int _syncInterval;
        private IDictionary<string, byte[]> _metaData;

        /// <summary>
        /// Open a new writer instance to write
        /// to a file path, using a Null codec.
        /// </summary>
        /// <param name="writer">Datum writer to use.</param>
        /// <param name="path">Path to the file.</param>
        /// <returns>A new file writer.</returns>
        public static IFileWriter<T> OpenWriter(DatumWriter<T> writer, string path)
        {
            return OpenWriter(writer, new FileStream(path, FileMode.Create), Codec.CreateCodec(Codec.Type.Null));
        }

        /// <summary>
        /// Open a new writer instance to write
        /// to an output stream, using a Null codec.
        /// </summary>
        /// <param name="writer">Datum writer to use.</param>
        /// <param name="outStream">Stream to write to.</param>
        /// <returns>A new file writer.</returns>
        public static IFileWriter<T> OpenWriter(DatumWriter<T> writer, Stream outStream)
        {
            return OpenWriter(writer, outStream, Codec.CreateCodec(Codec.Type.Null));
        }

        /// <summary>
        /// Open a new writer instance to write
        /// to a file path with a specified codec.
        /// </summary>
        /// <param name="writer">Datum writer to use.</param>
        /// <param name="path">Path to the file.</param>
        /// <param name="codec">Codec to use when writing.</param>
        /// <returns>A new file writer.</returns>
        public static IFileWriter<T> OpenWriter(DatumWriter<T> writer, string path, Codec codec)
        {
            return OpenWriter(writer, new FileStream(path, FileMode.Create), codec);
        }

        /// <summary>
        /// Open a new writer instance to write
        /// to an output stream with a specified codec.
        /// </summary>
        /// <param name="writer">Datum writer to use.</param>
        /// <param name="outStream">Stream to write to.</param>
        /// <param name="codec">Codec to use when writing.</param>
        /// <returns>A new file writer.</returns>
        public static IFileWriter<T> OpenWriter(DatumWriter<T> writer, Stream outStream, Codec codec)
        {
            return new DataFileWriter<T>(writer).Create(writer.Schema, outStream, codec);
        }

        DataFileWriter(DatumWriter<T> writer)
        {
            this._writer = writer;
            this._syncInterval = DataFileConstants.DefaultSyncInterval;
        }

        /// <inheritdoc/>
        public bool IsReservedMeta(string key)
        {
            return key.StartsWith(DataFileConstants.MetaDataReserved, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public void SetMeta(string key, byte[] value)
        {
            if (this.IsReservedMeta(key))
            {
                throw new AvroRuntimeException("Cannot set reserved meta key: " + key);
            }
            this._metaData.Add(key, value);
        }

        /// <inheritdoc/>
        public void SetMeta(string key, long value)
        {
            try
            {
                this.SetMeta(key, this.GetByteValue(value.ToString(CultureInfo.InvariantCulture)));
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void SetMeta(string key, string value)
        {
            try
            {
                this.SetMeta(key, this.GetByteValue(value));
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void SetSyncInterval(int syncInterval)
        {
            if (syncInterval < 32 || syncInterval > (1 << 30))
            {
                throw new AvroRuntimeException("Invalid sync interval value: " + syncInterval);
            }
            this._syncInterval = syncInterval;
        }

        /// <inheritdoc/>
        public void Append(T datum)
        {
            this.AssertOpen();
            this.EnsureHeader();

            long usedBuffer = this._blockStream.Position;

            try
            {
                this._writer.Write(datum, this._blockEncoder);
            }
            catch (Exception e)
            {
                this._blockStream.Position = usedBuffer;
                throw new AvroRuntimeException("Error appending datum to writer", e);
            }
            this._blockCount++;
            this.WriteIfBlockFull();
        }

        private void EnsureHeader()
        {
            if (!this._headerWritten)
            {
                this.WriteHeader();
                this._headerWritten = true;
            }
        }

        /// <inheritdoc/>
        public void Flush()
        {
            this.EnsureHeader();
            this.SyncInternal();
        }

        /// <inheritdoc/>
        public long Sync()
        {
            this.SyncInternal();
            return this._stream.Position;
        }

        private void SyncInternal()
        {
            this.AssertOpen();
            this.WriteBlock();
        }

        /// <inheritdoc/>
        public void Close()
        {
            this.EnsureHeader();
            this.Flush();
            this._stream.Flush();
            this._stream.Close();
            this._isOpen = false;
        }

        private void WriteHeader()
        {
            this._encoder.WriteFixed(DataFileConstants.Magic);
            this.WriteMetaData();
            this.WriteSyncData();
        }

        private void Init()
        {
            this._blockCount = 0;
            this._encoder = new BinaryEncoder(this._stream);
            this._blockStream = new MemoryStream();
            this._blockEncoder = new BinaryEncoder(this._blockStream);

            if (this._codec == null)
            {
                this._codec = Codec.CreateCodec(Codec.Type.Null);
            }

            this._isOpen = true;
        }

        private void AssertOpen()
        {
            if (!this._isOpen)
            {
                throw new AvroRuntimeException("Cannot complete operation: avro file/stream not open");
            }
        }

        private IFileWriter<T> Create(Schema schema, Stream outStream, Codec codec)
        {
            this._codec = codec;
            this._stream = outStream;
            this._metaData = new Dictionary<string, byte[]>();
            this._schema = schema;

            this.Init();

            return this;
        }

        private void WriteMetaData()
        {
            // Add sync, code & schema to metadata
            this.GenerateSyncData();
            //SetMetaInternal(DataFileConstants.MetaDataSync, _syncData); - Avro 1.5.4 C
            this.SetMetaInternal(DataFileConstants.MetaDataCodec, this.GetByteValue(this._codec.GetName()));
            this.SetMetaInternal(DataFileConstants.MetaDataSchema, this.GetByteValue(this._schema.ToString()));

            // write metadata
            int size = this._metaData.Count;
            this._encoder.WriteInt(size);

            foreach (KeyValuePair<string, byte[]> metaPair in this._metaData)
            {
                this._encoder.WriteString(metaPair.Key);
                this._encoder.WriteBytes(metaPair.Value);
            }
            this._encoder.WriteMapEnd();
        }

        private void WriteIfBlockFull()
        {
            if (this.BufferInUse() >= this._syncInterval)
            {
                this.WriteBlock();
            }
        }

        private long BufferInUse()
        {
            return this._blockStream.Position;
        }

        private void WriteBlock()
        {
            if (this._blockCount > 0)
            {
                byte[] dataToWrite = this._blockStream.ToArray();

                // write count
                this._encoder.WriteLong(this._blockCount);

                // write data
                this._encoder.WriteBytes(this._codec.Compress(dataToWrite));

                // write sync marker
                this._encoder.WriteFixed(this._syncData);

                // reset / re-init block
                this._blockCount = 0;
                this._blockStream = new MemoryStream();
                this._blockEncoder = new BinaryEncoder(this._blockStream);
            }
        }

        private void WriteSyncData()
        {
            this._encoder.WriteFixed(this._syncData);
        }

        private void GenerateSyncData()
        {
            this._syncData = new byte[16];

            Random random = new Random();
            random.NextBytes(this._syncData);
        }

        private void SetMetaInternal(string key, byte[] value)
        {
            this._metaData.Add(key, value);
        }

        private byte[] GetByteValue(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources associated with this <see cref="DataFileWriter{T}"/>.
        /// </summary>
        /// <param name="disposing">
        /// True if called from <see cref="Dispose()"/>; false otherwise.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            this.Close();
        }
    }
}
