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
namespace SharpPulsar.Trino.Naming
{
    /// <summary>
    /// Enumeration showing if a topic is persistent.
    /// </summary>
    public sealed class TopicDomain
    {
        public static readonly TopicDomain Persistent = new TopicDomain("persistent", InnerEnum.Persistent, "persistent");
        public static readonly TopicDomain NonPersistent = new TopicDomain("non_persistent", InnerEnum.NonPersistent, "non-persistent");

        private static readonly IList<TopicDomain> ValueList = new List<TopicDomain>();

        static TopicDomain()
        {
            ValueList.Add(Persistent);
            ValueList.Add(NonPersistent);
        }

        public enum InnerEnum
        {
            Persistent,
            NonPersistent
        }

        public readonly InnerEnum InnerEnumValue;
        private readonly string _nameValue;
        private readonly int _ordinalValue;
        private static int _nextOrdinal = 0;

        private readonly string _value;

        private TopicDomain(string name, InnerEnum innerEnum, string value)
        {
            _value = value;

            _nameValue = name;
            _ordinalValue = _nextOrdinal++;
            InnerEnumValue = innerEnum;
        }

        public string Value()
        {
            return _value;
        }

        public static TopicDomain GetEnum(string value)
        {
            foreach (var e in Values())
            {
                if (e._value.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return e;
                }
            }
            throw new ArgumentException("Invalid topic domain: '" + value + "'");
        }

        public override string ToString()
        {
            return _value.ToLower();
        }

        public static IList<TopicDomain> Values()
        {
            return ValueList;
        }

        public int Ordinal()
        {
            return _ordinalValue;
        }

        public static TopicDomain ValueOf(string name)
        {
            foreach (var enumInstance in ValueList)
            {
                if (enumInstance._nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new ArgumentException(name);
        }
    }
}