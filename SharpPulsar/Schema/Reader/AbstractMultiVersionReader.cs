using Avro;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl.Crypto;
using SharpPulsar.Protocol.Schema;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using SharpPulsar.Interfaces.ISchema;

namespace SharpPulsar.Schema.Reader
{
    public abstract class AbstractMultiVersionReader<T> : ISchemaReader<T>
    {
		protected internal readonly ISchemaReader<T> providerSchemaReader;
		protected internal ISchemaInfoProvider schemaInfoProvider;
		Cache<BytesSchemaVersion, ISchemaReader<T>> _readerCache = new Cache<BytesSchemaVersion, ISchemaReader<T>>(30);
		
		public AbstractMultiVersionReader(ISchemaReader<T> providerSchemaReader)
		{
			this.providerSchemaReader = providerSchemaReader;
		}

		public T Read(sbyte[] bytes, int offset, int length)
		{
			return providerSchemaReader.Read(bytes);
		}
		private T Read(sbyte[] bytes)
		{
			return providerSchemaReader.Read(bytes);
		}

		public T Read(Stream inputStream)
		{
			return providerSchemaReader.Read(inputStream);
		}

		public virtual T Read(Stream inputStream, sbyte[] schemaVersion)
		{
			try
			{
				return schemaVersion == null ? Read(inputStream) : _readerCache.Get(BytesSchemaVersion.Of(schemaVersion)).Read(inputStream);
			}
			catch (Exception e)
			{
				//LOG.error("Can't get generic schema for topic {} schema version {}", schemaInfoProvider.TopicName, Hex.encodeHexString(schemaVersion), e);
				throw new Exception("Can't get generic schema for topic " + schemaInfoProvider.TopicName);
			}
		}

		public virtual T Read(sbyte[] bytes, sbyte[] schemaVersion)
		{
			try
			{
				return schemaVersion == null ? Read(bytes) : _readerCache.Get(BytesSchemaVersion.Of(schemaVersion)).Read(bytes);
			}
			catch (Exception e) when (e is AvroTypeException)
			{
				if (e is AvroTypeException)
				{
					throw new SchemaSerializationException(e);
				}
				//LOG.error("Can't get generic schema for topic {} schema version {}", schemaInfoProvider.TopicName, Hex.encodeHexString(schemaVersion), e);
				throw new Exception("Can't get generic schema for topic " + schemaInfoProvider.TopicName);
			}
		}

		public ISchemaInfoProvider SchemaInfoProvider
		{
			set
			{
				this.schemaInfoProvider = value;
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
		protected internal virtual ISchemaInfo GtSchemaInfoByVersion(sbyte[] schemaVersion)
		{
			try
			{
				return schemaInfoProvider.GetSchemaByVersion(schemaVersion);
			}
			catch (Exception e)
			{
				throw new SerializationException("Interrupted at fetching schema info for " + SchemaUtils.GetStringSchemaVersion(schemaVersion), e);
			}
		}
	}
}
