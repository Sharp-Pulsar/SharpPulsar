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
	using Type = com.facebook.presto.spi.type.Type;
	using TypeSignature = com.facebook.presto.spi.type.TypeSignature;
	using JsonCreator = com.fasterxml.jackson.annotation.JsonCreator;
	using JsonProperty = com.fasterxml.jackson.annotation.JsonProperty;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class Column
	public class Column
	{
		public virtual Name {get;}
		public virtual Type {get;}
		public virtual TypeSignature {get;}

		public Column(string Name, Type Type) : this(Name, Type.TypeSignature)
		{
		}

		public Column(string Name, TypeSignature Signature) : this(Name, Signature.ToString(), new ClientTypeSignature(Signature))
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public Column(@JsonProperty("name") String name, @JsonProperty("type") String type, @JsonProperty("typeSignature") ClientTypeSignature typeSignature)
		public Column(string Name, string Type, ClientTypeSignature TypeSignature)
		{
			this.Name = requireNonNull(Name, "name is null");
			this.Type = requireNonNull(Type, "type is null");
			this.TypeSignature = TypeSignature;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getName()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getType()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public ClientTypeSignature getTypeSignature()
	}

}