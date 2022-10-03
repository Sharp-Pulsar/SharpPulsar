using System.Collections.Generic;

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
namespace SharpPulsar.Admin.Model
{
    public sealed class Mode
    {

        public static readonly Mode PERSISTENT = new Mode("PERSISTENT", InnerEnum.PERSISTENT, 0);
        public static readonly Mode NonPersistent = new Mode("NonPersistent", InnerEnum.NonPersistent, 1);
        public static readonly Mode ALL = new Mode("ALL", InnerEnum.ALL, 2);

        private static readonly List<Mode> valueList = new List<Mode>();

        static Mode()
        {
            valueList.Add(PERSISTENT);
            valueList.Add(NonPersistent);
            valueList.Add(ALL);
        }

        public enum InnerEnum
        {
            PERSISTENT,
            NonPersistent,
            ALL,

        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;
        private readonly int value;
        private Mode(string name, InnerEnum innerEnum, int value)
        {
            this.value = value;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }
        public int Value
        {
            get
            {
                return value;
            }
        }
        public static Mode ValueOf(int n)
        {
            switch (n)
            {
                case 0:
                    return PERSISTENT;
                case 1:
                    return NonPersistent;
                case 2:
                    return ALL;
                default:
                    return null;

            }
        }

        public static Mode[] Values()
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
    }

}