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
	using Duration = io.airlift.units.Duration;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.MoreObjects.toStringHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class ServerInfo
	public class ServerInfo
	{
		public virtual NodeVersion {get;}
		public virtual Environment {get;}
		private readonly bool coordinator;
		private readonly bool starting;

		// optional to maintain compatibility with older servers
		private readonly Optional<Duration> uptime;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public ServerInfo(@JsonProperty("nodeVersion") NodeVersion nodeVersion, @JsonProperty("environment") String environment, @JsonProperty("coordinator") boolean coordinator, @JsonProperty("starting") boolean starting, @JsonProperty("uptime") java.util.Optional<io.airlift.units.Duration> uptime)
		public ServerInfo(NodeVersion NodeVersion, string Environment, bool Coordinator, bool Starting, Optional<Duration> Uptime)
		{
			this.NodeVersion = requireNonNull(NodeVersion, "nodeVersion is null");
			this.Environment = requireNonNull(Environment, "environment is null");
			this.coordinator = Coordinator;
			this.starting = Starting;
			this.uptime = requireNonNull(Uptime, "uptime is null");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public NodeVersion getNodeVersion()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getEnvironment()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public boolean isCoordinator()
		public virtual bool Coordinator
		{
			get
			{
				return coordinator;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public boolean isStarting()
		public virtual bool Starting
		{
			get
			{
				return starting;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public java.util.Optional<io.airlift.units.Duration> getUptime()
		public virtual Optional<Duration> Uptime
		{
			get
			{
				return uptime;
			}
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

			ServerInfo That = (ServerInfo) O;
			return Objects.equals(NodeVersion, That.NodeVersion) && Objects.equals(Environment, That.Environment);
		}

		public override int GetHashCode()
		{
			return Objects.hash(NodeVersion, Environment);
		}

		public override string ToString()
		{
			return toStringHelper(this).add("nodeVersion", NodeVersion).add("environment", Environment).add("coordinator", coordinator).add("uptime", uptime.orElse(null)).omitNullValues().ToString();
		}
	}

}