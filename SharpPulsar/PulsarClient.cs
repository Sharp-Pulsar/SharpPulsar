﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar
{
    public sealed class PulsarClient:ReceiveActor
    {
        public PulsarClient()
        {
            //messages:
            //GetConnection returns ClientCnx
        }
    }
}