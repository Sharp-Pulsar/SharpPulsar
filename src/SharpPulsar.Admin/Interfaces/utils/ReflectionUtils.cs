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
namespace SharpPulsar.Admin.Interfaces.Utils
{

	public class ReflectionUtils
	{
		internal interface ISupplierWithException<T>
		{
			T Get();
		}

		public static T NewBuilder<T>(string className)
		{
			return CatchExceptions(() => (T) ReflectionUtils.getStaticMethod(ClassName, "builder", null).invoke(null, null));
		}

		internal static T CatchExceptions<T>(SupplierWithException<T> S)
		{
			try
			{
				return S.Get();
			}
			catch (Exception t)
			{
				if (t is InvocationTargetException)
				{
					// exception is thrown during invocation
					Exception cause = t.InnerException;
					if (cause is Exception)
					{
						throw (RuntimeException) cause;
					}
					else
					{
						throw new Exception(cause);
					}
				}
				throw new Exception(t);
			}
		}

		internal static Type<T> NewClassInstance<T>(string ClassName)
		{
			try
			{
				try
				{
					// when the API is loaded in the same classloader as the impl
					return (Type<T>) typeof(DefaultImplementation).getClassLoader().loadClass(ClassName);
				}
				catch (Exception)
				{
					// when the API is loaded in a separate classloader as the impl
					// the classloader that loaded the impl needs to be a child classloader of the classloader
					// that loaded the API
					return (Type<T>) Thread.CurrentThread.getContextClassLoader().loadClass(ClassName);
				}
			}
			catch (Exception e) when (e is ClassNotFoundException || e is NoClassDefFoundError)
			{
				throw new Exception(e);
			}
		}

		internal static System.Reflection.ConstructorInfo<T> GetConstructor<T>(string ClassName, params Type[] ArgTypes)
		{
			try
			{
				Type Clazz = NewClassInstance(ClassName);
				return Clazz.GetConstructor(ArgTypes);
			}
			catch (NoSuchMethodException E)
			{
				throw new Exception(E);
			}
		}

		internal static System.Reflection.MethodInfo GetStaticMethod<T>(string ClassName, string Method, params Type[] ArgTypes)
		{
			try
			{
				Type Clazz = NewClassInstance(ClassName);
				return Clazz.GetMethod(Method, ArgTypes);
			}
			catch (NoSuchMethodException E)
			{
				throw new Exception(E);
			}
		}
	}

}