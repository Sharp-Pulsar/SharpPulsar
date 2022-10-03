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
namespace SharpPulsar.Admin
{
    /// <summary>
    /// Pulsar admin exceptions.
    /// </summary>
    public class PulsarAdminException : Exception
    {
        private const int DefaultStatusCode = 500;

        private readonly string httpError;
        private readonly int statusCode;

        public PulsarAdminException(Exception t, string httpError, int statusCode) : base(t.ToString())
        {
            this.httpError = HttpError;
            this.statusCode = StatusCode;
        }

        public PulsarAdminException(string message, Exception t, string httpError, int statusCode) : base(message, t)
        {
            this.httpError = HttpError;
            this.statusCode = StatusCode;
        }

        public PulsarAdminException(Exception t) : base(t.ToString())
        {
            httpError = null;
            statusCode = DefaultStatusCode;
        }

        public PulsarAdminException(string message, Exception t) : base(message, t)
        {
            httpError = null;
            statusCode = DefaultStatusCode;
        }

        public PulsarAdminException(string message) : base(message)
        {
            httpError = null;
            statusCode = DefaultStatusCode;
        }

        public virtual string HttpError
        {
            get
            {
                return httpError;
            }
        }

        public virtual int StatusCode
        {
            get
            {
                return statusCode;
            }
        }



        /// <summary>
        /// Not Authorized Exception.
        /// </summary>
        public class NotAuthorizedException : PulsarAdminException
        {
            public NotAuthorizedException(Exception t, string httpError, int statusCode) : base(httpError, t, httpError, statusCode)
            {
            }

        }

        /// <summary>
        /// Not Found Exception.
        /// </summary>
        public class NotFoundException : PulsarAdminException
        {
            public NotFoundException(Exception t, string httpError, int statusCode) : base(httpError, t, httpError, statusCode)
            {
            }

        }

        /// <summary>
        /// Not Allowed Exception.
        /// </summary>
        public class NotAllowedException : PulsarAdminException
        {
            public NotAllowedException(Exception t, string httpError, int statusCode) : base(httpError, t, httpError, statusCode)
            {
            }

        }

        /// <summary>
        /// Conflict Exception.
        /// </summary>
        public class ConflictException : PulsarAdminException
        {
            public ConflictException(Exception t, string httpError, int statusCode) : base(httpError, t, httpError, statusCode)
            {
            }

        }

        /// <summary>
        /// Precondition Failed Exception.
        /// </summary>
        public class PreconditionFailedException : PulsarAdminException
        {
            public PreconditionFailedException(Exception t, string httpError, int statusCode) : base(httpError, t, httpError, statusCode)
            {
            }

        }

        /// <summary>
        /// Timeout Exception.
        /// </summary>
        public class TimeoutException : PulsarAdminException
        {
            public TimeoutException(Exception t) : base(t)
            {
            }

        }

        /// <summary>
        /// Server Side Error Exception.
        /// </summary>
        public class ServerSideErrorException : PulsarAdminException
        {
            public ServerSideErrorException(Exception t, string message, string httpError, int statusCode) : base(message, t, httpError, statusCode)
            {
            }

        }

        /// <summary>
        /// Http Error Exception.
        /// </summary>
        public class HttpErrorException : PulsarAdminException
        {
            public HttpErrorException(Exception e) : base(e)
            {
            }

        }

        /// <summary>
        /// Connect Exception.
        /// </summary>
        public class ConnectException : PulsarAdminException
        {
            public ConnectException(Exception t) : base(t)
            {
            }

            public ConnectException(string message, Exception t) : base(message, t)
            {
            }

        }

        /// <summary>
        /// Getting Authentication Data Exception.
        /// </summary>
        public class GettingAuthenticationDataException : PulsarAdminException
        {
            public GettingAuthenticationDataException(Exception t) : base(t)
            {
            }


        }

        /// <summary>
        /// Clone the exception and grab the current stacktrace. </summary>
        /// <param name="e"> a PulsarAdminException </param>
        /// <returns> a new PulsarAdminException, of the same class. </returns>
        public static PulsarAdminException Wrap(PulsarAdminException e)
        {
            var cloned = e;
            if (e.GetType() != cloned.GetType())
            {
                throw new InvalidOperationException("Cloning a " + e.GetType() + " generated a " + cloned.GetType() + ", this is a bug, original error is " + e, e);
            }
            // adding a reference to the original exception.
            // cloned..addSuppressed(E);
            //return (PulsarAdminException) cloned.fillInStackTrace();
            return cloned;
        }
    }

}