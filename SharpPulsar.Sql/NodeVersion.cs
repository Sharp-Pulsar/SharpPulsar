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
	using JsonCreator = com.fasterxml.jackson.annotation.JsonCreator;
	using JsonProperty = com.fasterxml.jackson.annotation.JsonProperty;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class NodeVersion
	public class NodeVersion
	{
		public static readonly NodeVersion UNKNOWN = new NodeVersion("<unknown>");

		public virtual Version {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public NodeVersion(@JsonProperty("version") String version)
		public NodeVersion(string Version)
		{
			this.Version = requireNonNull(Version, "version is null");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getVersion()

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

			NodeVersion That = (NodeVersion) O;
			return Objects.equals(Version, That.Version);
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