using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharpPulsar.Sql.Facebook.Type;

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
namespace SharpPulsar.Sql.Operator
{

	public class TypeSignatureParser
	{
		private static readonly Regex IdentifierPattern = new Regex("[a-zA-Z_]([a-zA-Z0-9_:@])*");
		private static readonly ISet<string> SimpleTypeWithSpaces = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

		static TypeSignatureParser()
		{
			SimpleTypeWithSpaces.Add(StandardTypes.TimeWithTimeZone);
			SimpleTypeWithSpaces.Add(StandardTypes.TimestampWithTimeZone);
			SimpleTypeWithSpaces.Add(StandardTypes.IntervalDayToSecond);
			SimpleTypeWithSpaces.Add(StandardTypes.IntervalYearToMonth);
			SimpleTypeWithSpaces.Add("double precision");
		}

		private TypeSignatureParser()
		{
		}

		public static TypeSignature ParseTypeSignature(string signature, ISet<string> literalCalculationParameters)
		{
			if (!signature.Contains("<") && !signature.Contains("("))
			{
				if (signature.Equals(StandardTypes.Varchar, StringComparison.OrdinalIgnoreCase))
				{
					return VarcharType.CreateUnboundedVarcharType().TypeSignature;
				}
				checkArgument(!literalCalculationParameters.Contains(signature), "Bad type signature: '%s'", signature);
				return new TypeSignature(signature, new List<>());
			}
			if (signature.ToLower(Locale.ENGLISH).StartsWith(StandardTypes.ROW + "(", StringComparison.Ordinal))
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
				if (c == '(')
				{
					if (bracketCount == 0)
					{
						verify(string.ReferenceEquals(baseName, null), "Expected baseName to be null");
						verify(parameterStart == -1, "Expected parameter start to be -1");
						baseName = signature.Substring(0, i);
						checkArgument(!literalCalculationParameters.Contains(baseName), "Bad type signature: '%s'", signature);
						parameterStart = i + 1;
					}
					bracketCount++;
				}
				else if (c == ')')
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
			TYPE,
			FINISHED,
		}

		private static TypeSignature ParseRowTypeSignature(string Signature, ISet<string> LiteralParameters)
		{
			checkArgument(Signature.ToLower(Locale.ENGLISH).StartsWith(StandardTypes.ROW + "(", StringComparison.Ordinal), "Not a row type signature: '%s'", Signature);

			RowTypeSignatureParsingState State = RowTypeSignatureParsingState.StartOfField;
			int BracketLevel = 1;
			int TokenStart = -1;
			string DelimitedColumnName = null;

			IList<TypeSignatureParameter> Fields = new List<TypeSignatureParameter>();

			for (int I = StandardTypes.ROW.length() + 1; I < Signature.Length; I++)
			{
				char C = Signature[I];
				switch (State)
				{
					case TypeSignatureParser.RowTypeSignatureParsingState.StartOfField:
						if (C == '"')
						{
							State = RowTypeSignatureParsingState.DelimitedName;
							TokenStart = I;
						}
						else if (IsValidStartOfIdentifier(C))
						{
							State = RowTypeSignatureParsingState.TypeOrNamedType;
							TokenStart = I;
						}
						else
						{
							checkArgument(C == ' ', "Bad type signature: '%s'", Signature);
						}
						break;

					case TypeSignatureParser.RowTypeSignatureParsingState.DelimitedName:
						if (C == '"')
						{
							if (I + 1 < Signature.Length && Signature[I + 1] == '"')
							{
								State = RowTypeSignatureParsingState.DelimitedNameEscaped;
							}
							else
							{
								// Remove quotes around the delimited column name
								verify(TokenStart >= 0, "Expect tokenStart to be non-negative");
								DelimitedColumnName = Signature.Substring(TokenStart + 1, I - (TokenStart + 1));
								TokenStart = I + 1;
								State = RowTypeSignatureParsingState.TYPE;
							}
						}
						break;

					case TypeSignatureParser.RowTypeSignatureParsingState.DelimitedNameEscaped:
						verify(C == '"', "Expect quote after escape");
						State = RowTypeSignatureParsingState.DelimitedName;
						break;

					case TypeSignatureParser.RowTypeSignatureParsingState.TypeOrNamedType:
						if (C == '(')
						{
							BracketLevel++;
						}
						else if (C == ')' && BracketLevel > 1)
						{
							BracketLevel--;
						}
						else if (C == ')')
						{
							verify(TokenStart >= 0, "Expect tokenStart to be non-negative");
							Fields.Add(ParseTypeOrNamedType(Signature.Substring(TokenStart, I - TokenStart).Trim(), LiteralParameters));
							TokenStart = -1;
							State = RowTypeSignatureParsingState.FINISHED;
						}
						else if (C == ',' && BracketLevel == 1)
						{
							verify(TokenStart >= 0, "Expect tokenStart to be non-negative");
							Fields.Add(ParseTypeOrNamedType(Signature.Substring(TokenStart, I - TokenStart).Trim(), LiteralParameters));
							TokenStart = -1;
							State = RowTypeSignatureParsingState.StartOfField;
						}
						break;

					case TypeSignatureParser.RowTypeSignatureParsingState.TYPE:
						if (C == '(')
						{
							BracketLevel++;
						}
						else if (C == ')' && BracketLevel > 1)
						{
							BracketLevel--;
						}
						else if (C == ')')
						{
							verify(TokenStart >= 0, "Expect tokenStart to be non-negative");
							verify(!string.ReferenceEquals(DelimitedColumnName, null), "Expect delimitedColumnName to be non-null");
							Fields.Add(TypeSignatureParameter.namedTypeParameter(new NamedTypeSignature((new RowFieldName(DelimitedColumnName)), ParseTypeSignature(Signature.Substring(TokenStart, I - TokenStart).Trim(), LiteralParameters))));
							DelimitedColumnName = null;
							TokenStart = -1;
							State = RowTypeSignatureParsingState.FINISHED;
						}
						else if (C == ',' && BracketLevel == 1)
						{
							verify(TokenStart >= 0, "Expect tokenStart to be non-negative");
							verify(!string.ReferenceEquals(DelimitedColumnName, null), "Expect delimitedColumnName to be non-null");
							Fields.Add(TypeSignatureParameter.namedTypeParameter(new NamedTypeSignature((new RowFieldName(DelimitedColumnName)), ParseTypeSignature(Signature.Substring(TokenStart, I - TokenStart).Trim(), LiteralParameters))));
							DelimitedColumnName = null;
							TokenStart = -1;
							State = RowTypeSignatureParsingState.StartOfField;
						}
						break;

					case TypeSignatureParser.RowTypeSignatureParsingState.FINISHED:
						throw new System.InvalidOperationException(format("Bad type signature: '%s'", Signature));

					default:
						throw new AssertionError(format("Unexpected RowTypeSignatureParsingState: %s", State));
				}
			}

			checkArgument(State == RowTypeSignatureParsingState.FINISHED, "Bad type signature: '%s'", Signature);
			return new TypeSignature(Signature.Substring(0, StandardTypes.ROW.length()), Fields);
		}

