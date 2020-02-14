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

using System;
using Newtonsoft.Json;
using SharpPulsar.Sql.Precondition;

namespace SharpPulsar.Sql.Facebook.Type
{
    public class NamedTypeSignature
	{
		private readonly RowFieldName _fieldName;
		public  TypeSignature TypeSignature { get;}
        [JsonConstructor]
		public NamedTypeSignature(RowFieldName fieldName, TypeSignature typeSignature)
		{
			_fieldName = ParameterCondition.RequireNonNull(fieldName, "fieldName", "fieldName is null");
			TypeSignature = ParameterCondition.RequireNonNull(typeSignature, "typeSignature", "typeSignature is null");
		}

		public virtual RowFieldName FieldName => _fieldName;

        public virtual string Name => FieldName.Name;

        public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}
			if (o == null || GetType() != o.GetType())
			{
				return false;
			}

			NamedTypeSignature other = (NamedTypeSignature) o;

			return Equals(_fieldName, other._fieldName) && Equals(TypeSignature, other.TypeSignature);
		}

		public override string ToString()
		{
			if (_fieldName != null)
			{
				return string.Format("%s %s", _fieldName.Name, TypeSignature);
			}
			return TypeSignature.ToString();
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_fieldName, TypeSignature);
		}
	}

}