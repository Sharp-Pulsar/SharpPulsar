using System.IO;

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
namespace SharpPulsar.Exceptions
{ 
	/// <summary>
	/// Exceptions for transaction coordinator client.
	/// </summary>
	public class TransactionCoordinatorClientException : IOException
	{

		public TransactionCoordinatorClientException(System.Exception t) : base(t.Message)
		{
		}

		public TransactionCoordinatorClientException(string message) : base(message)
		{
		}
		// <summary>
		/// Thrown when transaction not found in transaction coordinator.
		/// </summary>
		public class TransactionNotFoundException : TransactionCoordinatorClientException
		{
			public TransactionNotFoundException(string message) : base(message)
			{
			}
		}
        public class NoException : TransactionCoordinatorClientException
		{
            public static NoException Instance = new NoException();
            public NoException() : base("")
			{
			}
		}
        public class TransactionNotOpenedException : TransactionCoordinatorClientException
		{
            public TransactionNotOpenedException(string state) : base(state)
			{
			}
		}
		/// <summary>
		/// Thrown when transaction coordinator with unexpected state.
		/// </summary>
		public class CoordinatorClientStateException : TransactionCoordinatorClientException
		{

			public CoordinatorClientStateException() : base("Unexpected state for transaction metadata client.")
			{
			}

			public CoordinatorClientStateException(string message) : base(message)
			{
			}
		}

		/// <summary>
		/// Thrown when transaction coordinator not found in broker side.
		/// </summary>
		public class CoordinatorNotFoundException : TransactionCoordinatorClientException
		{
			public CoordinatorNotFoundException(string message) : base(message)
			{
			}
		}

		/// <summary>
		/// Thrown when transaction switch to a invalid status.
		/// </summary>
		public class InvalidTxnStatusException : TransactionCoordinatorClientException
		{
			public InvalidTxnStatusException(string message) : base(message)
			{
			}
		}

		/// <summary>
		/// Thrown when transaction meta store handler not exists.
		/// </summary>
		public class MetaStoreHandlerNotExistsException : TransactionCoordinatorClientException
		{

			public MetaStoreHandlerNotExistsException(long tcId) : base("Transaction meta store handler for transaction meta store {} not exists.")
			{
			}

			public MetaStoreHandlerNotExistsException(string message) : base(message)
			{
			}
		}

		/// <summary>
		/// Thrown when send request to transaction meta store but the transaction meta store handler not ready.
		/// </summary>
		public class MetaStoreHandlerNotReadyException : TransactionCoordinatorClientException
		{
			public MetaStoreHandlerNotReadyException(long tcId) : base("Transaction meta store handler for transaction meta store {} not ready now.")
			{
			}

			public MetaStoreHandlerNotReadyException(string message) : base(message)
			{
			}
		}

		public static TransactionCoordinatorClientException Unwrap(System.Exception t)
        {
            throw t;
        }
	}

}