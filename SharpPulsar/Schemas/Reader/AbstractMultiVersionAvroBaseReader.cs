
using SharpPulsar.Interfaces.ISchema;

namespace SharpPulsar.Schemas.Reader
{
	/// <summary>
	/// The abstract class of multi version avro base reader.
	/// </summary>
	public abstract class AbstractMultiVersionAvroBaseReader<T> : AbstractMultiVersionReader<T>
	{

		protected internal Avro.Schema readerSchema;

		public AbstractMultiVersionAvroBaseReader(ISchemaReader<T> providerSchemaReader, Avro.Schema readerSchema) : base(providerSchemaReader)
		{
			this.readerSchema = readerSchema;
		}
	}

}
