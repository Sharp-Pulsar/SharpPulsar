
using System;

namespace SharpPulsar.Table.Messages
{
    public readonly record struct ForEachAction<T>
    {
        public Action<string, T> Action { get; }
        public ForEachAction(Action<string, T> action)
        {
            Action = action;    
        }
    }
}
