using SharpPulsar.Schemas;
using SharpPulsar.Extension;
using System;
using System.Linq;
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

    public class DoubleSchemaTest
    {
        [Fact]
        public virtual void TestSchemaEncode()
        {
            DoubleSchema schema = DoubleSchema.Of();
            double data = 12345678.1234D;
            sbyte[] expected = BitConverter.GetBytes(data).Reverse().ToArray().ToSBytes();
            Assert.Equal(expected, schema.Encode(data));
        }
        [Fact]
        public virtual void TestSchemaEncodeDecodeFidelity()
        {
            DoubleSchema schema = DoubleSchema.Of();
            double dbl = 1234578.8754321D;
            sbyte[] bytes = schema.Encode(dbl);
            Assert.Equal(dbl, schema.Decode(bytes));
        }

    }

}