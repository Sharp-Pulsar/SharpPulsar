using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SharpPulsar.Sql
{
	using NamedTypeSignature = com.facebook.presto.spi.type.NamedTypeSignature;
	using ParameterKind = com.facebook.presto.spi.type.ParameterKind;
	using TypeSignature = com.facebook.presto.spi.type.TypeSignature;
	using TypeSignatureParameter = com.facebook.presto.spi.type.TypeSignatureParameter;
	using ImmutableList = com.google.common.collect.ImmutableList;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.BIGINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.BING_TILE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.CHAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.DECIMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.GEOMETRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.INTERVAL_DAY_TO_SECOND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.INTERVAL_YEAR_TO_MONTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.IPADDRESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.IPPREFIX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.JSON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.REAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.ROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.SMALLINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.TIMESTAMP_WITH_TIME_ZONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.TIME_WITH_TIME_ZONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.TINYINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.StandardTypes.VARCHAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.spi.type.TypeSignature.parseTypeSignature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	public sealed class FixJsonDataUtils
	{
		private FixJsonDataUtils()
		{
		}

		public static IEnumerable<IList<object>> FixData(IList<Column> Columns, IEnumerable<IList<object>> Data)
		{
			if (Data == null)
			{
				return null;
			}
			requireNonNull(Columns, "columns is null");
			IList<TypeSignature> Signatures = Columns.Select(column => parseTypeSignature(column.Type)).ToList();
			ImmutableList.Builder<IList<object>> Rows = ImmutableList.builder();
			foreach (IList<object> Row in Data)
			{
				checkArgument(Row.Count == Columns.Count, "row/column size mismatch");
				IList<object> NewRow = new List<object>();
				for (int I = 0; I < Row.Count; I++)
				{
					NewRow.Add(FixValue(Signatures[I], Row[I]));
				}
				Rows.add(unmodifiableList(NewRow)); // allow nulls in list
			}
			return Rows.build();
		}

		/// <summary>
		/// Force values coming from Jackson to have the expected object type.
		/// </summary>
		private static object FixValue(TypeSignature Signature, object Value)
		{
			if (Value == null)
			{
				return null;
			}

			if (Signature.Base.Equals(ARRAY))
			{
				IList<object> FixedValue = new List<object>();
				foreach (object Object in typeof(System.Collections.IList).cast(Value))
				{
					FixedValue.Add(FixValue(Signature.TypeParametersAsTypeSignatures.get(0), Object));
				}
				return FixedValue;
			}
			if (Signature.Base.Equals(MAP))
			{
				TypeSignature KeySignature = Signature.TypeParametersAsTypeSignatures.get(0);
				TypeSignature ValueSignature = Signature.TypeParametersAsTypeSignatures.get(1);
				IDictionary<object, object> FixedValue = new Dictionary<object, object>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<?, ?> entry : (java.util.Set<java.util.Map.Entry<?, ?>>) java.util.Map.class.cast(value).entrySet())
				foreach (KeyValuePair<object, ?> Entry in (ISet<KeyValuePair<object, ?>>) typeof(System.Collections.IDictionary).cast(Value).entrySet())
				{
					FixedValue[FixValue(KeySignature, Entry.Key)] = FixValue(ValueSignature, Entry.Value);
				}
				return FixedValue;
			}
			if (Signature.Base.Equals(ROW))
			{
				IDictionary<string, object> FixedValue = new LinkedHashMap<string, object>();
				IList<object> ListValue = typeof(System.Collections.IList).cast(Value);
				checkArgument(ListValue.Count == Signature.Parameters.size(), "Mismatched data values and row type");
				for (int I = 0; I < ListValue.Count; I++)
				{
					TypeSignatureParameter Parameter = Signature.Parameters.get(I);
					checkArgument(Parameter.Kind == ParameterKind.NAMED_TYPE, "Unexpected parameter [%s] for row type", Parameter);
					NamedTypeSignature NamedTypeSignature = Parameter.NamedTypeSignature;
					string Key = NamedTypeSignature.Name.orElse("field" + I);
					FixedValue[Key] = FixValue(NamedTypeSignature.TypeSignature, ListValue[I]);
				}
				return FixedValue;
			}
			switch (Signature.Base)
			{
				case BIGINT:
					if (Value is string)
					{
						return long.Parse((string) Value);
					}
					return ((Number) Value).longValue();
				case INTEGER:
					if (Value is string)
					{
						return int.Parse((string) Value);
					}
					return ((Number) Value).intValue();
				case SMALLINT:
					if (Value is string)
					{
						return short.Parse((string) Value);
					}
					return ((Number) Value).shortValue();
				case TINYINT:
					if (Value is string)
					{
						return sbyte.Parse((string) Value);
					}
					return ((Number) Value).byteValue();
				case DOUBLE:
					if (Value is string)
					{
						return double.Parse((string) Value);
					}
					return ((Number) Value).doubleValue();
				case REAL:
					if (Value is string)
					{
						return float.Parse((string) Value);
					}
					return ((Number) Value).floatValue();
				case BOOLEAN:
					if (Value is string)
					{
						return bool.Parse((string) Value);
					}
					return typeof(Boolean).cast(Value);
				case VARCHAR:
				case JSON:
				case TIME:
				case TIME_WITH_TIME_ZONE:
				case TIMESTAMP:
				case TIMESTAMP_WITH_TIME_ZONE:
				case DATE:
				case INTERVAL_YEAR_TO_MONTH:
				case INTERVAL_DAY_TO_SECOND:
				case IPADDRESS:
				case IPPREFIX:
				case DECIMAL:
				case CHAR:
				case GEOMETRY:
					return typeof(string).cast(Value);
				case BING_TILE:
					// Bing tiles are serialized as strings when used as map keys,
					// they are serialized as json otherwise (value will be a LinkedHashMap).
					return Value;
				default:
					// for now we assume that only the explicit types above are passed
					// as a plain text and everything else is base64 encoded binary
					if (Value is string)
					{
						return Base64.Decoder.decode((string) Value);
					}
					return Value;
			}
		}
	}

}