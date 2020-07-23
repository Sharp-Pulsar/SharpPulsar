using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SharpPulsar.Precondition;

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
namespace SharpPulsar.Presto.Facebook.Type
{

	public class TypeSignature
	{
		public string Base { get; }

		private List<TypeSignatureParameter> _parameters;
		private bool _calculated;
		private static readonly Regex IdentifierPattern = new Regex("[a-zA-Z_]([a-zA-Z0-9_:@])*");
		private static readonly Dictionary<string, string> BaseNameAliasToCanonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private static readonly ISet<string> SimpleTypeWithSpaces = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);

		static TypeSignature()
		{
			BaseNameAliasToCanonical["int"] = StandardTypes.Integer;

			SimpleTypeWithSpaces.Add(StandardTypes.TimeWithTimeZone);
			SimpleTypeWithSpaces.Add(StandardTypes.TimestampWithTimeZone);
			SimpleTypeWithSpaces.Add(StandardTypes.IntervalDayToSecond);
			SimpleTypeWithSpaces.Add(StandardTypes.IntervalYearToMonth);
			SimpleTypeWithSpaces.Add("double precision");
		}

		public TypeSignature(string @base, params TypeSignatureParameter[] parameters) : this(@base, parameters.ToList())
		{
		}

		public TypeSignature(string @base, IList<TypeSignatureParameter> parameters)
		{
			checkArgument(!string.ReferenceEquals(@base, null), "base is null");
			this.Base = @base;
			checkArgument(@base.Length > 0, "base is empty");
			checkArgument(validateName(@base), "Bad characters in base type: %s", @base);
			checkArgument(parameters != null, "parameters is null");
			_parameters = new List<TypeSignatureParameter>(parameters);

			_calculated = parameters.Any(TypeSignatureParameter.IsCalculated);
		}


		public virtual IList<TypeSignatureParameter> Parameters
		{
			get
			{
				return _parameters;
			}
		}

		public virtual IList<TypeSignature> TypeParametersAsTypeSignatures
		{
			get
			{
				IList<TypeSignature> result = new List<TypeSignature>();
				foreach (TypeSignatureParameter parameter in _parameters)
				{
					if (parameter.Kind != ParameterKind.Type)
					{
						throw new System.InvalidOperationException(format("Expected all parameters to be TypeSignatures but [%s] was found", parameter.ToString()));
					}
					result.Add(parameter.TypeSignature);
				}
				return result;
			}
		}

		public virtual bool Calculated => _calculated;

		public static TypeSignature ParseTypeSignature(string signature)
		{
			return ParseTypeSignature(signature, new HashSet<string>());
		}

		public static TypeSignature ParseTypeSignature(string signature, ISet<string> literalCalculationParameters)
		{
			if (!signature.Contains("<") && !signature.Contains("("))
			{
				if (signature.Equals(StandardTypes.Varchar, StringComparison.OrdinalIgnoreCase))
				{
					return VarcharType.CreateUnboundedVarcharType().GetTypeSignature();
				}
				checkArgument(!literalCalculationParameters.Contains(signature), "Bad type signature: '%s'", signature);
				return new TypeSignature(canonicalizeBaseName(signature), new List<>());
			}
			if (signature.ToLower(Locale.ENGLISH).StartsWith(StandardTypes.Row + "(", StringComparison.Ordinal))
			{
				return ParseRowTypeSignature(signature, literalCalculationParameters);
			}

			string baseName = null;
			IList<TypeSignatureParameter> parameters = new List<TypeSignatureParameter>();
			int parameterStart = -1;
			int bracketCount = 0;

			for (int i = 0; i < signature.Length; i++)
			{
				char c = signature[i];
				// TODO: remove angle brackets support once ROW<TYPE>(name) will be dropped
				// Angle brackets here are checked not for the support of ARRAY<> and MAP<>
				// but to correctly parse ARRAY(row<BIGINT, BIGINT>('a','b'))
				if (c == '(' || c == '<')
				{
					if (bracketCount == 0)
					{
						verify(string.ReferenceEquals(baseName, null), "Expected baseName to be null");
						verify(parameterStart == -1, "Expected parameter start to be -1");
						baseName = canonicalizeBaseName(signature.Substring(0, i));
						checkArgument(!literalCalculationParameters.Contains(baseName), "Bad type signature: '%s'", signature);
						parameterStart = i + 1;
					}
					bracketCount++;
				}
				else if (c == ')' || c == '>')
				{
					bracketCount--;
					checkArgument(bracketCount >= 0, "Bad type signature: '%s'", signature);
					if (bracketCount == 0)
					{
						checkArgument(parameterStart >= 0, "Bad type signature: '%s'", signature);
						parameters.Add(ParseTypeSignatureParameter(signature, parameterStart, i, literalCalculationParameters));
						parameterStart = i + 1;
						if (i == signature.Length - 1)
						{
							return new TypeSignature(baseName, parameters);
						}
					}
				}
				else if (c == ',')
				{
					if (bracketCount == 1)
					{
						checkArgument(parameterStart >= 0, "Bad type signature: '%s'", signature);
						parameters.Add(ParseTypeSignatureParameter(signature, parameterStart, i, literalCalculationParameters));
						parameterStart = i + 1;
					}
				}
			}

			throw new System.ArgumentException(format("Bad type signature: '%s'", signature));
		}

