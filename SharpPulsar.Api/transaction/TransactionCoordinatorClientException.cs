using System;
using System.Threading;

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
	/// Exceptions for transaction coordinator client.
	/// </summary>
	public class TransactionCoordinatorClientException : IOException
	{

		public TransactionCoordinatorClientException(Exception T) : base(T)
		{
		}

		public TransactionCoordinatorClientException(string Message) : base(Message)
		{
		}

		/// <summary>
		/// Thrown when transaction coordinator with unexpected state.
		/// </summary>
		public class CoordinatorClientStateException : TransactionCoordinatorClientException
		{

			public CoordinatorClientStateException() : base("Unexpected state for transaction metadata client.")
			{
			}

			public CoordinatorClientStateException(string Message) : base(Message)
			{
			}
		}

		/// <summary>
		/// Thrown when transaction coordinator not found in broker side.
		/// </summary>
		public class CoordinatorNotFoundException : TransactionCoordinatorClientException
		{
			public CoordinatorNotFoundException(string Message) : base(Message)
			{
			}
		}

		/// <summary>
		/// Thrown when transaction switch to a invalid status.
		/// </summary>
		public class InvalidTxnStatusException : TransactionCoordinatorClientException
		{
			public InvalidTxnStatusException(string Message) : base(Message)
			{
			}
		}

		/// <summary>
		/// Thrown when transaction meta store handler not exists.
		/// </summary>
		public class MetaStoreHandlerNotExistsException : TransactionCoordinatorClientException
		{

			public MetaStoreHandlerNotExistsException(long TcId) : base("Transaction meta store handler for transaction meta store {} not exists.")
			{
			}

			public MetaStoreHandlerNotExistsException(string Message) : base(Message)
			{
			}
		}

		/// <summary>
		/// Thrown when send request to transaction meta store but the transaction meta store handler not ready.
		/// </summary>
		public class MetaStoreHandlerNotReadyException : TransactionCoordinatorClientException
		{
			public MetaStoreHandlerNotReadyException(long TcId) : base("Transaction meta store handler for transaction meta store {} not ready now.")
			{
			}

			public MetaStoreHandlerNotReadyException(string Message) : base(Message)
			{
			}
		}

		public static TransactionCoordinatorClientException Unwrap(Exception T)
		{
			if (T is TransactionCoordinatorClientException)
			{
				return (TransactionCoordinatorClientException) T;
			}
			else if (T is Exception)
			{
				throw (Exception) T;
			}
			else if (T is InterruptedException)
			{
				Thread.CurrentThread.Interrupt();
				return new TransactionCoordinatorClientException(T);
			}
			else if (!(T is ExecutionException))
			{
				// Generic exception
				return new TransactionCoordinatorClientException(T);
			}

			Exception Cause = T.InnerException;
			string Msg = Cause.Message;

			if (Cause is CoordinatorNotFoundException)
			{
				return new CoordinatorNotFoundException(Msg);
			}
			else if (Cause is InvalidTxnStatusException)
			{
				return new InvalidTxnStatusException(Msg);
			}
			else
			{
				return new TransactionCoordinatorClientException(T);
			}

		}
	}

}