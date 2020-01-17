/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace org.apache.pulsar.common.util
{
	using Getter = lombok.Getter;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// Class working with file's modified time.
	/// </summary>
	public class FileModifiedTimeUpdater
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter String fileName;
		internal string fileName;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter FileTime lastModifiedTime;
		internal FileTime lastModifiedTime;

		public FileModifiedTimeUpdater(string fileName)
		{
			this.fileName = fileName;
			this.lastModifiedTime = updateLastModifiedTime();
		}

		private FileTime updateLastModifiedTime()
		{
			if (!string.ReferenceEquals(fileName, null))
			{
				Path p = Paths.get(fileName);
				try
				{
					return Files.getLastModifiedTime(p);
				}
				catch (IOException e)
				{
					LOG.error("Unable to fetch lastModified time for file {}: ", fileName, e);
				}
			}
			return null;
		}

		public virtual bool checkAndRefresh()
		{
			FileTime newLastModifiedTime = updateLastModifiedTime();
			if (newLastModifiedTime != null && !newLastModifiedTime.Equals(lastModifiedTime))
			{
				this.lastModifiedTime = newLastModifiedTime;
				return true;
			}
			return false;
		}

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(FileModifiedTimeUpdater));
	}

}