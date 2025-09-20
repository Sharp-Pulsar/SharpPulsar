
using System;
using System.Reflection;
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
namespace SharpPulsar.API.Internal
{
    internal class ReflectionUtils
    {
        internal interface SupplierWithException<T>
        {
            T Get();
        }

        internal static T CatchExceptions<T>(SupplierWithException<T> s)
        {
            try
            {
                return s.Get();
            }
            catch (Exception t)
            {
                if (t is TargetException)
                {
                    // exception is thrown during invocation
                    Exception cause = t.InnerException;
                    if (cause is Exception)
                    {
                        throw (Exception)cause;
                    }
                    else
                    {
                        throw ;
                    }
                }
                throw;
            }
        }

        internal static Type NewClassInstance<T>(string className)
        {
            try
            {
                try
                {
                    // when the API is loaded in the same classloader as the impl
                    var t = Type.GetType(className, true);
                    return (Type)Activator.CreateInstance(t);
                }
                catch (Exception)
                {
                    // when the API is loaded in a separate classloader as the impl
                    // the classloader that loaded the impl needs to be a child classloader of the classloader
                    // that loaded the API
                    return Type.GetType(className, true);//, Thread.CurrentThread.getContextClassLoader());
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        internal static ConstructorInfo GetConstructor<T>(string className, params Type[] argTypes)
        {
            try
            {
                Type clazz = NewClassInstance<T>(className);
                return clazz.GetConstructor(argTypes);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        internal static MethodInfo GetStaticMethod<T>(string className, string method, params Type[] argTypes)
        {
            try
            {
                Type clazz = NewClassInstance<T>(className);
                return clazz.GetMethod(method, argTypes);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
