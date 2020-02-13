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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{
	using Slf4j = lombok.@extern.slf4j.Slf4j;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j class WebTargets
	public class WebTargets
	{

		internal static WebTarget AddParts(WebTarget Target, string[] Parts)
		{
			if (Parts != null && Parts.Length > 0)
			{
				foreach (string Part in Parts)
				{
					string Encode;
					try
					{
						Encode = URLEncoder.encode(Part, StandardCharsets.UTF_8.ToString());
					}
					catch (UnsupportedEncodingException E)
					{
						log.error(string.Format("{0} is Unknown", StandardCharsets.UTF_8.ToString()) + "exception - [{}]", E);
						Encode = Part;
					}
					Target = Target.path(Encode);
				}
			}
			return Target;
		}

		private WebTargets()
		{
		}
	}

}