﻿namespace SharpPulsar.Admin
{
    public sealed class AdminResponse
    {
        public AdminResponse(object response)
        {
            Response = response;
        }

        public object Response { get; }
    }
}
