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
namespace Org.Apache.Pulsar.Client.Tutorial
{

	using Consumer = Org.Apache.Pulsar.Client.Api.Consumer;
	using CryptoKeyReader = Org.Apache.Pulsar.Client.Api.CryptoKeyReader;
	using EncryptionKeyInfo = Org.Apache.Pulsar.Client.Api.EncryptionKeyInfo;
	using Org.Apache.Pulsar.Client.Api;
	using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;

	using Slf4j = lombok.@extern.slf4j.Slf4j;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class SampleCryptoConsumer
	public class SampleCryptoConsumer
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws org.apache.pulsar.client.api.PulsarClientException, InterruptedException
		public static void Main(string[] Args)
		{

//JAVA TO C# CONVERTER TODO TASK: Local classes are not converted by Java to C# Converter:
//			class RawFileKeyReader implements org.apache.pulsar.client.api.CryptoKeyReader
	//		{
	//
	//			String publicKeyFile = "";
	//			String privateKeyFile = "";
	//
	//			RawFileKeyReader(String pubKeyFile, String privKeyFile)
	//			{
	//				publicKeyFile = pubKeyFile;
	//				privateKeyFile = privKeyFile;
	//			}
	//
	//			@@Override public EncryptionKeyInfo getPublicKey(String keyName, Map<String, String> keyMeta)
	//			{
	//
	//				// Read the public key from the file
	//				EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
	//				try
	//				{
	//					keyInfo.setKey(Files.readAllBytes(Paths.get(publicKeyFile)));
	//				}
	//				catch (IOException e)
	//				{
	//					log.error("Failed to read public key from file {}", publicKeyFile, e);
	//				}
	//				return keyInfo;
	//			}
	//
	//			@@Override public EncryptionKeyInfo getPrivateKey(String keyName, Map<String, String> keyMeta)
	//			{
	//
	//				// Read the private key from the file
	//				EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
	//				try
	//				{
	//					keyInfo.setKey(Files.readAllBytes(Paths.get(privateKeyFile)));
	//				}
	//				catch (IOException e)
	//				{
	//					log.error("Failed to read private key from file {}", privateKeyFile, e);
	//				}
	//				return keyInfo;
	//			}
	//		}

			// Create pulsar client
			PulsarClient PulsarClient = PulsarClient.builder().serviceUrl("http://127.0.0.1:8080").build();
			Consumer<sbyte[]> Consumer = PulsarClient.newConsumer().topic("persistent://my-tenant/my-ns/my-topic").subscriptionName("my-subscription-name").cryptoKeyReader(new RawFileKeyReader("test_ecdsa_pubkey.pem", "test_ecdsa_privkey.pem")).subscribe();

			Message<sbyte[]> Msg = null;

			for (int I = 0; I < 10; I++)
			{
				Msg = Consumer.receive();
				// process the messsage
				log.info("Received: {}", new string(Msg.Data));
			}

			// Acknowledge the consumption of all messages at once
			Consumer.acknowledgeCumulative(Msg);
			PulsarClient.Dispose();
		}
	}

}