using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Api;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class RegisterSchema
    {
        public RegisterSchema(ISchema schema, string topic)
        {
            Schema = schema;
            Topic = topic;
        }

        public ISchema Schema { get; }
        public string Topic { get; }
    }
}
