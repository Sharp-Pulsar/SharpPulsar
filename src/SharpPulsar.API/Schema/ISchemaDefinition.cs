using System.Reflection;
using Akka.Util;
using SharpPulsar.API.Internal;

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
namespace SharpPulsar.API.Schema
{
    /// <summary>
	/// Interface for schema definition.
	/// </summary>
	public interface ISchemaDefinition<T>
    {

        /// <summary>
        /// Get a new builder instance that can used to configure and build a <seealso cref="SchemaDefinition"/> instance.
        /// </summary>
        /// <returns> the <seealso cref="ISchemaDefinition<T>"/> </returns>
        static ISchemaDefinitionBuilder<T> Builder()
        {
            return DefaultImplementation.GetDefaultImplementation.NewSchemaDefinitionBuilder<T>();
        }

        /// <summary>
        /// Get schema whether always allow null or not.
        /// </summary>
        /// <returns> schema always null or not </returns>
        bool AlwaysAllowNull { get; }

        /// <summary>
        /// Get JSR310 conversion enabled.
        /// </summary>
        /// <returns> return true if enable JSR310 conversion. false means use Joda time conversion. </returns>
        bool IsJsr310ConversionEnabled();


        /// <summary>
        /// Get schema class.
        /// </summary>
        /// <returns> schema class </returns>
        IDictionary<string, string> Properties { get; }

        /// <summary>
        /// Get json schema definition.
        /// </summary>
        /// <returns> schema class </returns>
        string JsonDef { get; }

        /// <summary>
        /// Get pojo schema definition.
        /// </summary>
        /// <example>
        /// Assembly assembly = Assembly.LoadFrom("SharpPulsar");
        /// Type type = assembly.GetType("pojo");
        /// object x = Activator.CreateInstance(type);
        /// </example>
        /// <returns> pojo schema </returns>
        Type Pojo { get; }

        /// <summary>
        /// Get pojo classLoader.
        /// </summary>
        /// <example>
        /// Assembly assembly = Assembly.LoadFrom("SharpPulsar");
        /// Type type = assembly.GetType("pojo");
        /// object x = Activator.CreateInstance(type);
        /// </example>
        /// <returns> pojo schema </returns>
        Assembly ClassLoader();


        /// <summary>
        /// Get supportSchemaVersioning schema definition.
        /// </summary>
        /// <returns> the flag of supportSchemaVersioning </returns>
        bool SupportSchemaVersioning { get; }

        /// <summary>
        /// Get a configured schema reader.
        /// </summary>
        /// <returns> optional containing configured schema reader or empty optional if none is configure </returns>
        Option<ISchemaReader<T>> SchemaReaderOpt { get; }

        /// <summary>
        /// Get a configured schema writer.
        /// </summary>
        /// <returns> optional containing configured schema writer or empty optional if none is configure </returns>
        Option<ISchemaWriter<T>> SchemaWriterOpt { get; }
    }

}