		private static TypeSignatureParameter ParseTypeOrNamedType(string TypeOrNamedType, ISet<string> LiteralParameters)
		{
			int Split = TypeOrNamedType.IndexOf(' ');

			// Type without space or simple type with spaces
			if (Split == -1 || SimpleTypeWithSpaces.Contains(TypeOrNamedType))
			{
				return TypeSignatureParameter.namedTypeParameter(new NamedTypeSignature(null, ParseTypeSignature(TypeOrNamedType, LiteralParameters)));
			}

			// Assume the first part of a structured type always has non-alphabetical character.
			// If the first part is a valid identifier, parameter is a named field.
			string FirstPart = TypeOrNamedType.Substring(0, Split);
			if (IdentifierPattern.matcher(FirstPart).matches())
			{
				return TypeSignatureParameter.namedTypeParameter(new NamedTypeSignature((new RowFieldName(FirstPart)), ParseTypeSignature(TypeOrNamedType.Substring(Split + 1).Trim(), LiteralParameters)));
			}

			// Structured type composed from types with spaces. i.e. array(timestamp with time zone)
			return TypeSignatureParameter.namedTypeParameter(new NamedTypeSignature(null, ParseTypeSignature(TypeOrNamedType, LiteralParameters)));
		}

		private static TypeSignatureParameter ParseTypeSignatureParameter(string Signature, int Begin, int End, ISet<string> LiteralCalculationParameters)
		{
			string ParameterName = Signature.Substring(Begin, End - Begin).Trim();
			if (isDigit(Signature[Begin]))
			{
				return TypeSignatureParameter.numericParameter(long.Parse(ParameterName));
			}
			else if (LiteralCalculationParameters.Contains(ParameterName))
			{
				return TypeSignatureParameter.typeVariable(ParameterName);
			}
			else
			{
				return typeParameter(ParseTypeSignature(ParameterName, LiteralCalculationParameters));
			}
		}

		private static bool IsValidStartOfIdentifier(char C)
		{
			return (C >= 'a' && C <= 'z') || (C >= 'A' && C <= 'Z') || C == '_';
		}
	}

}