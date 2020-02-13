using System;

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
	using ObjectMapperProvider = com.facebook.airlift.json.ObjectMapperProvider;
	using NamedTypeSignature = com.facebook.presto.spi.type.NamedTypeSignature;
	using ParameterKind = com.facebook.presto.spi.type.ParameterKind;
	using TypeSignatureParameter = com.facebook.presto.spi.type.TypeSignatureParameter;
	using JsonCreator = com.fasterxml.jackson.annotation.JsonCreator;
	using JsonProperty = com.fasterxml.jackson.annotation.JsonProperty;
	using JsonParser = com.fasterxml.jackson.core.JsonParser;
	using DeserializationContext = com.fasterxml.jackson.databind.DeserializationContext;
	using JsonDeserializer = com.fasterxml.jackson.databind.JsonDeserializer;
	using JsonNode = com.fasterxml.jackson.databind.JsonNode;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using JsonDeserialize = com.fasterxml.jackson.databind.annotation.JsonDeserialize;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable @JsonDeserialize(using = ClientTypeSignatureParameter.ClientTypeSignatureParameterDeserializer.class) public class ClientTypeSignatureParameter
	public class ClientTypeSignatureParameter
	{
		public virtual Kind {get;}
		public virtual Value {get;}

		public ClientTypeSignatureParameter(TypeSignatureParameter TypeParameterSignature)
		{
			this.Kind = TypeParameterSignature.Kind;
			switch (Kind)
			{
				case TYPE:
					Value = new ClientTypeSignature(TypeParameterSignature.TypeSignature);
					break;
				case LONG:
					Value = TypeParameterSignature.LongLiteral;
					break;
				case NAMED_TYPE:
					Value = TypeParameterSignature.NamedTypeSignature;
					break;
				default:
					throw new System.NotSupportedException(format("Unknown kind [%s]", Kind));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public ClientTypeSignatureParameter(@JsonProperty("kind") com.facebook.presto.spi.type.ParameterKind kind, @JsonProperty("value") Object value)
		public ClientTypeSignatureParameter(ParameterKind Kind, object Value)
		{
			this.Kind = Kind;
			this.Value = Value;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public com.facebook.presto.spi.type.ParameterKind getKind()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public Object getValue()

		private A getValue<A>(ParameterKind ExpectedParameterKind, Type Target)
		{
			if (Kind != ExpectedParameterKind)
			{
				throw new System.ArgumentException(format("ParameterKind is [%s] but expected [%s]", Kind, ExpectedParameterKind));
			}
			return Target.cast(Value);
		}

		public virtual ClientTypeSignature TypeSignature
		{
			get
			{
				return GetValue(ParameterKind.TYPE, typeof(ClientTypeSignature));
			}
		}

		public virtual long? LongLiteral
		{
			get
			{
				return GetValue(ParameterKind.LONG, typeof(Long));
			}
		}

		public virtual NamedTypeSignature NamedTypeSignature
		{
			get
			{
				return GetValue(ParameterKind.NAMED_TYPE, typeof(NamedTypeSignature));
			}
		}

		public override string ToString()
		{
			return Value.ToString();
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

			ClientTypeSignatureParameter Other = (ClientTypeSignatureParameter) O;

			return Objects.equals(this.Kind, Other.Kind) && Objects.equals(this.Value, Other.Value);
		}

		public override int GetHashCode()
		{
			return Objects.hash(Kind, Value);
		}

		public class ClientTypeSignatureParameterDeserializer : JsonDeserializer<ClientTypeSignatureParameter>
		{
			internal static readonly ObjectMapper MAPPER = new ObjectMapperProvider().get();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public ClientTypeSignatureParameter deserialize(com.fasterxml.jackson.core.JsonParser jp, com.fasterxml.jackson.databind.DeserializationContext ctxt) throws java.io.IOException
			public override ClientTypeSignatureParameter Deserialize(JsonParser Jp, DeserializationContext Ctxt)
			{
				JsonNode Node = Jp.Codec.readTree(Jp);
				ParameterKind Kind = MAPPER.readValue(MAPPER.treeAsTokens(Node.get("kind")), typeof(ParameterKind));
				JsonParser JsonValue = MAPPER.treeAsTokens(Node.get("value"));
				object Value;
				switch (Kind)
				{
					case TYPE:
						Value = MAPPER.readValue(JsonValue, typeof(ClientTypeSignature));
						break;
					case NAMED_TYPE:
						Value = MAPPER.readValue(JsonValue, typeof(NamedTypeSignature));
						break;
					case LONG:
						Value = MAPPER.readValue(JsonValue, typeof(Long));
						break;
					default:
						throw new System.NotSupportedException(format("Unsupported kind [%s]", Kind));
				}
				return new ClientTypeSignatureParameter(Kind, Value);
			}
		}
	}

}