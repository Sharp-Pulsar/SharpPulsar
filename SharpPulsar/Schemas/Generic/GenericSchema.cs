using Avro.Generic;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.ISchema;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SharpPulsar.Schemas.Generic
{
	/// <summary>
	/// A generic schema representation for AvroBasedGenericSchema .
	/// warning :
	/// we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema 、GenericAvroSchema )
	/// </summary>
	public abstract class GenericSchema : ISchema<GenericRecord>
	{
        public ISchemaInfo SchemaInfo => throw new NotImplementedException();

        /// <summary>
        /// Create a generic schema out of a <tt>SchemaInfo</tt>.
        ///  warning : we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema 、GenericAvroSchema ) </summary>
        /// <param name="schemaInfo"> schema info </param>
        /// <returns> a generic schema instance </returns>
        public static GenericSchema Of(ISchemaInfo schemaInfo)
		{
			return Of(schemaInfo, true);
		}

		/// <summary>
		/// warning :
		/// we suggest migrate GenericSchemaImpl.of() to  <GenericSchema Implementor>.of() method (e.g. GenericJsonSchema 、GenericAvroSchema ) </summary>
		/// <param name="schemaInfo"> <seealso cref="SchemaInfo"/> </param>
		/// <param name="useProvidedSchemaAsReaderSchema"> <seealso cref="Boolean"/> </param>
		/// <returns> generic schema implementation </returns>
		public static GenericSchema Of(ISchemaInfo schemaInfo, bool useProvidedSchemaAsReaderSchema)
		{
			/*switch (schemaInfo.Type)
			{
				case SchemaType.AVRO a:
					return (GenericSchema)new GenericAvroSchema(schemaInfo);
				case JSON:
					return new GenericJsonSchema(schemaInfo, useProvidedSchemaAsReaderSchema);
				default:
			}*/

			throw new NotSupportedException("Generic schema is not supported on schema type " + schemaInfo.Type + "'");
		}

        public sbyte[] Encode([NotNull] GenericRecord message)
        {
            throw new NotImplementedException();
        }

        public ISchema<GenericRecord> Clone()
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }

}
