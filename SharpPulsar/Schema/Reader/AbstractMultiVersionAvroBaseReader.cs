using SharpPulsar.Interfaces.Interceptor.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Schema.Reader
{
	/// <summary>
	/// The abstract class of multi version avro base reader.
	/// </summary>
	public abstract class AbstractMultiVersionAvroBaseReader : AbstractMultiVersionReader
	{

		protected internal Avro.Schema readerSchema;

		public AbstractMultiVersionAvroBaseReader(ISchemaReader providerSchemaReader, Avro.Schema readerSchema) : base(providerSchemaReader)
		{
			this.readerSchema = readerSchema;
		}
	}

}
