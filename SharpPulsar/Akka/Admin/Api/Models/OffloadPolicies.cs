using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
namespace SharpPulsar.Akka.Admin.Api.Models
{
	//	import static org.apache.pulsar.common.util.FieldParser.value;
    /// <summary>
	/// Definition of the offload policies.
	/// </summary>
	public class OffloadPolicies
	{
        public const int DEFAULT_MAX_BLOCK_SIZE_IN_BYTES = 64 * 1024 * 1024; // 64MB
		public const int DEFAULT_READ_BUFFER_SIZE_IN_BYTES = 1024 * 1024; // 1MB
		public const int DEFAULT_OFFLOAD_MAX_THREADS = 2;
		public static readonly String[] DRIVER_NAMES = new String[] { "S3", "aws-s3", "google-cloud-storage", "filesystem" };
		public const String DEFAULT_OFFLOADER_DIRECTORY = "./offloaders";

		// common config
		private String offloadersDirectory = DEFAULT_OFFLOADER_DIRECTORY;
		private String managedLedgerOffloadDriver = null;
		private int managedLedgerOffloadMaxThreads = DEFAULT_OFFLOAD_MAX_THREADS;

		// s3 config, set by service configuration or cli
		private String s3ManagedLedgerOffloadRegion = null;
		private String s3ManagedLedgerOffloadBucket = null;
		private String s3ManagedLedgerOffloadServiceEndpoint = null;
		private int s3ManagedLedgerOffloadMaxBlockSizeInBytes = DEFAULT_MAX_BLOCK_SIZE_IN_BYTES;
		private int s3ManagedLedgerOffloadReadBufferSizeInBytes = DEFAULT_READ_BUFFER_SIZE_IN_BYTES;
		// s3 config, set by service configuration
		private String s3ManagedLedgerOffloadRole = null;
		private String s3ManagedLedgerOffloadRoleSessionName = "pulsar-s3-offload";

		// gcs config, set by service configuration or cli
		private String gcsManagedLedgerOffloadRegion = null;
		private String gcsManagedLedgerOffloadBucket = null;
		private int gcsManagedLedgerOffloadMaxBlockSizeInBytes = DEFAULT_MAX_BLOCK_SIZE_IN_BYTES;
		private int gcsManagedLedgerOffloadReadBufferSizeInBytes = DEFAULT_READ_BUFFER_SIZE_IN_BYTES;
		// gcs config, set by service configuration
		private String gcsManagedLedgerOffloadServiceAccountKeyFile = null;

		// file system config, set by service configuration
		private String fileSystemProfilePath = null;
		private String fileSystemURI = null;

		public static OffloadPolicies Create(string driver, string region, string bucket, string endpoint, int maxBlockSizeInBytes, int readBufferSizeInBytes)
        {
            var doc = LoadOffLoadPoliciesJava();
			if(!DRIVER_NAMES.Any(d => d.Equals(driver, StringComparison.OrdinalIgnoreCase)))
				throw new ArgumentException($"{driver} not supported");
            if (string.IsNullOrWhiteSpace(driver))
            {
                throw new ArgumentException($"Bucket is invalid: 'driver' is missing");
            }

            if (string.IsNullOrWhiteSpace(bucket))
            {
                throw new ArgumentException($"Bucket is invalid: 'bucket' is missing");
            }

            var isFileSystem = driver.Equals(DRIVER_NAMES[3], StringComparison.OrdinalIgnoreCase);
            var s3Driver = (driver.Equals(DRIVER_NAMES[0], StringComparison.OrdinalIgnoreCase) || driver.Equals(DRIVER_NAMES[1], StringComparison.OrdinalIgnoreCase));
            var gcsDriver = driver.Equals(DRIVER_NAMES[2], StringComparison.OrdinalIgnoreCase);
            if(!isFileSystem && !s3Driver && !gcsDriver)
                throw new ArgumentException($"Bucket is invalid: no supported driver found");

			OffloadPolicies offloadPolicies = new OffloadPolicies { managedLedgerOffloadDriver = driver };
            if (driver.Equals(DRIVER_NAMES[0], StringComparison.OrdinalIgnoreCase) || driver.Equals(DRIVER_NAMES[1], StringComparison.OrdinalIgnoreCase))
            {
				//replace lines in doc
                doc.Replace("private String s3ManagedLedgerOffloadRegion = null;",
                    $"private String s3ManagedLedgerOffloadRegion = \"{region}\";");
                offloadPolicies.s3ManagedLedgerOffloadRegion = region;
                offloadPolicies.s3ManagedLedgerOffloadBucket = bucket;
                offloadPolicies.s3ManagedLedgerOffloadServiceEndpoint = endpoint;
                offloadPolicies.s3ManagedLedgerOffloadMaxBlockSizeInBytes = maxBlockSizeInBytes;
                offloadPolicies.s3ManagedLedgerOffloadReadBufferSizeInBytes = readBufferSizeInBytes;
            }
            else if (driver.Equals(DRIVER_NAMES[2], StringComparison.OrdinalIgnoreCase))
            {
                offloadPolicies.gcsManagedLedgerOffloadRegion = region;
                offloadPolicies.gcsManagedLedgerOffloadBucket = bucket;
                offloadPolicies.gcsManagedLedgerOffloadMaxBlockSizeInBytes = maxBlockSizeInBytes;
                offloadPolicies.gcsManagedLedgerOffloadReadBufferSizeInBytes = readBufferSizeInBytes;
            }
			return offloadPolicies;
		}

        private static string LoadOffLoadPoliciesJava()
        {
            return File.ReadAllText("");
        }

	}
}
