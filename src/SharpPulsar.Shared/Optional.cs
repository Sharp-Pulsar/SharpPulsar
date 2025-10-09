
using System;

namespace SharpPulsar.Shared
{
    public struct Optional<T>
    {
        private readonly T _value; 
        public bool HasValue {  get; private set; } 
        public Optional(T value) 
        {
            _value = value;
            HasValue = true;
        }
        public Optional<U> Map<U>(Func<T,U> mapper)
        {
            if(HasValue) 
            {
                return new Optional<U>(mapper(_value));
            }
            return default(Optional<U>);    
        }
        public static readonly Optional<T> None;

        public bool IsEmpty => !HasValue;
        public T GetValueOrDefault(T defaultValue)
        {
            return HasValue ? _value : defaultValue;
        }
        public T GetOrElse(T fallbackValue)
        {
            if (!HasValue)
            {
                return fallbackValue;
            }

            return _value;
        }
        public T OrElseGet(Func<T> action)
        {
            return action();
        }
    }
}
