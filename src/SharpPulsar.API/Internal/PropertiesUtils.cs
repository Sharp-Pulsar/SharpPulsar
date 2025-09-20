
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

    /// <summary>
    /// Internal utility methods for filtering and mapping <seealso cref="Properties"/> objects.
    /// </summary>
    public class PropertiesUtils
    {

        /// <summary>
        /// Filters the <seealso cref="Properties"/> object so that only properties with the configured prefix are retained,
        /// and then removes that prefix and puts the key value pairs into the result map. </summary>
        /// <param name="props"> - the properties object to filter </param>
        /// <param name="prefix"> - the prefix to filter against and then remove for keys in the resulting map </param>
        /// <returns> a map of properties </returns>
        public static IDictionary<string, object> FilterAndMapProperties(Dictionary<object, object> props, string prefix)
        {
            return FilterAndMapProperties(props, prefix, "");
        }

        /// <summary>
        /// Filters the <seealso cref="Properties"/> object so that only properties with the configured prefix are retained,
        /// and then replaces the srcPrefix with the targetPrefix when putting the key value pairs in the resulting map. </summary>
        /// <param name="props"> - the properties object to filter </param>
        /// <param name="srcPrefix"> - the prefix to filter against and then remove for keys in the resulting map </param>
        /// <param name="targetPrefix"> - the prefix to add to keys in the result map </param>
        /// <returns> a map of properties </returns>
        public static IDictionary<string, object> FilterAndMapProperties(Dictionary<object, object> props, string srcPrefix, string targetPrefix)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            int prefixLength = srcPrefix.Length;
            foreach (KeyValuePair<object, object> kvp in props)
            {
                if (!(kvp.Key is string))
                {
                    
                }
                else {
                    string key = (string)kvp.Key;
                    if (key.StartsWith(srcPrefix, StringComparison.Ordinal) && kvp.Value != null)
                    {
                        string truncatedKey = key.Substring(prefixLength);
                        result[targetPrefix + truncatedKey] = kvp.Value;
                    }
                }
                
            }
            return result;
        }
    }
}
