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

namespace SharpPulsar.Presto
{
    using Duration = io.airlift.units.Duration;

	public class ServerInfo
	{
		public virtual NodeVersion NodeVersion {get;}
		public  string Environment {get;}
		private readonly bool coordinator;
		private readonly bool starting;

		// optional to maintain compatibility with older servers
		private readonly Optional<Duration> uptime;

		public ServerInfo(NodeVersion nodeVersion, string environment, bool coordinator, bool starting, Optional<Duration> uptime)
		{
			this.NodeVersion = requireNonNull(nodeVersion, "nodeVersion is null");
			this.Environment = requireNonNull(environment, "environment is null");
			this.coordinator = coordinator;
			this.starting = starting;
			this.uptime = requireNonNull(uptime, "uptime is null");
		}

		public virtual bool Coordinator
		{
			get
			{
				return coordinator;
			}
		}

		public virtual bool Starting
		{
			get
			{
				return starting;
			}
		}

		public virtual Optional<Duration> Uptime
		{
			get
			{
				return uptime;
			}
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

			ServerInfo that = (ServerInfo) o;
			return Objects.equals(NodeVersion, that.NodeVersion) && Objects.equals(Environment, that.Environment);
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