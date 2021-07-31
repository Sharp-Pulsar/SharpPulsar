using System;

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
    /// Base type of exception thrown by Pulsar client.
    /// </summary>
    public class PulsarClientException : Exception
	{
		private long _sequenceId = -1;
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
		/// Constructs an {@code PulsarClientException} with the specified detail message.
		/// </summary>
		/// <param name="msg">
		///        The detail message (which is saved for later retrieval
		///        by the <seealso cref="getMessage()"/> method) </param>
		/// <param name="sequenceId">
		///        The sequenceId of the message </param>
		public PulsarClientException(string msg, long sequenceId) : base(msg)
		{
			_sequenceId = sequenceId;
		}

		/// <summary>
		/// Constructs an {@code PulsarClientException} with the specified cause.
		/// </summary>
		/// <param name="t">
		///        The cause (which is saved for later retrieval by the
		///        <seealso cref="getCause()"/> method).  (A null value is permitted,
		///        and indicates that the cause is nonexistent or unknown.) </param>
		public PulsarClientException(Exception t) : base(t.Message, t)
		{
		}

		/// <summary>
		/// Constructs an {@code PulsarClientException} with the specified cause.
		/// </summary>
		/// <param name="msg">
		///            The detail message (which is saved for later retrieval by the <seealso cref="getMessage()"/> method)
		/// </param>
		/// <param name="t">
		///            The cause (which is saved for later retrieval by the <seealso cref="getCause()"/> method). (A null value is
		///            permitted, and indicates that the cause is nonexistent or unknown.) </param>
		public PulsarClientException(string msg, Exception t) : base(msg, t)
		{
		}

		/// <summary>
		/// Constructs an {@code PulsarClientException} with the specified cause.
		/// </summary>
		/// <param name="t">
		///            The cause (which is saved for later retrieval by the <seealso cref="getCause()"/> method). (A null value is
		///            permitted, and indicates that the cause is nonexistent or unknown.) </param>
		/// <param name="sequenceId">
		///            The sequenceId of the message </param>
		public PulsarClientException(Exception t, long sequenceId) : base(t.Message, t)
		{
			_sequenceId = sequenceId;
		}

		/// <summary>
		/// Invalid Service URL exception thrown by Pulsar client.
		/// </summary>
		public class InvalidServiceUrl : PulsarClientException
		{
			/// <summary>
			/// Constructs an {@code InvalidServiceURL} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public InvalidServiceUrl(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code InvalidServiceURL} with the specified cause.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public InvalidServiceUrl(string msg, Exception t) : base(msg, t)
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

			/// <summary>
			/// Constructs an {@code InvalidConfigurationException} with the specified cause.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public InvalidConfigurationException(string msg, Exception t) : base(msg, t)
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
			/// Constructs an {@code TimeoutException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			/// <param name="sequenceId">
			///        The sequenceId of the message </param>
			public TimeoutException(Exception t, long sequenceId) : base(t, sequenceId)
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

			/// <summary>
			/// Constructs an {@code TimeoutException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public TimeoutException(string msg, long sequenceId) : base(msg, sequenceId)
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

			/// <summary>
			/// Constructs an {@code AlreadyClosedException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			/// <param name="sequenceId">
			///        The sequenceId of the message </param>
			public AlreadyClosedException(string msg, long sequenceId) : base(msg, sequenceId)
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

			/// <summary>
			/// Constructs an {@code TopicTerminatedException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			/// <param name="sequenceId">
			///        The sequenceId of the message </param>
			public TopicTerminatedException(string msg, long sequenceId) : base(msg, sequenceId)
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

			public NotConnectedException(long sequenceId) : base("Not connected to broker", sequenceId)
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

			/// <summary>
			/// Constructs an {@code InvalidMessageException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			/// <param name="sequenceId">
			///        The sequenceId of the message </param>
			public InvalidMessageException(string msg, long sequenceId) : base(msg, sequenceId)
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

			/// <summary>
			/// Constructs an {@code ProducerQueueIsFullError} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			/// <param name="sequenceId">
			///        The sequenceId of the message </param>
			public ProducerQueueIsFullError(string msg, long sequenceId) : base(msg, sequenceId)
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
		/// Not allowed exception thrown by Pulsar client.
		/// </summary>
		public class NotAllowedException : PulsarClientException
		{

			/// <summary>
			/// Constructs an {@code NotAllowedException} with the specified detail message.
			/// </summary>
			/// <param name="msg">
			///        The detail message (which is saved for later retrieval
			///        by the <seealso cref="getMessage()"/> method) </param>
			public NotAllowedException(string msg) : base(msg)
			{
			}
		}
        /// <summary>
        /// Producer was fenced by the broker.
        /// </summary>
        public class ProducerFencedException : PulsarClientException
		{

            /// <summary>
            /// Constructs an {@code ProducerFencedException} with the specified detail message.
            /// </summary>
            /// <param name="msg">
            ///        The detail message (which is saved for later retrieval
            ///        by the <seealso cref="getMessage()"/> method) </param>
            public ProducerFencedException(string msg) : base(msg)
			{
			}
		}
		/// <summary>
		/// Consumer assign exception thrown by Pulsar client.
		/// </summary>
		public class TransactionConflictException : PulsarClientException
		{

			/// <summary>
			/// Constructs an {@code TransactionConflictException} with the specified cause.
			/// </summary>
			/// <param name="t">
			///        The cause (which is saved for later retrieval by the
			///        <seealso cref="getCause()"/> method).  (A null value is permitted,
			///        and indicates that the cause is nonexistent or unknown.) </param>
			public TransactionConflictException(Exception t) : base(t)
			{
			}

			/// <summary>
			/// Constructs an {@code TransactionConflictException} with the specified detail message. </summary>
			/// <param name="msg"> The detail message. </param>
			public TransactionConflictException(string msg) : base(msg)
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
		/// <summary>
		/// Consumer assign exception thrown by Pulsar client.
		/// </summary>
		public class ConsumerAssignException : PulsarClientException
		{

			/// <summary>
			/// Constructs an {@code ConsumerAssignException} with the specified detail message. </summary>
			/// <param name="msg"> The detail message. </param>
			public ConsumerAssignException(string msg) : base(msg)
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

            if (t is InvalidConfigurationException)
            {
                return new InvalidConfigurationException(msg);
            }
            if (t is AuthenticationException)
            {
                return new AuthenticationException(msg);
            }
            if (t is IncompatibleSchemaException)
            {
                return new IncompatibleSchemaException(msg);
            }
            if (t is TooManyRequestsException)
            {
                return new TooManyRequestsException(msg);
            }
            if (t is LookupException)
            {
                return new LookupException(msg);
            }
            if (t is ConnectException)
            {
                return new ConnectException(msg);
            }
            if (t is AlreadyClosedException)
            {
                return new AlreadyClosedException(msg);
            }
            if (t is TopicTerminatedException)
            {
                return new TopicTerminatedException(msg);
            }
            if (t is AuthorizationException)
            {
                return new AuthorizationException(msg);
            }
            if (t is GettingAuthenticationDataException)
            {
                return new GettingAuthenticationDataException(msg);
            }
            if (t is UnsupportedAuthenticationException)
            {
                return new UnsupportedAuthenticationException(msg);
            }
            if (t is BrokerPersistenceException)
            {
                return new BrokerPersistenceException(msg);
            }
            if (t is BrokerMetadataException)
            {
                return new BrokerMetadataException(msg);
            }
            if (t is ProducerBusyException)
            {
                return new ProducerBusyException(msg);
            }
            if (t is ConsumerBusyException)
            {
                return new ConsumerBusyException(msg);
            }
            if (t is NotConnectedException)
            {
                return new NotConnectedException();
            }
            if (t is InvalidMessageException)
            {
                return new InvalidMessageException(msg);
            }
            if (t is InvalidTopicNameException)
            {
                return new InvalidTopicNameException(msg);
            }
            if (t is NotSupportedException)
            {
                return new NotSupportedException(msg);
            }
            if (t is ProducerQueueIsFullError)
            {
                return new ProducerQueueIsFullError(msg);
            }
            if (t is ProducerBlockedQuotaExceededError)
            {
                return new ProducerBlockedQuotaExceededError(msg);
            }
            if (t is ProducerBlockedQuotaExceededException)
            {
                return new ProducerBlockedQuotaExceededException(msg);
            }
            if (t is ChecksumException)
            {
                return new ChecksumException(msg);
            }
            if (t is CryptoException)
            {
                return new CryptoException(msg);
            }
            if (t is ConsumerAssignException)
            {
                return new ConsumerAssignException(msg);
            }
            if (t is NotAllowedException)
            {
                return new NotAllowedException(msg);
            }
            if (t is TransactionConflictException)
            {
                return new TransactionConflictException(msg);
            }
            if (t is PulsarClientException)
            {
                return new PulsarClientException(msg);
            }
            if (t is Exception)
            {
                return new Exception(msg, t.InnerException);
            }

            return t;
		}

		public static PulsarClientException Unwrap(Exception t)
		{
			if (t is PulsarClientException exception)
			{
				return exception;
			}

            if (t is Exception)
            {
                return new PulsarClientException(t);
            }

            // Unwrap the exception to keep the same exception type but a stack trace that includes the application calling
			// site
			var cause = t.InnerException;
			var msg = cause.Message;
			if (cause is TimeoutException)
			{
				return new TimeoutException(msg);
			}

            if (cause is InvalidConfigurationException)
            {
                return new InvalidConfigurationException(msg);
            }
            if (cause is AuthenticationException)
            {
                return new AuthenticationException(msg);
            }
            if (cause is IncompatibleSchemaException)
            {
                return new IncompatibleSchemaException(msg);
            }
            if (cause is TooManyRequestsException)
            {
                return new TooManyRequestsException(msg);
            }
            if (cause is LookupException)
            {
                return new LookupException(msg);
            }
            if (cause is ConnectException)
            {
                return new ConnectException(msg);
            }
            if (cause is AlreadyClosedException)
            {
                return new AlreadyClosedException(msg);
            }
            if (cause is TopicTerminatedException)
            {
                return new TopicTerminatedException(msg);
            }
            if (cause is AuthorizationException)
            {
                return new AuthorizationException(msg);
            }
            if (cause is GettingAuthenticationDataException)
            {
                return new GettingAuthenticationDataException(msg);
            }
            if (cause is UnsupportedAuthenticationException)
            {
                return new UnsupportedAuthenticationException(msg);
            }
            if (cause is BrokerPersistenceException)
            {
                return new BrokerPersistenceException(msg);
            }
            if (cause is BrokerMetadataException)
            {
                return new BrokerMetadataException(msg);
            }
            if (cause is ProducerBusyException)
            {
                return new ProducerBusyException(msg);
            }
            if (cause is ConsumerBusyException)
            {
                return new ConsumerBusyException(msg);
            }
            if (cause is NotConnectedException)
            {
                return new NotConnectedException();
            }
            if (cause is InvalidMessageException)
            {
                return new InvalidMessageException(msg);
            }
            if (cause is InvalidTopicNameException)
            {
                return new InvalidTopicNameException(msg);
            }
            if (cause is NotSupportedException)
            {
                return new NotSupportedException(msg);
            }
            if (cause is ProducerQueueIsFullError)
            {
                return new ProducerQueueIsFullError(msg);
            }
            if (cause is ProducerBlockedQuotaExceededError)
            {
                return new ProducerBlockedQuotaExceededError(msg);
            }
            if (cause is ProducerBlockedQuotaExceededException)
            {
                return new ProducerBlockedQuotaExceededException(msg);
            }
            if (cause is ChecksumException)
            {
                return new ChecksumException(msg);
            }
            if (cause is CryptoException)
            {
                return new CryptoException(msg);
            }
            if (cause is TopicDoesNotExistException)
            {
                return new TopicDoesNotExistException(msg);
            }
            return new PulsarClientException(t);
        }

		public virtual long SequenceId
		{
			get => _sequenceId;
            set => _sequenceId = value;
        }


		public static bool IsRetriableError(Exception t)
		{
			if (t is AuthorizationException || t is InvalidServiceUrl || t is InvalidConfigurationException || t is NotFoundException || t is IncompatibleSchemaException || t is TopicDoesNotExistException || t is UnsupportedAuthenticationException || t is InvalidMessageException || t is InvalidTopicNameException || t is NotSupportedException || t is ChecksumException || t is CryptoException || t is ProducerBusyException || t is ConsumerBusyException)
			{
				return false;
			}
			return true;
		}
	}
}