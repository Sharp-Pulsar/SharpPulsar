using System;
using System.Collections.Generic;

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
namespace SharpPulsar.Common.Client
{

	/// <summary>
	/// This class defines a factory for {@code URL} stream
	/// protocol handlers.
	/// </summary>
	public class PulsarURLStreamHandlerFactory : URLStreamHandlerFactory
	{
		private static readonly IDictionary<string, Type> handlers;
		static PulsarURLStreamHandlerFactory()
		{
			handlers = new Dictionary<string, Type>();
			handlers["data"] = typeof(DataURLStreamHandler);
		}

		public override URLStreamHandler CreateURLStreamHandler(string protocol)
		{
			URLStreamHandler urlStreamHandler;
			try
			{
				Type handler = handlers[protocol];
				if (handler != null)
				{
					urlStreamHandler = Activator.CreateInstance(handler);
				}
				else
				{
					urlStreamHandler = null;
				}
			}
			catch (InstantiationException)
			{
				urlStreamHandler = null;
			}
			catch (IllegalAccessException)
			{
				urlStreamHandler = null;
			}
			return urlStreamHandler;
		}

	}

}