using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
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
namespace SharpPulsar.Util
{

	/// <summary>
	/// Class working with file's modified time.
	/// </summary>
	public class FileModifiedTimeUpdater
	{
		internal string fileName;
		internal DateTime lastModifiedTime;

		public FileModifiedTimeUpdater(string fileName)
		{
			this.fileName = fileName;
			this.lastModifiedTime = UpdateLastModifiedTime();
		}

		private DateTime UpdateLastModifiedTime()
		{
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				FileInfo p = new FileInfo(fileName);
				try
				{
					return p.LastWriteTimeUtc;
				}
				catch (IOException e)
				{
					log.LogError("Unable to fetch lastModified time for file {}: ", fileName, e);
				}
			}
			throw new Exception("Invalid file name");
		}

		public virtual bool CheckAndRefresh()
		{
			DateTime newLastModifiedTime = UpdateLastModifiedTime();
			if (newLastModifiedTime != null && !newLastModifiedTime.Equals(lastModifiedTime))
			{
				this.lastModifiedTime = newLastModifiedTime;
				return true;
			}
			return false;
		}

		private static readonly ILogger log = new LoggerFactory().CreateLogger<FileModifiedTimeUpdater>();
	}

}