using System;
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
namespace SharpPulsar.Sql.Facebook.Type
{


	public class TypeSignatureParameter
	{
		public ParameterKind Kind {get;}
		private readonly object _value;

		public static TypeSignatureParameter Of(TypeSignature typeSignature)
		{
			return new TypeSignatureParameter(ParameterKind.Type, typeSignature);
		}

		public static TypeSignatureParameter Of(long longLiteral)
		{
			return new TypeSignatureParameter(ParameterKind.Long, longLiteral);
		}

		public static TypeSignatureParameter Of(NamedTypeSignature namedTypeSignature)
		{
			return new TypeSignatureParameter(ParameterKind.NamedType, namedTypeSignature);
		}

		public static TypeSignatureParameter Of(string variable)
		{
			return new TypeSignatureParameter(ParameterKind.Variable, variable);
		}

		private TypeSignatureParameter(ParameterKind kind, object value)
		{
			if(kind == null)
				throw  new NullReferenceException("kind is null");
			this.Kind = kind;
			this._value = ParameterCondition.RequireNonNull(value, "value is null");
		}

		public override string ToString()
		{
			return _value.ToString();
		}


		public virtual bool IsTypeSignature => Kind == ParameterKind.Type;

        public virtual bool IsLongLiteral => Kind == ParameterKind.Long;

        public virtual bool IsNamedTypeSignature => Kind == ParameterKind.NamedType;

        public virtual bool IsVariable => Kind == ParameterKind.Variable;

        private TA GetValue<TA>(ParameterKind expectedParameterKind, System.Type target)
		{
			if (Kind != expectedParameterKind)
			{
				throw new ArgumentException(string.Format("ParameterKind is [%s] but expected [%s]", Kind, expectedParameterKind));
			}
			return (TA)Convert.ChangeType(_value, target);
		}

		public virtual TypeSignature TypeSignature => GetValue<TypeSignature>(ParameterKind.Type, typeof(TypeSignature));

        public virtual long? LongLiteral => GetValue<long>(ParameterKind.Long, typeof(long));

        public virtual NamedTypeSignature NamedTypeSignature => GetValue<NamedTypeSignature>(ParameterKind.NamedType, typeof(NamedTypeSignature));

        public virtual string Variable => GetValue<string>(ParameterKind.Variable, typeof(string));

        public virtual TypeSignature TypeSignatureOrNamedTypeSignature
		{
			get
			{
				switch (Kind)
				{
					case ParameterKind.Type:
						return TypeSignature;
					case ParameterKind.NamedType:
						return NamedTypeSignature.TypeSignature;
					default:
						return null;
				}
			}
		}

		public virtual bool Calculated
		{
			get
			{
				switch (Kind)
				{
					case ParameterKind.Type:
						return TypeSignature.Calculated;
					case ParameterKind.NamedType:
						return NamedTypeSignature.TypeSignature.Calculated;
					case ParameterKind.Long:
						return false;
					case ParameterKind.Variable:
						return true;
					default:
						throw new ArgumentException("Unexpected parameter kind: " + Kind);
				}
			}
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

			TypeSignatureParameter other = (TypeSignatureParameter) o;

			return object.Equals(this.Kind, other.Kind) && object.Equals(this._value, other._value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Kind, _value);
		}
	}

}