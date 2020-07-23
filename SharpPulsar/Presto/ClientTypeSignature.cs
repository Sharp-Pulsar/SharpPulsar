using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SharpPulsar.Precondition;
using SharpPulsar.Presto.Facebook.Type;
using SharpPulsar.Sql;

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
namespace SharpPulsar.Presto
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
			Condition.RequireNonNull(rawType, "rawType", "rawType is null");
			this.RawType = rawType;
			Condition.CheckArgument(rawType.Length > 0, "rawType is empty");
            Condition.CheckArgument(!Pattern.IsMatch(rawType), "Bad characters in rawType type: %s", rawType);
			if (arguments != null)
			{
				this._arguments = new List<ClientTypeSignatureParameter>(arguments);
			}
			else
			{
				Condition.RequireNonNull(typeArguments, "typeArguments", "typeArguments is null");
                Condition.RequireNonNull(literalArguments, "literalArguments", "literalArguments is null");
				List<ClientTypeSignatureParameter> convertedArguments = new List<ClientTypeSignatureParameter>();
				// Talking to a legacy server (< 0.133)
				if (rawType.Equals(StandardTypes.Row))
				{
					Condition.CheckArgument(typeArguments.Count == literalArguments.Count);
					for (int i = 0; i < typeArguments.Count; i++)
					{
						object value = literalArguments[i];
						Condition.CheckArgument(value is string, "Expected literalArgument %d in %s to be a string", i, literalArguments);
                        convertedArguments.Add(new ClientTypeSignatureParameter(TypeSignatureParameter.Of(new NamedTypeSignature(
                            new RowFieldName((string)value, false),
                            ToTypeSignature(typeArguments[i])))));
					}
				}
				else
				{
                    Condition.CheckArgument(literalArguments.Count == 0, "Unexpected literal arguments from legacy server");
					foreach (ClientTypeSignature typeArgument in typeArguments)
					{
						convertedArguments.Add(new ClientTypeSignatureParameter(ParameterKind.Type, typeArgument));
					}
				}
				this._arguments = convertedArguments;
			}
		}

		private static TypeSignature ToTypeSignature(ClientTypeSignature signature)
		{
			IList<TypeSignatureParameter> parameters = signature.Arguments.ToList().Select(LegacyClientTypeSignatureParameterToTypeSignatureParameter).ToList();
			return new TypeSignature(signature.RawType, parameters);
		}

		private static TypeSignatureParameter LegacyClientTypeSignatureParameterToTypeSignatureParameter(ClientTypeSignatureParameter parameter)
		{
			switch (parameter.Kind)
			{
				case ParameterKind.Long:
					throw new System.NotSupportedException("Unexpected long type literal returned by legacy server");
				case ParameterKind.Type:
					return TypeSignatureParameter.Of(ToTypeSignature(parameter.TypeSignature));
				case ParameterKind.NamedType:
					return TypeSignatureParameter.Of(parameter.NamedTypeSignature);
				default:
					throw new System.NotSupportedException("Unknown parameter kind " + parameter.Kind);
			}
		}

		public virtual IList<ClientTypeSignatureParameter> Arguments => _arguments;

        /// <summary>
		/// This field is deprecated and clients should switch to <seealso cref="getArguments()"/>
		/// </summary>
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
						case ParameterKind.Type:
							result.Add(argument.TypeSignature);
							break;
						case ParameterKind.NamedType:
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
						case ParameterKind.NamedType:
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
			if (RawType.Equals(StandardTypes.Row))
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
			var fields = _arguments.Select(x => x.NamedTypeSignature).Select(parameter =>
            {
                return !string.IsNullOrWhiteSpace(parameter.Name) ? string.Format("%s %s", parameter.Name, parameter.TypeSignature.ToString()) : parameter.TypeSignature.ToString();
            });

			return $"row {string.Join(",", fields)}";
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

			return object.Equals(this.RawType.ToLower(CultureInfo.GetCultureInfo("en-US")), other.RawType.ToLower(CultureInfo.GetCultureInfo("en-US"))) && object.Equals(this._arguments, other._arguments);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(RawType.ToLower(CultureInfo.GetCultureInfo("en-US")), _arguments);
		}
	}

}