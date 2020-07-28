using System.Threading.Tasks;
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
namespace SharpPulsar.Api.Transaction
{

	/// <summary>
	/// The builder to build a transaction for Pulsar.
	/// </summary>
	public interface TransactionBuilder
	{

		/// <summary>
		/// Configure the maximum amount of time that the transaction
		/// coordinator will for a transaction to be completed by the
		/// client before proactively aborting the ongoing transaction.
		/// 
		/// <para>The config value will be sent to the transaction coordinator
		/// along with the CommandNewTxn. Default is 60 seconds.
		/// 
		/// </para>
		/// </summary>
		/// <param name="timeout"> the transaction timeout value </param>
		/// <param name="timeoutUnit"> the transaction timeout unit </param>
		/// <returns> the transaction builder itself </returns>
		TransactionBuilder WithTransactionTimeout(long timeout);

		/// <summary>
		/// Build the transaction with the configured settings.
		/// </summary>
		/// <returns> a future represents the result of starting a new transaction </returns>
		ValueTask<ITransaction> Build();

	}

}