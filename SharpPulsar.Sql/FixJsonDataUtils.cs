using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpPulsar.Sql.Facebook.Type;
using SharpPulsar.Sql.Precondition;

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
    public sealed class FixJsonDataUtils
	{
		private FixJsonDataUtils()
		{
		}

		public static IList<object> FixData(IList<Column> columns, IEnumerable<IList<object>> data)
		{
			if (data == null)
			{
				return null;
			}
			ParameterCondition.RequireNonNull(columns, "columns", "columns is null");
			IList<TypeSignature> signatures = columns.ToList().Select(column => ParseTypeSignature(column.Type)).ToList();
			IList<object> rows = new List<object>();
			foreach (IList<object> row in data)
			{
				ParameterCondition.CheckArgument(row.Count == columns.Count, "row/column size mismatch");
				IList<object> newRow = new List<object>();
				for (int i = 0; i < row.Count; i++)
				{
					newRow.Add(FixValue(signatures[i], row[i]));
				}
				rows.Add(new List<object>(newRow)); // allow nulls in list
			}
			return rows.ToList();
		}

		/// <summary>
		/// Force values coming from Jackson to have the expected object type.
		/// </summary>
		private static object FixValue(TypeSignature signature, object value)
		{
			if (value == null)
			{
				return null;
			}

			if (signature.Base.Equals(StandardTypes.Array))
			{
				IList<object> fixedValue = new List<object>();
				foreach (object @object in (IList<object>)value)
				{
					fixedValue.Add(FixValue(signature.GetTypeParametersAsTypeSignatures().ToList()[0], @object));
				}
				return fixedValue;
			}
			if (signature.Base.Equals(StandardTypes.Map))
			{
				TypeSignature keySignature = signature.GetTypeParametersAsTypeSignatures().ToList()[0];
				TypeSignature valueSignature = signature.GetTypeParametersAsTypeSignatures().ToList()[1];
				IDictionary<object, object> fixedValue = new Dictionary<object, object>();
				foreach (var entry in ((IDictionary<object, object>)value).ToList())
				{
					fixedValue[FixValue(keySignature, entry.Key)] = FixValue(valueSignature, entry.Value);
				}
				return fixedValue;
			}
			if (signature.Base.Equals(StandardTypes.Row))
			{
				IDictionary<string, object> fixedValue = new Dictionary<string, object>();
				IList<object> listValue = (IList<object>)value;
				ParameterCondition.CheckArgument(listValue.Count == signature.Parameters.Count(), "Mismatched data values and row type");
				for (int i = 0; i < listValue.Count; i++)
				{
					TypeSignatureParameter parameter = signature.Parameters.ToList()[i];
                    ParameterCondition.CheckArgument(parameter.Kind == ParameterKind.NamedType, "Unexpected parameter [%s] for row type", parameter);
					NamedTypeSignature namedTypeSignature = parameter.NamedTypeSignature;
					string key = string.IsNullOrWhiteSpace(namedTypeSignature.Name) ?"field" + i: namedTypeSignature.Name;
					fixedValue[key] = FixValue(namedTypeSignature.TypeSignature, listValue[i]);
				}
				return fixedValue;
			}
			switch (signature.Base)
			{
				case "bigint":
					if (value is string s)
					{
						return long.Parse(s);
					}
					return ((long) value);
				case "integer":
					if (value is string value1)
					{
						return int.Parse(value1);
					}
					return ((int) value);
				case "smallint":
					if (value is string s1)
					{
						return short.Parse(s1);
					}
					return Convert.ToInt16(value);
				case "tinyint":
					if (value is string o)
					{
						return sbyte.Parse(o);
					}
					return (sbyte) value;
				case "double":
					if (value is string s2)
					{
						return double.Parse(s2);
					}
					return (double) value;
				case "real":
					if (value is string value2)
					{
						return float.Parse(value2);
					}
					return (float) value;
				case "boolean":
					if (value is string s3)
					{
						return bool.Parse(s3);
					}
					return Convert.ToBoolean(value);
				case "varchar":
				case "json":
				case "time":
				case "time with time zone":
				case "timestamp":
				case "timestamp with time zone":
				case "date":
				case "interval year to month":
				case "interval day to second":
				case "ipaddress":
				case "ipprefix":
				case "decimal":
				case "char":
				case "Geometry":
					return value.ToString();
				case "BingTile":
					// Bing tiles are serialized as strings when used as map keys,
					// they are serialized as json otherwise (value will be a LinkedHashMap).
					return value;
				default:
					// for now we assume that only the explicit types above are passed
					// as a plain text and everything else is base64 encoded binary
					if (value is string value3)
					{
						return Convert.ToBase64String(Encoding.UTF8.GetBytes(value3));
					}
					return value;
			}
		}
	}

}