using System;
using System.Collections.Generic;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    public class GetSchemaTest
    {
        private readonly ITestOutputHelper output;

        public GetSchemaTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public void TestSimpleFoo()
        {
            var simple = typeof(SimpleFoo).GetSchema();
            output.WriteLine(simple);
            Assert.Contains("Age", simple);
        }

        [Fact]
        public void TestFoo()
        {
            var simple = typeof(Foo).GetSchema();
            output.WriteLine(simple);
            Assert.Contains("Age", simple);
        }
    }

    public class SimpleFoo
    {
        public int Age { get; set; }
        public string Name { get; set; }
        public long FactTime { get; set; }
        public double Point { get; set; }
        public float Precision { get; set; }
        public bool Attending { get; set; }
        public byte[] Id { get; set; }
    }
    public class Foo
    {
        public SimpleFoo Fo { get; set; }
        public List<string> Courses { get; set; }
        public Dictionary<string, string> Normal { get; set; }

    }
}
