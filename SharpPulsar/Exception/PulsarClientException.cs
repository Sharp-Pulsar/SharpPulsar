using System;
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
namespace Pulsar.Api
{

	/// <summary>
	/// Base type of exception thrown by Pulsar client.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class PulsarClientException extends java.io.IOException
	public class PulsarClientException : IOException
	{

		/// <summary>
		/// Constructs an {@code PulsarClientException} with the specified detail message.
		/// </summary>
		/// <param name="msg">
		///        The detail message (which is saved for later retrieval
		///        by the <seealso cref="getMessage()"/> method) </param>
		public PulsarClientException(string msg) : base(msg)
		{
		}

		/// <summary>
		/// Constructs an {@code PulsarClientException} with the specified cause.
		/// </summary>
		/// <param name="t">
		///        The cause (which is saved for later retrieval by the
		///        <seealso cref="getCause()"/> method).  (A null value is permitted,
		///        and indicates that the cause is nonexistent or unknown.) </param>
		public PulsarClientException(Exception t) : base(t.Message)
		{
		}

		/// <summary>
		/// Invalid Service URL exception thrown by Pulsar client.
		/// </summary>
		public class InvalidServiceURL : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code InvalidServiceURL} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public InvalidServiceURL(Exception t) : base(t)
			{
			}
		}

		/// <summary>
		/// Invalid Configuration exception thrown by Pulsar client.
		/// </summary>
		public class InvalidConfigurationException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code InvalidConfigurationException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public InvalidConfigurationException(string msg) : base(msg)
			{
			}

			/// <summary>
			/// Constructs an {@code InvalidConfigurationException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public InvalidConfigurationException(Exception t) : base(t)
			{
			}
		}

		/// <summary>
		/// Not Found exception thrown by Pulsar client.
		/// </summary>
		public class NotFoundException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code NotFoundException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public NotFoundException(string msg) : base(msg)
			{
			}

			/// <summary>
			/// Constructs an {@code NotFoundException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public NotFoundException(Exception t) : base(t)
			{
			}
		}

		/// <summary>
		/// Timeout exception thrown by Pulsar client.
		/// </summary>
		public class TimeoutException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code TimeoutException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public TimeoutException(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code TimeoutException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public TimeoutException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Incompatible schema exception thrown by Pulsar client.
		/// </summary>
		public class IncompatibleSchemaException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code IncompatibleSchemaException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public IncompatibleSchemaException(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code IncompatibleSchemaException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public IncompatibleSchemaException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Topic does not exist and cannot be created.
		/// </summary>
		public class TopicDoesNotExistException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code TopicDoesNotExistException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public TopicDoesNotExistException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Lookup exception thrown by Pulsar client.
		/// </summary>
		public class LookupException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code LookupException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public LookupException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Too many requests exception thrown by Pulsar client.
		/// </summary>
		public class TooManyRequestsException : LookupException
		{
			/// <summary>
			/// Constructs an {@code TooManyRequestsException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public TooManyRequestsException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Connect exception thrown by Pulsar client.
		/// </summary>
		public class ConnectException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ConnectException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public ConnectException(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code ConnectException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ConnectException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Already closed exception thrown by Pulsar client.
		/// </summary>
		public class AlreadyClosedException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code AlreadyClosedException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public AlreadyClosedException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Topic terminated exception thrown by Pulsar client.
		/// </summary>
		public class TopicTerminatedException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code TopicTerminatedException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public TopicTerminatedException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Authentication exception thrown by Pulsar client.
		/// </summary>
		public class AuthenticationException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code AuthenticationException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public AuthenticationException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Authorization exception thrown by Pulsar client.
		/// </summary>
		public class AuthorizationException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code AuthorizationException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public AuthorizationException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Getting authentication data exception thrown by Pulsar client.
		/// </summary>
		public class GettingAuthenticationDataException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code GettingAuthenticationDataException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public GettingAuthenticationDataException(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code GettingAuthenticationDataException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public GettingAuthenticationDataException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Unsupported authentication exception thrown by Pulsar client.
		/// </summary>
		public class UnsupportedAuthenticationException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code UnsupportedAuthenticationException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public UnsupportedAuthenticationException(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code UnsupportedAuthenticationException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public UnsupportedAuthenticationException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Broker persistence exception thrown by Pulsar client.
		/// </summary>
		public class BrokerPersistenceException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code BrokerPersistenceException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public BrokerPersistenceException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Broker metadata exception thrown by Pulsar client.
		/// </summary>
		public class BrokerMetadataException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code BrokerMetadataException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public BrokerMetadataException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Producer busy exception thrown by Pulsar client.
		/// </summary>
		public class ProducerBusyException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ProducerBusyException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ProducerBusyException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Consumer busy exception thrown by Pulsar client.
		/// </summary>
		public class ConsumerBusyException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ConsumerBusyException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ConsumerBusyException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Not connected exception thrown by Pulsar client.
		/// </summary>
		public class NotConnectedException : PulsarClientException
		{

			public NotConnectedException() : base("Not connected to broker")
			{
			}
		}

		/// <summary>
		/// Invalid message exception thrown by Pulsar client.
		/// </summary>
		public class InvalidMessageException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code InvalidMessageException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public InvalidMessageException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Invalid topic name exception thrown by Pulsar client.
		/// </summary>
		public class InvalidTopicNameException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code InvalidTopicNameException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public InvalidTopicNameException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Not supported exception thrown by Pulsar client.
		/// </summary>
		public class NotSupportedException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code NotSupportedException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public NotSupportedException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Full producer queue error thrown by Pulsar client.
		/// </summary>
		public class ProducerQueueIsFullError : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ProducerQueueIsFullError} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ProducerQueueIsFullError(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Producer blocked quota exceeded error thrown by Pulsar client.
		/// </summary>
		public class ProducerBlockedQuotaExceededError : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ProducerBlockedQuotaExceededError} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ProducerBlockedQuotaExceededError(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Producer blocked quota exceeded exception thrown by Pulsar client.
		/// </summary>
		public class ProducerBlockedQuotaExceededException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ProducerBlockedQuotaExceededException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ProducerBlockedQuotaExceededException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Checksum exception thrown by Pulsar client.
		/// </summary>
		public class ChecksumException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code ChecksumException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public ChecksumException(string msg) : base(msg)
			{
			}
		}

		/// <summary>
		/// Crypto exception thrown by Pulsar client.
		/// </summary>
		public class CryptoException : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code CryptoException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public CryptoException(string msg) : base(msg)
			{
			}
		}

		// wrap an exception to enriching more info messages.
		public static Exception Wrap(Exception t, string msg)
		{
			msg += "\n" + t.Message;
			// wrap an exception with new message info
			if (t is TimeoutException)
			{
				return new TimeoutException(msg);
			}
			else if (t is InvalidConfigurationException)
			{
				return new InvalidConfigurationException(msg);
			}
			else if (t is AuthenticationException)
			{
				return new AuthenticationException(msg);
			}
			else if (t is IncompatibleSchemaException)
			{
				return new IncompatibleSchemaException(msg);
			}
			else if (t is TooManyRequestsException)
			{
				return new TooManyRequestsException(msg);
			}
			else if (t is LookupException)
			{
				return new LookupException(msg);
			}
			else if (t is ConnectException)
			{
				return new ConnectException(msg);
			}
			else if (t is AlreadyClosedException)
			{
				return new AlreadyClosedException(msg);
			}
			else if (t is TopicTerminatedException)
			{
				return new TopicTerminatedException(msg);
			}
			else if (t is AuthorizationException)
			{
				return new AuthorizationException(msg);
			}
			else if (t is GettingAuthenticationDataException)
			{
				return new GettingAuthenticationDataException(msg);
			}
			else if (t is UnsupportedAuthenticationException)
			{
				return new UnsupportedAuthenticationException(msg);
			}
			else if (t is BrokerPersistenceException)
			{
				return new BrokerPersistenceException(msg);
			}
			else if (t is BrokerMetadataException)
			{
				return new BrokerMetadataException(msg);
			}
			else if (t is ProducerBusyException)
			{
				return new ProducerBusyException(msg);
			}
			else if (t is ConsumerBusyException)
			{
				return new ConsumerBusyException(msg);
			}
			else if (t is NotConnectedException)
			{
				return new NotConnectedException();
			}
			else if (t is InvalidMessageException)
			{
				return new InvalidMessageException(msg);
			}
			else if (t is InvalidTopicNameException)
			{
				return new InvalidTopicNameException(msg);
			}
			else if (t is NotSupportedException)
			{
				return new NotSupportedException(msg);
			}
			else if (t is ProducerQueueIsFullError)
			{
				return new ProducerQueueIsFullError(msg);
			}
			else if (t is ProducerBlockedQuotaExceededError)
			{
				return new ProducerBlockedQuotaExceededError(msg);
			}
			else if (t is ProducerBlockedQuotaExceededException)
			{
				return new ProducerBlockedQuotaExceededException(msg);
			}
			else if (t is ChecksumException)
			{
				return new ChecksumException(msg);
			}
			else if (t is CryptoException)
			{
				return new CryptoException(msg);
			}
			else if (t is PulsarClientException)
			{
				return new PulsarClientException(msg);
			}
			else if (t is CompletionException)
			{
				return t;
			}
			else if (t is Exception)
			{
				return new Exception(msg, t.InnerException);
			}
			else if (t is InterruptedException)
			{
				return t;
			}
			else if (t is ExecutionException)
			{
				return t;
			}

			return t;
		}

		public static PulsarClientException Unwrap(Exception t)
		{
			if (t is PulsarClientException)
			{
				return (PulsarClientException) t;
			}
			else if (t is Exception)
			{
				throw (Exception) t;
			}
			else if (t is InterruptedException)
			{
				return new PulsarClientException(t);
			}
			else if (!(t is ExecutionException))
			{
				// Generic exception
				return new PulsarClientException(t);
			}

			// Unwrap the exception to keep the same exception type but a stack trace that includes the application calling
			// site
			Exception cause = t.InnerException;
			string msg = cause.Message;
			if (cause is TimeoutException)
			{
				return new TimeoutException(msg);
			}
			else if (cause is InvalidConfigurationException)
			{
				return new InvalidConfigurationException(msg);
			}
			else if (cause is AuthenticationException)
			{
				return new AuthenticationException(msg);
			}
			else if (cause is IncompatibleSchemaException)
			{
				return new IncompatibleSchemaException(msg);
			}
			else if (cause is TooManyRequestsException)
			{
				return new TooManyRequestsException(msg);
			}
			else if (cause is LookupException)
			{
				return new LookupException(msg);
			}
			else if (cause is ConnectException)
			{
				return new ConnectException(msg);
			}
			else if (cause is AlreadyClosedException)
			{
				return new AlreadyClosedException(msg);
			}
			else if (cause is TopicTerminatedException)
			{
				return new TopicTerminatedException(msg);
			}
			else if (cause is AuthorizationException)
			{
				return new AuthorizationException(msg);
			}
			else if (cause is GettingAuthenticationDataException)
			{
				return new GettingAuthenticationDataException(msg);
			}
			else if (cause is UnsupportedAuthenticationException)
			{
				return new UnsupportedAuthenticationException(msg);
			}
			else if (cause is BrokerPersistenceException)
			{
				return new BrokerPersistenceException(msg);
			}
			else if (cause is BrokerMetadataException)
			{
				return new BrokerMetadataException(msg);
			}
			else if (cause is ProducerBusyException)
			{
				return new ProducerBusyException(msg);
			}
			else if (cause is ConsumerBusyException)
			{
				return new ConsumerBusyException(msg);
			}
			else if (cause is NotConnectedException)
			{
				return new NotConnectedException();
			}
			else if (cause is InvalidMessageException)
			{
				return new InvalidMessageException(msg);
			}
			else if (cause is InvalidTopicNameException)
			{
				return new InvalidTopicNameException(msg);
			}
			else if (cause is NotSupportedException)
			{
				return new NotSupportedException(msg);
			}
			else if (cause is ProducerQueueIsFullError)
			{
				return new ProducerQueueIsFullError(msg);
			}
			else if (cause is ProducerBlockedQuotaExceededError)
			{
				return new ProducerBlockedQuotaExceededError(msg);
			}
			else if (cause is ProducerBlockedQuotaExceededException)
			{
				return new ProducerBlockedQuotaExceededException(msg);
			}
			else if (cause is ChecksumException)
			{
				return new ChecksumException(msg);
			}
			else if (cause is CryptoException)
			{
				return new CryptoException(msg);
			}
			else if (cause is TopicDoesNotExistException)
			{
				return new TopicDoesNotExistException(msg);
			}
			else
			{
				return new PulsarClientException(t);
			}
		}
	}
}