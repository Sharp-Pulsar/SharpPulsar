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

namespace SharpPulsar.Presto

	public class NodeVersion
	{
		public static readonly NodeVersion Unknown = new NodeVersion("<unknown>");

		public string  Version {get;}
		public NodeVersion(string version)
		{
			this.Version = Condition.RequireNonNull(version, "version is null");
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

			NodeVersion that = (NodeVersion) o;
			return Objects.equals(Version, that.Version);
		}

		public override int GetHashCode()
		{
			return Objects.hash(Version);
		}

		public override string ToString()
		{
			return Version;
		}
	}

}