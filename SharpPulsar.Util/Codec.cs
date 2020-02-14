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

	public class Codec
	{
		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(Codec));

		public static string encode(string s)
		{
			try
			{
				return URLEncoder.encode(s, StandardCharsets.UTF_8.ToString());
			}
			catch (UnsupportedEncodingException e)
			{
				LOG.error(string.Format("{0} is Unknown", StandardCharsets.UTF_8.ToString()) + "exception - [{}]", e);
				return s;
			}
		}

		public static string decode(string s)
		{
			try
			{
				return URLDecoder.decode(s, StandardCharsets.UTF_8.ToString());
			}
			catch (UnsupportedEncodingException e)
			{
				LOG.error(string.Format("{0} is Unknown", StandardCharsets.UTF_8.ToString()) + "exception - [{}]", e);
				return s;
			}
		}
	}

}