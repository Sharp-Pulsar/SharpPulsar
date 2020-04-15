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
    using System.Linq;
    using Avro.CodeGen;
    using Avro.Generic;
    using Avro.IO;
    using Avro.Specific;

    /// <summary>
    /// Provides access to Avro data written using the <see cref="DataFileWriter{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type to deserialze data objects to.</typeparam>
    public class DataFileReader<T> : IFileReader<T>
    {
        /// <summary>
        /// Defines the signature for a function that returns a new <see cref="DatumReader{T}"/>
        /// given a writer and reader schema.
        /// </summary>
        /// <param name="writerSchema">Schema used to write the datum.</param>
        /// <param name="readerSchema">Schema used to read the datum.</param>
        /// <returns>A datum reader.</returns>
        public delegate DatumReader<T> CreateDatumReader(Schema writerSchema, Schema readerSchema);

        private DatumReader<T> _reader;
        private Decoder _decoder, _datumDecoder;
        private Header _header;
        private Codec _codec;
        private DataBlock _currentBlock;
        private long _blockRemaining;
        private long _blockSize;
        private bool _availableBlock;
        private byte[] _syncBuffer;
        private long _blockStart;
        private Stream _stream;
        private Schema _readerSchema;
        private readonly CreateDatumReader _datumReaderFactory;

        /// <summary>
        ///  Open a reader for a file using path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IFileReader<T> OpenReader(string path)
        {
            return OpenReader(new FileStream(path, FileMode.Open), null);
        }

        /// <summary>
        ///  Open a reader for a file using path and the reader's schema.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="readerSchema">Schema used to read data from the file.</param>
        /// <returns>A new file reader.</returns>
        public static IFileReader<T> OpenReader(string path, Schema readerSchema)
        {
            return OpenReader(new FileStream(path, FileMode.Open), readerSchema);
        }

        /// <summary>
        ///  Open a reader for a stream.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public static IFileReader<T> OpenReader(Stream inStream)
        {
            return OpenReader(inStream, null);
        }

        /// <summary>
        /// Open a reader for a stream using the reader's schema.
        /// </summary>
        /// <param name="inStream">Stream containing the file contents.</param>
        /// <param name="readerSchema">Schema used to read the file.</param>
        /// <returns>A new file reader.</returns>
        public static IFileReader<T> OpenReader(Stream inStream, Schema readerSchema)
        {
            return OpenReader(inStream, readerSchema, CreateDefaultReader);
        }

        /// <summary>
        ///  Open a reader for a stream using the reader's schema and a custom DatumReader.
        /// </summary>
        /// <param name="inStream">Stream of file contents.</param>
        /// <param name="readerSchema">Schema used to read the file.</param>
        /// <param name="datumReaderFactory">Factory to create datum readers given a reader an writer schema.</param>
        /// <returns>A new file reader.</returns>
        public static IFileReader<T> OpenReader(Stream inStream, Schema readerSchema, CreateDatumReader datumReaderFactory)
        {
            return new DataFileReader<T>(inStream, readerSchema, datumReaderFactory);         // (not supporting 1.2 or below, format)
        }

        DataFileReader(Stream stream, Schema readerSchema, CreateDatumReader datumReaderFactory)
        {
            this._readerSchema = readerSchema;
            this._datumReaderFactory = datumReaderFactory;
            this.Init(stream);
            this.BlockFinished();
        }

        /// <inheritdoc/>
        public Header GetHeader()
        {
            return this._header;
        }

        /// <inheritdoc/>
        public Schema GetSchema()
        {
            return this._header.Schema;
        }

        /// <inheritdoc/>
        public ICollection<string> GetMetaKeys()
        {
            return this._header.MetaData.Keys;
        }

        /// <inheritdoc/>
        public byte[] GetMeta(string key)
        {
            try
            {
                return this._header.MetaData[key];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public long GetMetaLong(string key)
        {
            return long.Parse(this.GetMetaString(key), CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
        public string GetMetaString(string key)
        {
            byte[] value = this.GetMeta(key);
            if (value == null)
            {
                return null;
            }
            try
            {
                return System.Text.Encoding.UTF8.GetString(value);
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException(string.Format(CultureInfo.InvariantCulture,
                    "Error fetching meta data for key: {0}", key), e);
            }
        }

        /// <inheritdoc/>
        public void Seek(long position)
        {
            if (!this._stream.CanSeek)
            {
                throw new AvroRuntimeException("Not a valid input stream - must be seekable!");
            }

            this._stream.Position = position;
            this._decoder = new BinaryDecoder(this._stream);
            this._datumDecoder = null;
            this._blockRemaining = 0;
            this._blockStart = position;
        }

        /// <inheritdoc/>
        public void Sync(long position)
        {
            this.Seek(position);
            // work around an issue where 1.5.4 C stored sync in metadata
            if ((position == 0) && (this.GetMeta(DataFileConstants.MetaDataSync) != null))
            {
                this.Init(this._stream); // re-init to skip header
                return;
            }

            try
            {
                bool done = false;

                // read until sync mark matched
                do
                {
                    this._decoder.ReadFixed(this._syncBuffer);
                    if (Enumerable.SequenceEqual(this._syncBuffer, this._header.SyncData))
                    {
                        done = true;
                    }
                    else
                    {
                        this._stream.Position -= DataFileConstants.SyncSize - 1;
                    }
                }
                while (!done);
            }
            catch
            {
                // could not find .. default to EOF
            }

            this._blockStart = this._stream.Position;
        }

        /// <inheritdoc/>
        public bool PastSync(long position)
        {
            return (this._blockStart >= position + DataFileConstants.SyncSize) || (this._blockStart >= this._stream.Length);
        }

        /// <inheritdoc/>
        public long PreviousSync()
        {
            if (!this._stream.CanSeek)
            {
                throw new AvroRuntimeException("Not a valid input stream - must be seekable!");
            }

            return this._blockStart;
        }

        /// <inheritdoc/>
        public long Tell()
        {
            return this._stream.Position;
        }

        /// <inheritdoc/>
        public IEnumerable<T> NextEntries
        {
            get
            {
                while (this.HasNext())
                {
                    yield return this.Next();
                }
            }
        }

        /// <inheritdoc/>
        public bool HasNext()
        {
            try
            {
                if (this._blockRemaining == 0)
                {
                    // TODO: Check that the (block) stream is not partially read
                    /*if (_datumDecoder != null)
                    { }*/
                    if (this.HasNextBlock())
                    {
                        this._currentBlock = this.NextRawBlock(this._currentBlock);
                        this._currentBlock.Data = this._codec.Decompress(this._currentBlock.Data);
                        this._datumDecoder = new BinaryDecoder(this._currentBlock.GetDataAsStream());
                    }
                }
                return this._blockRemaining != 0;
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException(string.Format(CultureInfo.InvariantCulture,
                    "Error fetching next object from block: {0}", e));
            }
        }

        /// <summary>
        /// Resets this reader.
        /// </summary>
        public void Reset()
        {
            this.Init(this._stream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources associated with this <see cref="DataFileReader{T}"/>.
        /// </summary>
        /// <param name="disposing">
        /// True if called from <see cref="Dispose()"/>; false otherwise.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            this._stream.Close();
        }

        private void Init(Stream stream)
        {
            this._stream = stream;
            this._header = new Header();
            this._decoder = new BinaryDecoder(stream);
            this._syncBuffer = new byte[DataFileConstants.SyncSize];

            // read magic
            byte[] firstBytes = new byte[DataFileConstants.Magic.Length];
            try
            {
                this._decoder.ReadFixed(firstBytes);
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException("Not a valid data file!", e);
            }
            if (!firstBytes.SequenceEqual(DataFileConstants.Magic))
            {
                throw new AvroRuntimeException("Not a valid data file!");
            }

            // read meta data
            long len = this._decoder.ReadMapStart();
            if (len > 0)
            {
                do
                {
                    for (long i = 0; i < len; i++)
                    {
                        string key = this._decoder.ReadString();
                        byte[] val = this._decoder.ReadBytes();
                        this._header.MetaData.Add(key, val);
                    }
                } while ((len = this._decoder.ReadMapNext()) != 0);
            }

            // read in sync data
            this._decoder.ReadFixed(this._header.SyncData);

            // parse schema and set codec
            this._header.Schema = Schema.Parse(this.GetMetaString(DataFileConstants.MetaDataSchema));
            this._reader = this._datumReaderFactory(this._header.Schema, this._readerSchema ?? this._header.Schema);
            this._codec = this.ResolveCodec();
        }

        private static DatumReader<T> CreateDefaultReader(Schema writerSchema, Schema readerSchema)
        {
            DatumReader<T> reader = null;
            Type type = typeof(T);

            if (typeof(ISpecificRecord).IsAssignableFrom(type))
            {
                reader = new SpecificReader<T>(writerSchema, readerSchema);
            }
            else // generic
            {
                reader = new GenericReader<T>(writerSchema, readerSchema);
            }
            return reader;
        }

        private Codec ResolveCodec()
        {
            return Codec.CreateCodecFromString(this.GetMetaString(DataFileConstants.MetaDataCodec));
        }

        /// <inheritdoc/>
        public T Next()
        {
            return this.Next(default(T));
        }

        private T Next(T reuse)
        {
            try
            {
                if (!this.HasNext())
                {
                    throw new AvroRuntimeException("No more datum objects remaining in block!");
                }

                T result = this._reader.Read(reuse, this._datumDecoder);
                if (--this._blockRemaining == 0)
                {
                    this.BlockFinished();
                }
                return result;
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException(string.Format(CultureInfo.InvariantCulture,
                    "Error fetching next object from block: {0}", e));
            }
        }

        private void BlockFinished()
        {
            if (this._stream.CanSeek)
            {
                this._blockStart = this._stream.Position;
            }
        }

        private DataBlock NextRawBlock(DataBlock reuse)
        {
            if (!this.HasNextBlock())
            {
                throw new AvroRuntimeException("No data remaining in block!");
            }

            if (reuse == null || reuse.Data.Length < this._blockSize)
            {
                reuse = new DataBlock(this._blockRemaining, this._blockSize);
            }
            else
            {
                reuse.NumberOfEntries = this._blockRemaining;
                reuse.BlockSize = this._blockSize;
            }

            this._decoder.ReadFixed(reuse.Data, 0, (int)reuse.BlockSize);
            this._decoder.ReadFixed(this._syncBuffer);

            if (!Enumerable.SequenceEqual(this._syncBuffer, this._header.SyncData))
            {
                throw new AvroRuntimeException("Invalid sync!");
            }

            this._availableBlock = false;
            return reuse;
        }

        private bool DataLeft()
        {
            long currentPosition = this._stream.Position;
            if (this._stream.ReadByte() != -1)
            {
                this._stream.Position = currentPosition;
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool HasNextBlock()
        {
            try
            {
                // block currently being read
                if (this._availableBlock)
                {
                    return true;
                }

                // check to ensure still data to read
                if (this._stream.CanSeek)
                {
                    if (!this.DataLeft())
                    {
                        return false;
                    }

                    this._blockRemaining = this._decoder.ReadLong();      // read block count
                }
                else
                {
                    // when the stream is not seekable, the only way to know if there is still
                    // some data to read is to reach the end and raise an AvroException here.
                    try
                    {
                        this._blockRemaining = this._decoder.ReadLong();      // read block count
                    }
                    catch(AvroException)
                    {
                        return false;
                    }
                }

                this._blockSize = this._decoder.ReadLong();           // read block size
                if (this._blockSize > System.Int32.MaxValue || this._blockSize < 0)
                {
                    throw new AvroRuntimeException("Block size invalid or too large for this " +
                                                   "implementation: " + this._blockSize);
                }
                this._availableBlock = true;
                return true;
            }
            catch (Exception e)
            {
                throw new AvroRuntimeException(string.Format(CultureInfo.InvariantCulture,
                    "Error ascertaining if data has next block: {0}", e), e);
            }
        }

        /// <summary>
        /// Encapsulates a block of data read by the <see cref="DataFileReader{T}"/>.
        /// </summary>
        private class DataBlock
        {
            /// <summary>
            /// Gets or sets raw bytes within this block.
            /// </summary>
            public byte[] Data { get;  set; }

            /// <summary>
            /// Gets or sets number of entries in this block.
            /// </summary>
            public long NumberOfEntries { get; set; }

            /// <summary>
            /// Gets or sets size of this block in bytes.
            /// </summary>
            public long BlockSize { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DataBlock"/> class.
            /// </summary>
            /// <param name="numberOfEntries">Number of entries in this block.</param>
            /// <param name="blockSize">Size of this block in bytes.</param>
            public DataBlock(long numberOfEntries, long blockSize)
            {
                this.NumberOfEntries = numberOfEntries;
                this.BlockSize = blockSize;
                this.Data = new byte[blockSize];
            }

            internal Stream GetDataAsStream()
            {
                return new MemoryStream(this.Data);
            }
        }
    }
}
