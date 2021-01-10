using NodaTime;
using SharpPulsar.Interfaces;
using SharpPulsar.Schema;
using SharpPulsar.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xunit;

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
namespace SharpPulsar.Test.Schema
{

	/// <summary>
	/// Unit tests primitive schemas.
	/// </summary>
	public class PrimitiveSchemaTest
	{

		private static readonly IDictionary<object, IList<object>> _testData = new Dictionary<object, IList<object>>() 
		{
			{BooleanSchema.Of(), new List<object>{ false, true }},
			{StringSchema.Utf8(), new List<object>{ "my string" }},
			{ByteSchema.Of(), new List<object>{ unchecked((sbyte) 32767), unchecked((sbyte) -32768)} },
			{ShortSchema.Of(), new List<object>{ (short) 32767, (short) -32768} },
			{IntSchema.Of(), new List<object>{ (int) 423412424, (int) -41243432} },
			{LongSchema.Of(), new List<object>{ 922337203685477580L, -922337203685477581L } },
			{FloatSchema.Of(), new List<object>{ 5678567.12312f, -5678567.12341f } },
			{DoubleSchema.Of(), new List<object>{ 5678567.12312d, -5678567.12341d } },
			{BytesSchema.Of(), new List<object>{ Encoding.UTF8.GetBytes("my string") } },
			{DateSchema.Of(), new List<object>{ new DateTime(DateTime.Now.Ticks - 10000), new DateTime(DateTime.Now.Ticks) } },
			{TimeSchema.Of(), new List<object>{ TimeSpan.FromTicks((DateTime.Now).Ticks - 10000), TimeSpan.FromTicks((DateTime.Now).Ticks)} },
			{TimestampSchema.Of(), new List<object>{DateTimeOffset.FromUnixTimeMilliseconds((DateTime.Now).Ticks), DateTimeOffset.FromUnixTimeMilliseconds((DateTime.Now).Ticks)} },
			{InstantSchema.Of(), new List<object>{Instant.FromDateTimeUtc(DateTime.UtcNow), Instant.FromDateTimeUtc(DateTime.UtcNow.AddSeconds(-(60 * 23L)))} },
			{LocalDateSchema.Of(), new List<object>{LocalDate.FromDateTime(DateTime.Now), LocalDate.FromDateTime(DateTime.Now.AddDays(-2))} },
			{LocalTimeSchema.Of(), new List<object>{LocalTime.FromTicksSinceMidnight(DateTime.Now.Ticks), LocalTime.FromTicksSinceMidnight(DateTime.Now.AddHours(-2).Ticks)} },
			{LocalDateTimeSchema.Of(), new List<object>{ LocalDateTime.FromDateTime(DateTime.Now), LocalDateTime.FromDateTime(DateTime.Now.AddDays(-70))} },

        };

		public virtual object[][] Schemas()
		{
			return new object[][]
			{
				new object[] {_testData},
				//new object[] {_testData2}
			};
		}

		private void AllSchemasShouldSupportNull(IDictionary<ISchema<object>, IList<object>> TestData)
		{
			foreach (ISchema<object> Schema in TestData.Keys)
			{
				sbyte[] Bytes = null;
				try
				{
					Assert.Null(Schema.Encode(null));
					Assert.Null(Schema.Decode(Bytes));
				}
				catch (System.NullReferenceException Npe)
				{
					throw new System.NullReferenceException("NPE when using schema " + Schema + " : " + Npe.Message);
				}
			}
		}

		[Fact]
		public void AllSchemasShouldHaveSchemaType()
		{
			Assert.Equal(SchemaType.BOOLEAN, BooleanSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.INT8, ByteSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.INT16, ShortSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.INT32, IntSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.INT64, LongSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.FLOAT, FloatSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.DOUBLE, DoubleSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.STRING, StringSchema.Utf8().SchemaInfo.Type);
			Assert.Equal(SchemaType.BYTES, BytesSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.DATE, DateSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.TIME, TimeSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.TIMESTAMP, TimestampSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.INSTANT, InstantSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.LocalDate, LocalDateSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.LocalTime, LocalTimeSchema.Of().SchemaInfo.Type);
			Assert.Equal(SchemaType.LocalDateTime, LocalDateTimeSchema.Of().SchemaInfo.Type);
		}


	}

}