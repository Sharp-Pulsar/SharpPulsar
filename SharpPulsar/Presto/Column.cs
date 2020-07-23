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

using SharpPulsar.Precondition;
using SharpPulsar.Presto.Facebook.Type;

namespace SharpPulsar.Presto
{
    public class Column
	{
		public string Name {get;}
		public string Type {get;}
		public ClientTypeSignature TypeSignature {get;}

		public Column(string name, IType type) : this(name, type.GetTypeSignature())
		{
		}

		public Column(string name, TypeSignature signature) : this(name, signature.ToString(), new ClientTypeSignature(signature))
		{
		}

		public Column(string name, string type, ClientTypeSignature typeSignature)
		{
			this.Name = Condition.RequireNonNull(name, "name", "name is null");
			this.Type = Condition.RequireNonNull(type, "type", "type is null");
			this.TypeSignature = typeSignature;
		}

	}

}