using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

	public class ClientTypeSignature
	{
		private static readonly Regex Pattern = new Regex(".*[<>,].*");
		public virtual string RawType {get;}
		private readonly IList<ClientTypeSignatureParameter> _arguments;

		public ClientTypeSignature(TypeSignature typeSignature) : this(typeSignature.Base, new List<ClientTypeSignatureParameter>())
		{
		}

		public ClientTypeSignature(string rawType, IList<ClientTypeSignatureParameter> arguments) : this(rawType, new List<ClientTypeSignature>(), new List<object>(), arguments)
		{
			
		}

		public ClientTypeSignature(string rawType, IList<ClientTypeSignature> typeArguments, IList<object> literalArguments, IList<ClientTypeSignatureParameter> arguments)
		{
			ParameterCondition.RequireNonNull(rawType, "rawType", "rawType is null");
			this.RawType = rawType;
			ParameterCondition.CheckArgument(rawType.Length > 0, "rawType is empty");
            ParameterCondition.CheckArgument(!Pattern.IsMatch(rawType), "Bad characters in rawType type: %s", rawType);
			if (arguments != null)
			{
				this._arguments = new List<ClientTypeSignatureParameter>(arguments);
			}
			else
			{
				ParameterCondition.RequireNonNull(typeArguments, "typeArguments", "typeArguments is null");
                ParameterCondition.RequireNonNull(literalArguments, "literalArguments", "literalArguments is null");
				List<ClientTypeSignatureParameter> convertedArguments = new List<ClientTypeSignatureParameter>();
				// Talking to a legacy server (< 0.133)
				if (rawType.Equals(StandardTypes.Row))
				{
					ParameterCondition.CheckArgument(typeArguments.Count == literalArguments.Count);
					for (int i = 0; i < typeArguments.Count; i++)
					{
						object value = literalArguments[i];
						ParameterCondition.CheckArgument(value is string, "Expected literalArgument %d in %s to be a string", i, literalArguments);
						convertedArguments.Add(new ClientTypeSignatureParameter(new TypeSignatureParameter(new NamedTypeSignature((new RowFieldName((string) value, false)), ToTypeSignature(typeArguments[i])))));
					}
				}
				else
				{
					checkArgument(literalArguments.Count == 0, "Unexpected literal arguments from legacy server");
					foreach (ClientTypeSignature typeArgument in typeArguments)
					{
						convertedArguments.add(new ClientTypeSignatureParameter(ParameterKind.Type, typeArgument));
					}
				}
				this._arguments = convertedArguments.build();
			}
		}

		private static TypeSignature ToTypeSignature(ClientTypeSignature signature)
		{
			IList<TypeSignatureParameter> parameters = signature.Arguments.Select(ClientTypeSignature.legacyClientTypeSignatureParameterToTypeSignatureParameter).ToList();
			return new TypeSignature(signature.RawType, parameters);
		}

		private static TypeSignatureParameter LegacyClientTypeSignatureParameterToTypeSignatureParameter(ClientTypeSignatureParameter parameter)
		{
			switch (parameter.Kind)
			{
				case LONG:
					throw new System.NotSupportedException("Unexpected long type literal returned by legacy server");
				case TYPE:
					return TypeSignatureParameter.of(ToTypeSignature(parameter.TypeSignature));
				case NAMED_TYPE:
					return TypeSignatureParameter.of(parameter.NamedTypeSignature);
				default:
					throw new System.NotSupportedException("Unknown parameter kind " + parameter.Kind);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getRawType()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public java.util.List<ClientTypeSignatureParameter> getArguments()
		public virtual IList<ClientTypeSignatureParameter> Arguments
		{
			get
			{
				return _arguments;
			}
		}

		/// <summary>
		/// This field is deprecated and clients should switch to <seealso cref="getArguments()"/>
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @JsonProperty public java.util.List<ClientTypeSignature> getTypeArguments()
		[Obsolete]
		public virtual IList<ClientTypeSignature> TypeArguments
		{
			get
			{
				IList<ClientTypeSignature> result = new List<ClientTypeSignature>();
				foreach (ClientTypeSignatureParameter argument in _arguments)
				{
					switch (argument.Kind)
					{
						case TYPE:
							result.Add(argument.TypeSignature);
							break;
						case NAMED_TYPE:
							result.Add(new ClientTypeSignature(argument.NamedTypeSignature.TypeSignature));
							break;
						default:
							return new List<ClientTypeSignature>();
					}
				}
				return result;
			}
		}

		/// <summary>
		/// This field is deprecated and clients should switch to <seealso cref="getArguments()"/>
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @JsonProperty public java.util.List<Object> getLiteralArguments()
		[Obsolete]
		public virtual IList<object> LiteralArguments
		{
			get
			{
				IList<object> result = new List<object>();
				foreach (ClientTypeSignatureParameter argument in _arguments)
				{
					switch (argument.Kind)
					{
						case NAMED_TYPE:
							result.Add(argument.NamedTypeSignature.Name);
							break;
						default:
							return new List<object>();
					}
				}
				return result;
			}
		}

		public override string ToString()
		{
			if (RawType.Equals(StandardTypes.ROW))
			{
				return RowToString();
			}
			else
			{
				StringBuilder typeName = new StringBuilder(RawType);
				if (_arguments.Count > 0)
				{
					typeName.Append("(");
					bool first = true;
					foreach (ClientTypeSignatureParameter argument in _arguments)
					{
						if (!first)
						{
							typeName.Append(",");
						}
						first = false;
						typeName.Append(argument.ToString());
					}
					typeName.Append(")");
				}
				return typeName.ToString();
			}
		}

		[Obsolete]
		private string RowToString()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			string fields = _arguments.Select(ClientTypeSignatureParameter::getNamedTypeSignature).Select(parameter =>
			{
			if (parameter.Name.Present)
			{
				return format("%s %s", parameter.Name.get(), parameter.TypeSignature.ToString());
			}
			return parameter.TypeSignature.ToString();
			}).collect(Collectors.joining(","));

			return format("row(%s)", fields);
		}

		public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}
			if (o == null || this.GetType() != o.GetType())
			{
				return false;
			}

			ClientTypeSignature other = (ClientTypeSignature) o;

			return Objects.equals(this.RawType.ToLower(Locale.ENGLISH), other.RawType.ToLower(Locale.ENGLISH)) && Objects.equals(this._arguments, other._arguments);
		}

		public override int GetHashCode()
		{
			return Objects.hash(RawType.ToLower(Locale.ENGLISH), _arguments);
		}
	}

}