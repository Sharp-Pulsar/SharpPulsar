﻿using SharpPulsar.Schemas;
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
    [Collection("SchemaSpec")]
    public class DoubleSchemaTest
    {
        [Fact]
        public virtual void TestSchemaEncode()
        {
            var schema = DoubleSchema.Of();
            var data = 12345678.1234D;
            var expected = BitConverter.GetBytes(data).Reverse().ToArray();
            Assert.Equal(expected, schema.Encode(data));
        }
        [Fact]
        public virtual void TestSchemaEncodeDecodeFidelity()
        {
            var schema = DoubleSchema.Of();
            var dbl = 1234578.8754321D;
            var bytes = schema.Encode(dbl);
            Assert.Equal(dbl, schema.Decode(bytes));
        }

    }

}