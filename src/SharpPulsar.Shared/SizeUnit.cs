/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Shared
{
    /// <summary>
    /// Size unit converter.
    /// </summary>
    public sealed class SizeUnit
    {
        public static readonly SizeUnit BYTES = new SizeUnit("BYTES", InnerEnum.BYTES, 1L);
        public static readonly SizeUnit KILO_BYTES = new SizeUnit("KILO_BYTES", InnerEnum.KILO_BYTES, 1024L);
        public static readonly SizeUnit MEGA_BYTES = new SizeUnit("MEGA_BYTES", InnerEnum.MEGA_BYTES, 1024L * 1024L);
        public static readonly SizeUnit GIGA_BYTES = new SizeUnit("GIGA_BYTES", InnerEnum.GIGA_BYTES, 1024L * 1024L * 1024L);

        private static readonly List<SizeUnit> valueList = new List<SizeUnit>();

        static SizeUnit()
        {
            valueList.Add(BYTES);
            valueList.Add(KILO_BYTES);
            valueList.Add(MEGA_BYTES);
            valueList.Add(GIGA_BYTES);
        }

        public enum InnerEnum
        {
            BYTES,
            KILO_BYTES,
            MEGA_BYTES,
            GIGA_BYTES
        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        private readonly long bytes;

        internal SizeUnit(string name, InnerEnum innerEnum, long bytes)
        {
            this.bytes = bytes;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        public long ToBytes(long value)
        {
            return value * bytes;
        }

        public long ToKiloBytes(long value)
        {
            return ToBytes(value) / KILO_BYTES.bytes;
        }

        public long ToMegaBytes(long value)
        {
            return ToBytes(value) / MEGA_BYTES.bytes;
        }

        public long ToGigaBytes(long value)
        {
            return ToBytes(value) / GIGA_BYTES.bytes;
        }

        public static SizeUnit[] Values()
        {
            return valueList.ToArray();
        }

        public int Ordinal()
        {
            return ordinalValue;
        }

        public override string ToString()
        {
            return nameValue;
        }

        public static SizeUnit ValueOf(string name)
        {
            foreach (SizeUnit enumInstance in SizeUnit.valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }
    }
}
