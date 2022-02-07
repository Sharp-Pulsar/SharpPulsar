using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixture;
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
namespace SharpPulsar.Test.API.Schema
{
    [Collection(nameof(IntegrationCollection))]
    /// <summary>
    /// Unit test <seealso cref="BytesSchema"/>.
    /// </summary>
    public class BytesSchemaTest
    {
        [Fact]
        public void TestBytesSchemaOf()
        {
            TestBytesSchema(BytesSchema.Of());
        }

        [Fact]
        public void TestSchemaBYTES()
        {
            TestBytesSchema(ISchema<object>.Bytes);
        }

        private void TestBytesSchema(ISchema<byte[]> schema)
        {
            var data = Encoding.UTF8.GetBytes("hello world");

            var serializedData = schema.Encode(data);
            Assert.Same(data, serializedData);

            var deserializedData = schema.Decode(serializedData);
            Assert.Same(data, deserializedData);

        }

    }

}