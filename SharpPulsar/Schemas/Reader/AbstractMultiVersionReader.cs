using Avro;
using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Schema;
using System;
using System.IO;
using System.Runtime.Serialization;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Cache;

namespace SharpPulsar.Schemas.Reader
{
    public abstract class AbstractMultiVersionReader<T> : ISchemaReader<T>
    {
		protected internal readonly ISchemaReader<T> providerSchemaReader;
		protected internal ISchemaInfoProvider schemaInfoProvider;
		Cache<BytesSchemaVersion, ISchemaReader<T>> _readerCache = new Cache<BytesSchemaVersion, ISchemaReader<T>>(TimeSpan.FromMinutes(30));
		
		public AbstractMultiVersionReader(ISchemaReader<T> providerSchemaReader)
		{
			this.providerSchemaReader = providerSchemaReader;
		}

        public ISchemaReader<T> GetSchemaReader(byte[] schemaVersion)
        {
            var key = BytesSchemaVersion.Of(schemaVersion);
           return  _readerCache.Get(key, new Func<BytesSchemaVersion, ISchemaReader<T>>(LoadReader));
        }
        public T Read(byte[] bytes, int offset, int length)
		{
			return providerSchemaReader.Read(bytes);
		}
		private T Read(byte[] bytes)
		{
			return providerSchemaReader.Read(bytes);
		}

		public T Read(Stream inputStream)
		{
			return providerSchemaReader.Read(inputStream);
		}

		public virtual T Read(Stream inputStream, byte[] schemaVersion)
		{
			try
			{
                var key = BytesSchemaVersion.Of(schemaVersion);
                return schemaVersion == null ? Read(inputStream) : _readerCache.Get(key, new Func<BytesSchemaVersion, ISchemaReader<T>>(LoadReader)).Read(inputStream);
			}
			catch (Exception e)
			{
				//LOG.error("Can't get generic schema for topic {} schema version {}", schemaInfoProvider.Topic, Hex.encodeHexString(schemaVersion), e);
				throw new Exception("Can't get generic schema for topic " + schemaInfoProvider.TopicName);
			}
		}

		public virtual T Read(byte[] bytes, byte[] schemaVersion)
		{
			try
			{
                if (schemaVersion == null)
                    return Read(bytes);
                else
                {
                    var key = BytesSchemaVersion.Of(schemaVersion);
                    var sc = _readerCache.Get(key, new Func<BytesSchemaVersion, ISchemaReader<T>>(LoadReader));
                    return sc.Read(bytes);
                }
			}
			catch (Exception e) when (e is AvroTypeException)
			{
				if (e is AvroTypeException)
				{
					throw new SchemaSerializationException(e);
				}
				//LOG.error("Can't get generic schema for topic {} schema version {}", schemaInfoProvider.Topic, Hex.encodeHexString(schemaVersion), e);
				throw new Exception("Can't get generic schema for topic " + schemaInfoProvider.TopicName);
			}
		}

		public ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				schemaInfoProvider = value;
			}
		}

		/// <summary>
		/// Load the schema reader for reading messages encoded by the given schema version.
		/// </summary>
		/// <param name="schemaVersion"> the provided schema version </param>
		/// <returns> the schema reader for decoding messages encoded by the provided schema version. </returns>
		protected internal abstract ISchemaReader<T> LoadReader(BytesSchemaVersion schemaVersion);

		/// <summary>
		/// TODO: think about how to make this async
		/// </summary>
		protected internal virtual ISchemaInfo GetSchemaInfoByVersion(byte[] schemaVersion)
		{
			try
			{
                if (schemaInfoProvider == null)
                    return null;

				return schemaInfoProvider.GetSchemaByVersion(schemaVersion);
			}
			catch (Exception e)
			{
				throw new SerializationException("Interrupted at fetching schema info for " + SchemaUtils.GetStringSchemaVersion(schemaVersion), e);
			}
		}
	}
}
