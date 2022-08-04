

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
	/// Schema serialization exception.
	/// </summary>
	public class SchemaSerializationException : System.Exception
	{

		/// <summary>
		/// Constructs an {@code SchemaSerializationException} with the specified detail message.
		/// </summary>
		/// <param name="message">
		///        The detail message (which is saved for later retrieval
		///        by the <seealso cref="getMessage()"/> method) </param>
		public SchemaSerializationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructs an {@code SchemaSerializationException} with the specified cause.
		/// </summary>
		/// <param name="cause">
		///        The cause (which is saved for later retrieval by the
		///        <seealso cref="getCause()"/> method).  (A null value is permitted,
		///        and indicates that the cause is nonexistent or unknown.) </param>
		public SchemaSerializationException(System.Exception cause) : base(cause.Message, cause)
		{
		}
	}

}