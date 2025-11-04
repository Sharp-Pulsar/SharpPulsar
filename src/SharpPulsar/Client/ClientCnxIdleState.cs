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
using System.Threading;
using DotNetty.Common.Utilities;
using static SharpPulsar.Client.ClientCnx;

namespace SharpPulsar.Client
{

    public class ClientCnxIdleState
    {

        private readonly ClientHandler clientCnx;

        /// <summary>
        /// Stat. * </summary>
        private volatile State state;

        /// <summary>
        /// Create time. This field is only used to troubleshoot (analyze by dump file). * </summary>
        private readonly long createTime;

        /// <summary>
        /// The time when marks the connection is idle. * </summary>
        private long IdleMarkTime;

        public ClientCnxIdleState(ClientHandler clientCnx)
        {
            this.clientCnx = clientCnx;
            this.createTime = DateTimeHelper.CurrentUnixTimeMillis();
            this.state = State.USING;
        }

        private static readonly (ClientCnxIdleState, State) STATE_UPDATER = new (ClientCnxIdleState, State);

        /// <summary>
        /// Indicates the usage status of the connection and whether it has been released.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// It was using at the time of last check. At current time may be idle. * </summary>
            USING,
            /// <summary>
            /// The connection is in idle. * </summary>
            IDLE,
            /// <summary>
            /// The connection is in idle and will be released soon. In this state, the connection can still be used. * </summary>
            RELEASING,
            /// <summary>
            /// The connection has already been released. * </summary>
            RELEASED
        }

        /// <summary>
        /// Get idle-stat. </summary>
        /// <returns> connection idle-stat </returns>
        public virtual State IdleStat
        {
            get
            {
                return Interlocked.g STATE_UPDATER.get(this);
            }
        }
        /// <summary>
        /// Compare and switch idle-stat. </summary>
        /// <returns> Whether the update is successful.Because there may be other threads competing, possible return false. </returns>
        internal virtual bool CompareAndSetIdleStat(State originalStat, State newStat)
        {
            return STATE_UPDATER.compareAndSet(this, originalStat, newStat);
        }

        /// <returns> Whether this connection is in use. </returns>
        public virtual bool Using
        {
            get
            {
                return IdleStat == State.USING;
            }
        }

        /// <returns> Whether this connection is in idle. </returns>
        public virtual bool Idle
        {
            get
            {
                return IdleStat == State.IDLE;
            }
        }

        /// <returns> Whether this connection is in idle and will be released soon. </returns>
        public virtual bool Releasing
        {
            get
            {
                return IdleStat == State.RELEASING;
            }
        }

        /// <returns> Whether this connection has already been released. </returns>
        public virtual bool Released
        {
            get
            {
                return IdleStat == State.RELEASED;
            }
        }

        /// <summary>
        /// Try to transform the state of the connection to #<seealso cref="State.IDLE"/>, state should only be
        /// transformed to #<seealso cref="State.IDLE"/> from state  #<seealso cref="State.USING"/>. if the state
        /// is successfully transformed, "idleMarkTime" will be  assigned to current time.
        /// </summary>
        public virtual void TryMarkIdleAndInitIdleTime()
        {
            if (compareAndSetIdleStat(State.USING, State.IDLE))
            {
                idleMarkTime = DateTimeHelper.CurrentUnixTimeMillis();
            }
        }

        /// <summary>
        /// Changes the idle-state of the connection to #<seealso cref="State.USING"/> as much as possible, This method
        /// is used when connection borrow, and reset <seealso cref="idleMarkTime"/> if change state to
        /// #<seealso cref="State.USING"/> success. </summary>
        /// <returns> Whether change idle-stat to #<seealso cref="State.USING"/> success. False is returned only if the
        /// connection has already been released. </returns>
        public virtual bool TryMarkUsingAndClearIdleTime()
        {
            while (true)
            {
                // Ensure not released
                if (Released)
                {
                    return false;
                }
                // Try mark release
                if (compareAndSetIdleStat(State.IDLE, State.USING))
                {
                    IdleMarkTime = 0;
                    return true;
                }
                if (compareAndSetIdleStat(State.RELEASING, State.USING))
                {
                    IdleMarkTime = 0;
                    return true;
                }
                if (Using)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Changes the idle-state of the connection to #<seealso cref="State.RELEASING"/>, This method only changes this
        /// connection from the #<seealso cref="State.IDLE"/> state to the #<seealso cref="State.RELEASING"/> state. </summary>
        /// <returns> Whether change idle-stat to #<seealso cref="State.RELEASING"/> success. </returns>
        public virtual bool TryMarkReleasing()
        {
            return compareAndSetIdleStat(State.IDLE, State.RELEASING);
        }

        /// <summary>
        /// Changes the idle-state of the connection to #<seealso cref="State.RELEASED"/>, This method only changes this
        /// connection from the #<seealso cref="State.RELEASING"/> state to the #<seealso cref="State.RELEASED"/> </summary>
        /// state, and close {<param name="clientCnx">} if change state to #<seealso cref="State.RELEASED"/> success. </param>
        /// <returns> Whether change idle-stat to #<seealso cref="State.RELEASED"/> and close connection success. </returns>
        public virtual bool TryMarkReleasedAndCloseConnection()
        {
            if (!compareAndSetIdleStat(State.RELEASING, State.RELEASED))
            {
                return false;
            }
            clientCnx.close();
            return true;
        }

        /// <summary>
        /// Check whether the connection is idle, and if so, set the idle-state to #<seealso cref="State.IDLE"/>. </summary>
        /// If the state is already idle and the {<param name="maxIdleSeconds">} is reached, set the state to
        /// #<seealso cref="State.RELEASING"/>. </param>
        public virtual void DoIdleDetect(long maxIdleSeconds)
        {
            if (Releasing)
            {
                return;
            }
            if (Idle)
            {
                // check if the connection is still idle, if not, mark it as using
                if (!clientCnx.idleCheck() && compareAndSetIdleStat(State.IDLE, State.USING))
                {
                    idleMarkTime = 0;
                    return;
                }
                if (maxIdleSeconds * 1000 + idleMarkTime < DateTimeHelper.CurrentUnixTimeMillis())
                {
                    tryMarkReleasing();
                }
                return;
            }
            if (clientCnx.idleCheck())
            {
                tryMarkIdleAndInitIdleTime();
            }
        }
    }
}

//Helper class added by Java to C# Converter:

//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2023 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace calls to Java's System.currentTimeMillis with the C# equivalent.
//	Unix time is defined as the number of seconds that have elapsed since midnight UTC, 1 January 1970.
//---------------------------------------------------------------------------------------------------------
using System;

internal static class DateTimeHelper
{
    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long CurrentUnixTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }
}
