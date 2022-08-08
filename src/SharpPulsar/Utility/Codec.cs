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

using System;
using System.Web;
using Microsoft.Extensions.Logging;

namespace SharpPulsar.Utility
{

	public class Codec
	{
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(Codec));

		public static string Encode(string s)
		{
			try
			{
				return HttpUtility.UrlEncode(s);
			}
			catch (Exception e)
			{
				Log.LogError($"{e} is Unknown exception ");
				return s;
			}
		}

		public static string Decode(string s)
		{
			try
			{
				return HttpUtility.UrlDecode(s);
			}
			catch (Exception e)
			{
				Log.LogError(($"{e} is Unknown exception "));
				return s;
			}
		}
	}

}