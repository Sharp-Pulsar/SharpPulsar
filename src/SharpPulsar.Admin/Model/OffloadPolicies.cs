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
	using ReflectionUtils = org.apache.pulsar.client.admin.utils.ReflectionUtils;

	public interface OffloadPolicies
	{
		string OffloadersDirectory {get;}

		string ManagedLedgerOffloadDriver {get;}

		int? ManagedLedgerOffloadMaxThreads {get;}

		int? ManagedLedgerOffloadPrefetchRounds {get;}

		long? ManagedLedgerOffloadThresholdInBytes {get;}

		long? ManagedLedgerOffloadDeletionLagInMillis {get;}

		OffloadedReadPriority ManagedLedgerOffloadedReadPriority {get;}

		string S3ManagedLedgerOffloadRegion {get;}

		string S3ManagedLedgerOffloadBucket {get;}

		string S3ManagedLedgerOffloadServiceEndpoint {get;}

		int? S3ManagedLedgerOffloadMaxBlockSizeInBytes {get;}

		int? S3ManagedLedgerOffloadReadBufferSizeInBytes {get;}

		string S3ManagedLedgerOffloadCredentialId {get;}

		string S3ManagedLedgerOffloadCredentialSecret {get;}

		string S3ManagedLedgerOffloadRole {get;}

		string S3ManagedLedgerOffloadRoleSessionName {get;}

		string GcsManagedLedgerOffloadRegion {get;}

		string GcsManagedLedgerOffloadBucket {get;}

		int? GcsManagedLedgerOffloadMaxBlockSizeInBytes {get;}

		int? GcsManagedLedgerOffloadReadBufferSizeInBytes {get;}

		string GcsManagedLedgerOffloadServiceAccountKeyFile {get;}

		string FileSystemProfilePath {get;}

		string FileSystemURI {get;}

		string ManagedLedgerOffloadBucket {get;}

		string ManagedLedgerOffloadRegion {get;}

		string ManagedLedgerOffloadServiceEndpoint {get;}

		int? ManagedLedgerOffloadMaxBlockSizeInBytes {get;}

		int? ManagedLedgerOffloadReadBufferSizeInBytes {get;}

		static OffloadPoliciesBuilder Builder()
		{
			return ReflectionUtils.newBuilder("SharpPulsar.Admin.Model.OffloadPoliciesImpl");
		}
	}

	public interface OffloadPoliciesBuilder
	{

		OffloadPoliciesBuilder OffloadersDirectory(string offloadersDirectory);

		OffloadPoliciesBuilder ManagedLedgerOffloadDriver(string managedLedgerOffloadDriver);

		OffloadPoliciesBuilder ManagedLedgerOffloadMaxThreads(int? managedLedgerOffloadMaxThreads);

		OffloadPoliciesBuilder ManagedLedgerOffloadPrefetchRounds(int? managedLedgerOffloadPrefetchRounds);

		OffloadPoliciesBuilder ManagedLedgerOffloadThresholdInBytes(long? managedLedgerOffloadThresholdInBytes);

		OffloadPoliciesBuilder ManagedLedgerOffloadDeletionLagInMillis(long? managedLedgerOffloadDeletionLagInMillis);

		OffloadPoliciesBuilder ManagedLedgerOffloadedReadPriority(OffloadedReadPriority managedLedgerOffloadedReadPriority);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadRegion(string s3ManagedLedgerOffloadRegion);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadBucket(string s3ManagedLedgerOffloadBucket);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadServiceEndpoint(string s3ManagedLedgerOffloadServiceEndpoint);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadMaxBlockSizeInBytes(int? s3ManagedLedgerOffloadMaxBlockSizeInBytes);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadReadBufferSizeInBytes(int? s3ManagedLedgerOffloadReadBufferSizeInBytes);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadCredentialId(string s3ManagedLedgerOffloadCredentialId);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadCredentialSecret(string s3ManagedLedgerOffloadCredentialSecret);

		OffloadPoliciesBuilder S3ManagedLedgerOffloadRole(string s3ManagedLedgerOffloadRole);

		OffloadPoliciesBuilder setS3ManagedLedgerOffloadRoleSessionName(string s3ManagedLedgerOffloadRoleSessionName);

		OffloadPoliciesBuilder GcsManagedLedgerOffloadRegion(string gcsManagedLedgerOffloadRegion);

		OffloadPoliciesBuilder GcsManagedLedgerOffloadBucket(string gcsManagedLedgerOffloadBucket);

		OffloadPoliciesBuilder GcsManagedLedgerOffloadMaxBlockSizeInBytes(int? gcsManagedLedgerOffloadMaxBlockSizeInBytes);

		OffloadPoliciesBuilder GcsManagedLedgerOffloadReadBufferSizeInBytes(int? gcsManagedLedgerOffloadReadBufferSizeInBytes);

		OffloadPoliciesBuilder GcsManagedLedgerOffloadServiceAccountKeyFile(string gcsManagedLedgerOffloadServiceAccountKeyFile);

		OffloadPoliciesBuilder FileSystemProfilePath(string fileSystemProfilePath);

		OffloadPoliciesBuilder FileSystemURI(string fileSystemURI);

		OffloadPoliciesBuilder ManagedLedgerOffloadBucket(string managedLedgerOffloadBucket);

		OffloadPoliciesBuilder ManagedLedgerOffloadRegion(string managedLedgerOffloadRegion);

		OffloadPoliciesBuilder ManagedLedgerOffloadServiceEndpoint(string managedLedgerOffloadServiceEndpoint);

		OffloadPoliciesBuilder ManagedLedgerOffloadMaxBlockSizeInBytes(int? managedLedgerOffloadMaxBlockSizeInBytes);

		OffloadPoliciesBuilder ManagedLedgerOffloadReadBufferSizeInBytes(int? managedLedgerOffloadReadBufferSizeInBytes);

		OffloadPolicies Build();
	}

}