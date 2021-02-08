using System.Collections.Generic;
using PulsarAdmin.Models;
using SharpPulsar.Admin;
using SharpPulsar.Akka;
using SharpPulsar.Messages;
using Xunit;
using Xunit.Abstractions;

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
namespace SharpPulsar.Test.Api
{
    public abstract class ProducerConsumerBase 
	{
		public void ProducerBaseSetup(PulsarSystem system, ITestOutputHelper output)
		{

            system.PulsarAdmin(new Admin(AdminCommands.CreateCluster, new object[] { "test", new ClusterData("http://localhost:8080") },
                (f) => { }, (e) => output.WriteLine(e.ToString()), "http://localhost:8080", l => { output.WriteLine(l); }));


            system.PulsarAdmin(new Admin(AdminCommands.CreateTenant, new object[] { "my-property", new TenantInfo(new List<string> { "appid1", "appid2" }, new List<string> { "test" }) },
                (f) => { }, (e) => output.WriteLine(e.ToString()), "http://localhost:8080", l => { output.WriteLine(l); }));

            system.PulsarAdmin(new Admin(AdminCommands.CreateNamespace, new object[] { "my-property", "my-ns", new Policies(replicationClusters: new List<string> { "test" }), },
                (f) => { }, (e) => output.WriteLine(e.ToString()), "http://localhost:8080", l => { output.WriteLine(l); }));
			
            system.PulsarAdmin(new Admin(AdminCommands.SetNamespaceReplicationClusters, new object[] { "my-property", "my-ns", new List<string> { "test" } },
                (f) => { }, (e) => output.WriteLine(e.ToString()), "http://localhost:8080", l => { output.WriteLine(l); }));

		}

		public void TestMessageOrderAndDuplicates<T>(ISet<T> messagesReceived, T receivedMessage, T expectedMessage)
		{
			// Make sure that messages are received in order
			Assert.True(receivedMessage.Equals(expectedMessage), "Received message " + receivedMessage + " did not match the expected message " + expectedMessage);

			// Make sure that there are no duplicates
			Assert.True(messagesReceived.Add(receivedMessage), "Received duplicate message " + receivedMessage);
		}

	}

}