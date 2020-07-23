using System;
using System.Text;
using System.Text.RegularExpressions;

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
    public enum Type
    {
        Role,
        All,
        None
    }
	public class SelectedRole
	{

        private static readonly Regex Pattern = new Regex(@"(ROLE|ALL|NONE)(\{(.+?)\})?");

		public  Type Type {get;}
		private readonly string _role;
		public SelectedRole(Type type, string role)
		{
			if(type == null)
				throw new NullReferenceException( "type is null");
			_role = ParameterCondition.RequireNonNull(role,"Role", "role is null");
			if (type == Type.Role && !string.IsNullOrWhiteSpace(role))
			{
				throw new ArgumentException("Role must be present for the selected role type: " + type);
			}
		}

		public Type? GetToleType()
		{
			return Type;
		}

		public string Role => _role;

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
			var that = (SelectedRole) o;
			return Type == that.Type && Equals(_role, that._role);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, _role);
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			result.Append(Type);
            if (!string.IsNullOrWhiteSpace(_role))
                result.Append("{").Append(_role).Append("}");
			return result.ToString();
		}

		public static SelectedRole ValueOf(string value)
		{
			var m = Pattern.Match(value);
			if (m.Success)
			{
				var type = Enum.Parse(typeof(Type), m.Groups[1].Value);
				var role = m.Groups[3].Value;
				return new SelectedRole((Type)type, role);
			}
			throw new ArgumentException("Could not parse selected role: " + value);
		}
	}

}