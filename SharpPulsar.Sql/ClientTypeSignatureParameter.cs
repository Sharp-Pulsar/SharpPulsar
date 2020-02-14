using System;
using Newtonsoft.Json;
using SharpPulsar.Sql.Facebook.Type;
using Type = System.Type;

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
	public class ClientTypeSignatureParameter
	{
		public  ParameterKind Kind {get;}
		public object Value {get;}
        [JsonConstructor]
		public ClientTypeSignatureParameter(TypeSignatureParameter typeParameterSignature)
		{
			this.Kind = typeParameterSignature.Kind;
			switch (Kind)
			{
				case ParameterKind.Type:
					Value = new ClientTypeSignature(typeParameterSignature.TypeSignature);
					break;
				case ParameterKind.Long:
					Value = typeParameterSignature.LongLiteral;
					break;
				case ParameterKind.NamedType:
					Value = typeParameterSignature.NamedTypeSignature;
					break;
				default:
					throw new NotSupportedException(string.Format("Unknown kind [%s]", Kind));
			}
		}

		public ClientTypeSignatureParameter(ParameterKind kind, object value)
		{
			this.Kind = kind;
			this.Value = value;
		}


		private TA GetValue<TA>(ParameterKind expectedParameterKind, Type target)
		{
			if (Kind != expectedParameterKind)
			{
				throw new System.ArgumentException(string.Format("ParameterKind is [%s] but expected [%s]", Kind, expectedParameterKind));
			}
			return (TA)Convert.ChangeType(Value, target);
		}

		public virtual ClientTypeSignature TypeSignature => GetValue<ClientTypeSignature>(ParameterKind.Type, typeof(ClientTypeSignature));

        public virtual long? LongLiteral => GetValue<long>(ParameterKind.Long, typeof(long));

        public virtual NamedTypeSignature NamedTypeSignature => GetValue<NamedTypeSignature>(ParameterKind.NamedType, typeof(NamedTypeSignature));

        public override string ToString()
		{
			return Value.ToString();
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

			ClientTypeSignatureParameter other = (ClientTypeSignatureParameter) o;

			return object.Equals(this.Kind, other.Kind) && object.Equals(this.Value, other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Kind, Value);
		}

		/*public class ClientTypeSignatureParameterDeserializer : JsonDeserializer<ClientTypeSignatureParameter>
		{
			internal static readonly ObjectMapper Mapper = new ObjectMapperProvider().get();

			public override ClientTypeSignatureParameter Deserialize(JsonParser jp, DeserializationContext ctxt)
			{
				JsonNode node = jp.Codec.readTree(jp);
				ParameterKind kind = Mapper.readValue(Mapper.treeAsTokens(node.get("kind")), typeof(ParameterKind));
				JsonParser jsonValue = Mapper.treeAsTokens(node.get("value"));
				object value;
				switch (kind)
				{
					case TYPE:
						value = Mapper.readValue(jsonValue, typeof(ClientTypeSignature));
						break;
					case NAMED_TYPE:
						value = Mapper.readValue(jsonValue, typeof(NamedTypeSignature));
						break;
					case LONG:
						value = Mapper.readValue(jsonValue, typeof(Long));
						break;
					default:
						throw new System.NotSupportedException(format("Unsupported kind [%s]", kind));
				}
				return new ClientTypeSignatureParameter(kind, value);
			}
		}*/
	}

}