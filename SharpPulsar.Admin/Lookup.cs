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
namespace Org.Apache.Pulsar.Client.Admin
{
	/// <summary>
	/// This is an interface class to allow using command line tool to quickly lookup the broker serving the topic.
	/// </summary>
	public interface Lookup
	{

		/// <summary>
		/// Lookup a topic
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> the broker URL that serves the topic </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String lookupTopic(String topic) throws PulsarAdminException;
		string LookupTopic(string Topic);

		/// <summary>
		/// Get a bundle range of a topic
		/// </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getBundleRange(String topic) throws PulsarAdminException;
		string GetBundleRange(string Topic);
	}

}