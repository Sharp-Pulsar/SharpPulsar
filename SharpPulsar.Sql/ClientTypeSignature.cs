using System;
using System.Collections.Generic;
using System.Text;

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
	using RowFieldName = com.facebook.presto.spi.type.RowFieldName;
	using StandardTypes = com.facebook.presto.spi.type.StandardTypes;
	using TypeSignature = com.facebook.presto.spi.type.TypeSignature;
	using TypeSignatureParameter = com.facebook.presto.spi.type.TypeSignatureParameter;
	using JsonCreator = com.fasterxml.jackson.annotation.JsonCreator;
	using JsonProperty = com.fasterxml.jackson.annotation.JsonProperty;
	using ImmutableList = com.google.common.collect.ImmutableList;
	using Lists = com.google.common.collect.Lists;

	public class ClientTypeSignature
	{
		private static readonly Pattern PATTERN = Pattern.compile(".*[<>,].*");
		public virtual string RawType {get;}
		private readonly IList<ClientTypeSignatureParameter> arguments;

		public ClientTypeSignature(TypeSignature TypeSignature) : this(TypeSignature.Base, Lists.transform(TypeSignature.Parameters, ClientTypeSignatureParameter::new))
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		}

		public ClientTypeSignature(string RawType, IList<ClientTypeSignatureParameter> Arguments) : this(RawType, ImmutableList.of(), ImmutableList.of(), Arguments)
		{
		}

		public ClientTypeSignature(string RawType, IList<ClientTypeSignature> TypeArguments, IList<object> LiteralArguments, IList<ClientTypeSignatureParameter> Arguments)
		{
			requireNonNull(RawType, "rawType is null");
			this.RawType = RawType;
			checkArgument(RawType.Length > 0, "rawType is empty");
			checkArgument(!PATTERN.matcher(RawType).matches(), "Bad characters in rawType type: %s", RawType);
			if (Arguments != null)
			{
				this.arguments = unmodifiableList(new List<>(Arguments));
			}
			else
			{
				requireNonNull(TypeArguments, "typeArguments is null");
				requireNonNull(LiteralArguments, "literalArguments is null");
				ImmutableList.Builder<ClientTypeSignatureParameter> ConvertedArguments = ImmutableList.builder();
				// Talking to a legacy server (< 0.133)
				if (RawType.Equals(StandardTypes.ROW))
				{
					checkArgument(TypeArguments.Count == LiteralArguments.Count);
					for (int I = 0; I < TypeArguments.Count; I++)
					{
						object Value = LiteralArguments[I];
						checkArgument(Value is string, "Expected literalArgument %d in %s to be a string", I, LiteralArguments);
						ConvertedArguments.add(new ClientTypeSignatureParameter(TypeSignatureParameter.of(new NamedTypeSignature((new RowFieldName((string) Value, false)), ToTypeSignature(TypeArguments[I])))));
					}
				}
				else
				{
					checkArgument(LiteralArguments.Count == 0, "Unexpected literal arguments from legacy server");
					foreach (ClientTypeSignature TypeArgument in TypeArguments)
					{
						ConvertedArguments.add(new ClientTypeSignatureParameter(ParameterKind.TYPE, TypeArgument));
					}
				}
				this.arguments = ConvertedArguments.build();
			}
		}

		private static TypeSignature ToTypeSignature(ClientTypeSignature Signature)
		{
			IList<TypeSignatureParameter> Parameters = Signature.Arguments.Select(ClientTypeSignature.legacyClientTypeSignatureParameterToTypeSignatureParameter).ToList();
			return new TypeSignature(Signature.RawType, Parameters);
		}

		private static TypeSignatureParameter LegacyClientTypeSignatureParameterToTypeSignatureParameter(ClientTypeSignatureParameter Parameter)
		{
			switch (Parameter.Kind)
			{
				case LONG:
					throw new System.NotSupportedException("Unexpected long type literal returned by legacy server");
				case TYPE:
					return TypeSignatureParameter.of(ToTypeSignature(Parameter.TypeSignature));
				case NAMED_TYPE:
					return TypeSignatureParameter.of(Parameter.NamedTypeSignature);
				default:
					throw new System.NotSupportedException("Unknown parameter kind " + Parameter.Kind);
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
				return arguments;
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
				IList<ClientTypeSignature> Result = new List<ClientTypeSignature>();
				foreach (ClientTypeSignatureParameter Argument in arguments)
				{
					switch (Argument.Kind)
					{
						case TYPE:
							Result.Add(Argument.TypeSignature);
							break;
						case NAMED_TYPE:
							Result.Add(new ClientTypeSignature(Argument.NamedTypeSignature.TypeSignature));
							break;
						default:
							return new List<ClientTypeSignature>();
					}
				}
				return Result;
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
				IList<object> Result = new List<object>();
				foreach (ClientTypeSignatureParameter Argument in arguments)
				{
					switch (Argument.Kind)
					{
						case NAMED_TYPE:
							Result.Add(Argument.NamedTypeSignature.Name);
							break;
						default:
							return new List<object>();
					}
				}
				return Result;
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
				StringBuilder TypeName = new StringBuilder(RawType);
				if (arguments.Count > 0)
				{
					TypeName.Append("(");
					bool First = true;
					foreach (ClientTypeSignatureParameter Argument in arguments)
					{
						if (!First)
						{
							TypeName.Append(",");
						}
						First = false;
						TypeName.Append(Argument.ToString());
					}
					TypeName.Append(")");
				}
				return TypeName.ToString();
			}
		}

		[Obsolete]
		private string RowToString()
		{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			string Fields = arguments.Select(ClientTypeSignatureParameter::getNamedTypeSignature).Select(parameter =>
			{
			if (parameter.Name.Present)
			{
				return format("%s %s", parameter.Name.get(), parameter.TypeSignature.ToString());
			}
			return parameter.TypeSignature.ToString();
			}).collect(Collectors.joining(","));

			return format("row(%s)", Fields);
		}

		public override bool Equals(object O)
		{
			if (this == O)
			{
				return true;
			}
			if (O == null || this.GetType() != O.GetType())
			{
				return false;
			}

			ClientTypeSignature Other = (ClientTypeSignature) O;

			return Objects.equals(this.RawType.ToLower(Locale.ENGLISH), Other.RawType.ToLower(Locale.ENGLISH)) && Objects.equals(this.arguments, Other.arguments);
		}

		public override int GetHashCode()
		{
			return Objects.hash(RawType.ToLower(Locale.ENGLISH), arguments);
		}
	}

}