		public enum RowTypeSignatureParsingState
		{
			StartOfField,
			DelimitedName,
			DelimitedNameEscaped,
			TypeOrNamedType,
			Type,
			Finished,
		}

		private static TypeSignature ParseRowTypeSignature(string signature, ISet<string> literalParameters)
		{
			Condition.CheckArgument(signature.ToLower(Locale.ENGLISH).StartsWith(StandardTypes.Row + "(", StringComparison.Ordinal), "Not a row type signature: '%s'", signature);

			RowTypeSignatureParsingState state = RowTypeSignatureParsingState.StartOfField;
			int bracketLevel = 1;
			int tokenStart = -1;
			string delimitedColumnName = null;

			IList<TypeSignatureParameter> fields = new List<TypeSignatureParameter>();

			for (int i = StandardTypes.Row.Length + 1; i < signature.Length; i++)
			{
				char c = signature[i];
				switch (state)
				{
					case TypeSignature.RowTypeSignatureParsingState.StartOfField:
						if (c == '"')
						{
							state = RowTypeSignatureParsingState.DelimitedName;
							tokenStart = i;
						}
						else if (isValidStartOfIdentifier(c))
						{
							state = RowTypeSignatureParsingState.TypeOrNamedType;
							tokenStart = i;
						}
						else
						{
							checkArgument(c == ' ', "Bad type signature: '%s'", signature);
						}
						break;

					case TypeSignature.RowTypeSignatureParsingState.DelimitedName:
						if (c == '"')
						{
							if (i + 1 < signature.Length && signature[i + 1] == '"')
							{
								state = RowTypeSignatureParsingState.DelimitedNameEscaped;
							}
							else
							{
								// Remove quotes around the delimited column name
								verify(tokenStart >= 0, "Expect tokenStart to be non-negative");
								delimitedColumnName = signature.Substring(tokenStart + 1, i - (tokenStart + 1));
								tokenStart = i + 1;
								state = RowTypeSignatureParsingState.Type;
							}
						}
						break;

					case TypeSignature.RowTypeSignatureParsingState.DelimitedNameEscaped:
						verify(c == '"', "Expect quote after escape");
						state = RowTypeSignatureParsingState.DelimitedName;
						break;

					case TypeSignature.RowTypeSignatureParsingState.TypeOrNamedType:
						if (c == '(')
						{
							bracketLevel++;
						}
						else if (c == ')' && bracketLevel > 1)
						{
							bracketLevel--;
						}
						else if (c == ')')
						{
							verify(tokenStart >= 0, "Expect tokenStart to be non-negative");
							fields.Add(parseTypeOrNamedType(signature.Substring(tokenStart, i - tokenStart).Trim(), literalParameters));
							tokenStart = -1;
							state = RowTypeSignatureParsingState.Finished;
						}
						else if (c == ',' && bracketLevel == 1)
						{
							verify(tokenStart >= 0, "Expect tokenStart to be non-negative");
							fields.Add(parseTypeOrNamedType(signature.Substring(tokenStart, i - tokenStart).Trim(), literalParameters));
							tokenStart = -1;
							state = RowTypeSignatureParsingState.StartOfField;
						}
						break;

					case TypeSignature.RowTypeSignatureParsingState.Type:
						if (c == '(')
						{
							bracketLevel++;
						}
						else if (c == ')' && bracketLevel > 1)
						{
							bracketLevel--;
						}
						else if (c == ')')
						{
							verify(tokenStart >= 0, "Expect tokenStart to be non-negative");
							verify(!string.ReferenceEquals(delimitedColumnName, null), "Expect delimitedColumnName to be non-null");
							fields.Add(TypeSignatureParameter.Of(new NamedTypeSignature((new RowFieldName(delimitedColumnName, true)), ParseTypeSignature(signature.Substring(tokenStart, i - tokenStart).Trim(), literalParameters))));
							delimitedColumnName = null;
							tokenStart = -1;
							state = RowTypeSignatureParsingState.Finished;
						}
						else if (c == ',' && bracketLevel == 1)
						{
							verify(tokenStart >= 0, "Expect tokenStart to be non-negative");
							verify(!string.ReferenceEquals(delimitedColumnName, null), "Expect delimitedColumnName to be non-null");
							fields.Add(TypeSignatureParameter.Of(new NamedTypeSignature((new RowFieldName(delimitedColumnName, true)), ParseTypeSignature(signature.Substring(tokenStart, i - tokenStart).Trim(), literalParameters))));
							delimitedColumnName = null;
							tokenStart = -1;
							state = RowTypeSignatureParsingState.StartOfField;
						}
						break;

					case TypeSignature.RowTypeSignatureParsingState.Finished:
						throw new System.InvalidOperationException(format("Bad type signature: '%s'", signature));

					default:
						throw new AssertionError(format("Unexpected RowTypeSignatureParsingState: %s", state));
				}
			}

			checkArgument(state == RowTypeSignatureParsingState.Finished, "Bad type signature: '%s'", signature);
			return new TypeSignature(signature.Substring(0, StandardTypes.Row.Length), fields);
		}

