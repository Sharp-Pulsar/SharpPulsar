using System.Text.Json.Serialization;
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
namespace SharpPulsar.Admin.Model
{
	

	public class OffloadPolicies
	{
        [JsonPropertyName("offloadersDirectory")]
        public string OffloadersDirectory { get; set; }

        [JsonPropertyName("managedLedgerOffloadDriver")]
        public string ManagedLedgerOffloadDriver { get; set; }

        [JsonPropertyName("managedLedgerOffloadMaxThreads")]
        public int? ManagedLedgerOffloadMaxThreads { get; set; }

        [JsonPropertyName("managedLedgerOffloadPrefetchRounds")]
        public int? ManagedLedgerOffloadPrefetchRounds { get; set; }

        [JsonPropertyName("managedLedgerOffloadThresholdInBytes")]
        public long? ManagedLedgerOffloadThresholdInBytes { get; set; }

        [JsonPropertyName("managedLedgerOffloadDeletionLagInMillis")]
        public long? ManagedLedgerOffloadDeletionLagInMillis { get; set; }

        [JsonPropertyName("managedLedgerOffloadedReadPriority")]
        public OffloadedReadPriority ManagedLedgerOffloadedReadPriority { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadRegion")]
        public string S3ManagedLedgerOffloadRegion { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadBucket")]
        public string S3ManagedLedgerOffloadBucket { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadServiceEndpoint")]
        public string S3ManagedLedgerOffloadServiceEndpoint { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadMaxBlockSizeInBytes")]
        public int? S3ManagedLedgerOffloadMaxBlockSizeInBytes { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadReadBufferSizeInBytes")]
        public int? S3ManagedLedgerOffloadReadBufferSizeInBytes { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadCredentialId")]
        public string S3ManagedLedgerOffloadCredentialId { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadCredentialSecret")]
        public string S3ManagedLedgerOffloadCredentialSecret { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadRole")]
        public string S3ManagedLedgerOffloadRole { get; set; }

        [JsonPropertyName("s3ManagedLedgerOffloadRoleSessionName")]
        public string S3ManagedLedgerOffloadRoleSessionName { get; set; }

        [JsonPropertyName("gcsManagedLedgerOffloadRegion")]
        public string GcsManagedLedgerOffloadRegion { get; set; }

        [JsonPropertyName("gcsManagedLedgerOffloadBucket")]
        public string GcsManagedLedgerOffloadBucket { get; set; }

        [JsonPropertyName("gcsManagedLedgerOffloadMaxBlockSizeInBytes")]
        public int? GcsManagedLedgerOffloadMaxBlockSizeInBytes { get; set; }

        [JsonPropertyName("gcsManagedLedgerOffloadReadBufferSizeInBytes")]
        public int? GcsManagedLedgerOffloadReadBufferSizeInBytes { get; set; }

        [JsonPropertyName("gcsManagedLedgerOffloadServiceAccountKeyFile")]
        public string GcsManagedLedgerOffloadServiceAccountKeyFile { get; set; }

        [JsonPropertyName("fileSystemProfilePath")]
        public string FileSystemProfilePath { get; set; }

        [JsonPropertyName("fileSystemURI")]
        public string FileSystemURI { get; set; }

        [JsonPropertyName("managedLedgerOffloadBucket")]
        public string ManagedLedgerOffloadBucket { get; set; }

        [JsonPropertyName("managedLedgerOffloadRegion")]
        public string ManagedLedgerOffloadRegion { get; set; }

        [JsonPropertyName("managedLedgerOffloadServiceEndpoint")]
        public string ManagedLedgerOffloadServiceEndpoint { get; set; }

        [JsonPropertyName("managedLedgerOffloadMaxBlockSizeInBytes")]
        public int? ManagedLedgerOffloadMaxBlockSizeInBytes { get; set; }

        [JsonPropertyName("managedLedgerOffloadReadBufferSizeInBytes")]
        public int? ManagedLedgerOffloadReadBufferSizeInBytes { get; set; }

    }

}