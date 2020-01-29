using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace SharpPulsar.Util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using EnumResolver = com.fasterxml.jackson.databind.util.EnumResolver;
	using Lists = com.google.common.collect.Lists;
	using Sets = com.google.common.collect.Sets;
	using StringUtils = org.apache.commons.lang3.StringUtils;

	/// <summary>
	/// Generic value converter.
	/// 
	/// <para><h3>Use examples</h3>
	/// 
	/// <pre>
	/// String o1 = String.valueOf(1);
	/// ;
	/// Integer i = FieldParser.convert(o1, Integer.class);
	/// System.out.println(i); // 1
	/// 
	/// </pre>
	/// 
	/// </para>
	/// </summary>
	public sealed class FieldParser
	{

		private static readonly IDictionary<string, System.Reflection.MethodInfo> CONVERTERS = new Dictionary<string, System.Reflection.MethodInfo>();
		private static readonly IDictionary<Type, Type> WRAPPER_TYPES = new Dictionary<Type, Type>();

		static FieldParser()
		{
			// Preload converters and wrapperTypes.
			initConverters();
			initWrappers();
		}

		/// <summary>
		/// Convert the given object value to the given class.
		/// </summary>
		/// <param name="from">
		///            The object value to be converted. </param>
		/// <param name="to">
		///            The type class which the given object should be converted to. </param>
		/// <returns> The converted object value. </returns>
		/// <exception cref="UnsupportedOperationException">
		///             If no suitable converter can be found. </exception>
		/// <exception cref="RuntimeException">
		///             If conversion failed somehow. This can be caused by at least an ExceptionInInitializerError,
		///             IllegalAccessException or InvocationTargetException. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> T convert(Object from, Class<T> to)
		public static T convert<T>(object from, Type to)
		{

			checkNotNull(to);
			if (from == null)
			{
				return null;
			}

			to = (Type<T>) wrap(to);
			// Can we cast? Then just do it.
			if (to.IsAssignableFrom(from.GetType()))
			{
				return to.cast(from);
			}

			// Lookup the suitable converter.
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string converterId = from.GetType().FullName + "_" + to.FullName;
			System.Reflection.MethodInfo converter = CONVERTERS[converterId];

			if (to.IsEnum)
			{
				// Converting string to enum
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: com.fasterxml.jackson.databind.util.EnumResolver r = com.fasterxml.jackson.databind.util.EnumResolver.constructUsingToString((Class<Enum<?>>) to, null);
				EnumResolver r = EnumResolver.constructUsingToString((Type<Enum<object>>) to, null);
				T value = (T) r.findEnum((string) from);
				if (value == null)
				{
					throw new Exception("Invalid value '" + from + "' for enum " + to);
				}
				return value;
			}

			if (converter == null)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.NotSupportedException("Cannot convert from " + from.GetType().FullName + " to " + to.FullName + ". Requested converter does not exist.");
			}

			// Convert the value.
			try
			{
				object val = converter.invoke(to, from);
				return to.cast(val);
			}
			catch (Exception e)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new Exception("Cannot convert from " + from.GetType().FullName + " to " + to.FullName + ". Conversion failed with " + e.Message, e);
			}
		}

		/// <summary>
		/// Update given Object attribute by reading it from provided map properties.
		/// </summary>
		/// <param name="properties">
		///            which key-value pair of properties to assign those values to given object </param>
		/// <param name="obj">
		///            object which needs to be updated </param>
		/// <exception cref="IllegalArgumentException">
		///             if the properties key-value contains incorrect value type </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T> void update(java.util.Map<String, String> properties, T obj) throws IllegalArgumentException
		public static void update<T>(IDictionary<string, string> properties, T obj)
		{
			System.Reflection.FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			java.util.fields.ForEach(f =>
			{
			if (properties.ContainsKey(f.Name))
			{
				try
				{
					f.Accessible = true;
					string v = (string) properties[f.Name];
					if (!StringUtils.isBlank(v))
					{
						f.set(obj, value(v, f));
					}
					else
					{
						setEmptyValue(v, f, obj);
					}
				}
				catch (Exception e)
				{
					throw new System.ArgumentException(format("failed to initialize %s field while setting value %s", f.Name, properties[f.Name]), e);
				}
			}
			});
		}

		/// <summary>
		/// Converts value as per appropriate DataType of the field.
		/// </summary>
		/// <param name="strValue">
		///            : string value of the object </param>
		/// <param name="field">
		///            : field of the attribute
		/// @return </param>
		public static object value(string strValue, System.Reflection.FieldInfo field)
		{
			checkNotNull(field);
			// if field is not primitive type
			Type fieldType = field.GenericType;
			if (fieldType is ParameterizedType)
			{
				Type clazz = (Type)((ParameterizedType) field.GenericType).ActualTypeArguments[0];
				if (field.Type.Equals(typeof(System.Collections.IList)))
				{
					// convert to list
					return stringToList(strValue, clazz);
				}
				else if (field.Type.Equals(typeof(ISet<object>)))
				{
					// covert to set
					return stringToSet(strValue, clazz);
				}
				else if (field.Type.Equals(typeof(System.Collections.IDictionary)))
				{
					Type valueClass = (Type)((ParameterizedType) field.GenericType).ActualTypeArguments[1];
					return stringToMap(strValue, clazz, valueClass);
				}
				else if (field.Type.Equals(typeof(Optional)))
				{
					Type typeClazz = ((ParameterizedType) fieldType).ActualTypeArguments[0];
					if (typeClazz is ParameterizedType)
					{
						throw new System.ArgumentException(format("unsupported non-primitive Optional<%s> for %s", typeClazz.GetType(), field.Name));
					}
					return Optional.ofNullable(convert(strValue, (Type) typeClazz));
				}
				else
				{
					throw new System.ArgumentException(format("unsupported field-type %s for %s", field.Type, field.Name));
				}
			}
			else
			{
				return convert(strValue, field.Type);
			}
		}

		/// <summary>
		/// Sets the empty/null value if field is allowed to be set empty.
		/// </summary>
		/// <param name="strValue"> </param>
		/// <param name="field"> </param>
		/// <param name="obj"> </param>
		/// <exception cref="IllegalArgumentException"> </exception>
		/// <exception cref="IllegalAccessException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T> void setEmptyValue(String strValue, Field field, T obj) throws IllegalArgumentException, IllegalAccessException
		public static void setEmptyValue<T>(string strValue, System.Reflection.FieldInfo field, T obj)
		{
			checkNotNull(field);
			// if field is not primitive type
			Type fieldType = field.GenericType;
			if (fieldType is ParameterizedType)
			{
				if (field.Type.Equals(typeof(System.Collections.IList)))
				{
					field.set(obj, Lists.newArrayList());
				}
				else if (field.Type.Equals(typeof(ISet<object>)))
				{
					field.set(obj, Sets.newHashSet());
				}
				else if (field.Type.Equals(typeof(Optional)))
				{
					field.set(obj, null);
				}
				else
				{
					throw new System.ArgumentException(format("unsupported field-type %s for %s", field.Type, field.Name));
				}
			}
			else if (field.Type.IsAssignableFrom(typeof(Number)) || fieldType.GetType().Equals(typeof(string)))
			{
				field.set(obj, null);
			}
		}

		private static Type wrap(Type type)
		{
			return WRAPPER_TYPES.ContainsKey(type) ? WRAPPER_TYPES[type] : type;
		}

		private static void initConverters()
		{
			System.Reflection.MethodInfo[] methods = typeof(FieldParser).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			java.util.methods.ForEach(method =>
			{
			if (method.ParameterTypes.length == 1)
			{
				CONVERTERS[method.ParameterTypes[0].Name + "_" + method.ReturnType.Name] = method;
			}
			});
		}

		private static void initWrappers()
		{
			WRAPPER_TYPES[typeof(int)] = typeof(Integer);
			WRAPPER_TYPES[typeof(float)] = typeof(Float);
			WRAPPER_TYPES[typeof(double)] = typeof(Double);
			WRAPPER_TYPES[typeof(long)] = typeof(Long);
			WRAPPER_TYPES[typeof(bool)] = typeof(Boolean);
		}

		/// <summary>
		///*** --- Converters --- *** </summary>

		/// <summary>
		/// Converts String to Integer.
		/// </summary>
		/// <param name="val">
		///            The String to be converted. </param>
		/// <returns> The converted Integer value. </returns>
		public static int? stringToInteger(string val)
		{
			string v = trim(val);
			if (io.netty.util.@internal.StringUtil.isNullOrEmpty(v))
			{
				return null;
			}
			else
			{
				return Convert.ToInt32(v);
			}
		}

		/// <summary>
		/// Converts String to Long.
		/// </summary>
		/// <param name="val">
		///            The String to be converted. </param>
		/// <returns> The converted Long value. </returns>
		public static long? stringToLong(string val)
		{
			return Convert.ToInt64(trim(val));
		}

		/// <summary>
		/// Converts String to Double.
		/// </summary>
		/// <param name="val">
		///            The String to be converted. </param>
		/// <returns> The converted Double value. </returns>
		public static double? stringToDouble(string val)
		{
			string v = trim(val);
			if (io.netty.util.@internal.StringUtil.isNullOrEmpty(v))
			{
				return null;
			}
			else
			{
				return Convert.ToDouble(v);
			}
		}

		/// <summary>
		/// Converts String to float.
		/// </summary>
		/// <param name="val">
		///            The String to be converted. </param>
		/// <returns> The converted Double value. </returns>
		public static float? stringToFloat(string val)
		{
			return Convert.ToSingle(trim(val));
		}

		/// <summary>
		/// Converts comma separated string to List.
		/// </summary>
		/// @param <T>
		///            type of list </param>
		/// <param name="val">
		///            comma separated values. </param>
		/// <returns> The converted list with type {@code <T>}. </returns>
		public static IList<T> stringToList<T>(string val, Type type)
		{
			string[] tokens = trim(val).Split(",", true);
			return java.util.tokens.Select(t =>
			{
			return convert(t, type);
			}).ToList();
		}

		/// <summary>
		/// Converts comma separated string to Set.
		/// </summary>
		/// @param <T>
		///            type of set </param>
		/// <param name="val">
		///            comma separated values. </param>
		/// <returns> The converted set with type {@code <T>}. </returns>
		public static ISet<T> stringToSet<T>(string val, Type type)
		{
			string[] tokens = trim(val).Split(",", true);
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			return java.util.tokens.Select(t =>
			{
			return convert(t, type);
			}).collect(Collectors.toSet());
		}

		private static IDictionary<K, V> stringToMap<K, V>(string strValue, Type keyType, Type valueType)
		{
			string[] tokens = trim(strValue).Split(",", true);
			IDictionary<K, V> map = new Dictionary<K, V>();
			foreach (string token in tokens)
			{
				string[] keyValue = trim(token).Split("=", true);
				checkArgument(keyValue.Length == 2, strValue + " map-value is not in correct format key1=value,key2=value2");
				map[convert(keyValue[0], keyType)] = convert(keyValue[1], valueType);
			}
			return map;
		}

		private static string trim(string val)
		{
			checkNotNull(val);
			return val.Trim();
		}

		/// <summary>
		/// Converts Integer to String.
		/// </summary>
		/// <param name="value">
		///            The Integer to be converted. </param>
		/// <returns> The converted String value. </returns>
		public static string integerToString(int? value)
		{
			return value.ToString();
		}

		/// <summary>
		/// Converts Boolean to String.
		/// </summary>
		/// <param name="value">
		///            The Boolean to be converted. </param>
		/// <returns> The converted String value. </returns>

		public static string booleanToString(bool? value)
		{
			return value.ToString();
		}

		/// <summary>
		/// Converts String to Boolean.
		/// </summary>
		/// <param name="value">
		///            The String to be converted. </param>
		/// <returns> The converted Boolean value. </returns>
		public static bool? stringToBoolean(string value)
		{
			return Convert.ToBoolean(value);
		}

		// implement more converter methods here.

	}
}