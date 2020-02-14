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
using SharpPulsar.Sql.Precondition;

namespace SharpPulsar.Sql.Facebook.Type
{
    public class RowFieldName
	{
		public string Name {get;}
		private readonly bool _delimited;

		public RowFieldName(string name, bool delimited)
		{
			this.Name = ParameterCondition.RequireNonNull(name, "name","name is null");
			this._delimited = delimited;
		}

		public virtual bool Delimited => _delimited;

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

			RowFieldName other = (RowFieldName) o;

			return object.Equals(this.Name, other.Name) && object.Equals(this._delimited, other._delimited);
		}

		public override string ToString()
		{
			if (!Delimited)
			{
				return Name;
			}
			return '"' + Name.Replace(@"""", @"""""") + '"';
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, _delimited);
		}
	}

}