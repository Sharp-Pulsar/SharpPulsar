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
namespace org.apache.pulsar.client.admin.@internal
{
	using Slf4j = lombok.@extern.slf4j.Slf4j;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j class WebTargets
	internal class WebTargets
	{

		internal static WebTarget addParts(WebTarget target, string[] parts)
		{
			if (parts != null && parts.Length > 0)
			{
				foreach (string part in parts)
				{
					string encode;
					try
					{
						encode = URLEncoder.encode(part, StandardCharsets.UTF_8.ToString());
					}
					catch (UnsupportedEncodingException e)
					{
						log.error(string.Format("{0} is Unknown", StandardCharsets.UTF_8.ToString()) + "exception - [{}]", e);
						encode = part;
					}
					target = target.path(encode);
				}
			}
			return target;
		}

		private WebTargets()
		{
		}
	}

}