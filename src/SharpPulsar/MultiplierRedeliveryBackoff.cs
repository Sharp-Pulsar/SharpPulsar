
using System;
using SharpPulsar.Interfaces;
using SharpPulsar.Sql.Precondition;

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
namespace SharpPulsar
{
    /// <summary>
    /// MultiplierRedeliveryBackoff.
    /// </summary>
    public class MultiplierRedeliveryBackoff : IRedeliveryBackoff
    {

        private readonly long _minDelayMs;
        private readonly long _maxDelayMs;
        private readonly double _multiplier;
        private readonly int _maxMultiplierPow;

        private MultiplierRedeliveryBackoff(long minDelayMs, long maxDelayMs, double multiplier)
        {
            _minDelayMs = minDelayMs;
            _maxDelayMs = maxDelayMs;
            _multiplier = multiplier;
            _maxMultiplierPow = (int)(Math.Log((double)maxDelayMs / minDelayMs) / Math.Log(multiplier)) + 1;
        }

        public static MultiplierRedeliveryBackoffBuilder Builder()
        {
            return new MultiplierRedeliveryBackoffBuilder();
        }

        public virtual long MinDelayMs
        {
            get
            {
                return _minDelayMs;
            }
        }

        public virtual long MaxDelayMs
        {
            get
            {
                return _maxDelayMs;
            }
        }

        public long Next(int redeliveryCount)
        {
            if (redeliveryCount <= 0 || _minDelayMs <= 0)
            {
                return _minDelayMs;
            }
            if (redeliveryCount > _maxMultiplierPow)
            {
                return _maxDelayMs;
            }
            return Math.Min((long)(_minDelayMs * Math.Pow(_multiplier, redeliveryCount)), _maxDelayMs);
        }

        /// <summary>
        /// Builder of MultiplierRedeliveryBackoff.
        /// </summary>
        public class MultiplierRedeliveryBackoffBuilder
        {
            internal long _minDelayMs = 1000 * 10;
            internal long _maxDelayMs = 1000 * 60 * 10;
            internal double _multiplier = 2.0;

            public virtual MultiplierRedeliveryBackoffBuilder MinDelayMs(long minDelayMs)
            {
                _minDelayMs = minDelayMs;
                return this;
            }

            public virtual MultiplierRedeliveryBackoffBuilder MaxDelayMs(long maxDelayMs)
            {
                _maxDelayMs = maxDelayMs;
                return this;
            }

            public virtual MultiplierRedeliveryBackoffBuilder Multiplier(double multiplier)
            {
                _multiplier = multiplier;
                return this;
            }

            public virtual MultiplierRedeliveryBackoff Build()
            {
                Condition.CheckArgument(_minDelayMs >= 0, "min delay time must be >= 0");
                Condition.CheckArgument(_maxDelayMs >= _minDelayMs, "maxDelayMs must be >= minDelayMs");
                Condition.CheckArgument(_multiplier > 1, "multiplier must be > 1");
                return new MultiplierRedeliveryBackoff(_minDelayMs, _maxDelayMs, _multiplier);
            }
        }
    }

}