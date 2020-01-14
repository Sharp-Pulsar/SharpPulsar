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
namespace org.apache.pulsar.client.@internal
{

	using UtilityClass = lombok.experimental.UtilityClass;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UtilityClass class ReflectionUtils
	internal class ReflectionUtils
	{
		internal interface SupplierWithException<T>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T get() throws Exception;
			T get();
		}

		internal static T catchExceptions<T>(SupplierWithException<T> s)
		{
			try
			{
				return s.get();
			}
			catch (Exception t)
			{
				if (t is InvocationTargetException)
				{
					// exception is thrown during invocation
					Exception cause = t.InnerException;
					if (cause is Exception)
					{
						throw (Exception) cause;
					}
					else
					{
						throw new Exception(cause);
					}
				}
				throw new Exception(t);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static <T> Class<T> newClassInstance(String className)
		internal static Type<T> newClassInstance<T>(string className)
		{
			try
			{
				try
				{
					// when the API is loaded in the same classloader as the impl
					return (Type<T>) typeof(DefaultImplementation).ClassLoader.loadClass(className);
				}
				catch (Exception)
				{
					// when the API is loaded in a separate classloader as the impl
					// the classloader that loaded the impl needs to be a child classloader of the classloader
					// that loaded the API
					return (Type<T>) Thread.CurrentThread.ContextClassLoader.loadClass(className);
				}
			}
			catch (Exception e) when (e is ClassNotFoundException || e is NoClassDefFoundError)
			{
				throw new Exception(e);
			}
		}

		internal static System.Reflection.ConstructorInfo<T> getConstructor<T>(string className, params Type[] argTypes)
		{
			try
			{
				Type clazz = newClassInstance(className);
				return clazz.GetConstructor(argTypes);
			}
			catch (NoSuchMethodException e)
			{
				throw new Exception(e);
			}
		}

		internal static System.Reflection.MethodInfo getStaticMethod<T>(string className, string method, params Type[] argTypes)
		{
			try
			{
				Type clazz = newClassInstance(className);
				return clazz.GetMethod(method, argTypes);
			}
			catch (NoSuchMethodException e)
			{
				throw new Exception(e);
			}
		}
	}

}