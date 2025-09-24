
using System;
using System.Threading;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Shared.Exceptions
{

    /// <summary>
    /// Exceptions for transaction buffer client.
    /// </summary>
    public class TransactionBufferClientException : IOException
    {

        public TransactionBufferClientException(Exception t)
        {
        }

        public TransactionBufferClientException(string message) : base(message)
        {
        }

        /// <summary>
        /// Thrown when operation timeout.
        /// </summary>
        public class RequestTimeoutException : TransactionBufferClientException
        {

            public RequestTimeoutException() : base("Transaction buffer request timeout.")
            {
            }

            public RequestTimeoutException(string message) : base(message)
            {
            }
        }

        /// <summary>
        /// Thrown when transaction buffer op over max pending numbers.
        /// </summary>
        public class ReachMaxPendingOpsException : TransactionBufferClientException
        {

            public ReachMaxPendingOpsException() : base("Transaction buffer op reach max pending numbers.")
            {
            }

            public ReachMaxPendingOpsException(string message) : base(message)
            {
            }
        }

        public static TransactionBufferClientException Unwrap(Exception t)
        {
            if (t is TransactionBufferClientException)
            {
                return (TransactionBufferClientException)t;
            }
            else if (t is Exception)
            {
                throw t;
            }
            else if (t is ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
                return new TransactionBufferClientException(t);
            }
            else if (!(t.InnerException != null))
            {
                // Generic exception
                return new TransactionBufferClientException(t);
            }

            var cause = t.InnerException;
            var msg = cause.Message;

            if (cause is RequestTimeoutException)
            {
                return new RequestTimeoutException(msg);
            }
            else
            {
                return new TransactionBufferClientException(t);
            }

        }
    }
}
