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
		public readonly string FileName;
		public  DateTime LastModifiedTime;

		public FileModifiedTimeUpdater(string fileName)
		{
			this.FileName = fileName;
			this.LastModifiedTime = UpdateLastModifiedTime();
		}

		private DateTime UpdateLastModifiedTime()
		{
			if (!string.IsNullOrWhiteSpace(FileName))
			{
				var p = new FileInfo(FileName);
				try
				{
					return p.LastWriteTimeUtc;
				}
				catch (IOException e)
				{
					Log.LogError("Unable to fetch lastModified time for file {}: ", FileName, e);
				}
			}
			throw new Exception("Invalid file name");
		}

		public virtual bool CheckAndRefresh()
		{
			var newLastModifiedTime = UpdateLastModifiedTime();
			if (!newLastModifiedTime.Equals(LastModifiedTime))
			{
				this.LastModifiedTime = newLastModifiedTime;
				return true;
			}
			return false;
		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger<FileModifiedTimeUpdater>();
	}

}