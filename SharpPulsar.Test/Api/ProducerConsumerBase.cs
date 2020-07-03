using System.Collections.Generic;
using Xunit;

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
		public virtual void producerBaseSetup()
		{
			admin.clusters().createCluster("test", new ClusterData(org.apache.pulsar.WebServiceAddress));
			admin.tenants().createTenant("my-property", new TenantInfo(Sets.newHashSet("appid1", "appid2"), Sets.newHashSet("test")));
			admin.namespaces().createNamespace("my-property/my-ns");
			admin.namespaces().setNamespaceReplicationClusters("my-property/my-ns", Sets.newHashSet("test"));

			// so that clients can test short names
			admin.tenants().createTenant("public", new TenantInfo(Sets.newHashSet("appid1", "appid2"), Sets.newHashSet("test")));
			admin.namespaces().createNamespace("public/default");
			admin.namespaces().setNamespaceReplicationClusters("public/default", Sets.newHashSet("test"));
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