using SharpPulsar.Schemas;
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
    public class ShortSchemaTest
    {
        [Fact]
        public virtual void TestSchemaEncodeDecodeFidelity()
        {
            ShortSchema schema = ShortSchema.Of();
            short start = 3440;
            for (short i = 0; i < 100; ++i)
            {
                byte[] encode = schema.Encode((short)(start + i));
                int decoded = schema.Decode(encode);
                Assert.Equal(decoded, start + i);
            }
        }
        [Fact]
        public virtual void TestSchemaDecode()
        {
            byte[] byteData = new byte[] { 24, 42 };
            short? expected = 24 * 256 + 42;
            ShortSchema schema = ShortSchema.Of();
            Assert.Equal(expected, schema.Decode(byteData));
        }

    }

}