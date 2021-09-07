
using SharpPulsar.Interfaces.ISchema;

namespace SharpPulsar.Schemas.Reader
{
	/// <summary>
	/// The abstract class of multi version avro base reader.
	/// </summary>
	public abstract class AbstractMultiVersionAvroBaseReader<T> : AbstractMultiVersionReader<T>
	{

		protected internal Avro.RecordSchema readerSchema;

		public AbstractMultiVersionAvroBaseReader(ISchemaReader<T> providerSchemaReader, Avro.RecordSchema readerSchema) : base(providerSchemaReader)
		{
			this.readerSchema = readerSchema;
		}
	}

}