		private static TypeSignatureParameter ParseTypeOrNamedType(string typeOrNamedType, ISet<string> literalParameters)
		{
			int split = typeOrNamedType.IndexOf(' ');

			// Type without space or simple type with spaces
			if (split == -1 || SimpleTypeWithSpaces.Contains(typeOrNamedType))
			{
				return TypeSignatureParameter.Of(new NamedTypeSignature(null, ParseTypeSignature(typeOrNamedType, literalParameters)));
			}

			// Assume the first part of a structured type always has non-alphabetical character.
			// If the first part is a valid identifier, parameter is a named field.
			string firstPart = typeOrNamedType.Substring(0, split);
			if (IdentifierPattern.Matches(firstPart).Count > 0)
			{
				return TypeSignatureParameter.Of(new NamedTypeSignature((new RowFieldName(firstPart, false)), ParseTypeSignature(typeOrNamedType.Substring(split + 1).Trim(), literalParameters)));
			}

			// Structured type composed from types with spaces. i.e. array(timestamp with time zone)
			return TypeSignatureParameter.Of(new NamedTypeSignature(null, ParseTypeSignature(typeOrNamedType, literalParameters)));
		}

		private static TypeSignatureParameter ParseTypeSignatureParameter(string signature, int begin, int end, ISet<string> literalCalculationParameters)
		{
			string parameterName = signature.Substring(begin, end - begin).Trim();
			if (isDigit(signature[begin]))
			{
				return TypeSignatureParameter.Of(long.Parse(parameterName));
			}
			else if (literalCalculationParameters.Contains(parameterName))
			{
				return TypeSignatureParameter.Of(parameterName);
			}
			else
			{
				return TypeSignatureParameter.Of(ParseTypeSignature(parameterName, literalCalculationParameters));
			}
		}

		private static bool isValidStartOfIdentifier(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		}

		public override string ToString()
		{
			if (Base.Equals(StandardTypes.Row))
			{
				return "";
			}
			else if (Base.Equals(StandardTypes.Varchar) &&
					 Parameters.Count() == 1 &&
					 Parameters.ElementAt(0).IsLongLiteral && Parameters.ElementAt(0).LongLiteral == VarcharType.UnboundedLength
			)
			{
				return Base;
			}
			else
			{
				var typeName = new StringBuilder(Base);

				if (Parameters.Any())
				{
					typeName.Append($"({String.Join(",", Parameters.Select(x => x.ToString()))})");
				}

				return typeName.ToString();
			}
		}

		private static void checkArgument(bool argument, string format, params object[] args)
		{
			if (!argument)
			{
				throw new System.ArgumentException(format(format, args));
			}
		}

		private static void verify(bool argument, string message)
		{
			if (!argument)
			{
				throw new AssertionError(message);
			}
		}

		private static bool validateName(string name)
		{
			return name.chars().noneMatch(c => c == '<' || c == '>' || c == ',');
		}

		private static string canonicalizeBaseName(string baseName)
		{
			string canonicalBaseName = BaseNameAliasToCanonical[baseName];
			if (string.ReferenceEquals(canonicalBaseName, null))
			{
				return baseName;
			}
			return canonicalBaseName;
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

			TypeSignature other = (TypeSignature)o;

			return Objects.equals(this.Base.ToLower(Locale.ENGLISH), other.Base.ToLower(Locale.ENGLISH)) && Objects.equals(this._parameters, other._parameters);
		}

		public override int GetHashCode()
		{
			return Objects.hash(Base.ToLower(Locale.ENGLISH), _parameters);
		}
	}
}
