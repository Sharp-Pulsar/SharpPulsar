
namespace SharpPulsar.Utils
{
    internal interface IIterator<T>
    {
        bool HasNext();
        T Next();   
    }
    internal class ITerator<T> : IIterator<T>
    {
        T[] _iterators;
        int _position = 0;  
        internal ITerator(T[] iterators) => _iterators = iterators; 
        public bool HasNext()
        {
            if(_position >= _iterators.Length || _iterators[_position] == null)
            {
                return false;   
            }  
            return true; 
        }

        public T Next()
        {
            var iterator = _iterators[_position];  
            _position += 1;
            return iterator;
        }
    }
}
