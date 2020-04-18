using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace SharpPulsar.Akka.Admin
{
    public class AdminManager:ReceiveActor
    {
        public AdminManager(AdminConfiguration configuration)
        {
                
        }
        public static Props Prop(AdminConfiguration configuration)
        {
            return Props.Create(() => new AdminManager(configuration));
        }
    }
}
